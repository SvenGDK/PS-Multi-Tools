using Avalonia.Controls;
using Avalonia.Interactivity;
using LibOrbisPkg.SFO;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PSMultiTools.MultiPlatformTools;

public partial class PSClassicsfPKGBuilder : Window
{

    readonly string WineCDrivePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c");
    readonly string CacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "Cache");

    public PSClassicsfPKGBuilder()
    {
        InitializeComponent();
        Loaded += PSClassicsfPKGBuilder_Loaded;
    }

    private async void PSClassicsfPKGBuilder_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!OperatingSystem.IsWindows())
        {
            // Prevent working in the wrong directory when insalled in usr/lib
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4")))
            {
                if (OperatingSystem.IsLinux())
                {
                    Directory.SetCurrentDirectory("/usr/lib/PSMultiTools");
                }
            }

            //  Check if a wine prefix exists
            if (!Directory.Exists(WineCDrivePath))
            {
                var WineNotInstalledMessage = MessageBoxManager.GetMessageBoxStandard("Wine installation not complete", "A wine prefix will be created, please close the Wine Configuration Tool when the initialization finished.", ButtonEnum.Ok);
                await WineNotInstalledMessage.ShowWindowDialogAsync(this);

                // Check if winetricks is updated if previously installed
                Process BashProcess = new();
                BashProcess.StartInfo.FileName = OperatingSystem.IsLinux() ? "/bin/bash" : "/bin/sh";
                BashProcess.StartInfo.Arguments = $"-c \"winecfg\"";
                BashProcess.StartInfo.RedirectStandardOutput = true;
                BashProcess.StartInfo.RedirectStandardError = true;
                BashProcess.StartInfo.UseShellExecute = false;
                BashProcess.StartInfo.CreateNoWindow = false;
                BashProcess.Start();
                await BashProcess.WaitForExitAsync();
                BashProcess.Close();
            }

            // Check if wine prefix is 64bit
            if (!Directory.Exists(Path.Combine(WineCDrivePath, "windows", "syswow64")))
            {
                var Wine64NotInstalledMessage = MessageBoxManager.GetMessageBoxStandard("Wine prefix mismatch", "Current default wine prefix is 32bit only, please change to 64bit mode before continuing.", ButtonEnum.Ok);
                await Wine64NotInstalledMessage.ShowAsync();
                return;
            }

            // Check if PS4 tools exist in current wine prefix
            if (!Directory.Exists(Path.Combine(WineCDrivePath, "PS4")))
            {
                var ToolsNotInstalledMessage = MessageBoxManager.GetMessageBoxStandard("Wine installation not complete", "Tools will now be copied to the Wine's C: drive.", ButtonEnum.Ok);
                await ToolsNotInstalledMessage.ShowAsync();

                // Copy PS4 tools to the wine C:\ drive
                Utils.CopyFilesRecursively(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "PS4"));
            }

            // Create fPKG output folder if not exists in current wine prefix
            if (!Directory.Exists(Path.Combine(WineCDrivePath, "fPKG")))
            {
                Directory.CreateDirectory(Path.Combine(WineCDrivePath, "fPKG"));
            }

            //string? AvailableTerminale = Utils.GetAvailableTerminal();
            //if (AvailableTerminale != null)
            //{
            //    var box = MessageBoxManager.GetMessageBoxStandard("Available Terminal", AvailableTerminale, ButtonEnum.Ok);
            //    await box.ShowAsync();
            //}
        }
    }

    #region PS1

    private string CurrentPS1GameID = "";

    private async void BrowsePS1IconButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "Select a PNG icon file", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "PNG files", Extensions = { "png" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedPS1IconTextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePS1StartupImageButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "Select a PNG background file", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "PNG files", Extensions = { "png" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedPS1BGTextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePS1TXTConfigButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "Select a TXT file", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "TXT files", Extensions = { "txt" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedPS1TXTConfigTextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePS1LUAConfigButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "Select a LUA file", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "LUA files", Extensions = { "lua" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedPS1LUAConfigTextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePS1Disc1Button_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "BIN files", Extensions = { "bin" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;
        if (selectedFile[0] is not null)
        {
            ReadPS1BIN(selectedFile[0]);
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("PS Classics fPKG Builder", "Aborted", ButtonEnum.Ok);
            await box.ShowWindowDialogAsync(window);
        }
    }

    private async void BrowsePS1Disc2Button_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "BIN files", Extensions = { "bin" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            PS1SelectedDisc2TextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePS1Disc3Button_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "BIN files", Extensions = { "bin" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            PS1SelectedDisc3TextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePS1Disc4Button_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "BIN files", Extensions = { "bin" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            PS1SelectedDisc4TextBox.Text = selectedFile[0];
        }
    }

    private async void BuildPS1fPKGButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        // Checks before fPKG creation
        if (string.IsNullOrEmpty(PS1SelectedDisc1TextBox.Text))
        {
            var ErrorMessageBox = MessageBoxManager.GetMessageBoxStandard("Cannot create fPKG", "No disc 1 specified, fPKG creation will be aborted.", ButtonEnum.Ok);
            await ErrorMessageBox.ShowWindowDialogAsync(window);
            return;
        }
        if (string.IsNullOrEmpty(PS1TitleTextBox.Text))
        {
            var ErrorMessageBox = MessageBoxManager.GetMessageBoxStandard("Cannot create fPKG", "No game title specified, fPKG creation will be aborted.", ButtonEnum.Ok);
            await ErrorMessageBox.ShowWindowDialogAsync(window);
            return;
        }
        if (string.IsNullOrEmpty(PS1NPTitleTextBox.Text))
        {
            var ErrorMessageBox = MessageBoxManager.GetMessageBoxStandard("Cannot create fPKG", "No NP title specified, fPKG creation will be aborted.", ButtonEnum.Ok);
            await ErrorMessageBox.ShowWindowDialogAsync(window);
            return;
        }
        if (PS1NPTitleTextBox.Text.Length != 9)
        {
            var ErrorMessageBox = MessageBoxManager.GetMessageBoxStandard("Cannot create fPKG", "'NP Title' length mismatching, only 9 characters are allowed, fPKG creation will be aborted.", ButtonEnum.Ok);
            await ErrorMessageBox.ShowWindowDialogAsync(window);
            return;
        }

        // Select output folder
        var newOpenFolderDialog = new OpenFolderDialog() { Title = "Please select an output folder" };
        var selectedFolder = await newOpenFolderDialog.ShowAsync(window);

        // Continue if selection is not empty
        if (!string.IsNullOrWhiteSpace(selectedFolder))
        {
            string PKGOutputFolder = selectedFolder;
            string GameCacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "Cache", "PS1fPKG");

            // Remove previous fPKG creation & re-create the PS1fPKG cache folder
            if (Directory.Exists(GameCacheDirectory))
            {
                Directory.Delete(GameCacheDirectory, true);
            }
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "PS1fPKG.gp4")))
            {
                File.Delete(Path.Combine(Environment.CurrentDirectory, "Cache", "PS1fPKG.gp4"));
            }
            Directory.CreateDirectory(GameCacheDirectory);

            // Copy the PS1 emulator to the cache directory
            Utils.CopyFilesRecursively(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "emus", "ps1hd"), GameCacheDirectory);

            // Copy the selected icon and background to the cache directory
            if (!Directory.Exists(Path.Combine(GameCacheDirectory, "sce_sys")))
            {
                Directory.CreateDirectory(Path.Combine(GameCacheDirectory, "sce_sys"));
            }
            if (!string.IsNullOrEmpty(SelectedPS1IconTextBox.Text))
            {
                File.Copy(SelectedPS1IconTextBox.Text, Path.Combine(GameCacheDirectory, "sce_sys", "icon0.png"), true);
            }
            if (!string.IsNullOrEmpty(SelectedPS1BGTextBox.Text))
            {
                File.Copy(SelectedPS1BGTextBox.Text, Path.Combine(GameCacheDirectory, "sce_sys", "pic0.png"), true);
            }

            // PS1 Emulator configuration
            string Disc1CueFile = "";
            using (var ConfigWriter = new StreamWriter(Path.Combine(GameCacheDirectory, "config-title.txt"), false))
            {
                ConfigWriter.WriteLine("--ps4-trophies=0");
                ConfigWriter.WriteLine("--ps5-uds=0");
                ConfigWriter.WriteLine("--trophies=0");

                // Setup discs
                Disc1CueFile = PS1SelectedDisc1TextBox.Text.Replace(".bin", ".cue");
                ConfigWriter.WriteLine("--image=\"data/disc1.bin\"");

                string Disc1BINFileName = Path.GetFileName(PS1SelectedDisc1TextBox.Text);
                string Disc1CUEFileName = Path.GetFileName(Disc1CueFile);

                // Copy disc 1 to the wine 'fPKG' folder
                if (File.Exists(PS1SelectedDisc1TextBox.Text) && !File.Exists(Path.Combine(WineCDrivePath, "fPKG", Disc1BINFileName)))
                {
                    File.Copy(PS1SelectedDisc1TextBox.Text, Path.Combine(WineCDrivePath, "fPKG", Disc1BINFileName), true);
                    File.Copy(Disc1CueFile, Path.Combine(WineCDrivePath, "fPKG", Disc1CUEFileName), true);
                }

                // Copy other discs (if not empty) to the wine 'fPKG' folder
                if (!string.IsNullOrEmpty(PS1SelectedDisc2TextBox.Text))
                {
                    string Disc2CueFile = PS1SelectedDisc2TextBox.Text.Replace(".bin", ".cue");
                    ConfigWriter.WriteLine("--image=\"data/disc2.bin\"");

                    string Disc2BINFileName = Path.GetFileName(PS1SelectedDisc2TextBox.Text);
                    string Disc2CUEFileName = Path.GetFileName(Disc2CueFile);

                    if (File.Exists(PS1SelectedDisc2TextBox.Text) && !File.Exists(Path.Combine(WineCDrivePath, "fPKG", Disc2BINFileName)))
                    {
                        File.Copy(PS1SelectedDisc2TextBox.Text, Path.Combine(WineCDrivePath, "fPKG", Disc2BINFileName), true);
                        File.Copy(Disc2CueFile, Path.Combine(WineCDrivePath, "fPKG", Disc2CUEFileName), true);
                    }
                }
                if (!string.IsNullOrEmpty(PS1SelectedDisc3TextBox.Text))
                {
                    string Disc3CueFile = PS1SelectedDisc3TextBox.Text.Replace(".bin", ".cue");
                    ConfigWriter.WriteLine("--image=\"data/disc3.bin\"");

                    string Disc3BINFileName = Path.GetFileName(PS1SelectedDisc3TextBox.Text);
                    string Disc3CUEFileName = Path.GetFileName(Disc3CueFile);

                    if (File.Exists(PS1SelectedDisc3TextBox.Text) && !File.Exists(Path.Combine(WineCDrivePath, "fPKG", Disc3BINFileName)))
                    {
                        File.Copy(PS1SelectedDisc3TextBox.Text, Path.Combine(WineCDrivePath, "fPKG", Disc3BINFileName), true);
                        File.Copy(Disc3CueFile, Path.Combine(WineCDrivePath, "fPKG", Disc3CUEFileName), true);
                    }
                }
                if (!string.IsNullOrEmpty(PS1SelectedDisc4TextBox.Text))
                {
                    string Disc4CueFile = PS1SelectedDisc4TextBox.Text.Replace(".bin", ".cue");
                    ConfigWriter.WriteLine("--image=\"data/disc4.bin\"");

                    string Disc4BINFileName = Path.GetFileName(PS1SelectedDisc4TextBox.Text);
                    string Disc4CUEFileName = Path.GetFileName(Disc4CueFile);

                    if (File.Exists(PS1SelectedDisc4TextBox.Text) && !File.Exists(Path.Combine(WineCDrivePath, "fPKG", Disc4BINFileName)))
                    {
                        File.Copy(PS1SelectedDisc4TextBox.Text, Path.Combine(WineCDrivePath, "fPKG", Disc4BINFileName), true);
                        File.Copy(Disc4CueFile, Path.Combine(WineCDrivePath, "fPKG", Disc4CUEFileName), true);
                    }
                }

                // Check for libcrypt protection
                string FormattedGameID = "";
                if (string.IsNullOrEmpty(CurrentPS1GameID))
                {
                    FormattedGameID = PS1NPTitleTextBox.Text.Insert(4, "_").Insert(8, ".");
                }
                else
                {
                    FormattedGameID = CurrentPS1GameID.Replace("-", "_").Insert(8, ".");
                }
                string ProtectionValue = PS1Game.IsGameProtected(FormattedGameID);
                if (!string.IsNullOrEmpty(ProtectionValue))
                {
                    ConfigWriter.WriteLine("--libcrypt=" + ProtectionValue);
                }

                // Check for LUA config
                if (!string.IsNullOrEmpty(SelectedPS1LUAConfigTextBox.Text))
                {
                    ConfigWriter.WriteLine("--ps1-title-id=" + PS1NPTitleTextBox.Text);
                    File.Copy(SelectedPS1LUAConfigTextBox.Text, Path.Combine(GameCacheDirectory, "scripts", PS1NPTitleTextBox.Text + ".lua"), true);
                }

                // Graphics & other configs
                if (PS1UpscalingComboBox.SelectedItem is ComboBoxItem PS1UpscalingValue && PS1UpscalingValue.Content != null)
                {
                    ConfigWriter.WriteLine("--scale=" + PS1UpscalingValue.Content.ToString());
                }

                if (PS1SkipBootlogoCheckBox.IsChecked == true)
                {
                    ConfigWriter.WriteLine("--bios-hide-sce-osd=1");
                }
                if (PS1GunconCheckBox.IsChecked == true)
                {
                    ConfigWriter.WriteLine("--guncon");
                }
                if (PS1Force60HzCheckBox.IsChecked == true)
                {
                    ConfigWriter.WriteLine("--gpu-scanout-fps-override=60");
                }
                if (PS1EmulateAnalogSticksCheckBox.IsChecked == true)
                {
                    ConfigWriter.WriteLine("--sim-analog-pad=0x2020");
                }

                // Check for TXT config
                if (!string.IsNullOrEmpty(SelectedPS1TXTConfigTextBox.Text))
                {
                    ConfigWriter.WriteLine("#User imported config");
                    ConfigWriter.WriteLine(File.ReadAllText(SelectedPS1TXTConfigTextBox.Text));
                }
            }

            // Create a new PARAM.SFO file
            var NewPS4ParamSFO = new ParamSfo();
            NewPS4ParamSFO.SetValue("APP_TYPE", SfoEntryType.Integer, "1", 4);
            NewPS4ParamSFO.SetValue("APP_VER", SfoEntryType.Utf8, "01.00", 8);
            NewPS4ParamSFO.SetValue("ATTRIBUTE", SfoEntryType.Integer, "0", 4);
            NewPS4ParamSFO.SetValue("CATEGORY", SfoEntryType.Utf8, "gd", 4);
            NewPS4ParamSFO.SetValue("CONTENT_ID", SfoEntryType.Utf8, "UP9000-" + PS1NPTitleTextBox.Text + "_00-" + PS1NPTitleTextBox.Text + "PS1FPKG", 48);
            NewPS4ParamSFO.SetValue("DOWNLOAD_DATA_SIZE", SfoEntryType.Integer, "0", 4);
            NewPS4ParamSFO.SetValue("FORMAT", SfoEntryType.Utf8, "obs", 4);
            NewPS4ParamSFO.SetValue("PARENTAL_LEVEL", SfoEntryType.Integer, "5", 4);
            NewPS4ParamSFO.SetValue("SYSTEM_VER", SfoEntryType.Integer, "0", 4);
            NewPS4ParamSFO.SetValue("TITLE", SfoEntryType.Utf8, PS1TitleTextBox.Text, 128);
            NewPS4ParamSFO.SetValue("TITLE_ID", SfoEntryType.Utf8, PS1NPTitleTextBox.Text, 12);
            NewPS4ParamSFO.SetValue("VERSION", SfoEntryType.Utf8, "01.00", 8);

            File.WriteAllBytes(Path.Combine(GameCacheDirectory, "sce_sys", "param.sfo"), NewPS4ParamSFO.Serialize());

            // Create Disc TOC file
            var GameBinFileInfo = new FileInfo(PS1SelectedDisc1TextBox.Text);
            string CUE2TOCCMD = "\"" + Path.Combine(Environment.CurrentDirectory, "Tools", "cue2toc") + "\" \"" + Disc1CueFile + "\" --size " + GameBinFileInfo.Length.ToString();
            var EscapedArgs = CUE2TOCCMD.Replace("\"", "\\\"");
            var CUE2TOCProcess = new Process();
            CUE2TOCProcess.StartInfo.FileName = OperatingSystem.IsMacOS() ? "/bin/sh" : "/bin/bash";
            CUE2TOCProcess.StartInfo.Arguments = $"-c \"{EscapedArgs}\"";
            CUE2TOCProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(Disc1CueFile);
            CUE2TOCProcess.StartInfo.UseShellExecute = false;
            CUE2TOCProcess.StartInfo.CreateNoWindow = true;
            CUE2TOCProcess.Start();
            await CUE2TOCProcess.WaitForExitAsync();
            CUE2TOCProcess.Close();

            // Move Disc TOC file
            if (!Directory.Exists(Path.Combine(GameCacheDirectory, "data")))
            {
                Directory.CreateDirectory(Path.Combine(GameCacheDirectory, "data"));
            }
            File.Move(Path.Combine(Path.GetDirectoryName(PS1SelectedDisc1TextBox.Text)!, Path.GetFileNameWithoutExtension(PS1SelectedDisc1TextBox.Text) + ".TOC"), Path.Combine(GameCacheDirectory, "data", "disc1.toc"));

            // Generate a GP4 project
            string GenGP4CCMD = "wine \"c:\\PS4\\gengp4_patch.exe\" \"c:\\Cache\\PS1fPKG\"";
            EscapedArgs = GenGP4CCMD.Replace("\"", "\\\"");
            var NewProcess = new Process();
            NewProcess.StartInfo.FileName = "/bin/bash";
            NewProcess.StartInfo.Arguments = $"-c \"{EscapedArgs}\"";
            NewProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            NewProcess.StartInfo.CreateNoWindow = true;
            NewProcess.Start();
            await NewProcess.WaitForExitAsync();
            NewProcess.Close();

            // Modify the GP4 project and add disc info
            var BaseFileNameWithExt = Path.GetFileName(PS1SelectedDisc1TextBox.Text);
            var BaseFileName = BaseFileNameWithExt.Replace(".bin", "");
            string Disc1CUEFilePath = "C:\\fPKG\\" + BaseFileName + ".cue";
            string Disc1BINFilePath = "C:\\fPKG\\" + BaseFileName + ".bin";

            string XMLDisc1CuePath = $"\n    <file targ_path=\"data/disc1.cue\" orig_path=\"{Disc1CUEFilePath}\" pfs_compression=\"enable\"/>";
            string XMLDisc1BinPath = $"\n    <file targ_path=\"data/disc1.bin\" orig_path=\"{Disc1BINFilePath}\" pfs_compression=\"enable\"/>";

            var fileContents = File.ReadAllText(Path.Combine(CacheDirectory, "PS1fPKG.gp4"));
            fileContents = fileContents.Replace("<?xml version=\"1.1\"", "<?xml version=\"1.0\"");
            fileContents = fileContents.Replace("<scenarios default_id=\"1\">", "<scenarios default_id=\"0\">");
            fileContents = fileContents.Replace("</files>", $"{XMLDisc1CuePath}{XMLDisc1BinPath}\n</files>");
            File.WriteAllText(Path.Combine(CacheDirectory, "PS1fPKG.gp4"), fileContents);

            if (!string.IsNullOrEmpty(PS1SelectedDisc2TextBox.Text))
            {
                BaseFileNameWithExt = Path.GetFileName(PS1SelectedDisc2TextBox.Text);
                BaseFileName = BaseFileNameWithExt.Replace(".bin", "");
                string Disc2CUEFilePath = "C:\\fPKG\\" + BaseFileName + ".cue";
                string Disc2BINFilePath = "C:\\fPKG\\" + BaseFileName + ".bin";

                string XMLDisc2CuePath = $"\n    <file targ_path=\"data/disc2.cue\" orig_path=\"{Disc2CUEFilePath}\" pfs_compression=\"enable\"/>";
                string XMLDisc2BinPath = $"\n    <file targ_path=\"data/disc2.bin\" orig_path=\"{Disc2BINFilePath}\" pfs_compression=\"enable\"/>";

                fileContents = File.ReadAllText(Path.Combine(CacheDirectory, "PS1fPKG.gp4"));
                fileContents = fileContents.Replace("</files>", $"{XMLDisc2CuePath}{XMLDisc2BinPath}\n</files>");
                File.WriteAllText(Path.Combine(CacheDirectory, "PS1fPKG.gp4"), fileContents);
            }
            if (!string.IsNullOrEmpty(PS1SelectedDisc3TextBox.Text))
            {
                BaseFileNameWithExt = Path.GetFileName(PS1SelectedDisc3TextBox.Text);
                BaseFileName = BaseFileNameWithExt.Replace(".bin", "");
                string Disc3CUEFilePath = "C:\\fPKG\\" + BaseFileName + ".cue";
                string Disc3BINFilePath = "C:\\fPKG\\" + BaseFileName + ".bin";

                string XMLDisc3CuePath = $"\n    <file targ_path=\"data/disc3.cue\" orig_path=\"{Disc3CUEFilePath}\" pfs_compression=\"enable\"/>";
                string XMLDisc3BinPath = $"\n    <file targ_path=\"data/disc3.bin\" orig_path=\"{Disc3BINFilePath}\" pfs_compression=\"enable\"/>";

                fileContents = File.ReadAllText(Path.Combine(CacheDirectory, "PS1fPKG.gp4"));
                fileContents = fileContents.Replace("</files>", $"{XMLDisc3CuePath}{XMLDisc3BinPath}\n</files>");
                File.WriteAllText(Path.Combine(CacheDirectory, "PS1fPKG.gp4"), fileContents);
            }
            if (!string.IsNullOrEmpty(PS1SelectedDisc4TextBox.Text))
            {
                BaseFileNameWithExt = Path.GetFileName(PS1SelectedDisc4TextBox.Text);
                BaseFileName = BaseFileNameWithExt.Replace(".bin", "");
                string Disc4CUEFilePath = "C:\\fPKG\\" + BaseFileName + ".cue";
                string Disc4BINFilePath = "C:\\fPKG\\" + BaseFileName + ".bin";

                string XMLDisc4CuePath = $"\n    <file targ_path=\"data/disc4.cue\" orig_path=\"{Disc4CUEFilePath}\" pfs_compression=\"enable\"/>";
                string XMLDisc4BinPath = $"\n    <file targ_path=\"data/disc4.bin\" orig_path=\"{Disc4BINFilePath}\" pfs_compression=\"enable\"/>";

                fileContents = File.ReadAllText(Path.Combine(CacheDirectory, "PS1fPKG.gp4"));
                fileContents = fileContents.Replace("</files>", $"{XMLDisc4CuePath}{XMLDisc4BinPath}\n</files>");
                File.WriteAllText(Path.Combine(CacheDirectory, "PS1fPKG.gp4"), fileContents);
            }

            var DebugMessageBox = MessageBoxManager.GetMessageBoxStandard("PS fPKG Classics Builder", "All files ready!\r\nfPKG can be build.", ButtonEnum.Ok);
            await DebugMessageBox.ShowWindowDialogAsync(window);

            // Create the fPKG
            string PUBCMD = "wine \"c:\\PS4\\orbis-pub-cmd-3.38.exe\" img_create --oformat pkg --skip_digest --no_progress_bar \"c:\\Cache\\PS1fPKG.gp4\" \"c:\\fPKG\"";
            EscapedArgs = PUBCMD.Replace("\"", "\\\"");
            var PKGBuilderProcess = new Process();
            PKGBuilderProcess.StartInfo.FileName = OperatingSystem.IsMacOS() ? "/bin/sh" : "/bin/bash";
            PKGBuilderProcess.StartInfo.Arguments = $"-c \"{EscapedArgs}\"";
            PKGBuilderProcess.StartInfo.UseShellExecute = false;
            PKGBuilderProcess.StartInfo.RedirectStandardOutput = true;
            PKGBuilderProcess.StartInfo.CreateNoWindow = true;
            PKGBuilderProcess.Start();

            var NewStreamReader = PKGBuilderProcess.StandardOutput;
            string PKGBuilderProcessOutput = NewStreamReader.ReadToEnd();

            PKGBuilderProcess.WaitForExit();
            PKGBuilderProcess.Close();

            if (PKGBuilderProcessOutput.Contains("Create image Process finished with warning"))
            {
                string PKGFileName = "UP9000-" + PS1NPTitleTextBox.Text + "_00-" + PS1NPTitleTextBox.Text + "PS1FPKG-A0100-V0100.pkg";
                string PKGFilePath = WineCDrivePath + "/fPKG/" + PKGFileName;
                if (File.Exists(PKGFilePath))
                {
                    try
                    {
                        File.Move(PKGFilePath, PKGOutputFolder + "/" + PKGFileName);
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error);
                    }
                }

                var PKGBuildMessageBox = MessageBoxManager.GetMessageBoxStandard("DEBUG", "fPKG created with success!", ButtonEnum.Ok);
                await PKGBuildMessageBox.ShowWindowDialogAsync(window);
            }
            else
            {
                var PKGBuildMessageBox = MessageBoxManager.GetMessageBoxStandard("DEBUG", "Error creating fPKG", ButtonEnum.Ok);
                await PKGBuildMessageBox.ShowWindowDialogAsync(window);
            }

        }
        else { return; }

    }

    public async void ReadPS1BIN(string GameBIN)
    {
        string GameID = "";
        string GameTitle = "";

        using Process Bash = new();

        var StringsCMD = $"strings \"{GameBIN}\" | fgrep BOOT";
        var EscapedArgs = StringsCMD.Replace("\"", "\\\"");

        Bash.StartInfo.FileName = OperatingSystem.IsMacOS() ? "/bin/sh" : "/bin/bash";
        Bash.StartInfo.Arguments = $"-c \"{EscapedArgs}\"";
        Bash.StartInfo.RedirectStandardOutput = true;
        Bash.StartInfo.RedirectStandardError = true;
        Bash.StartInfo.UseShellExecute = false;
        Bash.StartInfo.CreateNoWindow = true;
        Bash.Start();

        var OutputReader = Bash.StandardOutput;
        string[] ProcessOutput = OutputReader.ReadToEnd().Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries);

        await Bash.WaitForExitAsync();
        Bash.Close();

        if (ProcessOutput.Length > 0)
        {
            foreach (var OutputLine in ProcessOutput)
            {
                if (OutputLine.Contains("BOOT =") | OutputLine.Contains("BOOT="))
                {
                    GameID = OutputLine.Replace(@"BOOT = cdrom:\", "").Replace(@"BOOT=cdrom:\", "").Replace("BOOT = cdrom:", "").Replace(";1", "").Replace("_", "-").Replace(".", "").Replace(@"MGS\", "").Trim();
                    GameID = GameID.ToUpper().Trim();

                    // Try to get a game title from the master list
                    if (GameID.Length == 10)
                    {
                        GameTitle = PS1Game.GetPS1GameTitleFromDatabaseList(GameID);
                    }

                    break;
                }
            }
        }

        if (!string.IsNullOrEmpty(GameID))
        {
            CurrentPS1GameID = GameID.Trim();
            PS1NPTitleTextBox.Text = GameID.Replace("-", "").Trim();
            PS1SelectedDisc1TextBox.Text = GameBIN;
        }
        if (!string.IsNullOrEmpty(GameTitle))
        {
            PS1TitleTextBox.Text = GameTitle;
        }

    }

    #endregion

    #region PS2

    private string CurrentPS2GameID = "";
    private string CurrentPS2GameCRC = "";

    private async void BrowsePS2Disc1Button_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "Select a PS2 ISO file.", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "ISO files", Extensions = { "iso" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;
        if (selectedFile[0] is not null)
        {
            string PS2GameID = await PS2Game.GetPS2GameIDAsync(selectedFile[0]);
            string ExtractedPS2GameELFPath = GetELFfromISO(selectedFile[0], PS2GameID);
            CurrentPS2GameID = PS2GameID;

            string PS2GameCRC = await GetGameCRCAsync(ExtractedPS2GameELFPath);
            PS2GameCRC = CRCRegex().Replace(PS2GameCRC, "");
            CurrentPS2GameCRC = PS2GameCRC;

            if (!string.IsNullOrEmpty(PS2GameID) && !string.IsNullOrEmpty(PS2GameCRC))
            {
                // Check for existing TXT config in DB
                if (IsConfigAvailable(PS2GameID, Environment.CurrentDirectory + @"/Tools/PS4/ps2-configs/configs_txt.dat"))
                {
                    PS2AddTXTConfigFromDatabaseCheckBox.IsChecked = true;
                    PS2AddTXTConfigFromDatabaseCheckBox.IsVisible = true;
                }
                else
                {
                    PS2AddTXTConfigFromDatabaseCheckBox.IsChecked = false;
                    PS2AddTXTConfigFromDatabaseCheckBox.IsVisible = false;
                }
                // Check for existing LUA config in DB
                if (IsConfigAvailable(PS2GameID, Environment.CurrentDirectory + @"/Tools/PS4/ps2-configs/configs_lua.dat"))
                {
                    PS2AddLUAConfigFromDatabaseCheckBox.IsChecked = true;
                    PS2AddLUAConfigFromDatabaseCheckBox.IsVisible = true;
                }
                else
                {
                    PS2AddLUAConfigFromDatabaseCheckBox.IsChecked = false;
                    PS2AddLUAConfigFromDatabaseCheckBox.IsVisible = false;
                }
                // Check for existing PS3 config in DB
                if (IsConfigAvailable(PS2GameID + ".CONFIG", Environment.CurrentDirectory + @"/Tools/PS4/ps2-configs/configs_ps3.dat"))
                {
                    PS2AddPS3ConfigFromDatabaseCheckBox.IsChecked = true;
                    PS2AddPS3ConfigFromDatabaseCheckBox.IsVisible = true;
                }
                else
                {
                    PS2AddPS3ConfigFromDatabaseCheckBox.IsChecked = false;
                    PS2AddPS3ConfigFromDatabaseCheckBox.IsVisible = false;
                }
                // Check for existing Widescreen patch in DB
                if (IsConfigAvailable(PS2GameCRC + ".lua", Environment.CurrentDirectory + @"/Tools/PS4/ps2-configs/widescreen.dat"))
                {
                    PS2UseWidescreenPatchCheckBox.IsChecked = true;
                    PS2UseWidescreenPatchCheckBox.IsEnabled = true;
                }
                else
                {
                    PS2UseWidescreenPatchCheckBox.IsEnabled = false;
                }

                PS2TitleTextBox.Text = PS2Game.GetPS2GameTitleFromDatabaseList(PS2GameID.Replace(".", "").Replace("_", "-").Trim());
                PS2NPTitleTextBox.Text = PS2GameID.Replace(".", "").Replace("_", "").Trim();
                SelectedDisc1TextBox.Text = selectedFile[0];
            }
            else
            {
                var NoGameIDMessage = MessageBoxManager.GetMessageBoxStandard("PS Classics fPKG Builder", "Could not find the PS2 game ID within the ISO file.\r\nDo you want to use this file anyway ?", ButtonEnum.YesNo);
                if (NoGameIDMessage.ShowWindowDialogAsync(window).Result == ButtonResult.Yes)
                {
                    SelectedDisc1TextBox.Text = selectedFile[0];
                }
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("PS Classics fPKG Builder", "Aborted", ButtonEnum.Ok);
            await box.ShowWindowDialogAsync(window);
        }

    }

    private async void BrowsePS2Disc2Button_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "ISO files", Extensions = { "iso" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedDisc2TextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePS2Disc3Button_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "ISO files", Extensions = { "iso" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedDisc3TextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePS2Disc4Button_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "ISO files", Extensions = { "iso" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedDisc4TextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePS2Disc5Button_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "ISO files", Extensions = { "iso" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedDisc5TextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePS2TXTConfigButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "TXT files", Extensions = { "txt" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedDisc5TextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePS2LUAConfigButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "LUA files", Extensions = { "lua" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedDisc5TextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePS2MCButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "BIN files", Extensions = { "bin" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedDisc5TextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePS2IconButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "Select a PNG icon file", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "PNG files", Extensions = { "png" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedIconTextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePS2StartupImageButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "Select a PNG background file", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "PNG files", Extensions = { "png" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedStartupImageTextBox.Text = selectedFile[0];
        }
    }

    private async void BuildPS2fPKGButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        // Checks before fPKG creation
        if (string.IsNullOrEmpty(SelectedDisc1TextBox.Text))
        {
            var ErrorMessageBox = MessageBoxManager.GetMessageBoxStandard("Cannot create fPKG", "No disc 1 specified, fPKG creation will be aborted.", ButtonEnum.Ok);
            await ErrorMessageBox.ShowWindowDialogAsync(window);
            return;
        }
        if (string.IsNullOrEmpty(PS2TitleTextBox.Text))
        {
            var ErrorMessageBox = MessageBoxManager.GetMessageBoxStandard("Cannot create fPKG", "No game title specified, fPKG creation will be aborted.", ButtonEnum.Ok);
            await ErrorMessageBox.ShowWindowDialogAsync(window);
            return;
        }
        if (string.IsNullOrEmpty(PS2NPTitleTextBox.Text))
        {
            var ErrorMessageBox = MessageBoxManager.GetMessageBoxStandard("Cannot create fPKG", "No NP title specified, fPKG creation will be aborted.", ButtonEnum.Ok);
            await ErrorMessageBox.ShowWindowDialogAsync(window);
            return;
        }
        if (PS2NPTitleTextBox.Text.Length != 9)
        {
            var ErrorMessageBox = MessageBoxManager.GetMessageBoxStandard("Cannot create fPKG", "'NP Title' length mismatching, only 9 characters are allowed, fPKG creation will be aborted.", ButtonEnum.Ok);
            await ErrorMessageBox.ShowWindowDialogAsync(window);
            return;
        }

        // Get disc count
        int DiscCount = 0;
        if (!string.IsNullOrEmpty(SelectedDisc1TextBox.Text))
        {
            DiscCount += 1;
        }
        if (!string.IsNullOrEmpty(SelectedDisc2TextBox.Text))
        {
            DiscCount += 1;
        }
        if (!string.IsNullOrEmpty(SelectedDisc3TextBox.Text))
        {
            DiscCount += 1;
        }
        if (!string.IsNullOrEmpty(SelectedDisc4TextBox.Text))
        {
            DiscCount += 1;
        }
        if (!string.IsNullOrEmpty(SelectedDisc5TextBox.Text))
        {
            DiscCount += 1;
        }

        // Select output folder
        var newOpenFolderDialog = new OpenFolderDialog() { Title = "Please select an output folder" };
        var selectedFolder = await newOpenFolderDialog.ShowAsync(window);

        if (selectedFolder != null)
        {
            string SelectedPS2Emulator = "";
            if (PS2EmulatorComboBox.SelectedItem is ComboBoxItem SelectedPS2EmulatorComboBoxItem)
            {
                if (SelectedPS2EmulatorComboBoxItem.Content != null)
                {
                    SelectedPS2Emulator = SelectedPS2EmulatorComboBoxItem.Content.ToString() ?? "";
                }
            }

            string FullPS2GameID = CurrentPS2GameID.Replace(".", "").Replace("_", "-").Trim();
            string PKGOutputFolder = selectedFolder;
            string GameCacheDirectory = Path.Combine(WineCDrivePath, "Cache", "PS2fPKG");

            // Remove previous fPKG creation & re-create the PS1fPKG cache folder
            if (Directory.Exists(GameCacheDirectory))
            {
                Directory.Delete(GameCacheDirectory, true);
            }
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "PS2fPKG.gp4")))
            {
                File.Delete(Path.Combine(Environment.CurrentDirectory, "Cache", "PS2fPKG.gp4"));
            }
            Directory.CreateDirectory(GameCacheDirectory);

            // Copy the PS2 emulator to the cache directory
            Utils.CopyFilesRecursively(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "emus", SelectedPS2Emulator), GameCacheDirectory);

            // Copy the selected icon and background to the cache directory
            if (!Directory.Exists(Path.Combine(GameCacheDirectory, "sce_sys")))
            {
                Directory.CreateDirectory(Path.Combine(GameCacheDirectory, "sce_sys"));
            }
            if (!string.IsNullOrEmpty(SelectedPS1IconTextBox.Text))
            {
                var IconBytes = File.ReadAllBytes(SelectedPS1IconTextBox.Text);
                var ConvertedIcon = Utils.ConvertTo24bppPNG(IconBytes, 512, 512);
                ConvertedIcon.Save(Path.Combine(GameCacheDirectory, "sce_sys", "icon0.png"));

                //File.Copy(SelectedPS1IconTextBox.Text, GameCacheDirectory + @"/sce_sys/icon0.png", true);
            }
            if (!string.IsNullOrEmpty(SelectedPS1BGTextBox.Text))
            {
                var BGBytes = File.ReadAllBytes(SelectedPS1BGTextBox.Text);
                var ConvertedBG = Utils.ConvertTo24bppPNG(BGBytes, 1920, 1080);
                ConvertedBG.Save(Path.Combine(GameCacheDirectory, "sce_sys", "pic0.png"));

                //File.Copy(SelectedPS1BGTextBox.Text, GameCacheDirectory + @"/sce_sys/pic0.png", true);
            }

            // Create a new PARAM.SFO file
            var NewPS4ParamSFO = new ParamSfo();
            NewPS4ParamSFO.SetValue("APP_TYPE", SfoEntryType.Integer, "1", 4);
            NewPS4ParamSFO.SetValue("APP_VER", SfoEntryType.Utf8, "01.00", 8);
            NewPS4ParamSFO.SetValue("ATTRIBUTE", SfoEntryType.Integer, "0", 4);
            NewPS4ParamSFO.SetValue("CATEGORY", SfoEntryType.Utf8, "gd", 4);
            NewPS4ParamSFO.SetValue("CONTENT_ID", SfoEntryType.Utf8, "UP9000-" + PS2NPTitleTextBox.Text + "_00-" + CurrentPS2GameID.Replace(".", "").Replace("_", "").Trim() + "0000001", 48);
            NewPS4ParamSFO.SetValue("DOWNLOAD_DATA_SIZE", SfoEntryType.Integer, "0", 4);
            NewPS4ParamSFO.SetValue("FORMAT", SfoEntryType.Utf8, "obs", 4);
            NewPS4ParamSFO.SetValue("PARENTAL_LEVEL", SfoEntryType.Integer, "5", 4);
            NewPS4ParamSFO.SetValue("REMOTE_PLAY_KEY_ASSIGN", SfoEntryType.Integer, "0", 4);
            NewPS4ParamSFO.SetValue("SYSTEM_VER", SfoEntryType.Integer, "0", 4);
            NewPS4ParamSFO.SetValue("TITLE", SfoEntryType.Utf8, PS2TitleTextBox.Text, 128);
            NewPS4ParamSFO.SetValue("TITLE_ID", SfoEntryType.Utf8, PS2NPTitleTextBox.Text, 12);
            NewPS4ParamSFO.SetValue("VERSION", SfoEntryType.Utf8, "01.00", 8);

            File.WriteAllBytes(Path.Combine(GameCacheDirectory, "sce_sys", "param.sfo"), NewPS4ParamSFO.Serialize());

            // Create a new PS2 emulator configuration file
            string UprenderValue = "";
            if (PS2EmulatorComboBox.SelectedItem is ComboBoxItem UprenderComboBoxItem)
            {
                if (UprenderComboBoxItem.Content != null)
                {
                    UprenderValue = UprenderComboBoxItem.Content.ToString() ?? "";
                }
            }
            string UpscalingValue = "";
            if (PS2EmulatorComboBox.SelectedItem is ComboBoxItem UpscalingComboBoxItem)
            {
                if (UpscalingComboBoxItem.Content != null)
                {
                    UprenderValue = UpscalingComboBoxItem.Content.ToString() ?? "";
                }
            }
            string DisplayModeValue = "";
            if (PS2EmulatorComboBox.SelectedItem is ComboBoxItem DisplayModeComboBoxItem)
            {
                if (DisplayModeComboBoxItem.Content != null)
                {
                    UprenderValue = DisplayModeComboBoxItem.Content.ToString() ?? "";
                }
            }

            string[] NewPS2EmulatorConfig = [ "--path-vmc=\"/tmp/vmc\"\r\n--config-local-lua=\"\"\r\n--ps2-title-id=",
                    FullPS2GameID, "\r\n--max-disc-num=",
                    DiscCount.ToString(),
                    "\r\n--gs-uprender=",
                    UprenderValue.ToLower(),
                    "\r\n--gs-upscale=",
                    UpscalingValue.ToLower(),
                    "\r\n--host-audio=1\r\n--rom=\"PS20220WD20050620.crack\"\r\n--verbose-cdvd-reads=0\r\n--host-display-mode=",
                    DisplayModeValue.ToLower() ];
            string NewPS2EmulatorConfigContent = string.Concat(NewPS2EmulatorConfig);

            // Reset on disc change config
            if (PS2RestartEmulatorOnDiscChangeCheckBox.IsChecked == false)
            {
                NewPS2EmulatorConfigContent += "\r\n#Disable emu reset on disc change\r\n--switch-disc-reset=0";
            }

            // Multitap config
            if (PS2MultitapComboBox.SelectedIndex == 1)
            {
                NewPS2EmulatorConfigContent += "\r\n#Enable Multitap\r\n--mtap1=always";
            }
            else if (PS2MultitapComboBox.SelectedIndex == 2)
            {
                NewPS2EmulatorConfigContent += "\r\n#Enable Multitap\r\n--mtap2=always";
            }
            else if (PS2MultitapComboBox.SelectedIndex == 3)
            {
                NewPS2EmulatorConfigContent += "\r\n#Enable Multitap\r\n--mtap1=always\r\n--mtap2=always";
            }

            // Check for PS3 config file
            if (PS2AddPS3ConfigFromDatabaseCheckBox.IsChecked == true)
            {
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "ps2-configs", "ps3", CurrentPS2GameID + ".CONFIG")))
                {
                    NewPS2EmulatorConfigContent += "\r\n--lopnor-config=1";

                    // Create patches directory
                    if (!Directory.Exists(Path.Combine(GameCacheDirectory, "patches", FullPS2GameID)))
                    {
                        Directory.CreateDirectory(Path.Combine(GameCacheDirectory, "patches", FullPS2GameID));
                    }

                    File.Copy(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "ps2-configs", "ps3", CurrentPS2GameID + ".CONFIG"), Path.Combine(GameCacheDirectory, "patches", FullPS2GameID, FullPS2GameID + "_lopnor.cfgbin"), true);
                }

                else if (IsConfigAvailable(CurrentPS2GameID + ".CONFIG", Environment.CurrentDirectory + @"/Tools/PS4/ps2-configs/configs_ps3.dat"))
                {
                    if (!Directory.Exists(Path.Combine(GameCacheDirectory, "patches", FullPS2GameID)))
                    {
                        Directory.CreateDirectory(Path.Combine(GameCacheDirectory, "patches", FullPS2GameID));
                    }

                    ExtractFileFromISO(Environment.CurrentDirectory + @"/Tools/PS4/ps2-configs/configs_ps3.dat", CurrentPS2GameID + ".CONFIG", Path.Combine(GameCacheDirectory, "patches", FullPS2GameID, FullPS2GameID + "_lopnor.cfgbin"));
                }
            }

            // Widescreen Patch cnofig
            if (PS2UseWidescreenPatchCheckBox.IsChecked == true)
            {
                string WidescreenPatch = "";

                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "ps2-configs", "widescreen", CurrentPS2GameCRC + ".lua")))
                {
                    WidescreenPatch = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "ps2-configs", "widescreen", CurrentPS2GameCRC + ".lua"));
                }
                else if (IsConfigAvailable(CurrentPS2GameCRC + ".lua", Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "ps2-configs", "widescreen.dat")))
                {
                    WidescreenPatch = GetPNACHFromDAT(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "ps2-configs", "widescreen.dat"), CurrentPS2GameCRC + ".lua");
                }

                if (!string.IsNullOrEmpty(WidescreenPatch))
                {
                    NewPS2EmulatorConfigContent += "\r\n--path-trophydata=\"/app0/trophy_data\"";

                    if (!Directory.Exists(Path.Combine(GameCacheDirectory, "trophy_data")))
                    {
                        Directory.CreateDirectory(Path.Combine(GameCacheDirectory, "trophy_data"));
                    }

                    File.WriteAllText(Path.Combine(GameCacheDirectory, "trophy_data", FullPS2GameID + "_trophies.lua"), WidescreenPatch);
                }
            }

            // Copy lua_include to cache directory
            Utils.CopyFilesRecursively(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "lua_include"), Path.Combine(GameCacheDirectory, "lua_include"));

            // Check for LUA config
            if (!string.IsNullOrEmpty(SelectedPS2LUAConfigTextBox.Text))
            {
                NewPS2EmulatorConfigContent += "\r\n--path-patches=\"/app0/patches";

                // Create patches directory if it doesn't exist yet
                if (!Directory.Exists(Path.Combine(GameCacheDirectory, "patches", FullPS2GameID)))
                {
                    Directory.CreateDirectory(Path.Combine(GameCacheDirectory, "patches", FullPS2GameID));
                }

                File.Copy(SelectedPS2LUAConfigTextBox.Text, Path.Combine(GameCacheDirectory, "patches", FullPS2GameID + "_config.lua"), true);
            }
            else if (PS2AddLUAConfigFromDatabaseCheckBox.IsChecked == true)
            {
                if (IsConfigAvailable(CurrentPS2GameID, Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "ps2-configs", "configs_lua.dat")))
                {
                    NewPS2EmulatorConfigContent += "\r\n--path-patches=\"/app0/patches";

                    if (!Directory.Exists(Path.Combine(GameCacheDirectory, "patches", FullPS2GameID)))
                    {
                        Directory.CreateDirectory(Path.Combine(GameCacheDirectory, "patches", FullPS2GameID));
                    }

                    ExtractFileFromISO(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "ps2-configs", "configs_lua.dat"), CurrentPS2GameID, Path.Combine(GameCacheDirectory, "patches", FullPS2GameID + "_config.lua"));
                }
            }

            // Emulator fixes
            string PS2EmulatorFixes = "";
            if (PS2ImproveSpeedCheckBox.IsChecked == true)
            {
                PS2EmulatorFixes += "\r\n#Improve Speed\r\n-vu0-opt-flags=1\r\n--vu1-opt-flags=1\r\n--cop2-opt-flags=1\r\n--vu0-const-prop=0\r\n--vu1-const-prop=0\r\n--vu1-jr-cache-policy=newprog\r\n--vu1-jalr-cache-policy=newprog\r\n--vu0-jr-cache-policy=newprog\r\n--vu0-jalr-cache-policy=newprog";
            }
            if (PS2FixGraphicsCheckBox.IsChecked == true)
            {
                PS2EmulatorFixes += "\r\n#Fix Graphics\r\n--fpu-no-clamping=0\r\n--fpu-clamp-results=1\r\n--vu0-no-clamping=0\r\n--vu0-clamp-results=1\r\n--vu1-no-clamping=0\r\n--vu1-clamp-results=1\r\n--cop2-no-clamping=0\r\n--cop2-clamp-results=1";
            }
            if (PS2DisableMTVUCheckBox.IsChecked == true)
            {
                PS2EmulatorFixes += "\r\n#Disable MTVU\r\n--vu1=jit-sync";
            }
            if (PS2DisableInstantVIF1TransferCheckBox.IsChecked == true)
            {
                PS2EmulatorFixes += "\r\n#Disable Instant VIF1 Transfer\r\n--vif1-instant-xfer=0";
            }

            // Check for TXT config
            if (!string.IsNullOrEmpty(SelectedPS2TXTConfigTextBox.Text))
            {
                string[] ModifiedPS2EmulatorConfigContent = [NewPS2EmulatorConfigContent, "\r\n", PS2EmulatorFixes, "\r\n#User Config\r\n", File.ReadAllText(SelectedPS2TXTConfigTextBox.Text)];
                NewPS2EmulatorConfigContent = string.Concat(ModifiedPS2EmulatorConfigContent);
            }
            else if (PS2AddTXTConfigFromDatabaseCheckBox.IsChecked == true)
            {
                if (IsConfigAvailable(CurrentPS2GameID, Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "ps2-configs", "configs_txt.dat")))
                {
                    NewPS2EmulatorConfigContent = NewPS2EmulatorConfigContent + "\r\n#" + CurrentPS2GameID + "\r\n" + GetPNACHFromDAT(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "ps2-configs", "configs_txt.dat"), CurrentPS2GameID);
                }
            }
            else
            {
                // Append only PS2EmulatorFixes
                NewPS2EmulatorConfigContent = NewPS2EmulatorConfigContent + "\r\n" + PS2EmulatorFixes;
            }

            // Write config-emu-ps4.txt
            if (File.Exists(Path.Combine(GameCacheDirectory, "config-emu-ps4.txt")))
            {
                File.Delete(Path.Combine(GameCacheDirectory, "config-emu-ps4.txt"));
            }
            File.WriteAllText(Path.Combine(GameCacheDirectory, "config-emu-ps4.txt"), NewPS2EmulatorConfigContent);

            if (!Directory.Exists(Path.Combine(GameCacheDirectory, "feature_data")))
            {
                Directory.CreateDirectory(Path.Combine(GameCacheDirectory, "feature_data"));
            }
            if (!Directory.Exists(Path.Combine(GameCacheDirectory, "feature_data", FullPS2GameID)))
            {
                Directory.CreateDirectory(Path.Combine(GameCacheDirectory, "feature_data", FullPS2GameID));
            }

            // Check for PS2 Memory Card file
            if (!string.IsNullOrEmpty(SelectedPS2MemoryCardTextBox.Text))
            {
                File.Copy(SelectedPS2MemoryCardTextBox.Text, Path.Combine(GameCacheDirectory, "feature_data", FullPS2GameID, "custom.card"), true);
            }

            if (!Directory.Exists(Path.Combine(GameCacheDirectory, "image")))
            {
                Directory.CreateDirectory(Path.Combine(GameCacheDirectory, "image"));
            }

            // Generate a GP4 project
            string GenGP4CCMD = "wine \"c:\\PS4\\gengp4_patch.exe\" \"c:\\Cache\\PS2fPKG\"";
            var EscapedArgs = GenGP4CCMD.Replace("\"", "\\\"");
            var NewProcess = new Process();
            NewProcess.StartInfo.FileName = OperatingSystem.IsMacOS() ? "/bin/sh" : "/bin/bash";
            NewProcess.StartInfo.Arguments = $"-c \"{EscapedArgs}\"";
            NewProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            NewProcess.StartInfo.CreateNoWindow = true;
            NewProcess.Start();
            await NewProcess.WaitForExitAsync();
            NewProcess.Close();

            // Modify the GP4 project and add disc info
            File.WriteAllText(Path.Combine(CacheDirectory, "PS2fPKG.gp4"), File.ReadAllText(Path.Combine(CacheDirectory, "PS2fPKG.gp4")).Replace("<?xml version=\"1.1\"", "<?xml version=\"1.0\""));
            File.WriteAllText(Path.Combine(CacheDirectory, "PS2fPKG.gp4"), File.ReadAllText(Path.Combine(CacheDirectory, "PS2fPKG.gp4")).Replace("<scenarios default_id=\"1\">", "<scenarios default_id=\"0\">"));

            // Copy selected discs to the inner fPKG folder (if not exists) and add to GP4 project file
            string FullDiscInfo = "";
            string Disc1FileNameWithExtension = Path.GetFileName(SelectedDisc1TextBox.Text);
            string Disc1FileNameWithoutExtension = Path.GetFileNameWithoutExtension(Disc1FileNameWithExtension);
            string Disc1ISOFilePath = "C:\\fPKG\\" + Disc1FileNameWithoutExtension + ".iso";

            string[] Disc1Info = [FullDiscInfo, $"\n    <file targ_path=\"image/disc01.iso\" orig_path=\"{Disc1ISOFilePath}\" pfs_compression=\"enable\"/>"];
            FullDiscInfo = string.Join("", Disc1Info);

            if (!File.Exists(Path.Combine(WineCDrivePath, "fPKG", Disc1FileNameWithExtension)))
            {
                File.Copy(SelectedDisc1TextBox.Text, Path.Combine(WineCDrivePath, "fPKG", Disc1FileNameWithExtension));
            }

            if (!string.IsNullOrEmpty(SelectedDisc2TextBox.Text))
            {
                string Disc2FileNameWithExtension = Path.GetFileName(SelectedDisc2TextBox.Text);
                string Disc2FileNameWithoutExtension = Path.GetFileNameWithoutExtension(Disc2FileNameWithExtension);
                string Disc2ISOFilePath = "C:\\fPKG\\" + Disc2FileNameWithoutExtension + ".iso";

                string[] Disc2Info = [FullDiscInfo, $"\n    <file targ_path=\"image/disc02.iso\" orig_path=\"{Disc2ISOFilePath}\" pfs_compression=\"enable\"/>"];
                FullDiscInfo = string.Join("", Disc2Info);

                if (!File.Exists(Path.Combine(WineCDrivePath, "fPKG", Disc2FileNameWithExtension)))
                {
                    File.Copy(SelectedDisc2TextBox.Text, Path.Combine(WineCDrivePath, "fPKG", Disc2FileNameWithExtension));
                }
            }
            if (!string.IsNullOrEmpty(SelectedDisc3TextBox.Text))
            {
                string Disc3FileNameWithExtension = Path.GetFileName(SelectedDisc3TextBox.Text);
                string Disc3FileNameWithoutExtension = Path.GetFileNameWithoutExtension(Disc3FileNameWithExtension);
                string Disc3ISOFilePath = "C:\\fPKG\\" + Disc3FileNameWithoutExtension + ".iso";

                string[] Disc3Info = [FullDiscInfo, $"\n    <file targ_path=\"image/disc03.iso\" orig_path=\"{Disc3ISOFilePath}\" pfs_compression=\"enable\"/>"];
                FullDiscInfo = string.Join("", Disc3Info);

                if (!File.Exists(Path.Combine(WineCDrivePath, "fPKG", Disc3FileNameWithExtension)))
                {
                    File.Copy(SelectedDisc3TextBox.Text, Path.Combine(WineCDrivePath, "fPKG", Disc3FileNameWithExtension));
                }
            }
            if (!string.IsNullOrEmpty(SelectedDisc4TextBox.Text))
            {
                string Disc4FileNameWithExtension = Path.GetFileName(SelectedDisc4TextBox.Text);
                string Disc4FileNameWithoutExtension = Path.GetFileNameWithoutExtension(Disc4FileNameWithExtension);
                string Disc4ISOFilePath = "C:\\fPKG\\" + Disc4FileNameWithoutExtension + ".iso";

                string[] Disc4Info = [FullDiscInfo, $"\n    <file targ_path=\"image/disc04.iso\" orig_path=\"{Disc4ISOFilePath}\" pfs_compression=\"enable\"/>"];
                FullDiscInfo = string.Join("", Disc4Info);

                if (!File.Exists(Path.Combine(WineCDrivePath, "fPKG", Disc4FileNameWithExtension)))
                {
                    File.Copy(SelectedDisc4TextBox.Text, Path.Combine(WineCDrivePath, "fPKG", Disc4FileNameWithExtension));
                }
            }
            if (!string.IsNullOrEmpty(SelectedDisc5TextBox.Text))
            {
                string Disc5FileNameWithExtension = Path.GetFileName(SelectedDisc5TextBox.Text);
                string Disc5FileNameWithoutExtension = Path.GetFileNameWithoutExtension(Disc5FileNameWithExtension);
                string Disc5ISOFilePath = "C:\\fPKG\\" + Disc5FileNameWithoutExtension + ".iso";

                string[] Disc5Info = [FullDiscInfo, $"\n    <file targ_path=\"image/disc05.iso\" orig_path=\"{Disc5ISOFilePath}\" pfs_compression=\"enable\"/>"];
                FullDiscInfo = string.Join("", Disc5Info);

                if (!File.Exists(Path.Combine(WineCDrivePath, "fPKG", Disc5FileNameWithExtension)))
                {
                    File.Copy(SelectedDisc5TextBox.Text, Path.Combine(WineCDrivePath, "fPKG", Disc5FileNameWithExtension));
                }
            }

            File.WriteAllText(Path.Combine(CacheDirectory, "PS2fPKG.gp4"), File.ReadAllText(Path.Combine(CacheDirectory, "PS2fPKG.gp4")).Replace("</files>", $"{FullDiscInfo}\n</files>"));

            var DebugMessageBox = MessageBoxManager.GetMessageBoxStandard("PS fPKG Classics Builder", "All files ready! \r\n fPKG can be build.", ButtonEnum.Ok);
            await DebugMessageBox.ShowWindowDialogAsync(window);

            // Create the fPKG
            string PUBCMD = "wine \"c:\\PS4\\orbis-pub-cmd-3.38.exe\" img_create --oformat pkg --skip_digest --no_progress_bar \"c:\\Cache\\PS2fPKG.gp4\" \"c:\\fPKG\"";
            EscapedArgs = PUBCMD.Replace("\"", "\\\"");

            Process PKGBuilderProcess = new();
            PKGBuilderProcess.StartInfo.FileName = OperatingSystem.IsMacOS() ? "/bin/sh" : "/bin/bash";
            PKGBuilderProcess.StartInfo.Arguments = $"-c \"{EscapedArgs}\"";
            PKGBuilderProcess.StartInfo.UseShellExecute = false;
            PKGBuilderProcess.StartInfo.RedirectStandardOutput = true;
            PKGBuilderProcess.StartInfo.CreateNoWindow = true;
            PKGBuilderProcess.Start();

            var NewStreamReader = PKGBuilderProcess.StandardOutput;
            string PKGBuilderProcessOutput = NewStreamReader.ReadToEnd();

            await PKGBuilderProcess.WaitForExitAsync();
            PKGBuilderProcess.Close();

            if (PKGBuilderProcessOutput.Contains("Create image Process finished with warning"))
            {
                string PKGFileName = "UP9000-" + PS2NPTitleTextBox.Text + "_00-" + CurrentPS2GameID.Replace(".", "").Replace("_", "").Trim() + "0000001" + "-A0100-V0100.pkg";
                string PKGFilePath = Path.Combine(WineCDrivePath, "fPKG", PKGFileName);
                if (File.Exists(PKGFilePath))
                {
                    try
                    {
                        File.Move(PKGFilePath, Path.Combine(PKGOutputFolder, PKGFileName));
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error);
                    }
                }

                var PKGBuildMessageBox = MessageBoxManager.GetMessageBoxStandard("DEBUG", "fPKG created with success!", ButtonEnum.Ok);
                await PKGBuildMessageBox.ShowWindowDialogAsync(window);
            }
            else
            {
                DebugMessageBox = MessageBoxManager.GetMessageBoxStandard("DEBUG", "Error creating fPKG", ButtonEnum.Ok);
                await DebugMessageBox.ShowWindowDialogAsync(window);
            }
        }
        else { return; }

    }

    private static bool IsConfigAvailable(string GameID, string ConfigDatabaseFile)
    {
        bool Exists;
        try
        {
            using var NewFileStream = new FileStream(ConfigDatabaseFile, FileMode.Open, FileAccess.Read);
            var NewUdfReader = new DiscUtils.Udf.UdfReader(NewFileStream, 2048);
            try
            {
                NewUdfReader.OpenFile(GameID, FileMode.Open);
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

    private static string GetELFfromISO(string GameISOFile, string GameELFName)
    {
        string ExtractedELFPath = "";

        string CacheDir = Path.Combine(Environment.CurrentDirectory, "Cache");
        if (!Directory.Exists(CacheDir))
        {
            Directory.CreateDirectory(CacheDir);
        }

        try
        {
            using var NewFileStream = new FileStream(GameISOFile, FileMode.Open, FileAccess.Read);
            var NewCDReader = new DiscUtils.Iso9660.CDReader(NewFileStream, true);
            try
            {
                var ELFExtractionFileStream = new FileStream(Path.Combine(CacheDir, GameELFName), FileMode.Create);
                NewCDReader.OpenFile(GameELFName, FileMode.Open).CopyTo(ELFExtractionFileStream);

                ExtractedELFPath = Path.Combine(CacheDir, GameELFName);
            }
            catch (Exception)
            {
                ExtractedELFPath = "";
            }
        }
        catch (Exception)
        {
            return ExtractedELFPath;
        }
        return ExtractedELFPath;
    }

    private static async Task<string> GetGameCRCAsync(string PS2GamePath)
    {
        // Get ELF crc
        string CRCCCMD = "\"" + Path.Combine(Environment.CurrentDirectory, "Tools", "crc") + "\" \"" + PS2GamePath + "\"";
        var EscapedArgs = CRCCCMD.Replace("\"", "\\\"");
        var CRCProcess = new Process();

        CRCProcess.StartInfo.FileName = OperatingSystem.IsMacOS() ? "/bin/sh" : "/bin/bash";
        CRCProcess.StartInfo.Arguments = $"-c \"{EscapedArgs}\"";
        CRCProcess.StartInfo.UseShellExecute = false;
        CRCProcess.StartInfo.CreateNoWindow = true;
        CRCProcess.StartInfo.RedirectStandardOutput = true;
        CRCProcess.Start();
        await CRCProcess.WaitForExitAsync();
        CRCProcess.Close();

        using var NewStreamReader = CRCProcess.StandardOutput;
        return NewStreamReader.ReadToEnd().Replace("crc:", "").Trim();
    }

    private static string ExtractFileFromISO(string GameISOFile, string FileToExtract, string FileDestinationPath)
    {
        string ReturnedFileDestinationPath;
        try
        {
            using var NewFileStream = new FileStream(GameISOFile, FileMode.Open, FileAccess.Read);
            var NewUdfReader = new DiscUtils.Udf.UdfReader(NewFileStream, 2048);
            try
            {
                var NewFileStream2 = new FileStream(FileDestinationPath, FileMode.Create);
                NewUdfReader.OpenFile(FileToExtract, FileMode.Open).CopyTo(NewFileStream2);
                NewFileStream2.Close();
                ReturnedFileDestinationPath = FileDestinationPath;
            }
            catch (Exception)
            {
                ReturnedFileDestinationPath = "";
            }
        }
        catch (Exception)
        {
            ReturnedFileDestinationPath = "";
        }
        return ReturnedFileDestinationPath;
    }

    private static string GetPNACHFromDAT(string DATFile, string FileToExtract)
    {
        string PNACHString;
        try
        {
            using var NewFileStream = new FileStream(DATFile, FileMode.Open, FileAccess.Read);
            var NewUdfReader = new DiscUtils.Udf.UdfReader(NewFileStream, 2048);
            try
            {
                PNACHString = new StreamReader(NewUdfReader.OpenFile(FileToExtract, FileMode.Open)).ReadToEnd();
            }
            catch (Exception)
            {
                PNACHString = "";
            }
        }
        catch (Exception)
        {
            PNACHString = "";
        }
        return PNACHString;
    }

    #endregion

    #region PSP

    private async void BrowsePSPIconButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "Select a PNG icon file", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "PNG files", Extensions = { "png" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedPSPIconTextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePSPBGButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "Select a PNG background file", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "PNG files", Extensions = { "png" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedPSPBGImageTextBox.Text = selectedFile[0];
        }
    }

    private async void BrowsePSPDiscButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "Select a PSP ISO file.", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "ISO files", Extensions = { "iso" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;
        if (selectedFile[0] is not null)
        {
            if (FileExistInISO(selectedFile[0], @"\PSP_GAME\PARAM.SFO"))
            {
                string CacheDir = Environment.CurrentDirectory + @"/Cache";
                string ExtractedUMDDataPath = ExtractFileFromPSPISO(selectedFile[0], "UMD_DATA.BIN", CacheDir + @"/temp_umd_data.bin");

                if (!string.IsNullOrEmpty(ExtractedUMDDataPath))
                {
                    PSPNPTitleTextBox.Text = System.Text.Encoding.ASCII.GetString(ReadUMDData(CacheDir + @"/temp_umd_data.bin", 0L, 10)).Replace("-", "");
                    PSPTitleTextBox.Text = PSPTitleRegex1().Replace(Path.GetFileNameWithoutExtension(selectedFile[0]), "");
                    PSPTitleTextBox.Text = PSPTitleRegex2().Replace(PSPTitleTextBox.Text, "");
                }

                SelectedPSPDiscTextBox.Text = selectedFile[0];
            }
            else
            {
                var NoValidFileMessage = MessageBoxManager.GetMessageBoxStandard("PS fPKG Classics Builder", "Could not find any PSP game information within the ISO file.\r\nDo you want to use this file anyway ?", ButtonEnum.YesNo);
                var NoValidFileMessageValue = await NoValidFileMessage.ShowWindowDialogAsync(window);
                if (NoValidFileMessageValue == ButtonResult.Yes)
                {
                    SelectedPSPDiscTextBox.Text = selectedFile[0];
                }
            }
        }
    }

    private async void BrowsePSPConfigButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "TXT files", Extensions = { "txt" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;

        if (selectedFile[0] is not null)
        {
            SelectedDisc5TextBox.Text = selectedFile[0];
        }
    }

    private async void BuildPSPfPKGButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        // Checks before fPKG creation
        if (string.IsNullOrEmpty(SelectedPSPDiscTextBox.Text))
        {
            var ErrorMessageBox = MessageBoxManager.GetMessageBoxStandard("Cannot create fPKG", "No disc specified, fPKG creation will be aborted.", ButtonEnum.Ok);
            await ErrorMessageBox.ShowWindowDialogAsync(window);
            return;
        }
        if (string.IsNullOrEmpty(PSPTitleTextBox.Text))
        {
            var ErrorMessageBox = MessageBoxManager.GetMessageBoxStandard("Cannot create fPKG", "No game title specified, fPKG creation will be aborted.", ButtonEnum.Ok);
            await ErrorMessageBox.ShowWindowDialogAsync(window);
            return;
        }
        if (string.IsNullOrEmpty(PSPNPTitleTextBox.Text))
        {
            var ErrorMessageBox = MessageBoxManager.GetMessageBoxStandard("Cannot create fPKG", "No NP title specified, fPKG creation will be aborted.", ButtonEnum.Ok);
            await ErrorMessageBox.ShowWindowDialogAsync(window);
            return;
        }
        if (PSPNPTitleTextBox.Text.Length != 9)
        {
            var ErrorMessageBox = MessageBoxManager.GetMessageBoxStandard("Cannot create fPKG", "'NP Title' length mismatching, only 9 characters are allowed, fPKG creation will be aborted.", ButtonEnum.Ok);
            await ErrorMessageBox.ShowWindowDialogAsync(window);
            return;
        }

        // Select output folder
        var newOpenFolderDialog = new OpenFolderDialog() { Title = "Please select an output folder" };
        var selectedFolder = await newOpenFolderDialog.ShowAsync(window);
        string SelectedISOFile = SelectedPSPDiscTextBox.Text;

        // Continue if selection is not empty
        if (!string.IsNullOrWhiteSpace(selectedFolder))
        {
            string PKGOutputFolder = selectedFolder;
            string GameCacheDirectory = Path.Combine(WineCDrivePath, "Cache", "PSPfPKG");

            // Remove previous fPKG creation & re-create the PS1fPKG cache folder
            if (Directory.Exists(GameCacheDirectory))
            {
                Directory.Delete(GameCacheDirectory, true);
                Directory.CreateDirectory(GameCacheDirectory);
            }
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "PSPfPKG.gp4")))
            {
                File.Delete(Path.Combine(Environment.CurrentDirectory, "Cache", "PSPfPKG.gp4"));
            }

            // Copy the PSP emulator to the cache directory
            Utils.CopyFilesRecursively(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "emus", "psphd"), GameCacheDirectory);

            // Copy the selected icon and background to the cache directory
            if (!Directory.Exists(Path.Combine(GameCacheDirectory, "sce_sys")))
            {
                Directory.CreateDirectory(Path.Combine(GameCacheDirectory, "sce_sys"));
            }
            if (!string.IsNullOrEmpty(SelectedPSPIconTextBox.Text))
            {
                File.Copy(SelectedPSPIconTextBox.Text, Path.Combine(GameCacheDirectory, "sce_sys", "icon0.png"), true);
            }
            if (!string.IsNullOrEmpty(SelectedPSPBGImageTextBox.Text))
            {
                File.Copy(SelectedPSPBGImageTextBox.Text, Path.Combine(GameCacheDirectory, "sce_sys", "pic0.png"), true);
            }

            // Get PSP EBOOT
            if (!File.Exists(ExtractFileFromPSPISO(SelectedISOFile, @"\PSP_GAME\SYSDIR\EBOOT.BIN", Path.Combine(CacheDirectory, "temp_eboot.bin"))))
            {
                File.Copy(SelectedISOFile, Path.Combine(GameCacheDirectory, "data", "USER_L0.IMG"), true);
                var NoValidFileMessage = MessageBoxManager.GetMessageBoxStandard("PS fPKG Classics Builder", "Cannot read the EBOOT.BIN file from the ISO.\r\nWarning: This game may not work!", ButtonEnum.Ok);
                await NoValidFileMessage.ShowWindowDialogAsync(window);
            }
            else
            {
                string PSPDecryptCMD = "\"" + Path.Combine(Environment.CurrentDirectory, "Tools", "pspdecrypt") + "\"" + " \"" + Path.Combine(CacheDirectory, "temp_eboot.bin") + "\"";
                var EscapedArgs = PSPDecryptCMD.Replace("\"", "\\\"");
                var PSPDecryptProcess = new Process();
                PSPDecryptProcess.StartInfo.FileName = OperatingSystem.IsMacOS() ? "/bin/sh" : "/bin/bash";
                PSPDecryptProcess.StartInfo.Arguments = $"-c \"{EscapedArgs}\"";
                PSPDecryptProcess.StartInfo.CreateNoWindow = true;
                PSPDecryptProcess.Start();
                await PSPDecryptProcess.WaitForExitAsync();
                PSPDecryptProcess.Close();

                if (!Directory.Exists(Path.Combine(GameCacheDirectory, "data")))
                {
                    Directory.CreateDirectory(Path.Combine(GameCacheDirectory, "data"));
                }

                File.Copy(SelectedISOFile, Path.Combine(GameCacheDirectory, "data", "USER_L0.IMG"), true);

                var NewFileInfo = new FileInfo(Path.Combine(CacheDirectory, "temp_eboot.bin"));
                long FileLength = NewFileInfo.Length;
                if (FileLength > 512320L)
                {
                    FileLength = 512320L;
                }

                byte[] TempEBOOTByteArray = ReadUMDData(Path.Combine(CacheDirectory, "temp_eboot.bin"), 0, (int)FileLength);
                var OffsetValue = RuntimeHelpers.GetObjectValue(FindOffset(Path.Combine(GameCacheDirectory, "data", "USER_L0.IMG"), TempEBOOTByteArray));
                byte[] DecTempEBOOTByteArray = ReadUMDData(Path.Combine(CacheDirectory, "temp_eboot.bin.dec"), 0, (int)NewFileInfo.Length);

                WriteData(Path.Combine(GameCacheDirectory, "data", "USER_L0.IMG"), Convert.ToInt64(OffsetValue), BitConverter.ToString(DecTempEBOOTByteArray));
            }

            // Remove temp files
            if (File.Exists(Path.Combine(CacheDirectory, "temp_eboot.bin")))
            {
                File.Delete(Path.Combine(CacheDirectory, "temp_eboot.bin"));
            }
            if (File.Exists(Path.Combine(CacheDirectory, "temp_eboot.bin.dec")))
            {
                File.Delete(Path.Combine(CacheDirectory, "temp_eboot.bin.dec"));
            }

            // Copy the selected icon and background to the cache directory
            if (!Directory.Exists(Path.Combine(GameCacheDirectory, "sce_sys")))
            {
                Directory.CreateDirectory(Path.Combine(GameCacheDirectory, "sce_sys"));
            }
            if (!string.IsNullOrEmpty(SelectedPSPIconTextBox.Text))
            {
                var IconBytes = File.ReadAllBytes(SelectedPSPIconTextBox.Text);
                var ConvertedIcon = Utils.ConvertTo24bppPNG(IconBytes, 512, 512);
                ConvertedIcon.Save(Path.Combine(GameCacheDirectory, "sce_sys", "icon0.png"));

                //File.Copy(SelectedPSPIconTextBox.Text, GameCacheDirectory + @"/sce_sys/icon0.png", true);
            }
            if (!string.IsNullOrEmpty(SelectedPSPBGImageTextBox.Text))
            {
                var BGBytes = File.ReadAllBytes(SelectedPSPBGImageTextBox.Text);
                var ConvertedBG = Utils.ConvertTo24bppPNG(BGBytes, 1920, 1080);
                ConvertedBG.Save(Path.Combine(GameCacheDirectory, "sce_sys", "pic0.png"));

                //File.Copy(SelectedPSPBGImageTextBox.Text, GameCacheDirectory + @"/sce_sys/pic0.png", true);
            }

            // PSP Emulator configuration
            string EmulatorConfig = string.Concat("--ps4-trophies=0\r\n--ps5-uds=0\r\n--trophies=0\r\n--image=\"data/USER_L0.IMG\"\r\n--antialias=SSAA4x\r\n--multisaves=true\r\n--notrophies=true\r\n\r\n");

            if (File.Exists(Path.Combine(GameCacheDirectory, "config-title.txt")))
            {
                File.Delete(Path.Combine(GameCacheDirectory, "config-title.txt"));
            }
            File.WriteAllText(Path.Combine(GameCacheDirectory, "config-title.txt"), EmulatorConfig);
            if (!string.IsNullOrEmpty(SelectedPSPConfigTextBox.Text))
            {
                File.AppendAllText(Path.Combine(GameCacheDirectory, "config-title.txt"), File.ReadAllText(SelectedPSPConfigTextBox.Text));
            }

            // Create a new PARAM.SFO file
            var NewPS4ParamSFO = new ParamSfo();
            NewPS4ParamSFO.SetValue("APP_TYPE", SfoEntryType.Integer, "1", 4);
            NewPS4ParamSFO.SetValue("APP_VER", SfoEntryType.Utf8, "01.00", 8);
            NewPS4ParamSFO.SetValue("ATTRIBUTE", SfoEntryType.Integer, "0", 4);
            NewPS4ParamSFO.SetValue("CATEGORY", SfoEntryType.Utf8, "gd", 4);
            NewPS4ParamSFO.SetValue("CONTENT_ID", SfoEntryType.Utf8, "UP9000-" + PSPNPTitleTextBox.Text + "_00-" + PSPNPTitleTextBox.Text + "PSPFPKG", 48);
            NewPS4ParamSFO.SetValue("DOWNLOAD_DATA_SIZE", SfoEntryType.Integer, "0", 4);
            NewPS4ParamSFO.SetValue("FORMAT", SfoEntryType.Utf8, "obs", 4);
            NewPS4ParamSFO.SetValue("PARENTAL_LEVEL", SfoEntryType.Integer, "5", 4);
            NewPS4ParamSFO.SetValue("SYSTEM_VER", SfoEntryType.Integer, "0", 4);
            NewPS4ParamSFO.SetValue("TITLE", SfoEntryType.Utf8, PSPTitleTextBox.Text, 128);
            NewPS4ParamSFO.SetValue("TITLE_ID", SfoEntryType.Utf8, PSPNPTitleTextBox.Text, 12);
            NewPS4ParamSFO.SetValue("VERSION", SfoEntryType.Utf8, "01.00", 8);

            File.WriteAllBytes(Path.Combine(GameCacheDirectory, "sce_sys", "param.sfo"), NewPS4ParamSFO.Serialize());

            // Generate a GP4 project
            string GenGP4CCMD = "wine \"c:\\PS4\\gengp4_patch.exe\" \"c:\\Cache\\PSPfPKG\"";
            var EscapedArgs2 = GenGP4CCMD.Replace("\"", "\\\"");
            var NewProcess = new Process();
            NewProcess.StartInfo.FileName = OperatingSystem.IsMacOS() ? "/bin/sh" : "/bin/bash";
            NewProcess.StartInfo.Arguments = $"-c \"{EscapedArgs2}\"";
            NewProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            NewProcess.StartInfo.CreateNoWindow = true;
            NewProcess.Start();
            NewProcess.WaitForExit();
            NewProcess.Close();

            // Modify the GP4 project
            File.WriteAllText(Path.Combine(CacheDirectory, "PSPfPKG.gp4"), File.ReadAllText(Path.Combine(CacheDirectory, "PSPfPKG.gp4").Replace("<?xml version=\"1.1\"", "<?xml version=\"1.0\"")));
            File.WriteAllText(Path.Combine(CacheDirectory, "PSPfPKG.gp4"), File.ReadAllText(Path.Combine(CacheDirectory, "PSPfPKG.gp4").Replace("<scenarios default_id=\"1\">", "<scenarios default_id=\"0\">")));

            var DebugMessageBox = MessageBoxManager.GetMessageBoxStandard("PS fPKG Classics Builder", "All files ready! \r\n fPKG can be build.", ButtonEnum.Ok);
            await DebugMessageBox.ShowWindowDialogAsync(window);

            // Create the fPKG
            string PUBCMD = "wine \"c:\\PS4\\orbis-pub-cmd-3.38.exe\" img_create --oformat pkg --skip_digest --no_progress_bar \"c:\\Cache\\PSPfPKG.gp4\" \"c:\\fPKG\"";
            EscapedArgs2 = PUBCMD.Replace("\"", "\\\"");

            var PKGBuilderProcess = new Process();
            PKGBuilderProcess.StartInfo.FileName = OperatingSystem.IsMacOS() ? "/bin/sh" : "/bin/bash";
            PKGBuilderProcess.StartInfo.Arguments = $"-c \"{EscapedArgs2}\"";
            PKGBuilderProcess.StartInfo.UseShellExecute = false;
            PKGBuilderProcess.StartInfo.RedirectStandardOutput = true;
            PKGBuilderProcess.StartInfo.CreateNoWindow = true;
            PKGBuilderProcess.Start();

            var NewStreamReader = PKGBuilderProcess.StandardOutput;
            string PKGBuilderProcessOutput = NewStreamReader.ReadToEnd();

            await PKGBuilderProcess.WaitForExitAsync();
            PKGBuilderProcess.Close();

            if (PKGBuilderProcessOutput.Contains("Create image Process finished with warning"))
            {
                string PKGFileName = "UP9000-" + PSPNPTitleTextBox.Text + "_00-" + PSPNPTitleTextBox.Text + "PSPFPKG-A0100-V0100.pkg";
                string PKGFilePath = Path.Combine(WineCDrivePath, "fPKG", PKGFileName);
                if (File.Exists(PKGFilePath))
                {
                    try
                    {
                        File.Move(PKGFilePath, Path.Combine(PKGOutputFolder, PKGFileName));
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error);
                    }
                }

                var PKGBuildMessageBox = MessageBoxManager.GetMessageBoxStandard("DEBUG", "fPKG created with success!", ButtonEnum.Ok);
                await PKGBuildMessageBox.ShowWindowDialogAsync(window);
            }
            else
            {
                DebugMessageBox = MessageBoxManager.GetMessageBoxStandard("DEBUG", "Error creating fPKG", ButtonEnum.Ok);
                await DebugMessageBox.ShowWindowDialogAsync(window);
            }
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

    public static string ExtractFileFromPSPISO(string path, string fileName, string DestinationPath)
    {
        string OutputDestination = "";
        string DesinationDirectoryName = Path.GetDirectoryName(DestinationPath) ?? "";
        if (!string.IsNullOrEmpty(DestinationPath) && !Directory.Exists(DesinationDirectoryName))
        {
            Directory.CreateDirectory(DesinationDirectoryName);
        }
        try
        {
            using var NewFileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var NewIso9660CDReader = new DiscUtils.Iso9660.CDReader(NewFileStream, true);
            try
            {
                var NewSparseStream = NewIso9660CDReader.OpenFile(fileName, FileMode.Open);
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

    public static byte[] ReadUMDData(string DataFile, long Offset, int Lenght)
    {
        var NewByte = new byte[(Lenght - 1 + 1)];
        using (var NewBinaryReader = new BinaryReader(File.Open(DataFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
        {
            long BaseStreamLenght = NewBinaryReader.BaseStream.Length;
            int Num = 0;
            NewBinaryReader.BaseStream.Seek(Offset, SeekOrigin.Begin);
            while (Offset < BaseStreamLenght & Num < Lenght)
            {
                NewByte[Num] = NewBinaryReader.ReadByte();
                Offset += 1L;
                Num += 1;
            }
        }
        return NewByte;
    }

    public static object FindOffset(string FileName, byte[] Query)
    {
        object ReturnLenght;
        using (var NewBinaryReader = new BinaryReader(File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
        {
            double BaseStreamLength = NewBinaryReader.BaseStream.Length;
            if (Query.Length <= BaseStreamLength)
            {

                byte[] NewByteArray = NewBinaryReader.ReadBytes(Query.Length);
                bool Flag = false;
                int NewQueryLenght = Query.Length - 1;
                int WhileInt = 0;

                while (WhileInt <= NewQueryLenght)
                {
                    if (NewByteArray[WhileInt] == Query[WhileInt])
                    {
                        Flag = true;
                        WhileInt += 1;
                    }
                    else
                    {
                        Flag = false;
                        break;
                    }
                }

                if (!Flag)
                {
                    double NewBaseStreamLength = BaseStreamLength - 1d;
                    double QueryLenght = Query.Length;
                    while (QueryLenght <= NewBaseStreamLength)
                    {
                        Array.Copy(NewByteArray, 1, NewByteArray, 0, NewByteArray.Length - 1);
                        NewByteArray[^1] = NewBinaryReader.ReadByte();
                        int length3 = Query.Length - 1;
                        int num3 = 0;
                        while (num3 <= length3)
                        {
                            if (NewByteArray[num3] == Query[num3])
                            {
                                Flag = true;
                                num3 += 1;
                            }
                            else
                            {
                                Flag = false;
                                break;
                            }
                        }
                        if (!Flag)
                        {
                            QueryLenght += 1d;
                        }
                        else
                        {
                            ReturnLenght = QueryLenght - (Query.Length - 1);
                            return ReturnLenght;
                        }
                    }
                }
                else
                {
                    ReturnLenght = 0;
                    return ReturnLenght;
                }
            }
        }
        ReturnLenght = -1;
        return ReturnLenght;
    }

    public static void WriteData(string FileToWrite, long Offset, string DataToWrite)
    {
        var NewFileStream = new FileStream(FileToWrite, FileMode.Open, FileAccess.Write, FileShare.Write);
        string[] NewStringArray = DataToWrite.Split(['-']);
        NewFileStream.Seek(Offset, SeekOrigin.Begin);
        int DoInt = 0;
        do
        {
            string str = NewStringArray[DoInt];
            NewFileStream.WriteByte(Convert.ToByte(Convert.ToInt32(str, 16)));
            DoInt += 1;
        }
        while (DoInt < NewStringArray.Length);
        NewFileStream.Close();
    }

    #endregion

    #region RegexStuff

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex CRCRegex();

    [GeneratedRegex(@"\((.*?)\)")]
    private static partial Regex PSPTitleRegex1();

    [GeneratedRegex(" {2,}")]
    private static partial Regex PSPTitleRegex2();

    #endregion

}