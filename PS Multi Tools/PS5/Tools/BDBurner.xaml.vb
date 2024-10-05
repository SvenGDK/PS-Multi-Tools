Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.InteropServices
Imports IMAPI2

Public Class BDBurner

    Dim WithEvents DiscFormatData As New MsftDiscFormat2Data()
    Private WithEvents BurnWorker As New BackgroundWorker() With {.WorkerReportsProgress = True, .WorkerSupportsCancellation = True}

    Dim SelectedFile As String = ""
    Private BurnStream As IStream = Nothing
    Private CurrentBurnData As New BURNDATA()

    Private Const STGM_SHARE_DENY_WRITE As UInteger = &H20
    Private Const STGM_SHARE_DENY_NONE As UInteger = &H40
    Private Const STGM_READ As UInteger = &H0
    Private Const STGM_WRITE As UInteger = &H1
    Private Const STGM_READWRITE As UInteger = &H2

    <DllImport("shlwapi.dll", CharSet:=CharSet.Unicode, ExactSpelling:=True, PreserveSig:=False, EntryPoint:="SHCreateStreamOnFileW")>
    Private Shared Sub SHCreateStreamOnFile(fileName As String, mode As UInteger, ByRef stream As IStream)
    End Sub

    Private Function GetComStreamForFile(fileName As String) As IStream
        Dim stream As IStream = Nothing
        SHCreateStreamOnFile(fileName, STGM_SHARE_DENY_WRITE, stream)
        Return stream
    End Function

    Public Structure BURNDATA

        Private _ElapsedTime As Integer
        Private _RemainingTime As Integer
        Private _TotalTime As Integer
        Private _CurrentAction As IMAPI_FORMAT2_DATA_WRITE_ACTION
        Private _StartLba As Integer
        Private _FreeSystemBuffer As Integer
        Private _TotalSystemBuffer As Integer
        Private _LastReadLba As Integer
        Private _SectorCount As Integer
        Private _LastWrittenLba As Integer
        Private _UsedSystemBuffer As Integer
        Private _StatusMessage As String

        Public Property StatusMessage As String
            Get
                Return _StatusMessage
            End Get
            Set
                _StatusMessage = Value
            End Set
        End Property

        Public Property ElapsedTime As Integer
            Get
                Return _ElapsedTime
            End Get
            Set
                _ElapsedTime = Value
            End Set
        End Property

        Public Property RemainingTime As Integer
            Get
                Return _RemainingTime
            End Get
            Set
                _RemainingTime = Value
            End Set
        End Property

        Public Property TotalTime As Integer
            Get
                Return _TotalTime
            End Get
            Set
                _TotalTime = Value
            End Set
        End Property

        Public Property CurrentAction As IMAPI_FORMAT2_DATA_WRITE_ACTION
            Get
                Return _CurrentAction
            End Get
            Set
                _CurrentAction = Value
            End Set
        End Property

        Public Property StartLba As Integer
            Get
                Return _StartLba
            End Get
            Set
                _StartLba = Value
            End Set
        End Property

        Public Property SectorCount As Integer
            Get
                Return _SectorCount
            End Get
            Set
                _SectorCount = Value
            End Set
        End Property

        Public Property LastReadLba As Integer
            Get
                Return _LastReadLba
            End Get
            Set
                _LastReadLba = Value
            End Set
        End Property

        Public Property LastWrittenLba As Integer
            Get
                Return _LastWrittenLba
            End Get
            Set
                _LastWrittenLba = Value
            End Set
        End Property

        Public Property TotalSystemBuffer As Integer
            Get
                Return _TotalSystemBuffer
            End Get
            Set
                _TotalSystemBuffer = Value
            End Set
        End Property

        Public Property UsedSystemBuffer As Integer
            Get
                Return _UsedSystemBuffer
            End Get
            Set
                _UsedSystemBuffer = Value
            End Set
        End Property

        Public Property FreeSystemBuffer As Integer
            Get
                Return _FreeSystemBuffer
            End Get
            Set
                _FreeSystemBuffer = Value
            End Set
        End Property

    End Structure

    Private Sub BDBurner_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        For Each DiscDrive As DriveInfo In DriveInfo.GetDrives()
            If DiscDrive.DriveType = DriveType.CDRom Then
                DiscDrivesComboBox.Items.Add(DiscDrive.Name)
            End If
        Next
    End Sub

    Private Sub BurnDiscButton_Click(sender As Object, e As RoutedEventArgs) Handles BurnDiscButton.Click
        If DiscDrivesComboBox.SelectedIndex = -1 Then
            MsgBox("No disc drive specified.", MsgBoxStyle.Critical, "Error reading disc")
            Return
        End If

        If String.IsNullOrEmpty(SelectedISOTextBox.Text) Then
            MsgBox("No ISO file specified.", MsgBoxStyle.Critical, "No file to burn")
            Return
        Else
            'Set cursor & status
            Cursor = Input.Cursors.Wait
            BurnStatusTextBlock.Text = "Buning disc, please wait ..."
            BurnProgressBar.IsIndeterminate = True

            BurnWorker.RunWorkerAsync()
        End If
    End Sub

    Private Sub CheckDiscButton_Click(sender As Object, e As RoutedEventArgs) Handles CheckDiscButton.Click
        If DiscDrivesComboBox.SelectedIndex = -1 Then
            MsgBox("No disc drive specified.", MsgBoxStyle.Critical, "Error reading disc")
            Return
        End If

        Dim DiscMaster As New MsftDiscMaster2()
        Dim DiscMasterID = DiscMaster.Item(0) 'First drive
        Dim DiscRecorder As New MsftDiscRecorder2()

        DiscRecorder.InitializeDiscRecorder(DiscMasterID)

        Dim FileSystemImage As IMAPI2FS.MsftFileSystemImage = Nothing
        Dim DiscFormatData As MsftDiscFormat2Data = Nothing

        Dim DiscMediaTypeString As String = ""
        Dim DiscSize As Long = 0

        Try
            DiscFormatData = New MsftDiscFormat2Data()

            If Not DiscFormatData.IsCurrentMediaSupported(DiscRecorder) Then
                MsgBox("No disc inserted or not supported !", MsgBoxStyle.Exclamation)
                Return
            Else
                DiscFormatData.Recorder = DiscRecorder

                Dim DiscMediaType As IMAPI_MEDIA_PHYSICAL_TYPE = DiscFormatData.CurrentPhysicalMediaType
                DiscMediaTypeString = GetMediaTypeString(DiscMediaType)

                FileSystemImage = New IMAPI2FS.MsftFileSystemImage()
                FileSystemImage.ChooseImageDefaultsForMediaType(CType(DiscMediaType, IMAPI2FS.IMAPI_MEDIA_PHYSICAL_TYPE))

                If Not DiscFormatData.MediaHeuristicallyBlank Then
                    FileSystemImage.MultisessionInterfaces = DiscFormatData.MultisessionInterfaces
                    FileSystemImage.ImportFileSystem()
                End If

                Dim DiscFreeMediaBlocks As Long = FileSystemImage.FreeMediaBlocks
                DiscSize = 2048 * DiscFreeMediaBlocks
            End If

            If Not String.IsNullOrEmpty(DiscMediaTypeString) Then
                DiscInfoTextBlock.Text = "Disc Type: " + DiscMediaTypeString + " - Size: " + FormatNumber(DiscSize / 1073741824, 2) + " GB"
            End If

        Catch ex As COMException
            MsgBox(ex.Message, MsgBoxStyle.Exclamation)
        Finally
            'Release
            If DiscFormatData IsNot Nothing Then
                Marshal.ReleaseComObject(DiscFormatData)
            End If
            If fileSystemImage IsNot Nothing Then
                Marshal.ReleaseComObject(fileSystemImage)
            End If
        End Try
    End Sub

    Private Function GetMediaTypeString(mediaType As IMAPI_MEDIA_PHYSICAL_TYPE) As String
        Select Case mediaType
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDROM
                Return ("CD-ROM")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDR
                Return ("CD-R")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDRW
                Return ("CD-RW")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDROM
                Return ("DVD ROM")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDRAM
                Return ("DVD-RAM")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR
                Return ("DVD+R")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW
                Return ("DVD+RW")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR_DUALLAYER
                Return ("DVD+R Dual Layer")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR
                Return ("DVD-R")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHRW
                Return ("DVD-RW")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR_DUALLAYER
                Return ("DVD-R Dual Layer")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DISK
                Return ("random-access writes")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW_DUALLAYER
                Return ("DVD+RW DL")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDROM
                Return ("HD DVD-ROM")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDR
                Return ("HD DVD-R")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDRAM
                Return ("HD DVD-RAM")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDROM
                Return ("Blu-ray DVD (BD-ROM)")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDR
                Return ("Blu-ray media")
            Case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDRE
                Return ("Blu-ray Rewritable media")
            Case Else
                Return ("Unknown Media Type")
        End Select
    End Function

    Private Sub BurnWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles BurnWorker.DoWork
        Dim DiscMaster As New MsftDiscMaster2()
        Dim DiscMasterID = DiscMaster.Item(0) 'First drive
        Dim DiscRecorder As New MsftDiscRecorder2()

        'Initialize the drive and acquire exclusive access
        DiscRecorder.InitializeDiscRecorder(DiscMasterID)
        DiscRecorder.AcquireExclusiveAccess(True, "PSMT")

        DiscFormatData = New MsftDiscFormat2Data With {.Recorder = DiscRecorder, .ClientName = "PSMT"}

        'Create the IStream from the ISO file
        BurnStream = GetComStreamForFile(SelectedFile)

        Try

            'Burn the disc
            DiscFormatData.Write(BurnStream)

        Catch ex As COMException
            MsgBox(ex.ToString)
        End Try

        'Eject the disc and release the exclusive access
        DiscRecorder.EjectMedia()
        DiscRecorder.ReleaseExclusiveAccess()
    End Sub

    'Progress update is currently not working
    Private Sub DiscFormatData_Update(sender As Object, progress As Object) Handles DiscFormatData.Update
        If BurnWorker.CancellationPending Then
            Dim Format2Data As IDiscFormat2Data = CType(sender, IDiscFormat2Data)
            Format2Data.CancelWrite()
            Return
        End If

        Dim BurnEventArgs As IDiscFormat2DataEventArgs = CType(progress, IDiscFormat2DataEventArgs)
        CurrentBurnData.ElapsedTime = BurnEventArgs.ElapsedTime
        CurrentBurnData.RemainingTime = BurnEventArgs.RemainingTime
        CurrentBurnData.TotalTime = BurnEventArgs.TotalTime
        CurrentBurnData.CurrentAction = BurnEventArgs.CurrentAction
        CurrentBurnData.StartLba = BurnEventArgs.StartLba
        CurrentBurnData.SectorCount = BurnEventArgs.SectorCount
        CurrentBurnData.LastReadLba = BurnEventArgs.LastReadLba
        CurrentBurnData.LastWrittenLba = BurnEventArgs.LastWrittenLba
        CurrentBurnData.TotalSystemBuffer = BurnEventArgs.TotalSystemBuffer
        CurrentBurnData.UsedSystemBuffer = BurnEventArgs.UsedSystemBuffer
        CurrentBurnData.FreeSystemBuffer = BurnEventArgs.FreeSystemBuffer

        BurnWorker.ReportProgress(0, CurrentBurnData)
    End Sub

    Private Sub BurnWorker_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles BurnWorker.ProgressChanged
        Dim WorkerBurnData As BURNDATA = CType(e.UserState, BURNDATA)

        Select Case WorkerBurnData.CurrentAction
            Case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_VALIDATING_MEDIA
                BurnStatusTextBlock.Text = "Validating current media..."
            Case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_FORMATTING_MEDIA
                BurnStatusTextBlock.Text = "Formatting media..."
            Case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_INITIALIZING_HARDWARE
                BurnStatusTextBlock.Text = "Initializing hardware..."
            Case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_CALIBRATING_POWER
                BurnStatusTextBlock.Text = "Optimizing laser intensity..."
            Case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_WRITING_DATA
                Dim writtenSectors As Long = WorkerBurnData.LastWrittenLba - WorkerBurnData.StartLba

                If writtenSectors > 0 AndAlso WorkerBurnData.SectorCount > 0 Then
                    Dim percent As Integer = CInt(((100 * writtenSectors) / WorkerBurnData.SectorCount))
                    BurnStatusTextBlock.Text = String.Format("Progress: {0}%", percent)
                    BurnProgressBar.Value = percent
                Else
                    BurnStatusTextBlock.Text = "Progress 0%"
                    BurnProgressBar.Value = 0
                End If

            Case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_FINALIZATION
                BurnStatusTextBlock.Text = "Finalizing writing..."
            Case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_COMPLETED
                BurnStatusTextBlock.Text = "Completed!"
        End Select

    End Sub

    Private Sub BurnWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BurnWorker.RunWorkerCompleted
        If CInt(e.Result) = 0 Then

            'Set status
            Cursor = Cursors.Arrow
            BurnStatusTextBlock.Text = "Finished !"
            BurnProgressBar.IsIndeterminate = False

            MsgBox("Finished Burning Disc!", MsgBoxStyle.Information)
        Else
            MsgBox("Error Burning Disc!", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub BrowseISOButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseISOButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.CheckFileExists = True, .Filter = "ISO files (*.iso)|*.iso", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedISOTextBox.Text = OFD.FileName
            SelectedFile = OFD.FileName
        End If
    End Sub

End Class
