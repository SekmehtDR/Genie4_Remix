using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenieClient
{
    // Starts Lich, or attaches to one that is already running.
    //
    // Lich is a proxy: it listens on LichServer:LichPort, Genie hands it the login key it got
    // from eaccess, and Lich connects on to the game. Genie used to launch a fresh Lich every
    // single time, before it had authenticated -- so a mistyped profile name or a bad password
    // left a Lich running with nothing connected to it, and the next attempt stacked another
    // one on top of it. Checking the port first also means a Lich already running as a
    // background service is simply used, which is the whole point of running one.
    public static class LichLauncher
    {
        public enum LaunchStatus
        {
            AlreadyRunning, // something is listening on the configured endpoint -- reuse it
            Started,        // we launched Lich and it opened the port
            StartedSlowly,  // we launched Lich but the port was not open when we stopped waiting
            Remote,         // LichServer is another machine, so there is nothing to launch here
            PathsMissing,
            StartFailed
        }

        public sealed class LaunchResult
        {
            public LaunchStatus Status;
            public string Message = string.Empty;

            // True when it is worth going on to authenticate and connect.
            public bool ShouldConnect
            {
                get
                {
                    return Status == LaunchStatus.AlreadyRunning
                        || Status == LaunchStatus.Started
                        || Status == LaunchStatus.StartedSlowly
                        || Status == LaunchStatus.Remote;
                }
            }
        }

        private static Process m_oLichProcess = null;

        // The Lich we started, while it is still running. Null if we never started one, or if
        // the one we started has since exited.
        public static Process TrackedProcess
        {
            get
            {
                try
                {
                    if (m_oLichProcess != null && m_oLichProcess.HasExited)
                    {
                        m_oLichProcess = null;
                    }
                }
                catch
                {
                    m_oLichProcess = null;
                }

                return m_oLichProcess;
            }
        }

        // True if anything on this machine is listening on the port.
        //
        // Deliberately passive. Lich stops listening the moment it accepts a client, so probing
        // by connecting would burn the one accept the real connection needs.
        public static bool IsListening(int iPort)
        {
            if (iPort <= 0 || iPort > 65535)
            {
                return false;
            }

            try
            {
                foreach (IPEndPoint oEndPoint in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
                {
                    if (oEndPoint.Port == iPort)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // If we cannot enumerate listeners, fall through and let the caller start Lich
                // the way it always did.
            }

            return false;
        }

        // True if something already has a connection open to the port.
        //
        // A Lich that has accepted a client stops listening, so IsListening alone cannot tell
        // "no Lich at all" apart from "Lich busy serving a session". This distinguishes them.
        public static bool IsInUse(int iPort)
        {
            if (iPort <= 0 || iPort > 65535)
            {
                return false;
            }

            try
            {
                foreach (TcpConnectionInformation oConnection in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections())
                {
                    if (oConnection.State == TcpState.Established && oConnection.RemoteEndPoint.Port == iPort)
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        // One line describing what Genie would do right now, for #ls.
        public static string DescribeStatus(string sHost, int iPort)
        {
            if (IsListening(iPort))
            {
                return "listening on " + sHost + ":" + iPort + " -- Genie will reuse it";
            }

            if (IsInUse(iPort))
            {
                return "running and serving a session on " + sHost + ":" + iPort;
            }

            return "not running -- Genie will start one when you connect";
        }

        // Whether LichServer points at this machine. Anything we cannot resolve is treated as
        // local so that a misconfigured host still behaves the way it used to.
        public static bool IsLocalMachine(string sHost)
        {
            if (string.IsNullOrWhiteSpace(sHost))
            {
                return true;
            }

            string s = sHost.Trim();
            if (s.Equals("localhost", StringComparison.OrdinalIgnoreCase) || s == ".")
            {
                return true;
            }

            try
            {
                IPAddress[] oTargets = Dns.GetHostAddresses(s);
                foreach (IPAddress oTarget in oTargets)
                {
                    if (IPAddress.IsLoopback(oTarget))
                    {
                        return true;
                    }
                }

                IPAddress[] oLocal = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (IPAddress oTarget in oTargets)
                {
                    foreach (IPAddress oMine in oLocal)
                    {
                        if (oTarget.Equals(oMine))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return true;
            }

            return false;
        }

        // How long to wait for another Genie window to finish claiming this endpoint, and how
        // long we hold our own claim if nothing ever connects (a login that fails, say).
        private const int ClaimWaitMs = 30000;
        private const int ClaimMaxHoldMs = 30000;

        private static Semaphore m_oClaim = null;
        private static readonly object m_oClaimSync = new object();

        private static string ClaimName(string sHost, int iPort)
        {
            var sb = new StringBuilder("GenieRemix.Lich.");
            foreach (char c in (sHost ?? string.Empty).ToLowerInvariant())
            {
                sb.Append(char.IsLetterOrDigit(c) ? c : '_');
            }

            sb.Append('.').Append(iPort);
            return sb.ToString();
        }

        // Claims the Lich endpoint across processes.
        //
        // Two Genie windows started together would both look at the port, both see the first
        // one's freshly started Lich listening, and both decide to use it -- but Lich serves a
        // single client, so the loser burned a whole login and then got connection refused. The
        // claim serialises that decision. A Semaphore rather than a Mutex because the claim is
        // released from a different thread than the one that took it.
        private static async Task<bool> ClaimEndpoint(string sHost, int iPort, Action<string> oProgress)
        {
            ReleaseClaim();

            try
            {
                bool bCreated;
                var oSemaphore = new Semaphore(1, 1, ClaimName(sHost, iPort), out bCreated);

                if (!oSemaphore.WaitOne(0))
                {
                    if (oProgress != null)
                    {
                        oProgress("Another Genie window is starting Lich on " + sHost + ":" + iPort + " -- waiting for it to finish.");
                    }

                    // On a pool thread: this blocks, and the caller may well be the UI thread.
                    bool bGotIt = await Task.Run(() => oSemaphore.WaitOne(ClaimWaitMs));
                    if (!bGotIt)
                    {
                        oSemaphore.Dispose();
                        return false;
                    }
                }

                lock (m_oClaimSync)
                {
                    m_oClaim = oSemaphore;
                }

                return true;
            }
            catch
            {
                // If the claim cannot be taken for any reason, carry on unclaimed rather than
                // refusing to connect -- worst case is the old behaviour.
                return false;
            }
        }

        private static void ReleaseClaim()
        {
            lock (m_oClaimSync)
            {
                if (m_oClaim == null)
                {
                    return;
                }

                try { m_oClaim.Release(); } catch { }
                try { m_oClaim.Dispose(); } catch { }
                m_oClaim = null;
            }
        }

        // Hold the claim until the endpoint has actually been taken.
        //
        // "Taken" is the transition listening -> not listening, not simply "not listening right
        // now". Lich can still be starting up and not yet bound: treating that as taken releases
        // the claim immediately and the serialisation does nothing at all, which is exactly what
        // happened on the first attempt at this -- Lich needed longer than lichstartpause, the
        // launcher returned StartedSlowly with the port not yet open, and the claim evaporated.
        //
        // The ceiling stops a login that never completes from blocking the other window forever.
        private static void ReleaseClaimWhenTaken(int iPort)
        {
            Task.Run(async () =>
            {
                int iWaited = 0;
                bool bHasListened = IsListening(iPort);
                try
                {
                    while (iWaited < ClaimMaxHoldMs)
                    {
                        await Task.Delay(250);
                        iWaited += 250;

                        bool bListening = IsListening(iPort);
                        if (bListening)
                        {
                            bHasListened = true;
                        }
                        else if (bHasListened)
                        {
                            break; // it came up and has now been accepted by someone
                        }
                    }
                }
                catch
                {
                }

                ReleaseClaim();
            });
        }

        public static Task<LaunchResult> EnsureRunning(Genie.Config oConfig)
        {
            return EnsureRunning(oConfig, null);
        }

        public static async Task<LaunchResult> EnsureRunning(Genie.Config oConfig, Action<string> oProgress)
        {
            var oResult = new LaunchResult();
            string sHost = oConfig.LichServer;
            int iPort = oConfig.LichPort;

            if (iPort <= 0 || iPort > 65535)
            {
                oResult.Status = LaunchStatus.PathsMissing;
                oResult.Message = "Lich port \"" + iPort + "\" is not a valid port number. Fix {lichport} in your #config.";
                return oResult;
            }

            if (!IsLocalMachine(sHost))
            {
                oResult.Status = LaunchStatus.Remote;
                oResult.Message = "Using Lich on " + sHost + ":" + iPort + " (another machine -- not starting one here).";
                return oResult;
            }

            // Take the claim before deciding anything: if another window is mid-launch we want to
            // wait for it and then look again, not act on what the port looked like a moment ago.
            await ClaimEndpoint(sHost, iPort, oProgress);

            if (IsListening(iPort))
            {
                oResult.Status = LaunchStatus.AlreadyRunning;
                oResult.Message = "Lich is already listening on " + sHost + ":" + iPort + " -- connecting to it.";
                ReleaseClaimWhenTaken(iPort);
                return oResult;
            }

            string sMissing = string.Empty;
            if (!File.Exists(oConfig.RubyPath))
            {
                sMissing += "Ruby not found at Path:\t" + oConfig.RubyPath + Environment.NewLine;
            }

            if (!File.Exists(oConfig.LichPath))
            {
                sMissing += "Lich not found at Path:\t" + oConfig.LichPath + Environment.NewLine;
            }

            if (sMissing.Length > 0)
            {
                oResult.Status = LaunchStatus.PathsMissing;
                oResult.Message = "Fix the following file paths in your #Config" + Environment.NewLine + sMissing;
                ReleaseClaim();
                return oResult;
            }

            // Launch Ruby directly rather than through cmd.exe. "cmd /C" exited as soon as it
            // had spawned Ruby, so the process handle it returned was useless for telling
            // whether Lich was still alive -- and the command line it built was unquoted, so a
            // space anywhere in the Ruby or Lich path broke the launch silently.
            var oInfo = new ProcessStartInfo(oConfig.RubyPath);
            oInfo.Arguments = "\"" + oConfig.LichPath + "\" " + oConfig.LichArguments;
            oInfo.UseShellExecute = false;
            oInfo.CreateNoWindow = true;

            try
            {
                m_oLichProcess = Process.Start(oInfo);
            }
            catch (Exception ex)
            {
                m_oLichProcess = null;
                oResult.Status = LaunchStatus.StartFailed;
                oResult.Message = "Unable to start Lich: " + ex.Message;
                ReleaseClaim();
                return oResult;
            }

            if (m_oLichProcess == null)
            {
                oResult.Status = LaunchStatus.StartFailed;
                oResult.Message = "Unable to start Lich (no process was created).";
                ReleaseClaim();
                return oResult;
            }

            // Wait for the port rather than sleeping blindly. LichStartPause stays the ceiling,
            // so this can never wait longer than it used to, but a healthy Lich normally opens
            // the port in well under a second.
            int iCeilingSeconds = Math.Max(oConfig.LichStartPause, 1);
            int iWaitedMs = 0;
            while (iWaitedMs < iCeilingSeconds * 1000)
            {
                if (m_oLichProcess.HasExited)
                {
                    int iExitCode = m_oLichProcess.ExitCode;
                    m_oLichProcess = null;
                    oResult.Status = LaunchStatus.StartFailed;
                    oResult.Message = "Lich exited immediately (exit code " + iExitCode
                                    + "). Check your Ruby path, Lich path and Lich arguments.";
                    ReleaseClaim();
                    return oResult;
                }

                if (IsListening(iPort))
                {
                    oResult.Status = LaunchStatus.Started;
                    oResult.Message = "Started Lich -- listening on " + sHost + ":" + iPort + ".";
                    ReleaseClaimWhenTaken(iPort);
                    return oResult;
                }

                await Task.Delay(250);
                iWaitedMs += 250;
            }

            oResult.Status = LaunchStatus.StartedSlowly;
            oResult.Message = "Started Lich, but it had not opened " + sHost + ":" + iPort + " after "
                            + iCeilingSeconds + "s. Connecting anyway -- raise {lichstartpause} if this keeps happening.";
            ReleaseClaimWhenTaken(iPort);
            return oResult;
        }
    }
}
