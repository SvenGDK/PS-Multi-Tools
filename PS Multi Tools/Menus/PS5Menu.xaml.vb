Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Security.Authentication
Imports System.Threading
Imports FluentFTP
Imports Microsoft.Web.WebView2.Core
Imports Newtonsoft.Json
Imports PS_Multi_Tools.INI

Public Class PS5Menu

    Dim MainConfig As New IniFile(Environment.CurrentDirectory + "\psmt-config.ini")

    Public SharedIPAddress As String = ""
    Public SharedFTPPort As String = ""
    Public SharedPayloadPort As String = ""

    Private IswebMANWebSrvWebViewReady As Boolean = False
    Private IswebMANWebSrvCommandExecuted As Boolean = False

    Public Shared ReadOnly IPChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(name:="ConsoleIPChanged", routingStrategy:=RoutingStrategy.Bubble, handlerType:=GetType(RoutedEventHandler), ownerType:=GetType(PS5Menu))
    Public Shared ReadOnly FTPPortChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(name:="ConsoleFTPPortChanged", routingStrategy:=RoutingStrategy.Bubble, handlerType:=GetType(RoutedEventHandler), ownerType:=GetType(PS5Menu))
    Public Shared ReadOnly PayloadPortChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(name:="ConsolePayloadPortChanged", routingStrategy:=RoutingStrategy.Bubble, handlerType:=GetType(RoutedEventHandler), ownerType:=GetType(PS5Menu))

    Public Custom Event IPTextChanged As RoutedEventHandler
        AddHandler(value As RoutedEventHandler)
            [AddHandler](IPChangedEvent, value)
        End AddHandler

        RemoveHandler(value As RoutedEventHandler)
            [RemoveHandler](IPChangedEvent, value)
        End RemoveHandler

        RaiseEvent(sender As Object, e As RoutedEventArgs)
            [RaiseEvent](e)
        End RaiseEvent
    End Event

    Public Custom Event FTPPortChanged As RoutedEventHandler
        AddHandler(value As RoutedEventHandler)
            [AddHandler](FTPPortChangedEvent, value)
        End AddHandler

        RemoveHandler(value As RoutedEventHandler)
            [RemoveHandler](FTPPortChangedEvent, value)
        End RemoveHandler

        RaiseEvent(sender As Object, e As RoutedEventArgs)
            [RaiseEvent](e)
        End RaiseEvent
    End Event

    Public Custom Event PayloadPortChanged As RoutedEventHandler
        AddHandler(value As RoutedEventHandler)
            [AddHandler](PayloadPortChangedEvent, value)
        End AddHandler

        RemoveHandler(value As RoutedEventHandler)
            [RemoveHandler](PayloadPortChangedEvent, value)
        End RemoveHandler

        RaiseEvent(sender As Object, e As RoutedEventArgs)
            [RaiseEvent](e)
        End RaiseEvent
    End Event

    Private Sub RaiseIPTextChangedRoutedEvent()
        Dim routedEventArgs As New RoutedEventArgs(routedEvent:=IPChangedEvent)
        [RaiseEvent](routedEventArgs)
    End Sub

    Private Sub RaiseFTPPortChangedRoutedEvent()
        Dim routedEventArgs As New RoutedEventArgs(routedEvent:=FTPPortChangedEvent)
        [RaiseEvent](routedEventArgs)
    End Sub

    Private Sub RaisePayloadPortChangedEvent()
        Dim routedEventArgs As New RoutedEventArgs(routedEvent:=PayloadPortChangedEvent)
        [RaiseEvent](routedEventArgs)
    End Sub

    Private Sub IPTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles IPTextBox.TextChanged
        If Not String.IsNullOrEmpty(IPTextBox.Text) Then
            SharedIPAddress = IPTextBox.Text
            RaiseIPTextChangedRoutedEvent()
        End If
    End Sub

    Private Sub FTPPortTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles FTPPortTextBox.TextChanged
        If Not String.IsNullOrEmpty(FTPPortTextBox.Text) Then
            SharedFTPPort = FTPPortTextBox.Text
            RaiseIPTextChangedRoutedEvent()
        End If
    End Sub

    Private Sub PayloadPortTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles PayloadPortTextBox.TextChanged
        If Not String.IsNullOrEmpty(PayloadPortTextBox.Text) Then
            SharedPayloadPort = PayloadPortTextBox.Text
            RaisePayloadPortChangedEvent()
        End If
    End Sub

    Private Sub PS5Menu_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Load config if exists
        If File.Exists(Environment.CurrentDirectory + "\psmt-config.ini") Then
            Try
                SharedIPAddress = MainConfig.IniReadValue("PS5 Tools", "IP")
                SharedFTPPort = MainConfig.IniReadValue("PS5 Tools", "FTPPort")
                SharedPayloadPort = MainConfig.IniReadValue("PS5 Tools", "PayloadPort")

                IPTextBox.Text = MainConfig.IniReadValue("PS5 Tools", "IP")
                FTPPortTextBox.Text = MainConfig.IniReadValue("PS5 Tools", "FTPPort")
                PayloadPortTextBox.Text = MainConfig.IniReadValue("PS5 Tools", "PayloadPort")
            Catch ex As FileNotFoundException
                MsgBox("Could not find a valid config file.", MsgBoxStyle.Exclamation)
            End Try
        End If
    End Sub

#Region "Tools"

    Private Sub SenderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles SenderMenuItem.Click
        Dim NewPS5Sender As New PS5Sender() With {.ShowActivated = True}

        'Set values if SharedConsoleAddress is set
        NewPS5Sender.IPTextBox.Text = SharedIPAddress
        NewPS5Sender.PortTextBox.Text = SharedPayloadPort

        NewPS5Sender.Show()
    End Sub

    Private Sub OpenFTPBrowserMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenFTPBrowserMenuItem.Click
        Dim NewFTPBrowser As New FTPBrowser() With {.ShowActivated = True, .FTPS5Mode = True}
        NewFTPBrowser.ConsoleIPTextBox.Text = SharedIPAddress
        NewFTPBrowser.PortTextBox.Text = SharedFTPPort
        NewFTPBrowser.Show()
    End Sub

    Private Sub OpenBDBurnerMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenBDBurnerMenuItem.Click
        Dim NewBDBurner As New BDBurner() With {.ShowActivated = True}
        NewBDBurner.Show()
    End Sub

    Private Sub OpenWebBrowserInstallerMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenWebBrowserInstallerMenuItem.Click
        Dim NewPS5WebBrowserAdder As New PS5WebBrowserAdder With {.ShowActivated = True, .ConsoleIP = SharedIPAddress}
        NewPS5WebBrowserAdder.Show()
    End Sub

    Private Sub OpenNotificationManagerMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenNotificationManagerMenuItem.Click
        Dim NewPS5NotificationsManager As New PS5Notifications() With {.ShowActivated = True}
        NewPS5NotificationsManager.IPTextBox.Text = SharedIPAddress
        NewPS5NotificationsManager.PortTextBox.Text = SharedFTPPort
        NewPS5NotificationsManager.Show()
    End Sub

    Private Sub ClearErrorHistoryMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles ClearErrorHistoryMenuItem.Click
        If Not String.IsNullOrEmpty(SharedIPAddress) AndAlso Not String.IsNullOrEmpty(SharedFTPPort) Then

            Cursor = Cursors.Wait

            Try
                Using conn As New FtpClient(SharedIPAddress, "anonymous", "anonymous", CInt(SharedFTPPort))

                    'Configurate the FTP connection
                    conn.Config.EncryptionMode = FtpEncryptionMode.None
                    conn.Config.SslProtocols = SslProtocols.None
                    conn.Config.DataConnectionEncryption = False

                    'Connect
                    conn.Connect()

                    Dim ErrorFiles As New List(Of FtpListItem)()

                    'List disc directory
                    For Each item In conn.GetListing("/system_data/priv/error/history")
                        Select Case item.Type
                            Case FtpObjectType.File
                                ErrorFiles.Add(item)
                        End Select
                    Next

                    For Each FTPFile In ErrorFiles
                        'Delete the error file
                        conn.DeleteFile(FTPFile.FullName)
                    Next

                    'Disonnect
                    conn.Disconnect()
                End Using

                MsgBox("The error history on the console has been deleted." + vbCrLf + "Check your Settings->System Software->Error History on the PS5.", MsgBoxStyle.Information)
            Catch ex As Exception
                MsgBox("Could delete the error history, please verify your connection.", MsgBoxStyle.Exclamation)
            End Try

            Cursor = Cursors.Arrow
        Else
            MsgBox("Please set your IP:Port in the settings first.", MsgBoxStyle.Information, "Cannot connect to the PS5")
        End If
    End Sub

    Private Sub OpenGP5ManagerMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenGP5ManagerMenuItem.Click
        Dim NewGP5Creator As New GP5Creator() With {.ShowActivated = True}
        NewGP5Creator.Show()
    End Sub

    Private Sub OpenRCODumperMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenRCODumperMenuItem.Click
        Dim NewPS5RcoDumper As New PS5RcoDumper With {.ShowActivated = True, .ConsoleIP = SharedIPAddress}
        NewPS5RcoDumper.Show()
    End Sub

    Private Sub OpenRCOExtractorMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenRCOExtractorMenuItem.Click
        Dim NewPS5RcoExtractor As New PS5RcoExtractor() With {.ShowActivated = True}
        NewPS5RcoExtractor.Show()
    End Sub

    Private Sub OpenParamEditorMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenParamEditorMenuItem.Click
        Dim NewParamEditor As New PS5ParamEditor() With {.ShowActivated = True}
        NewParamEditor.Show()
    End Sub

    Private Sub OpenPKGBuilderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPKGBuilderMenuItem.Click
        Dim NewPKGBuilder As New PS5PKGBuilder() With {.ShowActivated = True}
        NewPKGBuilder.Show()
    End Sub

    Private Sub OpenPKGExtractorMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPKGExtractorMenuItem.Click
        Dim NewPS5PKGExtractor As New PS5PKGExtractor() With {.ShowActivated = True}
        NewPS5PKGExtractor.Show()
    End Sub

    Private Sub OpenAudioConverterMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenAudioConverterMenuItem.Click
        Dim NewAudioConverter As New PS5AT9Converter() With {.ShowActivated = True}
        NewAudioConverter.Show()
    End Sub

    Private Sub OpenGamePatchesMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenGamePatchesMenuItem.Click
        Dim NewPS5GamePatches As New PS5GamePatches() With {.ShowActivated = True}
        NewPS5GamePatches.Show()
    End Sub

    Private Sub OpenPKGMergerMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPKGMergerMenuItem.Click
        Dim NewPS5PKGMerger As New PS5PKGMerger() With {.ShowActivated = True}
        NewPS5PKGMerger.Show()
    End Sub

    Private Sub OpenFTPGrabberMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenFTPGrabberMenuItem.Click
        Dim NewFTPGrabber As New PS5FTPGrabber With {.ShowActivated = True, .ConsoleIP = SharedIPAddress}
        NewFTPGrabber.Show()
    End Sub

    Private Sub OpenManifestEditorMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenManifestEditorMenuItem.Click
        Dim NewManifestEditor As New PS5ManifestEditor() With {.ShowActivated = True}
        NewManifestEditor.Show()
    End Sub

    Private Sub OpenMakefSELFMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenMakefSELFMenuItem.Click
        Dim NewMakefSELFWindow As New PS5MakefSelfs() With {.ShowActivated = True}
        NewMakefSELFWindow.Show()
    End Sub

    Private Sub OpenetaHENConfiguratorMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenetaHENConfiguratorMenuItem.Click
        Dim NewetaHENConfigurator As New PS5etaHENConfigurator With {.ShowActivated = True, .ConsoleIP = SharedIPAddress}
        NewetaHENConfigurator.Show()
    End Sub

    Private Sub OpenShortcutPKGCreatorMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenShortcutPKGCreatorMenuItem.Click
        Dim NewShortcutPKGCreator As New GP5PKGBuilder() With {.ShowActivated = True}
        NewShortcutPKGCreator.Show()
    End Sub

    Private Sub PayloadBuilderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PayloadBuilderMenuItem.Click
        Dim NewPS5PayloadBuilder As New PS5PayloadBuilder() With {.ShowActivated = True}
        NewPS5PayloadBuilder.Show()
    End Sub

    Private Sub OpenSELFDecrypterMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenSELFDecrypterMenuItem.Click
        Dim NewPS5SELFDecrypter As New PS5SELFDecrypter() With {.ShowActivated = True, .PS5Host = SharedIPAddress, .PS5Port = SharedPayloadPort}
        NewPS5SELFDecrypter.Show()
    End Sub

    Private Async Sub CheckForUpdatesMenuItems_Click(sender As Object, e As RoutedEventArgs) Handles CheckForUpdatesMenuItems.Click
        If Await Utils.IsPSMultiToolsUpdateAvailable() Then
            If MsgBox("An update is available, do you want to download it now ?", MsgBoxStyle.YesNo, "PS Multi Tools Update found") = MsgBoxResult.Yes Then
                Utils.DownloadAndExecuteUpdater()
            End If
        Else
            MsgBox("PS Multi Tools is up to date!", MsgBoxStyle.Information, "No update found")
        End If
    End Sub

    Private Sub OpenPKGSenderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPKGSenderMenuItem.Click
        If Not String.IsNullOrEmpty(SharedIPAddress) Then
            Dim NewPKGSender As New PS5PKGSender With {.ShowActivated = True, .ConsoleIP = SharedIPAddress}
            NewPKGSender.Show()
        Else
            MsgBox("Please set your IP in the settings first.", MsgBoxStyle.Information, "Cannot connect to the PS5")
        End If
    End Sub

    Private Sub OpenPortCheckerMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPortCheckerMenuItem.Click
        Dim NewPS5PortChecker As New PS5PortChecker() With {.ShowActivated = True, .PS5Host = SharedIPAddress}
        NewPS5PortChecker.Show()
    End Sub

    Private Sub OpenPSClassicsfPKGBuilderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPSClassicsfPKGBuilderMenuItem.Click
        Dim NewPSClassicsfPKGBuilder As New PSClassicsfPKGBuilder() With {.ShowActivated = True}
        NewPSClassicsfPKGBuilder.Show()
    End Sub

    Private Sub OpenKLogViewerMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenKLogViewerMenuItem.Click
        Dim NewLogWindow As New PS5Log() With {.ShowActivated = True}
        NewLogWindow.PS5IPTextBox.Text = SharedIPAddress
        NewLogWindow.Show()
    End Sub

    Private Sub OpenPKGReaderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPKGReaderMenuItem.Click
        Dim NewPS5PKGViewer As New PS5PKGViewer() With {.ShowActivated = True}
        NewPS5PKGViewer.Show()
    End Sub

    Private Sub OpenDiscParamReaderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenDiscParamReaderMenuItem.Click
        If Not String.IsNullOrEmpty(SharedIPAddress) Then
            Try
                Dim ParamJSONDownloaded As Boolean = False
                Using NewFTPConnection As New FtpClient(SharedIPAddress, "anonymous", "anonymous", CInt(SharedFTPPort))

                    'Configurate the FTP connection
                    NewFTPConnection.Config.EncryptionMode = FtpEncryptionMode.None
                    NewFTPConnection.Config.SslProtocols = SslProtocols.None
                    NewFTPConnection.Config.DataConnectionEncryption = False

                    'Connect
                    NewFTPConnection.Connect()

                    'Check if the disc is compatible
                    If NewFTPConnection.DirectoryExists("/mnt/disc/bd") Then
                        If NewFTPConnection.FileExists("/mnt/disc/bd/param.json") Then
                            'Get the param.json file
                            If NewFTPConnection.DownloadFile(Environment.CurrentDirectory + "\Cache\param.json", "/mnt/disc/bd/param.json", FtpLocalExists.Overwrite, FtpVerify.None, Nothing) = FtpStatus.Success Then
                                ParamJSONDownloaded = True
                            Else
                                ParamJSONDownloaded = False
                            End If
                        Else
                            MsgBox("Disc in tray is not compatible.", MsgBoxStyle.Information, "Unknown Disc")
                        End If
                    Else
                        MsgBox("Disc in tray is not compatible.", MsgBoxStyle.Information, "Unknown Disc")
                    End If

                    'Disonnect
                    NewFTPConnection.Disconnect()
                End Using

                If ParamJSONDownloaded Then
                    Dim ParamJSONData As List(Of String) = File.ReadAllLines(Environment.CurrentDirectory + "\Cache\param.json").ToList()

                    'Remove unreadable stuff
                    ParamJSONData.RemoveRange(0, 6)

                    'Read the param.json file
                    Dim ParamJSONString As String = String.Join(Environment.NewLine, ParamJSONData)
                    ParamJSONString = String.Concat("{" + vbCrLf, ParamJSONString)
                    Dim ParamDiscDetailsData As PS5DiscParamClass.PS5DiscParam = JsonConvert.DeserializeObject(Of PS5DiscParamClass.PS5DiscParam)(ParamJSONString)

                    If ParamDiscDetailsData IsNot Nothing Then

                        Dim FirstDiscDetails As PS5DiscParamClass.Disc = ParamDiscDetailsData.Disc(0)
                        Dim FirstDiscMasterDataID As String = FirstDiscDetails.MasterDataId
                        Dim FirstDiscRole As String = FirstDiscDetails.Role

                        MsgBox("Disc Master Data ID: " + FirstDiscMasterDataID + vbCrLf +
                               "Disc Role: " + FirstDiscRole + vbCrLf +
                               "Disc Number: " + ParamDiscDetailsData.DiscNumber.ToString() + vbCrLf +
                               "Disc Total: " + ParamDiscDetailsData.DiscTotal.ToString() + vbCrLf +
                               "Master Version: " + ParamDiscDetailsData.MasterVersion + vbCrLf +
                               "Pub Tools Version: " + ParamDiscDetailsData.PubtoolsVersion + vbCrLf +
                               "Required System Version: " + ParamDiscDetailsData.RequiredSystemSoftwareVersion)
                    End If
                Else
                    MsgBox("Could not find any valid disc information.", MsgBoxStyle.Exclamation)
                End If

            Catch ex As Exception
                MsgBox("Could not find any valid disc information.", MsgBoxStyle.Exclamation)
            End Try
        Else
            MsgBox("Please set your IP:Port in the settings first.", MsgBoxStyle.Information, "Cannot connect to the PS5")
        End If
    End Sub

    Private Sub SpoofFWMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles SpoofFWMenuItem.Click
        'Check if an IP address was entered
        If Not String.IsNullOrWhiteSpace(SharedIPAddress) Then
            Dim DeviceIP As IPAddress
            Dim DevicePort As Integer

            Try
                DeviceIP = IPAddress.Parse(SharedIPAddress)
                DevicePort = Integer.Parse(SharedPayloadPort)
            Catch ex As FormatException
                MsgBox("Could not parse the set console IP. Please check your IP in the settings.", MsgBoxStyle.Exclamation, "Error sending payload")
                Exit Sub
            End Try

            Dim SelectedELF As String = Environment.CurrentDirectory + "\Tools\PS5\spoof.elf"
            Dim ELFFileInfo As New FileInfo(SelectedELF)
            Try
                Using SenderSocket As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) With {.ReceiveTimeout = 3000}
                    'Connect
                    SenderSocket.Connect(DeviceIP, DevicePort)
                    'Send ELF
                    SenderSocket.SendFile(SelectedELF)
                    'Close the connection
                    SenderSocket.Close()
                End Using
            Catch ex As SocketException
                MsgBox("Could not send selected payload. Please make sure that your PS5 can receive payloads.", MsgBoxStyle.Exclamation, "Error sending payload")
                Exit Sub
            End Try

            MsgBox("Spoofing payload has been sent" + vbCrLf + "You will need to eject and insert the disc back again to install it." + vbCrLf + "To reverse the firmware spoofing simply send this payload again.", MsgBoxStyle.Information)
        Else
            MsgBox("Please set your IP:Port in the settings first.", MsgBoxStyle.Information, "Cannot connect to the PS5")
        End If
    End Sub

    Private Sub Togglekstuff_Click(sender As Object, e As RoutedEventArgs) Handles Togglekstuff.Click
        'Check if an IP address was entered
        If Not String.IsNullOrWhiteSpace(SharedIPAddress) Then
            Dim DeviceIP As IPAddress
            Dim DevicePort As Integer

            Try
                DeviceIP = IPAddress.Parse(SharedIPAddress)
                DevicePort = Integer.Parse(SharedPayloadPort)
            Catch ex As FormatException
                MsgBox("Could not parse the set console IP. Please check your IP in the settings.", MsgBoxStyle.Exclamation, "Error sending payload")
                Exit Sub
            End Try

            Dim SelectedELF As String = Environment.CurrentDirectory + "\Tools\PS5\kstuff-toggle.elf"
            Dim ELFFileInfo As New FileInfo(SelectedELF)
            Try
                Using SenderSocket As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) With {.ReceiveTimeout = 3000}
                    'Connect
                    SenderSocket.Connect(DeviceIP, DevicePort)
                    'Send ELF
                    SenderSocket.SendFile(SelectedELF)
                    'Close the connection
                    SenderSocket.Close()
                End Using
            Catch ex As SocketException
                MsgBox("Could not send selected payload. Please make sure that your PS5 can receive payloads", MsgBoxStyle.Exclamation, "Error sending payload")
                Exit Sub
            End Try

            MsgBox("kstuff toggled!" + vbCrLf + "To revert, simply click again.", MsgBoxStyle.Information)
        Else
            MsgBox("Please set your IP:Port in the settings first.", MsgBoxStyle.Information, "Cannot connect to the PS5")
        End If
    End Sub

#End Region

#Region "Downloads"

#Region "Homebrew"

    Private Async Sub DownloadDumpRunner_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDumpRunner.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/dump_runner.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadAvatarChanger_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAvatarChanger.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/AvatarChanger_v1.00.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFTPS5_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFTPS5.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/EchoStretch/FTPS5/releases/download/v1.4/ftps5-1.4.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPS5NetworkELFLoader650_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPS5NetworkELFLoader650.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/PS2NetworkELFLoader/VMC0-PS5-6-50.card") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPS5NetworkGameLoader650_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPS5NetworkGameLoader650.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/PS2NetworkGameLoader/mast1c0re-ps2-network-game-loader-PS5-6-50.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadInternetBrowserPKG_Click(sender As Object, e As RoutedEventArgs) Handles DownloadInternetBrowserPKG.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/pkg/InternetBrowserPS5.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadStorePreviewPKG_Click(sender As Object, e As RoutedEventArgs) Handles DownloadStorePreviewPKG.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/pkg/StorePreviewPS5.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadGameHubPreviewPKG_Click(sender As Object, e As RoutedEventArgs) Handles DownloadGameHubPreviewPKG.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/pkg/GameHubPreviewPS5.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadDebugPKG_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDebugPKG.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/pkg/DebugSettingsPS5.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPSMTPKG_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPSMTPKG.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/pkg/PSMultiToolsHost.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadNewKStuff_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNewKStuff.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/kstuff_v1.5.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadKStuffToggle_Click(sender As Object, e As RoutedEventArgs) Handles DownloadKStuffToggle.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/kstuff-toggle.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadLatestetaHEN_Click(sender As Object, e As RoutedEventArgs) Handles DownloadLatestetaHEN.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/etaHEN/etaHEN/releases/download/2.1B/etaHEN.bin") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadetaHENBDJB_Click(sender As Object, e As RoutedEventArgs) Handles DownloadetaHENBDJB.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/etaHEN_v2.0b-BDJ-IPV6.iso") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadHomebrewStore_Click(sender As Object, e As RoutedEventArgs) Handles DownloadHomebrewStore.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/Store-R2-PS5.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadItemzflow_Click(sender As Object, e As RoutedEventArgs) Handles DownloadItemzflow.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/ItemzflowGameManager_v1.10.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPS5Xplorer_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPS5Xplorer.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/PS5-Xplorer_v1.04.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadELFLdr_Click(sender As Object, e As RoutedEventArgs) Handles DownloadELFLdr.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/elfldr_v0.19.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFetchPKGPS5_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFetchPKGPS5.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/fetchpkg_v0.3.1.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFetchPKGWin_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFetchPKGWin.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/fetchpkg_v0.3.1_Win64.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFTPSrv_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFTPSrv.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/ftpsrv_v0.11.2.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadSHSrv_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSHSrv.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/shsrv_v0.15.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadWebSrv_Click(sender As Object, e As RoutedEventArgs) Handles DownloadWebSrv.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/websrv_v0.23.1.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadOffAct_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOffAct.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/OffAct_v0.3.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadSELFDecrypter_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSELFDecrypter.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/ps5-self-decrypter_v0.3.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadKLogSrv_Click(sender As Object, e As RoutedEventArgs) Handles DownloadKLogSrv.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/klogsrv_v0.5.3.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadELFLdrGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadELFLdrGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/elfldr/releases/latest/download/Payload.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFetchPKGPS5Github_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFetchPKGPS5Github.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/fetchpkg/releases/latest/download/PS5.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFetchPKGWinGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFetchPKGWinGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/fetchpkg/releases/latest/download/Win64.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFTPSrvGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFTPSrvGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/ftpsrv/releases/latest/download/Payload.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadKLogSrvGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadKLogSrvGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/klogsrv/releases/latest/download/Payload.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadOffActGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOffActGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/offact/releases/latest/download/Payload.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadSHSrvGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSHSrvGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/shsrv/releases/latest/download/Payload.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadWebSrvGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadWebSrvGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/websrv/releases/latest/download/Payload.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#Region "JAR"

    Private Async Sub DownloadByepervisor_Click(sender As Object, e As RoutedEventArgs) Handles DownloadByepervisor.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/byepervisor-1.0-SNAPSHOT.jar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadDebugSettings_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDebugSettings.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/debugsettings-1.0-SNAPSHOT.jar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadDumpClassPath_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDumpClassPath.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/dumpclasspath-1.0-SNAPSHOT.jar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadCurProc_Click(sender As Object, e As RoutedEventArgs) Handles DownloadCurProc.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/dumpcurproc-1.0-SNAPSHOT.jar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadJARFTPServer_Click(sender As Object, e As RoutedEventArgs) Handles DownloadJARFTPServer.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/ftpserver-1.0-SNAPSHOT.jar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadJailbreak_Click(sender As Object, e As RoutedEventArgs) Handles DownloadJailbreak.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/jailbreak-1.0-SNAPSHOT.jar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadKerneldump_Click(sender As Object, e As RoutedEventArgs) Handles DownloadKerneldump.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/kerneldump-1.0-SNAPSHOT.jar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadJARKlogSrv_Click(sender As Object, e As RoutedEventArgs) Handles DownloadJARKlogSrv.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/klogserver-1.0-SNAPSHOT.jar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadListDirEnts_Click(sender As Object, e As RoutedEventArgs) Handles DownloadListDirEnts.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/listdirents-1.0-SNAPSHOT.jar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadMiniTennis_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMiniTennis.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/minitennis-1.0-SNAPSHOT.jar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPrintSysProps_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPrintSysProps.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/printsysprops-1.0-SNAPSHOT.jar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadUMTX1_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUMTX1.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/umtx1-1.0-SNAPSHOT.jar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadUMTX2_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUMTX2.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/umtx2-1.0-SNAPSHOT.jar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#End Region

#Region "Firmwares"

    Private Async Sub DownloadRecoveryFW403_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRecoveryFW403.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/recovery/04.03/PS5UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadSystemFW403_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSystemFW403.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/system/04.03/PS5UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRecoveryFW451_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRecoveryFW451.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/recovery/04.51/PS5UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadSystemFW451_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSystemFW451.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/system/04.51/PS5UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRecoveryFW550_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRecoveryFW550.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/recovery/05.50/PS5UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadSystemFW550_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSystemFW550.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/system/05.50/PS5UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRecoveryFW650_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRecoveryFW650.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/recovery/06.50/PS5UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadSystemFW650_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSystemFW650.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/system/06.50/PS5UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#End Region

#Region "Exploits"

    Private Sub OpenMast1c0reGitHub_Click(sender As Object, e As RoutedEventArgs) Handles OpenMast1c0reGitHub.Click
        Process.Start(New ProcessStartInfo("https://github.com/McCaulay/mast1c0re") With {.UseShellExecute = True})
    End Sub

    Private Async Sub DownloadPS5IPV6Expl_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPS5IPV6Expl.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/ex/PS5-IPV6-Kernel-Exploit-1.03.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub OpenKexGitHub_Click(sender As Object, e As RoutedEventArgs) Handles OpenKexGitHub.Click
        Process.Start(New ProcessStartInfo("https://github.com/Cryptogenic/PS5-IPV6-Kernel-Exploit") With {.UseShellExecute = True})
    End Sub

    Private Sub OpenJARLoaderGitHub_Click(sender As Object, e As RoutedEventArgs) Handles OpenJARLoaderGitHub.Click
        Process.Start(New ProcessStartInfo("https://github.com/hammer-83/ps5-jar-loader") With {.UseShellExecute = True})
    End Sub

    Private Async Sub DownloadJARLoader_Click(sender As Object, e As RoutedEventArgs) Handles DownloadJARLoader.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/ps5-jar-loader-4.1.1.iso") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadUMTXJailbreak_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUMTXJailbreak.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/PS5Dev/PS5-UMTX-Jailbreak/archive/refs/tags/v1.2.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub OpenUMTXJailbreakGitHub_Click(sender As Object, e As RoutedEventArgs) Handles OpenUMTXJailbreakGitHub.Click
        Process.Start(New ProcessStartInfo("https://github.com/PS5Dev/PS5-UMTX-Jailbreak") With {.UseShellExecute = True})
    End Sub

#End Region

#Region "WebMAN WebSrv"

    Public Sub NavigateTowebMANWebSrvUrl(InputURL As String)
        If Not String.IsNullOrEmpty(SharedIPAddress) Then
            If IswebMANWebSrvWebViewReady AndAlso IswebMANWebSrvCommandExecuted Then
                IswebMANWebSrvCommandExecuted = False
                webMANwebsrvWebView.CoreWebView2.Navigate(InputURL)
            End If
        End If
    End Sub

    Private Sub WebMANWebView_CoreWebView2InitializationCompleted(sender As Object, e As CoreWebView2InitializationCompletedEventArgs) Handles webMANwebsrvWebView.CoreWebView2InitializationCompleted
        If e.IsSuccess Then
            IswebMANWebSrvWebViewReady = True
        End If
    End Sub

    Private Sub WebMANWebView_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs) Handles webMANwebsrvWebView.NavigationCompleted
        If e.IsSuccess Then
            IswebMANWebSrvCommandExecuted = True
        End If
    End Sub

    Private Sub OpenPS5WebSrvInterface_Click(sender As Object, e As RoutedEventArgs) Handles OpenPS5WebSrvInterface.Click
        If Not String.IsNullOrEmpty(SharedIPAddress) Then
            Dim NewwebMANMODWebGUI As New PS5webMANBrowser() With {.ShowActivated = True, .WebMANWebSrvAddress = "http://" & SharedIPAddress + ":8080"}
            NewwebMANMODWebGUI.Show()
        Else
            MsgBox("Please set your PS5 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub BrowsePS5FileSystem_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS5FileSystem.Click
        If Not String.IsNullOrEmpty(SharedIPAddress) Then
            Dim NewwebMANMODWebGUI As New PS5webMANBrowser() With {.ShowActivated = True, .WebMANWebSrvAddress = "http://" & SharedIPAddress + ":8080/fs/"}
            NewwebMANMODWebGUI.Show()
        Else
            MsgBox("Please set your PS5 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub ManagePS5Homebrew_Click(sender As Object, e As RoutedEventArgs) Handles ManagePS5Homebrew.Click
        Dim NewWebSrvHomebrewManager As New WebSrvHomebrewManager() With {.ShowActivated = True}
        NewWebSrvHomebrewManager.PS5IPTextBox.Text = SharedIPAddress
        NewWebSrvHomebrewManager.Show()
    End Sub

    Private Sub ManagePS5GameROMs_Click(sender As Object, e As RoutedEventArgs) Handles ManagePS5GameROMs.Click
        Dim NewWebSrvHomebrewManager As New WebSrvHomebrewManager() With {.ShowActivated = True}
        NewWebSrvHomebrewManager.PS5IPTextBox.Text = SharedIPAddress
        NewWebSrvHomebrewManager.Show()
    End Sub

    Private Sub GetAuthID_Click(sender As Object, e As RoutedEventArgs) Handles GetAuthID.Click
        Try
            Dim NewTelnetClient As New TelnetClient(SharedIPAddress, 2323)

            Thread.Sleep(20000) 'Wait 20sec until connected

            'Get welcome message to check if connected successfully
            Dim WelcomeMesssage As String = NewTelnetClient.Read()
            If Not String.IsNullOrEmpty(WelcomeMesssage) Then

                NewTelnetClient.Write("authid")

                Thread.Sleep(500)

                Dim RetrievedAuthID As String = NewTelnetClient.Read()
                MsgBox("AuthID: ")
            Else
                MsgBox("ShSrv took to long to respond.", MsgBoxStyle.Critical, "Error reading output")
                NewTelnetClient.Close()
            End If

            'Disconnect
            NewTelnetClient.Close()
        Catch ex As Exception
            MsgBox("An error occurred: " & ex.Message)
        End Try
    End Sub

    Private Sub GetConsoleInfo_Click(sender As Object, e As RoutedEventArgs) Handles GetConsoleInfo.Click
        Try
            Dim NewTelnetClient As New TelnetClient(SharedIPAddress, 2323)

            Thread.Sleep(20000) 'Wait 20sec until connected

            'Get welcome message and split console information output to values
            Dim WelcomeMesssage As String = NewTelnetClient.Read()
            If Not String.IsNullOrEmpty(WelcomeMesssage) Then
                Dim WelcomeData As New Dictionary(Of String, String)()
                Dim SplittedLines() As String = WelcomeMesssage.Split(New String() {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)

                For Each Line As String In SplittedLines
                    Dim LineParts() As String = Line.Split(":"c)
                    If LineParts.Length >= 2 Then
                        Dim DictKey As String = LineParts(0).Trim()
                        Dim DictValue As String = LineParts(1).Trim()
                        WelcomeData(DictKey) = DictValue
                    End If
                Next

                Dim ConsoleModel As String = If(WelcomeData.ContainsKey("Model"), WelcomeData("Model"), String.Empty)
                Dim ConsoleSerialNumber As String = If(WelcomeData.ContainsKey("S/N"), WelcomeData("S/N"), String.Empty)
                Dim ConsoleSoftwareVersion As String = If(WelcomeData.ContainsKey("S/W"), WelcomeData("S/W"), String.Empty)
                Dim ConsoleSoCTemp As String = If(WelcomeData.ContainsKey("SoC temp"), WelcomeData("SoC temp"), String.Empty)
                Dim ConsoleCPUTemp As String = If(WelcomeData.ContainsKey("CPU temp"), WelcomeData("CPU temp"), String.Empty)
                Dim ConsoleCPUFreq As String = If(WelcomeData.ContainsKey("CPU freq"), WelcomeData("CPU freq"), String.Empty)

                MsgBox("Model Number: " & ConsoleModel & vbCrLf &
                       "Serial Number: " & ConsoleSerialNumber & vbCrLf &
                       "Software Version: " & ConsoleSoftwareVersion & vbCrLf &
                       "SoC Temperature: " & ConsoleSoCTemp & vbCrLf &
                       "CPU Temperature: " & ConsoleCPUTemp & vbCrLf &
                       "CPU Frequency: " & ConsoleCPUFreq)
            Else
                MsgBox("ShSrv took to long to respond.", MsgBoxStyle.Critical, "Error reading output")
                NewTelnetClient.Close()
            End If

            'Disconnect
            NewTelnetClient.Close()
        Catch ex As Exception
            MsgBox("An error occurred: " & ex.Message)
        End Try
    End Sub

#End Region

End Class
