Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Text
Imports System.Windows.Forms
Imports Microsoft.Win32

Public Class PPPwner

    Dim WithEvents PPPwnWoker As New BackgroundWorker()
    Dim WithEvents PPPwn As New Process()

    Private Structure ComboBoxEthernetDevice
        Private _AdapterDescription As String
        Private _AdapterID As String
        Private _AdapterName As String
        Private _DisplayValue As String

        Public Property AdapterDescription As String
            Get
                Return _AdapterDescription
            End Get
            Set
                _AdapterDescription = Value
            End Set
        End Property

        Public Property AdapterID As String
            Get
                Return _AdapterID
            End Get
            Set
                _AdapterID = Value
            End Set
        End Property

        Public Property AdapterName As String
            Get
                Return _AdapterName
            End Get
            Set
                _AdapterName = Value
            End Set
        End Property

        Public Property DisplayValue As String
            Get
                Return _DisplayValue
            End Get
            Set
                _DisplayValue = Value
            End Set
        End Property
    End Structure

    Private Sub PPPwner_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        'List Ethernet Adapters
        Dim NewListOfEthernetAdapter As New List(Of ComboBoxEthernetDevice)()
        For Each AvailableNetworkInterface As NetworkInterface In NetworkInterface.GetAllNetworkInterfaces()
            Select Case AvailableNetworkInterface.NetworkInterfaceType
                'Show only Ethernet Interfaces
                Case NetworkInterfaceType.Ethernet, NetworkInterfaceType.FastEthernetT, NetworkInterfaceType.GigabitEthernet
                    Dim NewComboBoxEhternetDevice As New ComboBoxEthernetDevice() With {.AdapterDescription = AvailableNetworkInterface.Description, .AdapterID = AvailableNetworkInterface.Id, .AdapterName = AvailableNetworkInterface.Name,
                    .DisplayValue = "Name: " + AvailableNetworkInterface.Name + vbCrLf + "Description: " + AvailableNetworkInterface.Description + vbCrLf + "ID: " + AvailableNetworkInterface.Id}
                    NewListOfEthernetAdapter.Add(NewComboBoxEhternetDevice)
            End Select
        Next

        'Set EthernetInterfacesComboBox properties
        EthernetInterfacesComboBox.ItemsSource = NewListOfEthernetAdapter
        EthernetInterfacesComboBox.DisplayMemberPath = "DisplayValue"

        'Check if Npcap is installed
        If Registry.LocalMachine.OpenSubKey("SOFTWARE\Npcap", False) Is Nothing AndAlso Registry.LocalMachine.OpenSubKey("SOFTWARE\WOW6432Node\Npcap", False) Is Nothing Then
            If MsgBox("Npcap is not installed." + vbCrLf + "Do you want to install it now ? (Required)", MsgBoxStyle.YesNo, "Npcap Required") = MsgBoxResult.Yes Then
                If File.Exists(Environment.CurrentDirectory + "\Tools\npcap-1.79.exe") Then
                    Process.Start(Environment.CurrentDirectory + "\Tools\npcap-1.79.exe")
                Else
                    MsgBox(Environment.CurrentDirectory + "\Tools\npcap-1.79.exe NOT FOUND, please install manually from : https://npcap.com/#download", MsgBoxStyle.Critical, "Error")
                End If
            End If
        End If
    End Sub

    Private Sub StartPPPwnButton_Click(sender As Object, e As RoutedEventArgs) Handles StartPPPwnButton.Click
        If StartPPPwnButton.Content.ToString() = "Stop PPPwn" Then
            If PPPwn.HasExited = False Then
                'Stop PPPwn
                PPPwn.Kill()

                'Update Button
                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub()
                                               StartPPPwnButton.IsEnabled = False
                                               StartPPPwnButton.Content = "Start PPPWn"
                                           End Sub)
                Else
                    StartPPPwnButton.IsEnabled = False
                    StartPPPwnButton.Content = "Start PPPWn"
                End If
            End If
        Else
            If EthernetInterfacesComboBox.SelectedItem IsNot Nothing AndAlso FirmwaresComboBox.SelectedItem IsNot Nothing Then
                If File.Exists(Environment.CurrentDirectory + "\Tools\pppwn.exe") Then
                    'Get selected Ethernet interface
                    Dim SelectedEthernetInterfaceInComboBox As ComboBoxEthernetDevice = CType(EthernetInterfacesComboBox.SelectedItem, ComboBoxEthernetDevice)
                    Dim SelectedEthernetInterface As String = "\Device\NPF_" + SelectedEthernetInterfaceInComboBox.AdapterID

                    'Set firmware
                    Dim SelectedFirmware As String = ""
                    Select Case FirmwaresComboBox.Text
                        Case "7.50 / 7.51 / 7.55"
                            SelectedFirmware = "750"
                        Case "8.00 / 8.01 / 8.03"
                            SelectedFirmware = "800"
                        Case "8.50 / 8.52"
                            SelectedFirmware = "850"
                        Case "9.00"
                            SelectedFirmware = "900"
                        Case "9.03 / 9.04"
                            SelectedFirmware = "903"
                        Case "9.50 / 9.51 / 9.60"
                            SelectedFirmware = "950"
                        Case "10.00 / 10.01"
                            SelectedFirmware = "1000"
                        Case "10.50 / 10.70 / 10.71"
                            SelectedFirmware = "1050"
                        Case "11.00"
                            SelectedFirmware = "1100"
                    End Select

                    'Set the files for stage1 & stage2
                    Dim Stage1File As String = ""
                    Dim Stage2File As String = ""
                    If UseCustomStageFilesCheckBox.IsChecked Then
                        Stage1File = CustomStage1PayloadTextBox.Text
                        Stage2File = CustomStage2PayloadTextBox.Text
                    Else
                        Select Case SelectedFirmware
                            Case "750"
                                Stage1File = Environment.CurrentDirectory + "\Tools\PS4\stage1\ToF-stage1-750.bin"
                                Stage2File = Environment.CurrentDirectory + "\Tools\PS4\stage2\ToF-stage2-750.bin"
                            Case "800"
                                Stage1File = Environment.CurrentDirectory + "\Tools\PS4\stage1\ToF-stage1-800.bin"
                                Stage2File = Environment.CurrentDirectory + "\Tools\PS4\stage2\ToF-stage2-800.bin"
                            Case "850"
                                Stage1File = Environment.CurrentDirectory + "\Tools\PS4\stage1\ToF-stage1-850.bin"
                                Stage2File = Environment.CurrentDirectory + "\Tools\PS4\stage2\ToF-stage2-850.bin"
                            Case "900"
                                Stage1File = Environment.CurrentDirectory + "\Tools\PS4\stage1\SiS-stage1-900.bin"
                                Stage2File = Environment.CurrentDirectory + "\Tools\PS4\stage2\SiS-stage2-900.bin"
                            Case "903"
                                Stage1File = Environment.CurrentDirectory + "\Tools\PS4\stage1\ToF-stage1-903.bin"
                                Stage2File = Environment.CurrentDirectory + "\Tools\PS4\stage2\ToF-stage2-903.bin"
                            Case "950"
                                Stage1File = Environment.CurrentDirectory + "\Tools\PS4\stage1\SiS-stage1-950.bin"
                                Stage2File = Environment.CurrentDirectory + "\Tools\PS4\stage2\SiS-stage2-950.bin"
                            Case "1000"
                                Stage1File = Environment.CurrentDirectory + "\Tools\PS4\stage1\SiS-stage1-1000.bin"
                                Stage2File = Environment.CurrentDirectory + "\Tools\PS4\stage2\SiS-stage2-1000.bin"
                            Case "1050"
                                Stage1File = Environment.CurrentDirectory + "\Tools\PS4\stage1\ToF-stage1-1050.bin"
                                Stage2File = Environment.CurrentDirectory + "\Tools\PS4\stage2\ToF-stage2-1050.bin"
                            Case "1100"
                                Stage1File = Environment.CurrentDirectory + "\Tools\PS4\stage1\SiS-stage1-1100.bin"
                                Stage2File = Environment.CurrentDirectory + "\Tools\PS4\stage2\SiS-stage2-1100.bin"
                        End Select
                    End If

                    'Build the arguments string
                    Dim NewStringBuilder As New StringBuilder()
                    NewStringBuilder.Append("--interface """ + SelectedEthernetInterface + """ --fw " + SelectedFirmware + " --stage1 """ + Stage1File + """ --stage2 """ + Stage2File + """")

                    If UseResponseTimeoutCheckBox.IsChecked Then
                        If Not String.IsNullOrEmpty(ResponseTimeoutValueTextBox.Text) AndAlso Utils.IsInt(ResponseTimeoutValueTextBox.Text) Then
                            NewStringBuilder.Append(" --timeout " + ResponseTimeoutValueTextBox.Text)
                        End If
                    End If
                    If UsePinningWaitingTimeCheckBox.IsChecked Then
                        If Not String.IsNullOrEmpty(PinningWaitingTimeValueTextBox.Text) AndAlso Utils.IsInt(PinningWaitingTimeValueTextBox.Text) Then
                            NewStringBuilder.Append(" --wait-after-pin " + PinningWaitingTimeValueTextBox.Text)
                        End If
                    End If
                    If UseGroomDelayCheckBox.IsChecked Then
                        If Not String.IsNullOrEmpty(GroomDelayValueTextBox.Text) AndAlso Utils.IsInt(GroomDelayValueTextBox.Text) Then
                            NewStringBuilder.Append(" --groom-delay " + GroomDelayValueTextBox.Text)
                        End If

                    End If
                    If SpecifyPCAPBufferSizeCheckBox.IsChecked Then
                        If Not String.IsNullOrEmpty(PCAPBufferSizeValueTextBox.Text) AndAlso Utils.IsInt(PCAPBufferSizeValueTextBox.Text) Then
                            NewStringBuilder.Append(" --buffer-size " + PCAPBufferSizeValueTextBox.Text)
                        End If
                    End If

                    If AutoRetryCheckBox.IsChecked Then
                        NewStringBuilder.Append(" --auto-retry")
                    End If
                    If DontWaitPADICheckBox.IsChecked Then
                        NewStringBuilder.Append(" --no-wait-padi")
                    End If
                    If UseCPUCheckBox.IsChecked Then
                        NewStringBuilder.Append(" --real-sleep")
                    End If

                    'Run PPPwn
                    PPPwnWoker.RunWorkerAsync(NewStringBuilder.ToString())

                    'Update button
                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub()
                                                   EthernetInterfacesComboBox.IsEnabled = False
                                                   FirmwaresComboBox.IsEnabled = False
                                                   AutoRetryCheckBox.IsEnabled = False
                                                   UseResponseTimeoutCheckBox.IsEnabled = False
                                                   UsePinningWaitingTimeCheckBox.IsEnabled = False
                                                   UseGroomDelayCheckBox.IsEnabled = False
                                                   SpecifyPCAPBufferSizeCheckBox.IsEnabled = False
                                                   DontWaitPADICheckBox.IsEnabled = False
                                                   UseCPUCheckBox.IsEnabled = False
                                                   ResponseTimeoutValueTextBox.IsEnabled = False
                                                   PinningWaitingTimeValueTextBox.IsEnabled = False
                                                   GroomDelayValueTextBox.IsEnabled = False
                                                   PCAPBufferSizeValueTextBox.IsEnabled = False

                                                   StartPPPwnButton.Content = "Stop PPPWn"
                                               End Sub)
                    Else
                        EthernetInterfacesComboBox.IsEnabled = False
                        FirmwaresComboBox.IsEnabled = False
                        AutoRetryCheckBox.IsEnabled = False
                        UseResponseTimeoutCheckBox.IsEnabled = False
                        UsePinningWaitingTimeCheckBox.IsEnabled = False
                        UseGroomDelayCheckBox.IsEnabled = False
                        SpecifyPCAPBufferSizeCheckBox.IsEnabled = False
                        DontWaitPADICheckBox.IsEnabled = False
                        UseCPUCheckBox.IsEnabled = False
                        ResponseTimeoutValueTextBox.IsEnabled = False
                        PinningWaitingTimeValueTextBox.IsEnabled = False
                        GroomDelayValueTextBox.IsEnabled = False
                        PCAPBufferSizeValueTextBox.IsEnabled = False

                        StartPPPwnButton.Content = "Stop PPPWn"
                    End If
                Else
                    MsgBox("Could not find PPPwn at " + Environment.CurrentDirectory + "\Tools\pppwn.exe", MsgBoxStyle.Critical, "Error")
                End If
            Else
                MsgBox("Please select your Ethernet interface, PS4 firmware and Payload.", MsgBoxStyle.Critical, "Error")
            End If
        End If
    End Sub

    Private Sub PPPwn_Exited(sender As Object, e As EventArgs) Handles PPPwn.Exited
        PPPwn.Dispose()

        'Update button on exit
        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub()
                                       EthernetInterfacesComboBox.IsEnabled = True
                                       FirmwaresComboBox.IsEnabled = True
                                       AutoRetryCheckBox.IsEnabled = True
                                       UseResponseTimeoutCheckBox.IsEnabled = True
                                       UsePinningWaitingTimeCheckBox.IsEnabled = True
                                       UseGroomDelayCheckBox.IsEnabled = True
                                       SpecifyPCAPBufferSizeCheckBox.IsEnabled = True
                                       DontWaitPADICheckBox.IsEnabled = True
                                       UseCPUCheckBox.IsEnabled = True
                                       ResponseTimeoutValueTextBox.IsEnabled = True
                                       PinningWaitingTimeValueTextBox.IsEnabled = True
                                       GroomDelayValueTextBox.IsEnabled = True
                                       PCAPBufferSizeValueTextBox.IsEnabled = True

                                       StartPPPwnButton.Content = "Start PPPWn"
                                   End Sub)
        Else
            EthernetInterfacesComboBox.IsEnabled = True
            FirmwaresComboBox.IsEnabled = True
            AutoRetryCheckBox.IsEnabled = True
            UseResponseTimeoutCheckBox.IsEnabled = True
            UsePinningWaitingTimeCheckBox.IsEnabled = True
            UseGroomDelayCheckBox.IsEnabled = True
            SpecifyPCAPBufferSizeCheckBox.IsEnabled = True
            DontWaitPADICheckBox.IsEnabled = True
            UseCPUCheckBox.IsEnabled = True
            ResponseTimeoutValueTextBox.IsEnabled = True
            PinningWaitingTimeValueTextBox.IsEnabled = True
            GroomDelayValueTextBox.IsEnabled = True
            PCAPBufferSizeValueTextBox.IsEnabled = True

            StartPPPwnButton.Content = "Start PPPWn"
        End If
    End Sub

    Private Sub BrowseStage1PayloadButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseStage1PayloadButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Select a stage1 payload", .Filter = "BIN files (*.bin)|*.bin"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            CustomStage1PayloadTextBox.Text = OFD.FileName
        Else
            MsgBox("No file selected.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub BrowseStage2PayloadButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseStage2PayloadButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Select a stage2 payload", .Filter = "BIN files (*.bin)|*.bin"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            CustomStage2PayloadTextBox.Text = OFD.FileName
        Else
            MsgBox("No file selected.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub PPPwnWoker_DoWork(sender As Object, e As DoWorkEventArgs) Handles PPPwnWoker.DoWork
        'Set PPPwn process properties
        PPPwn = New Process()
        PPPwn.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\pppwn.exe"
        PPPwn.StartInfo.Arguments = e.Argument.ToString()
        PPPwn.StartInfo.RedirectStandardOutput = True
        PPPwn.StartInfo.RedirectStandardError = True
        PPPwn.StartInfo.UseShellExecute = False
        PPPwn.StartInfo.CreateNoWindow = True
        PPPwn.EnableRaisingEvents = True

        AddHandler PPPwn.OutputDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                 If Not String.IsNullOrEmpty(DataArgs.Data) Then
                                                     'Append output log from PPPWn
                                                     If Dispatcher.CheckAccess() = False Then
                                                         Dispatcher.BeginInvoke(Sub()
                                                                                    LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                                    LogTextBox.ScrollToEnd()
                                                                                End Sub)
                                                     Else
                                                         LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                         LogTextBox.ScrollToEnd()
                                                     End If
                                                 End If
                                             End Sub

        AddHandler PPPwn.ErrorDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                If Not String.IsNullOrEmpty(DataArgs.Data) Then
                                                    'Append error log from PPPWn
                                                    If Dispatcher.CheckAccess() = False Then
                                                        Dispatcher.BeginInvoke(Sub()
                                                                                   LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                                   LogTextBox.ScrollToEnd()
                                                                               End Sub)
                                                    Else
                                                        LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                        LogTextBox.ScrollToEnd()
                                                    End If
                                                End If
                                            End Sub

        'Start PPPwn & read process output data
        PPPwn.Start()
        PPPwn.BeginOutputReadLine()
        PPPwn.BeginErrorReadLine()
    End Sub

    Private Sub CopyGoldHENButton_Click(sender As Object, e As RoutedEventArgs) Handles CopyGoldHENButton.Click
        Dim FBD As New FolderBrowserDialog() With {.Description = "Select an USB drive"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            If File.Exists(Environment.CurrentDirectory + "\Tools\PS4\goldhen.bin") Then
                Try
                    File.Copy(Environment.CurrentDirectory + "\Tools\PS4\goldhen.bin", FBD.SelectedPath + "goldhen.bin", True)
                    MsgBox("Copy done!", MsgBoxStyle.Information)
                Catch ex As Exception
                    MsgBox("Could not copy GoldHEN to selected USB drive.", MsgBoxStyle.Exclamation, "Error")
                End Try
            Else
                MsgBox("Could not find goldhen.bin at " + Environment.CurrentDirectory + "\Tools\PS4\goldhen.bin", MsgBoxStyle.Exclamation, "Error")
            End If
        Else
            MsgBox("No USB drive selected.", MsgBoxStyle.Exclamation, "Error")
        End If
    End Sub

    Private Async Sub DownloadGoldHENButton_Click(sender As Object, e As RoutedEventArgs) Handles DownloadGoldHENButton.Click
        Dim FBD As New FolderBrowserDialog() With {.Description = "Select an USB drive"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            Try
                If Await Utils.IsURLValid("http://X.X.X.X/ps4/ex/goldhen_v2.4b18.3.bin") Then
                    Dim DownloadClient As New WebClient()
                    DownloadClient.DownloadFile(New Uri("http://X.X.X.X/ps4/ex/goldhen_v2.4b18.3.bin"), FBD.SelectedPath + "goldhen.bin")
                    MsgBox("GoldHEN downloaded to : " + FBD.SelectedPath + "goldhen.bin", MsgBoxStyle.Information, "Success")
                Else
                    MsgBox("Could not download GoldHEN to : " + FBD.SelectedPath + "goldhen.bin" + vbCrLf + "File is not available.", MsgBoxStyle.Information, "Error")
                End If
            Catch ex As Exception
                MsgBox("Could not copy GoldHEN to selected USB drive.", MsgBoxStyle.Exclamation, "Error")
            End Try
        End If
    End Sub

End Class
