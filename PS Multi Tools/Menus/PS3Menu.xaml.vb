Imports System.IO
Imports System.Windows.Forms
Imports Microsoft.Web.WebView2.Core
Imports PS_Multi_Tools.INI

Public Class PS3Menu

    Private WithEvents PS3NetSrvProcess As Process = Nothing
    Private IswebMANMODWebViewReady As Boolean = False
    Private IswebMANMODCommandExecuted As Boolean = False

    Public SharedConsoleAddress As String = ""

#Region "IP Change Events"
    Public Shared ReadOnly IPChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(name:="ConsoleAddressChanged", routingStrategy:=RoutingStrategy.Bubble, handlerType:=GetType(RoutedEventHandler), ownerType:=GetType(PS3Menu))

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
#End Region

    Private Sub PS3Menu_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Load config if exists
        If File.Exists(Environment.CurrentDirectory + "\psmt-config.ini") Then
            Try
                Dim MainConfig As New IniFile(Environment.CurrentDirectory + "\psmt-config.ini")
                SharedConsoleAddress = MainConfig.IniReadValue("PS3 Tools", "IP") + ":" + MainConfig.IniReadValue("PS3 Tools", "Port")
                FTPIPTextBox.Text = MainConfig.IniReadValue("PS3 Tools", "IP") + ":" + MainConfig.IniReadValue("PS3 Tools", "Port")
            Catch ex As FileNotFoundException
                MsgBox("Could not find a valid config file.", MsgBoxStyle.Exclamation)
            End Try
        End If
    End Sub

#Region "Tools"


    Private Sub BatchRenameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles BatchRenameMenuItem.Click
        Dim NewBatchRename As New BatchRename() With {.ShowActivated = True}
        NewBatchRename.Show()
    End Sub

    Private Sub OpenSFOEditor_Click(sender As Object, e As RoutedEventArgs) Handles OpenSFOEditor.Click
        Dim NewSFOEditor As New SFOEditor() With {.ShowActivated = True}
        NewSFOEditor.Show()
    End Sub

    Private Sub OpenISOTools_Click(sender As Object, e As RoutedEventArgs) Handles OpenISOTools.Click
        Dim NewISOTools As New PS3ISOTools() With {.ShowActivated = True}
        NewISOTools.Show()
    End Sub

    Private Sub OpenCoreOSTools_Click(sender As Object, e As RoutedEventArgs) Handles OpenCoreOSTools.Click
        Dim NewCoreOSTools As New PS3CoreOSTools() With {.ShowActivated = True}
        NewCoreOSTools.Show()
    End Sub

    Private Sub OpenFixTar_Click(sender As Object, e As RoutedEventArgs) Handles OpenFixTar.Click
        Dim NewFixTar As New PS3FixTar() With {.ShowActivated = True}
        NewFixTar.Show()
    End Sub

    Private Sub OpenPUPUnpacker_Click(sender As Object, e As RoutedEventArgs) Handles OpenPUPUnpacker.Click
        Dim NewPUPUnpacker As New PS3PUPUnpacker() With {.ShowActivated = True}
        NewPUPUnpacker.Show()
    End Sub

    Private Sub OpenRCODumper_Click(sender As Object, e As RoutedEventArgs) Handles OpenRCODumper.Click
        Dim NewRCODumper As New PS3RCODumper() With {.ShowActivated = True}
        NewRCODumper.Show()
    End Sub

    Private Sub OpenSELFReader_Click(sender As Object, e As RoutedEventArgs) Handles OpenSELFReader.Click
        Dim NewSELFReader As New PS3ReadSELF() With {.ShowActivated = True}
        NewSELFReader.Show()
    End Sub

    Private Sub OpenFTPBrowser_Click(sender As Object, e As RoutedEventArgs) Handles OpenFTPBrowser.Click
        Dim NewFTPBrowser As New FTPBrowser() With {.ShowActivated = True}
        NewFTPBrowser.Show()
    End Sub

    Private Sub OpenPKGExtractor_Click(sender As Object, e As RoutedEventArgs) Handles OpenPKGExtractor.Click
        Dim NewPKGExtractor As New PS3PKGExtractor() With {.ShowActivated = True}
        NewPKGExtractor.Show()
    End Sub

    Private Async Sub CheckForUpdatesMenuItems_Click(sender As Object, e As RoutedEventArgs) Handles CheckForUpdatesMenuItem.Click
        If Await Utils.IsPSMultiToolsUpdateAvailable() Then
            If MsgBox("An update is available, do you want to download it now ?", MsgBoxStyle.YesNo, "PS Multi Tools Update found") = MsgBoxResult.Yes Then
                Utils.DownloadAndExecuteUpdater()
            End If
        Else
            MsgBox("PS Multi Tools is up to date!", MsgBoxStyle.Information, "No update found")
        End If
    End Sub

#End Region

#Region "Menu Downloads"

#Region "Homebrew"

    Private Async Sub DownloadAdvancedPowerOptions_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAdvancedPowerOptions.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/Advanced_Power_Options_v1.11.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadAdvancedTools_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAdvancedTools.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/PS3AdvancedTools_v1.0.1.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadApollo_Click(sender As Object, e As RoutedEventArgs) Handles DownloadApollo.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/apollo-ps3-v1.8.4.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadApolloGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadApolloGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/bucanero/apollo-ps3/releases/latest/download/apollo-ps3.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadArtemis_Click(sender As Object, e As RoutedEventArgs) Handles DownloadArtemis.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/ArtemisPS3-GUI-r6.3..pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadArtemisGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadArtemisGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/bucanero/ArtemisPS3/releases/latest/download/ArtemisPS3-GUI.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadAwesomeMPManager_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAwesomeMPManager.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/Awesome_MountPoint_Manager_1.1a.AllCFW.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadCCAPI_Click(sender As Object, e As RoutedEventArgs) Handles DownloadCCAPI.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/CCAPI_v2.80_Rev10.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadComgenieGeohot_Click(sender As Object, e As RoutedEventArgs) Handles DownloadComgenieGeohot.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/ComgenieAwesomeFilemanager355.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadComgenieNew_Click(sender As Object, e As RoutedEventArgs) Handles DownloadComgenieNew.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/ComgenieAwesomeFilemanager421.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadComgenieOld_Click(sender As Object, e As RoutedEventArgs) Handles DownloadComgenieOld.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/ComgenieAwesomeFilemanager.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadIrisman_Click(sender As Object, e As RoutedEventArgs) Handles DownloadIrisman.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/IRISMAN_4.90.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadIrismanGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadIrismanGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/IRISMAN/releases/download/4.90/IRISMAN_4.90.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadManagunzBM_Click(sender As Object, e As RoutedEventArgs) Handles DownloadManagunzBM.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/ManaGunZ_v1.41.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadManagunzFM_Click(sender As Object, e As RoutedEventArgs) Handles DownloadManagunzFM.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/ManaGunZ_FileManager_v1.41.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadMovian_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMovian.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/movian-5.0.730-deank-playstation3.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadMultiMAN_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMultiMAN.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/multiMAN_04.85.01_BASE_(20191010).pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPKGi_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPKGi.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/pkgi-ps3.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPKGiGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPKGiGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/bucanero/pkgi-ps3/releases/latest/download/pkgi-ps3.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadReact_Click(sender As Object, e As RoutedEventArgs) Handles DownloadReact.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/reActPSN_v3.20+.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRebugToolbox_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRebugToolbox.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/REBUG_TOOLBOX_02.03.06.MULTI.16.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadSENEnabler_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSENEnabler.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/SEN_Enabler_v6.2.7_[CEX-DEX]_[4.87].pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadUltimateToolbox_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUltimateToolbox.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/Ultimate_Toolbox_v2.03_FULL_version.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadUnlockHDDSpace_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUnlockHDDSpace.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/Unlock_HDD_Space.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#Region "webMAN MOD Downloads"

    Private Async Sub DownloadCoversPackPS3_Click(sender As Object, e As RoutedEventArgs) Handles DownloadCoversPackPS3.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/1.0/EP0001-BLES80608_00-COVERS0000000000.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadCoversPackPSXPS2_Click(sender As Object, e As RoutedEventArgs) Handles DownloadCoversPackPSXPS2.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/1.0/EP0001-BLES80608_00-COVERS00000RETRO.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadMultiMANMod_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMultiMANMod.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/multiMAN/multiMAN_MOD_based_mmCM_4.85.01.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadNetsrv_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNetsrv.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/webMAN-MOD/releases/download/1.47.48/ps3netsrv_20250501.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPrepIso_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPrepIso.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/webMAN-MOD/releases/download/1.47.48/prepISO_1.33.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPS2ClassicsLauncher_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPS2ClassicsLauncher.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/webMAN-MOD/releases/download/1.47.48/PS2_Classics_Launcher.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPS2Config_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPS2Config.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/webMAN-MOD/releases/download/1.47.48/PS2CONFIG.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPSPMinisLauncher_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPSPMinisLauncher.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/webMAN-MOD/releases/download/1.47.48/PSP_Minis_Launcher.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPSPRemastersLauncher_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPSPRemastersLauncher.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/webMAN-MOD/releases/download/1.47.48/PSP_Remasters_Launcher.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadWebManMod_Click(sender As Object, e As RoutedEventArgs) Handles DownloadWebManMod.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/webMAN-MOD/releases/download/1.47.48/webMAN_MOD_1.47.48_Installer.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadColorfulWMTheme_Click(sender As Object, e As RoutedEventArgs) Handles DownloadColorfulWMTheme.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/Themes/wm_theme_colorful.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFlowerificationWMTheme_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFlowerificationWMTheme.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/Themes/wm_theme_flowerification.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadMetalificationWMTheme_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMetalificationWMTheme.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/Themes/wm_theme_metalification.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRebugificationWMTheme_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRebugificationWMTheme.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/Themes/wm_theme_rebugification.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRetroArchWebMANMod_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroArchWebMANMod.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/RetroArch_CE/RetroArch_CE.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadStandardWMTheme_Click(sender As Object, e As RoutedEventArgs) Handles DownloadStandardWMTheme.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/Themes/wm_theme_standard.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#End Region

#Region "Firmwares"

#Region "Custom"

#Region "Classic"

    Private Async Sub Download355DexDowngrader_Click(sender As Object, e As RoutedEventArgs) Handles Download355DexDowngrader.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/PS3-CFW-3.55-DEX-DOWNGRADER_PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadCobra355_Click(sender As Object, e As RoutedEventArgs) Handles DownloadCobra355.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/Cobra%203.55%20CFW/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadGeoHot_Click(sender As Object, e As RoutedEventArgs) Handles DownloadGeoHot.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/GeoHot%203.55%20CFW/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadKmeaw_Click(sender As Object, e As RoutedEventArgs) Handles DownloadKmeaw.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/Kmeaw%203.55%20CFW/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadMiralaTijera_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMiralaTijera.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/MiralaTijera%203.55%20CFW/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadOTHEROSColdBoot_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOTHEROSColdBoot.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/OTHEROS++%20COLD-BOOT%203.55%20CFW/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadOTHEROSSpecial_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOTHEROSSpecial.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/OTHEROS++%20SPECIAL%203.55%20CFW/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPS3ITA_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPS3ITA.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/PS3ITA%203.55%20CFW%20v1.1/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPS3ULTIMATE_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPS3ULTIMATE.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/PS3ULTIMATE%203.55%20CFW/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRebugRex_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRebugRex.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/REBUG%20REX%20EDITION%203.55.4%20CFW/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRogero_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRogero.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/Rogero%20v3.7%203.55%20CFW/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadWaninkoko_Click(sender As Object, e As RoutedEventArgs) Handles DownloadWaninkoko.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/Waninkoko%203.55%20CFW%20v2/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadWutangrza_Click(sender As Object, e As RoutedEventArgs) Handles DownloadWutangrza.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/Wutangrza%203.55%20CFW/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "Current"

    Private Async Sub DownloadREBUGDRex484_Click(sender As Object, e As RoutedEventArgs) Handles DownloadREBUGDRex484.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.84/REBUG%20D-REX%20EDITION%204.84.2%20CFW.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadREBUGRex484_Click(sender As Object, e As RoutedEventArgs) Handles DownloadREBUGRex484.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.84/REBUG%20REX%20EDITION%204.84.2%20CFW.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadEvilnatCobra490Cex_Click(sender As Object, e As RoutedEventArgs) Handles DownloadEvilnatCobra490Cex.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.90/CFW%204.90%20Evilnat%20Cobra%208.4%20[CEX].rar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadEvilnatCobra490Dex_Click(sender As Object, e As RoutedEventArgs) Handles DownloadEvilnatCobra490Dex.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.90/CFW%204.90%20Evilnat%20Cobra%208.4%20[DEX].rar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadEvilnatCobra492PEX_Click(sender As Object, e As RoutedEventArgs) Handles DownloadEvilnatCobra492PEX.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.92/CFW%204.92%20Evilnat%20Cobra%208.5%20%5BPEX%5D.rar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadEvilnatCobra492PEXNoBD_Click(sender As Object, e As RoutedEventArgs) Handles DownloadEvilnatCobra492PEXNoBD.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.92/CFW%204.92%20Evilnat%20Cobra%208.5%20%5BPEX%5D%20%5BnoBD%5D.rar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadEvilnatCobra492PEXNoBDNoBT_Click(sender As Object, e As RoutedEventArgs) Handles DownloadEvilnatCobra492PEXNoBDNoBT.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.92/CFW%204.92%20Evilnat%20Cobra%208.5%20%5BPEX%5D%20%5BnoBD%2BnoBT%5D.rar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadEvilnatCobra492PEXNoBT_Click(sender As Object, e As RoutedEventArgs) Handles DownloadEvilnatCobra492PEXNoBT.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.92/CFW%204.92%20Evilnat%20Cobra%208.5%20%5BPEX%5D%20%5BnoBT%5D.rar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadEvilnatCobra492PEXOC_Click(sender As Object, e As RoutedEventArgs) Handles DownloadEvilnatCobra492PEXOC.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.92/CFW%204.92%20Evilnat%20Cobra%208.5%20%5BPEX%5D%20%5BOC%5D.rar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#End Region

#Region "Official"

    Private Async Sub DownloadOFW102_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOFW102.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/ofw/1.02/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadOFW315_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOFW315.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/ofw/3.15/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadOFW355_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOFW355.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/ofw/3.55/PS3UPDAT.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#End Region

#Region "Emulators"

    Private Async Sub DownloadRetroArchCommunity_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroArchCommunity.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/emu/RetroArch_Psx-Place_Community_Edition_unofficial_beta-20220315.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

    Private Async Sub DownloadDKeyFiles_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDKeyFiles.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/DKEY.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadKeyFiles_Click(sender As Object, e As RoutedEventArgs) Handles DownloadKeyFiles.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/KEY.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "webMAN MOD Tools"

    Public Sub NavigateTowebMANMODUrl(InputURL As String)
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate(InputURL)
            End If
        End If
    End Sub

    Private Sub EjectDisc_Click(sender As Object, e As RoutedEventArgs) Handles EjectDisc.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/eject.ps3")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub ExitToXMB_Click(sender As Object, e As RoutedEventArgs) Handles ExitToXMB.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/xmb.ps3$exit")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub DownloadPKGFromURLToPS3_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPKGFromURLToPS3.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                Dim NewInputDialog As New InputDialog With {.Title = "Download a PKG to the PS3"}
                NewInputDialog.NewValueTextBox.Text = ""
                NewInputDialog.InputDialogTitleTextBlock.Text = "Enter the PKG URL :"
                NewInputDialog.ConfirmButton.Content = "Download"

                If NewInputDialog.ShowDialog() = True Then
                    Dim InputDialogResult As String = NewInputDialog.NewValueTextBox.Text
                    IswebMANMODCommandExecuted = False
                    WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/xmb.ps3/download.ps3?url=" + InputDialogResult)
                End If
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub DownloadFileFromURLToPS3_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFileFromURLToPS3.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                Dim NewURLInputDialog As New InputDialog With {.Title = "Download a file to the PS3"}
                NewURLInputDialog.NewValueTextBox.Text = ""
                NewURLInputDialog.InputDialogTitleTextBlock.Text = "Enter the file URL :"
                NewURLInputDialog.ConfirmButton.Content = "Download"

                Dim NewDestinationInputDialog As New InputDialog With {.Title = "Download Destination"}
                NewDestinationInputDialog.NewValueTextBox.Text = "/dev_hdd0/FOLDER/FILENAME.EXTENSION"
                NewDestinationInputDialog.InputDialogTitleTextBlock.Text = "Enter the destination path :"
                NewDestinationInputDialog.ConfirmButton.Content = "Confirm"

                If NewURLInputDialog.ShowDialog() = True AndAlso NewDestinationInputDialog.ShowDialog() = True Then
                    Dim NewURLInputDialogResult As String = NewURLInputDialog.NewValueTextBox.Text
                    Dim NewDestinationInputDialogResult As String = NewDestinationInputDialog.NewValueTextBox.Text

                    IswebMANMODCommandExecuted = False
                    WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/xmb.ps3/download.ps3?to=" + NewDestinationInputDialogResult + "&url=" + NewURLInputDialogResult)
                End If
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub DownloadAndInstallPKGFromURL_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAndInstallPKGFromURL.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                Dim NewInputDialog As New InputDialog With {.Title = "Download & Install a PKG on the PS3"}
                NewInputDialog.NewValueTextBox.Text = ""
                NewInputDialog.InputDialogTitleTextBlock.Text = "Enter the PKG URL :"
                NewInputDialog.ConfirmButton.Content = "Install"

                If NewInputDialog.ShowDialog() = True Then
                    Dim InputDialogResult As String = NewInputDialog.NewValueTextBox.Text
                    IswebMANMODCommandExecuted = False
                    WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/xmb.ps3/install.ps3?url=" + InputDialogResult)
                End If
            Else
                MsgBox("Please wait a couple seconds until WebView2 for webMAN MOD is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub HardRebootPS3_Click(sender As Object, e As RoutedEventArgs) Handles HardRebootPS3.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/reboot.ps3")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub InsertDisc_Click(sender As Object, e As RoutedEventArgs) Handles InsertDisc.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/insert.ps3")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub InstallPKGFromPS3HDD_Click(sender As Object, e As RoutedEventArgs) Handles InstallPKGFromPS3HDD.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                Dim NewInputDialog As New InputDialog With {.Title = "Install a PKG"}
                NewInputDialog.NewValueTextBox.Text = "/dev_hdd0/packages/Homebrew.pkg"
                NewInputDialog.InputDialogTitleTextBlock.Text = "Enter the full path to the .pkg file:"
                NewInputDialog.ConfirmButton.Content = "Install"

                If NewInputDialog.ShowDialog() = True Then
                    Dim InputDialogResult As String = NewInputDialog.NewValueTextBox.Text
                    IswebMANMODCommandExecuted = False
                    WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/install_ps3" + InputDialogResult)
                End If
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub InstallThemeFromPS3HDD_Click(sender As Object, e As RoutedEventArgs) Handles InstallThemeFromPS3HDD.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                Dim NewInputDialog As New InputDialog With {.Title = "Install a Theme"}
                NewInputDialog.NewValueTextBox.Text = "/dev_hdd0/Themes/THEME.p3t"
                NewInputDialog.InputDialogTitleTextBlock.Text = "Enter the full path to the .p3t file:"
                NewInputDialog.ConfirmButton.Content = "Install"

                If NewInputDialog.ShowDialog() = True Then
                    Dim InputDialogResult As String = NewInputDialog.NewValueTextBox.Text
                    IswebMANMODCommandExecuted = False
                    WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/install.ps3" + InputDialogResult)
                End If
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub OpenPS3WebBrowserURL_Click(sender As Object, e As RoutedEventArgs) Handles OpenPS3WebBrowserURL.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                Dim NewInputDialog As New InputDialog With {.Title = "Open PS3 Web Browser with URL"}
                NewInputDialog.NewValueTextBox.Text = ""
                NewInputDialog.InputDialogTitleTextBlock.Text = "Enter an URL to browse :"
                NewInputDialog.ConfirmButton.Content = "Open"

                If NewInputDialog.ShowDialog() = True Then
                    IswebMANMODCommandExecuted = False
                    Dim InputDialogResult As String = NewInputDialog.NewValueTextBox.Text
                    WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/browser.ps3?" + InputDialogResult)
                End If
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub OpenWebGUICelcius_Click(sender As Object, e As RoutedEventArgs) Handles OpenWebGUICelcius.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            Dim NewwebMANMODWebGUI As New PS3webMANBrowser() With {.ShowActivated = True, .WebMANAddress = "http://" & SharedConsoleAddress.Split(":"c)(0) & "/tempc.html"}
            NewwebMANMODWebGUI.Show()
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub OpenWebGUIFahrenheit_Click(sender As Object, e As RoutedEventArgs) Handles OpenWebGUIFahrenheit.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            Dim NewwebMANMODWebGUI As New PS3webMANBrowser() With {.ShowActivated = True, .WebMANAddress = "http://" & SharedConsoleAddress.Split(":"c)(0) & "/tempf.html"}
            NewwebMANMODWebGUI.Show()
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub OpenwebMANMODWebGUI_Click(sender As Object, e As RoutedEventArgs) Handles OpenwebMANMODWebGUI.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            Dim NewwebMANMODWebGUI As New PS3webMANBrowser() With {.ShowActivated = True, .WebMANAddress = "http://" & SharedConsoleAddress.Split(":"c)(0)}
            NewwebMANMODWebGUI.Show()
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub PlayDiscMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PlayDiscMenuItem.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/play.ps3")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub QuickRebootPS3_Click(sender As Object, e As RoutedEventArgs) Handles QuickRebootPS3.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/reboot.ps3?quick")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub RebootPS3UsingVSH_Click(sender As Object, e As RoutedEventArgs) Handles RebootPS3UsingVSH.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/reboot.ps3?vsh")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub ReloadGame_Click(sender As Object, e As RoutedEventArgs) Handles ReloadGame.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/xmb.ps3$reloadgame")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub RescanGamesRefreshXML_Click(sender As Object, e As RoutedEventArgs) Handles RescanGamesRefreshXML.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/refresh.ps3?xmb")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub RestartPS3_Click(sender As Object, e As RoutedEventArgs) Handles RestartPS3.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/restart.ps3")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub RestartPS3ShowMinVersion_Click(sender As Object, e As RoutedEventArgs) Handles RestartPS3ShowMinVersion.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/restart.ps3?min")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub RestartPS3WithContentScan_Click(sender As Object, e As RoutedEventArgs) Handles RestartPS3WithContentScan.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/restart.ps3?0")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub ShowSystemInfoOnPS3_Click(sender As Object, e As RoutedEventArgs) Handles ShowSystemInfoOnPS3.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/popup.ps3")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub ShutdownPS3_Click(sender As Object, e As RoutedEventArgs) Handles ShutdownPS3.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/shutdown.ps3")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub SoftRebootPS3_Click(sender As Object, e As RoutedEventArgs) Handles SoftRebootPS3.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/reboot.ps3?soft")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub ToggleInGameBGMusicPlayback_Click(sender As Object, e As RoutedEventArgs) Handles ToggleInGameBGMusicPlayback.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/sysbgm.ps3")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub ToggleVideoRecording_Click(sender As Object, e As RoutedEventArgs) Handles ToggleVideoRecording.Click
        If Not String.IsNullOrEmpty(SharedConsoleAddress) Then
            If IswebMANMODWebViewReady AndAlso IswebMANMODCommandExecuted Then
                IswebMANMODCommandExecuted = False
                WebMANWebView.CoreWebView2.Navigate("http://" & SharedConsoleAddress.Split(":"c)(0) & "/videorec.ps3")
            Else
                MsgBox("Please wait a couple seconds until WebView2 is ready.", MsgBoxStyle.Information, "webMAN MOD not ready")
            End If
        Else
            MsgBox("Please set your PS3 IP address in the Settings.", MsgBoxStyle.Information, "No IP Address")
        End If
    End Sub

    Private Sub WebMANWebView_CoreWebView2InitializationCompleted(sender As Object, e As CoreWebView2InitializationCompletedEventArgs) Handles WebMANWebView.CoreWebView2InitializationCompleted
        If e.IsSuccess Then
            IswebMANMODWebViewReady = True
        End If
    End Sub

    Private Sub WebMANWebView_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs) Handles WebMANWebView.NavigationCompleted
        If e.IsSuccess Then
            IswebMANMODCommandExecuted = True
        End If
    End Sub

    Private Sub CreateFolderStructure_Click(sender As Object, e As RoutedEventArgs) Handles CreateFolderStructure.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a destination path"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            Directory.CreateDirectory(FBD.SelectedPath + "\GAMES")
            Directory.CreateDirectory(FBD.SelectedPath + "\PS3ISO")
            Directory.CreateDirectory(FBD.SelectedPath + "\PSXISO")
            Directory.CreateDirectory(FBD.SelectedPath + "\PS2ISO")
            Directory.CreateDirectory(FBD.SelectedPath + "\PSPISO")
            Directory.CreateDirectory(FBD.SelectedPath + "\BDISO")
            Directory.CreateDirectory(FBD.SelectedPath + "\DVDISO")
            Directory.CreateDirectory(FBD.SelectedPath + "\ROMS")
            Directory.CreateDirectory(FBD.SelectedPath + "\GAMEI")
            Directory.CreateDirectory(FBD.SelectedPath + "\PKG")
            Directory.CreateDirectory(FBD.SelectedPath + "\MOVIES")
            Directory.CreateDirectory(FBD.SelectedPath + "\MUSIC")
            Directory.CreateDirectory(FBD.SelectedPath + "\PICTURE")
            Directory.CreateDirectory(FBD.SelectedPath + "\REDKEY")

            MsgBox("Directories created!", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub ManageVirtualFolders_Click(sender As Object, e As RoutedEventArgs) Handles ManageVirtualFolders.Click
        Dim NewVirtualFolderManager As New PS3VirtualFolderManager() With {.ShowActivated = True}
        NewVirtualFolderManager.Show()
    End Sub

    Private Sub ShareASingleFolder_Click(sender As Object, e As RoutedEventArgs) Handles ShareASingleFolder.Click
        Select Case ShareASingleFolder.Header.ToString()
            Case "Share a single folder"
                If File.Exists(Environment.CurrentDirectory + "\Tools\ps3netsrv\ps3netsrv.exe") Then
                    Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select the folder you want to share"}
                    If FBD.ShowDialog() = Forms.DialogResult.OK Then
                        If MsgBox(FBD.SelectedPath + " will be shared using ps3netsrv. Continue ?", MsgBoxStyle.YesNo, "Please confirm sharing the selected folder") = MsgBoxResult.Yes Then

                            Dim NewArgs As String = ""
                            If FBD.SelectedPath.Length <= 3 Then
                                NewArgs = FBD.SelectedPath + "\"
                            Else
                                NewArgs = """" + FBD.SelectedPath + """"
                            End If

                            PS3NetSrvProcess = New Process() With {.EnableRaisingEvents = True, .StartInfo = New ProcessStartInfo With {
                                .Arguments = NewArgs,
                                .FileName = Environment.CurrentDirectory + "\Tools\ps3netsrv\ps3netsrv.exe"}}

                            PS3NetSrvProcess.Start()

                            If Dispatcher.CheckAccess() = False Then
                                Dispatcher.BeginInvoke(Sub() ShareASingleFolder.Header = "Stop sharing")
                            Else
                                ShareASingleFolder.Header = "Stop sharing"
                            End If

                        Else
                            Exit Sub
                        End If
                    End If
                Else
                    MsgBox("Could not find " + Environment.CurrentDirectory + "\Tools\ps3netsrv\ps3netsrv.exe", MsgBoxStyle.Critical, "Cannot share without ps3netsrv")
                End If
            Case "Stop sharing"
                If PS3NetSrvProcess IsNot Nothing Then
                    If PS3NetSrvProcess.HasExited = False Then

                        PS3NetSrvProcess.Kill()

                        If Dispatcher.CheckAccess() = False Then
                            Dispatcher.BeginInvoke(Sub() ShareASingleFolder.Header = "Share a single folder")
                        Else
                            ShareASingleFolder.Header = "Share a single folder"
                        End If

                    End If
                End If
        End Select
    End Sub

    Private Sub ShareManagedFolders_Click(sender As Object, e As RoutedEventArgs) Handles ShareManagedFolders.Click
        Select Case ShareManagedFolders.Header.ToString()
            Case "Share configured managed virtual folders"
                If File.Exists(Environment.CurrentDirectory + "\Tools\ps3netsrv\ps3netsrv.exe") Then

                    Directory.SetCurrentDirectory(Environment.CurrentDirectory + "\Tools\ps3netsrv")

                    PS3NetSrvProcess = New Process() With {.EnableRaisingEvents = True, .StartInfo = New ProcessStartInfo With {.Arguments = ".", .FileName = "ps3netsrv.exe"}}
                    PS3NetSrvProcess.Start()

                    Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory)

                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub() ShareManagedFolders.Header = "Stop sharing")
                    Else
                        ShareManagedFolders.Header = "Stop sharing"
                    End If
                Else
                    MsgBox("Could not find " + Environment.CurrentDirectory + "\Tools\ps3netsrv\ps3netsrv.exe", MsgBoxStyle.Critical, "Cannot share without ps3netsrv")
                End If
            Case "Stop sharing"
                If PS3NetSrvProcess IsNot Nothing Then
                    If PS3NetSrvProcess.HasExited = False Then

                        PS3NetSrvProcess.Kill()

                        If Dispatcher.CheckAccess() = False Then
                            Dispatcher.BeginInvoke(Sub() ShareASingleFolder.Header = "Share a single folder")
                        Else
                            ShareASingleFolder.Header = "Share a single folder"
                        End If

                    End If
                End If
        End Select
    End Sub

    Private Sub PS3NetSrvProcess_Exited(sender As Object, e As EventArgs) Handles PS3NetSrvProcess.Exited
        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub()
                                       ShareASingleFolder.Header = "Share a single folder"
                                       ShareManagedFolders.Header = "Share configured managed virtual folders"
                                   End Sub)
        Else
            ShareASingleFolder.Header = "Share a single folder"
            ShareManagedFolders.Header = "Share configured managed virtual folders"
        End If

        PS3NetSrvProcess.Dispose()
        PS3NetSrvProcess = Nothing
    End Sub

#End Region

End Class
