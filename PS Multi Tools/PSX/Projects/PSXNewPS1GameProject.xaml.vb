Imports System.IO

Public Class PSXNewPS1GameProject

    Private Sub BrowseIMAGE0Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowseIMAGE0Button.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Choose your .VCD file.", .Filter = "VCD files (*.VCD)|*.VCD"}

        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            IMAGE0PathTextBox.Text = OFD.FileName

            If MsgBox("Do you want to retrieve the Game ID from the VCD file ? This can take up to 30-45sec.", MsgBoxStyle.YesNo, "Load Game ID ?") = MsgBoxResult.Yes Then
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

                        'Get the game ID
                        For Each OutputLine In ProcessOutput
                            If OutputLine.Contains("BOOT =") Or OutputLine.Contains("BOOT=") Then
                                Dim GameID As String = OutputLine.Replace("BOOT = cdrom:\", "").Replace("BOOT=cdrom:\", "").Replace("BOOT = cdrom:", "").Replace(";1", "").Replace("_", "-").Replace(".", "").Trim()
                                ProjectIDTextBox.Text = UCase(GameID)
                                Exit For
                            End If
                        Next

                        'Get the game title
                        If Not String.IsNullOrEmpty(ProjectIDTextBox.Text) Then
                            If ProjectIDTextBox.Text.Length = 10 Then
                                ProjectTitleTextBox.Text = GetPS1GameTitleFromDatabaseList(ProjectIDTextBox.Text)
                            End If
                        End If

                    Else
                        MsgBox("Could not get the Game ID from the VCD file.", MsgBoxStyle.Information)
                    End If
                End Using
            End If

            UpdateDiscsInfo()
        End If
    End Sub

    Private Sub BrowseIMAGE1Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowseIMAGE1Button.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Choose your .VCD file.", .Filter = "VCD files (*.VCD)|*.VCD"}

        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            IMAGE1PathTextBox.Text = OFD.FileName
            UpdateDiscsInfo()
        End If
    End Sub

    Private Sub BrowseIMAGE2Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowseIMAGE2Button.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Choose your .VCD file.", .Filter = "VCD files (*.VCD)|*.VCD"}

        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            IMAGE2PathTextBox.Text = OFD.FileName
            UpdateDiscsInfo()
        End If
    End Sub

    Private Sub BrowseIMAGE3Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowseIMAGE3Button.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Choose your .VCD file.", .Filter = "VCD files (*.VCD)|*.VCD"}

        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            IMAGE3PathTextBox.Text = OFD.FileName
            UpdateDiscsInfo()
        End If
    End Sub

    Private Sub UpdateDiscsInfo()
        DISCSInfoTextBox.Clear()

        If Not String.IsNullOrEmpty(IMAGE0PathTextBox.Text) Then
            Dim IMAGE0FileName As String = Path.GetFileName(IMAGE0PathTextBox.Text)
            DISCSInfoTextBox.AppendText(IMAGE0FileName & vbCrLf)
        End If

        If Not String.IsNullOrEmpty(IMAGE1PathTextBox.Text) Then
            Dim IMAGE1FileName As String = Path.GetFileName(IMAGE1PathTextBox.Text)
            DISCSInfoTextBox.AppendText(IMAGE1FileName & vbCrLf)
        End If

        If Not String.IsNullOrEmpty(IMAGE2PathTextBox.Text) Then
            Dim IMAGE2FileName As String = Path.GetFileName(IMAGE2PathTextBox.Text)
            DISCSInfoTextBox.AppendText(IMAGE2FileName & vbCrLf)
        End If

        If Not String.IsNullOrEmpty(IMAGE3PathTextBox.Text) Then
            Dim IMAGE3FileName As String = Path.GetFileName(IMAGE3PathTextBox.Text)
            DISCSInfoTextBox.AppendText(IMAGE3FileName & vbCrLf)
        End If

        Dim DiscInfoLines() As String = DISCSInfoTextBox.Text.Split(vbCrLf.ToCharArray, StringSplitOptions.RemoveEmptyEntries)
        DISCSInfoTextBox.Text = String.Join(vbCrLf, DiscInfoLines)
    End Sub

    Private Sub BrowseProjectPathButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseProjectPathButton.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Please select a folder to save your PS1 game project.", .ShowNewFolderButton = True}

        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            ProjectDirectoryTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseIconButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseIconButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Choose your .ico file.", .Filter = "ico files (*.ico)|*.ico"}

        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            ProjectIconPathTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub SaveProjectButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveProjectButton.Click
        If String.IsNullOrEmpty(ProjectDirectoryTextBox.Text) Then
            MsgBox("Please select the project save path first.", MsgBoxStyle.Information, "No save path")
        Else

            If Not File.Exists(ProjectDirectoryTextBox.Text + "\IMAGE0.VCD") Then
                MsgBox("VCD files will now be copied to the project save path.", MsgBoxStyle.Information)
                FileIO.FileSystem.CopyFile(IMAGE0PathTextBox.Text, ProjectDirectoryTextBox.Text + "\IMAGE0.VCD", FileIO.UIOption.AllDialogs, FileIO.UICancelOption.DoNothing)
            End If

            'Write Project settings to .CFG
            Using ProjectWriter As New StreamWriter(Environment.CurrentDirectory + "\Projects\" + ProjectNameTextBox.Text + ".CFG", False)
                ProjectWriter.WriteLine("TITLE=" + ProjectNameTextBox.Text)
                ProjectWriter.WriteLine("ID=" + ProjectIDTextBox.Text)
                ProjectWriter.WriteLine("DIR=" + ProjectDirectoryTextBox.Text)
                ProjectWriter.WriteLine("ELForISO=" + IMAGE0PathTextBox.Text)
                ProjectWriter.WriteLine("TYPE=GAME")
                ProjectWriter.WriteLine("SIGNED=FALSE")
                ProjectWriter.WriteLine("GAMETYPE=PS1")

                If MultiDiscCheckBox.IsChecked Then
                    If Not String.IsNullOrEmpty(IMAGE1PathTextBox.Text) Then
                        ProjectWriter.WriteLine("IMAGE1=" + IMAGE1PathTextBox.Text)

                        If Not File.Exists(ProjectDirectoryTextBox.Text + "\IMAGE1.VCD") Then
                            FileIO.FileSystem.CopyFile(IMAGE1PathTextBox.Text, ProjectDirectoryTextBox.Text + "\IMAGE1.VCD", FileIO.UIOption.AllDialogs, FileIO.UICancelOption.DoNothing)
                        End If
                    End If
                    If Not String.IsNullOrEmpty(IMAGE2PathTextBox.Text) Then
                        ProjectWriter.WriteLine("IMAGE2=" + IMAGE2PathTextBox.Text)

                        If Not File.Exists(ProjectDirectoryTextBox.Text + "\IMAGE2.VCD") Then
                            FileIO.FileSystem.CopyFile(IMAGE2PathTextBox.Text, ProjectDirectoryTextBox.Text + "\IMAGE2.VCD", FileIO.UIOption.AllDialogs, FileIO.UICancelOption.DoNothing)
                        End If
                    End If
                    If Not String.IsNullOrEmpty(IMAGE3PathTextBox.Text) Then
                        ProjectWriter.WriteLine("IMAGE3=" + IMAGE3PathTextBox.Text)

                        If Not File.Exists(ProjectDirectoryTextBox.Text + "\IMAGE3.VCD") Then
                            FileIO.FileSystem.CopyFile(IMAGE3PathTextBox.Text, ProjectDirectoryTextBox.Text + "\IMAGE3.VCD", FileIO.UIOption.AllDialogs, FileIO.UICancelOption.DoNothing)
                        End If
                    End If
                End If
            End Using

            'Write SYSTEM.CNF to project directory
            Using CNFWriter As New StreamWriter(ProjectDirectoryTextBox.Text + "\SYSTEM.CNF", False)
                CNFWriter.WriteLine("BOOT2 = pfs:/EXECUTE.KELF") 'Loads EXECUTE.KELF (POPSTARTER)
                CNFWriter.WriteLine("VER = 1.01")
                CNFWriter.WriteLine("VMODE = NTSC")
                CNFWriter.WriteLine("HDDUNITPOWER = NICHDD")
            End Using

            'Write icon.sys to project directory
            Using CNFWriter As New StreamWriter(ProjectDirectoryTextBox.Text + "\icon.sys", False)
                CNFWriter.WriteLine("PS2X")
                CNFWriter.WriteLine("title0=" + ProjectTitleTextBox.Text)
                CNFWriter.WriteLine("title1=" + ProjectIDTextBox.Text)
                CNFWriter.WriteLine("bgcola=0")
                CNFWriter.WriteLine("bgcol0=0,0,0")
                CNFWriter.WriteLine("bgcol1=0,0,0")
                CNFWriter.WriteLine("bgcol2=0,0,0")
                CNFWriter.WriteLine("bgcol3=0,0,0")
                CNFWriter.WriteLine("lightdir0=1.0,-1.0,1.0")
                CNFWriter.WriteLine("lightdir1=-1.0,1.0,-1.0")
                CNFWriter.WriteLine("lightdir2=0.0,0.0,0.0")
                CNFWriter.WriteLine("lightcolamb=64,64,64")
                CNFWriter.WriteLine("lightcol0=64,64,64")
                CNFWriter.WriteLine("lightcol1=16,16,16")
                CNFWriter.WriteLine("lightcol2=0,0,0")
                CNFWriter.WriteLine("uninstallmes0=" + ProjectUninstallMsgTextBox.Text)
                CNFWriter.WriteLine("uninstallmes1=")
                CNFWriter.WriteLine("uninstallmes2=")
            End Using

            'Copy selected ico to project folder
            If Not ProjectIconPathTextBox.Text = "" Then
                File.Copy(ProjectIconPathTextBox.Text, ProjectDirectoryTextBox.Text + "\list.ico")
            End If

            'Write DISCS.TXT to project directory
            Using DISCSWriter As New StreamWriter(ProjectDirectoryTextBox.Text + "\DISCS.TXT", False)
                DISCSWriter.WriteLine("IMAGE0.VCD")
                If Not String.IsNullOrEmpty(IMAGE1PathTextBox.Text) Then
                    DISCSWriter.WriteLine("IMAGE1.VCD")
                Else
                    DISCSWriter.WriteLine("")
                End If
                If Not String.IsNullOrEmpty(IMAGE2PathTextBox.Text) Then
                    DISCSWriter.WriteLine("IMAGE2.VCD")
                Else
                    DISCSWriter.WriteLine("")
                End If
                If Not String.IsNullOrEmpty(IMAGE3PathTextBox.Text) Then
                    DISCSWriter.WriteLine("IMAGE3.VCD")
                Else
                    DISCSWriter.Write("")
                End If
            End Using

            If MsgBox("PS1 Game Project saved. Close this window ?", MsgBoxStyle.YesNo, "Project saved") = MsgBoxResult.Yes Then
                Utils.ReloadProjects()

                'Reactivate the main window
                For Each Win In System.Windows.Application.Current.Windows()
                    If Win.ToString = "psmt_lib.PSXMainWindow" Then
                        CType(Win, PSXMainWindow).Activate()
                        Exit For
                    End If
                Next

                Close()
            Else
                Utils.ReloadProjects()
            End If
        End If
    End Sub

    Private Sub EditResourcesButton_Click(sender As Object, e As RoutedEventArgs) Handles EditResourcesButton.Click
        If String.IsNullOrEmpty(ProjectDirectoryTextBox.Text) Then
            MsgBox("Please select the project save path first.", MsgBoxStyle.Information, "No save path")
        Else
            Dim NewGameEditor As New PSXPS1GameEditor() With {.ProjectDirectory = ProjectDirectoryTextBox.Text, .Title = "Game Ressources Editor - " + ProjectDirectoryTextBox.Text}
            If Directory.Exists(ProjectDirectoryTextBox.Text) AndAlso Directory.Exists(ProjectDirectoryTextBox.Text + "\res") Then

                If File.Exists(ProjectDirectoryTextBox.Text + "\res\jkt_001.png") Then
                    NewGameEditor.GameCoverImage.Source = New BitmapImage(New Uri(ProjectDirectoryTextBox.Text + "\res\jkt_001.png"))
                    NewGameEditor.GameCoverImage.Tag = ProjectDirectoryTextBox.Text + "\res\jkt_001.png"
                End If
                If File.Exists(ProjectDirectoryTextBox.Text + "\res\image\0.png") Then
                    NewGameEditor.BackgroundImagePictureBox.Source = New BitmapImage(New Uri(ProjectDirectoryTextBox.Text + "\res\image\0.png"))
                    NewGameEditor.BackgroundImagePictureBox.Tag = ProjectDirectoryTextBox.Text + "\res\image\0.png"
                End If
                If File.Exists(ProjectDirectoryTextBox.Text + "\res\image\1.png") Then
                    NewGameEditor.ScreenshotImage1PictureBox.Source = New BitmapImage(New Uri(ProjectDirectoryTextBox.Text + "\res\image\1.png"))
                    NewGameEditor.ScreenshotImage1PictureBox.Tag = ProjectDirectoryTextBox.Text + "\res\image\1.png"
                End If
                If File.Exists(ProjectDirectoryTextBox.Text + "\res\image\2.png") Then
                    NewGameEditor.ScreenshotImage2PictureBox.Source = New BitmapImage(New Uri(ProjectDirectoryTextBox.Text + "\res\image\2.png"))
                    NewGameEditor.ScreenshotImage2PictureBox.Tag = ProjectDirectoryTextBox.Text + "\res\image\2.png"
                End If

                If File.Exists(ProjectDirectoryTextBox.Text + "\res\info.sys") Then
                    Dim GameInfos As String() = File.ReadAllLines(ProjectDirectoryTextBox.Text + "\res\info.sys")
                    NewGameEditor.GameTitleTextBox.Text = GameInfos(0).Split("="c)(1).Trim()
                    NewGameEditor.GameIDTextBox.Text = GameInfos(1).Split("="c)(1).Replace("_", "-").Replace(".", "").Trim()

                    If Not GameInfos(2).Split("="c)(1).Trim() = "0" Then
                        NewGameEditor.ShowGameIDCheckBox.IsChecked = True
                    End If

                    NewGameEditor.GameReleaseDateTextBox.Text = GameInfos(3).Split("="c)(1).Trim()
                    NewGameEditor.GameDeveloperTextBox.Text = GameInfos(4).Split("="c)(1).Trim()
                    NewGameEditor.GamePublisherTextBox.Text = GameInfos(5).Split("="c)(1).Trim()
                    NewGameEditor.GameNoteTextBox.Text = GameInfos(6).Split("="c)(1).Trim()
                    NewGameEditor.GameWebsiteTextBox.Text = GameInfos(7).Split("="c)(1).Trim()

                    If Not GameInfos(13).Split("="c)(1).Trim() = "0" Then
                        NewGameEditor.ShowCopyrightCheckBox.IsChecked = True
                    End If

                    NewGameEditor.GameGenreTextBox.Text = GameInfos(14).Split("="c)(1).Trim()
                    NewGameEditor.GameRegionTextBox.Text = GameInfos(18).Split("="c)(1).Trim()
                Else
                    If ProjectIDTextBox.Text.Contains("_") Then
                        NewGameEditor.GameIDTextBox.Text = ProjectIDTextBox.Text.Replace("_", "-").Replace(".", "").Trim()
                    Else
                        NewGameEditor.GameIDTextBox.Text = ProjectIDTextBox.Text
                    End If
                End If

                NewGameEditor.Show()
            Else
                'Create res directory
                Directory.CreateDirectory(ProjectDirectoryTextBox.Text + "\res")

                NewGameEditor.GameTitleTextBox.Text = ProjectTitleTextBox.Text
                NewGameEditor.GameIDTextBox.Text = ProjectIDTextBox.Text.Replace("_", "-").Replace(".", "").Trim()
                NewGameEditor.Show()
            End If
        End If
    End Sub

    Private Sub MultiDiscCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles MultiDiscCheckBox.Checked
        IMAGE1PathTextBox.IsEnabled = True
        BrowseIMAGE1Button.IsEnabled = True
        IMAGE2PathTextBox.IsEnabled = True
        BrowseIMAGE2Button.IsEnabled = True
        IMAGE3PathTextBox.IsEnabled = True
        BrowseIMAGE3Button.IsEnabled = True
    End Sub

    Private Sub MultiDiscCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles MultiDiscCheckBox.Unchecked
        IMAGE1PathTextBox.IsEnabled = False
        BrowseIMAGE1Button.IsEnabled = False
        IMAGE2PathTextBox.IsEnabled = False
        BrowseIMAGE2Button.IsEnabled = False
        IMAGE3PathTextBox.IsEnabled = False
        BrowseIMAGE3Button.IsEnabled = False
    End Sub

    Public Function GetPS1GameTitleFromDatabaseList(GameID As String) As String
        Dim FoundGameTitle As String = ""

        For Each GameTitle As String In File.ReadLines(Environment.CurrentDirectory + "\Tools\ps1ids.txt")
            If GameTitle.Contains(GameID) Then
                FoundGameTitle = GameTitle.Split(";"c)(1)
                Exit For
            End If
        Next

        If String.IsNullOrEmpty(FoundGameTitle) Then
            Return ""
        Else
            Return FoundGameTitle
        End If
    End Function

    Public Sub ImportFromPSMT(GameFilePath As String, GameTitle As String, NewGameProjectDirectory As String, GameID As String)
        IMAGE0PathTextBox.Text = GameFilePath
        ProjectNameTextBox.Text = GameTitle
        ProjectDirectoryTextBox.Text = NewGameProjectDirectory
        ProjectTitleTextBox.Text = GameTitle
        ProjectIDTextBox.Text = GameID
        ProjectUninstallMsgTextBox.Text = "Do you want to uninstall this game ?"
    End Sub

End Class
