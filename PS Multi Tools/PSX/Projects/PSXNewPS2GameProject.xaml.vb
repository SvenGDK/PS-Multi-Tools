Imports System.IO

Public Class PSXNewPS2GameProject

    Private Sub BrowseProjectPathButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseProjectPathButton.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Please select a folder to save your game project.", .ShowNewFolderButton = True}

        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            ProjectDirectoryTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseGameISOButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseGameISOButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Choose your .iso or .cue file.", .Filter = "ISO files (*.iso)|*.iso|CUE files (*.cue)|*.cue"}

        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            ProjectISOFileTextBox.Text = OFD.FileName

            If Path.GetExtension(OFD.FileName) = ".iso" Then
                If MsgBox("Do you want to load the game ID from the disc?" + vbCrLf + "The Game ID is required to install the game.", MsgBoxStyle.YesNo, "") = MsgBoxResult.Yes Then
                    Using SevenZip As New Process()
                        SevenZip.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\7z.exe"
                        SevenZip.StartInfo.Arguments = "l -ba """ + OFD.FileName + """"
                        SevenZip.StartInfo.RedirectStandardOutput = True
                        SevenZip.StartInfo.UseShellExecute = False
                        SevenZip.StartInfo.CreateNoWindow = True
                        SevenZip.Start()

                        'Read the output
                        Dim OutputReader As StreamReader = SevenZip.StandardOutput
                        Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.None)

                        For Each Line As String In ProcessOutput
                            If Line.Contains("SLES_") Or Line.Contains("SLUS_") Or Line.Contains("SCES_") Or Line.Contains("SCUS_") Or Line.Contains("SLPS_") Or Line.Contains("SCCS_") Or Line.Contains("SLPM_") Or Line.Contains("SLKA_") Then
                                If Line.Contains("Volume:") Then 'ID found in the ISO Header
                                    ProjectIDTextBox.Text = Line.Split(New String() {"Volume: "}, StringSplitOptions.RemoveEmptyEntries)(1)
                                    Exit For
                                Else 'ID found in the ISO files
                                    ProjectIDTextBox.Text = String.Join(" ", Line.Split(New Char() {}, StringSplitOptions.RemoveEmptyEntries)).Split(" "c)(5).Trim()
                                    Exit For
                                End If
                            End If
                        Next

                    End Using
                End If
            End If

        End If
    End Sub

    Private Sub BrowseIconButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseIconButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Choose your .ico file.", .Filter = "ico files (*.ico)|*.ico"}

        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            ProjectIconPathTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub EditResourcesButton_Click(sender As Object, e As RoutedEventArgs) Handles EditResourcesButton.Click
        If String.IsNullOrEmpty(ProjectDirectoryTextBox.Text) Then
            MsgBox("Please select the project save path first.", MsgBoxStyle.Information, "No save path")
        Else
            Dim NewGameEditor As New PSXPS2GameEditor() With {.ProjectDirectory = ProjectDirectoryTextBox.Text, .Title = "Game Ressources Editor - " + ProjectDirectoryTextBox.Text}
            If Directory.Exists(ProjectDirectoryTextBox.Text) AndAlso Directory.Exists(ProjectDirectoryTextBox.Text + "\res") Then

                If File.Exists(ProjectDirectoryTextBox.Text + "\res\jkt_001.png") Then
                    NewGameEditor.CoverPictureBox.Source = New BitmapImage(New Uri(ProjectDirectoryTextBox.Text + "\res\jkt_001.png"))
                    NewGameEditor.CoverPictureBox.Tag = ProjectDirectoryTextBox.Text + "\res\jkt_001.png"
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
                    NewGameEditor.PublisherTextBox.Text = GameInfos(5).Split("="c)(1).Trim()
                    NewGameEditor.GameNoteTextBox.Text = GameInfos(6).Split("="c)(1).Trim()
                    NewGameEditor.GameWebsiteTextBox.Text = GameInfos(7).Split("="c)(1).Trim()
                    If Not GameInfos(13).Split("="c)(1).Trim() = "0" Then
                        NewGameEditor.ShowCopyrightCheckBox.IsChecked = True
                    End If
                    NewGameEditor.GameGenreTextBox.Text = GameInfos(14).Split("="c)(1).Trim()
                    NewGameEditor.RegionTextBox.Text = GameInfos(18).Split("="c)(1).Trim()
                Else
                    If ProjectIDTextBox.Text.Contains("_") Then
                        NewGameEditor.GameIDTextBox.Text = ProjectIDTextBox.Text.Replace("_", "-").Replace(".", "").Trim()
                    Else
                        NewGameEditor.GameIDTextBox.Text = ProjectIDTextBox.Text
                    End If
                End If

                NewGameEditor.Show()
            Else
                Directory.CreateDirectory(ProjectDirectoryTextBox.Text + "\res")
                NewGameEditor.GameIDTextBox.Text = ProjectIDTextBox.Text.Replace("_", "-").Replace(".", "").Trim()
                NewGameEditor.Show()
            End If
        End If
    End Sub

    Private Sub SaveProjectButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveProjectButton.Click
        If String.IsNullOrEmpty(ProjectDirectoryTextBox.Text) Then
            MsgBox("Please select the project save path first.", MsgBoxStyle.Information, "No save path")
        Else
            'Write Project settings to .CFG
            Using ProjectWriter As New StreamWriter(Environment.CurrentDirectory + "\Projects\" + ProjectNameTextBox.Text + ".CFG", False)
                ProjectWriter.WriteLine("TITLE=" + ProjectNameTextBox.Text)
                ProjectWriter.WriteLine("ID=" + ProjectIDTextBox.Text)
                ProjectWriter.WriteLine("DIR=" + ProjectDirectoryTextBox.Text)
                ProjectWriter.WriteLine("ELForISO=" + ProjectISOFileTextBox.Text)
                ProjectWriter.WriteLine("TYPE=GAME")
                ProjectWriter.WriteLine("SIGNED=FALSE")
                ProjectWriter.WriteLine("GAMETYPE=PS2")
            End Using

            'Write SYSTEM.CNF to project directory
            Using CNFWriter As New StreamWriter(ProjectDirectoryTextBox.Text + "\SYSTEM.CNF", False)
                CNFWriter.WriteLine("BOOT2 = pfs:/EXECUTE.KELF") 'Loads EXECUTE.KELF
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

            If MsgBox("Game Project saved. Close this window ?", MsgBoxStyle.YesNo, "Project saved") = MsgBoxResult.Yes Then
                Utils.ReloadProjects()
                Close()
            Else
                Utils.ReloadProjects()
            End If
        End If
    End Sub

    Public Sub ImportFromPSMT(GameFilePath As String, GameTitle As String, NewGameProjectDirectory As String, GameID As String)
        ProjectISOFileTextBox.Text = GameFilePath
        ProjectNameTextBox.Text = GameTitle
        ProjectDirectoryTextBox.Text = NewGameProjectDirectory
        ProjectTitleTextBox.Text = GameTitle
        ProjectIDTextBox.Text = GameID.Replace("-", "_").Insert(8, ".")
        ProjectUninstallMsgTextBox.Text = "Do you want to uninstall this game ?"
    End Sub

End Class
