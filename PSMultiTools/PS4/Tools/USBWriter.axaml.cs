using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PSMultiTools.PS4.Tools;

public partial class USBWriter : Window
{

    private USBDrive SelectedDrive = default;
    private string? SelectedDrivePath;

    public struct USBDrive
    {
        public string DriveLetter { get; set; }

        public string DriveDeviceID { get; set; }
    }

    public class LsblkRoot
    {
        public List<LsblkBlockDevice>? blockdevices { get; set; }
    }

    public class LsblkBlockDevice
    {
        public string? name { get; set; }
        public string? tran { get; set; }
        public bool rm { get; set; }
        public string? type { get; set; }
        public string? mountpoint { get; set; }
        public string? size { get; set; }
        public string? model { get; set; }
        public List<LsblkBlockDevice>? children { get; set; }
    }

    public USBWriter()
    {
        InitializeComponent();

        Loaded += USBWriter_Loaded;
        DrivesComboBox.SelectionChanged += DrivesComboBox_SelectionChanged;
    }

    private async void USBWriter_Loaded(object? sender, RoutedEventArgs e)
    {
        if (OperatingSystem.IsWindows())
        {
            // Add removable drives
            foreach (DriveInfo Drive in DriveInfo.GetDrives())
            {
                if (Drive.DriveType == DriveType.Removable)
                {
                    DrivesComboBox.Items.Add(Drive.Name);
                }
            }
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
        {
            if (!Utils.IsRunningAsAdministratorOrRoot())
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Root required", "This feature requires Root permissions. Please restart PS Multi Tools and retry.", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                await box.ShowAsync();
                IsEnabled = false;
            }
            else
            {
                string AvailableDrives = await GetLinuxDisksAsync();
                List<LsblkBlockDevice> AvailableUSBDrives = ReturnUSBDrives(AvailableDrives);

                foreach (LsblkBlockDevice FoundUSB in AvailableUSBDrives)
                {
                    DrivesComboBox.Items.Add("/dev/" + FoundUSB.name);
                }
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            string AvailableDrives = await GetMacUSBDrivesAsync();

            if (AvailableDrives != null)
            {
                string[] DiskUtilLines = AvailableDrives.Split("\n"); // Split new lines
                foreach (var DiskUtilLine in DiskUtilLines)
                {
                    if (DiskUtilLine.Trim().StartsWith("/dev")) // Get the device path line
                    {
                        DrivesComboBox.Items.Add(DiskUtilLine.Split(" ")[0]); // Get the device path only
                    }
                }
            }
        }
    }

    private async void DrivesComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DrivesComboBox.SelectedItem is not null)
        {
            if (OperatingSystem.IsWindows())
            {
                string SelectedDriveLetter = DrivesComboBox.SelectedItem.ToString()![..2];
                string SelectedDriveDeviceID = string.Empty;

                using (Process WMIC = new())
                {
                    WMIC.StartInfo.FileName = "wmic";
                    WMIC.StartInfo.Arguments = "volume get Driveletter,DeviceID";
                    WMIC.StartInfo.RedirectStandardOutput = true;
                    WMIC.StartInfo.UseShellExecute = false;
                    WMIC.StartInfo.CreateNoWindow = true;
                    WMIC.Start();

                    // Read the output
                    var OutputReader = WMIC.StandardOutput;
                    string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.None);

                    WMIC.WaitForExit();
                    WMIC.Close();

                    // Find the drive
                    foreach (string Line in ProcessOutput)
                    {
                        if (!string.IsNullOrWhiteSpace(Line))
                        {
                            if (Line.Contains(SelectedDriveLetter))
                            {
                                string DeviceID = Line.Split(["  "], StringSplitOptions.RemoveEmptyEntries)[0].Trim(); // Get the DeviceID
                                SelectedDriveDeviceID = DeviceID[..^1].Replace(@"\\?\", @"\\.\"); // Format for dd
                                break;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(SelectedDriveDeviceID))
                {
                    SelectedDrive = new USBDrive() { DriveLetter = SelectedDriveLetter, DriveDeviceID = SelectedDriveDeviceID };
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Warning", "Could not set device ID for the selected drive." + Environment.NewLine + "Please try with a different USB or do not use this tool.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
            {
                SelectedDrivePath = DrivesComboBox.SelectedItem.ToString();
            }
            else if (OperatingSystem.IsMacOS())
            {
                SelectedDrivePath = DrivesComboBox.SelectedItem.ToString();
            }
        }
    }

    private async void BrowseFileButton_Click(object? sender, RoutedEventArgs e)
    {
        var imgFileFilter = new FileDialogFilter
        {
            Name = "IMG File",
            Extensions = ["img"]
        };
        var OFD = new OpenFileDialog() { Filters = { imgFileFilter }, AllowMultiple = false, Title = "Select an .img file" };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedFileTextBox.Text = OFDResult[0];
        }
    }

    private async void RefreshButton_Click(object? sender, RoutedEventArgs e)
    {
        DrivesComboBox.Items.Clear();

        if (OperatingSystem.IsWindows())
        {
            // Add removable drives
            foreach (DriveInfo Drive in DriveInfo.GetDrives())
            {
                if (Drive.DriveType == DriveType.Removable)
                {
                    DrivesComboBox.Items.Add(Drive.Name);
                }
            }
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
        {
            string AvailableDrives = await GetLinuxDisksAsync();
            List<LsblkBlockDevice> AvailableUSBDrives = ReturnUSBDrives(AvailableDrives);

            foreach (LsblkBlockDevice FoundUSB in AvailableUSBDrives)
            {
                DrivesComboBox.Items.Add("/dev/" + FoundUSB.name);
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            string AvailableDrives = await GetMacUSBDrivesAsync();

            if (AvailableDrives != null)
            {
                string[] DiskUtilLines = AvailableDrives.Split("\n"); // Split new lines
                foreach (var DiskUtilLine in DiskUtilLines)
                {
                    if (DiskUtilLine.Trim().StartsWith("/dev")) // Get the device path line
                    {
                        DrivesComboBox.Items.Add(DiskUtilLine.Split(" ")[0]); // Get the device path only
                    }
                }
            }
        }
    }

    private async void WriteButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DrivesComboBox.SelectedItem is not null && !string.IsNullOrEmpty(SelectedFileTextBox.Text))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Do you really want to format the selected drive" + Environment.NewLine + "[" + SelectedDrive.DriveLetter + "]" + Environment.NewLine + SelectedDrive.DriveDeviceID + Environment.NewLine + "and write the selected image [" + SelectedFileTextBox.Text + "] on it ?" + " This will destroy all data on the drive !", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);
            if (boxresult == ButtonResult.Yes)
            {
                if (OperatingSystem.IsWindows())
                {
                    Process DD = new();
                    DD.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Tools", "dd.exe");
                    DD.StartInfo.Arguments = "if=" + "\"" + SelectedFileTextBox.Text + "\" of=" + SelectedDrive.DriveDeviceID + " bs=1440k";
                    DD.StartInfo.RedirectStandardOutput = true;
                    DD.StartInfo.RedirectStandardError = true;
                    DD.StartInfo.UseShellExecute = false;
                    DD.StartInfo.CreateNoWindow = true;
                    DD.Start();

                    // Read the output
                    var OutputReader = DD.StandardOutput;
                    var ErrorReader = DD.StandardError;

                    await DD.WaitForExitAsync();
                    DD.Close();

                    string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.None);
                    string[] ErrorOutput = ErrorReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.None);

                    bool InVal = false;
                    bool OutVal = false;
                    foreach (string Line in ErrorOutput)
                    {
                        if (!string.IsNullOrWhiteSpace(Line))
                        {
                            if (Line.Contains("2+1 records in"))
                            {
                                InVal = true;
                            }
                            else if (Line.Contains("2+1 records out"))
                            {
                                OutVal = true;
                            }
                        }
                    }

                    if (InVal == true & OutVal == true)
                    {
                        var box2 = MessageBoxManager.GetMessageBoxStandard("Info", "Success!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box2.ShowWindowAsync();
                    }
                    else
                    {
                        var box3 = MessageBoxManager.GetMessageBoxStandard("Error", "Error writing to USB:" + Environment.NewLine + OutputReader.ReadToEnd(), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box3.ShowWindowAsync();
                    }
                }
                else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
                {
                    if (SelectedDrivePath != null)
                    {
                        string AvailableDrives = await GetLinuxDisksAsync();
                        List<LsblkBlockDevice> AvailableUSBDrives = ReturnUSBDrives(AvailableDrives);

                        foreach (LsblkBlockDevice FoundUSB in AvailableUSBDrives)
                        {
                            // Get partitions of the selected drive
                            if ("/dev/" + FoundUSB.name == SelectedDrivePath)
                            {
                                // Unmount partitions before writing
                                foreach (var child in FoundUSB.children!)
                                {
                                    if (!string.IsNullOrEmpty(child.mountpoint))
                                    {
                                        string partitionPath = "/dev/" + child.name;
                                        await UnmountUSBLinuxAsync(partitionPath);
                                    }
                                }

                                break;
                            }
                        }

                        Thread.Sleep(2000);

                        // Write
                        var WriteResult = await WriteImageAsync(SelectedFileTextBox.Text, SelectedDrivePath);
                        if (WriteResult.Item2 != null)
                        {
                            var box2 = MessageBoxManager.GetMessageBoxStandard("Info", WriteResult.Item2, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box2.ShowWindowAsync();
                        }
                    }
                    else
                    {
                        var box3 = MessageBoxManager.GetMessageBoxStandard("Error", "No drive selected!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box3.ShowWindowAsync();
                    }
                }
                else if (OperatingSystem.IsMacOS())
                {
                    if (SelectedDrivePath != null)
                    {
                        // Unmount drive
                        await UnmountUSBMacAsync(SelectedDrivePath);

                        Thread.Sleep(2000);

                        // Write
                        var WriteResult = await WriteImageAsync(SelectedFileTextBox.Text, SelectedDrivePath);
                        if (WriteResult.Item2 != null)
                        {
                            var box2 = MessageBoxManager.GetMessageBoxStandard("Info", WriteResult.Item2, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box2.ShowWindowAsync();
                        }
                    }
                    else
                    {
                        var box3 = MessageBoxManager.GetMessageBoxStandard("Error", "No drive selected!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box3.ShowWindowAsync();
                    }
                }
            }
        }
    }

    private static async Task<(int ExitCode, string StdOut, string StdErr)> RunUSBUtilityAsync(string fileName, string args)
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
        await NewProc.WaitForExitAsync();
        NewProc.Close();
        return (NewProc.ExitCode, outp, err);
    }

    public static async Task<string> GetMacUSBDrivesAsync()
    {
        var (ExitCode, StdOut, StdErr) = await RunUSBUtilityAsync("diskutil", "list external physical | grep -i /dev/disk");
        return StdOut + StdErr;
    }

    public static async Task<string> GetLinuxDisksAsync()
    {
        var (ExitCode, StdOut, StdErr) = await RunUSBUtilityAsync("lsblk", "-J -o NAME,TRAN,RM,TYPE,MOUNTPOINT,SIZE,MODEL");
        return StdOut + StdErr;
    }

    public static async Task<(int, string, string)> UnmountUSBMacAsync(string diskIdentifier)
    {
        return await RunUSBUtilityAsync("diskutil", $"unmountDisk /dev/{diskIdentifier}");
    }

    public static async Task<(int, string, string)> UnmountUSBLinuxAsync(string devicePath)
    {
        return await RunUSBUtilityAsync("umount", devicePath + "*");
    }

    public static async Task<(int, string, string)> WriteImageAsync(string imagePath, string devicePath, string bs = "4M")
    {
        string args = $"if=\"{imagePath}\" of=\"{devicePath}\" bs={bs} conv=fsync status=progress";

        if (OperatingSystem.IsMacOS())
        {
            args = $"/bin/dd if=\"{imagePath}\" of=\"{devicePath}\" bs={bs} conv=fsync status=progress".Replace("\\", "\\\\").Replace("\"", "\\\"");
            string appleScript = $"do shell script \"{args}\" with administrator privileges";

            using Process NewProc = new();
            NewProc.StartInfo.FileName = "/usr/bin/osascript";
            NewProc.StartInfo.Arguments = $"-e \"{appleScript}\"";
            NewProc.StartInfo.RedirectStandardOutput = true;
            NewProc.StartInfo.RedirectStandardError = true;
            NewProc.StartInfo.UseShellExecute = false;
            NewProc.StartInfo.CreateNoWindow = true;
            NewProc.Start();
            string outp = NewProc.StandardOutput.ReadToEnd();
            string err = NewProc.StandardError.ReadToEnd();
            await NewProc.WaitForExitAsync();
            NewProc.Close();
            return (NewProc.ExitCode, outp, err);
        }
        else
        {
            return await RunUSBUtilityAsync("dd", args);
        }
    }

    public static List<LsblkBlockDevice> ReturnUSBDrives(string JSONData)
    {
        var NewJsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var root = JsonSerializer.Deserialize<LsblkRoot>(JSONData, NewJsonSerializerOptions);

        if (root?.blockdevices == null)
            return [];

        return [.. root.blockdevices
                .Where(d =>
                    d.type == "disk" &&
                    d.tran == "usb")];
    }

}