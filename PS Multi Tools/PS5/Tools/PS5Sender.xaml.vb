Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

Public Class PS5Sender

    Public ConsoleIP As String = String.Empty

    Dim WithEvents DefaultSenderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents SenderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}

    ReadOnly Magic As UInteger = &HEA6E
    Dim TotalBytes As Integer = 0
    Dim CurrentType As SendType
    Public SelectedISO As String

    Private Sub PS5Sender_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Not String.IsNullOrEmpty(SelectedISO) Then
            SelectedELFTextBox.Text = SelectedISO
        End If
        If Not String.IsNullOrEmpty(ConsoleIP) Then
            IPTextBox.Text = ConsoleIP
        End If

        'Check for downloaded payloads and add to DownloadedPayloadsComboBox
        If Directory.Exists(Environment.CurrentDirectory + "\Downloads") Then
            Dim PayloadList As IEnumerable(Of String) = Directory.EnumerateFiles(Environment.CurrentDirectory + "\Downloads",
                                                                                 "*.*",
                                                                                 SearchOption.AllDirectories).Where(Function(s) s.EndsWith(".elf") OrElse s.EndsWith(".bin"))
            For Each Payload In PayloadList
                Dim NewDownloadedPayloadItem As New DownloadedPayloadItem() With {.PayloadName = Path.GetFileName(Payload), .PayloadPath = Payload}
                DownloadedPayloadsComboBox.Items.Add(NewDownloadedPayloadItem)
            Next

            DownloadedPayloadsComboBox.DisplayMemberPath = "PayloadName"
            DownloadedPayloadsComboBox.Items.Refresh()
        End If
    End Sub

    Public Structure WorkerArgs
        Private _DeviceIP As IPAddress
        Private _FileToSend As String
        Private _ChunkSize As Integer
        Private _DevicePort As Integer

        Public Property DeviceIP As IPAddress
            Get
                Return _DeviceIP
            End Get
            Set
                _DeviceIP = Value
            End Set
        End Property

        Public Property DevicePort As Integer
            Get
                Return _DevicePort
            End Get
            Set
                _DevicePort = Value
            End Set
        End Property

        Public Property FileToSend As String
            Get
                Return _FileToSend
            End Get
            Set
                _FileToSend = Value
            End Set
        End Property

        Public Property ChunkSize As Integer
            Get
                Return _ChunkSize
            End Get
            Set
                _ChunkSize = Value
            End Set
        End Property

    End Structure

    Public Structure DownloadedPayloadItem

        Private _PayloadPath As String
        Private _PayloadName As String

        Public Property PayloadPath As String
            Get
                Return _PayloadPath
            End Get
            Set
                _PayloadPath = Value
            End Set
        End Property

        Public Property PayloadName As String
            Get
                Return _PayloadName
            End Get
            Set
                _PayloadName = Value
            End Set
        End Property
    End Structure

    Enum SendType
        PAYLOAD
        ISO
        CONF
    End Enum

    Private Sub SendButton_Click(sender As Object, e As RoutedEventArgs) Handles SendButton.Click
        'Check first if not both items are selected
        If Not String.IsNullOrEmpty(SelectedELFTextBox.Text) AndAlso DownloadedPayloadsComboBox.SelectedItem IsNot Nothing Then
            MsgBox("To prevent sending 2 payloads at the same time you need to mnually remove the selected payload or downloaded payload.")
        Else

            Dim SelectedPayload As String = ""
            If Not String.IsNullOrEmpty(SelectedELFTextBox.Text) Then
                'Set payload to manually selected file
                SelectedPayload = SelectedELFTextBox.Text
            ElseIf DownloadedPayloadsComboBox.SelectedItem IsNot Nothing Then
                'Set payload to selected downloaded file
                If TypeOf DownloadedPayloadsComboBox.SelectedItem Is DownloadedPayloadItem Then
                    SelectedPayload = CType(DownloadedPayloadsComboBox.SelectedItem, DownloadedPayloadItem).PayloadPath
                End If
            Else
                MsgBox("Please select a file first.", MsgBoxStyle.Exclamation, "No file selected")
                Exit Sub
            End If

            'Check if an IP address was entered
            If Not String.IsNullOrWhiteSpace(IPTextBox.Text) Then

                Dim DeviceIP As IPAddress
                Try
                    DeviceIP = IPAddress.Parse(IPTextBox.Text)
                Catch ex As FormatException
                    MsgBox("Could not send selected payload. Please check your IP.", MsgBoxStyle.Exclamation, "Error sending payload")
                    Exit Sub
                End Try

                Dim ELFFileInfo As New FileInfo(SelectedPayload)

                SendButton.IsEnabled = False
                SendISOButton.IsEnabled = False
                BrowseButton.IsEnabled = False
                BrowseISOButton.IsEnabled = False

                'Set the progress bar maximum and TotalBytes to send
                SendProgressBar.Value = 0
                SendProgressBar.Maximum = CDbl(ELFFileInfo.Length)
                TotalBytes = CInt(ELFFileInfo.Length)

                'Start sending
                CurrentType = SendType.PAYLOAD
                If Not String.IsNullOrEmpty(PortTextBox.Text) Then
                    Dim DevicePort As Integer = Integer.Parse(PortTextBox.Text)
                    DefaultSenderWorker.RunWorkerAsync(New WorkerArgs() With {.DeviceIP = DeviceIP, .FileToSend = SelectedPayload, .DevicePort = DevicePort})
                Else
                    SenderWorker.RunWorkerAsync(New WorkerArgs() With {.DeviceIP = DeviceIP, .FileToSend = SelectedPayload, .ChunkSize = 4096})
                End If

                'Reset selected combobox item
                DownloadedPayloadsComboBox.SelectedItem = Nothing
            Else
                MsgBox("No IP address was entered." + vbCrLf + "Please enter an IP address.", MsgBoxStyle.Exclamation, "No IP address")
            End If
        End If
    End Sub

    Private Sub SendISOButton_Click(sender As Object, e As RoutedEventArgs) Handles SendISOButton.Click
        'Check if a game is selected
        If Not String.IsNullOrEmpty(SelectedISOTextBox.Text) Then
            'Check if an IP address was entered before
            If Not String.IsNullOrWhiteSpace(IPTextBox.Text) Then

                If MsgBox("Send " + SelectedISOTextBox.Text + " to the console ?", MsgBoxStyle.YesNo, "Confirm") = MsgBoxResult.Yes Then

                    Dim DeviceIP As IPAddress = IPAddress.Parse(IPTextBox.Text)
                    Dim GameFileInfo As New FileInfo(SelectedISOTextBox.Text)

                    SendButton.IsEnabled = False
                    SendISOButton.IsEnabled = False
                    BrowseButton.IsEnabled = False
                    BrowseISOButton.IsEnabled = False

                    'Set the progress bar maximum and TotalBytes to send
                    SendProgressBar.Maximum = CDbl(GameFileInfo.Length)
                    TotalBytes = CInt(GameFileInfo.Length)

                    'Start sending
                    Dim WorkArgs As New WorkerArgs() With {.DeviceIP = DeviceIP, .FileToSend = SelectedISOTextBox.Text, .ChunkSize = 63488}
                    CurrentType = SendType.ISO
                    SenderWorker.RunWorkerAsync(WorkArgs)
                End If

            Else
                MsgBox("No IP address was entered." + vbCrLf + "Please enter an IP address on the main window and re-open the backup manager.", MsgBoxStyle.Exclamation, "No IP address")
            End If
        Else
            MsgBox("No game selected." + vbCrLf + "Please select a game first.", MsgBoxStyle.Exclamation, "No game selected")
        End If
    End Sub

    Private Sub SendConfigButton_Click(sender As Object, e As RoutedEventArgs) Handles SendConfigButton.Click
        'Open choose config dialog
        Dim OFD As New OpenFileDialog() With {.Title = "Select a .conf file", .Filter = "Config files (*.conf)|*.conf"}
        Dim DeviceIP As IPAddress = IPAddress.Parse(IPTextBox.Text)

        If OFD.ShowDialog() = Forms.DialogResult.OK Then

            SendButton.IsEnabled = False
            SendISOButton.IsEnabled = False
            BrowseButton.IsEnabled = False
            BrowseISOButton.IsEnabled = False

            Dim ConfigFileInfo As New FileInfo(OFD.FileName)
            Dim FilePath As String = Path.GetFullPath(OFD.FileName)

            'Set the progress bar maximum and TotalBytes to send
            SendProgressBar.Value = 0
            SendProgressBar.Maximum = CDbl(ConfigFileInfo.Length)
            TotalBytes = CInt(ConfigFileInfo.Length)

            'Start sending
            Dim WorkArgs As New WorkerArgs() With {.DeviceIP = DeviceIP, .FileToSend = FilePath, .ChunkSize = 10}
            CurrentType = SendType.CONF
            SenderWorker.RunWorkerAsync(WorkArgs)
        End If
    End Sub

    Private Sub SenderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles SenderWorker.DoWork
        Dim CurrentWorkerArgs As WorkerArgs = CType(e.Argument, WorkerArgs)

        Dim FileInfos As New FileInfo(CurrentWorkerArgs.FileToSend)
        Dim FileSizeAsLong As Long = FileInfos.Length
        Dim FileSizeAsULong As ULong = CULng(FileInfos.Length)

        Dim MagicBytes = BytesConverter.ToLittleEndian(Magic)
        Dim NewFileSizeBytes = BytesConverter.ToLittleEndian(FileSizeAsULong)

        Using SenderSocket As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) With {.ReceiveTimeout = 3000}

            SenderSocket.Connect(CurrentWorkerArgs.DeviceIP, 9045)

            SenderSocket.Send(MagicBytes)
            SenderSocket.Send(NewFileSizeBytes)

            Dim BytesRead As Integer = 0
            Dim SendBytes As Integer = 0
            Dim Buffer(CurrentWorkerArgs.ChunkSize - 1) As Byte

            'Open the file and read
            Using SenderFileStream As New FileStream(CurrentWorkerArgs.FileToSend, FileMode.Open, FileAccess.Read)

                Do
                    BytesRead = SenderFileStream.Read(Buffer, 0, Buffer.Length)

                    If BytesRead > 0 Then
                        'Send bytes
                        SendBytes += SenderSocket.Send(Buffer, 0, BytesRead, SocketFlags.None)

                        'Update the status text
                        If SendStatusTextBlock.Dispatcher.CheckAccess() = False Then
                            SendStatusTextBlock.Dispatcher.BeginInvoke(Sub() SendStatusTextBlock.Text = "Sending file: " + SendBytes.ToString + " bytes of " + TotalBytes.ToString + " bytes sent.")
                        Else
                            SendStatusTextBlock.Text = "Sending file: " + SendBytes.ToString + " of " + TotalBytes.ToString + " sent."
                        End If

                        'Update the status progress bar
                        If SendProgressBar.Dispatcher.CheckAccess() = False Then
                            SendProgressBar.Dispatcher.BeginInvoke(Sub() SendProgressBar.Value = SendBytes)
                        Else
                            SendProgressBar.Value = SendBytes
                        End If

                    End If
                Loop While BytesRead > 0

            End Using

            'Close the connection
            SenderSocket.Close()
        End Using
    End Sub

    Private Sub SenderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles SenderWorker.RunWorkerCompleted
        SendStatusTextBlock.Text = "Status:"
        SendProgressBar.Value = 0

        If Not e.Cancelled Then
            Select Case CurrentType
                Case SendType.PAYLOAD
                    SendConfigButton.IsEnabled = True
                    SendButton.IsEnabled = True
                    SendISOButton.IsEnabled = True
                    BrowseButton.IsEnabled = True
                    BrowseISOButton.IsEnabled = True
                    MsgBox("Payload successfully sent!", MsgBoxStyle.Information, "Success")
                Case SendType.ISO
                    SendConfigButton.IsEnabled = True
                    SendButton.IsEnabled = True
                    SendISOButton.IsEnabled = True
                    BrowseButton.IsEnabled = True
                    BrowseISOButton.IsEnabled = True
                    MsgBox("Game successfully sent!" + vbCrLf + "You can now send a config file if you want to.", MsgBoxStyle.Information, "Success")
                Case SendType.CONF
                    SendConfigButton.IsEnabled = False
                    SendButton.IsEnabled = True
                    SendISOButton.IsEnabled = True
                    BrowseButton.IsEnabled = True
                    BrowseISOButton.IsEnabled = True
                    MsgBox("Config successfully sent!", MsgBoxStyle.Information, "Success")
            End Select
        End If
    End Sub

    Private Sub BrowseButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select an .elf or .bin file", .Filter = "ELF files (*.elf)|*.elf|BIN files (*.bin)|*.bin"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedELFTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseISOButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseISOButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select an .iso file", .Filter = "ELF files (*.iso)|*.iso"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedISOTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub DefaultSenderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles DefaultSenderWorker.DoWork
        Dim CurrentWorkerArgs As WorkerArgs = CType(e.Argument, WorkerArgs)

        Using SenderSocket As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) With {.ReceiveTimeout = 3000}
            'Connect
            SenderSocket.Connect(CurrentWorkerArgs.DeviceIP, CurrentWorkerArgs.DevicePort)
            'Send ELF
            SenderSocket.SendFile(CurrentWorkerArgs.FileToSend)
            'Close the connection
            SenderSocket.Close()
        End Using
    End Sub

    Private Sub DefaultSenderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles DefaultSenderWorker.RunWorkerCompleted
        SendConfigButton.IsEnabled = True
        SendButton.IsEnabled = True
        SendISOButton.IsEnabled = True
        BrowseButton.IsEnabled = True
        BrowseISOButton.IsEnabled = True

        MsgBox("Successfully sent!", MsgBoxStyle.Information, "Success")
    End Sub

    Private Sub PortTextBox_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles PortTextBox.PreviewTextInput
        Dim NumbersOnly As New Regex("[^0-9]+")
        e.Handled = NumbersOnly.IsMatch(e.Text)
    End Sub

    Private Sub IPTextBox_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles IPTextBox.PreviewTextInput
        Dim OnlyNumbersAndDot As New Regex("[^0-9.]+")
        e.Handled = OnlyNumbersAndDot.IsMatch(e.Text)
    End Sub

End Class
