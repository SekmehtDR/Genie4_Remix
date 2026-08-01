using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GenieClient
{
    /// <summary>
    /// Self-update for Genie Remix, against THIS repository.
    ///
    /// This deliberately replaces the old Lamp-based path in Utility/Updater.cs, which pointed at
    /// GenieClient/Genie4 and would pull a Remix install back to upstream. Nothing here runs
    /// automatically -- it is only reached from Help -> Check For Updates.
    ///
    /// Install strategy: a running Genie.exe cannot overwrite its own folder, so the payload is
    /// downloaded, verified and extracted while Genie is still running, then a small PowerShell
    /// helper waits for Genie to exit, copies the payload over the install folder and relaunches.
    /// The release ZIP contains no user data (see docs/RELEASING.md), so copying over it leaves
    /// Config/, Scripts/, Maps/, Plugins/, Logs/, Icons/ and Sounds/ untouched.
    /// </summary>
    public static class RemixUpdater
    {
        public const string RepoOwner = "SekmehtDR";
        public const string RepoName = "Genie4_Remix";

        private const string LatestReleaseUrl = "https://api.github.com/repos/" + RepoOwner + "/" + RepoName + "/releases/latest";
        public const string ReleasesPageUrl = "https://github.com/" + RepoOwner + "/" + RepoName + "/releases/latest";

        private const string ChecksumAssetName = "SHA256SUMS.txt";
        private const string UserAgent = "Genie-Remix-Updater";

        /// <summary>Details of the newest published release.</summary>
        public sealed class UpdateInfo
        {
            public Version Version;
            public string Tag;
            public string Title;
            public string Notes;
            public string AssetName;
            public string AssetUrl;
            public long AssetSize;
            /// <summary>Null when the release predates SHA256SUMS.txt.</summary>
            public string ExpectedSha256;
        }

        /// <summary>
        /// The version this build reports everywhere else -- the title bar, the $version script
        /// variable, and the FE:GENIE handshake. Always four numeric components.
        /// </summary>
        public static Version CurrentVersion
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return v == null ? new Version(0, 0, 0, 0) : Normalize(v);
            }
        }

        /// <summary>Where the running client is installed. Everything ships beside the exe.</summary>
        public static string InstallDirectory
        {
            get { return AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar); }
        }

        private static HttpClient CreateClient()
        {
            var oClient = new HttpClient();
            oClient.Timeout = TimeSpan.FromMinutes(30); // the payload is ~55 MB
            oClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            oClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            return oClient;
        }

        /// <summary>Pads a version out to four components so comparisons never see -1.</summary>
        private static Version Normalize(Version v)
        {
            return new Version(
                v.Major < 0 ? 0 : v.Major,
                v.Minor < 0 ? 0 : v.Minor,
                v.Build < 0 ? 0 : v.Build,
                v.Revision < 0 ? 0 : v.Revision);
        }

        /// <summary>
        /// Turns a release tag into a comparable version. Handles every tag shape this repo has
        /// used: "v4.1.1", "v4.1.0.0", "4.0.2.9", and pre-release forms like "v4.2.0-rc.1".
        /// </summary>
        public static bool TryParseTag(string sTag, out Version oVersion)
        {
            oVersion = null;
            if (string.IsNullOrWhiteSpace(sTag)) return false;

            string s = sTag.Trim();
            if (s.StartsWith("v", StringComparison.OrdinalIgnoreCase)) s = s.Substring(1);

            // Drop any pre-release label or build metadata: 4.2.0-rc.1+abc1234 -> 4.2.0
            int iCut = s.IndexOfAny(new[] { '-', '+' });
            if (iCut >= 0) s = s.Substring(0, iCut);

            Version parsed;
            if (!Version.TryParse(s, out parsed)) return false;

            oVersion = Normalize(parsed);
            return true;
        }

        /// <summary>
        /// Fetches the newest published release. Returns null if the release cannot be understood
        /// (no usable asset, unparseable tag). Throws on network or API failure.
        ///
        /// GitHub's /releases/latest excludes pre-releases, so testers on a stable build are never
        /// offered an rc by accident.
        /// </summary>
        public static async Task<UpdateInfo> GetLatestReleaseAsync(CancellationToken oCancel = default)
        {
            using (var oClient = CreateClient())
            {
                string sJson = await oClient.GetStringAsync(LatestReleaseUrl).ConfigureAwait(false);

                using (var oDoc = JsonDocument.Parse(sJson))
                {
                    var oRoot = oDoc.RootElement;

                    string sTag = GetString(oRoot, "tag_name");
                    Version oVersion;
                    if (!TryParseTag(sTag, out oVersion)) return null;

                    var oInfo = new UpdateInfo
                    {
                        Tag = sTag,
                        Version = oVersion,
                        Title = GetString(oRoot, "name"),
                        Notes = GetString(oRoot, "body")
                    };

                    JsonElement oAssets;
                    if (!oRoot.TryGetProperty("assets", out oAssets) || oAssets.ValueKind != JsonValueKind.Array)
                    {
                        return null;
                    }

                    string sChecksumUrl = null;
                    foreach (var oAsset in oAssets.EnumerateArray())
                    {
                        string sName = GetString(oAsset, "name");
                        string sUrl = GetString(oAsset, "browser_download_url");
                        if (string.IsNullOrEmpty(sName) || string.IsNullOrEmpty(sUrl)) continue;

                        // Matches both the current Genie-Remix-<version>.zip and the older
                        // unversioned Genie-Remix.zip.
                        if (oInfo.AssetName == null
                            && sName.StartsWith("Genie-Remix", StringComparison.OrdinalIgnoreCase)
                            && sName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        {
                            oInfo.AssetName = sName;
                            oInfo.AssetUrl = sUrl;
                            long lSize;
                            JsonElement oSize;
                            if (oAsset.TryGetProperty("size", out oSize) && oSize.TryGetInt64(out lSize))
                            {
                                oInfo.AssetSize = lSize;
                            }
                        }
                        else if (string.Equals(sName, ChecksumAssetName, StringComparison.OrdinalIgnoreCase))
                        {
                            sChecksumUrl = sUrl;
                        }
                    }

                    if (oInfo.AssetName == null) return null;

                    if (sChecksumUrl != null)
                    {
                        try
                        {
                            string sSums = await oClient.GetStringAsync(sChecksumUrl).ConfigureAwait(false);
                            oInfo.ExpectedSha256 = FindHash(sSums, oInfo.AssetName);
                        }
                        catch
                        {
                            // A missing checksum file is not fatal; the caller warns instead.
                            oInfo.ExpectedSha256 = null;
                        }
                    }

                    return oInfo;
                }
            }
        }

        /// <summary>Parses a "sha256sum" style file: "&lt;hash&gt;  &lt;filename&gt;" per line.</summary>
        private static string FindHash(string sContent, string sFileName)
        {
            if (string.IsNullOrEmpty(sContent)) return null;

            foreach (string sLine in sContent.Split('\n'))
            {
                string s = sLine.Trim();
                if (s.Length == 0) continue;

                string[] aParts = s.Split(new[] { ' ', '\t', '*' }, StringSplitOptions.RemoveEmptyEntries);
                if (aParts.Length < 2) continue;

                if (string.Equals(aParts[aParts.Length - 1], sFileName, StringComparison.OrdinalIgnoreCase))
                {
                    return aParts[0].Trim().ToLowerInvariant();
                }
            }
            return null;
        }

        private static string GetString(JsonElement oElement, string sName)
        {
            JsonElement oValue;
            if (oElement.TryGetProperty(sName, out oValue) && oValue.ValueKind == JsonValueKind.String)
            {
                return oValue.GetString();
            }
            return null;
        }

        /// <summary>True when the release is strictly newer than what is running.</summary>
        public static bool IsNewerThanCurrent(UpdateInfo oInfo)
        {
            return oInfo != null && oInfo.Version > CurrentVersion;
        }

        /// <summary>
        /// Downloads the release ZIP, verifies its checksum, and extracts it to a temporary
        /// staging folder. Returns the folder holding the new Genie.exe.
        ///
        /// Nothing in the install folder is touched here -- a failure at any point leaves the
        /// running installation exactly as it was.
        /// </summary>
        public static async Task<string> DownloadAndStageAsync(UpdateInfo oInfo, IProgress<int> oProgress, CancellationToken oCancel = default)
        {
            if (oInfo == null) throw new ArgumentNullException("oInfo");

            string sWorkDir = Path.Combine(Path.GetTempPath(), "GenieRemixUpdate", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(sWorkDir);

            string sZipPath = Path.Combine(sWorkDir, oInfo.AssetName);

            using (var oClient = CreateClient())
            using (var oResponse = await oClient.GetAsync(oInfo.AssetUrl, HttpCompletionOption.ResponseHeadersRead, oCancel).ConfigureAwait(false))
            {
                oResponse.EnsureSuccessStatusCode();

                long lTotal = oResponse.Content.Headers.ContentLength ?? oInfo.AssetSize;
                long lRead = 0;
                int iLastPercent = -1;

                using (var oHttpStream = await oResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using (var oFile = new FileStream(sZipPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                {
                    var aBuffer = new byte[81920];
                    int iCount;
                    while ((iCount = await oHttpStream.ReadAsync(aBuffer, 0, aBuffer.Length, oCancel).ConfigureAwait(false)) > 0)
                    {
                        await oFile.WriteAsync(aBuffer, 0, iCount, oCancel).ConfigureAwait(false);
                        lRead += iCount;

                        if (oProgress != null && lTotal > 0)
                        {
                            int iPercent = (int)(lRead * 100L / lTotal);
                            if (iPercent != iLastPercent)
                            {
                                iLastPercent = iPercent;
                                oProgress.Report(iPercent);
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(oInfo.ExpectedSha256))
            {
                string sActual = ComputeSha256(sZipPath);
                if (!string.Equals(sActual, oInfo.ExpectedSha256, StringComparison.OrdinalIgnoreCase))
                {
                    TryDeleteDirectory(sWorkDir);
                    throw new InvalidDataException(
                        "The downloaded file does not match the checksum published with the release. " +
                        "The download may be corrupt. Nothing has been changed." + Environment.NewLine +
                        "Expected: " + oInfo.ExpectedSha256 + Environment.NewLine +
                        "Actual:   " + sActual);
                }
            }

            string sExtractDir = Path.Combine(sWorkDir, "extract");
            Directory.CreateDirectory(sExtractDir);
            ZipFile.ExtractToDirectory(sZipPath, sExtractDir);

            // Delete the ZIP now that it is unpacked -- saves ~55 MB of temp space.
            try { File.Delete(sZipPath); } catch { }

            string sPayload = ResolvePayloadRoot(sExtractDir);
            if (sPayload == null || !File.Exists(Path.Combine(sPayload, "Genie.exe")))
            {
                TryDeleteDirectory(sWorkDir);
                throw new InvalidDataException("The downloaded release does not contain Genie.exe. Nothing has been changed.");
            }

            return sPayload;
        }

        /// <summary>
        /// The ZIP nests everything under a Genie-Remix/ folder. Tolerate both that and a flat
        /// archive, so an older or hand-made ZIP still installs.
        /// </summary>
        private static string ResolvePayloadRoot(string sExtractDir)
        {
            if (File.Exists(Path.Combine(sExtractDir, "Genie.exe"))) return sExtractDir;

            var aDirs = Directory.GetDirectories(sExtractDir);
            var aFiles = Directory.GetFiles(sExtractDir);
            if (aDirs.Length == 1 && aFiles.Length == 0) return aDirs[0];

            foreach (string sDir in aDirs)
            {
                if (File.Exists(Path.Combine(sDir, "Genie.exe"))) return sDir;
            }
            return null;
        }

        private static string ComputeSha256(string sPath)
        {
            using (var oSha = SHA256.Create())
            using (var oStream = File.OpenRead(sPath))
            {
                byte[] aHash = oSha.ComputeHash(oStream);
                var oBuilder = new StringBuilder(aHash.Length * 2);
                foreach (byte b in aHash) oBuilder.Append(b.ToString("x2"));
                return oBuilder.ToString();
            }
        }

        private static void TryDeleteDirectory(string sPath)
        {
            try { if (Directory.Exists(sPath)) Directory.Delete(sPath, true); } catch { }
        }

        /// <summary>
        /// Hands off to a helper that waits for this process to exit, copies the staged payload
        /// over the install folder, and relaunches Genie.
        ///
        /// The caller must save configuration and exit immediately after this returns.
        /// </summary>
        public static void InstallAndRestart(string sPayloadDir)
        {
            if (string.IsNullOrEmpty(sPayloadDir) || !Directory.Exists(sPayloadDir))
            {
                throw new DirectoryNotFoundException("The staged update folder is missing: " + sPayloadDir);
            }

            string sWorkDir = Directory.GetParent(sPayloadDir).FullName;
            string sScript = Path.Combine(sWorkDir, "apply-update.ps1");
            File.WriteAllText(sScript, InstallerScript, new UTF8Encoding(false));

            string sTarget = InstallDirectory;
            string sExe = Path.Combine(sTarget, "Genie.exe");
            string sLog = Path.Combine(sWorkDir, "update.log");

            var oStart = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = string.Join(" ", new[]
                {
                    "-NoProfile",
                    "-NonInteractive",
                    "-ExecutionPolicy", "Bypass",
                    "-WindowStyle", "Hidden",
                    "-File", Quote(sScript),
                    "-ProcessId", Process.GetCurrentProcess().Id.ToString(),
                    "-Source", Quote(sPayloadDir),
                    "-Target", Quote(sTarget),
                    "-Exe", Quote(sExe),
                    "-LogPath", Quote(sLog)
                })
            };

            Process.Start(oStart);
        }

        private static string Quote(string s)
        {
            return "\"" + s + "\"";
        }

        /// <summary>
        /// Runs after Genie exits, so it cannot be part of Genie itself.
        ///
        /// Copies rather than deletes-then-copies: the release ZIP holds no user data, so files
        /// it does not mention (a player's Config/, Scripts/, Maps/, and any plugins they added)
        /// are left alone.
        /// </summary>
        private const string InstallerScript = @"
param(
    [int]$ProcessId,
    [string]$Source,
    [string]$Target,
    [string]$Exe,
    [string]$LogPath
)

$ErrorActionPreference = 'Stop'

function Write-Log([string]$Message) {
    $line = ('{0:yyyy-MM-dd HH:mm:ss}  {1}' -f (Get-Date), $Message)
    try { Add-Content -Path $LogPath -Value $line } catch { }
}

try {
    Write-Log ""Applying update. pid=$ProcessId source=$Source target=$Target""

    # Wait for Genie to release its files. Give up after 2 minutes rather than hang forever.
    $deadline = (Get-Date).AddMinutes(2)
    while ((Get-Date) -lt $deadline) {
        $proc = Get-Process -Id $ProcessId -ErrorAction SilentlyContinue
        if (-not $proc) { break }
        Start-Sleep -Milliseconds 250
    }

    if (Get-Process -Id $ProcessId -ErrorAction SilentlyContinue) {
        Write-Log 'Genie is still running after 2 minutes. Aborting; nothing was changed.'
        exit 1
    }

    # Windows can hold file locks briefly after a process exits.
    Start-Sleep -Milliseconds 750

    $failed = @()
    $copied = 0
    $items = Get-ChildItem -Path $Source -Recurse -File

    foreach ($item in $items) {
        $relative = $item.FullName.Substring($Source.Length).TrimStart('\', '/')
        $dest = Join-Path $Target $relative
        $destDir = Split-Path $dest -Parent
        if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir -Force | Out-Null }

        $ok = $false
        for ($attempt = 1; $attempt -le 5; $attempt++) {
            try {
                Copy-Item -LiteralPath $item.FullName -Destination $dest -Force
                $ok = $true
                break
            } catch {
                Start-Sleep -Milliseconds (200 * $attempt)
            }
        }

        if ($ok) { $copied++ } else { $failed += $relative }
    }

    Write-Log ""Copied $copied file(s). Failed: $($failed.Count)""
    if ($failed.Count -gt 0) {
        Write-Log ('Failed files: ' + ($failed -join ', '))
        Write-Log 'The update is incomplete. Re-extract the release ZIP over the folder by hand.'
    }

    Write-Log 'Relaunching Genie.'
    Start-Process -FilePath $Exe -WorkingDirectory $Target
} catch {
    Write-Log ""Update failed: $($_.Exception.Message)""
    exit 1
} finally {
    # Best effort: remove the staged payload. The log lives here too, so only clear the payload.
    try { Remove-Item -LiteralPath $Source -Recurse -Force -ErrorAction SilentlyContinue } catch { }
}
";
    }
}
