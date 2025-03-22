Imports System.ComponentModel
Imports System.IO

Public Class CopyWindow

    Dim WithEvents CopyWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}

    Public BackupPath As String
    Public BackupDestinationPath As String

    Public MultipleFileList As New List(Of PS3Game)()
    Public FilesCount As Integer
    Public GameIcon As ImageSource

    Public Structure CopyWorkerArgs
        Public Property IsBackupDirectory As Boolean
        Public Property MultipleItems As Boolean
    End Structure

    Private Sub CopyWindow_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered

        If GameIcon IsNot Nothing Then
            GameIconImage.Source = GameIcon
        End If

        BeginCopy()
    End Sub

    Public Sub BeginCopy()
        Try
            If MultipleFileList.Count > 1 Then 'Combination of pkgs and/or backup folders
                FilesCount = MultipleFileList.Count
                CopyProgressBar.Maximum = MultipleFileList.Count
                StatusTextBlock.Text = "0 / " + MultipleFileList.Count.ToString

                CopyWorker.RunWorkerAsync(New CopyWorkerArgs With {.MultipleItems = True})
            Else
                If BackupPath.EndsWith(".pkg") Or BackupPath.EndsWith(".iso") Or BackupPath.EndsWith(".cso") Then 'Single file
                    FilesCount = 1
                    CopyProgressBar.Maximum = 1
                    StatusTextBlock.Text = "0 / 1 file"

                    CopyWorker.RunWorkerAsync(New CopyWorkerArgs With {.IsBackupDirectory = False, .MultipleItems = False})
                Else 'Single Backup folder
                    CountFiles(BackupPath, FilesCount)
                    CopyProgressBar.Maximum = FilesCount
                    StatusTextBlock.Text = "0 / " + FilesCount.ToString + " files"

                    CopyWorker.RunWorkerAsync(New CopyWorkerArgs With {.IsBackupDirectory = True, .MultipleItems = False})
                End If
            End If
        Catch ex As Exception
            MsgBox("Cannot copy selected backup(s)", MsgBoxStyle.Critical, "Error! Returning...")
            Close()
        End Try
    End Sub

    Private Sub CountFiles(InFolder As String, ByRef Result As Integer)
        Result += Directory.GetFiles(InFolder).Length
        For Each f As String In Directory.GetDirectories(InFolder)
            CountFiles(f, Result)
        Next
    End Sub

    Private Sub CopyWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles CopyWorker.DoWork
        Dim Args As CopyWorkerArgs = CType(e.Argument, CopyWorkerArgs)

        If Args.MultipleItems Then
            For Each FileToCopy As PS3Game In MultipleFileList
                If FileToCopy.GameFileType = PS3Game.GameFileTypes.PKG Then
                    CopyFile(FileToCopy.GameFilePath, BackupDestinationPath)
                    CopyWorker.ReportProgress(+1)
                ElseIf FileToCopy.GameFileType = PS3Game.GameFileTypes.Backup Then
                    Dim BackupName As New DirectoryInfo(FileToCopy.GameFolderPath)
                    CopyDirectory(FileToCopy.GameFolderPath, BackupDestinationPath + BackupName.Name)
                    CopyWorker.ReportProgress(+1)
                ElseIf FileToCopy.GameFileType = PS3Game.GameFileTypes.PS3ISO Then
                    CopyFile(FileToCopy.GameFilePath, BackupDestinationPath)
                    CopyWorker.ReportProgress(+1)
                End If
            Next
        Else
            If Args.IsBackupDirectory Then
                Dim BackupName As New DirectoryInfo(BackupPath)
                Directory.CreateDirectory(BackupDestinationPath + BackupName.Name)
                CopyDirectory(BackupPath, BackupDestinationPath + BackupName.Name)
            Else
                CopyFile(BackupPath, BackupDestinationPath)
            End If
        End If
    End Sub

    Private Sub CopyWorker_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles CopyWorker.ProgressChanged
        Dim CopyInfo As Structures.CopyItem = CType(e.UserState, Structures.CopyItem)
        CopyProgressBar.Value += e.ProgressPercentage

        If StatusTextBlock.Dispatcher.CheckAccess() = False Then
            StatusTextBlock.Dispatcher.BeginInvoke(Sub() StatusTextBlock.Text = "Copying " + CopyInfo.FileName)
        Else
            StatusTextBlock.Text = "Copying " + CopyInfo.FileName
        End If

        If StatusTextBlock2.Dispatcher.CheckAccess() = False Then
            StatusTextBlock2.Dispatcher.BeginInvoke(Sub() StatusTextBlock2.Text = CopyProgressBar.Value.ToString + " / " + FilesCount.ToString + " files")
        Else
            StatusTextBlock2.Text = CopyProgressBar.Value.ToString + " / " + FilesCount.ToString + " files"
        End If
    End Sub

    Private Sub CopyWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles CopyWorker.RunWorkerCompleted
        If MultipleFileList.Count > 1 Then
            If MsgBox("All selected backups copied with success!" + vbCrLf + "Do you want to open the destination folder?", MsgBoxStyle.YesNo, "Done") = MsgBoxResult.Yes Then
                Process.Start("explorer", BackupDestinationPath)
                Close()
            Else
                Close()
            End If
        Else
            If MsgBox("Selected backup copied with success!" + vbCrLf + "Do you want to open the destination folder?", MsgBoxStyle.YesNo, "Done") = MsgBoxResult.Yes Then
                Process.Start("explorer", BackupDestinationPath)
                Close()
            Else
                Close()
            End If
        End If
    End Sub

    Public Sub CopyDirectory(sourcePath As String, destinationPath As String)
        'Re-create the entire backup folder at the destination with recursive call
        Dim SourceDirInfo As New DirectoryInfo(sourcePath)

        If Not Directory.Exists(destinationPath) Then
            Directory.CreateDirectory(destinationPath)
        End If

        For Each FileSysInfo As FileSystemInfo In SourceDirInfo.GetFileSystemInfos
            Dim destinationFileName As String = Path.Combine(destinationPath, FileSysInfo.Name)

            If TypeOf FileSysInfo Is FileInfo Then
                File.Copy(FileSysInfo.FullName, destinationFileName, True)
                If MultipleFileList.Count > 1 Then
                    CopyWorker.ReportProgress(0, New Structures.CopyItem With {.FileName = FileSysInfo.Name})
                Else
                    CopyWorker.ReportProgress(+1, New Structures.CopyItem With {.FileName = FileSysInfo.Name})
                End If
            Else
                CopyDirectory(FileSysInfo.FullName, destinationFileName)
            End If
        Next
    End Sub

    Public Sub CopyFile(sourceFile As String, destinationFile As String)
        Dim FI As New FileInfo(sourceFile)
        File.Copy(sourceFile, destinationFile + FI.Name, True)
        If MultipleFileList.Count > 1 Then
            CopyWorker.ReportProgress(0, New Structures.CopyItem With {.FileName = FI.Name})
        Else
            CopyWorker.ReportProgress(+1, New Structures.CopyItem With {.FileName = FI.Name})
        End If
    End Sub

End Class
