Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Imports PS_Multi_Tools.Structures
Imports PS_Multi_Tools.Utils

Public Class InstallWindow

    Public MountedDrive As MountedPSXDrive

    Private WithEvents HDL_Dump As New Process()
    Private HDLGameID As String = ""

    Public ProjectToInstall As ComboBoxProjectItem
    Public CurrentProjectDirectory As String = ""

    Public InstallStatus As String
    Public InstallForPS2 As Boolean = False

    Private Sub InstallWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If ProjectToInstall IsNot Nothing Then

            If File.Exists(ProjectToInstall.ProjectFile) Then

                Dim GameAppTitle As String = File.ReadAllLines(ProjectToInstall.ProjectFile)(0).Split("="c)(1)
                Dim GameAppID As String = File.ReadAllLines(ProjectToInstall.ProjectFile)(1).Split("="c)(1)
                Dim GameAppDirectory As String = File.ReadAllLines(ProjectToInstall.ProjectFile)(2).Split("="c)(1)

                'Set cover
                If File.Exists(GameAppDirectory + "\res\jkt_001.png") Then
                    Dispatcher.BeginInvoke(Sub()
                                               Dim TempBitmapImage = New BitmapImage()
                                               TempBitmapImage.BeginInit()
                                               TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                               TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                               TempBitmapImage.UriSource = New Uri(GameAppDirectory + "\res\jkt_001.png", UriKind.RelativeOrAbsolute)
                                               TempBitmapImage.EndInit()
                                               InstallImage.Source = TempBitmapImage
                                           End Sub)
                Else
                    If Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameAppID + ".jpg") Then
                        Dispatcher.BeginInvoke(Sub()
                                                   Dim TempBitmapImage = New BitmapImage()
                                                   TempBitmapImage.BeginInit()
                                                   TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                   TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                   TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameAppID + ".jpg", UriKind.RelativeOrAbsolute)
                                                   TempBitmapImage.EndInit()
                                                   InstallImage.Source = TempBitmapImage
                                               End Sub)
                    ElseIf Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameAppID + ".jpg") Then
                        Dispatcher.BeginInvoke(Sub()
                                                   Dim TempBitmapImage = New BitmapImage()
                                                   TempBitmapImage.BeginInit()
                                                   TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                   TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                   TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameAppID + ".jpg", UriKind.RelativeOrAbsolute)
                                                   TempBitmapImage.EndInit()
                                                   InstallImage.Source = TempBitmapImage
                                               End Sub)
                    End If
                End If
            End If

            'Set current status
            If Not String.IsNullOrEmpty(InstallStatus) Then
                InstallationStatusTextBlock.Text = InstallStatus
            End If
        Else
            MsgBox("Could not load the selected project to install.", MsgBoxStyle.Critical, "Error")
            Close()
        End If
    End Sub

    Private Sub InstallWindow_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        Thread.Sleep(200)
        If InstallForPS2 Then
            InstallPS2Game()
        Else
            InstallPS1Game()
        End If
    End Sub

    Public Sub InstallApp()
        'Check if drive is already identified, if not get the drive name
        If String.IsNullOrEmpty(MountedDrive.HDLDriveName) Then
            MountedDrive.HDLDriveName = GetHDLDriveName()
            'Retry
            InstallApp()
        Else
            If ProjectToInstall IsNot Nothing Then
                'Proceed to installation on HDD

                'Get homebrew properties
                Dim HomebrewTitle As String = File.ReadAllLines(ProjectToInstall.ProjectFile)(0).Split("="c)(1)
                Dim HomebrewELF As String = File.ReadAllLines(ProjectToInstall.ProjectFile)(3).Split("="c)(1)
                Dim HomebrewPartition As String

                CurrentProjectDirectory = File.ReadAllLines(ProjectToInstall.ProjectFile)(2).Split("="c)(1)

                'Set a PP partition name on known homebrew
                If HomebrewTitle.Contains("Open PS2 Loader") Or HomebrewTitle.Contains("OPL") Then
                    HomebrewPartition = "PP.APPS-00001..OPL"
                ElseIf HomebrewTitle.Contains("LaunchELF") Or HomebrewTitle.Contains("uLE") Or HomebrewTitle.Contains("wLE") Then
                    HomebrewPartition = "PP.APPS-00002..WLE"
                ElseIf HomebrewTitle.Contains("hdl_srv") Or HomebrewTitle.Contains("hdl_server") Or HomebrewTitle.Contains("hdl server") Then
                    HomebrewPartition = "PP.APPS-00003..HDL"
                ElseIf HomebrewTitle.Contains("SMS") Or HomebrewTitle.Contains("Simple Media System") Then
                    HomebrewPartition = "PP.APPS-00004..SMS"
                ElseIf HomebrewTitle.Contains("GSM") Then
                    HomebrewPartition = "PP.APPS-00005..GSM"
                Else
                    'Set own PP partition name
                    HomebrewPartition = InputBox("Please enter a valid partition name:", "Could not determine partition for this homebrew.", "PP.APPS-00001..TITLE")
                End If

                'Update UI
                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub() InstallationStatusTextBlock.Text = "Creating partition, please wait...")
                Else
                    InstallationStatusTextBlock.Text = "Creating partition, please wait..."
                End If

                If Not String.IsNullOrEmpty(HomebrewPartition) Then
                    CreateHomebrewPartition(HomebrewPartition)
                Else
                    MsgBox("Partition name cannot be empty! Please try again.", MsgBoxStyle.Exclamation, "Error")
                    Exit Sub
                End If
            End If
        End If
    End Sub

    Public Sub InstallPS2Game()
        If ProjectToInstall IsNot Nothing Then
            'Proceed to installation on HDD
            'Get game properties
            Dim GameTitle As String = File.ReadAllLines(ProjectToInstall.ProjectFile)(0).Split("="c)(1)
            Dim GameID As String = File.ReadAllLines(ProjectToInstall.ProjectFile)(1).Split("="c)(1)
            Dim GameISO As String = File.ReadAllLines(ProjectToInstall.ProjectFile)(3).Split("="c)(1)

            HDLGameID = File.ReadAllLines(ProjectToInstall.ProjectFile)(1).Split("="c)(1).Replace("_", "-").Replace(".", "").Trim()
            CurrentProjectDirectory = File.ReadAllLines(ProjectToInstall.ProjectFile)(2).Split("="c)(1)

            'Set up hdl_dump
            HDL_Dump = New Process()
            HDL_Dump.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\hdl_dump.exe"
            HDL_Dump.StartInfo.RedirectStandardOutput = True
            AddHandler HDL_Dump.OutputDataReceived, AddressOf HDLDumpOutputDataHandler
            HDL_Dump.StartInfo.UseShellExecute = False
            HDL_Dump.StartInfo.CreateNoWindow = True
            HDL_Dump.EnableRaisingEvents = True

            'Check if it's a CD or DVD and start injecting the game
            If GetDiscType(GameISO) = DiscType.DVD Then
                HDL_Dump.StartInfo.Arguments = "inject_dvd " + MountedDrive.HDLDriveName + " """ + GameTitle + """ """ + GameISO + """ " + GameID + " *u4 -hide"
                HDL_Dump.Start()
                HDL_Dump.BeginOutputReadLine()
            Else
                HDL_Dump.StartInfo.Arguments = "inject_cd " + MountedDrive.HDLDriveName + " """ + GameTitle + """ """ + GameISO + """ " + GameID + " *u4 -hide"
                HDL_Dump.Start()
                HDL_Dump.BeginOutputReadLine()
            End If
        Else
            MsgBox("Could not load the project to install.", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

    Public Sub InstallPS1Game()
        If ProjectToInstall IsNot Nothing Then
            'Proceed to installation on HDD
            'Get game properties
            Dim ProjectInfos As String() = File.ReadAllLines(ProjectToInstall.ProjectFile)
            Dim GameTitle As String = ProjectInfos(0).Split("="c)(1)
            Dim GameID As String = ProjectInfos(1).Split("="c)(1)
            CurrentProjectDirectory = ProjectInfos(2).Split("="c)(1)
            Dim GameVCD As String = ProjectInfos(3).Split("="c)(1)

            'Ask for a partition name
            Dim PPPartitionName As String = InputBox("Enter a valid partition name starting with PP. including the dot." + vbCrLf + vbCrLf + "WARNING: No + sign in the partition name and no whitespaces !", "Creating the game PP partition", "PP.SHORT_GAME_TITLE")
            If Not String.IsNullOrEmpty(PPPartitionName) Then
                If PPPartitionName.StartsWith("PP.") Then
                    If PPPartitionName.Length < 50 Then

                        If PPPartitionName.Contains("+") Then
                            MsgBox("A + sign has been detected in the partition name and will be replaced with ""_"".", MsgBoxStyle.Information, "Unallowed character detected")
                            PPPartitionName.Replace("+", "_")
                        End If

                        'Trim the final PPPartitionName
                        PPPartitionName = PPPartitionName.Trim()
                    Else
                        MsgBox("Partition name is too long. Please retry the installation with a shorter name.", MsgBoxStyle.Critical, "Partition name invalid")
                        Close()
                    End If
                Else
                    MsgBox("Partition name needs to start with ""PP."". Please retry the installation.", MsgBoxStyle.Critical, "Partition name invalid")
                    Close()
                End If
            Else
                MsgBox("No partition name entered. Exiting installation.", MsgBoxStyle.Critical, "Partition name invalid")
                Close()
            End If

            'Calculate the required partition size
            Dim TotalGameSize As Long = New FileInfo(GameVCD).Length
            For Each ProjectFileLine As String In ProjectInfos
                If ProjectFileLine.StartsWith("IMAGE1=") Then TotalGameSize += New FileInfo(ProjectFileLine.Split("="c)(1)).Length
                If ProjectFileLine.StartsWith("IMAGE2=") Then TotalGameSize += New FileInfo(ProjectFileLine.Split("="c)(1)).Length
                If ProjectFileLine.StartsWith("IMAGE3=") Then TotalGameSize += New FileInfo(ProjectFileLine.Split("="c)(1)).Length
            Next

            Dim GameTotalSizeInMB As Double = TotalGameSize / 1024 / 1024
            Dim GameTotalSizeRoundedValue As Double = Math.Round(GameTotalSizeInMB, 0, MidpointRounding.AwayFromZero)
            Dim GameRequiredPartitionSizeInMB As Double

            'Final partition size should be a multiple of 128MiB
            If GameTotalSizeRoundedValue >= 1000 Then
                GameRequiredPartitionSizeInMB = 128 * Integer.Parse(GameTotalSizeRoundedValue.ToString().Substring(0, 2))
            Else
                GameRequiredPartitionSizeInMB = 128 * Integer.Parse(GameTotalSizeRoundedValue.ToString().Substring(0, 1))
            End If

            If MsgBox("A new partition " + PPPartitionName + " with " + GameRequiredPartitionSizeInMB.ToString() + "M will be created." + vbCrLf + "Do you want to proceed with the installation ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then

                '1. Set mkpart command for the PP partition
                Using CommandFileWriter As New StreamWriter(My.Computer.FileSystem.CurrentDirectory + "\Tools\cmdlist\mkpart.txt", False)
                    CommandFileWriter.WriteLine("device " + MountedDrive.DriveID)
                    CommandFileWriter.WriteLine("mkpart " + PPPartitionName + " " + GameRequiredPartitionSizeInMB.ToString() + "M PFS")
                    CommandFileWriter.WriteLine("exit")
                End Using

                'Update UI 
                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub() InstallationStatusTextBlock.Text = "Creating game partition...")
                Else
                    InstallationStatusTextBlock.Text = "Creating game partition..."
                End If

                Thread.Sleep(200)

                '2. Proceed to partition creation
                Dim PFSShellOutput As String
                Using PFSShellProcess As New Process()
                    PFSShellProcess.StartInfo.FileName = "cmd"
                    PFSShellProcess.StartInfo.Arguments = """/c type """ + My.Computer.FileSystem.CurrentDirectory + "\Tools\cmdlist\mkpart.txt"" | """ + My.Computer.FileSystem.CurrentDirectory + "\Tools\pfsshell.exe"" 2>&1"

                    PFSShellProcess.StartInfo.RedirectStandardOutput = True
                    PFSShellProcess.StartInfo.UseShellExecute = False
                    PFSShellProcess.StartInfo.CreateNoWindow = True

                    PFSShellProcess.Start()

                    Dim ShellReader As StreamReader = PFSShellProcess.StandardOutput
                    Dim ProcessOutput As String = ShellReader.ReadToEnd()

                    ShellReader.Close()
                    PFSShellOutput = ProcessOutput
                End Using

                '3. Read partition creation output
                If PFSShellOutput.Contains("created.") Then

                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub() InstallationStatusTextBlock.Text = PPPartitionName + " created. Now adding files ...")
                    Else
                        InstallationStatusTextBlock.Text = PPPartitionName + " created. Now adding files ..."
                    End If

                    Thread.Sleep(200)

                    '4. Add files to the partition
                    PS1AddFilesToPartition(PPPartitionName)
                Else
                    MsgBox("There was an error in creating the game's PP partition, please check if the name doesn't already exists and if you have enough space.", MsgBoxStyle.Exclamation, "Error installing game")

                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub() InstallationStatusTextBlock.Text = "")
                    Else
                        InstallationStatusTextBlock.Text = ""
                    End If

                    Exit Sub
                End If
            Else
                MsgBox("Exiting game installation.", MsgBoxStyle.Critical, "Installation aborted")
                Close()
            End If
        Else
            MsgBox("Could not load the project to install.", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

    Private Sub CreateGamePartition()
        Dim CreatedGamePartition As String = ""

        'Get the created partition
        '1. List partitions
        Dim QueryOutput As String()
        Using HDLDump As New Process()
            HDLDump.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\hdl_dump.exe"
            HDLDump.StartInfo.Arguments = "toc " + MountedDrive.HDLDriveName
            HDLDump.StartInfo.RedirectStandardOutput = True
            HDLDump.StartInfo.UseShellExecute = False
            HDLDump.StartInfo.CreateNoWindow = True
            HDLDump.Start()

            Dim OutputReader As StreamReader = HDLDump.StandardOutput
            QueryOutput = OutputReader.ReadToEnd().Split({vbCrLf}, StringSplitOptions.None)
        End Using

        '2. Search for the created hidden partition
        For Each HDDPartition As String In QueryOutput
            If Not String.IsNullOrEmpty(HDDPartition) Then
                If HDDPartition.Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries).Count >= 3 Then
                    HDDPartition = HDDPartition.Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries)(4)
                    If HDDPartition.Trim().StartsWith("__." + HDLGameID) Then 'The created hidden partition
                        CreatedGamePartition = HDDPartition.Trim()
                        Exit For
                    End If
                End If
            End If
        Next

        '3. Set mkpart command for the PP partition
        Using CommandFileWriter As New StreamWriter(My.Computer.FileSystem.CurrentDirectory + "\Tools\cmdlist\mkpart.txt", False)
            CommandFileWriter.WriteLine("device " + MountedDrive.DriveID)
            CommandFileWriter.WriteLine("mkpart " + CreatedGamePartition.Replace("__.", "PP.") + " 128M PFS")
            CommandFileWriter.WriteLine("exit")
        End Using

        '4. Proceed to partition creation
        Dim PFSShellOutput As String
        Using PFSShellProcess As New Process()
            PFSShellProcess.StartInfo.FileName = "cmd"
            PFSShellProcess.StartInfo.Arguments = """/c type """ + My.Computer.FileSystem.CurrentDirectory + "\Tools\cmdlist\mkpart.txt"" | """ + My.Computer.FileSystem.CurrentDirectory + "\Tools\pfsshell.exe"" 2>&1"

            PFSShellProcess.StartInfo.RedirectStandardOutput = True
            PFSShellProcess.StartInfo.UseShellExecute = False
            PFSShellProcess.StartInfo.CreateNoWindow = True

            PFSShellProcess.Start()

            Dim ShellReader As StreamReader = PFSShellProcess.StandardOutput
            Dim ProcessOutput As String = ShellReader.ReadToEnd()

            ShellReader.Close()
            PFSShellOutput = ProcessOutput
        End Using

        '5. Read partition creation output
        If PFSShellOutput.Contains("Main partition of 128M created.") Then

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub() InstallationStatusTextBlock.Text = "Partition created, modifying header...")
            Else
                InstallationStatusTextBlock.Text = "Partition created, modifying header..."
            End If

            '6. Modify the created partition
            ModifyPartitionHeader(CreatedGamePartition.Replace("__.", "PP."), False)
        Else
            MsgBox("There was an error in creating the game's PP partition, please check if the name doesn't already exists and if you have enough space.", MsgBoxStyle.Exclamation, "Error installing game")

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub() InstallationStatusTextBlock.Text = "")
            Else
                InstallationStatusTextBlock.Text = ""
            End If

            Exit Sub
        End If
    End Sub

    Public Sub CreateHomebrewPartition(PartitionName As String)
        If ProjectToInstall IsNot Nothing Then
            Dim ProjectDirectory As String = File.ReadAllLines(ProjectToInstall.ProjectFile)(2).Split("="c)(1)

            '1. Set mkpart command for the PP partition
            Using CommandFileWriter As New StreamWriter(My.Computer.FileSystem.CurrentDirectory + "\Tools\cmdlist\mkpart.txt", False)
                CommandFileWriter.WriteLine("device " + MountedDrive.DriveID)
                CommandFileWriter.WriteLine("mkpart " + PartitionName + " 128M PFS")
                CommandFileWriter.WriteLine("exit")
            End Using

            '2. Proceed to partition creation
            Dim PFSShellOutput As String
            Using PFSShellProcess As New Process()
                PFSShellProcess.StartInfo.FileName = "cmd"
                PFSShellProcess.StartInfo.Arguments = """/c type """ + My.Computer.FileSystem.CurrentDirectory + "\Tools\cmdlist\mkpart.txt"" | """ + My.Computer.FileSystem.CurrentDirectory + "\Tools\pfsshell.exe"" 2>&1"

                PFSShellProcess.StartInfo.RedirectStandardOutput = True
                PFSShellProcess.StartInfo.UseShellExecute = False

                PFSShellProcess.Start()

                Dim ShellReader As StreamReader = PFSShellProcess.StandardOutput
                Dim ProcessOutput As String = ShellReader.ReadToEnd()

                ShellReader.Close()
                PFSShellOutput = ProcessOutput
            End Using

            '3. Read partition creation output
            If PFSShellOutput.Contains("Main partition of 128M created.") Then
                InstallationStatusTextBlock.Text = "Partition created, modifying header..."

                '4. Modify the created partition
                ModifyPartitionHeader(PartitionName, False)
            Else
                MsgBox("There was an error in creating the homebrew's PP partition." + vbCrLf + "Please check if the partition name '" + PartitionName + "' does not already exists of if HDD space is sufficient.", MsgBoxStyle.Exclamation, "Error while installing homebrew")
                Exit Sub
            End If
        End If
    End Sub

    Public Sub ModifyPartitionHeader(PartitionName As String, FinalizePS1 As Boolean)
        '1. Create a copy of hdl_dump in the project directory
        File.Copy(My.Computer.FileSystem.CurrentDirectory + "\Tools\hdl_dump.exe", CurrentProjectDirectory + "\hdl_dump.exe", True)

        '2. Switch to project directory and inject the files
        Directory.SetCurrentDirectory(CurrentProjectDirectory)

        '3. Modify the partition header using hdl_dump
        Dim HDLDumpOutput As String = ""
        Using HDLDump As New Process()
            HDLDump.StartInfo.FileName = "hdl_dump.exe"
            HDLDump.StartInfo.Arguments = "modify_header " + MountedDrive.HDLDriveName + " " + PartitionName
            HDLDump.StartInfo.RedirectStandardOutput = True
            HDLDump.StartInfo.UseShellExecute = False
            HDLDump.StartInfo.CreateNoWindow = True
            HDLDump.Start()

            HDLDumpOutput = HDLDump.StandardOutput.ReadToEnd()
        End Using

        '4. Read hdl_dump output
        If Not HDLDumpOutput.Contains("partition not found:") Then
            If FinalizePS1 Then

                'Set the current directory back
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory)

                If InstallationStatusTextBlock.Dispatcher.CheckAccess() = False Then
                    InstallationStatusTextBlock.Dispatcher.BeginInvoke(Sub() InstallationStatusTextBlock.Text = "Partition header modified. Installation is done!")
                Else
                    InstallationStatusTextBlock.Text = "Partition header modified. Installation is done!"
                End If

                If MsgBox("Installation completed with success!", MsgBoxStyle.OkOnly, "Success") = MsgBoxResult.Ok Then
                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub() Close())
                    Else
                        Close()
                    End If
                End If
            Else
                If InstallationStatusTextBlock.Dispatcher.CheckAccess() = False Then
                    InstallationStatusTextBlock.Dispatcher.BeginInvoke(Sub() InstallationStatusTextBlock.Text = "Partition header modified, adding files...")
                Else
                    InstallationStatusTextBlock.Text = "Partition header modified, adding files..."
                End If

                '5. Add files to the partition
                PS2AddFilesToPartition(PartitionName)
            End If
        Else
            MsgBox("There was an error while modifying the partition, please check if you have enough space and report the next error." + vbCrLf + HDLDumpOutput, MsgBoxStyle.Exclamation, "Error installing game")
            'Set the current directory back
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory)
            Exit Sub
        End If
    End Sub

    Public Sub PS2AddFilesToPartition(PartitionName As String)
        'Now put the "res" folder and EXECUTE.KELF file into the partition
        Dim PFSShellOutput As String

        'Set the mkdir & put commands
        Using CommandFileWriter As New StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Tools\cmdlist\push.txt", False)
            CommandFileWriter.WriteLine("device " + MountedDrive.DriveID)
            CommandFileWriter.WriteLine("mount " + PartitionName)
            CommandFileWriter.WriteLine("put EXECUTE.KELF")
            CommandFileWriter.WriteLine("mkdir res")
            CommandFileWriter.WriteLine("cd res")

            If File.Exists("res\info.sys") Then
                CommandFileWriter.WriteLine("put res\info.sys")
                CommandFileWriter.WriteLine("rename res\info.sys info.sys")
            End If
            If File.Exists("res\jkt_001.png") Then
                CommandFileWriter.WriteLine("put res\jkt_001.png")
                CommandFileWriter.WriteLine("rename res\jkt_001.png jkt_001.png")
            End If
            If File.Exists("res\jkt_002.png") Then
                CommandFileWriter.WriteLine("put res\jkt_002.png")
                CommandFileWriter.WriteLine("rename res\jkt_002.png jkt_002.png")
            End If
            If File.Exists("res\jkt_cp.png") Then
                CommandFileWriter.WriteLine("put res\jkt_cp.png")
                CommandFileWriter.WriteLine("rename res\jkt_cp.png jkt_cp.png")
            End If
            If File.Exists("res\man.xml") Then
                CommandFileWriter.WriteLine("put res\man.xml")
                CommandFileWriter.WriteLine("rename res\man.xml man.xml")
            End If
            If File.Exists("res\notice.jpg") Then
                CommandFileWriter.WriteLine("put res\notice.jpg")
                CommandFileWriter.WriteLine("rename res\notice.jpg notice.jpg")
            End If

            If Directory.Exists("res\image") Then
                CommandFileWriter.WriteLine("mkdir image")
                CommandFileWriter.WriteLine("cd image")

                If File.Exists("res\image\0.png") Then
                    CommandFileWriter.WriteLine("put res\image\0.png")
                    CommandFileWriter.WriteLine("rename res\image\0.png 0.png")
                End If
                If File.Exists("res\image\1.png") Then
                    CommandFileWriter.WriteLine("put res\image\1.png")
                    CommandFileWriter.WriteLine("rename res\image\1.png 1.png")
                End If
                If File.Exists("res\image\2.png") Then
                    CommandFileWriter.WriteLine("put res\image\2.png")
                    CommandFileWriter.WriteLine("rename res\image\2.png 2.png")
                End If
            End If

            CommandFileWriter.WriteLine("umount")
            CommandFileWriter.WriteLine("exit")
        End Using

        'Put all detected files to the partition using pfsshell
        Using PFSShellProcess As New Process()
            PFSShellProcess.StartInfo.FileName = "cmd"
            PFSShellProcess.StartInfo.Arguments = """/c type """ + AppDomain.CurrentDomain.BaseDirectory + "Tools\cmdlist\push.txt"" | """ + AppDomain.CurrentDomain.BaseDirectory + "Tools\pfsshell.exe"" 2>&1"
            PFSShellProcess.StartInfo.RedirectStandardOutput = True
            PFSShellProcess.StartInfo.UseShellExecute = False
            PFSShellProcess.StartInfo.CreateNoWindow = True

            PFSShellProcess.Start()
            PFSShellProcess.WaitForExit()

            Dim PFSShellReader As StreamReader = PFSShellProcess.StandardOutput
            Dim ProcessOutput As String = PFSShellReader.ReadToEnd()

            PFSShellReader.Close()
            PFSShellOutput = ProcessOutput
        End Using

        'Update UI when finished
        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub() InstallationStatusTextBlock.Text = "")
        Else
            InstallationStatusTextBlock.Text = ""
        End If

        'Set the current directory back
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory)

        If MsgBox("Installation completed with success!", MsgBoxStyle.OkOnly, "Success") = MsgBoxResult.Ok Then
            If Dispatcher.CheckAccess() = False Then
                'Owned by different thread
                Dispatcher.BeginInvoke(Sub() Close())
            Else
                Close()
            End If
        End If
    End Sub

    Public Sub PS1AddFilesToPartition(PartitionName As String)

        'Switch to project directory and add the files
        Directory.SetCurrentDirectory(CurrentProjectDirectory)

        'Now put the game VCD(s), (DISCS.TXT) the "res" folder and EXECUTE.KELF file into the partition
        Dim PFSShellOutput As String

        'Set the mkdir & put commands
        Using CommandFileWriter As New StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Tools\cmdlist\push.txt", False)
            CommandFileWriter.WriteLine("device " + MountedDrive.DriveID)
            CommandFileWriter.WriteLine("mount " + PartitionName)
            CommandFileWriter.WriteLine("put EXECUTE.KELF")

            If File.Exists("DISCS.TXT") Then
                CommandFileWriter.WriteLine("put DISCS.TXT")
            End If

            If File.Exists("IMAGE0.VCD") Then
                CommandFileWriter.WriteLine("put IMAGE0.VCD")
            End If

            If File.Exists("IMAGE1.VCD") Then
                CommandFileWriter.WriteLine("put IMAGE1.VCD")
            End If

            If File.Exists("IMAGE2.VCD") Then
                CommandFileWriter.WriteLine("put IMAGE2.VCD")
            End If

            If File.Exists("IMAGE3.VCD") Then
                CommandFileWriter.WriteLine("put IMAGE3.VCD")
            End If

            CommandFileWriter.WriteLine("mkdir res")
            CommandFileWriter.WriteLine("cd res")

            If File.Exists("res\info.sys") Then
                CommandFileWriter.WriteLine("put res\info.sys")
                CommandFileWriter.WriteLine("rename res\info.sys info.sys")
            End If
            If File.Exists("res\jkt_001.png") Then
                CommandFileWriter.WriteLine("put res\jkt_001.png")
                CommandFileWriter.WriteLine("rename res\jkt_001.png jkt_001.png")
            End If
            If File.Exists("res\jkt_002.png") Then
                CommandFileWriter.WriteLine("put res\jkt_002.png")
                CommandFileWriter.WriteLine("rename res\jkt_002.png jkt_002.png")
            End If
            If File.Exists("res\jkt_cp.png") Then
                CommandFileWriter.WriteLine("put res\jkt_cp.png")
                CommandFileWriter.WriteLine("rename res\jkt_cp.png jkt_cp.png")
            End If
            If File.Exists("res\man.xml") Then
                CommandFileWriter.WriteLine("put res\man.xml")
                CommandFileWriter.WriteLine("rename res\man.xml man.xml")
            End If
            If File.Exists("res\notice.jpg") Then
                CommandFileWriter.WriteLine("put res\notice.jpg")
                CommandFileWriter.WriteLine("rename res\notice.jpg notice.jpg")
            End If

            If Directory.Exists("res\image") Then
                CommandFileWriter.WriteLine("mkdir image")
                CommandFileWriter.WriteLine("cd image")

                If File.Exists("res\image\0.png") Then
                    CommandFileWriter.WriteLine("put res\image\0.png")
                    CommandFileWriter.WriteLine("rename res\image\0.png 0.png")
                End If
                If File.Exists("res\image\1.png") Then
                    CommandFileWriter.WriteLine("put res\image\1.png")
                    CommandFileWriter.WriteLine("rename res\image\1.png 1.png")
                End If
                If File.Exists("res\image\2.png") Then
                    CommandFileWriter.WriteLine("put res\image\2.png")
                    CommandFileWriter.WriteLine("rename res\image\2.png 2.png")
                End If
            End If

            CommandFileWriter.WriteLine("umount")
            CommandFileWriter.WriteLine("exit")
        End Using

        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub() InstallationStatusTextBlock.Text = "Adding files... This can take some time.")
        Else
            InstallationStatusTextBlock.Text = "Adding files... This can take some time."
        End If

        Mouse.SetCursor(Cursors.Wait)
        Thread.Sleep(200)

        'Put all detected files to the partition using pfsshell
        Using PFSShellProcess As New Process()
            PFSShellProcess.StartInfo.FileName = "cmd"
            PFSShellProcess.StartInfo.Arguments = """/c type """ + AppDomain.CurrentDomain.BaseDirectory + "Tools\cmdlist\push.txt"" | """ + AppDomain.CurrentDomain.BaseDirectory + "Tools\pfsshell.exe"" 2>&1"
            PFSShellProcess.StartInfo.RedirectStandardOutput = True
            PFSShellProcess.StartInfo.UseShellExecute = False
            PFSShellProcess.StartInfo.CreateNoWindow = True

            PFSShellProcess.Start()
            PFSShellProcess.WaitForExit()

            Dim PFSShellReader As StreamReader = PFSShellProcess.StandardOutput
            Dim ProcessOutput As String = PFSShellReader.ReadToEnd()

            PFSShellReader.Close()
            PFSShellOutput = ProcessOutput
        End Using

        'Update UI when finished
        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub() InstallationStatusTextBlock.Text = "Files added to the game partition. Finalizing...")
        Else
            InstallationStatusTextBlock.Text = "Files added to the game partition. Finalizing..."
        End If

        Mouse.SetCursor(Cursors.Arrow)
        Thread.Sleep(200)

        'Set the current directory back
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory)

        '5. Modify the partition header
        ModifyPartitionHeader(PartitionName, True)
    End Sub

    Public Sub HDLDumpOutputDataHandler(sender As Object, e As DataReceivedEventArgs)
        If Not String.IsNullOrEmpty(e.Data) Then

            'Update UI and show hdl_dump installation progress

            'Progress status
            If InstallationStatusTextBlock.CheckAccess() = False Then
                InstallationStatusTextBlock.Dispatcher.BeginInvoke(Sub() InstallationStatusTextBlock.Text = e.Data)
            Else
                InstallationStatusTextBlock.Text = e.Data
            End If

            'Progress percentage
            Dim ProgressPercentage As Double = 0
            If Regex.Match(e.Data, "\d\d[%]+").Success Then
                If Double.TryParse(Regex.Match(e.Data, "\d\d[%]+").Value.Replace("%", ""), ProgressPercentage) = True Then
                    If InstallationProgressBar.CheckAccess() = False Then
                        InstallationProgressBar.Dispatcher.BeginInvoke(Sub() InstallationProgressBar.Value = ProgressPercentage)
                    Else
                        InstallationProgressBar.Value = ProgressPercentage
                    End If
                End If
            End If

        End If
    End Sub

    Private Sub HDL_Dump_Exited(sender As Object, e As EventArgs) Handles HDL_Dump.Exited
        HDL_Dump.CancelOutputRead()
        HDL_Dump.Dispose()

        If InstallForPS2 Then
            'Proceed to the creation of the game's PP partition
            If InstallationStatusTextBlock.CheckAccess() = False Then
                InstallationStatusTextBlock.Dispatcher.BeginInvoke(Sub() InstallationStatusTextBlock.Text = "Creating game PP partition ...")
            Else
                InstallationStatusTextBlock.Text = "Creating game PP partition ..."
            End If
            CreateGamePartition()
        End If
    End Sub

End Class
