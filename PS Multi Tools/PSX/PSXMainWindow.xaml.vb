Imports System.IO
Imports System.Net
Imports System.Windows.Media.Animation
Imports System.Windows.Threading
Imports PS_Multi_Tools.Structures
Imports PS_Multi_Tools.Utils

Public Class PSXMainWindow

    Private MountedDrive As MountedPSXDrive
    Private WNBDClientPath As String = ""

    Private WithEvents ConnectDelay As New DispatcherTimer With {.Interval = TimeSpan.FromSeconds(2)}
    Private WithEvents ContentDownloader As New WebClient()

    Private Sub NewMainWindow_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        Title = "PSX XMB Manager"

        If Not Directory.Exists(My.Computer.FileSystem.CurrentDirectory + "\Projects") Then
            'Set up a projects directory to save all created projects
            Directory.CreateDirectory(My.Computer.FileSystem.CurrentDirectory + "\Projects")
        Else
            'Load saved projects
            For Each SavedProject In Directory.GetFiles(My.Computer.FileSystem.CurrentDirectory + "\Projects", "*.CFG")
                Dim NewCBProjectItem As New ComboBoxProjectItem() With {.ProjectFile = Path.GetFullPath(SavedProject), .ProjectName = Path.GetFileNameWithoutExtension(SavedProject)}
                Dim ProjectState As String = File.ReadAllLines(SavedProject)(5).Split("="c)(1)

                If ProjectState = "FALSE" Then
                    ProjectListComboBox.Items.Add(NewCBProjectItem)
                Else
                    ProjectListComboBox.Items.Add(NewCBProjectItem)
                    PreparedProjectsComboBox.Items.Add(NewCBProjectItem)
                End If
            Next
        End If

        'Set DisplayMemberPath
        ProjectListComboBox.DisplayMemberPath = "ProjectName"
        PreparedProjectsComboBox.DisplayMemberPath = "ProjectName"

        'Set wnbd-client
        If File.Exists(My.Computer.FileSystem.SpecialDirectories.ProgramFiles + "\Ceph\bin\wnbd-client.exe") Then
            WNBDClientPath = My.Computer.FileSystem.SpecialDirectories.ProgramFiles + "\Ceph\bin\wnbd-client.exe"
        ElseIf File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\wnbd-client.exe") Then
            WNBDClientPath = My.Computer.FileSystem.CurrentDirectory + "\Tools\wnbd-client.exe"
        End If

        'Check if NBD driver is installed
        If Not String.IsNullOrEmpty(WNBDClientPath) Then
            Using WNBDClient As New Process()
                WNBDClient.StartInfo.FileName = WNBDClientPath
                WNBDClient.StartInfo.Arguments = "-v"
                WNBDClient.StartInfo.RedirectStandardOutput = True
                WNBDClient.StartInfo.UseShellExecute = False
                WNBDClient.StartInfo.CreateNoWindow = True
                WNBDClient.Start()

                Dim OutputReader As StreamReader = WNBDClient.StandardOutput
                Dim ProcessOutput As String = OutputReader.ReadToEnd()
                Dim SplittedOutput As String() = ProcessOutput.Split({vbCrLf}, StringSplitOptions.None)

                Dim NBDDriverVersion As String

                If Not SplittedOutput(2).Trim() = "" Then
                    NBDDriverVersion = SplittedOutput(2).Trim().Split(":"c)(1).Trim()
                    NBDDriverVersionLabel.Text = NBDDriverVersion
                    NBDDriverVersionLabel.Foreground = Brushes.Green
                Else
                    NBDDriverVersionLabel.Text = "Not installed"
                    NBDDriverVersionLabel.Foreground = Brushes.Red
                End If
            End Using

            'Check if NBD is connected or if the HDD is connected locally
            Dim ConnectedNBDDriveName As String = IsNBDConnected(WNBDClientPath)
            If Not String.IsNullOrEmpty(ConnectedNBDDriveName) Then 'NBD
                MountedDrive.NBDDriveName = ConnectedNBDDriveName

                If PSXIPTextBox.Dispatcher.CheckAccess() = False Then
                    PSXIPTextBox.Dispatcher.BeginInvoke(Sub()
                                                            PSXIPTextBox.Text = GetConnectedNBDIP(WNBDClientPath, ConnectedNBDDriveName)
                                                        End Sub)
                Else
                    PSXIPTextBox.Text = GetConnectedNBDIP(WNBDClientPath, ConnectedNBDDriveName)
                End If

                'Get HDL Drive Name
                Dim HDLDriveName As String = GetHDLDriveName()
                If Not String.IsNullOrEmpty(HDLDriveName) Then

                    'Update UI
                    If MountStatusLabel.CheckAccess() = False Then
                        MountStatusLabel.Dispatcher.BeginInvoke(Sub()
                                                                    MountStatusLabel.Text = "On " + HDLDriveName
                                                                    MountStatusLabel.Foreground = Brushes.Green
                                                                End Sub)
                    Else
                        MountStatusLabel.Text = "On " + HDLDriveName
                        MountStatusLabel.Foreground = Brushes.Green
                    End If

                    MountedDrive.HDLDriveName = HDLDriveName
                End If

                MountedDrive.DriveID = GetHDDID()

                InstallProjectButton.IsEnabled = True
                NBDConnectionLabel.Text = "Connected"
                NBDConnectionLabel.Foreground = Brushes.Green
                ConnectButton.Content = "Disconnect"

            ElseIf Not String.IsNullOrEmpty(IsLocalHDDConnected()) Then 'Local HDD
                Dim ConnectedLocalHDDDriveName As String = IsLocalHDDConnected()
                MountStatusLabel.Text = "on " + ConnectedLocalHDDDriveName
                MountStatusLabel.Foreground = Brushes.Green
                MountedDrive.HDLDriveName = ConnectedLocalHDDDriveName

                MountedDrive.DriveID = GetHDDID()

                InstallProjectButton.IsEnabled = True
                NBDConnectionStatusLabel.Text = "Local Connection:"
                NBDConnectionLabel.Text = "Connected"

                EnterIPLabel.Text = "Local PS2/PSX HDD detected & connected."
                EnterIPLabel.TextAlignment = TextAlignment.Center
                ConnectButton.IsEnabled = False
                PSXIPTextBox.IsEnabled = False
                PSXIPTextBox.Text = "Local Connection"
                ConnectButton.Foreground = Brushes.Black

                NBDConnectionLabel.Foreground = Brushes.Green
                ConnectButton.Content = "Disabled"
            End If
        End If

        'Check if Dokan driver is installed
        If Directory.Exists(My.Computer.FileSystem.SpecialDirectories.ProgramFiles + "\Dokan") Then
            Dim DokanLibraryFolder As String = ""
            For Each Folder In Directory.GetDirectories(My.Computer.FileSystem.SpecialDirectories.ProgramFiles + "\Dokan")
                Dim FolderInfo As New DirectoryInfo(Folder)
                If FolderInfo.Name.Contains("DokanLibrary") Or FolderInfo.Name.Contains("Dokan Library") Then
                    DokanLibraryFolder = Folder
                    Exit For
                End If
            Next
            If Not String.IsNullOrEmpty(DokanLibraryFolder) Then
                'Check if NBD driver is installed
                Using DokanCTL As New Process()
                    DokanCTL.StartInfo.FileName = DokanLibraryFolder + "\dokanctl.exe"
                    DokanCTL.StartInfo.Arguments = "/v"
                    DokanCTL.StartInfo.RedirectStandardOutput = True
                    DokanCTL.StartInfo.UseShellExecute = False
                    DokanCTL.StartInfo.CreateNoWindow = True
                    DokanCTL.Start()

                    Dim OutputReader As StreamReader = DokanCTL.StandardOutput
                    Dim ProcessOutput As String = OutputReader.ReadToEnd()
                    Dim SplittedOutput As String() = ProcessOutput.Split({vbCrLf}, StringSplitOptions.None)

                    Dim DokanVersion As String = ""
                    Dim DokanDriverVersion As String = ""

                    If Not SplittedOutput(2).Trim() = "" Then
                        DokanVersion = SplittedOutput(2).Trim().Split(":"c)(1).Trim()
                        DokanDriverVersion = SplittedOutput(3).Trim().Split(":"c)(1).Trim()

                        DokanDriverVersionLabel.Text = "Library: " + DokanVersion + " - Driver: " + DokanDriverVersion
                        DokanDriverVersionLabel.Foreground = Brushes.Green
                    End If
                End Using
            End If
        End If
    End Sub

    Private Sub ConnectButton_Click(sender As Object, e As RoutedEventArgs) Handles ConnectButton.Click
        If ConnectButton.Content.ToString = "Connect" Then

            Using WNBDConnectClient As New Process()
                If File.Exists(My.Computer.FileSystem.SpecialDirectories.ProgramFiles + "\Ceph\bin\wnbd-client.exe") Then
                    WNBDConnectClient.StartInfo.FileName = My.Computer.FileSystem.SpecialDirectories.ProgramFiles + "\Ceph\bin\wnbd-client.exe"
                Else
                    WNBDConnectClient.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\wnbd-client.exe"
                End If

                WNBDConnectClient.StartInfo.Arguments = "map PSXHDD " + PSXIPTextBox.Text
                WNBDConnectClient.StartInfo.UseShellExecute = False
                WNBDConnectClient.StartInfo.CreateNoWindow = True
                WNBDConnectClient.Start()
            End Using

            ConnectDelay.Start()

        ElseIf ConnectButton.Content.ToString = "Disconnect" Then

            Using WNBDProcess As New Process()
                If File.Exists(My.Computer.FileSystem.SpecialDirectories.ProgramFiles + "\Ceph\bin\wnbd-client.exe") Then
                    WNBDProcess.StartInfo.FileName = My.Computer.FileSystem.SpecialDirectories.ProgramFiles + "\Ceph\bin\wnbd-client.exe"
                Else
                    WNBDProcess.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\wnbd-client.exe"
                End If

                WNBDProcess.StartInfo.Arguments = "unmap PSXHDD"
                WNBDProcess.StartInfo.CreateNoWindow = True
                WNBDProcess.Start()
            End Using

            InstallProjectButton.IsEnabled = True
            NBDConnectionLabel.Text = "Disconnected"
            NBDConnectionLabel.Foreground = Brushes.Red
            MountStatusLabel.Text = "Not mounted"
            MountStatusLabel.Foreground = Brushes.Orange
            ConnectButton.Content = "Connect"

            MsgBox("Your PSX HDD is now disconnected." + vbCrLf + "You can now safely close the NBD server.", MsgBoxStyle.Information)
        End If
    End Sub

    Public Sub ReloadProjects()
        ProjectListComboBox.Items.Clear()
        PreparedProjectsComboBox.Items.Clear()

        If Directory.Exists(My.Computer.FileSystem.CurrentDirectory + "\Projects") Then
            'Load saved projects
            For Each SavedProject In Directory.GetFiles(My.Computer.FileSystem.CurrentDirectory + "\Projects", "*.CFG")
                Dim NewCBProjectItem As New ComboBoxProjectItem() With {.ProjectFile = SavedProject, .ProjectName = Path.GetFileNameWithoutExtension(SavedProject)}
                Dim ProjectState As String = File.ReadAllLines(SavedProject)(5).Split("="c)(1)

                If ProjectState = "FALSE" Then
                    ProjectListComboBox.Items.Add(NewCBProjectItem)
                Else
                    ProjectListComboBox.Items.Add(NewCBProjectItem)
                    PreparedProjectsComboBox.Items.Add(NewCBProjectItem)
                End If
            Next
        Else
            MsgBox("Could not find the Projects directory at " + My.Computer.FileSystem.CurrentDirectory + "\Projects", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

#Region "Menu"

    Private Sub StartMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles StartMenuItem.Click
        'Switch to the StartGrid
        StartMenuItem.Background = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#FF004671"), Color))
        ProjectsMenuItem.Background = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#FF00619C"), Color))
        NBDDriverMenuItem.Background = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#FF00619C"), Color))

        Dim ProjectsGridAnimation As New DoubleAnimation With {.From = 1, .To = 0, .Duration = New Duration(TimeSpan.FromMilliseconds(300))}
        Dim StartGridAnimation As New DoubleAnimation With {.From = 0, .To = 1, .Duration = New Duration(TimeSpan.FromMilliseconds(300))}

        StartGrid.Visibility = Visibility.Visible

        ProjectsGrid.BeginAnimation(OpacityProperty, ProjectsGridAnimation)
        StartGrid.BeginAnimation(OpacityProperty, StartGridAnimation)

        ProjectsGrid.Visibility = Visibility.Hidden
    End Sub

    Private Sub ProjectsMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles ProjectsMenuItem.Click
        'Switch to the ProjectsGrid
        StartMenuItem.Background = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#FF00619C"), Color))
        ProjectsMenuItem.Background = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#FF004671"), Color))
        NBDDriverMenuItem.Background = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#FF00619C"), Color))

        Dim ProjectsGridAnimation As New DoubleAnimation With {.From = 0, .To = 1, .Duration = New Duration(TimeSpan.FromMilliseconds(300))}
        Dim StartGridAnimation As New DoubleAnimation With {.From = 1, .To = 0, .Duration = New Duration(TimeSpan.FromMilliseconds(300))}

        ProjectsGrid.Visibility = Visibility.Visible

        StartGrid.BeginAnimation(OpacityProperty, StartGridAnimation)
        ProjectsGrid.BeginAnimation(OpacityProperty, ProjectsGridAnimation)

        StartGrid.Visibility = Visibility.Hidden
    End Sub

    Private Sub NBDDriverMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles NBDDriverMenuItem.Click
        Process.Start(New ProcessStartInfo("https://cloudbase.it/ceph-for-windows/") With {.UseShellExecute = True})
    End Sub

    Private Sub DokanDriverMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles DokanDriverMenuItem.Click
        Process.Start(New ProcessStartInfo("https://github.com/dokan-dev/dokany/releases") With {.UseShellExecute = True})
    End Sub

    Private Sub HDDManagerMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles HDDManagerMenuItem.Click
        If Not String.IsNullOrEmpty(ConnectedPSXHDD.HDLDriveName) Then
            Dim NewPSXHDDManager As New PSXPartitionManager() With {.ShowActivated = True}
            NewPSXHDDManager.Show()
        Else
            MsgBox("Please connect to your PSX HDD first using the Project Manager before using the HDD Partition Manager.", MsgBoxStyle.Information, "Cannot start the HDD Partition Manager")
        End If
    End Sub

    Private Sub XMBToolsMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles XMBToolsMenuItem.Click
        Dim NewPSXXMBTools As New PSXAssetsBrowser() With {.ShowActivated = True}
        NewPSXXMBTools.Show()
    End Sub

#End Region

#Region "Projects"

    Private Sub NewHomebrewProjectMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles NewHomebrewProjectButton.Click
        Dim NewHomebrewProjectWindow As New PSXNewAppProject() With {.ShowActivated = True}
        NewHomebrewProjectWindow.Show()
    End Sub

    Private Sub NewGameProjectMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles NewGameProjectButton.Click
        Dim NewGameProjectWindow As New PSXNewPS2GameProject() With {.ShowActivated = True}
        NewGameProjectWindow.Show()
    End Sub

    Private Sub NewPS1GameProjectButton_Click(sender As Object, e As RoutedEventArgs) Handles NewPS1GameProjectButton.Click
        Dim NewGameProjectWindow As New PSXNewPS1GameProject() With {.ShowActivated = True}
        NewGameProjectWindow.Show()
    End Sub

    Private Sub EditProjectButton_Click(sender As Object, e As RoutedEventArgs) Handles EditProjectButton.Click
        If ProjectListComboBox.SelectedItem IsNot Nothing Then
            'Get project infos
            Dim SelectedProject As ComboBoxProjectItem = CType(ProjectListComboBox.SelectedItem, ComboBoxProjectItem)
            Dim ProjectInfos As String() = File.ReadAllLines(SelectedProject.ProjectFile)
            Dim ProjectName As String = ProjectInfos(0).Split("="c)(1)
            Dim ProjectSubtitle As String = ProjectInfos(1).Split("="c)(1)
            Dim ProjectDirectory As String = ProjectInfos(2).Split("="c)(1)
            Dim ProjectFile As String = ProjectInfos(3).Split("="c)(1)
            Dim ProjectType As String = ProjectInfos(4).Split("="c)(1)

            If ProjectType = "APP" Then
                Dim HomebrewInfos As String() = File.ReadAllLines(ProjectDirectory + "\icon.sys")
                Dim HomebrewProjectEditor As New PSXNewAppProject() With {.Title = "Editing project " + ProjectName + " - " + ProjectDirectory}

                HomebrewProjectEditor.ProjectNameTextBox.Text = ProjectName
                HomebrewProjectEditor.ProjectDirectoryTextBox.Text = ProjectDirectory
                HomebrewProjectEditor.ProjectTitleTextBox.Text = HomebrewInfos(1).Split("="c)(1)
                HomebrewProjectEditor.ProjectSubTitleTextBox.Text = ProjectSubtitle
                HomebrewProjectEditor.ProjectSubTitleTextBox.Text = HomebrewInfos(2).Split("="c)(1)
                HomebrewProjectEditor.ProjectUninstallMsgTextBox.Text = HomebrewInfos(15).Split("="c)(1)
                HomebrewProjectEditor.ProjectELFFileTextBox.Text = ProjectFile

                If File.Exists(ProjectDirectory + "\list.ico") Then
                    HomebrewProjectEditor.ProjectIconPathTextBox.Text = ProjectDirectory + "\list.ico"
                End If

                HomebrewProjectEditor.Show()
            ElseIf ProjectType = "GAME" Then
                Dim GameType As String = ProjectInfos(6).Split("="c)(1)
                Dim GameInfos As String() = File.ReadAllLines(ProjectDirectory + "\icon.sys")

                Select Case GameType
                    Case "PS1"
                        Dim GameProjectEditor As New PSXNewPS1GameProject() With {.Title = "Editing project " + ProjectName + " - " + ProjectDirectory}
                        GameProjectEditor.ProjectNameTextBox.Text = ProjectName
                        GameProjectEditor.ProjectDirectoryTextBox.Text = ProjectDirectory
                        GameProjectEditor.ProjectTitleTextBox.Text = GameInfos(1).Split("="c)(1)
                        GameProjectEditor.ProjectIDTextBox.Text = ProjectSubtitle
                        GameProjectEditor.ProjectIDTextBox.Text = GameInfos(2).Split("="c)(1)
                        GameProjectEditor.ProjectUninstallMsgTextBox.Text = GameInfos(15).Split("="c)(1)

                        GameProjectEditor.IMAGE0PathTextBox.Text = ProjectFile
                        GameProjectEditor.DISCSInfoTextBox.AppendText(Path.GetFileName(ProjectFile) + vbCrLf)

                        'Check for multiple images and tick MultiDiscCheckBox if we have at least 2 discs
                        For Each ProjectFileLine As String In ProjectInfos
                            If ProjectFileLine.StartsWith("IMAGE1=") Then
                                GameProjectEditor.IMAGE1PathTextBox.Text = ProjectInfos(7).Split("="c)(1)
                                GameProjectEditor.MultiDiscCheckBox.IsChecked = True
                                GameProjectEditor.DISCSInfoTextBox.AppendText(Path.GetFileName(ProjectInfos(7).Split("="c)(1)) + vbCrLf)
                            End If
                            If ProjectFileLine.StartsWith("IMAGE2=") Then
                                GameProjectEditor.IMAGE2PathTextBox.Text = ProjectInfos(8).Split("="c)(1)
                                GameProjectEditor.DISCSInfoTextBox.AppendText(Path.GetFileName(ProjectInfos(8).Split("="c)(1)) + vbCrLf)
                            End If
                            If ProjectFileLine.StartsWith("IMAGE3=") Then
                                GameProjectEditor.IMAGE3PathTextBox.Text = ProjectInfos(9).Split("="c)(1)
                                GameProjectEditor.DISCSInfoTextBox.AppendText(Path.GetFileName(ProjectInfos(9).Split("="c)(1)))
                            End If
                        Next

                        If File.Exists(ProjectDirectory + "\list.ico") Then
                            GameProjectEditor.ProjectIconPathTextBox.Text = ProjectDirectory + "\list.ico"
                        End If

                        GameProjectEditor.Show()
                    Case "PS2"
                        Dim GameProjectEditor As New PSXNewPS2GameProject() With {.Title = "Editing project " + ProjectName + " - " + ProjectDirectory}
                        GameProjectEditor.ProjectNameTextBox.Text = ProjectName
                        GameProjectEditor.ProjectDirectoryTextBox.Text = ProjectDirectory
                        GameProjectEditor.ProjectTitleTextBox.Text = GameInfos(1).Split("="c)(1)
                        GameProjectEditor.ProjectIDTextBox.Text = ProjectSubtitle
                        GameProjectEditor.ProjectIDTextBox.Text = GameInfos(2).Split("="c)(1)
                        GameProjectEditor.ProjectUninstallMsgTextBox.Text = GameInfos(15).Split("="c)(1)
                        GameProjectEditor.ProjectISOFileTextBox.Text = ProjectFile

                        If File.Exists(ProjectDirectory + "\list.ico") Then
                            GameProjectEditor.ProjectIconPathTextBox.Text = ProjectDirectory + "\list.ico"
                        End If

                        GameProjectEditor.Show()
                End Select
            End If
        End If
    End Sub

    Private Sub DeleteProjectButton_Click(sender As Object, e As RoutedEventArgs) Handles DeleteProjectButton.Click
        If ProjectListComboBox.SelectedItem IsNot Nothing Then
            Dim SelectedProject As ComboBoxProjectItem = CType(ProjectListComboBox.SelectedItem, ComboBoxProjectItem)
            If File.Exists(SelectedProject.ProjectFile) Then
                File.Delete(SelectedProject.ProjectFile)
                ProjectListComboBox.Items.Remove(SelectedProject)
                ReloadProjects()
            End If
        End If
    End Sub

    Private Sub PrepareProjectButton_Click(sender As Object, e As RoutedEventArgs) Handles PrepareProjectButton.Click
        If ProjectListComboBox.SelectedItem IsNot Nothing Then
            Dim SelectedProject As ComboBoxProjectItem = CType(ProjectListComboBox.SelectedItem, ComboBoxProjectItem)
            Dim ProjectDIR As String = File.ReadAllLines(SelectedProject.ProjectFile)(2).Split("="c)(1)
            Dim SignedStatus As String = File.ReadAllLines(SelectedProject.ProjectFile)(5).Split("="c)(1)
            Dim SignedELF As Boolean = False

            'Check if KELF already exists
            If File.Exists(ProjectDIR + "\EXECUTE.KELF") Or File.Exists(ProjectDIR + "\boot.elf") Or File.Exists(ProjectDIR + "\boot.kelf") Then SignedELF = True

            If SignedStatus = "TRUE" AndAlso SignedELF = True Then
                MsgBox("Your Project doesn't need to be prepared again.", MsgBoxStyle.Information)
            Else
                Dim ProjectELForISO As String = File.ReadAllLines(SelectedProject.ProjectFile)(3).Split("="c)(1)
                Dim ProjectTYPE As String = File.ReadAllLines(SelectedProject.ProjectFile)(4).Split("="c)(1)

                If ProjectTYPE = "APP" Then
                    'Wrap the application ELF as EXECUTE.KELF
                    Dim WrapProcess As New Process()
                    WrapProcess.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\SCEDoormat_NoME.exe"
                    WrapProcess.StartInfo.Arguments = """" + ProjectELForISO + """ " + ProjectDIR + "\EXECUTE.KELF"
                    WrapProcess.StartInfo.CreateNoWindow = True
                    WrapProcess.Start()
                    WrapProcess.WaitForExit()

                    'Mark project as SIGNED
                    Dim ProjectConfigFileLines() As String = File.ReadAllLines(SelectedProject.ProjectFile)
                    ProjectConfigFileLines(5) = "SIGNED=TRUE"
                    File.WriteAllLines(SelectedProject.ProjectFile, ProjectConfigFileLines)

                    MsgBox("Homebrew Project prepared with success !" + vbCrLf + "You can now proceed with the installation on the PSX.", MsgBoxStyle.Information, "Success")
                Else

                    'PS1 games get POPSTARTER and PS2 games get OPL-Launcher
                    Dim GameType As String = File.ReadAllLines(SelectedProject.ProjectFile)(6).Split("="c)(1)

                    Select Case GameType
                        Case "PS1"
                            'Copy included POPSTARTER to project folder
                            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\POPSTARTER.KELF") Then
                                File.Copy(My.Computer.FileSystem.CurrentDirectory + "\Tools\POPSTARTER.KELF", ProjectDIR + "\EXECUTE.KELF", True) 'Save as EXECUTE.KELF
                            Else
                                MsgBox("POPSTARTER.KELF is missing in the Tools directory.", MsgBoxStyle.Critical, "Error setting up the project")
                            End If
                        Case "PS2"
                            'Copy included OPL-Launcher to project folder
                            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\EXECUTE.KELF") Then
                                File.Copy(My.Computer.FileSystem.CurrentDirectory + "\Tools\EXECUTE.KELF", ProjectDIR + "\EXECUTE.KELF", True)
                            Else
                                'OPL-Launcher not found...
                                Dim HomebrewELF As String = ""

                                HomebrewELF = InputBox("OPL-Launcher has been deleted from the Tools folder." + vbCrLf + "Please enter the full path to the .elf file or leave the URL to download OPL-Launcher.",
                                                           "Missing file",
                                                           "https://github.com/ps2homebrew/OPL-Launcher/releases/download/latest/OPL-Launcher.elf")

                                If Not String.IsNullOrEmpty(HomebrewELF) Then
                                    If HomebrewELF = "https://github.com/ps2homebrew/OPL-Launcher/releases/download/latest/OPL-Launcher.elf" Then
                                        'Download latest OPL-Launcher
                                        ContentDownloader.DownloadFile("https://github.com/ps2homebrew/OPL-Launcher/releases/download/latest/OPL-Launcher.elf", My.Computer.FileSystem.CurrentDirectory + "\Tools\OPL-Launcher.elf")
                                    End If
                                Else
                                    MsgBox("Not valid file provided, aborting ...", MsgBoxStyle.Exclamation, "Aborting")
                                    Exit Sub
                                End If

                                'Wrap OPL-Launcher as EXECUTE.KELF
                                Dim WrapProcess As New Process()
                                WrapProcess.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\SCEDoormat_NoME.exe"
                                WrapProcess.StartInfo.Arguments = """" + My.Computer.FileSystem.CurrentDirectory + "\Tools\OPL-Launcher.elf"" """ + ProjectDIR + "\EXECUTE.KELF"""
                                WrapProcess.StartInfo.CreateNoWindow = True
                                WrapProcess.Start()
                                WrapProcess.WaitForExit()
                            End If
                    End Select

                    'Mark project as SIGNED
                    Dim ProjectConfigFileLines() As String = File.ReadAllLines(SelectedProject.ProjectFile)
                    ProjectConfigFileLines(5) = "SIGNED=TRUE"
                    File.WriteAllLines(SelectedProject.ProjectFile, ProjectConfigFileLines)

                    MsgBox("Game Project is now prepared !" + vbCrLf + "You can now proceed with the installation on the PSX.", MsgBoxStyle.Information, "Success")
                End If
            End If

            ReloadProjects()
        End If
    End Sub

    Private Sub InstallProjectButton_Click(sender As Object, e As RoutedEventArgs) Handles InstallProjectButton.Click
        If PreparedProjectsComboBox.SelectedItem IsNot Nothing Then
            Dim SelectedProject As ComboBoxProjectItem = CType(PreparedProjectsComboBox.SelectedItem, ComboBoxProjectItem)

            If File.Exists(SelectedProject.ProjectFile) Then
                Dim ProjectTitle As String = File.ReadAllLines(SelectedProject.ProjectFile)(0).Split("="c)(1)
                If MsgBox("Do you really want to install " + ProjectTitle + " on your PSX ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then

                    'Identify project type
                    Dim ProjectType As String = File.ReadAllLines(SelectedProject.ProjectFile)(4).Split("="c)(1)
                    Dim NewInstallWindow As New InstallWindow() With {.ProjectToInstall = SelectedProject, .MountedDrive = MountedDrive, .Title = "Installing " + ProjectTitle}

                    If ProjectType = "APP" Then
                        NewInstallWindow.InstallStatus = "Installing Homebrew, please wait..."
                        NewInstallWindow.InstallApp()

                        NewInstallWindow.ShowDialog()
                    ElseIf ProjectType = "GAME" Then
                        Dim GameType As String = File.ReadAllLines(SelectedProject.ProjectFile)(6).Split("="c)(1)

                        Select Case GameType
                            Case "PS1"
                                NewInstallWindow.InstallStatus = "Installing PS1 Game, do not close when it freezes or hangs."
                                NewInstallWindow.InstallationProgressBar.IsIndeterminate = True
                                NewInstallWindow.InstallForPS2 = False
                            Case "PS2"
                                NewInstallWindow.InstallStatus = "Installing PS2 Game, please wait..."
                                NewInstallWindow.InstallForPS2 = True
                        End Select

                        NewInstallWindow.ShowDialog()
                    End If

                Else
                    MsgBox("Installation aborted.", MsgBoxStyle.Information, "Aborted")
                End If
            Else
                MsgBox("Could not find the selected project: " + SelectedProject.ProjectFile, MsgBoxStyle.Critical, "Error")
            End If

        Else
            MsgBox("No project selected.", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

#End Region

    Private Sub ConnectDelay_Tick(sender As Object, e As EventArgs) Handles ConnectDelay.Tick
        'Get drive properties after the connect delay
        Dim ConnectedNBDDriveName As String = IsNBDConnected(WNBDClientPath)
        If Not String.IsNullOrEmpty(ConnectedNBDDriveName) Then
            MountedDrive.NBDDriveName = ConnectedNBDDriveName

            'Get HDL Drive Name
            Dim HDLDriveName As String = GetHDLDriveName()
            If Not String.IsNullOrEmpty(HDLDriveName) Then

                'Update UI
                If MountStatusLabel.CheckAccess() = False Then
                    MountStatusLabel.Dispatcher.BeginInvoke(Sub()
                                                                MountStatusLabel.Text = "On " + HDLDriveName
                                                                MountStatusLabel.Foreground = Brushes.Green
                                                            End Sub)
                Else
                    MountStatusLabel.Text = "On " + HDLDriveName
                    MountStatusLabel.Foreground = Brushes.Green
                End If

                MountedDrive.HDLDriveName = HDLDriveName
            End If

            MountedDrive.DriveID = GetHDDID()

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           InstallProjectButton.IsEnabled = True
                                           NBDConnectionLabel.Text = "Connected"
                                           NBDConnectionLabel.Foreground = Brushes.Green
                                           ConnectButton.Content = "Disconnect"
                                       End Sub)
            Else
                InstallProjectButton.IsEnabled = True
                NBDConnectionLabel.Text = "Connected"
                NBDConnectionLabel.Foreground = Brushes.Green
                ConnectButton.Content = "Disconnect"
            End If

            ConnectedPSXHDD = MountedDrive
            MsgBox("PSX HDD is now connected." + vbCrLf + "You can now install a project on the PSX.", MsgBoxStyle.Information, "Success")
        ElseIf Not String.IsNullOrEmpty(IsLocalHDDConnected()) Then 'Local HDD
            Dim ConnectedLocalHDDDriveName As String = IsLocalHDDConnected()
            MountStatusLabel.Text = "on " + ConnectedLocalHDDDriveName
            MountStatusLabel.Foreground = Brushes.Green
            MountedDrive.HDLDriveName = ConnectedLocalHDDDriveName

            MountedDrive.DriveID = GetHDDID()

            InstallProjectButton.IsEnabled = True
            NBDConnectionStatusLabel.Text = "Local Connection:"
            NBDConnectionLabel.Text = "Connected"

            EnterIPLabel.Text = "Local PS2/PSX HDD detected & connected."
            EnterIPLabel.TextAlignment = TextAlignment.Center
            ConnectButton.IsEnabled = False
            PSXIPTextBox.IsEnabled = False
            PSXIPTextBox.Text = "Local Connection"
            ConnectButton.Foreground = Brushes.Black

            NBDConnectionLabel.Foreground = Brushes.Green
            ConnectButton.Content = "Disabled"
            ConnectedPSXHDD = MountedDrive
        Else
            MsgBox("Could not connect to the PSX." + vbCrLf + "Please check the IP address.", MsgBoxStyle.Critical, "Error")
        End If

        ConnectDelay.Stop()
    End Sub


End Class
