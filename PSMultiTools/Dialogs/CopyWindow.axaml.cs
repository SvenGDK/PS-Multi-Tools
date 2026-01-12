using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace PSMultiTools.Dialogs;

public partial class CopyWindow : Window
{

    private readonly BackgroundWorker CopyWorker = new() { WorkerReportsProgress = true };

    public required string BackupPath;
    public required string BackupDestinationPath;

    public List<PS3Game> MultipleFileList = [];
    public int FilesCount;
    public AnyBitmap? GameIcon;

    public struct CopyWorkerArgs
    {
        public bool IsBackupDirectory { get; set; }
        public bool MultipleItems { get; set; }
    }

    public CopyWindow()
    {
        InitializeComponent();

        Loaded += CopyWindow_Loaded;
        CopyWorker.DoWork += CopyWorker_DoWork;
        CopyWorker.ProgressChanged += CopyWorker_ProgressChanged;
        CopyWorker.RunWorkerCompleted += CopyWorker_RunWorkerCompleted;
    }

    private void CopyWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        if (GameIcon != null)
        {
            GameIconImage.Source = new Avalonia.Media.Imaging.Bitmap(GameIcon.GetStream());
        }

        BeginCopy();
    }

    public async void BeginCopy()
    {
        try
        {
            if (MultipleFileList.Count > 1) // Combination of pkgs and/or backup folders
            {
                FilesCount = MultipleFileList.Count;
                CopyProgressBar.Maximum = MultipleFileList.Count;
                StatusTextBlock.Text = "0 / " + MultipleFileList.Count.ToString();

                CopyWorker.RunWorkerAsync(new CopyWorkerArgs() { MultipleItems = true });
            }
            else if (BackupPath.EndsWith(".pkg") | BackupPath.EndsWith(".iso") | BackupPath.EndsWith(".cso")) // Single file
            {
                FilesCount = 1;
                CopyProgressBar.Maximum = 1;
                StatusTextBlock.Text = "0 / 1 file";

                CopyWorker.RunWorkerAsync(new CopyWorkerArgs() { IsBackupDirectory = false, MultipleItems = false });
            }
            else // Single Backup folder
            {
                CountFiles(BackupPath, ref FilesCount);
                CopyProgressBar.Maximum = FilesCount;
                StatusTextBlock.Text = "0 / " + FilesCount.ToString() + " files";

                CopyWorker.RunWorkerAsync(new CopyWorkerArgs() { IsBackupDirectory = true, MultipleItems = false });
            }
        }
        catch (Exception)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error! Returning...", "Cannot copy selected backup(s)", ButtonEnum.Ok);
            await box.ShowWindowAsync();
            Close();
        }
    }

    private static void CountFiles(string InFolder, ref int Result)
    {
        Result += Directory.GetFiles(InFolder).Length;
        foreach (string f in Directory.GetDirectories(InFolder))
            CountFiles(f, ref Result);
    }

    private void CopyWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
        if (e.Argument != null)
        {
            CopyWorkerArgs Args = (CopyWorkerArgs)e.Argument;

            if (Args.MultipleItems)
            {
                foreach (PS3Game FileToCopy in MultipleFileList)
                {
                    if (FileToCopy.GameFileType == PS3Game.GameFileTypes.PKG)
                    {
                        if (FileToCopy.GameFilePath != null)
                        {
                            CopyFile(FileToCopy.GameFilePath, BackupDestinationPath);
                            CopyWorker.ReportProgress(+1);
                        }
                    }
                    else if (FileToCopy.GameFileType == PS3Game.GameFileTypes.Backup)
                    {
                        if (FileToCopy.GameFolderPath != null)
                        {
                            var BackupName = new DirectoryInfo(FileToCopy.GameFolderPath);
                            CopyDirectory(FileToCopy.GameFolderPath, BackupDestinationPath + BackupName.Name);
                            CopyWorker.ReportProgress(+1);
                        }
                    }
                    else if (FileToCopy.GameFileType == PS3Game.GameFileTypes.PS3ISO)
                    {
                        if (FileToCopy.GameFilePath != null)
                        {
                            CopyFile(FileToCopy.GameFilePath, BackupDestinationPath);
                            CopyWorker.ReportProgress(+1);
                        }
                    }
                }
            }
            else if (Args.IsBackupDirectory)
            {
                var BackupName = new DirectoryInfo(BackupPath);
                Directory.CreateDirectory(BackupDestinationPath + BackupName.Name);
                CopyDirectory(BackupPath, BackupDestinationPath + BackupName.Name);
            }
            else
            {
                CopyFile(BackupPath, BackupDestinationPath);
            }
        }
    }

    private void CopyWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        if (e.UserState != null)
        {
            Structures.CopyItem CopyInfo = (Structures.CopyItem)e.UserState;
            CopyProgressBar.Value += e.ProgressPercentage;

            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() => StatusTextBlock.Text = "Copying " + CopyInfo.FileName);
            }
            else
            {
                StatusTextBlock.Text = "Copying " + CopyInfo.FileName;
            }
        }

        if (Dispatcher.UIThread.CheckAccess() == false)
        {
            Dispatcher.UIThread.Invoke(() => StatusTextBlock2.Text = CopyProgressBar.Value.ToString() + " / " + FilesCount.ToString() + " files");
        }
        else
        {
            StatusTextBlock2.Text = CopyProgressBar.Value.ToString() + " / " + FilesCount.ToString() + " files";
        }
    }

    private async void CopyWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        if (MultipleFileList.Count > 1)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Done", "All selected backups copied with success!" + Environment.NewLine + "Do you want to open the destination folder?", ButtonEnum.YesNo);
            var boxresult = await box.ShowWindowDialogAsync(this);

            if (boxresult == ButtonResult.Yes)
            {
                Utils.OpenFolder(BackupDestinationPath);
                Close(true);
            }
            else
            {
                Close(true);
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Done", "Selected backup copied with success!" + Environment.NewLine + "Do you want to open the destination folder?", ButtonEnum.YesNo);
            var boxresult = await box.ShowWindowDialogAsync(this);

            if (boxresult == ButtonResult.Yes)
            {
                Utils.OpenFolder(BackupDestinationPath);
                Close(true);
            }
            else
            {
                Close(true);
            }
        }
    }

    public void CopyDirectory(string sourcePath, string destinationPath)
    {
        // Re-create the entire backup folder at the destination with recursive call
        var SourceDirInfo = new DirectoryInfo(sourcePath);

        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
        }

        foreach (FileSystemInfo FileSysInfo in SourceDirInfo.GetFileSystemInfos())
        {
            string destinationFileName = Path.Combine(destinationPath, FileSysInfo.Name);

            if (FileSysInfo is FileInfo)
            {
                File.Copy(FileSysInfo.FullName, destinationFileName, true);
                if (MultipleFileList.Count > 1)
                {
                    CopyWorker.ReportProgress(0, new Structures.CopyItem() { FileName = FileSysInfo.Name });
                }
                else
                {
                    CopyWorker.ReportProgress(+1, new Structures.CopyItem() { FileName = FileSysInfo.Name });
                }
            }
            else
            {
                CopyDirectory(FileSysInfo.FullName, destinationFileName);
            }
        }
    }

    public void CopyFile(string sourceFile, string destinationFile)
    {
        var FI = new FileInfo(sourceFile);
        File.Copy(sourceFile, destinationFile + FI.Name, true);
        if (MultipleFileList.Count > 1)
        {
            CopyWorker.ReportProgress(0, new Structures.CopyItem() { FileName = FI.Name });
        }
        else
        {
            CopyWorker.ReportProgress(+1, new Structures.CopyItem() { FileName = FI.Name });
        }
    }

}