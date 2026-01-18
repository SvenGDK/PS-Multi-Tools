using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using static PSMultiTools.Classes.Structures;
using static PSMultiTools.Classes.Utils;

namespace PSMultiTools;

public partial class InstallWindow : Window
{

    public MountedPSXDrive MountedDrive;

    private Process HDL_Dump = new();
    private string HDLGameID = "";

    public ComboBoxProjectItem? ProjectToInstall = null;
    public string CurrentProjectDirectory = "";

    public string InstallStatus = "";
    public bool InstallForPS2 = false;

    private readonly string pfsshellPath = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "pfsshell.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "pfsshell");
    private readonly string mkpartPath = Path.Combine(Environment.CurrentDirectory, "Tools", "cmdlist", "mkpart.txt");
    private readonly string pushPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "cmdlist", "push.txt");

    public InstallWindow()
    {
        InitializeComponent();

        Loaded += InstallWindow_Loaded;

        HDL_Dump.Exited += HDL_Dump_Exited;
    }

    private async void InstallWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        if (ProjectToInstall != null)
        {

            if (File.Exists(ProjectToInstall.ProjectFile))
            {

                string GameAppTitle = File.ReadAllLines(ProjectToInstall.ProjectFile)[0].Split('=')[1];
                string GameAppID = File.ReadAllLines(ProjectToInstall.ProjectFile)[1].Split('=')[1];
                string GameAppDirectory = File.ReadAllLines(ProjectToInstall.ProjectFile)[2].Split('=')[1];

                // Set cover
                if (File.Exists(Path.Combine(GameAppDirectory, "res", "jkt_001.png")))
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(GameAppDirectory, "res", "jkt_001.png"));
                        InstallImage.Source = AnyBitmapToIImage(TempBitmapImage);
                    });
                }
                else if (await IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameAppID + ".jpg"))
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        var TempBitmapImage = AnyBitmap.FromUri(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameAppID + ".jpg", UriKind.RelativeOrAbsolute));
                        InstallImage.Source = AnyBitmapToIImage(TempBitmapImage);
                    });
                }
                else if (await IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameAppID + ".jpg"))
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        var TempBitmapImage = AnyBitmap.FromUri(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameAppID + ".jpg", UriKind.RelativeOrAbsolute));
                        InstallImage.Source = AnyBitmapToIImage(TempBitmapImage);
                    });
                }
            }

            Thread.Sleep(200);

            if (InstallForPS2)
            {
                InstallPS2Game();
            }
            else
            {
                InstallPS1Game();
            }

        }
    }

    public async void InstallApp()
    {
        // Check if drive is already identified, if not get the drive name
        if (string.IsNullOrEmpty(MountedDrive.HDLDriveName))
        {
            MountedDrive.HDLDriveName = GetHDLDriveName();
            // Retry
            InstallApp();
        }
        else if (ProjectToInstall is not null)
        {
            // Proceed to installation on HDD

            // Get homebrew properties
            string HomebrewTitle = File.ReadAllLines(ProjectToInstall.ProjectFile!)[0].Split('=')[1];
            string HomebrewELF = File.ReadAllLines(ProjectToInstall.ProjectFile!)[3].Split('=')[1];
            string HomebrewPartition;

            CurrentProjectDirectory = File.ReadAllLines(ProjectToInstall.ProjectFile!)[2].Split('=')[1];

            // Set a PP partition name on known homebrew
            if (HomebrewTitle.Contains("Open PS2 Loader") | HomebrewTitle.Contains("OPL"))
            {
                HomebrewPartition = "PP.APPS-00001..OPL";
            }
            else if (HomebrewTitle.Contains("LaunchELF") | HomebrewTitle.Contains("uLE") | HomebrewTitle.Contains("wLE"))
            {
                HomebrewPartition = "PP.APPS-00002..WLE";
            }
            else if (HomebrewTitle.Contains("hdl_srv") | HomebrewTitle.Contains("hdl_server") | HomebrewTitle.Contains("hdl server"))
            {
                HomebrewPartition = "PP.APPS-00003..HDL";
            }
            else if (HomebrewTitle.Contains("SMS") | HomebrewTitle.Contains("Simple Media System"))
            {
                HomebrewPartition = "PP.APPS-00004..SMS";
            }
            else if (HomebrewTitle.Contains("GSM"))
            {
                HomebrewPartition = "PP.APPS-00005..GSM";
            }
            else
            {
                // Set own PP partition name
                HomebrewPartition = Microsoft.VisualBasic.Interaction.InputBox("Please enter a valid partition name:", "Could not determine partition for this homebrew.", "PP.APPS-00001..TITLE");
            }

            // Update UI
            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() => InstallationStatusTextBlock.Text = "Creating partition, please wait...");
            }
            else
            {
                InstallationStatusTextBlock.Text = "Creating partition, please wait...";
            }

            if (!string.IsNullOrEmpty(HomebrewPartition))
            {
                CreateHomebrewPartition(HomebrewPartition);
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Partition name cannot be empty! Please try again.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowDialogAsync(this);
                return;
            }
        }
    }

    public async void InstallPS2Game()
    {
        if (ProjectToInstall is not null)
        {
            // Proceed to installation on HDD
            // Get game properties
            string GameTitle = File.ReadAllLines(ProjectToInstall.ProjectFile!)[0].Split('=')[1];
            string GameID = File.ReadAllLines(ProjectToInstall.ProjectFile!)[1].Split('=')[1];
            string GameISO = File.ReadAllLines(ProjectToInstall.ProjectFile!)[3].Split('=')[1];

            HDLGameID = File.ReadAllLines(ProjectToInstall.ProjectFile!)[1].Split('=')[1].Replace("_", "-").Replace(".", "").Trim();
            CurrentProjectDirectory = File.ReadAllLines(ProjectToInstall.ProjectFile!)[2].Split('=')[1];

            // Set up hdl_dump
            HDL_Dump = new Process();
            HDL_Dump.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "hdl_dump.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "hdl_dump");
            HDL_Dump.StartInfo.RedirectStandardOutput = true;
            HDL_Dump.OutputDataReceived += HDLDumpOutputDataHandler;
            HDL_Dump.StartInfo.UseShellExecute = false;
            HDL_Dump.StartInfo.CreateNoWindow = true;
            HDL_Dump.EnableRaisingEvents = true;

            // Check if it's a CD or DVD and start injecting the game
            if (GetDiscType(GameISO) == DiscType.DVD)
            {
                HDL_Dump.StartInfo.Arguments = $"inject_dvd {MountedDrive.HDLDriveName} \"{GameTitle}\" \"{GameISO}\" {GameID} *u4 -hide";
                HDL_Dump.Start();
                HDL_Dump.BeginOutputReadLine();
            }
            else
            {
                HDL_Dump.StartInfo.Arguments = $"inject_cd {MountedDrive.HDLDriveName} \"{GameTitle}\" \"{GameISO}\" {GameID} *u4 -hide";
                HDL_Dump.Start();
                HDL_Dump.BeginOutputReadLine();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not load the project to install.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    public async void InstallPS1Game()
    {
        if (ProjectToInstall is not null)
        {
            // Proceed to installation on HDD
            // Get game properties
            string[] ProjectInfos = File.ReadAllLines(ProjectToInstall.ProjectFile!);
            string GameTitle = ProjectInfos[0].Split('=')[1];
            string GameID = ProjectInfos[1].Split('=')[1];
            CurrentProjectDirectory = ProjectInfos[2].Split('=')[1];
            string GameVCD = ProjectInfos[3].Split('=')[1];

            // Ask for a partition name
            string PPPartitionName = Microsoft.VisualBasic.Interaction.InputBox("Enter a valid partition name starting with PP. including the dot." + Environment.NewLine + Environment.NewLine + "WARNING: No + sign in the partition name and no whitespaces !", "Creating the game PP partition", "PP.SHORT_GAME_TITLE");
            if (!string.IsNullOrEmpty(PPPartitionName))
            {
                if (PPPartitionName.StartsWith("PP."))
                {
                    if (PPPartitionName.Length < 50)
                    {

                        if (PPPartitionName.Contains('+'))
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Unallowed character detected", "A '+' sign has been detected in the partition name and will be replaced with \"_\".", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowDialogAsync(this);
                            PPPartitionName = PPPartitionName.Replace("+", "_");
                        }

                        // Trim the final PPPartitionName
                        PPPartitionName = PPPartitionName.Trim();
                    }
                    else
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Partition name invalid", "Could not load the project to install.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowDialogAsync(this);
                        Close();
                    }
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Partition name invalid", "Partition name needs to start with \"PP.\". Please retry the installation.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowDialogAsync(this);
                    Close();
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Partition name invalid", "No partition name entered. Exiting installation.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowDialogAsync(this);
                Close();
            }

            // Calculate the required partition size
            long TotalGameSize = new FileInfo(GameVCD).Length;
            foreach (string ProjectFileLine in ProjectInfos)
            {
                if (ProjectFileLine.StartsWith("IMAGE1="))
                    TotalGameSize += new FileInfo(ProjectFileLine.Split('=')[1]).Length;
                if (ProjectFileLine.StartsWith("IMAGE2="))
                    TotalGameSize += new FileInfo(ProjectFileLine.Split('=')[1]).Length;
                if (ProjectFileLine.StartsWith("IMAGE3="))
                    TotalGameSize += new FileInfo(ProjectFileLine.Split('=')[1]).Length;
            }

            double GameTotalSizeInMB = TotalGameSize / 1024d / 1024d;
            double GameTotalSizeRoundedValue = Math.Round(GameTotalSizeInMB, 0, MidpointRounding.AwayFromZero);
            double GameRequiredPartitionSizeInMB;

            // Final partition size should be a multiple of 128MiB
            if (GameTotalSizeRoundedValue >= 1000d)
            {
                GameRequiredPartitionSizeInMB = 128 * int.Parse(GameTotalSizeRoundedValue.ToString()[..2]);
            }
            else
            {
                GameRequiredPartitionSizeInMB = 128 * int.Parse(GameTotalSizeRoundedValue.ToString()[..1]);
            }

            // Confirm partition creation
            var box2 = MessageBoxManager.GetMessageBoxStandard("Please confirm", "A new partition " + PPPartitionName + " with " + GameRequiredPartitionSizeInMB.ToString() + "M will be created." + Environment.NewLine + "Do you want to proceed with the installation ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box2.ShowWindowDialogAsync(this);
            if (boxresult == ButtonResult.Yes)
            {

                // 1. Set mkpart command for the PP partition
                using (var CommandFileWriter = new StreamWriter(mkpartPath, false))
                {
                    CommandFileWriter.WriteLine("device " + MountedDrive.DriveID);
                    CommandFileWriter.WriteLine("mkpart " + PPPartitionName + " " + GameRequiredPartitionSizeInMB.ToString() + "M PFS");
                    CommandFileWriter.WriteLine("exit");
                }

                // Update UI 
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() => InstallationStatusTextBlock.Text = "Creating game partition...");
                }
                else
                {
                    InstallationStatusTextBlock.Text = "Creating game partition...";
                }

                Thread.Sleep(200);

                // 2. Proceed to partition creation
                string PFSShellOutput;
                using (var PFSShellProcess = new Process())
                {
                    PFSShellProcess.StartInfo.FileName = "cmd";
                    PFSShellProcess.StartInfo.Arguments = $"/c type \"{mkpartPath}\" | \"{pfsshellPath}\" 2>&1";

                    PFSShellProcess.StartInfo.RedirectStandardOutput = true;
                    PFSShellProcess.StartInfo.UseShellExecute = false;
                    PFSShellProcess.StartInfo.CreateNoWindow = true;

                    PFSShellProcess.Start();

                    var ShellReader = PFSShellProcess.StandardOutput;
                    string ProcessOutput = ShellReader.ReadToEnd();

                    ShellReader.Close();
                    PFSShellOutput = ProcessOutput;
                }

                // 3. Read partition creation output
                if (PFSShellOutput.Contains("created."))
                {

                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => InstallationStatusTextBlock.Text = PPPartitionName + " created. Now adding files ...");
                    }
                    else
                    {
                        InstallationStatusTextBlock.Text = PPPartitionName + " created. Now adding files ...";
                    }

                    Thread.Sleep(200);

                    // 4. Add files to the partition
                    PS1AddFilesToPartition(PPPartitionName);
                }
                else
                {

                    var box = MessageBoxManager.GetMessageBoxStandard("Error installing game", "There was an error in creating the game's PP partition, please check if the name doesn't already exists and if you have enough space.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowDialogAsync(this);

                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => InstallationStatusTextBlock.Text = "");
                    }
                    else
                    {
                        InstallationStatusTextBlock.Text = "";
                    }

                    return;
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Installation aborted", "Exiting game installation.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowDialogAsync(this);
                Close();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error installing game", "Could not load the project to install.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void CreateGamePartition()
    {
        string CreatedGamePartition = "";

        // Get the created partition
        // 1. List partitions
        string[] QueryOutput;
        using (var HDLDump = new Process())
        {
            HDLDump.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "hdl_dump.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "hdl_dump");
            HDLDump.StartInfo.Arguments = "toc " + MountedDrive.HDLDriveName;
            HDLDump.StartInfo.RedirectStandardOutput = true;
            HDLDump.StartInfo.UseShellExecute = false;
            HDLDump.StartInfo.CreateNoWindow = true;
            HDLDump.Start();

            var OutputReader = HDLDump.StandardOutput;
            QueryOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.None);
        }

        // 2. Search for the created hidden partition
        foreach (string HDDPartition in QueryOutput)
        {
            if (!string.IsNullOrEmpty(HDDPartition))
            {
                if (HDDPartition.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries).Length >= 3)
                {
                    var FoundHDDPartition = HDDPartition.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries)[4];
                    if (FoundHDDPartition.Trim().StartsWith("__." + HDLGameID)) // The created hidden partition
                    {
                        CreatedGamePartition = FoundHDDPartition.Trim();
                        break;
                    }
                }
            }
        }

        // 3. Set mkpart command for the PP partition
        using (var CommandFileWriter = new StreamWriter(mkpartPath, false))
        {
            CommandFileWriter.WriteLine("device " + MountedDrive.DriveID);
            CommandFileWriter.WriteLine("mkpart " + CreatedGamePartition.Replace("__.", "PP.") + " 128M PFS");
            CommandFileWriter.WriteLine("exit");
        }

        // 4. Proceed to partition creation
        string PFSShellOutput;
        using (var PFSShellProcess = new Process())
        {
            PFSShellProcess.StartInfo.FileName = "cmd";
            PFSShellProcess.StartInfo.Arguments = $"/c type \"{mkpartPath}\" | \"{pfsshellPath}\" 2>&1";

            PFSShellProcess.StartInfo.RedirectStandardOutput = true;
            PFSShellProcess.StartInfo.UseShellExecute = false;
            PFSShellProcess.StartInfo.CreateNoWindow = true;

            PFSShellProcess.Start();

            var ShellReader = PFSShellProcess.StandardOutput;
            string ProcessOutput = ShellReader.ReadToEnd();

            ShellReader.Close();
            PFSShellOutput = ProcessOutput;
        }

        // 5. Read partition creation output
        if (PFSShellOutput.Contains("Main partition of 128M created."))
        {

            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() => InstallationStatusTextBlock.Text = "Partition created, modifying header...");
            }
            else
            {
                InstallationStatusTextBlock.Text = "Partition created, modifying header...";
            }

            // 6. Modify the created partition
            ModifyPartitionHeader(CreatedGamePartition.Replace("__.", "PP."), false);
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error installing game", "There was an error in creating the game's PP partition, please check if the name doesn't already exists and if you have enough space.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);

            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() => InstallationStatusTextBlock.Text = "");
            }
            else
            {
                InstallationStatusTextBlock.Text = "";
            }

            return;
        }
    }

    public async void CreateHomebrewPartition(string PartitionName)
    {
        if (ProjectToInstall is not null)
        {
            //string ProjectDirectory = File.ReadAllLines(ProjectToInstall.ProjectFile)[2].Split('=')[1];

            // 1. Set mkpart command for the PP partition
            using (var CommandFileWriter = new StreamWriter(mkpartPath, false))
            {
                CommandFileWriter.WriteLine("device " + MountedDrive.DriveID);
                CommandFileWriter.WriteLine("mkpart " + PartitionName + " 128M PFS");
                CommandFileWriter.WriteLine("exit");
            }

            // 2. Proceed to partition creation
            string PFSShellOutput;
            using (var PFSShellProcess = new Process())
            {
                PFSShellProcess.StartInfo.FileName = "cmd";
                PFSShellProcess.StartInfo.Arguments = $"/c type \"{mkpartPath}\" | \"{pfsshellPath}\" 2>&1";

                PFSShellProcess.StartInfo.RedirectStandardOutput = true;
                PFSShellProcess.StartInfo.UseShellExecute = false;

                PFSShellProcess.Start();

                var ShellReader = PFSShellProcess.StandardOutput;
                string ProcessOutput = ShellReader.ReadToEnd();

                ShellReader.Close();
                PFSShellOutput = ProcessOutput;
            }

            // 3. Read partition creation output
            if (PFSShellOutput.Contains("Main partition of 128M created."))
            {
                InstallationStatusTextBlock.Text = "Partition created, modifying header...";

                // 4. Modify the created partition
                ModifyPartitionHeader(PartitionName, false);
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error while installing homebrew", "There was an error in creating the homebrew's PP partition." + Environment.NewLine + "Please check if the partition name '" + PartitionName + "' does not already exists of if HDD space is sufficient.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowDialogAsync(this);
                return;
            }
        }
    }

    public async void ModifyPartitionHeader(string PartitionName, bool FinalizePS1)
    {
        // 1. Create a copy of hdl_dump in the project directory
        File.Copy(
            OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "hdl_dump.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "hdl_dump"),
            OperatingSystem.IsWindows() ? Path.Combine(CurrentProjectDirectory, "hdl_dump.exe") : Path.Combine(CurrentProjectDirectory, "hdl_dump"),
            true);

        // 2. Switch to project directory and inject the files
        Directory.SetCurrentDirectory(CurrentProjectDirectory);

        // 3. Modify the partition header using hdl_dump
        string HDLDumpOutput = "";
        using (var HDLDump = new Process())
        {
            HDLDump.StartInfo.FileName = OperatingSystem.IsWindows() ? "hdl_dump.exe" : "hdl_dump";
            HDLDump.StartInfo.Arguments = $"modify_header {MountedDrive.HDLDriveName} {PartitionName}";
            HDLDump.StartInfo.RedirectStandardOutput = true;
            HDLDump.StartInfo.UseShellExecute = false;
            HDLDump.StartInfo.CreateNoWindow = true;
            HDLDump.Start();

            HDLDumpOutput = HDLDump.StandardOutput.ReadToEnd();
        }

        // 4. Read hdl_dump output
        if (!HDLDumpOutput.Contains("partition not found:"))
        {
            if (FinalizePS1)
            {

                // Set the current directory back
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() => InstallationStatusTextBlock.Text = "Partition header modified. Installation is done!");
                }
                else
                {
                    InstallationStatusTextBlock.Text = "Partition header modified. Installation is done!";
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Success", "Installation completed with success!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                var boxresult = await box.ShowWindowDialogAsync(this);
                if (boxresult == ButtonResult.Ok)
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => Close());
                    }
                    else
                    {
                        Close();
                    }
                }
            }
            else // Continue for PS2
            {
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() => InstallationStatusTextBlock.Text = "Partition header modified, adding files...");
                }
                else
                {
                    InstallationStatusTextBlock.Text = "Partition header modified, adding files...";
                }

                // 5. Add files to the partition
                PS2AddFilesToPartition(PartitionName);
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error installing game", "There was an error while modifying the partition, please check if you have enough space and report the next error." + Environment.NewLine + HDLDumpOutput, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);

            // Set the current directory back
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            return;
        }
    }

    public async void PS2AddFilesToPartition(string PartitionName)
    {
        // Now put the "res" folder and EXECUTE.KELF file into the partition
        string PFSShellOutput;

        // Set the mkdir & put commands
        using (var CommandFileWriter = new StreamWriter(pushPath, false))
        {
            CommandFileWriter.WriteLine("device " + MountedDrive.DriveID);
            CommandFileWriter.WriteLine("mount " + PartitionName);
            CommandFileWriter.WriteLine("put EXECUTE.KELF");
            CommandFileWriter.WriteLine("mkdir res");
            CommandFileWriter.WriteLine("cd res");

            if (File.Exists(@"res\info.sys"))
            {
                CommandFileWriter.WriteLine(@"put res\info.sys");
                CommandFileWriter.WriteLine(@"rename res\info.sys info.sys");
            }
            if (File.Exists(@"res\jkt_001.png"))
            {
                CommandFileWriter.WriteLine(@"put res\jkt_001.png");
                CommandFileWriter.WriteLine(@"rename res\jkt_001.png jkt_001.png");
            }
            if (File.Exists(@"res\jkt_002.png"))
            {
                CommandFileWriter.WriteLine(@"put res\jkt_002.png");
                CommandFileWriter.WriteLine(@"rename res\jkt_002.png jkt_002.png");
            }
            if (File.Exists(@"res\jkt_cp.png"))
            {
                CommandFileWriter.WriteLine(@"put res\jkt_cp.png");
                CommandFileWriter.WriteLine(@"rename res\jkt_cp.png jkt_cp.png");
            }
            if (File.Exists(@"res\man.xml"))
            {
                CommandFileWriter.WriteLine(@"put res\man.xml");
                CommandFileWriter.WriteLine(@"rename res\man.xml man.xml");
            }
            if (File.Exists(@"res\notice.jpg"))
            {
                CommandFileWriter.WriteLine(@"put res\notice.jpg");
                CommandFileWriter.WriteLine(@"rename res\notice.jpg notice.jpg");
            }

            if (Directory.Exists(@"res\image"))
            {
                CommandFileWriter.WriteLine("mkdir image");
                CommandFileWriter.WriteLine("cd image");

                if (File.Exists(@"res\image\0.png"))
                {
                    CommandFileWriter.WriteLine(@"put res\image\0.png");
                    CommandFileWriter.WriteLine(@"rename res\image\0.png 0.png");
                }
                if (File.Exists(@"res\image\1.png"))
                {
                    CommandFileWriter.WriteLine(@"put res\image\1.png");
                    CommandFileWriter.WriteLine(@"rename res\image\1.png 1.png");
                }
                if (File.Exists(@"res\image\2.png"))
                {
                    CommandFileWriter.WriteLine(@"put res\image\2.png");
                    CommandFileWriter.WriteLine(@"rename res\image\2.png 2.png");
                }
            }

            CommandFileWriter.WriteLine("umount");
            CommandFileWriter.WriteLine("exit");
        }

        // Put all detected files to the partition using pfsshell
        using (var PFSShellProcess = new Process())
        {
            PFSShellProcess.StartInfo.FileName = "cmd";
            PFSShellProcess.StartInfo.Arguments = $"/c type \"{pushPath}\" | \"{pfsshellPath}\" 2>&1";
            PFSShellProcess.StartInfo.RedirectStandardOutput = true;
            PFSShellProcess.StartInfo.UseShellExecute = false;
            PFSShellProcess.StartInfo.CreateNoWindow = true;

            PFSShellProcess.Start();
            await PFSShellProcess.WaitForExitAsync();

            var PFSShellReader = PFSShellProcess.StandardOutput;
            string ProcessOutput = PFSShellReader.ReadToEnd();

            PFSShellReader.Close();
            PFSShellOutput = ProcessOutput;
        }

        // Update UI when finished
        if (Dispatcher.UIThread.CheckAccess() == false)
        {
            Dispatcher.UIThread.Invoke(() => InstallationStatusTextBlock.Text = "");
        }
        else
        {
            InstallationStatusTextBlock.Text = "";
        }

        // Set the current directory back
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        var box = MessageBoxManager.GetMessageBoxStandard("Success", "Installation completed with success!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
        var boxresult = await box.ShowWindowDialogAsync(this);
        if (boxresult == ButtonResult.Ok)
        {
            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                // Owned by different thread
                Dispatcher.UIThread.Invoke(() => Close());
            }
            else
            {
                Close();
            }
        }
    }

    public async void PS1AddFilesToPartition(string PartitionName)
    {

        // Switch to project directory and add the files
        Directory.SetCurrentDirectory(CurrentProjectDirectory);

        // Now put the game VCD(s), (DISCS.TXT) the "res" folder and EXECUTE.KELF file into the partition
        string PFSShellOutput;

        // Set the mkdir & put commands
        using (var CommandFileWriter = new StreamWriter(pushPath, false))
        {
            CommandFileWriter.WriteLine("device " + MountedDrive.DriveID);
            CommandFileWriter.WriteLine("mount " + PartitionName);
            CommandFileWriter.WriteLine("put EXECUTE.KELF");

            if (File.Exists("DISCS.TXT"))
            {
                CommandFileWriter.WriteLine("put DISCS.TXT");
            }

            if (File.Exists("IMAGE0.VCD"))
            {
                CommandFileWriter.WriteLine("put IMAGE0.VCD");
            }

            if (File.Exists("IMAGE1.VCD"))
            {
                CommandFileWriter.WriteLine("put IMAGE1.VCD");
            }

            if (File.Exists("IMAGE2.VCD"))
            {
                CommandFileWriter.WriteLine("put IMAGE2.VCD");
            }

            if (File.Exists("IMAGE3.VCD"))
            {
                CommandFileWriter.WriteLine("put IMAGE3.VCD");
            }

            CommandFileWriter.WriteLine("mkdir res");
            CommandFileWriter.WriteLine("cd res");

            if (File.Exists(@"res\info.sys"))
            {
                CommandFileWriter.WriteLine(@"put res\info.sys");
                CommandFileWriter.WriteLine(@"rename res\info.sys info.sys");
            }
            if (File.Exists(@"res\jkt_001.png"))
            {
                CommandFileWriter.WriteLine(@"put res\jkt_001.png");
                CommandFileWriter.WriteLine(@"rename res\jkt_001.png jkt_001.png");
            }
            if (File.Exists(@"res\jkt_002.png"))
            {
                CommandFileWriter.WriteLine(@"put res\jkt_002.png");
                CommandFileWriter.WriteLine(@"rename res\jkt_002.png jkt_002.png");
            }
            if (File.Exists(@"res\jkt_cp.png"))
            {
                CommandFileWriter.WriteLine(@"put res\jkt_cp.png");
                CommandFileWriter.WriteLine(@"rename res\jkt_cp.png jkt_cp.png");
            }
            if (File.Exists(@"res\man.xml"))
            {
                CommandFileWriter.WriteLine(@"put res\man.xml");
                CommandFileWriter.WriteLine(@"rename res\man.xml man.xml");
            }
            if (File.Exists(@"res\notice.jpg"))
            {
                CommandFileWriter.WriteLine(@"put res\notice.jpg");
                CommandFileWriter.WriteLine(@"rename res\notice.jpg notice.jpg");
            }

            if (Directory.Exists(@"res\image"))
            {
                CommandFileWriter.WriteLine("mkdir image");
                CommandFileWriter.WriteLine("cd image");

                if (File.Exists(@"res\image\0.png"))
                {
                    CommandFileWriter.WriteLine(@"put res\image\0.png");
                    CommandFileWriter.WriteLine(@"rename res\image\0.png 0.png");
                }
                if (File.Exists(@"res\image\1.png"))
                {
                    CommandFileWriter.WriteLine(@"put res\image\1.png");
                    CommandFileWriter.WriteLine(@"rename res\image\1.png 1.png");
                }
                if (File.Exists(@"res\image\2.png"))
                {
                    CommandFileWriter.WriteLine(@"put res\image\2.png");
                    CommandFileWriter.WriteLine(@"rename res\image\2.png 2.png");
                }
            }

            CommandFileWriter.WriteLine("umount");
            CommandFileWriter.WriteLine("exit");
        }

        if (Dispatcher.UIThread.CheckAccess() == false)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                InstallationStatusTextBlock.Text = "Adding files... This can take some time.";
                Cursor = new Cursor(StandardCursorType.Wait);
            });
        }
        else
        {
            InstallationStatusTextBlock.Text = "Adding files... This can take some time.";
            Cursor = new Cursor(StandardCursorType.Wait);
        }

        Thread.Sleep(200);

        // Put all detected files to the partition using pfsshell
        using (var PFSShellProcess = new Process())
        {
            PFSShellProcess.StartInfo.FileName = "cmd";
            PFSShellProcess.StartInfo.Arguments = $"/c type \"{pushPath}\" | \"{pfsshellPath}\" 2>&1";
            PFSShellProcess.StartInfo.RedirectStandardOutput = true;
            PFSShellProcess.StartInfo.UseShellExecute = false;
            PFSShellProcess.StartInfo.CreateNoWindow = true;

            PFSShellProcess.Start();
            await PFSShellProcess.WaitForExitAsync();

            var PFSShellReader = PFSShellProcess.StandardOutput;
            string ProcessOutput = PFSShellReader.ReadToEnd();

            PFSShellReader.Close();
            PFSShellOutput = ProcessOutput;
        }

        // Update UI when finished
        if (Dispatcher.UIThread.CheckAccess() == false)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                InstallationStatusTextBlock.Text = "Files added to the game partition. Finalizing...";
                Cursor = new Cursor(StandardCursorType.Arrow);
            });
        }
        else
        {
            InstallationStatusTextBlock.Text = "Files added to the game partition. Finalizing...";
            Cursor = new Cursor(StandardCursorType.Arrow);
        }

        Thread.Sleep(200);

        // Set the current directory back
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        // 5. Modify the partition header
        ModifyPartitionHeader(PartitionName, true);
    }

    public void HDLDumpOutputDataHandler(object? sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {

            // Update UI and show hdl_dump installation progress

            // Progress status
            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() => InstallationStatusTextBlock.Text = e.Data);
            }
            else
            {
                InstallationStatusTextBlock.Text = e.Data;
            }

            // Progress percentage
            double ProgressPercentage = 0d;
            if (Regex.Match(e.Data, @"\d\d[%]+").Success)
            {
                if (double.TryParse(Regex.Match(e.Data, @"\d\d[%]+").Value.Replace("%", ""), out ProgressPercentage) == true)
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => InstallationProgressBar.Value = ProgressPercentage);
                    }
                    else
                    {
                        InstallationProgressBar.Value = ProgressPercentage;
                    }
                }
            }

        }
    }

    private void HDL_Dump_Exited(object? sender, EventArgs e)
    {
        HDL_Dump.CancelOutputRead();
        HDL_Dump.Dispose();

        if (InstallForPS2)
        {
            // Proceed to the creation of the game's PP partition
            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() => InstallationStatusTextBlock.Text = "Creating game PP partition ...");
            }
            else
            {
                InstallationStatusTextBlock.Text = "Creating game PP partition ...";
            }
            CreateGamePartition();
        }
    }

}