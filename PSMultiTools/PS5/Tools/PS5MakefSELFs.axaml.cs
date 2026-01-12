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

    public PS5MakefSELFs()
    {
        InitializeComponent();

        PatchSDKCheckBox.IsCheckedChanged += PatchSDKCheckBox_IsCheckedChanged;
    }

    // PS5 Make Fake Self Batch Script by EchoStretch
    // PS5 SDK Patch (Auto_backport) Utility by Markus95
    // Translated C# code by SvenGDK

    private bool AlreadyPatched = false;
    private long TotalSize = 0L;

    private const uint PT_SCE_PROCPARAM = 1627389953U;
    private const uint PT_SCE_MODULE_PARAM = 1627389954U;

    private const uint SCE_PROCESS_PARAM_MAGIC = 1229083215U;
    private const uint SCE_MODULE_PARAM_MAGIC = 1007940799U;

    private const long SCE_PARAM_PS5_SDK_OFFSET = 12L;
    private const long SCE_PARAM_BASE_SDK_OFFSET = 8L;

    private const long PHT_OFFSET_OFFSET = 32L;
    private const int PHT_OFFSET_SIZE = 8;
    private const long PHT_COUNT_OFFSET = 56L;
    private const int PHT_COUNT_SIZE = 2;

    private const long PHDR_ENTRY_SIZE = 56L;
    private const long PHDR_TYPE_OFFSET = 0L;
    private const int PHDR_TYPE_SIZE = 4;
    private const long PHDR_OFFSET_OFFSET = 8L;
    private const int PHDR_OFFSET_SIZE = 8;

    private readonly byte[] ELF_MAGIC = [127, 69, 76, 70];
    private readonly byte[] PS4_FSELF_MAGIC = [79, 21, 61, 29];
    private readonly byte[] PS5_FSELF_MAGIC = [84, 20, 245, 238];

    private readonly string[] ExecutableExtensions = [".bin", ".elf", ".self", ".prx", ".sprx"];

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
            AlreadyPatched = true;
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

            // Check if files should be patched before fake signing
            if (PatchSDKCheckBox.IsChecked == true)
            {
                // Get selected Target SDK version
                string SelectedTargetSDKVersionValue = TargetSDKVersionComboBox.Text!;
                uint ConvertedTargetSDKVersionValue = 0U;
                try
                {
                    if (SelectedTargetSDKVersionValue.StartsWith("0x"))
                    {
                        ConvertedTargetSDKVersionValue = Convert.ToUInt32(SelectedTargetSDKVersionValue[2..], 16);
                    }
                    else
                    {
                        ConvertedTargetSDKVersionValue = Convert.ToUInt32(SelectedTargetSDKVersionValue);
                    }
                }
                catch (Exception)
                {
                    var box2 = MessageBoxManager.GetMessageBoxStandard("", "Invalid value for --target_sdk", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box2.ShowWindowAsync();
                    return;
                }

                bool CreateBackupFiles = false;
                if (CreateBackupCheckBox.IsChecked == true)
                {
                    CreateBackupFiles = true;
                }

                if (Directory.Exists(SelectedDirectoryTextBox.Text))
                {
                    var BackupFiles = Directory.GetFiles(SelectedDirectoryTextBox.Text, "*.*", SearchOption.AllDirectories).Where(f => ExecutableExtensions.Contains(Path.GetExtension(f).ToLower()));
                    foreach (var BackupFilePath in BackupFiles)
                        ProcessFile(BackupFilePath, CreateBackupFiles, ConvertedTargetSDKVersionValue);
                }
                else
                {
                    var box3 = MessageBoxManager.GetMessageBoxStandard("Error", "The backup folder cannot be found.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box3.ShowWindowAsync();
                    return;
                }

                var box4 = MessageBoxManager.GetMessageBoxStandard("Patch SDK", "Done patching Base SDK Version.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box4.ShowWindowAsync();
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
                    Make_fSELF.StartInfo.FileName = Environment.CurrentDirectory + @"\Tools\make_fself_python3-1.exe";
                    Make_fSELF.StartInfo.Arguments = $"\"{FileToSign}\" \"{FullTempFilePath}\"";
                    Make_fSELF.StartInfo.RedirectStandardOutput = true;
                    Make_fSELF.StartInfo.UseShellExecute = false;
                    Make_fSELF.StartInfo.CreateNoWindow = true;
                    Make_fSELF.Start();
                    Make_fSELF.WaitForExit();

                    // Read the output
                    var OutputReader = Make_fSELF.StandardOutput;
                    string ProcessOutput = OutputReader.ReadToEnd();

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

    private bool PatchFile(FileStream FS, uint TargetSDKVersion)
    {
        // Read the segment count (2 bytes at PHT_COUNT_OFFSET)
        var PHTCountBuffer = new byte[2];
        FS.Seek(PHT_COUNT_OFFSET, SeekOrigin.Begin);
        FS.ReadExactly(PHTCountBuffer, 0, PHT_COUNT_SIZE);
        ushort SegmentCount = BitConverter.ToUInt16(PHTCountBuffer, 0);

        // Read the offset to the program header table (8 bytes at PHT_OFFSET_OFFSET)
        var PHTOffsetBuffer = new byte[8];
        FS.Seek(PHT_OFFSET_OFFSET, SeekOrigin.Begin);
        FS.ReadExactly(PHTOffsetBuffer, 0, PHT_OFFSET_SIZE);
        ulong PHTOffset = BitConverter.ToUInt64(PHTOffsetBuffer, 0);

        // Loop through each segment
        for (uint i = 0U, loopTo = (uint)(SegmentCount - 1); i <= loopTo; i++)
        {
            long EntryOffset = (long)Math.Round(PHTOffset + (decimal)(i * PHDR_ENTRY_SIZE));

            // Read segment type (4 bytes)
            FS.Seek(EntryOffset + PHDR_TYPE_OFFSET, SeekOrigin.Begin);
            var SegmentTypeBuffer = new byte[4];
            FS.ReadExactly(SegmentTypeBuffer, 0, PHDR_TYPE_SIZE);
            uint SegmentType = BitConverter.ToUInt32(SegmentTypeBuffer, 0);

            // Read segment file offset (8 bytes)
            FS.Seek(EntryOffset + PHDR_OFFSET_OFFSET, SeekOrigin.Begin);
            var SegOffsetBuffer = new byte[8];
            FS.ReadExactly(SegOffsetBuffer, 0, PHDR_OFFSET_SIZE);
            ulong StructStartOffset = BitConverter.ToUInt64(SegOffsetBuffer, 0);

            // Read the parameter magic (first 4 bytes at the structure)
            FS.Seek((long)StructStartOffset, SeekOrigin.Begin);
            var TParamMagicBuffer = new byte[4];
            FS.ReadExactly(TParamMagicBuffer, 0, 4);
            uint ParamMagic = BitConverter.ToUInt32(TParamMagicBuffer, 0);

            // Check magic values depending on segment type
            if (SegmentType == PT_SCE_PROCPARAM)
            {
                if (ParamMagic != SCE_PROCESS_PARAM_MAGIC)
                {
                    StructStartOffset += 8UL;
                    FS.Seek((long)StructStartOffset, SeekOrigin.Begin);
                    FS.ReadExactly(TParamMagicBuffer, 0, 4);
                    ParamMagic = BitConverter.ToUInt32(TParamMagicBuffer, 0);
                    if (ParamMagic != SCE_PROCESS_PARAM_MAGIC)
                    {
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                MakeLogTextBox.Text += ("Invalid process param magic" + "\r\n");
                                ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                                LogTextBoxScrollViewer?.ScrollToEnd();
                            });
                        }
                        else
                        {
                            MakeLogTextBox.Text += ("Invalid process param magic" + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        }
                    }
                }
            }
            else if (SegmentType == PT_SCE_MODULE_PARAM)
            {
                if (ParamMagic != SCE_MODULE_PARAM_MAGIC)
                {
                    StructStartOffset += 8UL;
                    FS.Seek((long)StructStartOffset, SeekOrigin.Begin);
                    FS.ReadExactly(TParamMagicBuffer, 0, 4);
                    ParamMagic = BitConverter.ToUInt32(TParamMagicBuffer, 0);
                    if (ParamMagic != SCE_MODULE_PARAM_MAGIC)
                    {
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                MakeLogTextBox.Text += ("Invalid module param magic for file '" + FS.Name + "', skipping" + "\r\n");
                                ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                                LogTextBoxScrollViewer?.ScrollToEnd();
                            });
                        }
                        else
                        {
                            MakeLogTextBox.Text += ("Invalid module param magic for file '" + FS.Name + "', skipping" + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        }
                        continue;
                    }
                }
            }
            else
            {
                continue;
            }

            // Patch the PS5 SDK version at SCE_PARAM_PS5_SDK_OFFSET
            FS.Seek((long)Math.Round(StructStartOffset + (decimal)SCE_PARAM_PS5_SDK_OFFSET), SeekOrigin.Begin);
            var SDKBuffer = new byte[4];
            FS.ReadExactly(SDKBuffer, 0, 4);
            uint CurrentSDK = BitConverter.ToUInt32(SDKBuffer, 0);

            byte[] TargetSDKBytes = BitConverter.GetBytes(TargetSDKVersion);
            FS.Seek((long)Math.Round(StructStartOffset + (decimal)SCE_PARAM_PS5_SDK_OFFSET), SeekOrigin.Begin);
            FS.Write(TargetSDKBytes, 0, 4);

            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MakeLogTextBox.Text += ("Patched PS5 SDK version from 0x" + CurrentSDK.ToString("X8") + " to 0x" + TargetSDKVersion.ToString("X8") + " for file '" + FS.Name + "'" + "\r\n");
                    ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    LogTextBoxScrollViewer?.ScrollToEnd();
                });
            }
            else
            {
                MakeLogTextBox.Text += ("Patched PS5 SDK version from 0x" + CurrentSDK.ToString("X8") + " to 0x" + TargetSDKVersion.ToString("X8") + " for file '" + FS.Name + "'" + "\r\n");
                ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                LogTextBoxScrollViewer?.ScrollToEnd();
            }

            // Patch the base SDK version at SCE_PARAM_BASE_SDK_OFFSET
            FS.Seek((long)Math.Round(StructStartOffset + (decimal)SCE_PARAM_BASE_SDK_OFFSET), SeekOrigin.Begin);
            FS.ReadExactly(SDKBuffer, 0, 4);
            uint baseSDK = BitConverter.ToUInt32(SDKBuffer, 0);
            FS.Seek((long)Math.Round(StructStartOffset + (decimal)SCE_PARAM_BASE_SDK_OFFSET), SeekOrigin.Begin);
            FS.Write(TargetSDKBytes, 0, 4);

            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MakeLogTextBox.Text += ("Patched base SDK version from 0x" + baseSDK.ToString("X8") + " to 0x" + TargetSDKVersion.ToString("X8") + " for file '" + FS.Name + "'" + "\r\n");
                    ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    LogTextBoxScrollViewer?.ScrollToEnd();
                });
            }
            else
            {
                MakeLogTextBox.Text += ("Patched base SDK version from 0x" + baseSDK.ToString("X8") + " to 0x" + TargetSDKVersion.ToString("X8") + " for file '" + FS.Name + "'" + "\r\n");
                ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                LogTextBoxScrollViewer?.ScrollToEnd();
            }

            return true;
        }

        return false;
    }

    private void ProcessFile(string FilePath, bool CreateBackup, uint TargetSDKVersion)
    {
        using var NewFileStream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);
        var NewBinaryReader = new BinaryReader(NewFileStream);
        NewFileStream.Seek(0L, SeekOrigin.Begin);
        byte[] FileMagic = NewBinaryReader.ReadBytes(4);

        // File checks
        if (!FileMagic.SequenceEqual(ELF_MAGIC))
        {
            if (FileMagic.SequenceEqual(PS4_FSELF_MAGIC) || FileMagic.SequenceEqual(PS5_FSELF_MAGIC))
            {
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        MakeLogTextBox.Text += ("Aborting, File '" + FilePath + "' is a signed file. This script expects unsigned ELF files." + "\r\n");
                        ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    });
                }
                else
                {
                    MakeLogTextBox.Text += ("Aborting, File '" + FilePath + "' is a signed file. This script expects unsigned ELF files." + "\r\n");
                    ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    LogTextBoxScrollViewer?.ScrollToEnd();
                }
            }
            return;
        }

        NewFileStream.Seek(0L, SeekOrigin.Begin);

        // Create backup file of original file
        if (CreateBackup && !File.Exists(FilePath + ".psbak"))
        {
            File.Copy(FilePath, FilePath + ".psbak");

            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MakeLogTextBox.Text += ("Backup file created for '" + FilePath + "'" + "\r\n");
                    ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    LogTextBoxScrollViewer?.ScrollToEnd();
                });
            }
            else
            {
                MakeLogTextBox.Text += ("Backup file created for '" + FilePath + "'" + "\r\n");
                ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                LogTextBoxScrollViewer?.ScrollToEnd();
            }
        }

        // Patch the file
        bool FilePatched = PatchFile(NewFileStream, TargetSDKVersion);
        if (FilePatched)
        {
            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MakeLogTextBox.Text += ("Patched '" + FilePath + "'" + "\r\n");
                    ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    LogTextBoxScrollViewer?.ScrollToEnd();
                });
            }
            else
            {
                MakeLogTextBox.Text += ("Patched '" + FilePath + "'" + "\r\n");
                ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                LogTextBoxScrollViewer?.ScrollToEnd();
            }
        }
        else if (Dispatcher.UIThread.CheckAccess() == false)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                MakeLogTextBox.Text += ("Failed to patch '" + FilePath + "'" + "\r\n");
                ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                LogTextBoxScrollViewer?.ScrollToEnd();
            });
        }
        else
        {
            MakeLogTextBox.Text += ("Failed to patch '" + FilePath + "'" + "\r\n");
            ScrollViewer? LogTextBoxScrollViewer = MakeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
            LogTextBoxScrollViewer?.ScrollToEnd();
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

    private async void RestoreOriginalBackup(string BackupPath)
    {
        // Get already patched files & delete them
        var FilesToDelete = Directory.EnumerateFiles(BackupPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".prx") || s.EndsWith(".elf") || s.EndsWith(".bin") || s.EndsWith(".sprx") || s.EndsWith(".self"));
        foreach (var FileToDelete in FilesToDelete)
            File.Delete(FileToDelete);

        // Get backup files & restore them
        var FilesToRecover = Directory.EnumerateFiles(BackupPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".esbak"));
        foreach (var FileToRecover in FilesToRecover)
        {
            string NewFilePath = Path.Combine(Path.GetDirectoryName(FileToRecover)!, Path.GetFileNameWithoutExtension(FileToRecover));
            File.Move(FileToRecover, NewFilePath, true);
        }

        AlreadyPatched = false;
        var box = MessageBoxManager.GetMessageBoxStandard("Done recovering", "Backup files have been restored. The selected backup can now be patched and/or faked signed again.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
        await box.ShowWindowAsync();
    }

    private async void PatchSDKCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (PatchSDKCheckBox.IsChecked == true)
        {
            if (!string.IsNullOrEmpty(SelectedDirectoryTextBox.Text))
            {
                if (AlreadyPatched)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("PS Multi Tools", "This backup has been already modified and is probably patched." + Environment.NewLine + "Do you want to restore the original backup files before patching this backup again ? (Recommended)", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {
                        RestoreOriginalBackup(SelectedDirectoryTextBox.Text);
                    }
                }
            }
            else
            {
                // Reset AlreadyPatched when SelectedDirectoryTextBox.Text is empty and PatchSDK got checked
                AlreadyPatched = false;
            }
        }
    }

}