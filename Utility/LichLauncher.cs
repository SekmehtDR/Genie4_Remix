using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
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

        public static async Task<LaunchResult> EnsureRunning(Genie.Config oConfig)
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

            if (IsListening(iPort))
            {
                oResult.Status = LaunchStatus.AlreadyRunning;
                oResult.Message = "Lich is already listening on " + sHost + ":" + iPort + " -- connecting to it.";
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
                return oResult;
            }

            if (m_oLichProcess == null)
            {
                oResult.Status = LaunchStatus.StartFailed;
                oResult.Message = "Unable to start Lich (no process was created).";
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
                    return oResult;
                }

                if (IsListening(iPort))
                {
                    oResult.Status = LaunchStatus.Started;
                    oResult.Message = "Started Lich -- listening on " + sHost + ":" + iPort + ".";
                    return oResult;
                }

                await Task.Delay(250);
                iWaitedMs += 250;
            }

            oResult.Status = LaunchStatus.StartedSlowly;
            oResult.Message = "Started Lich, but it had not opened " + sHost + ":" + iPort + " after "
                            + iCeilingSeconds + "s. Connecting anyway -- raise {lichstartpause} if this keeps happening.";
            return oResult;
        }
    }
}
