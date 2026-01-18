using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using DiscUtils.Iso9660;
using FluentFTP;
using IronSoftware.Drawing;
using Microsoft.Data.Sqlite;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.PS5.Tools.Editors;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PSMultiTools.Classes
{

    public partial class Utils
    {

        [DllImport("libc")]
        private static extern uint geteuid();

        public static readonly byte[] ByBytes = [0x62, 0x79, 0x20, 0x53, 0x76, 0x65, 0x6E, 0x47, 0x44, 0x4B];
        public static readonly string[] SpaceSeparator = [" "];

        private static Process? currentffplayProc;
        private static CancellationTokenSource? currentffplayCts;
        private static readonly Lock ffplayprocLock = new();

        private static readonly HttpClient NewhttpClient = new() { Timeout = Timeout.InfiniteTimeSpan };

        #region Library Music

        public static void PlayGameSoundFile(string SoundFile)
        {
            StopGameSound();
            StopGameSoundBytes();

            var ffplay = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ffplay.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ffplay");
            var ffplayStartInfo = new ProcessStartInfo
            {
                FileName = ffplay,
                Arguments = $"-nodisp -autoexit \"{SoundFile}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var proc = Process.Start(ffplayStartInfo);
            if (proc == null) return;

            proc.BeginErrorReadLine();

            lock (ffplayprocLock)
            {
                var PreviousProcess = Interlocked.Exchange(ref currentffplayProc, proc);
                try { PreviousProcess?.Kill(); PreviousProcess?.WaitForExit(2000); PreviousProcess?.Dispose(); } catch { }
            }
        }

        public static void StopGameSound()
        {
            var proc = Interlocked.Exchange(ref currentffplayProc, null);
            if (proc == null) return;

            try
            {
                if (!proc.HasExited)
                {
                    proc.Kill(true);
                    proc.WaitForExit(2000);
                }
            }
            catch { }
            finally
            {
                try
                {
                    proc.Dispose();
                }
                catch { }
            }
        }

        public static void StartAndStreamSoundBytes(byte[] data)
        {
            StopGameSound();
            StopGameSoundBytes();

            var ffplay = OperatingSystem.IsWindows()
                ? Path.Combine(Environment.CurrentDirectory, "Tools", "ffplay.exe")
                : Path.Combine(Environment.CurrentDirectory, "Tools", "ffplay");

            var psi = new ProcessStartInfo
            {
                FileName = ffplay,
                Arguments = "-nodisp -autoexit -i -",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var proc = Process.Start(psi);
            if (proc == null) return;

            proc.BeginErrorReadLine();

            var previousProc = Interlocked.Exchange(ref currentffplayProc, proc);
            previousProc?.Dispose();

            var cts = new CancellationTokenSource();
            var previousCts = Interlocked.Exchange(ref currentffplayCts, cts);
            previousCts?.Cancel();
            previousCts?.Dispose();

            _ = Task.Run(async () =>
            {
                try
                {
                    using var stdin = proc.StandardInput.BaseStream;
                    const int chunkSize = 64 * 1024;
                    int offset = 0;

                    while (offset < data.Length)
                    {
                        cts.Token.ThrowIfCancellationRequested();

                        int toWrite = Math.Min(chunkSize, data.Length - offset);
                        await stdin.WriteAsync(data.AsMemory(offset, toWrite), cts.Token).ConfigureAwait(false);
                        await stdin.FlushAsync(cts.Token).ConfigureAwait(false);
                        offset += toWrite;

                        await Task.Yield();
                    }

                    try { stdin.Close(); } catch { }
                }
                catch (OperationCanceledException)
                {
                    try { proc.StandardInput.BaseStream.Close(); } catch { }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Stream error: {ex}");
                }
                finally
                {
                    try
                    {
                        if (!proc.HasExited)
                        {
                            proc.WaitForExit(2000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"WaitForExit failed: {ex}");
                    }
                }
            });
        }

        public static void StopGameSoundBytes()
        {
            var cts = Interlocked.Exchange(ref currentffplayCts, null);
            try { cts?.Cancel(); } catch { }
            try { cts?.Dispose(); } catch { }

            var proc = Interlocked.Exchange(ref currentffplayProc, null);
            if (proc == null) return;

            try
            {
                if (!proc.HasExited)
                {
                    proc.Kill(true);
                    proc.WaitForExit(2000);
                }
            }
            catch { }
            finally
            {
                try
                {
                    proc.Dispose();
                }
                catch { }
            }
        }

        #endregion

        #region Converters

        public static string HexStringToAscii(string HexString, bool CleanEndOfString)
        {
            string ascii;

            try
            {
                string str = "";

                while (HexString.Length > 0)
                {
                    str += Convert.ToChar(Convert.ToUInt32(HexString[..2], 16)).ToString();
                    HexString = HexString[2..];
                }

                if (CleanEndOfString)
                    str = str.Replace("\0", "");
                ascii = str;
            }
            catch (Exception)
            {
                ascii = "";
            }

            return ascii;
        }

        public static string ByteArrayToAscii(byte[] ByteArray, int startPos, int length, bool cleanEndOfString)
        {
            byte[] numArray = new byte[(length - 1 + 1)];
            Array.Copy(ByteArray, startPos, numArray, 0, numArray.Length);
            return HexStringToAscii(ByteArrayToHexString(numArray), cleanEndOfString);
        }

        public static string ByteArrayToHexString(byte[] ByteArray)
        {
            string hexString = "";
            int num = ByteArray.Length - 1;
            int index = 0;

            while (index <= num)
            {
                hexString += ByteArray[index].ToString("X2");
                index += 1;
            }

            return hexString;
        }

        #endregion

        #region Image Processing

        public static Bitmap ConvertTo24bppPNG(byte[] rgbPixelData, int width, int height)
        {
            Vector dpi = new(96, 96);
            var NewBitmap = new WriteableBitmap(new PixelSize(width, height), dpi, Avalonia.Platform.PixelFormats.Rgb24);
            using (var frameBuffer = NewBitmap.Lock())
            {
                Marshal.Copy(rgbPixelData, 0, frameBuffer.Address, rgbPixelData.Length);
            }
            return NewBitmap;
        }

        public static ImageBrush AnyBitmapToImageBrush(AnyBitmap initialAnyBitmap)
        {
            using MemoryStream memory = new();
            initialAnyBitmap.ExportStream(memory);
            memory.Position = 0;
            Bitmap avaloniaBitmap = new(memory);

            var avaloniaImageBrush = new ImageBrush
            {
                Source = avaloniaBitmap,
                Stretch = Stretch.UniformToFill,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center
            };

            return avaloniaImageBrush;
        }

        public static ImageBrush ByteArrayToImageBrush(byte[] initialByteArray)
        {
            using MemoryStream memory = new();
            AnyBitmap NewAnyBitmap = new(initialByteArray);
            NewAnyBitmap.ExportStream(memory);
            memory.Position = 0;
            Bitmap avaloniaBitmap = new(memory);

            var avaloniaImageBrush = new ImageBrush
            {
                Source = avaloniaBitmap,
                Stretch = Stretch.UniformToFill,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center
            };

            return avaloniaImageBrush;
        }

        public static IImage AnyBitmapToIImage(AnyBitmap initialAnyBitmap)
        {
            IImage NewImage = new Bitmap(initialAnyBitmap.GetStream());
            return NewImage;
        }

        public static IImage AnyBitmapToIImage(byte[] initialByteArray)
        {
            using MemoryStream memory = new();
            AnyBitmap NewAnyBitmap = new(initialByteArray);
            NewAnyBitmap.ExportStream(memory);
            memory.Position = 0;
            IImage NewImage = new Bitmap(memory);

            return NewImage;
        }

        #endregion

        #region PSX Related

        private static Structures.MountedPSXDrive _ConnectedPSXHDD;

        public static Structures.MountedPSXDrive ConnectedPSXHDD
        {
            get
            {
                return _ConnectedPSXHDD;
            }
            set
            {
                _ConnectedPSXHDD = value;
            }
        }

        public enum DiscType
        {
            CD,
            DVD
        }

        public static DiscType GetDiscType(string ISOFile)
        {
            double ISOFileSize = new FileInfo(ISOFile).Length / 1048576d;

            if (ISOFileSize > 700d)
            {
                return DiscType.DVD;
            }
            else
            {
                return DiscType.CD;
            }
        }

        public static string IsNBDConnected(string WNBDClientPath)
        {
            string[] ProcessOutput;
            string NBDDriveName = "";

            // List connected clients
            if (!string.IsNullOrEmpty(WNBDClientPath))
            {
                using (var WNBDClient = new Process())
                {
                    WNBDClient.StartInfo.FileName = WNBDClientPath;
                    WNBDClient.StartInfo.Arguments = "list";
                    WNBDClient.StartInfo.RedirectStandardOutput = true;
                    WNBDClient.StartInfo.UseShellExecute = false;
                    WNBDClient.StartInfo.CreateNoWindow = true;
                    WNBDClient.Start();
                    WNBDClient.WaitForExit();

                    var OutputReader = WNBDClient.StandardOutput;
                    ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.None);
                }

                foreach (string ReturnedLine in ProcessOutput)
                {
                    if (ReturnedLine.Contains("wnbd-client"))
                    {
                        NBDDriveName = ReturnedLine.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries)[4].Trim();
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(NBDDriveName))
            {
                return NBDDriveName;
            }
            else
            {
                return "";
            }
        }

        public static string IsLocalHDDConnected()
        {
            // Query the drives
            if (File.Exists(Environment.CurrentDirectory + @"\Tools\hdl_dump.exe"))
            {
                using var HDLDump = new Process();
                HDLDump.StartInfo.FileName = Environment.CurrentDirectory + @"\Tools\hdl_dump.exe";
                HDLDump.StartInfo.Arguments = "query";
                HDLDump.StartInfo.RedirectStandardOutput = true;
                HDLDump.StartInfo.UseShellExecute = false;
                HDLDump.StartInfo.CreateNoWindow = true;
                HDLDump.Start();

                // Read the output
                var OutputReader = HDLDump.StandardOutput;
                string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.None);

                string DriveHDLName = "";

                // Find the local drive
                foreach (string Line in ProcessOutput)
                {
                    if (!string.IsNullOrWhiteSpace(Line))
                    {
                        if (Line.Contains("formatted Playstation 2 HDD"))
                        {
                            // Set the found drive as mounted PSX drive
                            string[] DriveInfos = Line.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries);
                            if (DriveInfos[0] is not null)
                            {
                                DriveHDLName = DriveInfos[0].Trim();
                                break;
                            }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(DriveHDLName))
                {
                    return DriveHDLName;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        public static string GetConnectedNBDIP(string WNBDClientPath, string NBDDriveName)
        {
            // Get the connected IP address
            if (!string.IsNullOrEmpty(WNBDClientPath))
            {
                string[] ProcessOutput;
                string NBDIP = "";

                using (var WNBDClient = new Process())
                {
                    WNBDClient.StartInfo.FileName = WNBDClientPath;
                    WNBDClient.StartInfo.Arguments = "show " + NBDDriveName;
                    WNBDClient.StartInfo.RedirectStandardOutput = true;
                    WNBDClient.StartInfo.UseShellExecute = false;
                    WNBDClient.StartInfo.CreateNoWindow = true;
                    WNBDClient.Start();
                    WNBDClient.WaitForExit();

                    var OutputReader = WNBDClient.StandardOutput;
                    ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.None);
                }

                foreach (string ReturnedLine in ProcessOutput)
                {
                    if (ReturnedLine.Contains("Hostname"))
                    {
                        NBDIP = ReturnedLine.Split(':')[1].Trim();
                        break;
                    }
                }

                return NBDIP;
            }
            else
            {
                return "";
            }
        }

        public static string GetHDLDriveName()
        {
            if (File.Exists(Environment.CurrentDirectory + @"\Tools\hdl_dump.exe"))
            {
                string HDLDriveName = "";

                // Query the drives
                using (var HDLDump = new Process())
                {
                    HDLDump.StartInfo.FileName = Environment.CurrentDirectory + @"\Tools\hdl_dump.exe";
                    HDLDump.StartInfo.Arguments = "query";
                    HDLDump.StartInfo.RedirectStandardOutput = true;
                    HDLDump.StartInfo.UseShellExecute = false;
                    HDLDump.StartInfo.CreateNoWindow = true;
                    HDLDump.Start();
                    HDLDump.WaitForExit();

                    // Read the output
                    var OutputReader = HDLDump.StandardOutput;
                    string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.None);

                    // Find the drive
                    foreach (string Line in ProcessOutput)
                    {
                        if (!string.IsNullOrWhiteSpace(Line))
                        {
                            if (Line.Contains("formatted Playstation 2 HDD"))
                            {
                                // Set the found drive as mounted PSX drive
                                string[] DriveInfos = Line.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries);
                                HDLDriveName = DriveInfos[0].Trim();
                                break;
                            }
                        }
                    }
                }

                return HDLDriveName;
            }
            else
            {
                return "";
            }
        }

        public static string GetHDDID()
        {
            string DriveID = "";

            // Query the drives
            using (var WMIC = new Process())
            {
                WMIC.StartInfo.FileName = "wmic";
                WMIC.StartInfo.Arguments = "diskdrive get Caption,DeviceID";
                WMIC.StartInfo.RedirectStandardOutput = true;
                WMIC.StartInfo.UseShellExecute = false;
                WMIC.StartInfo.CreateNoWindow = true;
                WMIC.Start();
                WMIC.WaitForExit();

                // Read the output
                var OutputReader = WMIC.StandardOutput;
                string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.None);

                // Find the drive
                foreach (string Line in ProcessOutput)
                {
                    if (!string.IsNullOrWhiteSpace(Line))
                    {
                        if (Line.Contains("WNBD WNBD_DISK SCSI Disk Device"))
                        {
                            DriveID = Line.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries)[5].Trim();
                            break;
                        }
                        else if (Line.Contains("Microsoft Virtual Disk")) // For testing with local VHD
                        {
                            DriveID = Line.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries)[3].Trim();
                            break;
                        }
                    }
                }
            }

            return DriveID;
        }

        public static string FindNextAvailableDriveLetter()
        {
            var AlphabetCollection = new StringCollection();
            int LowerBound = Convert.ToInt16('a');
            int UpperBound = Convert.ToInt16('z');

            for (int i = LowerBound, loopTo = UpperBound - 1; i <= loopTo; i++)
            {
                char DriveLetter = (char)i;
                AlphabetCollection.Add(DriveLetter.ToString());
            }

            DriveInfo[] Drives = DriveInfo.GetDrives();
            foreach (DriveInfo Drive in Drives)
                AlphabetCollection.Remove(Drive.Name[..1].ToLower());

            if (AlphabetCollection.Count > 0)
            {
                return AlphabetCollection[0]!;
            }
            else
            {
                throw new ApplicationException("No drive letter available.");
            }
        }

        public static bool IsRunningAsAdministratorOrRoot()
        {
            if (OperatingSystem.IsWindows())
            {
                var CurrentWindowsIdentity = WindowsIdentity.GetCurrent();
                var CurrentWindowsPrincipal = new WindowsPrincipal(CurrentWindowsIdentity);
                return CurrentWindowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            else // Linux / macOS
            {
                try
                {
                    return geteuid() == 0;
                }
                catch
                {
                    // Fallback
                    return string.Equals(Environment.UserName, "root", StringComparison.Ordinal);
                }
            }
        }

        public static void RunAsAdministrator()
        {

            if (OperatingSystem.IsWindows())
            {
                var NewProcessStartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Path.Combine(Environment.CurrentDirectory, "PSMultiTools.exe"),
                    Verb = "runas"
                };

                try
                {
                    var NewProcess = Process.Start(NewProcessStartInfo);
                }
                catch (Exception)
                {
                    return;
                }
            }
            else
            {
                var NewProcessStartInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/pkexec",
                    Arguments = Path.Combine(Environment.CurrentDirectory, "PSMultiTools"),
                    UseShellExecute = false
                };

                // Get desktop environment for polkit agent
                var display = Environment.GetEnvironmentVariable("DISPLAY");
                if (!string.IsNullOrEmpty(display)) NewProcessStartInfo.Environment["DISPLAY"] = display;
                var xauth = Environment.GetEnvironmentVariable("XAUTHORITY");
                if (!string.IsNullOrEmpty(xauth)) NewProcessStartInfo.Environment["XAUTHORITY"] = xauth;
                var wayland = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
                if (!string.IsNullOrEmpty(wayland)) NewProcessStartInfo.Environment["WAYLAND_DISPLAY"] = wayland;

                try
                {
                    var NewProcess = Process.Start(NewProcessStartInfo);
                }
                catch (Exception)
                {
                    return;
                }
            }

            Environment.Exit(0);
        }

        #endregion

        #region Web Functions

        public static async Task<bool> IsURLValid(string url, TimeSpan? timeout = null)
        {
            if (!NetworkInterface.GetIsNetworkAvailable()) return false;

            var cts = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(5));
            try
            {
                using var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
                var headResponse = await NewhttpClient.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false);

                if (headResponse.IsSuccessStatusCode) return true;

                if (headResponse.StatusCode == HttpStatusCode.MethodNotAllowed ||
                    headResponse.StatusCode == HttpStatusCode.NotImplemented)
                {
                    headResponse.Dispose();
                    using var getResponse = await NewhttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false);
                    return getResponse.IsSuccessStatusCode;
                }

                return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                cts.Dispose();
            }
        }

        public static string GetFilenameFromUrl(Uri FileURL)
        {
            return FileURL.Segments[^1];
        }

        public static async Task<double> WebFileSize(string sURL)
        {
            var client = new HttpClient();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, sURL);
                using var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode && response.Content.Headers.ContentLength.HasValue)
                {
                    return Math.Round(response.Content.Headers.ContentLength.Value / 1024d / 1024d, 2);
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static long GetFTPDirectorySize(FtpClient client, string path)
        {
            long FolderSize = 0L;

            foreach (var BackupFileEntry in client.GetListing(path))
            {
                if (BackupFileEntry.Type == FtpObjectType.File)
                {
                    FolderSize += BackupFileEntry.Size;
                }
                else if (BackupFileEntry.Type == FtpObjectType.Directory)
                {
                    FolderSize += GetFTPDirectorySize(client, BackupFileEntry.FullName);
                }
            }

            return FolderSize;
        }

        public static string GetPSMultiToolsVersion()
        {
            // Try different methods if one fails on a specific Linux distro, FreeBSD or in macOS
            var GetAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            try
            {
                var infoAttr = GetAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (!string.IsNullOrWhiteSpace(infoAttr?.InformationalVersion))
                    return infoAttr.InformationalVersion;
            }
            catch { }

            try
            {
                var path = GetAssembly.Location;
                if (string.IsNullOrWhiteSpace(path))
                    path = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    var fvi = FileVersionInfo.GetVersionInfo(path);
                    if (!string.IsNullOrWhiteSpace(fvi.ProductVersion))
                        return fvi.ProductVersion;
                }
            }
            catch { }

            try
            {
                var asmVer = GetAssembly.GetName().Version;
                if (asmVer != null)
                    return asmVer.ToString();
            }
            catch { }

            // Return current version if one of the methods above failed - Requires change every build!
            return "16.1.0";
        }

        public static async Task<bool> IsPSMultiToolsUpdateAvailable()
        {
            if (await IsURLValid("https://github.com/SvenGDK/PS-Multi-Tools/raw/main/LatestBuild.txt"))
            {
                var PSMultiToolsVersion = GetPSMultiToolsVersion();

                Console.WriteLine($"Current version: " + PSMultiToolsVersion); // Output in terminal

                using var VerCheckClient = new HttpClient();
                string NewPSMultiToolsVersion = await VerCheckClient.GetStringAsync("https://github.com/SvenGDK/PS-Multi-Tools/raw/main/LatestBuild.txt");

                Console.WriteLine($"Latest available version: " + NewPSMultiToolsVersion); // Output in terminal

                if (string.Compare(PSMultiToolsVersion, NewPSMultiToolsVersion, false) < 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static async void DownloadAndExecuteUpdater()
        {
            // Check if any updater exists
            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "PSMT-Update.exe")) | !File.Exists(Path.Combine(Environment.CurrentDirectory, "PSMTUpdate.sh")))
            {
                // Download if not exists
                if (OperatingSystem.IsWindows())
                {
                    using var NewHttpClient = new HttpClient();
                    var NewHttpResponseMessage = await NewHttpClient.GetAsync("https://raw.githubusercontent.com/SvenGDK/PS-Multi-Tools/main/PSMT-Update.exe");
                    if (NewHttpResponseMessage.IsSuccessStatusCode)
                    {
                        var NewStream = await NewHttpResponseMessage.Content.ReadAsStreamAsync();
                        using (var NewFileStream = new FileStream("PSMT-Update.exe", FileMode.Create))
                        {
                            NewStream.CopyTo(NewFileStream);
                        }
                        Process.Start(Path.Combine(Environment.CurrentDirectory, "PSMT-Update.exe"));
                        Environment.Exit(0);
                    }
                }
                else
                {
                    using var NewHttpClient = new HttpClient();
                    var NewHttpResponseMessage = await NewHttpClient.GetAsync("https://raw.githubusercontent.com/SvenGDK/PS-Multi-Tools/main/PSMTUpdate.sh");
                    if (NewHttpResponseMessage.IsSuccessStatusCode)
                    {
                        var NewStream = await NewHttpResponseMessage.Content.ReadAsStreamAsync();
                        using (var NewFileStream = new FileStream("PSMTUpdate.sh", FileMode.Create))
                        {
                            NewStream.CopyTo(NewFileStream);
                        }

                        string fileName = File.Exists("/bin/bash") ? "/bin/bash" : "/bin/sh";
                        string args = $"\"{Path.Combine(Environment.CurrentDirectory, "PSMTUpdate.sh")}\"";

                        ProcessStartInfo PSI = new()
                        {
                            FileName = fileName,
                            Arguments = args,
                            UseShellExecute = false,
                            WorkingDirectory = Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, "PSMTUpdate.sh"))
                        };

                        Process.Start(PSI);
                        Environment.Exit(0);
                    }
                }
            }
            else
            {
                // Execute updater
                if (OperatingSystem.IsWindows())
                {
                    Process.Start(Path.Combine(Environment.CurrentDirectory, "PSMT-Update.exe"));
                    Environment.Exit(0);
                }
                else
                {
                    string fileName = File.Exists("/bin/bash") ? "/bin/bash" : "/bin/sh";
                    string args = $"\"{Path.Combine(Environment.CurrentDirectory, "PSMTUpdate.sh")}\"";

                    ProcessStartInfo PSI = new()
                    {
                        FileName = fileName,
                        Arguments = args,
                        UseShellExecute = false,
                        WorkingDirectory = Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, "PSMTUpdate.sh"))
                    };

                    Process.Start(PSI);
                    Environment.Exit(0);
                }
            }
        }

        #endregion

        #region PS5 Related

        public static int BlockAppOrGameUpdates(string connectionString, string titleId, string contentVersion, string versionFileUri)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                int totalRows = 0;

                // Update JSON inside tbl_contentinfo.AppInfoJson using json_set
                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = @"
                    UPDATE tbl_contentinfo
                    SET AppInfoJson = json_set(
                        AppInfoJson,
                        '$.CONTENT_VERSION', $contentVersion,
                        '$.VERSION_FILE_URI', $versionFileUri
                    )
                    WHERE titleId = $titleId;
                ";
                    cmd.Parameters.AddWithValue("$contentVersion", contentVersion ?? string.Empty);
                    cmd.Parameters.AddWithValue("$versionFileUri", versionFileUri ?? string.Empty);
                    cmd.Parameters.AddWithValue("$titleId", titleId);

                    totalRows += cmd.ExecuteNonQuery();
                }

                // Update tbl_appinfo columns
                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = @"
                    UPDATE tbl_appinfo
                    SET VERSION_FILE_URI = $versionFileUri,
                        CONTENT_VERSION   = $contentVersion
                    WHERE titleId = $titleId;
                ";
                    cmd.Parameters.AddWithValue("$versionFileUri", versionFileUri ?? string.Empty);
                    cmd.Parameters.AddWithValue("$contentVersion", contentVersion ?? string.Empty);
                    cmd.Parameters.AddWithValue("$titleId", titleId);

                    totalRows += cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return totalRows;
            }
            catch
            {
                try { transaction.Rollback(); } catch { }
                throw;
            }
            finally
            {
                connection.Close();
            }
        }

        #endregion

        public static bool IncrementArray(ref byte[] sourceArray, int position)
        {
            if (sourceArray[position] == 255)
            {
                if (position != 0)
                {
                    if (IncrementArray(ref sourceArray, position - 1))
                    {
                        sourceArray[position] = 0;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                sourceArray[position] += 1;
                return true;
            }
        }

        public static long GetDirectorySize(string path)
        {
            long total = 0;
            var dir = new DirectoryInfo(path);
            if (!dir.Exists) return 0;

            try
            {
                total += dir.EnumerateFiles().Sum(f => f.Length);

                foreach (var sub in dir.EnumerateDirectories())
                {
                    total += GetDirectorySize(sub.FullName);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (PathTooLongException) { }

            return total;
        }

        public static Structures.PackageInfo GetFileSizeAndDate(string FileSize, string TheDate)
        {
            var NewPKGInfo = new Structures.PackageInfo();

            if (long.TryParse(FileSize.ToString().Trim(), out long PKGSizeStr))
            {
                NewPKGInfo.FileSize = HumanReadableBytes(PKGSizeStr);
            }
            else
            {
                NewPKGInfo.FileSize = FileSize;
            }
            if (DateTime.TryParseExact(TheDate, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out DateTime PKGDate))
            {
                NewPKGInfo.FileDate = Convert.ToString(PKGDate.Date);
            }
            else
            {
                NewPKGInfo.FileDate = TheDate;
            }

            return NewPKGInfo;
        }

        public static string GetPKGTitleID(string PKGFilePath)
        {
            string PKGID = "";
            try
            {
                var NewStringBuilder = new StringBuilder();
                if (PKGFilePath.ToLower().EndsWith(".pkg"))
                {
                    using (var PKGBinaryReader = new BinaryReader(new StreamReader(PKGFilePath).BaseStream))
                    {
                        PKGBinaryReader.BaseStream.Position = 0x30L;
                        byte[] PKGBytes = PKGBinaryReader.ReadBytes(36);
                        PKGBinaryReader.Close();
                        string str3 = Encoding.ASCII.GetString(PKGBytes);
                        if (str3.Trim().Replace("\0", "").Length >= 7)
                        {
                            NewStringBuilder.AppendLine(str3.Substring(7, 9));
                        }
                        else
                        {
                            NewStringBuilder.AppendLine("XXXX#####");
                        }
                    }
                    return NewStringBuilder.ToString().Trim();
                }
                else
                {
                    return PKGID;
                }
            }

            catch (Exception)
            {
                return PKGID;
            }
        }

        public static void ReCreateDirectoryStructure(string SourceDirectory, string TargetDirectory, string RootDirectory = "")
        {
            if (string.IsNullOrEmpty(RootDirectory))
            {
                RootDirectory = SourceDirectory;
            }
            string[] AllFolders = Directory.GetDirectories(SourceDirectory);
            foreach (string folder in AllFolders)
            {
                Directory.CreateDirectory(folder.Replace(RootDirectory, TargetDirectory));
                ReCreateDirectoryStructure(folder, TargetDirectory, RootDirectory);
            }
        }

        public static string CleanTitle(string Title)
        {
            return Title.Replace("¢", "").Replace("„", "").Replace("â", "").Replace("Â", "").Replace("Ô", "").Replace("Ê", "").Replace("ô", "").Replace("ê", "").Replace(",", "").Replace(";", "");
        }

        public static void UpdatePS5ParamEditor(PS5ParamClass.PS5Param UpdatedParams)
        {
            var AppLifetime = (Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
            foreach (Window OpenWin in AppLifetime.Windows)
            {
                if (OpenWin is PS5ParamEditor PS5ParamEditorWindow)
                {
                    PS5ParamEditorWindow.CurrentParamJson = UpdatedParams;
                    break;
                }
            }
        }

        public static void UpdatePS5ManifestEditor(PS5ManifestClass.PS5Manifest UpdatedParams)
        {
            var AppLifetime = (Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
            foreach (Window OpenWin in AppLifetime.Windows)
            {
                if (OpenWin is PS5ManifestEditor PS5ManifestEditorWindow)
                {
                    PS5ManifestEditorWindow.CurrentManifestJson = UpdatedParams;
                    break;
                }
            }
        }

        public static ScrollViewer? FindScrollViewer(Control root)
        {
            return root.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        }

        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                return;

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo @file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, @file.Name);
                @file.CopyTo(targetFilePath);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        //Always recursive
        public static void CopyDirectory(string sourceDir, string destinationDir)
        {
            DirectoryInfo dir = new(sourceDir);

            if (!dir.Exists)
                return;

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }

        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        public static bool FileExistInISO(string GameISOPath, string FileToSearch)
        {
            bool Exists = false;
            try
            {
                using var NewFileStream = new FileStream(GameISOPath, FileMode.Open, FileAccess.Read);
                var NewIso9660CDReader = new DiscUtils.Iso9660.CDReader(NewFileStream, true);
                try
                {
                    NewIso9660CDReader.OpenFile(FileToSearch, FileMode.Open);
                    Exists = true;
                }
                catch (Exception)
                {
                    Exists = false;
                }
            }
            catch (Exception)
            {
                Exists = false;
            }
            return Exists;
        }

        public static string ExtractFileFromISO9660(string ISOPath, string FileName, string DestinationPath)
        {
            string OutputDestination = "";
            string? DesinationDirectoryName = Path.GetDirectoryName(DestinationPath);
            if (!string.IsNullOrEmpty(DesinationDirectoryName))
            {
                if (!Directory.Exists(DesinationDirectoryName))
                {
                    Directory.CreateDirectory(DesinationDirectoryName);
                }
            }

            try
            {
                using var NewFileStream = new FileStream(ISOPath, FileMode.Open, FileAccess.Read);
                var NewIso9660CDReader = new DiscUtils.Iso9660.CDReader(NewFileStream, true);
                try
                {
                    var NewSparseStream = NewIso9660CDReader.OpenFile(FileName, FileMode.Open);
                    var OutputFileStream = new FileStream(DestinationPath, FileMode.Create);
                    NewSparseStream.CopyTo(OutputFileStream);
                    OutputFileStream.Close();
                    OutputDestination = DestinationPath;
                }
                catch (Exception)
                {
                    OutputDestination = "";
                }
            }
            catch (Exception)
            {
                OutputDestination = "";
            }
            return OutputDestination;
        }

        public static async Task<bool> ExtractISOFiles(string ISOFile, string CachePath, bool PS3)
        {
            try
            {
                string[] targets = [];
                if (PS3)
                {
                    targets = ["PS3_GAME/PARAM.SFO", "PS3_GAME/ICON0.PNG", "PS3_GAME/PIC1.PNG", "PS3_GAME/SND0.AT3"];
                }
                else
                {
                    targets = ["PSP_GAME/PARAM.SFO", "PSP_GAME/ICON0.PNG", "PSP_GAME/PIC1.PNG", "PSP_GAME/SND0.AT3"];
                }

                string ISOFileName = Path.GetFileNameWithoutExtension(ISOFile);
                string DestinationPath = Path.Combine(CachePath, ISOFileName);

                if (!Directory.Exists(DestinationPath))
                {
                    Directory.CreateDirectory(DestinationPath);
                }

                bool extractedAny = false;

                using (var NewFileStream = File.Open(ISOFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using var NewCDReader = new CDReader(NewFileStream, true);
                    foreach (var relPath in targets)
                    {
                        string isoEntryPath = relPath.Replace('\\', '/');

                        if (!NewCDReader.FileExists(isoEntryPath))
                        {
                            continue;
                        }

                        string destPath = Path.Combine(DestinationPath, isoEntryPath.Replace('/', Path.DirectorySeparatorChar));
                        string destDir = Path.GetDirectoryName(destPath)!;
                        Directory.CreateDirectory(destDir);

                        using (Stream ISOStream = NewCDReader.OpenFile(isoEntryPath, FileMode.Open))
                        using (FileStream OutFileStream = new(destPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            ISOStream.CopyTo(OutFileStream);
                        }

                        extractedAny = true;
                    }
                }

                return extractedAny;
            }
            catch { return false; }
        }

        public static string GetDKeyFromGameID(string HTMLKeysDatabase, string GameID)
        {
            var NewHTMLDoc = new HtmlAgilityPack.HtmlDocument();
            NewHTMLDoc.Load(HTMLKeysDatabase);

            string XPath = "//tr[td/a[normalize-space(text())='" + GameID + "']]";
            var FoundHtmlNode = NewHTMLDoc.DocumentNode.SelectSingleNode(XPath);

            if (FoundHtmlNode is not null)
            {
                var AlltdNodes = FoundHtmlNode.SelectNodes("td");
                if (AlltdNodes is not null && AlltdNodes.Count > 0)
                {
                    string GameDKey = AlltdNodes[^1].InnerText.Trim();
                    return GameDKey;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetRegionalTitleFromGameID(string HTMLKeysDatabase, string GameID)
        {
            var NewHTMLDoc = new HtmlAgilityPack.HtmlDocument();
            NewHTMLDoc.Load(HTMLKeysDatabase);

            string XPath = "//tr[td/a[normalize-space(text())='" + GameID + "']]";
            var FoundHtmlNode = NewHTMLDoc.DocumentNode.SelectSingleNode(XPath);

            if (FoundHtmlNode is not null)
            {
                var tdNodes = FoundHtmlNode.SelectNodes("td");
                if (tdNodes is not null && tdNodes.Count > 1)
                {
                    string FoundGameTitle = tdNodes[1].InnerText.Trim();
                    return Path.GetFileNameWithoutExtension(FoundGameTitle);
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public static bool RenameFolderUsingPowershell(string InputFolderPath, string NewFolderName)
        {
            var NewProcessStartInfo = new ProcessStartInfo(OperatingSystem.IsWindows() ? "powershell.exe" : "powershell")
            {
                Arguments = $"-NoProfile -Command \"Rename-Item -Path '{InputFolderPath}' -NewName '{NewFolderName}'\"",
                Verb = OperatingSystem.IsWindows() ? "runas" : "",
                UseShellExecute = true
            };
            var NewPowershellProcess = Process.Start(NewProcessStartInfo);

            if (NewPowershellProcess is null)
            {
                return false;
            }

            NewPowershellProcess.WaitForExit();

            int PowershellProcessExitCode = NewPowershellProcess.ExitCode;
            string ParentInputFolder = Path.GetDirectoryName(InputFolderPath)!;
            string NewFolderPath = Path.Combine(ParentInputFolder, NewFolderName);
            bool RenameSucceeded = PowershellProcessExitCode == 0 && !Directory.Exists(InputFolderPath) && Directory.Exists(NewFolderPath);

            return RenameSucceeded;
        }

        public static string GetDownloadsFolderPath()
        {
            if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }
            else
            {
                return "";
            }
        }

        public static void OpenDownloadsFolder()
        {
            if (OperatingSystem.IsWindows())
            {
                if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")))
                {
                    Process.Start("explorer", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));
                }
            }
            else if (OperatingSystem.IsLinux())
            {
                if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")))
                {
                    Process.Start("xdg-open", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));
                }
            }
            else if (OperatingSystem.IsMacOS())
            {
                if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")))
                {
                    Process.Start("open", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));
                }
            }
        }

        public static string EnsureTrailingSeparator(string InputFolder)
        {
            if (string.IsNullOrEmpty(InputFolder)) return InputFolder;
            string DirSep = Path.DirectorySeparatorChar.ToString();
            string AltDirSep = Path.AltDirectorySeparatorChar.ToString();

            if (!InputFolder.EndsWith(DirSep) && !InputFolder.EndsWith(AltDirSep))
            {
                InputFolder += Path.DirectorySeparatorChar;
            }
            return InputFolder;
        }

        public static async void ShowDownloadErrorMessage()
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }

        public static void OpenFolder(string FolderName)
        {
            if (OperatingSystem.IsWindows())
            {
                var psi = new ProcessStartInfo
                {
                    FileName = FolderName,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = $"\"{FolderName}\"",
                    UseShellExecute = false
                });
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    Arguments = $"\"{FolderName}\"",
                    UseShellExecute = false
                });
            }
        }

        public static string HumanReadableBytes(long bytes, bool useBinary = true)
        {
            double baseUnit = useBinary ? 1024.0 : 1000.0;
            string[] units = useBinary ? ["B", "KiB", "MiB", "GiB", "TiB"] : ["B", "kB", "MB", "GB", "TB"];

            if (bytes < 0) return "-" + HumanReadableBytes(-bytes, useBinary);
            if (bytes == 0) return "0 B";

            int exp = (int)(Math.Log(bytes) / Math.Log(baseUnit));
            exp = Math.Min(exp, units.Length - 1);
            double value = bytes / Math.Pow(baseUnit, exp);
            return $"{value:N2} {units[exp]}";
        }

        public static string? GetAvailableTerminal()
        {
            string[] Candidates = ["gnome-terminal", "tilix", "konsole", "xfce4-terminal", "terminator", "alacritty", "kitty", "xterm", "urxvt", "mate-terminal", "lxterminal", "x-terminal-emulator"];
            var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            var paths = pathEnv.Split(':', StringSplitOptions.RemoveEmptyEntries);

            foreach (var candidate in Candidates)
            {
                if (Path.IsPathRooted(candidate))
                {
                    if (File.Exists(candidate)) return candidate;
                    continue;
                }

                foreach (var dir in paths)
                {
                    try
                    {
                        var full = Path.Combine(dir, candidate);
                        if (File.Exists(full) && !IsDirectory(full) && IsExecutable(full))
                            return candidate;
                    }
                    catch
                    {

                    }
                }
            }

            return null;
        }

        static bool IsDirectory(string path) => (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;

        static bool IsExecutable(string path)
        {
            if (OperatingSystem.IsWindows()) return true;
            try
            {
                var fileInfo = new FileInfo(path);
                return fileInfo.Exists;
            }
            catch
            {
                return false;
            }
        }

        [SupportedOSPlatform("Windows")]
        public static void RegisterPKGAssociation(string exePath)
        {
            // Per-user registration (no admin required)
            using var ext = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\.pkg");
            ext.SetValue("", "PlayStation.PkgFile");

            using var prog = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\PlayStation.PkgFile");
            prog.SetValue("", "PlayStation PKG File");
            prog.CreateSubKey("DefaultIcon").SetValue("", $"\"{exePath}\",0");
            prog.CreateSubKey(@"shell\open\command").SetValue("", $"\"{exePath}\" \"%1\"");
        }

        [SupportedOSPlatform("Linux")]
        public static void RegisterPKGAssociation(string mimeXmlContent, string desktopContent)
        {
            // Write mime XML
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "mime/packages/x-PSMultiTools.xml"), mimeXmlContent);

            // Write .desktop file
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "applications/PSMultiTools.desktop"), desktopContent);

            // Run commands to update mime types
            static void ExecuteCommand(string cmd, string args)
            {
                Process NewProcess = new();
                NewProcess.StartInfo.FileName = cmd;
                NewProcess.StartInfo.Arguments = args;
                NewProcess.StartInfo.RedirectStandardOutput = true;
                NewProcess.Start();
                NewProcess.WaitForExit();
            }

            ExecuteCommand("update-mime-database", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "mime"));
            ExecuteCommand("update-desktop-database", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "applications"));
            ExecuteCommand("xdg-mime", "default myapp.desktop application/x-myapp");
        }

        #region RegexFunctions

        public static bool IsInt(string Input)
        {
            var DigitOnly = IsIntRegex();
            return DigitOnly.IsMatch(Input);
        }

        public static bool IsHex(string Input)
        {
            return IsHexRegex().IsMatch(Input);
        }

        public static int GetIntOnly(string Value)
        {
            string ReturnValue = string.Empty;
            var MatchCol = GetIntRegex().Matches(Value);
            foreach (Match m in MatchCol)
                ReturnValue += m.ToString();
            return Convert.ToInt32(ReturnValue);
        }

        [GeneratedRegex(@"^\d+$")]
        private static partial Regex IsIntRegex();
        [GeneratedRegex(@"\A\b[0-9a-fA-F]+\b\Z")]
        private static partial Regex IsHexRegex();
        [GeneratedRegex(@"\d+")]
        private static partial Regex GetIntRegex();

        #endregion

    }

}