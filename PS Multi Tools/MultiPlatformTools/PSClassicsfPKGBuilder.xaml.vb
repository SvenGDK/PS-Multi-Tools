Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports DiscUtils
Imports LibOrbisPkg.SFO
Imports Newtonsoft.Json.Linq

Public Class PSClassicsfPKGBuilder

#Region "PS1"

    Dim CurrentPS1GameID As String = ""

    Private Sub BrowsePS1Disc1Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS1Disc1Button.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PS1 BIN file.", .Multiselect = False, .Filter = "BIN (*.bin)|*.bin"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then

            Dim GameID As String = ""
            Dim GameTitle As String = ""

            'Get the game ID from the BIN file using strings
            Using WindowsCMD As New Process()
                WindowsCMD.StartInfo.FileName = "cmd"
                WindowsCMD.StartInfo.Arguments = "/c strings.exe /accepteula -nobanner -b 7340032 """ + OFD.FileName + """ | findstr BOOT"
                WindowsCMD.StartInfo.RedirectStandardOutput = True
                WindowsCMD.StartInfo.UseShellExecute = False
                WindowsCMD.StartInfo.CreateNoWindow = True
                WindowsCMD.Start()
                WindowsCMD.WaitForExit()

                Dim OutputReader As StreamReader = WindowsCMD.StandardOutput
                Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                If ProcessOutput.Length > 0 Then
                    For Each OutputLine In ProcessOutput
                        If OutputLine.Contains("BOOT =") Or OutputLine.Contains("BOOT=") Then

                            GameID = OutputLine.Replace("BOOT = cdrom:\", "").Replace("BOOT=cdrom:\", "").Replace("BOOT = cdrom:", "").Replace(";1", "").Replace("_", "-").Replace(".", "").Replace("MGS\", "").Trim()
                            GameID = UCase(GameID).Trim()

                            'Try to get a game title from the master list
                            If GameID.Length = 10 Then
                                GameTitle = PS1Game.GetPS1GameTitleFromDatabaseList(GameID)
                            End If

                            Exit For
                        End If
                    Next
                End If
            End Using

            If Not String.IsNullOrEmpty(GameID) Then
                CurrentPS1GameID = GameID.Trim()
                PS1NPTitleTextBox.Text = GameID.Replace("-"c, "").Trim()
                PS1SelectedDisc1TextBox.Text = OFD.FileName
            Else
                If MsgBox("Cannot find the game ID within the PS1 BIN file, do you want to use this file anyway ?" + vbCrLf + "Do not forget to specify a NP title and title if you do so.", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                    PS1SelectedDisc1TextBox.Text = OFD.FileName
                End If
            End If
            If Not String.IsNullOrEmpty(GameTitle) Then
                PS1TitleTextBox.Text = GameTitle
            End If
        Else
            MsgBox("Aborted.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub BrowsePS1IconButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS1IconButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PNG icon file.", .Multiselect = False, .Filter = "PNG (*.png)|*.png"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPS1IconTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePS1StartupImageButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS1StartupImageButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PNG Startup Background file.", .Multiselect = False, .Filter = "PNG (*.png)|*.png"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPS1BGTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePS1Disc2Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS1Disc2Button.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PS1 BIN file.", .Multiselect = False, .Filter = "BIN (*.bin)|*.bin"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            PS1SelectedDisc2TextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePS1Disc3Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS1Disc3Button.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PS1 BIN file.", .Multiselect = False, .Filter = "BIN (*.bin)|*.bin"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            PS1SelectedDisc3TextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePS1Disc4Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS1Disc4Button.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PS1 BIN file.", .Multiselect = False, .Filter = "BIN (*.bin)|*.bin"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            PS1SelectedDisc4TextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePS1TXTConfigButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS1TXTConfigButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a TXT config file.", .Multiselect = False, .Filter = "TXT (*.txt)|*.txt"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPS1TXTConfigTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePS1LUAConfigButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS1LUAConfigButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a LUA config file.", .Multiselect = False, .Filter = "LUA (*.lua)|*.lua"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPS1LUAConfigTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BuildPS1fPKGButton_Click(sender As Object, e As RoutedEventArgs) Handles BuildPS1fPKGButton.Click

        'Checks before fPKG creation
        If String.IsNullOrEmpty(PS1SelectedDisc1TextBox.Text) Then
            MsgBox("No disc 1 specified, fPKG creation will be aborted.", MsgBoxStyle.Critical, "Cannot create fPKG")
            Exit Sub
        End If
        If String.IsNullOrEmpty(PS1TitleTextBox.Text) Then
            MsgBox("No game title specified, fPKG creation will be aborted.", MsgBoxStyle.Critical, "Cannot create fPKG")
            Exit Sub
        End If
        If String.IsNullOrEmpty(PS1NPTitleTextBox.Text) Then
            MsgBox("No NP title specified, fPKG creation will be aborted.", MsgBoxStyle.Critical, "Cannot create fPKG")
            Exit Sub
        End If
        If PS1NPTitleTextBox.Text.Length <> 9 Then
            MsgBox("'NP Title' length mismatching, only 9 characters are allowed, fPKG creation will be aborted.", MsgBoxStyle.Critical, "Cannot create fPKG")
            Exit Sub
        End If

        Dim FBD As New FolderBrowserDialog() With {.Description = "Please select an output folder", .ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then

            Dim PKGOutputFolder As String = FBD.SelectedPath
            Dim GameCacheDirectory As String = Environment.CurrentDirectory + "\Cache\PS1fPKG"

            'Remove previous fPKG creation & re-create the PS1fPKG cache folder
            If Directory.Exists(GameCacheDirectory) Then
                Directory.Delete(GameCacheDirectory, True)
            End If
            If File.Exists(Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4") Then
                File.Delete(Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4")
            End If
            Directory.CreateDirectory(GameCacheDirectory)

            'Copy the PS1 emulator to the cache directory
            Utils.CopyDirectory(Environment.CurrentDirectory + "\Tools\PS4\emus\ps1hd", GameCacheDirectory, True)

            'Copy the selected icon and background to the cache directory
            If Not Directory.Exists(GameCacheDirectory + "\sce_sys") Then
                Directory.CreateDirectory(GameCacheDirectory + "\sce_sys")
            End If
            If Not String.IsNullOrEmpty(SelectedPS1IconTextBox.Text) Then
                Using NewFileStream As New FileStream(SelectedPS1IconTextBox.Text, FileMode.Open, FileAccess.Read)
                    Utils.ConvertTo24bppPNG(Utils.ResizeAsImage(System.Drawing.Image.FromStream(NewFileStream), 512, 512)).Save(GameCacheDirectory + "\sce_sys\icon0.png", ImageFormat.Png)
                End Using
            End If
            If Not String.IsNullOrEmpty(SelectedPS1BGTextBox.Text) Then
                Using NewFileStream As New FileStream(SelectedPS1BGTextBox.Text, FileMode.Open, FileAccess.Read)
                    Utils.ConvertTo24bppPNG(Utils.ResizeAsImage(System.Drawing.Image.FromStream(NewFileStream), 1920, 1080)).Save(GameCacheDirectory + "\sce_sys\pic0.png", ImageFormat.Png)
                End Using
            End If

            Dim Disc1CueFile As String = ""
            Dim Disc2CueFile As String = ""
            Dim Disc3CueFile As String = ""
            Dim Disc4CueFile As String = ""

            'PS1 Emulator configuration
            Using ConfigWriter As New StreamWriter(GameCacheDirectory + "\config-title.txt", False)
                ConfigWriter.WriteLine("--ps4-trophies=0")
                ConfigWriter.WriteLine("--ps5-uds=0")
                ConfigWriter.WriteLine("--trophies=0")

                'Add disc information
                Disc1CueFile = PS1SelectedDisc1TextBox.Text.Replace(".bin", ".cue")
                ConfigWriter.WriteLine("--image=""data/disc1.bin""")

                If Not String.IsNullOrEmpty(PS1SelectedDisc2TextBox.Text) Then
                    Disc2CueFile = PS1SelectedDisc2TextBox.Text.Replace(".bin", ".cue")
                    ConfigWriter.WriteLine("--image=""data/disc2.bin""")
                End If
                If Not String.IsNullOrEmpty(PS1SelectedDisc3TextBox.Text) Then
                    Disc3CueFile = PS1SelectedDisc3TextBox.Text.Replace(".bin", ".cue")
                    ConfigWriter.WriteLine("--image=""data/disc3.bin""")
                End If
                If Not String.IsNullOrEmpty(PS1SelectedDisc4TextBox.Text) Then
                    Disc4CueFile = PS1SelectedDisc4TextBox.Text.Replace(".bin", ".cue")
                    ConfigWriter.WriteLine("--image=""data/disc4.bin""")
                End If

                'Check for libcrypt protection
                Dim FormattedGameID As String = ""
                If String.IsNullOrEmpty(CurrentPS1GameID) Then
                    FormattedGameID = PS1NPTitleTextBox.Text.Insert(4, "_").Insert(8, ".")
                Else
                    FormattedGameID = CurrentPS1GameID.Replace("-", "_").Insert(8, ".")
                End If
                Dim ProtectionValue As String = PS1Game.IsGameProtected(FormattedGameID)
                If Not String.IsNullOrEmpty(ProtectionValue) Then
                    If MsgBox("This game is Libcrypt protected. Do you want to add --libcrypt to the configuration file ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                        ConfigWriter.WriteLine("--libcrypt=" + ProtectionValue)
                    End If
                End If

                'Check for LUA config
                If Not String.IsNullOrEmpty(SelectedPS1LUAConfigTextBox.Text) Then
                    ConfigWriter.WriteLine("--ps1-title-id=" + PS1NPTitleTextBox.Text)
                    File.Copy(SelectedPS1LUAConfigTextBox.Text, GameCacheDirectory + "\scripts\" + PS1NPTitleTextBox.Text + ".lua", True)
                End If

                'Graphics & other configs
                ConfigWriter.WriteLine("--scale=" + PS1UpscalingComboBox.Text)
                If PS1SkipBootlogoCheckBox.IsChecked Then
                    ConfigWriter.WriteLine("--bios-hide-sce-osd=1")
                End If
                If PS1GunconCheckBox.IsChecked Then
                    ConfigWriter.WriteLine("--guncon")
                End If
                If PS1Force60HzCheckBox.IsChecked Then
                    ConfigWriter.WriteLine("--gpu-scanout-fps-override=60")
                End If
                If PS1EmulateAnalogSticksCheckBox.IsChecked Then
                    ConfigWriter.WriteLine("--sim-analog-pad=0x2020")
                End If

                'Check for TXT config
                If Not String.IsNullOrEmpty(SelectedPS1TXTConfigTextBox.Text) Then
                    ConfigWriter.WriteLine("#User imported config")
                    ConfigWriter.WriteLine(File.ReadAllText(SelectedPS1TXTConfigTextBox.Text))
                End If
            End Using

            'Create a new PARAM.SFO file
            Dim NewPS4ParamSFO As New ParamSfo()
            NewPS4ParamSFO.SetValue("APP_TYPE", SfoEntryType.Integer, "1", 4)
            NewPS4ParamSFO.SetValue("APP_VER", SfoEntryType.Utf8, "01.00", 8)
            NewPS4ParamSFO.SetValue("ATTRIBUTE", SfoEntryType.Integer, "0", 4)
            NewPS4ParamSFO.SetValue("CATEGORY", SfoEntryType.Utf8, "gd", 4)
            NewPS4ParamSFO.SetValue("CONTENT_ID", SfoEntryType.Utf8, "UP9000-" + PS1NPTitleTextBox.Text + "_00-" + PS1NPTitleTextBox.Text + "PS1FPKG", 48)
            NewPS4ParamSFO.SetValue("DOWNLOAD_DATA_SIZE", SfoEntryType.Integer, "0", 4)
            NewPS4ParamSFO.SetValue("FORMAT", SfoEntryType.Utf8, "obs", 4)
            NewPS4ParamSFO.SetValue("PARENTAL_LEVEL", SfoEntryType.Integer, "5", 4)
            NewPS4ParamSFO.SetValue("SYSTEM_VER", SfoEntryType.Integer, "0", 4)
            NewPS4ParamSFO.SetValue("TITLE", SfoEntryType.Utf8, PS1TitleTextBox.Text, 128)
            NewPS4ParamSFO.SetValue("TITLE_ID", SfoEntryType.Utf8, PS1NPTitleTextBox.Text, 12)
            NewPS4ParamSFO.SetValue("VERSION", SfoEntryType.Utf8, "01.00", 8)

            File.WriteAllBytes(GameCacheDirectory + "\sce_sys\param.sfo", NewPS4ParamSFO.Serialize)

            'Create TOC file
            Dim CUE2TOCProcess As New Process()
            CUE2TOCProcess.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\cue2toc.exe"
            CUE2TOCProcess.StartInfo.Arguments = """" + Disc1CueFile + """"
            CUE2TOCProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(Disc1CueFile)
            CUE2TOCProcess.StartInfo.UseShellExecute = False
            CUE2TOCProcess.StartInfo.CreateNoWindow = True
            CUE2TOCProcess.Start()
            CUE2TOCProcess.WaitForExit()

            File.Move(Path.GetDirectoryName(PS1SelectedDisc1TextBox.Text) + "\" + Path.GetFileNameWithoutExtension(PS1SelectedDisc1TextBox.Text) + ".toc", GameCacheDirectory + "\data\disc1.toc")

            'Generate GP4 project
            Dim NewProcess As New Process()
            NewProcess.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\PS4\gengp4_patch.exe"
            NewProcess.StartInfo.Arguments = """" + GameCacheDirectory + """"
            NewProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
            NewProcess.StartInfo.CreateNoWindow = True
            NewProcess.Start()
            NewProcess.WaitForExit()

            'Modify the GP4 project and add disc info
            Dim Disc1CuePath As String = vbCrLf + "    <file targ_path=""data/disc1.cue"" orig_path=""" + Disc1CueFile + """ pfs_compression=""enable""/>"
            Dim Disc1BinPath As String = vbCrLf + "    <file targ_path=""data/disc1.bin"" orig_path=""" + PS1SelectedDisc1TextBox.Text + """ pfs_compression=""enable""/>"
            File.WriteAllText(Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4", File.ReadAllText(Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4").Replace("<?xml version=""1.1""", "<?xml version=""1.0"""))
            File.WriteAllText(Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4", File.ReadAllText(Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4").Replace("<scenarios default_id=""1"">", "<scenarios default_id=""0"">"))
            File.WriteAllText(Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4", File.ReadAllText(Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4").Replace("</files>", Disc1CuePath + Disc1BinPath + vbCrLf + "</files>"))

            If Not String.IsNullOrEmpty(PS1SelectedDisc2TextBox.Text) Then
                Dim Disc2CuePath As String = vbCrLf + "    <file targ_path=""data/disc2.cue"" orig_path=""" + Disc2CueFile + """ pfs_compression=""enable""/>"
                Dim Disc2BinPath As String = vbCrLf + "    <file targ_path=""data/disc2.bin"" orig_path=""" + PS1SelectedDisc2TextBox.Text + """ pfs_compression=""enable""/>"
                File.WriteAllText(Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4", File.ReadAllText(Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4").Replace("</files>", Disc2CuePath + Disc2BinPath + vbCrLf + "</files>"))
            End If
            If Not String.IsNullOrEmpty(PS1SelectedDisc2TextBox.Text) Then
                Dim Disc3CuePath As String = vbCrLf + "    <file targ_path=""data/disc3.cue"" orig_path=""" + Disc3CueFile + """ pfs_compression=""enable""/>"
                Dim Disc3BinPath As String = vbCrLf + "    <file targ_path=""data/disc3.bin"" orig_path=""" + PS1SelectedDisc3TextBox.Text + """ pfs_compression=""enable""/>"
                File.WriteAllText(Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4", File.ReadAllText(Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4").Replace("</files>", Disc3CuePath + Disc3BinPath + vbCrLf + "</files>"))
            End If
            If Not String.IsNullOrEmpty(PS1SelectedDisc2TextBox.Text) Then
                Dim Disc4CuePath As String = vbCrLf + "    <file targ_path=""data/disc4.cue"" orig_path=""" + Disc4CueFile + """ pfs_compression=""enable""/>"
                Dim Disc4BinPath As String = vbCrLf + "    <file targ_path=""data/disc4.bin"" orig_path=""" + PS1SelectedDisc4TextBox.Text + """ pfs_compression=""enable""/>"
                File.WriteAllText(Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4", File.ReadAllText(Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4").Replace("</files>", Disc4CuePath + Disc4BinPath + vbCrLf + "</files>"))
            End If

            MsgBox("All files ready for PKG creation.", MsgBoxStyle.Information)

            'Create the fPKG
            Dim PKGBuilderProcessOutput As String
            Dim PKGBuilderProcess As New Process()
            PKGBuilderProcess.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\PS4\mod-pub\orbis-pub-cmd-3.38.exe"
            PKGBuilderProcess.StartInfo.Arguments = "img_create --oformat pkg --skip_digest --no_progress_bar """ + Environment.CurrentDirectory + "\Cache\PS1fPKG.gp4"" """ + PKGOutputFolder + """"
            PKGBuilderProcess.StartInfo.UseShellExecute = False
            PKGBuilderProcess.StartInfo.RedirectStandardOutput = True
            PKGBuilderProcess.StartInfo.CreateNoWindow = True
            PKGBuilderProcess.Start()
            PKGBuilderProcess.WaitForExit()

            Using NewStreamReader As StreamReader = PKGBuilderProcess.StandardOutput
                PKGBuilderProcessOutput = NewStreamReader.ReadToEnd()
            End Using
            If PKGBuilderProcessOutput.Contains("Create image Process finished with warning") Then
                If MsgBox("PKG created!" + vbCrLf + "Do you want to open the output folder ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                    Process.Start("explorer", FBD.SelectedPath)
                End If
            Else
                MsgBox("PKG creation failed!", MsgBoxStyle.Information)
                MsgBox(PKGBuilderProcessOutput)
            End If
        Else
            MsgBox("Aborted", MsgBoxStyle.Information)
        End If

    End Sub

#End Region

#Region "PS2"

    Dim CurrentPS2GameID As String = ""
    Dim CurrentPS2GameCRC As String = ""

    Private Sub BrowsePS2Disc1Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS2Disc1Button.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PS2 ISO file.", .Multiselect = False, .Filter = "ISO (*.iso)|*.iso"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then

            Dim PS2GameID As String = PS2Game.GetPS2GameID(OFD.FileName)
            Dim ExtractedPS2GameELFPath As String = GetELFfromISO(OFD.FileName, PS2GameID)
            CurrentPS2GameID = PS2GameID

            Dim PS2GameCRC As String = GetGameCRC(ExtractedPS2GameELFPath)
            PS2GameCRC = Regex.Replace(PS2GameCRC, "[^a-zA-Z0-9]", "")
            CurrentPS2GameCRC = PS2GameCRC

            If Not String.IsNullOrEmpty(PS2GameID) AndAlso Not String.IsNullOrEmpty(PS2GameCRC) Then

                If IsConfigAvailable(PS2GameID, Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\configs_txt.dat") Then
                    PS2AddTXTConfigFromDatabaseCheckBox.IsChecked = True
                    PS2AddTXTConfigFromDatabaseCheckBox.Visibility = Visibility.Visible
                Else
                    PS2AddTXTConfigFromDatabaseCheckBox.IsChecked = False
                    PS2AddTXTConfigFromDatabaseCheckBox.Visibility = Visibility.Hidden
                End If

                If IsConfigAvailable(PS2GameID, Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\configs_lua.dat") Then
                    PS2AddLUAConfigFromDatabaseCheckBox.IsChecked = True
                    PS2AddLUAConfigFromDatabaseCheckBox.Visibility = Visibility.Visible
                Else
                    PS2AddLUAConfigFromDatabaseCheckBox.IsChecked = False
                    PS2AddLUAConfigFromDatabaseCheckBox.Visibility = Visibility.Hidden
                End If

                If IsConfigAvailable(PS2GameID + ".CONFIG", Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\configs_ps3.dat") Then
                    PS2AddPS3ConfigFromDatabaseCheckBox.IsChecked = True
                    PS2AddPS3ConfigFromDatabaseCheckBox.Visibility = Visibility.Visible
                Else
                    PS2AddPS3ConfigFromDatabaseCheckBox.IsChecked = False
                    PS2AddPS3ConfigFromDatabaseCheckBox.Visibility = Visibility.Hidden
                End If

                If IsConfigAvailable(PS2GameCRC + ".lua", Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\widescreen.dat") Then
                    PS2UseWidescreenPatchCheckBox.IsChecked = True
                    PS2UseWidescreenPatchCheckBox.IsEnabled = True
                Else
                    PS2UseWidescreenPatchCheckBox.IsEnabled = False
                End If

                PS2TitleTextBox.Text = PS2Game.GetPS2GameTitleFromDatabaseList(PS2GameID.Replace(".", "").Replace("_", "-").Trim())
                PS2NPTitleTextBox.Text = PS2GameID.Replace(".", "").Replace("_", "").Trim()
                SelectedDisc1TextBox.Text = OFD.FileName
            Else
                If MsgBox("Could not find the PS2 game ID within the ISO file." + vbCrLf + "Do you want to use this file anyway ?", MsgBoxStyle.YesNo, "PS2 Game ID not found") = MsgBoxResult.Yes Then
                    SelectedDisc1TextBox.Text = OFD.FileName
                End If
            End If

        End If
    End Sub

    Public Shared Function IsConfigAvailable(GameID As String, ConfigDatabaseFile As String) As Boolean
        Dim Exists As Boolean
        Try
            Using NewFileStream As New FileStream(ConfigDatabaseFile, FileMode.Open, FileAccess.Read)
                Dim NewUdfReader As New Udf.UdfReader(NewFileStream, 2048)
                Try
                    NewUdfReader.OpenFile(GameID, FileMode.Open)
                    Exists = True
                Catch ex As Exception
                    Exists = False
                End Try
            End Using
        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Exists = False
        End Try
        Return Exists
    End Function

    Public Shared Function GetELFfromISO(GameISOFile As String, GameELFName As String) As String
        Dim ExtractedELFPath As String = ""

        Dim CacheDir As String = Environment.CurrentDirectory + "\Cache"
        If Not Directory.Exists(CacheDir) Then
            Directory.CreateDirectory(CacheDir)
        End If

        Try
            Using NewFileStream As New FileStream(GameISOFile, FileMode.Open, FileAccess.Read)
                Dim NewCDReader As New Iso9660.CDReader(NewFileStream, True)
                Try
                    Dim ELFExtractionFileStream As New FileStream(CacheDir + "\" + GameELFName, FileMode.Create)
                    NewCDReader.OpenFile(GameELFName, FileMode.Open).CopyTo(ELFExtractionFileStream)

                    ExtractedELFPath = CacheDir + "\" + GameELFName
                Catch ex As Exception
                    ExtractedELFPath = ""
                End Try
            End Using
        Catch ex As Exception
            Return ExtractedELFPath
        End Try
        Return ExtractedELFPath
    End Function

    Public Shared Function GetGameCRC(PS2GamePath As String) As String
        Dim NewProcessStartInfo As New ProcessStartInfo(Environment.CurrentDirectory + "\Tools\PS4\crc.exe", PS2GamePath) With {.UseShellExecute = False, .RedirectStandardOutput = True, .CreateNoWindow = True, .Arguments = """" + PS2GamePath + """"}
        Dim NewProcess As New Process With {.StartInfo = NewProcessStartInfo}
        NewProcess.Start()
        Using NewStreamReader As StreamReader = NewProcess.StandardOutput
            Return NewStreamReader.ReadToEnd().Replace("crc:", "").Trim()
        End Using
    End Function

    Public Shared Function ExtractFileFromISO(GameISOFile As String, FileToExtract As String, FileDestinationPath As String) As String
        Dim ReturnedFileDestinationPath As String
        Try
            Using NewFileStream As New FileStream(GameISOFile, FileMode.Open, FileAccess.Read)
                Dim NewUdfReader As New Udf.UdfReader(NewFileStream, 2048)
                Try
                    Dim NewFileStream2 As New FileStream(FileDestinationPath, FileMode.Create)
                    NewUdfReader.OpenFile(FileToExtract, FileMode.Open).CopyTo(NewFileStream2)
                    NewFileStream2.Close()
                    ReturnedFileDestinationPath = FileDestinationPath
                Catch exception1 As Exception
                    ReturnedFileDestinationPath = ""
                End Try
            End Using
        Catch exception3 As Exception
            ReturnedFileDestinationPath = ""
        End Try
        Return ReturnedFileDestinationPath
    End Function

    Public Shared Function GetPNACHFromDAT(DATFile As String, FileToExtract As String) As String
        Dim PNACHString As String
        Try
            Using NewFileStream As New FileStream(DATFile, FileMode.Open, FileAccess.Read)
                Dim NewUdfReader As New Udf.UdfReader(NewFileStream, 2048)
                Try
                    PNACHString = New StreamReader(NewUdfReader.OpenFile(FileToExtract, FileMode.Open)).ReadToEnd()
                Catch exception1 As Exception
                    PNACHString = ""
                End Try
            End Using
        Catch exception3 As Exception
            PNACHString = ""
        End Try
        Return PNACHString
    End Function

    Private Sub BrowsePS2IconButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS2IconButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PNG icon file.", .Multiselect = False, .Filter = "PNG (*.png)|*.png"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedIconTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePS2StartupImageButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS2StartupImageButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PNG background file.", .Multiselect = False, .Filter = "PNG (*.png)|*.png"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedStartupImageTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePS2MCButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS2MCButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PS2 Memory Card file.", .Multiselect = False, .Filter = "VM2 Card (*.vm2)|*.vm2|PS2 Card (*.ps2)|*.ps2|BIN Card (*.bin)|*.bin"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPS2MemoryCardTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePS2TXTConfigButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS2TXTConfigButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a TXT config file.", .Multiselect = False, .Filter = "TXT (*.txt)|*.txt"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPS2TXTConfigTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePS2LUAConfigButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS2LUAConfigButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a LUA config file.", .Multiselect = False, .Filter = "LUA (*.lua)|*.lua"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPS2LUAConfigTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePS2Disc2Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS2Disc2Button.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PS2 ISO file.", .Multiselect = False, .Filter = "ISO (*.iso)|*.iso"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedDisc2TextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePS2Disc3Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS2Disc3Button.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PS2 ISO file.", .Multiselect = False, .Filter = "ISO (*.iso)|*.iso"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedDisc3TextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePS2Disc4Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS2Disc4Button.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PS2 ISO file.", .Multiselect = False, .Filter = "ISO (*.iso)|*.iso"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedDisc4TextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePS2Disc5Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePS2Disc5Button.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PS2 ISO file.", .Multiselect = False, .Filter = "ISO (*.iso)|*.iso"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedDisc5TextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BuildPS2fPKGButton_Click(sender As Object, e As RoutedEventArgs) Handles BuildPS2fPKGButton.Click

        'Checks before fPKG creation
        If String.IsNullOrEmpty(SelectedDisc1TextBox.Text) Then
            MsgBox("No disc 1 specified, fPKG creation will be aborted.", MsgBoxStyle.Critical, "Cannot create fPKG")
            Exit Sub
        End If
        If String.IsNullOrEmpty(PS2TitleTextBox.Text) Then
            MsgBox("No game title specified, fPKG creation will be aborted.", MsgBoxStyle.Critical, "Cannot create fPKG")
            Exit Sub
        End If
        If String.IsNullOrEmpty(PS2NPTitleTextBox.Text) Then
            MsgBox("No NP title specified, fPKG creation will be aborted.", MsgBoxStyle.Critical, "Cannot create fPKG")
            Exit Sub
        End If
        If PS2NPTitleTextBox.Text.Length <> 9 Then
            MsgBox("'NP Title' length mismatching, only 9 characters are allowed, fPKG creation will be aborted.", MsgBoxStyle.Critical, "Cannot create fPKG")
            Exit Sub
        End If

        'Get disc count
        Dim DiscCount As Integer = 0
        If Not String.IsNullOrEmpty(SelectedDisc1TextBox.Text) Then
            DiscCount += 1
        End If
        If Not String.IsNullOrEmpty(SelectedDisc2TextBox.Text) Then
            DiscCount += 1
        End If
        If Not String.IsNullOrEmpty(SelectedDisc3TextBox.Text) Then
            DiscCount += 1
        End If
        If Not String.IsNullOrEmpty(SelectedDisc4TextBox.Text) Then
            DiscCount += 1
        End If
        If Not String.IsNullOrEmpty(SelectedDisc5TextBox.Text) Then
            DiscCount += 1
        End If

        'Select an output folder
        Dim FBD As New FolderBrowserDialog() With {.Description = "Please select an output folder", .ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then

            Dim PKGOutputFolder As String = FBD.SelectedPath
            Dim SelectedPS2Emulator As String = PS2EmulatorComboBox.Text
            Dim FullPS2GameID As String = CurrentPS2GameID.Replace(".", "").Replace("_", "-").Trim()
            Dim GameCacheDirectory As String = Environment.CurrentDirectory + "\Cache\PS2fPKG"

            'Remove previous fPKG creation & re-create the PS2fPKG cache folder
            If Directory.Exists(GameCacheDirectory) Then
                Directory.Delete(GameCacheDirectory, True)
            End If
            If File.Exists(Environment.CurrentDirectory + "\Cache\PS2fPKG.gp4") Then
                File.Delete(Environment.CurrentDirectory + "\Cache\PS2fPKG.gp4")
            End If
            Directory.CreateDirectory(GameCacheDirectory)

            'Copy the selected PS2 emulator to the cache directory
            Utils.CopyDirectory(Environment.CurrentDirectory + "\Tools\PS4\emus\" + SelectedPS2Emulator, GameCacheDirectory, True)

            'Copy the selected icon and background to the cache directory
            If Not Directory.Exists(GameCacheDirectory + "\sce_sys") Then
                Directory.CreateDirectory(GameCacheDirectory + "\sce_sys")
            End If
            If Not String.IsNullOrEmpty(SelectedIconTextBox.Text) Then
                Using NewFileStream As New FileStream(SelectedIconTextBox.Text, FileMode.Open, FileAccess.Read)
                    Utils.ConvertTo24bppPNG(Utils.ResizeAsImage(System.Drawing.Image.FromStream(NewFileStream), 512, 512)).Save(GameCacheDirectory + "\sce_sys\icon0.png", ImageFormat.Png)
                End Using
            End If
            If Not String.IsNullOrEmpty(SelectedStartupImageTextBox.Text) Then
                Using NewFileStream As New FileStream(SelectedStartupImageTextBox.Text, FileMode.Open, FileAccess.Read)
                    Utils.ConvertTo24bppPNG(Utils.ResizeAsImage(System.Drawing.Image.FromStream(NewFileStream), 1920, 1080)).Save(GameCacheDirectory + "\sce_sys\pic0.png", ImageFormat.Png)
                End Using
            End If

            'Create a new PARAM.SFO file
            Dim NewPS4ParamSFO As New ParamSfo()
            NewPS4ParamSFO.SetValue("APP_TYPE", SfoEntryType.Integer, "1", 4)
            NewPS4ParamSFO.SetValue("APP_VER", SfoEntryType.Utf8, "01.00", 8)
            NewPS4ParamSFO.SetValue("ATTRIBUTE", SfoEntryType.Integer, "0", 4)
            NewPS4ParamSFO.SetValue("CATEGORY", SfoEntryType.Utf8, "gd", 4)
            NewPS4ParamSFO.SetValue("CONTENT_ID", SfoEntryType.Utf8, String.Concat(New String() {"UP9000-", PS2NPTitleTextBox.Text, "_00-", CurrentPS2GameID.Replace(".", "").Replace("_", "").Trim(), "0000001"}), 48)
            NewPS4ParamSFO.SetValue("DOWNLOAD_DATA_SIZE", SfoEntryType.Integer, "0", 4)
            NewPS4ParamSFO.SetValue("FORMAT", SfoEntryType.Utf8, "obs", 4)
            NewPS4ParamSFO.SetValue("PARENTAL_LEVEL", SfoEntryType.Integer, "5", 4)
            NewPS4ParamSFO.SetValue("REMOTE_PLAY_KEY_ASSIGN", SfoEntryType.Integer, "0", 4)
            NewPS4ParamSFO.SetValue("SYSTEM_VER", SfoEntryType.Integer, "0", 4)
            NewPS4ParamSFO.SetValue("TITLE", SfoEntryType.Utf8, PS2TitleTextBox.Text, 128)
            NewPS4ParamSFO.SetValue("TITLE_ID", SfoEntryType.Utf8, PS2NPTitleTextBox.Text, 12)
            NewPS4ParamSFO.SetValue("VERSION", SfoEntryType.Utf8, "01.00", 8)

            File.WriteAllBytes(GameCacheDirectory + "\sce_sys\param.sfo", NewPS4ParamSFO.Serialize)

            'Create a new PS2 emulator configuration file
            Dim NewPS2EmulatorConfig As String() = New String() {"--path-vmc=""/tmp/vmc""" + vbCrLf + "--config-local-lua=""""" + vbCrLf + "--ps2-title-id=",
                FullPS2GameID,
                vbCrLf + "--max-disc-num=", DiscCount.ToString(),
                vbCrLf + "--gs-uprender=", LCase(PS2UprenderComboBox.Text),
                vbCrLf + "--gs-upscale=", LCase(PS2UpscalingComboBox.Text),
                vbCrLf + "--host-audio=1" + vbCrLf +
                "--rom=""PS20220WD20050620.crack""" + vbCrLf +
                "--verbose-cdvd-reads=0" + vbCrLf +
                "--host-display-mode=", LCase(PS2DisplayModeComboBox.Text)}
            Dim NewPS2EmulatorConfigContent As String = String.Concat(NewPS2EmulatorConfig)

            'Reset on disc change config
            If Not PS2RestartEmulatorOnDiscChangeCheckBox.IsChecked Then
                NewPS2EmulatorConfigContent = NewPS2EmulatorConfigContent + vbCrLf + "#Disable emu reset on disc change" + vbCrLf + "--switch-disc-reset=0"
            End If

            'Multitap config
            If PS2MultitapComboBox.SelectedIndex = 1 Then
                NewPS2EmulatorConfigContent = NewPS2EmulatorConfigContent + vbCrLf + "#Enable Multitap" + vbCrLf + "--mtap1=always"
            ElseIf PS2MultitapComboBox.SelectedIndex = 2 Then
                NewPS2EmulatorConfigContent = NewPS2EmulatorConfigContent + vbCrLf + "#Enable Multitap" + vbCrLf + "--mtap2=always"
            ElseIf PS2MultitapComboBox.SelectedIndex = 3 Then
                NewPS2EmulatorConfigContent = (NewPS2EmulatorConfigContent + vbCrLf + "#Enable Multitap" + vbCrLf + "--mtap1=always" + vbCrLf + "--mtap2=always")
            End If

            'Check for PS3 config file
            If PS2AddPS3ConfigFromDatabaseCheckBox.IsChecked Then
                If File.Exists(Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\ps3\" + CurrentPS2GameID + ".CONFIG") Then

                    NewPS2EmulatorConfigContent = NewPS2EmulatorConfigContent + vbCrLf + "--lopnor-config=1"

                    'Create patches directory
                    If Not Directory.Exists(GameCacheDirectory + "\patches\" + FullPS2GameID) Then
                        Directory.CreateDirectory(GameCacheDirectory + "\patches\" + FullPS2GameID)
                    End If

                    File.Copy(Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\ps3\" + CurrentPS2GameID + ".CONFIG",
                              GameCacheDirectory + "\patches\" + FullPS2GameID + "\" + FullPS2GameID + "_lopnor.cfgbin", True)

                ElseIf IsConfigAvailable(CurrentPS2GameID + ".CONFIG", Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\configs_ps3.dat") Then

                    If Not Directory.Exists(GameCacheDirectory + "\patches\" + FullPS2GameID) Then
                        Directory.CreateDirectory(GameCacheDirectory + "\patches\" + FullPS2GameID)
                    End If

                    ExtractFileFromISO(Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\configs_ps3.dat",
                                       CurrentPS2GameID + ".CONFIG", GameCacheDirectory + "\patches\" + FullPS2GameID + "\" + FullPS2GameID + "_lopnor.cfgbin")

                End If
            End If

            'Widescreen Patch cnofig
            If PS2UseWidescreenPatchCheckBox.IsChecked Then
                Dim WidescreenPatch As String = ""

                If File.Exists(Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\widescreen\" + CurrentPS2GameCRC + ".lua") Then
                    WidescreenPatch = File.ReadAllText(Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\widescreen\" + CurrentPS2GameCRC + ".lua")
                ElseIf IsConfigAvailable(CurrentPS2GameCRC + ".lua", Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\widescreen.dat") Then
                    WidescreenPatch = GetPNACHFromDAT(Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\widescreen.dat", CurrentPS2GameCRC + ".lua")
                End If

                If Not String.IsNullOrEmpty(WidescreenPatch) Then
                    NewPS2EmulatorConfigContent = NewPS2EmulatorConfigContent + vbCrLf + "--path-trophydata=""/app0/trophy_data"""

                    If Not Directory.Exists(GameCacheDirectory + "\trophy_data\") Then
                        Directory.CreateDirectory(GameCacheDirectory + "\trophy_data\")
                    End If

                    File.WriteAllText(GameCacheDirectory + "\trophy_data\" + FullPS2GameID + "_trophies.lua", WidescreenPatch)
                End If
            End If

            'Copy lua_include to cache directory
            Utils.CopyDirectory(Environment.CurrentDirectory + "\Tools\PS4\lua_include", GameCacheDirectory + "\lua_include", True)

            'Check for LUA config
            If Not String.IsNullOrEmpty(SelectedPS2LUAConfigTextBox.Text) Then

                NewPS2EmulatorConfigContent = NewPS2EmulatorConfigContent + vbCrLf + "--path-patches=""/app0/patches"

                'Create patches directory if it doesn't exist yet
                If Not Directory.Exists(GameCacheDirectory + "\patches\" + FullPS2GameID) Then
                    Directory.CreateDirectory(GameCacheDirectory + "\patches\" + FullPS2GameID)
                End If

                File.Copy(SelectedPS2LUAConfigTextBox.Text, GameCacheDirectory + "\patches\" + FullPS2GameID + "_config.lua", True)
            Else
                If PS2AddLUAConfigFromDatabaseCheckBox.IsChecked Then
                    If IsConfigAvailable(CurrentPS2GameID, Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\configs_lua.dat") Then

                        NewPS2EmulatorConfigContent = NewPS2EmulatorConfigContent + vbCrLf + "--path-patches=""/app0/patches"

                        If Not Directory.Exists(GameCacheDirectory + "\patches\" + FullPS2GameID) Then
                            Directory.CreateDirectory(GameCacheDirectory + "\patches\" + FullPS2GameID)
                        End If

                        ExtractFileFromISO(Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\configs_lua.dat", CurrentPS2GameID, GameCacheDirectory + "\patches\" + FullPS2GameID + "_config.lua")
                    End If
                End If
            End If

            'Emulator fixes
            Dim PS2EmulatorFixes As String = ""
            If PS2ImproveSpeedCheckBox.IsChecked Then
                PS2EmulatorFixes = PS2EmulatorFixes + vbCrLf + "#Improve Speed" + vbCrLf + "-vu0-opt-flags=1" + vbCrLf + "--vu1-opt-flags=1" + vbCrLf + "--cop2-opt-flags=1" + vbCrLf + "--vu0-const-prop=0" + vbCrLf + "--vu1-const-prop=0" + vbCrLf + "--vu1-jr-cache-policy=newprog" + vbCrLf + "--vu1-jalr-cache-policy=newprog" + vbCrLf + "--vu0-jr-cache-policy=newprog" + vbCrLf + "--vu0-jalr-cache-policy=newprog"
            End If
            If PS2FixGraphicsCheckBox.IsChecked Then
                PS2EmulatorFixes = PS2EmulatorFixes + vbCrLf + "#Fix Graphics" + vbCrLf + "--fpu-no-clamping=0" + vbCrLf + "--fpu-clamp-results=1" + vbCrLf + "--vu0-no-clamping=0" + vbCrLf + "--vu0-clamp-results=1" + vbCrLf + "--vu1-no-clamping=0" + vbCrLf + "--vu1-clamp-results=1" + vbCrLf + "--cop2-no-clamping=0" + vbCrLf + "--cop2-clamp-results=1"
            End If
            If PS2DisableMTVUCheckBox.IsChecked Then
                PS2EmulatorFixes = PS2EmulatorFixes + vbCrLf + "#Disable MTVU" + vbCrLf + "--vu1=jit-sync"
            End If
            If PS2DisableInstantVIF1TransferCheckBox.IsChecked Then
                PS2EmulatorFixes = PS2EmulatorFixes + vbCrLf + "#Disable Instant VIF1 Transfer" + vbCrLf + "--vif1-instant-xfer=0"
            End If

            'Check for TXT config
            If Not String.IsNullOrEmpty(SelectedPS2TXTConfigTextBox.Text) Then
                Dim ModifiedPS2EmulatorConfigContent As String() = New String() {NewPS2EmulatorConfigContent, vbCrLf, PS2EmulatorFixes, vbCrLf + "#User Config" + vbCrLf, File.ReadAllText(SelectedPS2TXTConfigTextBox.Text)}
                NewPS2EmulatorConfigContent = String.Concat(ModifiedPS2EmulatorConfigContent)
            Else
                If PS2AddTXTConfigFromDatabaseCheckBox.IsChecked Then
                    If IsConfigAvailable(CurrentPS2GameID, Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\configs_txt.dat") Then
                        NewPS2EmulatorConfigContent = NewPS2EmulatorConfigContent + vbCrLf + "#" + CurrentPS2GameID + vbCrLf + GetPNACHFromDAT(Environment.CurrentDirectory + "\Tools\PS4\ps2-configs\configs_txt.dat", CurrentPS2GameID)
                    End If
                Else
                    'Append only PS2EmulatorFixes
                    NewPS2EmulatorConfigContent = NewPS2EmulatorConfigContent + vbCrLf + PS2EmulatorFixes
                End If
            End If

            'Write config-emu-ps4.txt
            If File.Exists(GameCacheDirectory + "\config-emu-ps4.txt") Then
                File.Delete(GameCacheDirectory + "\config-emu-ps4.txt")
            End If
            File.WriteAllText(GameCacheDirectory + "\config-emu-ps4.txt", NewPS2EmulatorConfigContent)

            If Not Directory.Exists(GameCacheDirectory + "\feature_data\") Then
                Directory.CreateDirectory(GameCacheDirectory + "\feature_data\")
            End If
            If Not Directory.Exists(GameCacheDirectory + "\feature_data\" + FullPS2GameID) Then
                Directory.CreateDirectory(GameCacheDirectory + "\feature_data\" + FullPS2GameID)
            End If

            'Check for PS2 Memory Card file
            If Not String.IsNullOrEmpty(SelectedPS2MemoryCardTextBox.Text) Then
                File.Copy(SelectedPS2MemoryCardTextBox.Text, GameCacheDirectory + "\feature_data\" + FullPS2GameID + "\custom.card", True)
            End If

            If Not Directory.Exists(GameCacheDirectory + "\image") Then
                Directory.CreateDirectory(GameCacheDirectory + "\image")
            End If

            'Create a GP4 project
            Dim NewProcess As New Process()
            NewProcess.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\PS4\gengp4_patch.exe"
            NewProcess.StartInfo.Arguments = """" + GameCacheDirectory + """"
            NewProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
            NewProcess.Start()
            NewProcess.WaitForExit()

            File.WriteAllText(Environment.CurrentDirectory + "\Cache\PS2fPKG.gp4", File.ReadAllText(Environment.CurrentDirectory + "\Cache\PS2fPKG.gp4").Replace("<?xml version=""1.1""", "<?xml version=""1.0"""))
            File.WriteAllText(Environment.CurrentDirectory + "\Cache\PS2fPKG.gp4", File.ReadAllText(Environment.CurrentDirectory + "\Cache\PS2fPKG.gp4").Replace("<scenarios default_id=""1"">", "<scenarios default_id=""0"">"))

            'Check compression config
            Dim UseCompression As String = "disable"
            If PS2UseCompressionCheckBox.IsChecked Then
                UseCompression = "enable"
            End If

            'Add disc information
            Dim FullDiscInfo As String = ""
            If Not String.IsNullOrEmpty(SelectedDisc1TextBox.Text) Then
                Dim NewDiscInfo As String() = New String() {FullDiscInfo, vbCrLf + "    <file targ_path=""image/disc01.iso"" orig_path=""", SelectedDisc1TextBox.Text, """ pfs_compression=""", UseCompression, """/>"}
                FullDiscInfo = String.Concat(NewDiscInfo)
            End If
            If Not String.IsNullOrEmpty(SelectedDisc2TextBox.Text) Then
                Dim NewDiscInfo As String() = New String() {FullDiscInfo, vbCrLf + "    <file targ_path=""image/disc02.iso"" orig_path=""", SelectedDisc2TextBox.Text, """ pfs_compression=""", UseCompression, """/>"}
                FullDiscInfo = String.Concat(NewDiscInfo)
            End If
            If Not String.IsNullOrEmpty(SelectedDisc3TextBox.Text) Then
                Dim NewDiscInfo As String() = New String() {FullDiscInfo, vbCrLf + "    <file targ_path=""image/disc03.iso"" orig_path=""", SelectedDisc3TextBox.Text, """ pfs_compression=""", UseCompression, """/>"}
                FullDiscInfo = String.Concat(NewDiscInfo)
            End If
            If Not String.IsNullOrEmpty(SelectedDisc4TextBox.Text) Then
                Dim NewDiscInfo As String() = New String() {FullDiscInfo, vbCrLf + "    <file targ_path=""image/disc04.iso"" orig_path=""", SelectedDisc4TextBox.Text, """ pfs_compression=""", UseCompression, """/>"}
                FullDiscInfo = String.Concat(NewDiscInfo)
            End If
            If Not String.IsNullOrEmpty(SelectedDisc5TextBox.Text) Then
                Dim NewDiscInfo As String() = New String() {FullDiscInfo, vbCrLf + "    <file targ_path=""image/disc05.iso"" orig_path=""", SelectedDisc5TextBox.Text, """ pfs_compression=""", UseCompression, """/>"}
                FullDiscInfo = String.Concat(NewDiscInfo)
            End If

            File.WriteAllText(Environment.CurrentDirectory + "\Cache\PS2fPKG.gp4", File.ReadAllText(Environment.CurrentDirectory + "\Cache\PS2fPKG.gp4").Replace("</files>", FullDiscInfo + vbCrLf + "</files>"))

            MsgBox("All files ready for PKG creation.", MsgBoxStyle.Information)

            'Create the fPKG
            Dim PKGBuilderProcessOutput As String = ""
            Dim PKGBuilderProcess As New Process()
            PKGBuilderProcess.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\PS4\mod-pub\orbis-pub-cmd-3.38.exe"
            PKGBuilderProcess.StartInfo.Arguments = "img_create --oformat pkg --skip_digest --no_progress_bar """ + Environment.CurrentDirectory + "\Cache\PS2fPKG.gp4"" """ + PKGOutputFolder + """"
            PKGBuilderProcess.StartInfo.UseShellExecute = False
            PKGBuilderProcess.StartInfo.RedirectStandardOutput = True
            PKGBuilderProcess.StartInfo.CreateNoWindow = True
            PKGBuilderProcess.Start()
            PKGBuilderProcess.WaitForExit()

            Using OutputStreamReader As StreamReader = PKGBuilderProcess.StandardOutput
                PKGBuilderProcessOutput = OutputStreamReader.ReadToEnd()
            End Using

            If PKGBuilderProcessOutput.Contains("Create image Process finished with warning") Then
                If MsgBox("PKG created!" + vbCrLf + "Do you want to open the output folder ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                    Process.Start("explorer", FBD.SelectedPath)
                End If
            Else
                MsgBox("Error while creating the fPKG.", MsgBoxStyle.Critical)
                MsgBox(PKGBuilderProcessOutput)
            End If

        Else
            MsgBox("Aborted", MsgBoxStyle.Information)
        End If
    End Sub

#End Region

#Region "PSP"

    Public Shared Function ReadUMDData(DataFile As String, Offset As Long, Lenght As Integer) As Byte()
        Dim NewByte(Lenght - 1 + 1 - 1) As Byte
        Using NewBinaryReader As New BinaryReader(File.Open(DataFile, FileMode.Open))
            Dim BaseStreamLenght As Long = NewBinaryReader.BaseStream.Length
            Dim Num As Integer = 0
            NewBinaryReader.BaseStream.Seek(Offset, SeekOrigin.Begin)
            While Offset < BaseStreamLenght And Num < Lenght
                NewByte(Num) = NewBinaryReader.ReadByte()
                Offset += 1
                Num += 1
            End While
        End Using
        Return NewByte
    End Function

    Public Shared Function FindOffset(FileName As String, Query As Byte()) As Object
        Dim ReturnLenght As Object
        Using NewBinaryReader As New BinaryReader(File.Open(FileName, FileMode.Open))
            Dim BaseStreamLength As Double = NewBinaryReader.BaseStream.Length
            If Query.Length <= BaseStreamLength Then

                Dim NewByteArray As Byte() = NewBinaryReader.ReadBytes(Query.Length)
                Dim Flag As Boolean = False
                Dim NewQueryLenght As Integer = Query.Length - 1
                Dim WhileInt As Integer = 0

                While WhileInt <= NewQueryLenght
                    If NewByteArray(WhileInt) = Query(WhileInt) Then
                        Flag = True
                        WhileInt += 1
                    Else
                        Flag = False
                        Exit While
                    End If
                End While

                If Not Flag Then
                    Dim NewBaseStreamLength As Double = BaseStreamLength - 1
                    Dim QueryLenght As Double = Query.Length
                    While QueryLenght <= NewBaseStreamLength
                        Array.Copy(NewByteArray, 1, NewByteArray, 0, NewByteArray.Length - 1)
                        NewByteArray(NewByteArray.Length - 1) = NewBinaryReader.ReadByte()
                        Dim length3 As Integer = Query.Length - 1
                        Dim num3 As Integer = 0
                        While num3 <= length3
                            If NewByteArray(num3) = Query(num3) Then
                                Flag = True
                                num3 += 1
                            Else
                                Flag = False
                                Exit While
                            End If
                        End While
                        If Not Flag Then
                            QueryLenght += 1
                        Else
                            ReturnLenght = QueryLenght - (Query.Length - 1)
                            Return ReturnLenght
                        End If
                    End While
                Else
                    ReturnLenght = 0
                    Return ReturnLenght
                End If
            End If
        End Using
        ReturnLenght = -1
        Return ReturnLenght
    End Function

    Public Shared Sub WriteData(FileToWrite As String, Offset As Long, DataToWrite As String)
        Dim NewFileStream As New FileStream(FileToWrite, FileMode.Open, FileAccess.Write, FileShare.Write)
        Dim NewStringArray As String() = DataToWrite.Split(New Char() {"-"c})
        NewFileStream.Seek(Offset, SeekOrigin.Begin)
        Dim DoInt As Integer = 0
        Do
            Dim str As String = NewStringArray(DoInt)
            NewFileStream.WriteByte(Convert.ToByte(Convert.ToInt32(str, 16)))
            DoInt += 1
        Loop While DoInt < NewStringArray.Length
        NewFileStream.Close()
    End Sub

    Private Sub BrowsePSPIconButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePSPIconButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PNG icon file.", .Multiselect = False, .Filter = "PNG (*.png)|*.png"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPSPIconTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePSPBGButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePSPBGButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PNG background file.", .Multiselect = False, .Filter = "PNG (*.png)|*.png"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPSPBGImageTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePSPDiscButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePSPDiscButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a PSP ISO file.", .Multiselect = False, .Filter = "ISO (*.iso)|*.iso"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then

            If Utils.FileExistInISO(OFD.FileName, "\PSP_GAME\PARAM.SFO") Then
                Dim CacheDir As String = Environment.CurrentDirectory + ""
                Dim ExtractedUMDDataPath As String = Utils.ExtractFileFromISO9660(OFD.FileName, "UMD_DATA.BIN", CacheDir + "\temp_umd_data.bin")

                If Not String.IsNullOrEmpty(ExtractedUMDDataPath) Then
                    PSPNPTitleTextBox.Text = Text.Encoding.ASCII.GetString(ReadUMDData(CacheDir + "\temp_umd_data.bin", 0, 10)).Replace("-", "")
                    PSPTitleTextBox.Text = Regex.Replace(Path.GetFileNameWithoutExtension(OFD.FileName), "\((.*?)\)", "")
                    PSPTitleTextBox.Text = Regex.Replace(PSPTitleTextBox.Text, " {2,}", "")
                End If

                SelectedPSPDiscTextBox.Text = OFD.FileName
            Else
                If MsgBox("Could not find any PSP game information within the ISO file." + vbCrLf + "Do you want to use this file anyway ?", MsgBoxStyle.YesNo, "Unknown PSP ISO file") = MsgBoxResult.Yes Then
                    SelectedPSPDiscTextBox.Text = OFD.FileName
                End If
            End If

        End If
    End Sub

    Private Sub BrowsePSPConfigButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePSPConfigButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a config-title.txt file.", .Multiselect = False, .Filter = "TXT (*.txt)|*.txt"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPSPConfigTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BuildPSPfPKGButton_Click(sender As Object, e As RoutedEventArgs) Handles BuildPSPfPKGButton.Click

        'Checks before fPKG creation
        If String.IsNullOrEmpty(SelectedPSPDiscTextBox.Text) Then
            MsgBox("No disc 1 specified, fPKG creation will be aborted.", MsgBoxStyle.Critical, "Cannot create fPKG")
            Exit Sub
        End If
        If String.IsNullOrEmpty(PSPTitleTextBox.Text) Then
            MsgBox("No game title specified, fPKG creation will be aborted.", MsgBoxStyle.Critical, "Cannot create fPKG")
            Exit Sub
        End If
        If String.IsNullOrEmpty(PSPNPTitleTextBox.Text) Then
            MsgBox("No NP title specified, fPKG creation will be aborted.", MsgBoxStyle.Critical, "Cannot create fPKG")
            Exit Sub
        End If
        If PSPNPTitleTextBox.Text.Length <> 9 Then
            MsgBox("'NP Title' length mismatching, only 9 characters are allowed, fPKG creation will be aborted.", MsgBoxStyle.Critical, "Cannot create fPKG")
            Exit Sub
        End If

        Dim FBD As New FolderBrowserDialog() With {.Description = "Please select an output folder", .ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then

            Dim CacheDirectory As String = Environment.CurrentDirectory + "\Cache"
            Dim GameCacheDirectory As String = Environment.CurrentDirectory + "\Cache\PSPfPKG"
            Dim SelectedISOFile As String = SelectedPSPDiscTextBox.Text

            'Remove previous fPKG creation & re-create the PSPfPKG cache folder
            If Directory.Exists(GameCacheDirectory) Then
                Directory.Delete(GameCacheDirectory, True)
            End If
            If File.Exists(Environment.CurrentDirectory + "\Cache\PSPfPKG.gp4") Then
                File.Delete(Environment.CurrentDirectory + "\Cache\PSPfPKG.gp4")
            End If
            Directory.CreateDirectory(GameCacheDirectory)

            'Copy the selected PS2 emulator to the cache directory
            Utils.CopyDirectory(Environment.CurrentDirectory + "\Tools\PS4\emus\psphd", GameCacheDirectory, True)

            'Get PSP EBOOT
            If Not File.Exists(Utils.ExtractFileFromISO9660(SelectedISOFile, "\PSP_GAME\SYSDIR\EBOOT.BIN", CacheDirectory + "\temp_eboot.bin")) Then
                File.Copy(SelectedISOFile, GameCacheDirectory + "\data\USER_L0.IMG", True)
                MsgBox("Cannot read the EBOOT.BIN file from the ISO." + vbCrLf + "Warning: This game may not work!", MsgBoxStyle.Exclamation)
            Else
                Dim NewProcess As New Process()
                NewProcess.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\pspdecrypt.exe"
                NewProcess.StartInfo.Arguments = """" + CacheDirectory + "\temp_eboot.bin"""
                NewProcess.StartInfo.CreateNoWindow = True
                NewProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
                NewProcess.Start()
                NewProcess.WaitForExit()

                File.Copy(SelectedISOFile, GameCacheDirectory + "\data\USER_L0.IMG", True)

                Dim NewFileInfo As New FileInfo(CacheDirectory + "\temp_eboot.bin")
                Dim FileLength As Long = NewFileInfo.Length
                If FileLength > 512320 Then
                    FileLength = 512320
                End If

                Dim TempEBOOTByteArray As Byte() = ReadUMDData(CacheDirectory + "\temp_eboot.bin", 0, CInt(FileLength))
                Dim OffsetValue As Object = RuntimeHelpers.GetObjectValue(FindOffset(GameCacheDirectory + "\data\USER_L0.IMG", TempEBOOTByteArray))
                Dim DecTempEBOOTByteArray As Byte() = ReadUMDData(CacheDirectory + "\temp_eboot.bin.dec", 0, CInt(NewFileInfo.Length))

                WriteData(GameCacheDirectory + "\data\USER_L0.IMG", CLng(OffsetValue), BitConverter.ToString(DecTempEBOOTByteArray))
            End If

            'Remove temp files
            If File.Exists(CacheDirectory + "\temp_eboot.bin") Then
                File.Delete(CacheDirectory + "\temp_eboot.bin")
            End If
            If File.Exists(CacheDirectory + "\temp_eboot.bin.dec") Then
                File.Delete(CacheDirectory + "\temp_eboot.bin.dec")
            End If
            If File.Exists(GameCacheDirectory + "\sce_sys\icon0.png") Then
                File.Delete(GameCacheDirectory + "\sce_sys\icon0.png")
            End If
            If File.Exists(GameCacheDirectory + "\sce_sys\pic1.png") Then
                File.Delete(GameCacheDirectory + "\sce_sys\pic1.png")
            End If

            'Copy the selected icon and background to the cache directory
            If Not Directory.Exists(GameCacheDirectory + "\sce_sys") Then
                Directory.CreateDirectory(GameCacheDirectory + "\sce_sys")
            End If
            If Not String.IsNullOrEmpty(SelectedPSPIconTextBox.Text) Then
                Using NewFileStream As New FileStream(SelectedPSPIconTextBox.Text, FileMode.Open, FileAccess.Read)
                    Utils.ConvertTo24bppPNG(Utils.ResizeAsImage(System.Drawing.Image.FromStream(NewFileStream), 512, 512)).Save(GameCacheDirectory + "\sce_sys\icon0.png", ImageFormat.Png)
                End Using
            End If
            If Not String.IsNullOrEmpty(SelectedPSPBGImageTextBox.Text) Then
                Using NewFileStream As New FileStream(SelectedPSPBGImageTextBox.Text, FileMode.Open, FileAccess.Read)
                    Utils.ConvertTo24bppPNG(Utils.ResizeAsImage(System.Drawing.Image.FromStream(NewFileStream), 1920, 1080)).Save(GameCacheDirectory + "\sce_sys\pic0.png", ImageFormat.Png)
                End Using
            End If

            'PSP Emulator configuration
            Dim EmulatorConfig As String = String.Concat("--ps4-trophies=0" + vbCrLf +
                                                         "--ps5-uds=0" + vbCrLf +
                                                         "--trophies=0" + vbCrLf +
                                                         "--image=""data/USER_L0.IMG""" + vbCrLf +
                                                         "--antialias=SSAA4x" + vbCrLf +
                                                         "--multisaves=true" + vbCrLf +
                                                         "--notrophies=true" + vbCrLf +
                                                         "" + vbCrLf + "")

            If File.Exists(GameCacheDirectory + "/config-title.txt") Then
                File.Delete(GameCacheDirectory + "/config-title.txt")
            End If
            File.WriteAllText(GameCacheDirectory + "/config-title.txt", EmulatorConfig)
            If Not String.IsNullOrEmpty(SelectedPSPConfigTextBox.Text) Then
                File.AppendAllText(GameCacheDirectory + "/config-title.txt", File.ReadAllText(SelectedPSPConfigTextBox.Text))
            End If

            'Create a new PARAM.SFO file
            Dim NewPS4ParamSFO As New ParamSfo()
            NewPS4ParamSFO.SetValue("APP_TYPE", SfoEntryType.Integer, "1", 4)
            NewPS4ParamSFO.SetValue("APP_VER", SfoEntryType.Utf8, "01.00", 8)
            NewPS4ParamSFO.SetValue("ATTRIBUTE", SfoEntryType.Integer, "0", 4)
            NewPS4ParamSFO.SetValue("CATEGORY", SfoEntryType.Utf8, "gd", 4)
            NewPS4ParamSFO.SetValue("CONTENT_ID", SfoEntryType.Utf8, String.Concat(New String() {"UP9000-", PSPNPTitleTextBox.Text, "_00-", PSPNPTitleTextBox.Text, "PSPFPKG"}), 48)
            NewPS4ParamSFO.SetValue("DOWNLOAD_DATA_SIZE", SfoEntryType.Integer, "0", 4)
            NewPS4ParamSFO.SetValue("FORMAT", SfoEntryType.Utf8, "obs", 4)
            NewPS4ParamSFO.SetValue("PARENTAL_LEVEL", SfoEntryType.Integer, "5", 4)
            NewPS4ParamSFO.SetValue("SYSTEM_VER", SfoEntryType.Integer, "0", 4)
            NewPS4ParamSFO.SetValue("TITLE", SfoEntryType.Utf8, PSPTitleTextBox.Text, 128)
            NewPS4ParamSFO.SetValue("TITLE_ID", SfoEntryType.Utf8, PSPNPTitleTextBox.Text, 12)
            NewPS4ParamSFO.SetValue("VERSION", SfoEntryType.Utf8, "01.00", 8)

            File.WriteAllBytes(GameCacheDirectory + "\sce_sys\param.sfo", NewPS4ParamSFO.Serialize())

            'Create a GP4 project
            Dim GenGP4Process As New Process()
            GenGP4Process.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\PS4\gengp4_patch.exe"
            GenGP4Process.StartInfo.Arguments = """" + GameCacheDirectory + """"
            GenGP4Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
            GenGP4Process.Start()
            GenGP4Process.WaitForExit()

            File.WriteAllText(CacheDirectory + "\PSPfPKG.gp4", File.ReadAllText(CacheDirectory + "\PSPfPKG.gp4").Replace("<?xml version=""1.1""", "<?xml version=""1.0"""))
            File.WriteAllText(CacheDirectory + "\PSPfPKG.gp4", File.ReadAllText(CacheDirectory + "\PSPfPKG.gp4").Replace("<scenarios default_id=""1"">", "<scenarios default_id=""0"">"))

            MsgBox("All files ready for PKG creation.", MsgBoxStyle.Information)

            Dim PKGBuilderProcessOutput As String = ""
            Dim PKGBuilderProcess As New Process()
            PKGBuilderProcess.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\PS4\mod-pub\orbis-pub-cmd-3.38.exe"
            PKGBuilderProcess.StartInfo.Arguments = "img_create --oformat pkg --skip_digest --no_progress_bar """ + CacheDirectory + "\PSPfPKG.gp4"" """ + FBD.SelectedPath + """"
            PKGBuilderProcess.StartInfo.UseShellExecute = False
            PKGBuilderProcess.StartInfo.RedirectStandardOutput = True
            PKGBuilderProcess.StartInfo.CreateNoWindow = True
            PKGBuilderProcess.Start()
            PKGBuilderProcess.WaitForExit()

            Using standardOutput As StreamReader = PKGBuilderProcess.StandardOutput
                PKGBuilderProcessOutput = standardOutput.ReadToEnd()
            End Using

            If Not PKGBuilderProcessOutput.Contains("Create image Process finished with warning") Then
                MsgBox(PKGBuilderProcessOutput)
            Else
                If MsgBox("PKG created!" + vbCrLf + "Do you want to open the output folder ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                    Process.Start("explorer", FBD.SelectedPath)
                End If
            End If

        End If

    End Sub

#End Region

End Class
