Imports System.ComponentModel
Imports System.IO
Imports System.Threading

Public Class PSVLibrary

    Dim WithEvents GameLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents CoverBrowser As New Forms.WebBrowser() With {.ScriptErrorsSuppressed = True}
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PS Vita files", .ShowActivated = True}

    Dim FoldersCount As Integer = 0
    Dim PKGCount As Integer = 0

    Dim URLs As New List(Of String)()
    Dim CurrentURL As Integer = 0

    'Selected game context menu
    Dim WithEvents NewContextMenu As New Controls.ContextMenu()
    Dim WithEvents CopyToMenuItem As New Controls.MenuItem() With {.Header = "Copy to", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}
    Dim WithEvents PKGInfoMenuItem As New Controls.MenuItem() With {.Header = "PKG Details", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/information-button.png", UriKind.Relative))}}
    Dim WithEvents PlayGameMenuItem As New Controls.MenuItem() With {.Header = "Play with vita3k", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/controller.png", UriKind.Relative))}}

    'Supplemental library menu items
    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load a backup folder"}
    Dim WithEvents LoadFTPMenuItem As New Controls.MenuItem() With {.Header = "Load installed backups over FTP"}
    Dim WithEvents LoadLibraryMenuItem As New Controls.MenuItem() With {.Header = "Show games library"}
    Dim WithEvents LoadDLFolderMenuItem As New Controls.MenuItem() With {.Header = "Open Downloads folder"}

    'Supplemental emulator menu item
    Dim WithEvents EMU_Settings As New Controls.MenuItem() With {.Header = "Vita3k Settings"}

    Private Sub PSVLibrary_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Set the controls in the shared library
        NewPSVMenu.GamesLView = GamesListView

        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As Controls.MenuItem = CType(NewPSVMenu.Items(0), Controls.MenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadLibraryMenuItem)
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem)

        'Add the new PKG Browser
        Dim PKGDownloaderMenuItem As New Controls.MenuItem() With {.Header = "PKG Browser & Downloader"}
        AddHandler PKGDownloaderMenuItem.Click, AddressOf OpenPKGBrowser
        NewPSVMenu.Items.Add(PKGDownloaderMenuItem)

        NewContextMenu.Items.Add(CopyToMenuItem)
        NewContextMenu.Items.Add(PKGInfoMenuItem)
        NewContextMenu.Items.Add(PlayGameMenuItem)
        GamesListView.ContextMenu = NewContextMenu

        'Add supplemental emulator menu item
        If File.Exists(Environment.CurrentDirectory + "\Emulators\vita3k\Vita3K.exe") Then
            NewPSVMenu.Items.Add(EMU_Settings)
        End If
    End Sub

#Region "Game Loader"

    Private Sub GameLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles GameLoaderWorker.DoWork
        Try
            'PSV encrypted/decrypted folders
            For Each Game In Directory.GetFiles(e.Argument.ToString(), "*.sfo", SearchOption.AllDirectories)
                Dim NewPSVGame As New PSVGame() With {.GridWidth = 125, .GridHeight = 175, .ImageWidth = 100, .ImageHeight = 128}
                Using SFOReader As New Process()
                    SFOReader.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\sfo.exe"
                    SFOReader.StartInfo.Arguments = """" + Game + """"
                    SFOReader.StartInfo.RedirectStandardOutput = True
                    SFOReader.StartInfo.UseShellExecute = False
                    SFOReader.StartInfo.CreateNoWindow = True
                    SFOReader.Start()

                    Dim OutputReader As StreamReader = SFOReader.StandardOutput
                    Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                    If ProcessOutput.Length > 0 Then

                        'Load game infos
                        For Each Line In ProcessOutput
                            If Line.StartsWith("TITLE=") Then
                                NewPSVGame.GameTitle = Utils.CleanTitle(Line.Split("="c)(1).Trim(""""c).Trim())
                            ElseIf Line.StartsWith("TITLE_ID=") Then
                                NewPSVGame.GameID = Line.Split("="c)(1).Trim(""""c).Trim()
                            ElseIf Line.StartsWith("CATEGORY=") Then
                                NewPSVGame.GameCategory = PSVGame.GetCategory(Line.Split("="c)(1).Trim(""""c))
                            ElseIf Line.StartsWith("APP_VER=") Then
                                NewPSVGame.GameAppVer = Line.Split("="c)(1).Trim(""""c).Trim()
                            ElseIf Line.StartsWith("PSP2_DISP_VER=") Then
                                NewPSVGame.GameRequiredFW = Line.Split("="c)(1).Trim(""""c).Trim()
                            ElseIf Line.StartsWith("VERSION=") Then
                                NewPSVGame.GameVer = Line.Split("="c)(1).Trim(""""c).Trim()
                            ElseIf Line.StartsWith("CONTENT_ID=") Then
                                NewPSVGame.ContentID = Line.Split("="c)(1).Trim(""""c).Trim()
                            End If
                        Next
                        Dim PSVGAMEFolder As String = Path.GetDirectoryName(Directory.GetParent(Game).FullName)
                        Dim PSVGAMEFolderSize As Long = Utils.DirSize(PSVGAMEFolder, True)
                        NewPSVGame.GameSize = FormatNumber(PSVGAMEFolderSize / 1073741824, 2) + " GB"
                        NewPSVGame.GameFolderPath = PSVGAMEFolder
                        NewPSVGame.GameFileType = PSVGame.GameFileTypes.Backup

                        If Not String.IsNullOrEmpty(NewPSVGame.GameID) Then
                            NewPSVGame.GameRegion = PSVGame.GetGameRegion(NewPSVGame.GameID)
                            If Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png") Then
                                If Dispatcher.CheckAccess() = False Then
                                    Dispatcher.BeginInvoke(Sub()
                                                               Dim TempBitmapImage = New BitmapImage()
                                                               TempBitmapImage.BeginInit()
                                                               TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                               TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                               TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png", UriKind.RelativeOrAbsolute)
                                                               TempBitmapImage.EndInit()
                                                               NewPSVGame.GameCoverSource = TempBitmapImage
                                                           End Sub)
                                Else
                                    Dim TempBitmapImage = New BitmapImage()
                                    TempBitmapImage.BeginInit()
                                    TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                    TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                    TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png", UriKind.RelativeOrAbsolute)
                                    TempBitmapImage.EndInit()
                                    NewPSVGame.GameCoverSource = TempBitmapImage
                                End If
                            End If
                        End If

                        Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                        Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading folder " + (NewLoadingWindow.LoadProgressBar.Value - PKGCount).ToString + " of " + FoldersCount.ToString())

                        Thread.Sleep(250)

                        'Add to the ListView
                        If GamesListView.Dispatcher.CheckAccess() = False Then
                            GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPSVGame))
                        Else
                            GamesListView.Items.Add(NewPSVGame)
                        End If
                    End If
                End Using
            Next

            'PSV PSN pkgs
            For Each GamePKG In Directory.GetFiles(e.Argument.ToString(), "*.pkg", SearchOption.AllDirectories)
                Dim NewPSVGame As New PSVGame() With {.GridWidth = 125, .GridHeight = 175, .ImageWidth = 100, .ImageHeight = 128}
                Dim GameInfo As New FileInfo(GamePKG)

                Using SFOReader As New Process()
                    SFOReader.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\PSN_get_pkg_info.exe"
                    SFOReader.StartInfo.Arguments = """" + GamePKG + """"
                    SFOReader.StartInfo.RedirectStandardOutput = True
                    SFOReader.StartInfo.UseShellExecute = False
                    SFOReader.StartInfo.CreateNoWindow = True
                    SFOReader.Start()
                    Dim OutputReader As StreamReader = SFOReader.StandardOutput
                    Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                    If ProcessOutput.Length > 0 Then

                        'Load game infos
                        For Each Line In ProcessOutput
                            If Line.StartsWith("Title:") Then
                                NewPSVGame.GameTitle = Utils.CleanTitle(Line.Split(":"c)(1).Trim(""""c).Trim())
                            ElseIf Line.StartsWith("Title ID:") Then
                                NewPSVGame.GameID = Line.Split(":"c)(1).Trim(""""c).Trim()
                            ElseIf Line.StartsWith("NPS Type:") Then
                                NewPSVGame.GameCategory = Line.Split(":"c)(1).Trim(""""c).Trim()
                            ElseIf Line.StartsWith("App Ver:") Then
                                NewPSVGame.GameAppVer = FormatNumber(Line.Split(":"c)(1).Trim(""""c), 2).Replace(","c, "").Insert(1, ".")
                            ElseIf Line.StartsWith("Min FW:") Then
                                NewPSVGame.GameRequiredFW = FormatNumber(Line.Split(":"c)(1).Trim(""""c), 2).Replace(","c, "").Replace("."c, "").Insert(2, ".")
                            ElseIf Line.StartsWith("Version:") Then
                                NewPSVGame.GameVer = FormatNumber(Line.Split(":"c)(1).Trim(""""c), 2).Replace(","c, "").Insert(1, ".")
                            ElseIf Line.StartsWith("Content ID:") Then
                                NewPSVGame.ContentID = Line.Split(":"c)(1).Trim(""""c).Trim()
                            ElseIf Line.StartsWith("Region:") Then
                                NewPSVGame.GameRegion = Line.Split(":"c)(1).Trim(""""c).Trim()
                            End If
                        Next
                        NewPSVGame.GameSize = FormatNumber(GameInfo.Length / 1073741824, 2) + " GB"
                        NewPSVGame.GameFilePath = GamePKG
                        NewPSVGame.GameFileType = PSVGame.GameFileTypes.PKG
                        If Not String.IsNullOrEmpty(NewPSVGame.GameID) Then
                            If Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png") Then
                                If Dispatcher.CheckAccess() = False Then
                                    Dispatcher.BeginInvoke(Sub()
                                                               Dim TempBitmapImage = New BitmapImage()
                                                               TempBitmapImage.BeginInit()
                                                               TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                               TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                               TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png", UriKind.RelativeOrAbsolute)
                                                               TempBitmapImage.EndInit()
                                                               NewPSVGame.GameCoverSource = TempBitmapImage
                                                           End Sub)
                                Else
                                    Dim TempBitmapImage = New BitmapImage()
                                    TempBitmapImage.BeginInit()
                                    TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                    TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                    TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png", UriKind.RelativeOrAbsolute)
                                    TempBitmapImage.EndInit()
                                    NewPSVGame.GameCoverSource = TempBitmapImage
                                End If
                            End If
                        End If

                        Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                        Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading PKG " + (NewLoadingWindow.LoadProgressBar.Value - FoldersCount).ToString + " of " + PKGCount.ToString())

                        Thread.Sleep(250)

                        'Add to the ListView
                        If GamesListView.Dispatcher.CheckAccess() = False Then
                            GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPSVGame))
                        Else
                            GamesListView.Items.Add(NewPSVGame)
                        End If
                    End If
                End Using
            Next
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub

    Private Sub GameLoaderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles GameLoaderWorker.RunWorkerCompleted
        NewLoadingWindow.Close()
        GamesListView.Items.Refresh()
    End Sub

#End Region

#Region "Contextmenu Actions"

    Private Sub CopyToMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles CopyToMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPSVGame As PSVGame = CType(GamesListView.SelectedItem, PSVGame)
            Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Where do you want to save the selected game ?"}

            If FBD.ShowDialog() = Forms.DialogResult.OK Then
                Dim NewCopyWindow As New CopyWindow() With {.ShowActivated = True,
                    .WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    .BackupDestinationPath = FBD.SelectedPath + "\",
                    .Title = "Copying " + SelectedPSVGame.GameTitle + " to " + FBD.SelectedPath + "\" + Path.GetFileName(SelectedPSVGame.GameFilePath)}

                If SelectedPSVGame.GameFileType = PSVGame.GameFileTypes.Backup Then
                    NewCopyWindow.BackupPath = SelectedPSVGame.GameFolderPath
                ElseIf SelectedPSVGame.GameFileType = PSVGame.GameFileTypes.PKG Then
                    NewCopyWindow.BackupPath = SelectedPSVGame.GameFilePath
                End If

                If NewCopyWindow.ShowDialog() = True Then
                    MsgBox("Game copied with success !", MsgBoxStyle.Information, "Completed")
                End If
            End If

        End If
    End Sub

    Private Sub PKGInfoMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PKGInfoMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPSVGame As PSVGame = CType(GamesListView.SelectedItem, PSVGame)
            Dim NewPKGInfo As New PKGInfo() With {.SelectedPKG = SelectedPSVGame.GameFilePath, .Console = "PSV"}
            NewPKGInfo.Show()
        End If
    End Sub

    Private Sub PlayGameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PlayGameMenuItem.Click
        If File.Exists(Environment.CurrentDirectory + "\Emulators\vita3k\Vita3K.exe") Then
            If GamesListView.SelectedItem IsNot Nothing Then
                Dim SelectedPSVGame As PSVGame = CType(GamesListView.SelectedItem, PSVGame)

                'Check if vita3k has been configured before
                If Not File.Exists(Environment.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
                    If MsgBox("In order to play PS Vita games using vita3k you need to finish the initial setup." + vbCrLf + "Do you want to start the initial setup now ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                        If File.Exists(Environment.CurrentDirectory + "\Emulators\vita3k\Vita3K.exe") Then
                            Process.Start(Environment.CurrentDirectory + "\Emulators\vita3k\Vita3K.exe")
                        Else
                            MsgBox("vita3k not found at " + Environment.CurrentDirectory + "\Emulators\vita3k", MsgBoxStyle.Critical, "Error")
                            Exit Sub
                        End If
                    Else
                        MsgBox("Aborting", MsgBoxStyle.Critical)
                        Exit Sub
                    End If
                Else
                    'Read vita3k config & check if initial setup done
                    Dim ConfigLines() As String = File.ReadAllLines(Environment.CurrentDirectory + "\Emulators\vita3k\config.yml", Text.Encoding.UTF8)
                    If Not ConfigLines(1) = "initial-setup: true" Then
                        If MsgBox("In order to play PS Vita games using vita3k you need to finish the initial setup." + vbCrLf + "Do you want to start the initial setup now ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                            If File.Exists(Environment.CurrentDirectory + "\Emulators\vita3k\Vita3K.exe") Then
                                Process.Start(Environment.CurrentDirectory + "\Emulators\vita3k\Vita3K.exe")
                            Else
                                MsgBox("vita3k not found at " + Environment.CurrentDirectory + "\Emulators\vita3k", MsgBoxStyle.Critical, "Error")
                                Exit Sub
                            End If
                        Else
                            MsgBox("Aborting", MsgBoxStyle.Critical)
                            Exit Sub
                        End If
                    Else
                        'Proceed

                        'Get the vita3k pref path (emulator path)
                        Dim Vita3kPrefPath As String = ""
                        For Each ConfigLine As String In ConfigLines
                            If ConfigLine.Contains("pref-path:") Then
                                Vita3kPrefPath = ConfigLine.Replace("pref-path: ", "")
                                Exit For
                            End If
                        Next

                        If String.IsNullOrEmpty(Vita3kPrefPath) Then
                            MsgBox("Error reading vita3k config.yaml", MsgBoxStyle.Critical, "Error")
                            Exit Sub
                        Else

                            Dim FontPackageIntalled As Boolean = False
                            Dim FirmwareIntalled As Boolean = False

                            MsgBox("Checking if " + Vita3kPrefPath + "sa0\data\font\pvf\ltn0.pvf exists.")

                            'Check if firmware fonts package is installed
                            If Not File.Exists(Vita3kPrefPath + "sa0\data\font\pvf\ltn0.pvf") Then
                                If MsgBox("PS Vita Firmware Fonts Package is not installed but recommended." + vbCrLf +
                                          "Do you want to intall a PSP2UPDAT.PUP file now ?" + vbCrLf +
                                          "If you select YES then close vita3k after the firmware installation if still open.", MsgBoxStyle.YesNo, "Firmware Fonts Package not installed") = MsgBoxResult.Yes Then

                                    Dim OFD As New Forms.OpenFileDialog() With {.Title = "Select the PSP2UPDAT.PUP file to install.", .Filter = "PUP File (*.PUP)|*.PUP", .Multiselect = False}
                                    If OFD.ShowDialog() = Forms.DialogResult.OK Then

                                        'Set up rpcs3 to install the selected PS3 firmware
                                        Dim EmulatorLauncherStartInfo As New ProcessStartInfo()
                                        Dim EmulatorLauncher As New Process() With {.StartInfo = EmulatorLauncherStartInfo}
                                        EmulatorLauncherStartInfo.FileName = Environment.CurrentDirectory + "\Emulators\vita3k\Vita3K.exe"
                                        EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Environment.CurrentDirectory + "\Emulators\vita3k\Vita3K.exe")
                                        EmulatorLauncherStartInfo.Arguments = "--firmware """ + OFD.FileName + """"
                                        EmulatorLauncher.Start()
                                        EmulatorLauncher.WaitForExit()
                                        EmulatorLauncher.Dispose()

                                        FontPackageIntalled = True
                                    Else
                                        MsgBox("No PSP2UPDAT.PUP file specified.", MsgBoxStyle.Critical, "Error")
                                        Exit Sub
                                    End If

                                End If
                            Else
                                FontPackageIntalled = True
                            End If

                            'Check if PS Vita firmware is installed
                            If Not File.Exists(Vita3kPrefPath + "os0\kd\registry.db0") Then
                                If MsgBox("PS Vita Firmware is not installed but required to launch games." + vbCrLf +
                                          "Do you want to intall a PSVUPDAT.PUP file now ?" + vbCrLf +
                                          "If you select YES then close vita3k after the firmware installation if still open.", MsgBoxStyle.YesNo, "PS Vita Firmware not intalled") = MsgBoxResult.Yes Then

                                    Dim OFD As New Forms.OpenFileDialog() With {.Title = "Select the PSVUPDAT.PUP file to install.", .Filter = "PUP File (*.PUP)|*.PUP", .Multiselect = False}
                                    If OFD.ShowDialog() = Forms.DialogResult.OK Then

                                        'Set up rpcs3 to install the selected PS3 firmware
                                        Dim EmulatorLauncherStartInfo As New ProcessStartInfo()
                                        Dim EmulatorLauncher As New Process() With {.StartInfo = EmulatorLauncherStartInfo}
                                        EmulatorLauncherStartInfo.FileName = Environment.CurrentDirectory + "\Emulators\vita3k\Vita3K.exe"
                                        EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Environment.CurrentDirectory + "\Emulators\vita3k\Vita3K.exe")
                                        EmulatorLauncherStartInfo.Arguments = "--firmware """ + OFD.FileName + """"
                                        EmulatorLauncher.Start()
                                        EmulatorLauncher.WaitForExit()
                                        EmulatorLauncher.Dispose()

                                        FirmwareIntalled = True
                                    Else
                                        MsgBox("No PSVUPDAT.PUP file specified.", MsgBoxStyle.Critical, "Error")
                                        Exit Sub
                                    End If
                                Else
                                    Exit Sub
                                End If
                            Else
                                FirmwareIntalled = True
                            End If

                            'Proceed if PS Vita Firmware is installed
                            If FirmwareIntalled Then

                                If MsgBox("Start " + SelectedPSVGame.GameTitle + " using vita3k ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then
                                    Dim EmulatorLauncherStartInfo As New ProcessStartInfo()
                                    Dim EmulatorLauncher As New Process() With {.StartInfo = EmulatorLauncherStartInfo}
                                    EmulatorLauncherStartInfo.FileName = Environment.CurrentDirectory + "\Emulators\vita3k\Vita3K.exe"
                                    EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Environment.CurrentDirectory + "\Emulators\vita3k\Vita3K.exe")

                                    Select Case SelectedPSVGame.GameFileType
                                        Case PSVGame.GameFileTypes.PKG

                                            'Check if game is already installed
                                            If Not Directory.Exists(Vita3kPrefPath + "ux0\app\" + SelectedPSVGame.GameID) Then

                                                If MsgBox("Playing games in PKG format will require you to install them first with the zRIF key." + vbCrLf + vbCrLf +
                                                          "The game will start automatically after the installation." + vbCrLf + vbCrLf +
                                                          "Do you want to continue ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then

                                                    'Get zRIF using the online or offline database or user input
                                                    Dim RequiredzRIF As String = Utils.GetzRIF(SelectedPSVGame.ContentID)
                                                    If String.IsNullOrEmpty(RequiredzRIF) Then
                                                        RequiredzRIF = InputBox("Enter the zRIF Key string for the selected PKG:", "zRIF required", "")
                                                    End If

                                                    EmulatorLauncherStartInfo.Arguments = "--pkg """ + SelectedPSVGame.GameFilePath + """ --zrif " + RequiredzRIF
                                                    EmulatorLauncher.Start()
                                                    EmulatorLauncher.WaitForExit()
                                                    EmulatorLauncher.Dispose()

                                                    'Start after installation
                                                    EmulatorLauncher = New Process() With {.StartInfo = EmulatorLauncherStartInfo}
                                                    EmulatorLauncherStartInfo.Arguments = "-r " + SelectedPSVGame.GameID
                                                    EmulatorLauncher.Start()
                                                End If
                                            Else
                                                'Start an already installed game
                                                EmulatorLauncherStartInfo.Arguments = "-r " + SelectedPSVGame.GameID
                                                EmulatorLauncher.Start()
                                            End If

                                        Case PSVGame.GameFileTypes.Backup
                                            'Game will be decrypted automatically by vita3k if the selected game folder is still encrypted
                                            EmulatorLauncherStartInfo.Arguments = """" + SelectedPSVGame.GameFolderPath + """"
                                            EmulatorLauncher.Start()
                                    End Select

                                End If

                            End If

                        End If

                    End If
                End If

            End If
        Else
            MsgBox("Cannot start vita3k." + vbCrLf + "Emulator pack is not installed.", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

    Private Sub GamesListView_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles GamesListView.ContextMenuOpening
        NewContextMenu.Items.Clear()

        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPSVGame As PSVGame = CType(GamesListView.SelectedItem, PSVGame)

            NewContextMenu.Items.Add(CopyToMenuItem)
            NewContextMenu.Items.Add(PlayGameMenuItem)

            If SelectedPSVGame.GameFileType = PS3Game.GameFileTypes.PKG Then
                NewContextMenu.Items.Add(PKGInfoMenuItem)
            End If
        End If
    End Sub

    Private Sub GamesListView_ContextMenuClosing(sender As Object, e As ContextMenuEventArgs) Handles GamesListView.ContextMenuClosing
        NewContextMenu.Items.Clear()
    End Sub

#End Region

#Region "Menu Actions"

    Private Sub LoadFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadFolderMenuItem.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Select your PSV backup folder"}

        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            PKGCount = Directory.GetFiles(FBD.SelectedPath, "*.pkg", SearchOption.AllDirectories).Count
            FoldersCount = Directory.GetFiles(FBD.SelectedPath, "*.sfo", SearchOption.AllDirectories).Count

            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS Vita files", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = PKGCount + FoldersCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + (PKGCount + FoldersCount).ToString()
            NewLoadingWindow.Show()

            GameLoaderWorker.RunWorkerAsync(FBD.SelectedPath)
        End If
    End Sub

    Private Sub LoadDLFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadDLFolderMenuItem.Click
        If Directory.Exists(Environment.CurrentDirectory + "\Downloads") Then
            Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
        End If
    End Sub

#End Region

    Private Sub OpenPKGBrowser(sender As Object, e As RoutedEventArgs)
        Dim NewPKGBrowser As New PKGBrowser() With {.Console = "PSV", .ShowActivated = True}
        NewPKGBrowser.Show()
    End Sub

    Private Sub GamesListView_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs) Handles GamesListView.PreviewMouseWheel
        Dim OpenWindowsListViewScrollViewer As ScrollViewer = Utils.FindScrollViewer(GamesListView)
        Dim HorizontalOffset As Double = OpenWindowsListViewScrollViewer.HorizontalOffset
        OpenWindowsListViewScrollViewer.ScrollToHorizontalOffset(HorizontalOffset - (e.Delta / 100))
        e.Handled = True
    End Sub

    Private Sub GamesListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles GamesListView.SelectionChanged
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPSVGame As PSVGame = CType(GamesListView.SelectedItem, PSVGame)

            GameTitleTextBlock.Text = SelectedPSVGame.GameTitle
            GameIDTextBlock.Text = "Title ID: " & SelectedPSVGame.GameID
            GameContentIDTextBlock.Text = "Content ID: " & SelectedPSVGame.ContentID
            GameRegionTextBlock.Text = "Region: " & SelectedPSVGame.GameRegion
            GameVersionTextBlock.Text = "Game Version: " & SelectedPSVGame.GameVer
            GameAppVersionTextBlock.Text = "Application Version: " & SelectedPSVGame.GameAppVer
            GameCategoryTextBlock.Text = "Category: " & SelectedPSVGame.GameCategory
            GameSizeTextBlock.Text = "Size: " & SelectedPSVGame.GameSize
            GameRequiredFirmwareTextBlock.Text = "Required Firmware: " & SelectedPSVGame.GameRequiredFW

            GameBackupTypeTextBlock.Text = "Backup Type: " & SelectedPSVGame.GameFileType.ToString()

            If Not String.IsNullOrEmpty(SelectedPSVGame.GameFilePath) Then
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " & New DirectoryInfo(Path.GetDirectoryName(SelectedPSVGame.GameFilePath)).Name
            Else
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " & New DirectoryInfo(SelectedPSVGame.GameFolderPath).Name
            End If

        End If
    End Sub

    Private Sub EMU_Settings_Click(sender As Object, e As RoutedEventArgs) Handles EMU_Settings.Click
        Dim NewPSVEmulatorSettingsWindow As New PSVEmulatorSettings() With {.ShowActivated = True}
        NewPSVEmulatorSettingsWindow.Show()
    End Sub

End Class
