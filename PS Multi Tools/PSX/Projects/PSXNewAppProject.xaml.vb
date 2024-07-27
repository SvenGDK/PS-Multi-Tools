Imports System.IO

Public Class PSXNewAppProject

    Private Sub BrowseProjectPathButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseProjectPathButton.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Please select a folder to save your homebrew project.", .ShowNewFolderButton = True}

        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            ProjectDirectoryTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseELFButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseELFButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Choose your .elf file.", .Filter = "elf files (*.elf)|*.elf"}

        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            ProjectELFFileTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseIconButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseIconButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Choose your .ico file.", .Filter = "ico files (*.ico)|*.ico"}

        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            ProjectIconPathTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub AdvancedSettingsButton_Click(sender As Object, e As RoutedEventArgs) Handles EditResourcesButton.Click
        Dim NewAppEditor As New PSXAppEditor() With {.ProjectDirectory = ProjectDirectoryTextBox.Text, .Title = "Homebrew Ressources Editor - " + ProjectDirectoryTextBox.Text}

        If Directory.Exists(ProjectDirectoryTextBox.Text) AndAlso Directory.Exists(ProjectDirectoryTextBox.Text + "\res") Then
            If File.Exists(ProjectDirectoryTextBox.Text + "\res\jkt_002.png") Then
                NewAppEditor.CoverPictureBox.Source = New BitmapImage(New Uri(ProjectDirectoryTextBox.Text + "\res\jkt_002.png"))
                NewAppEditor.CoverPictureBox.Tag = ProjectDirectoryTextBox.Text + "\res\jkt_002.png"
            End If

            If File.Exists(ProjectDirectoryTextBox.Text + "\res\info.sys") Then
                Dim GameInfos As String() = File.ReadAllLines(ProjectDirectoryTextBox.Text + "\res\info.sys")
                NewAppEditor.HomebrewTitleTextBox.Text = GameInfos(0).Split("="c)(1).Trim()
                NewAppEditor.HomebrewSubtitleTextBox.Text = GameInfos(1).Split("="c)(1).Trim()
                If Not GameInfos(2).Split("="c)(1).Trim() = "0" Then
                    NewAppEditor.ShowGameIDCheckBox.IsChecked = True
                End If
                NewAppEditor.HomebrewReleaseDateTextBox.Text = GameInfos(3).Split("="c)(1).Trim()
                NewAppEditor.HomebrewDeveloperTextBox.Text = GameInfos(4).Split("="c)(1).Trim()
                NewAppEditor.PublisherTextBox.Text = GameInfos(5).Split("="c)(1).Trim()
                NewAppEditor.HomebrewNoteTextBox.Text = GameInfos(6).Split("="c)(1).Trim()
                NewAppEditor.HomebrewWebsiteTextBox.Text = GameInfos(7).Split("="c)(1).Trim()
                If Not GameInfos(13).Split("="c)(1).Trim() = "0" Then
                    NewAppEditor.ShowCopyrightCheckBox.IsChecked = True
                End If
                NewAppEditor.HomebrewGenreTextBox.Text = GameInfos(14).Split("="c)(1).Trim()
                NewAppEditor.RegionTextBox.Text = GameInfos(18).Split("="c)(1).Trim()
            Else
                NewAppEditor.HomebrewSubtitleTextBox.Text = ProjectSubTitleTextBox.Text
            End If

            NewAppEditor.Show()
        Else
            Directory.CreateDirectory(ProjectDirectoryTextBox.Text + "\res")
            NewAppEditor.Show()
        End If
    End Sub

    Private Sub SaveProjectButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveProjectButton.Click
        'Write Project settings to .CFG
        Using ProjectWriter As New StreamWriter(".\Projects\" + ProjectNameTextBox.Text + ".CFG", False)
            ProjectWriter.WriteLine("TITLE=" + ProjectNameTextBox.Text)
            ProjectWriter.WriteLine("ID=" + ProjectSubTitleTextBox.Text)
            ProjectWriter.WriteLine("DIR=" + ProjectDirectoryTextBox.Text)
            ProjectWriter.WriteLine("ELForISO=" + ProjectELFFileTextBox.Text)
            ProjectWriter.WriteLine("TYPE=APP")
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
            CNFWriter.WriteLine("title1=" + ProjectSubTitleTextBox.Text)
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

        If MsgBox("Homebrew Project saved. Close this window ?", MsgBoxStyle.YesNo, "Project saved") = MsgBoxResult.Yes Then
            Utils.ReloadProjects()
            Close()
        Else
            Utils.ReloadProjects()
        End If
    End Sub

End Class
