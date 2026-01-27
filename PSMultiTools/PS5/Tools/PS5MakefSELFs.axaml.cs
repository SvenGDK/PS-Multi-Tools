using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using PSMultiTools.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static PSMultiTools.Classes.PS5ParamClass;

namespace PSMultiTools.PS5.Tools;

public partial class PS5MakefSELFs : Window
{
    // PS5 Make Fake Self Batch Script by EchoStretch
    // Translated C# code by SvenGDK

    private long TotalSize = 0;

    public PS5MakefSELFs()
    {
        InitializeComponent();
    }

    private async void BrowseFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog();
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedDirectoryTextBox.Text = FBDResult;
            MakeButton.IsEnabled = true;

            if (!string.IsNullOrEmpty(MakeLogTextBox.Text))
            {
                MakeLogTextBox.Clear();
            }

            CheckFiles(FBDResult);
        }
    }

    private async void CheckFiles(string FilePath)
    {

        string ParamPath = Path.Combine(FilePath, "sce_sys", "param.json");

        // Check if game is an extracted PS5 backup
        if (!File.Exists(ParamPath))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Cannot process this backup", "This is not a valid PS5 backup.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return;
        }
        else
        {
            MakeLogTextBox.Text += ("Found valid '" + ParamPath + "'" + "\r\n");
            ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
            LogTextBoxScrollViewer?.ScrollToEnd();
        }

        // Check if .esbak backup files exist in selected backup
        if (File.Exists(Path.Combine(FilePath, "eboot.bin.esbak")))
        {
            MakeLogTextBox.Text += ("Existing '.esbak' files found." + "\r\n");
            ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
            LogTextBoxScrollViewer?.ScrollToEnd();
        }

        // Display icon if exists
        if (File.Exists(Path.Combine(FilePath, "sce_sys", "icon0.png")))
        {
            var TempBitmapImage = new Avalonia.Media.Imaging.Bitmap(Path.Combine(FilePath, "sce_sys", "icon0.png"));
            BackupIconImage.Source = TempBitmapImage;
        }

        // Load backup data
        var ParamData = JsonConvert.DeserializeObject<PS5Param>(File.ReadAllText(ParamPath))!;
        var ParamFileInfo = new FileInfo(ParamPath);
        string BackupFolderPath = Directory.GetParent(ParamFileInfo.FullName)!.Parent!.FullName;

        if (ParamData.TitleId is not null)
        {
            TitleIDTextBlock.Text = "Title ID: " + ParamData.TitleId;
            RegionTextBlock.Text = "Region: " + PS5Game.GetGameRegion(ParamData.TitleId);
        }
        if (ParamData.LocalizedParameters!.EnUS is not null)
        {
            TitleTextBlock.Text = ParamData.LocalizedParameters.EnUS.TitleName;
        }
        if (ParamData.ContentId is not null)
        {
            ContentIDTextBlock.Text = "Content ID: " + ParamData.ContentId;
        }
        if (ParamData.ApplicationCategoryType == 0)
        {
            TypeTextBlock.Text = "Type: Game";
        }
        else if (ParamData.ApplicationCategoryType == 65536)
        {
            TypeTextBlock.Text = "Type: Native Media App";
        }
        else if (ParamData.ApplicationCategoryType == 65792)
        {
            TypeTextBlock.Text = "Type: RNPS Media App";
        }
        else if (ParamData.ApplicationCategoryType == 131328)
        {
            TypeTextBlock.Text = "Type: System Built-in App";
        }
        else if (ParamData.ApplicationCategoryType == 131584)
        {
            TypeTextBlock.Text = "Type: Big Daemon";
        }
        else if (ParamData.ApplicationCategoryType == 16777216)
        {
            TypeTextBlock.Text = "Type: ShellUI";
        }
        else if (ParamData.ApplicationCategoryType == 33554432)
        {
            TypeTextBlock.Text = "Type: Daemon";
        }
        else if (ParamData.ApplicationCategoryType == 67108864)
        {
            TypeTextBlock.Text = "Type: ShellApp";
        }
        else
        {
            TypeTextBlock.Text = "Type: Unknown";
        }

        SizeTextBlock.Text = "Size: " + Microsoft.VisualBasic.Strings.FormatNumber(GetDirSize(BackupFolderPath) / 1073741824d, 2) + " GB";

        // Set Base SDK Version based on param.json
        if (ParamData.SdkVersion is not null)
        {
            MakeLogTextBox.Text += "Retrieved Base SDK Version from param.json: '" + ParamData.SdkVersion.Remove(9, 8) + "'" + "\r\n";
            ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
            LogTextBoxScrollViewer?.ScrollToEnd();
        }

    }

    private async void MakeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedDirectoryTextBox.Text))
        {

            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MakeButton.IsEnabled = false;
                    SelectedDirectoryTextBox.IsEnabled = false;
                    BrowseFolderButton.IsEnabled = false;
                });
            }
            else
            {
                MakeButton.IsEnabled = false;
                SelectedDirectoryTextBox.IsEnabled = false;
                BrowseFolderButton.IsEnabled = false;
            }

            // Collect all files that need to be signed
            var FilesToSign = Directory.EnumerateFiles(SelectedDirectoryTextBox.Text, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".prx") || s.EndsWith(".sprx") || s.EndsWith(".elf") || s.EndsWith(".self") || s.EndsWith(".bin"));

            // Fake sign each file with make_fself_python3-1
            foreach (var FileToSign in FilesToSign)
            {
                string FileToSignDirectory = Path.GetDirectoryName(FileToSign)!;
                string FileToSignFileName = Path.GetFileName(FileToSign);
                string FileToSignTempFileName = FileToSignFileName + ".estemp";
                string BackupFileName = FileToSignFileName + ".esbak";
                string FullTempFilePath = Path.Combine(FileToSignDirectory, FileToSignTempFileName);
                string BackupFilePath = Path.Combine(FileToSignDirectory, BackupFileName);

                using (var Make_fSELF = new Process())
                {
                    Make_fSELF.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "make_fself_ps5.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "make_fself_ps5");
                    Make_fSELF.StartInfo.Arguments = $"\"{FileToSign}\" \"{FullTempFilePath}\"";
                    Make_fSELF.StartInfo.RedirectStandardOutput = true;
                    Make_fSELF.StartInfo.UseShellExecute = false;
                    Make_fSELF.StartInfo.CreateNoWindow = true;
                    Make_fSELF.Start();

                    // Read the output
                    var OutputReader = Make_fSELF.StandardOutput;
                    string ProcessOutput = OutputReader.ReadToEnd();

                    await Make_fSELF.WaitForExitAsync();
                    Make_fSELF.Close();

                    if (!string.IsNullOrEmpty(ProcessOutput))
                    {
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                MakeLogTextBox.Text += (ProcessOutput + "\r\n");
                                ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                                LogTextBoxScrollViewer?.ScrollToEnd();
                            });
                        }
                        else
                        {
                            MakeLogTextBox.Text += (ProcessOutput + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        }
                    }
                }

                // Backup original file
                File.Move(FileToSign, BackupFilePath);
            }

            // Rename the estemp files to their origin file name
            foreach (var TempFile in Directory.GetFiles(SelectedDirectoryTextBox.Text, "*.estemp", SearchOption.AllDirectories))
            {
                string TempFileDirectory = Path.GetDirectoryName(TempFile)!;
                string TempFileNameWithoutTempExtension = Path.GetFileNameWithoutExtension(TempFile);
                string NewFilePath = Path.Combine(TempFileDirectory, TempFileNameWithoutTempExtension);
                File.Move(TempFile, NewFilePath);
            }

            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MakeButton.IsEnabled = true;
                    SelectedDirectoryTextBox.IsEnabled = true;
                    BrowseFolderButton.IsEnabled = true;
                });
            }
            else
            {
                MakeButton.IsEnabled = true;
                SelectedDirectoryTextBox.IsEnabled = true;
                BrowseFolderButton.IsEnabled = true;
            }

            var box5 = MessageBoxManager.GetMessageBoxStandard("Done", "SELF files fake signed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box5.ShowWindowAsync();
        }
        else
        {
            var box6 = MessageBoxManager.GetMessageBoxStandard("Error", "No folder selected!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box6.ShowWindowAsync();
        }
    }

    private long GetDirSize(string RootFolder)
    {
        var FolderInfo = new DirectoryInfo(RootFolder);
        foreach (var File in FolderInfo.GetFiles())
            TotalSize += File.Length;
        foreach (var SubFolderInfo in FolderInfo.GetDirectories())
            GetDirSize(SubFolderInfo.FullName);
        return TotalSize;
    }

}