Imports FluentFTP
Imports Microsoft.Web.WebView2.Core
Imports Newtonsoft.Json
Imports PS_Multi_Tools.INI
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Security.Authentication

Public Class PS5Menu

    Public SharedConsoleAddress As String = ""

    Private IswebMANWebSrvWebViewReady As Boolean = False
    Private IswebMANWebSrvCommandExecuted As Boolean = False

    Public Shared ReadOnly IPChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(name:="ConsoleAddressChanged", routingStrategy:=RoutingStrategy.Bubble, handlerType:=GetType(RoutedEventHandler), ownerType:=GetType(PS5Menu))

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

    Private Sub RaiseIPTextChangedRoutedEvent()
        Dim routedEventArgs As New RoutedEventArgs(routedEvent:=IPChangedEvent)
        [RaiseEvent](routedEventArgs)
    End Sub

    Private Sub FTPIPTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles FTPIPTextBox.TextChanged
        If Not String.IsNullOrEmpty(FTPIPTextBox.Text) And FTPIPTextBox.Text.Contains(":"c) Then
            SharedConsoleAddress = FTPIPTextBox.Text
            RaiseIPTextChangedRoutedEvent()
        End If
    End Sub

    Private Sub PS5Menu_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Load config if exists
        If File.Exists(Environment.CurrentDirectory + "\psmt-config.ini") Then
            Try
                Dim MainConfig As New IniFile(Environment.CurrentDirectory + "\psmt-config.ini")
                SharedConsoleAddress = MainConfig.IniReadValue("PS5 Tools", "IP") + ":" + MainConfig.IniReadValue("PS5 Tools", "Port")
                FTPIPTextBox.Text = MainConfig.IniReadValue("PS5 Tools", "IP") + ":" + MainConfig.IniReadValue("PS5 Tools", "Port")
            Catch ex As FileNotFoundException
                MsgBox("Could not find a valid config file.", MsgBoxStyle.Exclamation)
            End Try
        End If
    End Sub

#Region "Tools"

    Private Sub SenderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles SenderMenuItem.Click
        Dim NewPS5Sender As New PS5Sender() With {.ShowActivated = True}

        'Set values if SharedConsoleAddress is set
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            NewPS5Sender.IPTextBox.Text = SharedConsoleAddress.Split(":"c)(0)
            NewPS5Sender.PortTextBox.Text = SharedConsoleAddress.Split(":"c)(1)
        End If

        NewPS5Sender.Show()
    End Sub

    Private Sub OpenFTPBrowserMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenFTPBrowserMenuItem.Click
        Dim NewFTPBrowser As New FTPBrowser() With {.ShowActivated = True, .FTPS5Mode = True}
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            NewFTPBrowser.ConsoleIPTextBox.Text = SharedConsoleAddress.Split(":"c)(0)
            NewFTPBrowser.PortTextBox.Text = SharedConsoleAddress.Split(":"c)(1)
        End If
        NewFTPBrowser.Show()
    End Sub

    Private Sub OpenBDBurnerMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenBDBurnerMenuItem.Click
        Dim NewBDBurner As New BDBurner() With {.ShowActivated = True}
        NewBDBurner.Show()
    End Sub

    Private Sub OpenWebBrowserInstallerMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenWebBrowserInstallerMenuItem.Click
        Dim NewPS5WebBrowserAdder As New PS5WebBrowserAdder() With {.ShowActivated = True}
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            NewPS5WebBrowserAdder.ConsoleIP = SharedConsoleAddress.Split(":"c)(0)
        End If
        NewPS5WebBrowserAdder.Show()
    End Sub

    Private Sub OpenNotificationManagerMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenNotificationManagerMenuItem.Click
        Dim NewPS5NotificationsManager As New PS5Notifications() With {.ShowActivated = True}
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            NewPS5NotificationsManager.IPTextBox.Text = SharedConsoleAddress.Split(":"c)(0)
            NewPS5NotificationsManager.PortTextBox.Text = SharedConsoleAddress.Split(":"c)(1)
        End If
        NewPS5NotificationsManager.Show()
    End Sub

    Private Sub ClearErrorHistoryMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles ClearErrorHistoryMenuItem.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then

            Cursor = Cursors.Wait

            Try
                Using conn As New FtpClient(SharedConsoleAddress.Split(":"c)(0), "anonymous", "anonymous", 1337)

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
        Dim NewGP5Creator As New GP5Creator() With {.PubToolsPath = Environment.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe", .ShowActivated = True}
        NewGP5Creator.Show()
    End Sub

    Private Sub OpenRCODumperMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenRCODumperMenuItem.Click
        Dim NewPS5RcoDumper As New PS5RcoDumper() With {.ShowActivated = True}

        'Set values if SharedConsoleAddress is set
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            NewPS5RcoDumper.ConsoleIP = SharedConsoleAddress.Split(":"c)(0)
        End If

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
        Dim NewPKGBuilder As New PS5PKGBuilder() With {.PubToolsPath = Environment.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe", .ShowActivated = True}
        NewPKGBuilder.Show()
    End Sub

    Private Sub OpenAudioConverterMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenAudioConverterMenuItem.Click
        Dim NewAudioConverter As New PS5AT9Converter() With {.ShowActivated = True}

        If File.Exists(Environment.CurrentDirectory + "\Tools\PS5\at9tool.exe") Then
            NewAudioConverter.AT9Tool = Environment.CurrentDirectory + "\Tools\PS5\at9tool.exe"
        Else
            NewAudioConverter.IsEnabled = False
            MsgBox("Could not find the at9tool." + vbCrLf + "Please add it inside the 'Tools\PS5' folder inside PS Multi Tools.", MsgBoxStyle.Information, "at9tool not available")
        End If

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
        Dim NewFTPGrabber As New PS5FTPGrabber() With {.ShowActivated = True}
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            NewFTPGrabber.ConsoleIP = SharedConsoleAddress.Split(":"c)(0)
        End If
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
        Dim NewetaHENConfigurator As New PS5etaHENConfigurator() With {.ShowActivated = True}
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            NewetaHENConfigurator.ConsoleIP = SharedConsoleAddress.Split(":"c)(0)
        End If
        NewetaHENConfigurator.Show()
    End Sub

    Private Sub OpenShortcutPKGCreatorMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenShortcutPKGCreatorMenuItem.Click
        Dim NewShortcutPKGCreator As New GP5PKGBuilder() With {.ShowActivated = True}
        NewShortcutPKGCreator.Show()
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
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            Dim NewPKGSender As New PS5PKGSender With {.ShowActivated = True, .ConsoleIP = SharedConsoleAddress.Split(":"c)(0)}
            NewPKGSender.Show()
        Else
            MsgBox("Please set your IP:Port in the settings first.", MsgBoxStyle.Information, "Cannot connect to the PS5")
        End If
    End Sub

    Private Sub OpenPSClassicsfPKGBuilderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPSClassicsfPKGBuilderMenuItem.Click
        Dim NewPSClassicsfPKGBuilder As New PSClassicsfPKGBuilder() With {.ShowActivated = True}
        NewPSClassicsfPKGBuilder.Show()
    End Sub

    Private Sub OpenKLogViewerMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenKLogViewerMenuItem.Click
        Dim NewLogWindow As New PS5Log() With {.ShowActivated = True}
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            NewLogWindow.PS5IPTextBox.Text = SharedConsoleAddress.Split(":"c)(0)
        End If
        NewLogWindow.Show()
    End Sub

    Private Sub OpenPKGReaderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPKGReaderMenuItem.Click
        Dim NewPS5PKGViewer As New PS5PKGViewer() With {.ShowActivated = True}
        NewPS5PKGViewer.Show()
    End Sub

    Private Sub OpenDiscParamReaderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenDiscParamReaderMenuItem.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            Try
                Dim ParamJSONDownloaded As Boolean = False
                Using NewFTPConnection As New FtpClient(SharedConsoleAddress.Split(":"c)(0), "anonymous", "anonymous", 1337)

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
        If Not String.IsNullOrWhiteSpace(SharedConsoleAddress) Then
            Dim DeviceIP As IPAddress

            Try
                DeviceIP = IPAddress.Parse(SharedConsoleAddress.Split(":"c)(0))
            Catch ex As FormatException
                MsgBox("Could not parse the set console IP. Please check your IP in the settings.", MsgBoxStyle.Exclamation, "Error sending payload")
                Exit Sub
            End Try

            Dim SelectedELF As String = Environment.CurrentDirectory + "\Tools\PS5\spoof.elf"
            Dim ELFFileInfo As New FileInfo(SelectedELF)
            Try
                Using SenderSocket As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) With {.ReceiveTimeout = 3000}
                    'Connect
                    SenderSocket.Connect(DeviceIP, 9020)
                    'Send ELF
                    SenderSocket.SendFile(SelectedELF)
                    'Close the connection
                    SenderSocket.Close()
                End Using
            Catch ex As SocketException
                MsgBox("Could not send selected payload. Please make sure that your PS5 can receive payloads on port 9020.", MsgBoxStyle.Exclamation, "Error sending payload")
                Exit Sub
            End Try

            MsgBox("Spoofing payload has been sent" + vbCrLf + "You will need to eject and insert the disc back again to install it." + vbCrLf + "To reverse the firmware spoofing simply send this payload again.", MsgBoxStyle.Information)
        Else
            MsgBox("Please set your IP:Port in the settings first.", MsgBoxStyle.Information, "Cannot connect to the PS5")
        End If
    End Sub

#End Region

#Region "Downloads"

#Region "Homebrew"

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

    Private Async Sub DownloadKStuff_Click(sender As Object, e As RoutedEventArgs) Handles DownloadKStuff.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://sleirsgoevy.github.io/ps4jb2/ps5-403/ps5-kstuff.bin") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadNewKStuff_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNewKStuff.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/kstuff_6.50.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadetaHEN_Click(sender As Object, e As RoutedEventArgs) Handles DownloadetaHEN.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/etaHEN/etaHEN/releases/download/1.9b/etaHEN.bin") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadetaHENNoToolbox_Click(sender As Object, e As RoutedEventArgs) Handles DownloadetaHENNoToolbox.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/etaHEN/etaHEN/releases/download/1.9b/etaHEN_no_toolbox.bin") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadetaHENBeta_Click(sender As Object, e As RoutedEventArgs) Handles DownloadetaHENBeta.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/etaHEN/etaHEN/releases/download/2.0b-pre/etaHEN.bin") = False Then
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
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/ItemzflowGameManager_v1.08.pkg") = False Then
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
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/elfldr_v0.18.1.elf") = False Then
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
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/shsrv_v0.14.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadWebSrv_Click(sender As Object, e As RoutedEventArgs) Handles DownloadWebSrv.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5"}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/websrv_v0.21.elf") = False Then
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
        Process.Start("https://github.com/McCaulay/mast1c0re")
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
        Process.Start("https://github.com/Cryptogenic/PS5-IPV6-Kernel-Exploit")
    End Sub

    Private Sub OpenJARLoaderGitHub_Click(sender As Object, e As RoutedEventArgs) Handles OpenJARLoaderGitHub.Click
        Process.Start("https://github.com/hammer-83/ps5-jar-loader")
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
        Process.Start("https://github.com/PS5Dev/PS5-UMTX-Jailbreak")
    End Sub

#End Region

#Region "WebMAN WebSrv"

    Public Sub NavigateTowebMANWebSrvUrl(InputURL As String)
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
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

    Private Sub ShowSystemInfoOnPS5_Click(sender As Object, e As RoutedEventArgs) Handles ShowSystemInfoOnPS5.Click
        MsgBox("Not ready yet.", MsgBoxStyle.Information)
    End Sub

    Private Sub OpenPS5WebSrvInterface_Click(sender As Object, e As RoutedEventArgs) Handles OpenPS5WebSrvInterface.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            Dim NewwebMANMODWebGUI As New PS5webMANBrowser() With {.ShowActivated = True, .WebMANWebSrvAddress = "http://" & SharedConsoleAddress.Split(":"c)(0) + ":8080"}
            NewwebMANMODWebGUI.Show()
        Else
            MsgBox("Please set your PS5 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub BrowsePS5FileSystem_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS5FileSystem.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            Dim NewwebMANMODWebGUI As New PS5webMANBrowser() With {.ShowActivated = True, .WebMANWebSrvAddress = "http://" & SharedConsoleAddress.Split(":"c)(0) + ":8080/fs/"}
            NewwebMANMODWebGUI.Show()
        Else
            MsgBox("Please set your PS5 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub ManagePS5Homebrew_Click(sender As Object, e As RoutedEventArgs) Handles ManagePS5Homebrew.Click
        Dim NewWebSrvHomebrewManager As New WebSrvHomebrewManager() With {.ShowActivated = True}

        'Set values if SharedConsoleAddress is set
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            NewWebSrvHomebrewManager.PS5IPTextBox.Text = SharedConsoleAddress.Split(":"c)(0)
        End If

        NewWebSrvHomebrewManager.Show()
    End Sub

    Private Sub ManagePS5GameROMs_Click(sender As Object, e As RoutedEventArgs) Handles ManagePS5GameROMs.Click
        'Will be replaced later
        Dim NewWebSrvHomebrewManager As New WebSrvHomebrewManager() With {.ShowActivated = True}

        'Set values if SharedConsoleAddress is set
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            NewWebSrvHomebrewManager.PS5IPTextBox.Text = SharedConsoleAddress.Split(":"c)(0)
        End If

        NewWebSrvHomebrewManager.Show()
    End Sub

#End Region

End Class
