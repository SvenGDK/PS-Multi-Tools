using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PSMultiTools.MemoryCard;

public partial class PS2MCManager : Window
{

    public PS2MCManager()
    {
        InitializeComponent();

        MemoryCardContentListBox.PointerPressed += MemoryCardContentListBox_PointerPressed;
    }

    private bool PS2MCCardConnectionSuccess = false;

    private async void OpenPathButton_Click(object? sender, RoutedEventArgs e)
    {
        if (PS2MCCardConnectionSuccess == true && !string.IsNullOrEmpty(CurrentMCPathTextBox.Text))
        {
            LoadPS2MCDirectory(CurrentMCPathTextBox.Text);
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No PS2 Memory Card loaded.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    public struct PS2MCContentListViewItem
    {
        public string FileName { get; set; }

        public string FileType { get; set; }

        public string LastModification { get; set; }
    }

    private async void LoadPS2MC()
    {

        using (var PS2MCReader = new Process())
        {
            // Read MC information
            PS2MCReader.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool");
            PS2MCReader.StartInfo.Arguments = "-i";
            PS2MCReader.StartInfo.RedirectStandardOutput = true;
            PS2MCReader.StartInfo.UseShellExecute = false;
            PS2MCReader.StartInfo.CreateNoWindow = true;
            PS2MCReader.Start();
            PS2MCReader.WaitForExit();

            var OutputReader = PS2MCReader.StandardOutput;
            string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

            if (ProcessOutput.Length > 0)
            {
                if (ProcessOutput[1] == "PS2 Memory Card Informations")
                {

                    // Get values
                    PageSizeTextBlock.Text = ProcessOutput[2].Split(':')[1];
                    BlockSizeTextBlock.Text = ProcessOutput[3].Split(':')[1];
                    MCSizeTextBlock.Text = ProcessOutput[4].Split(':')[1];

                    if (ProcessOutput[5] == "MC claims to support ECC")
                    {
                        ECCSupportTextBlock.Text = "Yes";
                    }
                    else
                    {
                        ECCSupportTextBlock.Text = "No";
                    }

                    if (ProcessOutput[6] == "MC claims to support bad blocks management")
                    {
                        BBManagementTextBlock.Text = "Yes";
                    }
                    else
                    {
                        ECCSupportTextBlock.Text = "No";
                    }

                    EraseByteTextBlock.Text = ProcessOutput[7].Split(':')[1];

                    PS2MCCardConnectionSuccess = true;
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not read the PS2 Memory Card." + Environment.NewLine + "Please make sure that the MC Adaptor driver is installed and the Memory Card is plugged in correctly.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }

        if (PS2MCCardConnectionSuccess)
        {
            // Get the free space on the MC
            using (var PS2MCReader = new Process())
            {
                PS2MCReader.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool");
                PS2MCReader.StartInfo.Arguments = "-f";
                PS2MCReader.StartInfo.RedirectStandardOutput = true;
                PS2MCReader.StartInfo.UseShellExecute = false;
                PS2MCReader.StartInfo.CreateNoWindow = true;
                PS2MCReader.Start();

                var OutputReader = PS2MCReader.StandardOutput;
                string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                if (ProcessOutput.Length > 0)
                {
                    if (ProcessOutput[1] == "PS2 Memory Card free space")
                    {
                        // Get values
                        MCFreeSpaceTextBlock.Text = ProcessOutput[3].Split(':')[1];
                    }
                }
            }

            // Load the root directory of the MC
            LoadPS2MCDirectory("/");
        }
    }

    private void LoadPS2MCDirectory(string SelectedPath)
    {
        MemoryCardContentListBox.Items.Clear();

        if (!string.IsNullOrEmpty(SelectedPath))
        {
            using var PS2MCReader = new Process();
            PS2MCReader.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool");
            PS2MCReader.StartInfo.Arguments = "-ls " + SelectedPath;
            PS2MCReader.StartInfo.RedirectStandardOutput = true;
            PS2MCReader.StartInfo.UseShellExecute = false;
            PS2MCReader.StartInfo.CreateNoWindow = true;
            PS2MCReader.Start();
            PS2MCReader.WaitForExit();

            var OutputReader = PS2MCReader.StandardOutput;
            string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

            if (ProcessOutput.Length > 0)
            {
                if (ProcessOutput[1].Contains("Filename"))
                {

                    // Get MC content
                    foreach (var ProcessOutputLine in ProcessOutput.Skip(2))
                    {
                        if (!string.IsNullOrEmpty(ProcessOutputLine))
                        {
                            // Split the content line
                            string[] SplittedValues = ProcessOutputLine.Split('|');
                            if (SplittedValues.Length > 1)
                            {
                                var NewMCContent = new PS2MCContentListViewItem() { FileName = SplittedValues[0].Trim(), FileType = SplittedValues[1].Trim(), LastModification = SplittedValues[2].Trim() };
                                MemoryCardContentListBox.Items.Add(NewMCContent);
                            }
                        }
                    }

                }
            }
        }

    }

    private void ReloadMCButton_Click(object? sender, RoutedEventArgs e)
    {
        LoadPS2MC();
    }

    private async void InjectFileButton_Click(object? sender, RoutedEventArgs e)
    {
        if (PS2MCCardConnectionSuccess == true && !string.IsNullOrEmpty(CurrentMCPathTextBox.Text))
        {

            var OFD = new OpenFileDialog() { Title = "Select a file to inject into the PS2 Memory Card", AllowMultiple = false };
            var OFDResult = await OFD.ShowAsync(this);
            if (OFDResult != null && OFDResult.Length > 0)
            {

                string SelectedFileToInject = OFDResult[0];
                string SelectedFileNameToInject = Path.GetFileName(OFDResult[0]);

                using var PS2MCTool = new Process();
                PS2MCTool.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool");

                // Set -in command
                if (CurrentMCPathTextBox.Text == "/")
                {
                    string DestinationPath = "/" + SelectedFileNameToInject;
                    PS2MCTool.StartInfo.Arguments = "-in " + "\"" + SelectedFileToInject + "\" " + DestinationPath;
                }
                else
                {
                    string DestinationPath = CurrentMCPathTextBox.Text + "/" + SelectedFileNameToInject;
                    PS2MCTool.StartInfo.Arguments = "-in " + "\"" + SelectedFileToInject + "\" " + DestinationPath;
                }

                PS2MCTool.StartInfo.RedirectStandardOutput = true;
                PS2MCTool.StartInfo.UseShellExecute = false;
                PS2MCTool.StartInfo.CreateNoWindow = true;
                PS2MCTool.Start();
                PS2MCTool.WaitForExit();

                var OutputReader = PS2MCTool.StandardOutput;
                string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                if (ProcessOutput.Length > 1)
                {
                    if (ProcessOutput[1].Contains("Reading file:"))
                    {

                        if (ProcessOutput.Length > 2)
                        {
                            if (!ProcessOutput[2].Contains("Error:"))
                            {
                                var box = MessageBoxManager.GetMessageBoxStandard("File written!", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                                await box.ShowWindowAsync();
                                // Reload data
                                LoadPS2MCDirectory(CurrentMCPathTextBox.Text);
                            }
                            else
                            {
                                var box = MessageBoxManager.GetMessageBoxStandard("Error writing data", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                await box.ShowWindowAsync();
                            }
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("File written!", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowAsync();
                            // Reload data
                            LoadPS2MCDirectory(CurrentMCPathTextBox.Text);
                        }
                    }

                    else
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error writing data", "Could not find any PS2 Memory Card to write on.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }
                }

            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No PS2 Memory Card loaded.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void ExtractFileButton_Click(object? sender, RoutedEventArgs e)
    {
        if (PS2MCCardConnectionSuccess == true && !string.IsNullOrEmpty(CurrentMCPathTextBox.Text))
        {

            if (MemoryCardContentListBox.SelectedItem is not null)
            {

                PS2MCContentListViewItem SelectedMCContent = (PS2MCContentListViewItem)MemoryCardContentListBox.SelectedItem;
                if (!(SelectedMCContent.FileName == ".") && !(SelectedMCContent.FileName == "..") && !(SelectedMCContent.FileType == "<dir>"))
                {

                    var FBD = new OpenFolderDialog() { Title = "Select an output folder" };
                    var FBDResult = await FBD.ShowAsync(this);

                    if (FBDResult != null)
                    {

                        using var PS2MCTool = new Process();
                        PS2MCTool.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool");

                        // Set -x command
                        if (CurrentMCPathTextBox.Text == "/")
                        {
                            string MCFilePath = "/" + SelectedMCContent.FileName;
                            PS2MCTool.StartInfo.Arguments = "-x " + MCFilePath + " \"" + FBDResult + @"\" + SelectedMCContent.FileName + "\"";
                        }
                        else
                        {
                            string MCFilePath = CurrentMCPathTextBox.Text + "/" + SelectedMCContent.FileName;
                            PS2MCTool.StartInfo.Arguments = "-x " + MCFilePath + " \"" + FBDResult + @"\" + SelectedMCContent.FileName + "\"";
                        }

                        PS2MCTool.StartInfo.RedirectStandardOutput = true;
                        PS2MCTool.StartInfo.UseShellExecute = false;
                        PS2MCTool.StartInfo.CreateNoWindow = true;
                        PS2MCTool.Start();
                        PS2MCTool.WaitForExit();

                        var OutputReader = PS2MCTool.StandardOutput;
                        string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                        if (ProcessOutput.Length > 1)
                        {
                            if (ProcessOutput[1].Contains("Reading file:"))
                            {

                                if (ProcessOutput.Length > 2)
                                {
                                    if (!ProcessOutput[2].Contains("Error:"))
                                    {
                                        var box = MessageBoxManager.GetMessageBoxStandard("File written!", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                                        await box.ShowWindowAsync();
                                        // Reload data
                                        LoadPS2MCDirectory(CurrentMCPathTextBox.Text);
                                    }
                                    else
                                    {
                                        var box = MessageBoxManager.GetMessageBoxStandard("Error writing data", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                        await box.ShowWindowAsync();
                                    }
                                }
                                else
                                {
                                    var box = MessageBoxManager.GetMessageBoxStandard("File written!", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                                    await box.ShowWindowAsync();
                                    // Reload data
                                    LoadPS2MCDirectory(CurrentMCPathTextBox.Text);
                                }
                            }

                            else
                            {
                                var box = MessageBoxManager.GetMessageBoxStandard("Error writing data", "Could not find any PS2 Memory Card to write on.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                await box.ShowWindowAsync();
                            }
                        }

                    }
                }

                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Only a file can be extracted.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }

            }
        }

        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No PS2 Memory Card loaded.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void DeleteFileButton_Click(object? sender, RoutedEventArgs e)
    {
        if (PS2MCCardConnectionSuccess == true && !string.IsNullOrEmpty(CurrentMCPathTextBox.Text))
        {

            if (MemoryCardContentListBox.SelectedItem is not null)
            {

                PS2MCContentListViewItem SelectedMCContent = (PS2MCContentListViewItem)MemoryCardContentListBox.SelectedItem;
                if (!(SelectedMCContent.FileName == ".") && !(SelectedMCContent.FileName == "..") && !(SelectedMCContent.FileType == "<dir>"))
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Please confirm to delete the file: " + SelectedMCContent.FileName, ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {

                        using var PS2MCTool = new Process();
                        PS2MCTool.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool");

                        // Set -rm command
                        if (CurrentMCPathTextBox.Text == "/")
                        {
                            string MCFilePath = "/" + SelectedMCContent.FileName;
                            PS2MCTool.StartInfo.Arguments = "-rm " + MCFilePath;
                        }
                        else
                        {
                            string MCFilePath = CurrentMCPathTextBox.Text + "/" + SelectedMCContent.FileName;
                            PS2MCTool.StartInfo.Arguments = "-rm " + MCFilePath;
                        }

                        PS2MCTool.StartInfo.RedirectStandardOutput = true;
                        PS2MCTool.StartInfo.UseShellExecute = false;
                        PS2MCTool.StartInfo.CreateNoWindow = true;
                        PS2MCTool.Start();
                        PS2MCTool.WaitForExit();

                        var OutputReader = PS2MCTool.StandardOutput;
                        string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                        if (ProcessOutput.Length > 1)
                        {
                            if (ProcessOutput[1].Contains("Removing file:"))
                            {

                                if (ProcessOutput.Length > 2)
                                {
                                    if (!ProcessOutput[2].Contains("Error:"))
                                    {
                                        var box2 = MessageBoxManager.GetMessageBoxStandard("File removed!", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                                        await box2.ShowWindowAsync();
                                        // Reload data
                                        LoadPS2MCDirectory(CurrentMCPathTextBox.Text);
                                    }
                                    else
                                    {
                                        var box2 = MessageBoxManager.GetMessageBoxStandard("Error removing data", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                        await box2.ShowWindowAsync();
                                    }
                                }
                                else
                                {
                                    var box2 = MessageBoxManager.GetMessageBoxStandard("File removed!", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                                    await box2.ShowWindowAsync();
                                    // Reload data
                                    LoadPS2MCDirectory(CurrentMCPathTextBox.Text);
                                }
                            }

                            else
                            {
                                var box2 = MessageBoxManager.GetMessageBoxStandard("Error writing data", "Could not find any PS2 Memory Card to write on.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                await box2.ShowWindowAsync();
                            }
                        }
                    }

                    else
                    {
                        var box2 = MessageBoxManager.GetMessageBoxStandard("Aborted", "Aborted.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box2.ShowWindowAsync();
                    }
                }

                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Only a file can be deleted using this button.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }

            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No PS2 Memory Card loaded.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void FormatMCButton_Click(object? sender, RoutedEventArgs e)
    {
        if (PS2MCCardConnectionSuccess == true && !string.IsNullOrEmpty(CurrentMCPathTextBox.Text))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Please confirm to format the PS2 Memory Card", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);
            if (boxresult == ButtonResult.Yes)
            {

                using var PS2MCTool = new Process();
                PS2MCTool.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool");
                PS2MCTool.StartInfo.Arguments = "--mc-format";
                PS2MCTool.StartInfo.RedirectStandardOutput = true;
                PS2MCTool.StartInfo.UseShellExecute = false;
                PS2MCTool.StartInfo.CreateNoWindow = true;
                PS2MCTool.Start();
                PS2MCTool.WaitForExit();

                var OutputReader = PS2MCTool.StandardOutput;
                string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                if (ProcessOutput.Length > 1)
                {
                    if (ProcessOutput[1].Contains("PS2 Memory Card format"))
                    {

                        if (ProcessOutput.Length > 3)
                        {

                            if (ProcessOutput[3].Contains("Memory card succesfully formated."))
                            {
                                var box2 = MessageBoxManager.GetMessageBoxStandard("Memory Card formatted!", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                                await box2.ShowWindowAsync();

                                // Reload MC
                                CurrentMCPathTextBox.Text = "/";
                                LoadPS2MCDirectory("/");
                            }
                            else
                            {
                                var box2 = MessageBoxManager.GetMessageBoxStandard("Error formatting PS2 Memory Card", ProcessOutput[3], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                await box2.ShowWindowAsync();
                            }
                        }
                        else
                        {
                            var box2 = MessageBoxManager.GetMessageBoxStandard("Memory Card formatted!", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box2.ShowWindowAsync();

                            // Reload MC
                            CurrentMCPathTextBox.Text = "/";
                            LoadPS2MCDirectory("/");
                        }
                    }
                    else
                    {
                        var box2 = MessageBoxManager.GetMessageBoxStandard("Error formatting PS2 Memory Card", "Could not find any PS2 Memory Card to format.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box2.ShowWindowAsync();
                    }
                }
            }
            else
            {
                var box2 = MessageBoxManager.GetMessageBoxStandard("Aborted", "Aborted.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box2.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No PS2 Memory Card loaded.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void CreateDirectoryButton_Click(object? sender, RoutedEventArgs e)
    {
        if (PS2MCCardConnectionSuccess == true && !string.IsNullOrEmpty(CurrentMCPathTextBox.Text))
        {
            var NewInputDialog = new InputDialog() { Title = "Create a new directory" };
            NewInputDialog.InputDialogTitleTextBlock.Text = "Please enter a directory name:";
            NewInputDialog.NewValueTextBox.Text = "";

            var NewDirectoryName = await NewInputDialog.ShowDialog<string>(this);
            if (!string.IsNullOrEmpty(NewDirectoryName))
            {
                if (!string.IsNullOrEmpty(CurrentMCPathTextBox.Text))
                {
                    using var PS2MCTool = new Process();
                    PS2MCTool.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool");

                    // Set mkdir command
                    if (CurrentMCPathTextBox.Text == "/")
                    {
                        PS2MCTool.StartInfo.Arguments = "-mkdir /" + NewDirectoryName;
                    }
                    else
                    {
                        PS2MCTool.StartInfo.Arguments = "-mkdir " + CurrentMCPathTextBox.Text + "/" + NewDirectoryName;
                    }

                    PS2MCTool.StartInfo.RedirectStandardOutput = true;
                    PS2MCTool.StartInfo.UseShellExecute = false;
                    PS2MCTool.StartInfo.CreateNoWindow = true;
                    PS2MCTool.Start();
                    PS2MCTool.WaitForExit();

                    var OutputReader = PS2MCTool.StandardOutput;
                    string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                    if (ProcessOutput.Length > 1)
                    {
                        if (ProcessOutput[1].Contains("Creating directory:"))
                        {

                            if (ProcessOutput.Length > 2)
                            {

                                if (!ProcessOutput[2].Contains("Error:"))
                                {
                                    var box = MessageBoxManager.GetMessageBoxStandard("Directory created!", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                                    await box.ShowWindowAsync();
                                    // Reload data
                                    LoadPS2MCDirectory(CurrentMCPathTextBox.Text);
                                }
                                else
                                {
                                    var box = MessageBoxManager.GetMessageBoxStandard("Error writing data", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                    await box.ShowWindowAsync();
                                }
                            }
                            else
                            {
                                var box = MessageBoxManager.GetMessageBoxStandard("Directory created!", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                                await box.ShowWindowAsync();
                                // Reload data
                                LoadPS2MCDirectory(CurrentMCPathTextBox.Text);
                            }
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error writing data", "Could not find any PS2 Memory Card to write on.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "No PS2 Memory Card loaded.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No new directory name specified.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }

        }
    }

    private async void DeleteSelectedDirectoryButton_Click(object? sender, RoutedEventArgs e)
    {
        if (PS2MCCardConnectionSuccess == true && !string.IsNullOrEmpty(CurrentMCPathTextBox.Text))
        {
            if (MemoryCardContentListBox.SelectedItem is not null)
            {

                PS2MCContentListViewItem SelectedMCContent = (PS2MCContentListViewItem)MemoryCardContentListBox.SelectedItem;
                if (!(SelectedMCContent.FileName == ".") && !(SelectedMCContent.FileName == "..") && !(SelectedMCContent.FileType == "<file>"))
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Please confirm to delete the directory: " + SelectedMCContent.FileName, ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {

                        using var PS2MCTool = new Process();
                        PS2MCTool.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool");

                        // Set -rmdir command (SelectedMCContent.FileName = directory name)
                        if (CurrentMCPathTextBox.Text == "/")
                        {
                            string MCFilePath = "/" + SelectedMCContent.FileName;
                            PS2MCTool.StartInfo.Arguments = "-rmdir " + MCFilePath;
                        }
                        else
                        {
                            string MCFilePath = CurrentMCPathTextBox.Text + "/" + SelectedMCContent.FileName;
                            PS2MCTool.StartInfo.Arguments = "-rmdir " + MCFilePath;
                        }

                        PS2MCTool.StartInfo.RedirectStandardOutput = true;
                        PS2MCTool.StartInfo.UseShellExecute = false;
                        PS2MCTool.StartInfo.CreateNoWindow = true;
                        PS2MCTool.Start();
                        PS2MCTool.WaitForExit();

                        var OutputReader = PS2MCTool.StandardOutput;
                        string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                        if (ProcessOutput.Length > 1)
                        {
                            if (ProcessOutput[1].Contains("Removing directory:"))
                            {

                                if (ProcessOutput.Length > 2)
                                {
                                    if (!ProcessOutput[2].Contains("Error:"))
                                    {
                                        var box2 = MessageBoxManager.GetMessageBoxStandard("Directory removed!", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                                        await box2.ShowWindowAsync();
                                        // Reload data
                                        LoadPS2MCDirectory(CurrentMCPathTextBox.Text);
                                    }
                                    else
                                    {
                                        var box2 = MessageBoxManager.GetMessageBoxStandard("Error removing data", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                        await box2.ShowWindowAsync();
                                    }
                                }
                                else
                                {
                                    var box2 = MessageBoxManager.GetMessageBoxStandard("Directory removed!", ProcessOutput[2], ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                                    await box2.ShowWindowAsync();
                                    // Reload data
                                    LoadPS2MCDirectory(CurrentMCPathTextBox.Text);
                                }
                            }
                            else
                            {
                                var box2 = MessageBoxManager.GetMessageBoxStandard("Error removing data", "Could not find any PS2 Memory Card to write on.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                await box2.ShowWindowAsync();
                            }
                        }
                    }
                    else
                    {
                        var box2 = MessageBoxManager.GetMessageBoxStandard("Aborted", "Aborted.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box2.ShowWindowAsync();
                    }
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Only a directory can be deleted using this button.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No PS2 Memory Card loaded.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void InstallFMCBButton_Click(object? sender, RoutedEventArgs e)
    {
        if (PS2MCCardConnectionSuccess == true && !string.IsNullOrEmpty(CurrentMCPathTextBox.Text))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "This will install the 'Free MC Boot v1.94 Multi Region/Model Full Installation' package on your PS2 Memory Card that can be updated afterwards." + Environment.NewLine + "The installation process will take up to 4-5 minutes." + Environment.NewLine + "Do you want to continue ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);
            if (boxresult == ButtonResult.Yes)
            {

                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() => Cursor = new Cursor(StandardCursorType.Wait));
                }
                else
                {
                    Cursor = new Cursor(StandardCursorType.Wait);
                }

                var ExecutionValues = new List<bool>();
                string FMCBInstallPath = Path.Combine(Environment.CurrentDirectory, "Tools", "PS2", "FMCB");
                string FMCBSystemInstallPath = Path.Combine(Environment.CurrentDirectory, "Tools", "PS2", "FMCB", "SYSTEM");
                string FMCBBootInstallPath = Path.Combine(Environment.CurrentDirectory, "Tools", "PS2", "FMCB", "BOOT");

                // Sign KELFs
                ExecutionValues.Add(ExecutePS3MCACommand($"-k \"{Path.Combine(FMCBSystemInstallPath, "FMCB.XLF")}\" \"{Path.Combine(FMCBSystemInstallPath, "osdmain.elf")}\""));
                ExecutionValues.Add(ExecutePS3MCACommand($"-k \"{Path.Combine(FMCBSystemInstallPath, "OSD110.XLF")}\" \"{Path.Combine(FMCBSystemInstallPath, "osd110.elf")}\""));
                ExecutionValues.Add(ExecutePS3MCACommand($"-k \"{Path.Combine(FMCBSystemInstallPath, "OSDSYS.XLF")}\" \"{Path.Combine(FMCBSystemInstallPath, "osdsys.elf")}\""));

                // Create required directories on the PS2 Memory Card
                ExecutionValues.Add(ExecutePS3MCACommand("-mkdir /APPS"));
                ExecutionValues.Add(ExecutePS3MCACommand("-mkdir /BOOT"));
                ExecutionValues.Add(ExecutePS3MCACommand("-mkdir /BAEXEC-SYSTEM"));
                ExecutionValues.Add(ExecutePS3MCACommand("-mkdir /BCEXEC-SYSTEM"));
                ExecutionValues.Add(ExecutePS3MCACommand("-mkdir /BEEXEC-SYSTEM"));
                ExecutionValues.Add(ExecutePS3MCACommand("-mkdir /BIEXEC-SYSTEM"));
                ExecutionValues.Add(ExecutePS3MCACommand("-mkdir /SYS-CONF"));

                // Copy signed KELFs to the PS2 Memory Card
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBSystemInstallPath, "osdmain.elf")}\" /BIEXEC-SYSTEM/osdmain.elf"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBSystemInstallPath, "osd110.elf")}\" /BIEXEC-SYSTEM/osd110.elf"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBSystemInstallPath, "osdsys.elf")}\" /BIEXEC-SYSTEM/osdsys.elf"));

                // Write required files on the PS2 Memory Card
                // SYS-CONF
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBInstallPath, "SYS-CONF", "REEMCB.CNF")}\" /SYS-CONF/FREEMCB.CNF"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBInstallPath, "SYS-CONF", "FMCB_CFG.ELF")}\" /SYS-CONF/FMCB_CFG.ELF"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBInstallPath, "SYS-CONF", "USBD.IRX")}\" /SYS-CONF/USBD.IRX"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBInstallPath, "SYS-CONF", "USBHDFSD.IRX")}\" /SYS-CONF/USBHDFSD.IRX"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBInstallPath, "SYS-CONF", "icon.sys")}\" /SYS-CONF/icon.sys"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBInstallPath, "SYS-CONF", "sysconf.icn")}\" /SYS-CONF/sysconf.icn"));
                // SYSTEM
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBSystemInstallPath, "ATAD.IRX")}\" /SYSTEM/ATAD.IRX"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBSystemInstallPath, "HDDLOAD.IRX")}\" /SYSTEM/HDDLOAD.IRX"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBSystemInstallPath, "icon.sys")}\" /SYSTEM/icon.sys"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBSystemInstallPath, "FMCB.icn")}\" /SYSTEM/FMCB.icn"));
                // APPS
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBInstallPath, "APPS", "icon.sys")}\" /APPS/icon.sys"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBInstallPath, "APPS", "FMCBapps.icn")}\" /APPS/FMCBapps.icn"));
                // BOOT
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBBootInstallPath, "icon.sys")}\" /BOOT/icon.sys"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBBootInstallPath, "BOOT.icn")}\" /BOOT/BOOT.icn"));

                // Write homebrew applications on the PS2 Memory Card
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBBootInstallPath, "BOOT.ELF")}\" /BOOT/BOOT.ELF"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBBootInstallPath, "ESR.ELF")}\" /BOOT/ESR.ELF"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBBootInstallPath, "ESRGUI.ELF")}\" /BOOT/ESRGUI.ELF"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBBootInstallPath, "OPL.ELF")}\" /BOOT/OPL.ELF"));
                ExecutionValues.Add(ExecutePS3MCACommand($"-in \"{Path.Combine(FMCBBootInstallPath, "SMS.ELF")}\" /BOOT/SMS.ELF"));

                // Cross-link files for multi region/model installation
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/osdmain.elf /BAEXEC-SYSTEM/osd120.elf"));
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/osdmain.elf /BAEXEC-SYSTEM/osdmain.elf"));
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/osdmain.elf /BCEXEC-SYSTEM/osdmain.elf"));
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/osdmain.elf /BEEXEC-SYSTEM/osdmain.elf"));
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/osdmain.elf /BEEXEC-SYSTEM/osd130.elf"));
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/osdmain.elf /BIEXEC-SYSTEM/osd130.elf"));
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/osdmain.elf /BAEXEC-SYSTEM/osd130.elf"));
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/icon.sys /BAEXEC-SYSTEM/icon.sys"));
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/icon.sys /BCEXEC-SYSTEM/icon.sys"));
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/icon.sys /BEEXEC-SYSTEM/icon.sys"));
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/FMCB.icn /BAEXEC-SYSTEM/FMCB.icn"));
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/FMCB.icn /BCEXEC-SYSTEM/FMCB.icn"));
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/FMCB.icn /BEEXEC-SYSTEM/FMCB.icn"));

                // Delete temporary files
                if (File.Exists(Path.Combine(FMCBInstallPath, "SYSTEM", "osdmain.elf")))
                {
                    File.Delete(Path.Combine(FMCBInstallPath, "SYSTEM", "osdmain.elf"));
                }
                if (File.Exists(Path.Combine(FMCBInstallPath, "SYSTEM", "osd110.elf")))
                {
                    File.Delete(Path.Combine(FMCBInstallPath, "SYSTEM", "osd110.elf"));
                }
                if (File.Exists(Path.Combine(FMCBInstallPath, "SYSTEM", "osdsys.elf")))
                {
                    File.Delete(Path.Combine(FMCBInstallPath, "SYSTEM", "osdsys.elf"));
                }

                // Check if an executed command returned an error (only 1 error will make FMCB unstable -> critical error)
                bool ErrorOccuredDuringExecution = false;
                foreach (var ReturnedValue in ExecutionValues)
                {
                    if (ReturnedValue == false)
                    {
                        ErrorOccuredDuringExecution = true;
                        break;
                    }
                }

                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() => Cursor = new Cursor(StandardCursorType.Arrow));
                }
                else
                {
                    Cursor = new Cursor(StandardCursorType.Arrow);
                }

                if (!ErrorOccuredDuringExecution)
                {
                    var box2 = MessageBoxManager.GetMessageBoxStandard("Files written!", "FMCB Installation completed with success!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box2.ShowWindowAsync();
                }
                else
                {
                    var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "An error occured while installing FMCB on the PS2 Memory Card. Please format your PS2 Memory Card and retry OR try another 8MB PS2 Memory Card.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box2.ShowWindowAsync();
                }

            }

        }
    }

    private static bool ExecutePS3MCACommand(string Argument)
    {
        using var PS2MCTool = new Process();
        PS2MCTool.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3mca-tool");
        PS2MCTool.StartInfo.Arguments = Argument;
        PS2MCTool.StartInfo.RedirectStandardOutput = true;
        PS2MCTool.StartInfo.UseShellExecute = false;
        PS2MCTool.StartInfo.CreateNoWindow = true;
        PS2MCTool.Start();
        PS2MCTool.WaitForExit();

        var OutputReader = PS2MCTool.StandardOutput;
        string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

        if (ProcessOutput.Length > 1)
        {
            if (!ProcessOutput.Contains("ERROR:"))
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

    private void MemoryCardContentListBox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            if (MemoryCardContentListBox.SelectedItem is not null)
            {

                PS2MCContentListViewItem SelectedMCContent = (PS2MCContentListViewItem)MemoryCardContentListBox.SelectedItem;
                if (SelectedMCContent.FileName == "..")
                {
                    if (!string.IsNullOrEmpty(CurrentMCPathTextBox.Text) && !(CurrentMCPathTextBox.Text == "/"))
                    {
                        // Go back
                        string NewPath = CurrentMCPathTextBox.Text[..CurrentMCPathTextBox.Text.LastIndexOf('/')] + "/";
                        LoadPS2MCDirectory(NewPath);
                        CurrentMCPathTextBox.Text = NewPath;
                    }
                }
                else if (!(SelectedMCContent.FileName == "."))
                {
                    // Browse next path
                    string NewPath = CurrentMCPathTextBox.Text + SelectedMCContent.FileName;
                    LoadPS2MCDirectory(NewPath);
                    CurrentMCPathTextBox.Text = NewPath;
                }

            }
        }
    }

}