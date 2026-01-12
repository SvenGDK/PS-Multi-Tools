using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using IMAPI2;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

[assembly: ComVisible(true)] // Required on Windows for IMAPI2
namespace PSMultiTools.MultiPlatformTools;

public partial class BDBurner : Window
{

    public string SelectedFile = "";
    private string SelectedMacDrivePath = "";

    [SupportedOSPlatform("windows")]
    private MsftDiscFormat2Data? NewDiscDataWriter;
    private DDiscFormat2DataEvents_UpdateEventHandler? NewUpdateHandler;
    private MsftDiscRecorder2? NewRecorder;

    public class LinuxLsblkRoot
    {
        public List<LinuxLsblkBlockDevice>? blockdevices { get; set; }
    }

    public class LinuxLsblkBlockDevice
    {
        public string? name { get; set; }
        public string? tran { get; set; }
        public bool rm { get; set; }
        public string? type { get; set; }
        public string? mountpoint { get; set; }
        public string? size { get; set; }
        public string? model { get; set; }
        public List<LinuxLsblkBlockDevice>? children { get; set; }
    }

    public class MacDiscDrive
    {
        public string? DiscDriveName { get; set; }

        public string? DiscDrivePath { get; set; }
    }

    private List<MacDiscDrive> MacDiscDrives = [];

    public BDBurner()
    {
        InitializeComponent();

        Loaded += BDBurner_Loaded;
        DiscDrivesComboBox.SelectionChanged += DiscDrivesComboBox_SelectionChanged;
    }

    private void DiscDrivesComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DiscDrivesComboBox.SelectedItem != null && DiscDrivesComboBox.SelectedIndex > -1)
        {
            if (OperatingSystem.IsMacOS())
            {
                SelectedMacDrivePath = MacDiscDrives[DiscDrivesComboBox.SelectedIndex].DiscDrivePath!;
            }
        }
    }

    private async void BDBurner_Loaded(object? sender, RoutedEventArgs e)
    {
        if (OperatingSystem.IsWindows())
        {
            foreach (DriveInfo DiscDrive in DriveInfo.GetDrives())
            {
                if (DiscDrive.DriveType == DriveType.CDRom)
                {
                    DiscDrivesComboBox.Items.Add(DiscDrive.Name);
                }
            }
        }
        else if (OperatingSystem.IsLinux())
        {
            // Get disc drives using 'lsblk -I 11 --json -o NAME,TYPE,TRAN,MODEL'
            string AvailableDrives = GetLinuxDiscDrives();
            List<LinuxLsblkBlockDevice> AvailableUSBDrives = ReturnDiscDrives(AvailableDrives);

            foreach (LinuxLsblkBlockDevice FoundDrive in AvailableUSBDrives)
            {
                DiscDrivesComboBox.Items.Add("/dev/" + FoundDrive.name);
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            // Get disc drives using 'hdiutil burn -list'
            string AvailableDrives = GetMacDiscDrives();
            var FormattedOutput = new List<string>();
            if (!string.IsNullOrEmpty(AvailableDrives))
            {
                AvailableDrives = AvailableDrives.Trim();
                FormattedOutput.AddRange(AvailableDrives.Split(['\n'], StringSplitOptions.RemoveEmptyEntries));
            }
            // Format output
            if (FormattedOutput.Count > 0)
            {
                if (FormattedOutput.Count > 1)
                {
                    var DiscDrive1 = new MacDiscDrive() { DiscDriveName = FormattedOutput[0], DiscDrivePath = FormattedOutput[1].Trim() };
                    MacDiscDrives.Add(DiscDrive1);
                }
            }
            // Add to DiscDrivesComboBox
            foreach (var AvailableDrive in MacDiscDrives)
            {
                DiscDrivesComboBox.Items.Add(AvailableDrive.DiscDriveName);
            }
        }
    }

    private async void BurnDiscButton_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(DiscDrivesComboBox.Text))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No disc drive selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return;
        }

        if (string.IsNullOrEmpty(SelectedISOTextBox.Text))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No ISO file specified.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return;
        }

        if (Dispatcher.UIThread.CheckAccess() == false)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                // Set cursor & status
                Cursor = new Cursor(StandardCursorType.Wait);
                BurnStatusTextBlock.Text = "Buning disc, please wait ...";
                BurnProgressBar.IsIndeterminate = true;
            });
        }
        else
        {
            Cursor = new Cursor(StandardCursorType.Wait);
            BurnStatusTextBlock.Text = "Buning disc, please wait ...";
            BurnProgressBar.IsIndeterminate = true;
        }

        if (OperatingSystem.IsWindows())
        {
            StartBurnOnStaThread(SelectedISOTextBox.Text);
        }
        else if (OperatingSystem.IsLinux())
        {
            await BurnDiscAsyncLinux(SelectedISOTextBox.Text, DiscDrivesComboBox.Text);
        }
        else if (OperatingSystem.IsMacOS())
        {
            await BurnDiscAsyncMacOS();
        }
    }

    private async void CheckDiscButton_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(DiscDrivesComboBox.Text))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No disc drive selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return;
        }

        if (OperatingSystem.IsWindows())
        {

            var DiscMaster = new MsftDiscMaster2();
            string DiscMasterID = DiscMaster[0]; // First drive
            var DiscRecorder = new MsftDiscRecorder2();

            DiscRecorder.InitializeDiscRecorder(DiscMasterID);

            IMAPI2FS.MsftFileSystemImage FileSystemImage = new();
            MsftDiscFormat2Data DiscFormatData = new();

            string DiscMediaTypeString = "";
            long DiscSize = 0L;

            try
            {
                if (!DiscFormatData.IsCurrentMediaSupported(DiscRecorder))
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "No disc inserted or not supported !", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                    return;
                }
                else
                {
                    DiscFormatData.Recorder = DiscRecorder;

                    var DiscMediaType = DiscFormatData.CurrentPhysicalMediaType;
                    DiscMediaTypeString = GetMediaTypeString(DiscMediaType);

                    FileSystemImage.ChooseImageDefaultsForMediaType((IMAPI2FS.IMAPI_MEDIA_PHYSICAL_TYPE)DiscMediaType);

                    if (!DiscFormatData.MediaHeuristicallyBlank) // Not blank
                    {
                        FileSystemImage.MultisessionInterfaces = DiscFormatData.MultisessionInterfaces;
                        FileSystemImage.ImportFileSystem();

                        if (DiscMediaTypeString == "Blu-ray Rewritable media") // If rewritable then enable EraseDiscButton
                        {
                            EraseDiscButton.IsVisible = true;
                            EraseDiscButton.IsEnabled = true;
                        }
                    }
                    else // Blank
                    {
                        if (DiscMediaTypeString == "Blu-ray Rewritable media")
                        {
                            EraseDiscButton.IsVisible = true;
                            EraseDiscButton.IsEnabled = false;
                        }
                    }

                    long DiscFreeMediaBlocks = FileSystemImage.FreeMediaBlocks;
                    DiscSize = 2048L * DiscFreeMediaBlocks;

                    if (!string.IsNullOrEmpty(DiscMediaTypeString))
                    {
                        DiscInfoTextBlock.Text = "Disc Type: " + DiscMediaTypeString + " - Size: " + Microsoft.VisualBasic.Strings.FormatNumber(DiscSize / 1073741824d, 2) + " GB";
                    }
                }
            }

            catch (COMException ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
            finally
            {
                // Release
                if (DiscFormatData is not null)
                {
                    Marshal.ReleaseComObject(DiscFormatData);
                }
                if (FileSystemImage is not null)
                {
                    Marshal.ReleaseComObject(FileSystemImage);
                }
            }
        }
        else
        {
            DiscInfoTextBlock.Text = "Not supported yet on Linux & macOS.";
            var box = MessageBoxManager.GetMessageBoxStandard("Info", "Not supported yet on Linux & macOS.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private async void BrowseISOButton_Click(object sender, RoutedEventArgs e)
    {
        var isoFileFilter = new FileDialogFilter
        {
            Name = "ISO File",
            Extensions = ["iso"]
        };
        var OFD = new OpenFileDialog() { Title = "Select an ISO file", Filters = { isoFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedISOTextBox.Text = OFDResult[0];
            SelectedFile = OFDResult[0];
        }
    }

    private async void EreaseDiscButton_Click(object? sender, RoutedEventArgs e)
    {
        if (OperatingSystem.IsWindows())
        {
            // Initialize the drive
            var DiscMaster = new MsftDiscMaster2();
            string DiscMasterID = DiscMaster[0];

            MsftDiscRecorder2 NewDiscRecorder = new();
            NewDiscRecorder.InitializeDiscRecorder(DiscMasterID);
            NewDiscRecorder.AcquireExclusiveAccess(true, "PSMultiTools");

            // Initialize the eraser
            MsftDiscFormat2Erase eraser = new()
            {
                Recorder = NewDiscRecorder,
                ClientName = "PSMultiTools",
                FullErase = false
            };

            // Notify about erasure
            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    // Set cursor & status
                    Cursor = new Cursor(StandardCursorType.Wait);
                    BurnStatusTextBlock.Text = "Erasing disc, please wait ...";
                    BurnProgressBar.IsIndeterminate = true;
                });
            }
            else
            {
                Cursor = new Cursor(StandardCursorType.Wait);
                BurnStatusTextBlock.Text = "Erasing disc, please wait ...";
                BurnProgressBar.IsIndeterminate = true;
            }

            try
            {
                eraser.EraseMedia(); // will raise Update events and may throw COMException on failure     
            }
            catch (COMException ex)
            {
                Console.WriteLine($"Erase failed: 0x{ex.ErrorCode:X} {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erase failed: {ex.Message}");
            }
            finally
            {
                eraser.Recorder.ReleaseExclusiveAccess();
                Marshal.ReleaseComObject(eraser);
                Marshal.ReleaseComObject(NewDiscRecorder);
            }

            // Update on finish
            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    // Set cursor & status
                    Cursor = new Cursor(StandardCursorType.Arrow);
                    BurnStatusTextBlock.Text = "Disc erased!";
                    BurnProgressBar.IsIndeterminate = true;
                });
            }
            else
            {
                Cursor = new Cursor(StandardCursorType.Arrow);
                BurnStatusTextBlock.Text = "Disc erased!";
                BurnProgressBar.IsIndeterminate = true;
            }
        }

    }

    #region LinuxBurning

    public static string GetLinuxDiscDrives()
    {
        var (StdOut, StdErr) = RunCDDVDUtility("lsblk", "-I 11 --json -o NAME,TYPE,TRAN,MODEL");
        return StdOut + StdErr;
    }

    public static List<LinuxLsblkBlockDevice> ReturnDiscDrives(string JSONData)
    {
        var NewJsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var root = JsonSerializer.Deserialize<LinuxLsblkRoot>(JSONData, NewJsonSerializerOptions);

        if (root?.blockdevices == null)
            return [];

        return [.. root.blockdevices];
    }

    public async Task BurnDiscAsyncLinux(string isoPath, string devicePath, CancellationToken ct = default)
    {
        isoPath = isoPath.Replace("'", "'\\''");
        devicePath = devicePath.Replace("'", "'\\''");

        string AvailableDiscBurner = "xorriso";
        string args = $"-as cdrecord -v dev={devicePath} \"{isoPath}\"";

        var psi = new ProcessStartInfo(AvailableDiscBurner, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var DiscBurner = new Process { StartInfo = psi, EnableRaisingEvents = true };
        DiscBurner.OutputDataReceived += (s, e) => ProcessBurnLineLinux(e.Data!);
        DiscBurner.ErrorDataReceived += (s, e) => ProcessBurnLineLinux(e.Data!);
        DiscBurner.Exited += (s, e) => Dispatcher.UIThread.Invoke(() =>
        {
            BurnProgressBar.IsIndeterminate = false;
            BurnStatusTextBlock.Text = $"Burn complete. Exite Code {DiscBurner.ExitCode}";
            BurnDiscButton.IsEnabled = true;
            Cursor = new Cursor(StandardCursorType.Arrow);
        });

        Dispatcher.UIThread.Invoke(() =>
        {
            LogTextBox.Text = "";
            BurnProgressBar.IsIndeterminate = true;
            BurnStatusTextBlock.Text = "Starting burn...";
            BurnDiscButton.IsEnabled = false;
        });

        DiscBurner.Start();
        DiscBurner.BeginOutputReadLine();
        DiscBurner.BeginErrorReadLine();
        await DiscBurner.WaitForExitAsync(ct);
    }

    private void ProcessBurnLineLinux(string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        Dispatcher.UIThread.Invoke(() => LogTextBox.Text += line + Environment.NewLine);
    }

    #endregion

    #region macOSBurning

    public static string GetMacDiscDrives()
    {
        var (StdOut, StdErr) = RunCDDVDUtility("hdiutil", "burn -list");
        return StdOut + StdErr;
    }

    public async Task BurnDiscAsyncMacOS()
    {
        if (SelectedMacDrivePath != null)
        {
            var isoPath = SelectedISOTextBox.Text!.Trim();
            string selectedDrive = SelectedMacDrivePath;

            LogTextBox.Text = string.Empty;
            BurnProgressBar.IsIndeterminate = true;
            BurnProgressBar.Value = 0;
            BurnStatusTextBlock.Text = "Starting burn...";
            BurnDiscButton.IsEnabled = false;

            isoPath = isoPath.Replace("'", "'\\''");
            selectedDrive = selectedDrive.Replace("'", "'\\''");

            var args = $"-c \"hdiutil burn '{isoPath}' -device '{selectedDrive}'\"";
            var hdiutil = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/sh",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            hdiutil.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        LogTextBox.Text += e.Data + Environment.NewLine;
                    });

                    var TryGetPercent = PercentRegex.Match(e.Data);
                    if (TryGetPercent.Success && double.TryParse(TryGetPercent.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var PercentValue))
                    {
                        var ClampedValue = Math.Max(0.0, Math.Min(100.0, PercentValue));
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            BurnProgressBar.IsIndeterminate = false;
                            BurnProgressBar.Value = ClampedValue;
                            BurnStatusTextBlock.Text = $"Burning... {ClampedValue:0.0}%";
                        });
                        return;
                    }

                    if (e.Data.Contains("eject", StringComparison.OrdinalIgnoreCase) ||
                        e.Data.Contains("finished", StringComparison.OrdinalIgnoreCase) ||
                        e.Data.Contains("done", StringComparison.OrdinalIgnoreCase))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            BurnStatusTextBlock.Text = "Finished (waiting for process exit)";
                        });
                    }
                }
            };

            hdiutil.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        LogTextBox.Text += e.Data + Environment.NewLine;
                    });

                    var TryGetPercent = PercentRegex.Match(e.Data);
                    if (TryGetPercent.Success && double.TryParse(TryGetPercent.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var PercentValue))
                    {
                        var ClampedValue = Math.Max(0.0, Math.Min(100.0, PercentValue));
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            BurnProgressBar.IsIndeterminate = false;
                            BurnProgressBar.Value = ClampedValue;
                            BurnStatusTextBlock.Text = $"Burning... {ClampedValue:0.0}%";
                        });
                        return;
                    }

                    if (e.Data.Contains("eject", StringComparison.OrdinalIgnoreCase) ||
                        e.Data.Contains("finished", StringComparison.OrdinalIgnoreCase) ||
                        e.Data.Contains("done", StringComparison.OrdinalIgnoreCase))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            BurnStatusTextBlock.Text = "Finished (waiting for process exit)";
                        });
                    }
                }
            };

            hdiutil.Exited += (s, e) =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    BurnProgressBar.IsIndeterminate = false;
                    BurnStatusTextBlock.Text = $"Process exited (code {hdiutil.ExitCode})";
                    BurnDiscButton.IsEnabled = true;
                });
            };

            try
            {
                hdiutil.Start();
                hdiutil.BeginOutputReadLine();
                hdiutil.BeginErrorReadLine();
                await hdiutil.WaitForExitAsync();
            }
            catch (Exception)
            {
                await Dispatcher.UIThread.InvokeAsync(() => BurnStatusTextBlock.Text = "Failed to start burn process");
                BurnDiscButton.IsEnabled = true;
                BurnProgressBar.IsIndeterminate = false;
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not determine the selected disc drive.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    #endregion

    #region WindowsBurning

    public const uint STGM_SHARE_DENY_WRITE = 32U;

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false, EntryPoint = "SHCreateStreamOnFileW")]
    private static extern void SHCreateStreamOnFile(string fileName, uint mode, ref IStream stream);
    private static IStream GetComStreamForFile(string fileName)
    {
        IStream? stream = null;
        SHCreateStreamOnFile(fileName, STGM_SHARE_DENY_WRITE, ref stream!);
        return stream;
    }

    private static string GetMediaTypeString(IMAPI_MEDIA_PHYSICAL_TYPE mediaType)
    {
        switch (mediaType)
        {
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDROM:
                {
                    return "CD-ROM";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDR:
                {
                    return "CD-R";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDRW:
                {
                    return "CD-RW";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDROM:
                {
                    return "DVD ROM";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDRAM:
                {
                    return "DVD-RAM";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR:
                {
                    return "DVD+R";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW:
                {
                    return "DVD+RW";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR_DUALLAYER:
                {
                    return "DVD+R Dual Layer";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR:
                {
                    return "DVD-R";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHRW:
                {
                    return "DVD-RW";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR_DUALLAYER:
                {
                    return "DVD-R Dual Layer";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DISK:
                {
                    return "random-access writes";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW_DUALLAYER:
                {
                    return "DVD+RW DL";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDROM:
                {
                    return "HD DVD-ROM";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDR:
                {
                    return "HD DVD-R";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDRAM:
                {
                    return "HD DVD-RAM";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDROM:
                {
                    return "Blu-ray DVD (BD-ROM)";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDR:
                {
                    return "Blu-ray media";
                }
            case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDRE:
                {
                    return "Blu-ray Rewritable media";
                }

            default:
                {
                    return "Unknown Media Type";
                }
        }
    }

    public async void BurnProgressChanged(object sender, object progress)
    {
        IDiscFormat2DataEventArgs bProgress = (IDiscFormat2DataEventArgs)progress;

        try
        {
            switch (bProgress.CurrentAction)
            {
                case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_COMPLETED:

                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        await Dispatcher.UIThread.Invoke(async () =>
                        {
                            BurnStatusTextBlock.Text = "Burning completed!";
                            BurnProgressBar.Value = 0;
                            Cursor = new Cursor(StandardCursorType.Arrow);

                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "Burn completed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        });
                    }
                    else
                    {
                        BurnStatusTextBlock.Text = "Burning completed!";
                        BurnProgressBar.Value = 0;
                        Cursor = new Cursor(StandardCursorType.Arrow);

                        var box = MessageBoxManager.GetMessageBoxStandard("Success", "Burn completed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }

                    break;
                case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_FINALIZATION:

                    break;
                case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_WRITING_DATA:

                    int totalSectors;
                    int writtenSectors;
                    int startLba;
                    int lastWrittenLba;
                    totalSectors = bProgress.SectorCount;
                    startLba = bProgress.StartLba;
                    lastWrittenLba = bProgress.LastWrittenLba;
                    writtenSectors = lastWrittenLba - startLba;
                    int currPercent = writtenSectors / totalSectors;

                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        await Dispatcher.UIThread.Invoke(async () =>
                        {
                            BurnStatusTextBlock.Text = $"Progress: {currPercent}%";
                            BurnProgressBar.Value = (double)currPercent;
                        });
                    }
                    else
                    {
                        BurnStatusTextBlock.Text = $"Progress: {currPercent}%";
                        BurnProgressBar.Value = (double)currPercent;
                    }
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }

    }

    [SupportedOSPlatform("windows")]
    private void StartBurnOnStaThread(string ISOToBurn)
    {
        var NewBurnThread = new Thread(() => BurnDiscWindows(ISOToBurn))
        {
            IsBackground = true
        };
        NewBurnThread.SetApartmentState(ApartmentState.STA);
        NewBurnThread.Start();
    }

    [SupportedOSPlatform("windows")]
    private void BurnDiscWindows(string ISOToBurn)
    {
        IStream? burnStream = null;
        try
        {
            var discMaster = new MsftDiscMaster2();
            string discMasterId = discMaster[0];
            NewRecorder = new MsftDiscRecorder2();
            NewRecorder.InitializeDiscRecorder(discMasterId);

            NewDiscDataWriter = new MsftDiscFormat2Data
            {
                Recorder = NewRecorder,
                ClientName = "PSMultiTools"
            };

            NewUpdateHandler = new DDiscFormat2DataEvents_UpdateEventHandler(BurnProgressChanged);
            NewDiscDataWriter.Update += NewUpdateHandler;

            burnStream = GetComStreamForFile(ISOToBurn);
            if (burnStream == null)
                throw new InvalidOperationException("Failed to open burn stream.");

            NewDiscDataWriter.Write(burnStream);

            Dispatcher.UIThread.Invoke(() =>
            {
                BurnStatusTextBlock.Text = "Burn finished!";
                BurnProgressBar.IsIndeterminate = false;
            });
        }
        catch (COMException comEx)
        {
            Dispatcher.UIThread.Invoke(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", comEx.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            });
        }
        catch (Exception ex)
        {
            Dispatcher.UIThread.Invoke(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.ToString(), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            });
        }
        finally
        {
            try
            {
                if (NewDiscDataWriter != null && NewUpdateHandler != null)
                {
                    NewDiscDataWriter.Update -= NewUpdateHandler;
                }
            }
            catch { }

            if (burnStream != null)
            {
                Marshal.FinalReleaseComObject(burnStream);
                burnStream = null;
            }

            if (NewDiscDataWriter != null)
            {
                Marshal.FinalReleaseComObject(NewDiscDataWriter);
                NewDiscDataWriter = null;
            }

            if (NewRecorder != null)
            {
                Marshal.FinalReleaseComObject(NewRecorder);
                NewRecorder = null;
            }
        }
    }

    #endregion

    private static (string StdOut, string StdErr) RunCDDVDUtility(string fileName, string args)
    {
        using Process NewProc = new();
        NewProc.StartInfo.FileName = fileName;
        NewProc.StartInfo.Arguments = args;
        NewProc.StartInfo.RedirectStandardOutput = true;
        NewProc.StartInfo.RedirectStandardError = true;
        NewProc.StartInfo.UseShellExecute = false;
        NewProc.StartInfo.CreateNoWindow = true;
        NewProc.Start();
        string outp = NewProc.StandardOutput.ReadToEnd();
        string err = NewProc.StandardError.ReadToEnd();
        NewProc.WaitForExit();
        return (outp, err);
    }

    private readonly Regex PercentRegex = GenPercentRegex();
    [GeneratedRegex(@"(\d{1,3}(?:\.\d+)?)\s*%", RegexOptions.Compiled)]
    private static partial Regex GenPercentRegex();

    private static readonly Regex BytesRegex = GenBytesRegex();
    [GeneratedRegex(@"written[:=]?\s*(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex GenBytesRegex();
}