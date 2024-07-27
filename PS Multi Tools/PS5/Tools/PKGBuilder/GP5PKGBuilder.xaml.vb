Imports System.IO
Imports System.Windows.Forms
Imports System.Windows.Media.Animation
Imports System.Windows.Threading
Imports Newtonsoft.Json
Imports PS_Multi_Tools.INI
Imports PS_Multi_Tools.PS5ManifestClass
Imports PS_Multi_Tools.PS5ParamClass
Imports PS_Multi_Tools.Structures

Public Class GP5PKGBuilder

    Dim CurrentProject As PKGBuilderProject
    Dim PubToolsPath As String = ""

    Dim WithEvents PKGBuilder As New Process()
    Dim PKGBuilderOuput As String = ""
    Dim CurrentLanguage As String = ""

    Private WithEvents ClockTimer As DispatcherTimer
    ReadOnly MoveRightToLeftAnimation As New ThicknessAnimation With {.From = New Thickness(175, 305, 0, 0), .To = New Thickness(55, 305, 0, 0), .Duration = New Duration(TimeSpan.FromMilliseconds(500))}
    ReadOnly MoveLeftToRightAnimation As New ThicknessAnimation With {.From = New Thickness(55, 305, 0, 0), .To = New Thickness(175, 305, 0, 0), .Duration = New Duration(TimeSpan.FromMilliseconds(500))}
    ReadOnly ShowAnimation As New DoubleAnimation With {.From = 0, .To = 1, .Duration = New Duration(TimeSpan.FromMilliseconds(200))}
    ReadOnly HideAnimation As New DoubleAnimation With {.From = 1, .To = 0, .Duration = New Duration(TimeSpan.FromMilliseconds(200))}

    Private Sub CreateNewProjectButton_Click(sender As Object, e As RoutedEventArgs) Handles CreateNewProjectButton.Click
        Dim SFD As New FolderBrowserDialog() With {.Description = "Select a folder to save your project", .ShowNewFolderButton = True}
        If SFD.ShowDialog() = Windows.Forms.DialogResult.OK Then

            Dim NewProjectTitle As String = InputBox("Enter a title for your new PKG project :", "Title required", "No Title")
            If Not String.IsNullOrEmpty(NewProjectTitle) Then

                'Create a new project
                Dim NewProject As New PKGBuilderProject() With {.ProjectBackground = Nothing,
            .ProjectCategory = "Game",
            .ProjectIcon = Nothing,
            .ProjectSoundtrack = "",
            .ProjectTitle = NewProjectTitle,
            .ProjectURL = "",
            .ProjectPath = SFD.SelectedPath,
            .GP5Created = False}

                'Create the sce_sys directory
                Directory.CreateDirectory(SFD.SelectedPath + "\sce_sys")

                'Create a new param.json file inside the sce_sys directory
                CreateParam(SFD.SelectedPath + "\sce_sys\param.json")
                ChangeParam(SFD.SelectedPath + "\sce_sys\param.json", "Title", NewProjectTitle)

                'Create a new manifest.json file
                CreateManifest(SFD.SelectedPath + "\manifest.json")

                'Save project configuration file
                Dim NewProjectINI As New IniFile(SFD.SelectedPath + "\" + NewProjectTitle + ".ini")
                NewProjectINI.IniWriteValue("PKGProject", "Title", NewProjectTitle)
                NewProjectINI.IniWriteValue("PKGProject", "Background", "")
                NewProjectINI.IniWriteValue("PKGProject", "Category", "Game")
                NewProjectINI.IniWriteValue("PKGProject", "Icon", "")
                NewProjectINI.IniWriteValue("PKGProject", "Soundtrack", "")
                NewProjectINI.IniWriteValue("PKGProject", "URL", "")
                NewProjectINI.IniWriteValue("PKGProject", "Path", SFD.SelectedPath)
                NewProjectINI.IniWriteValue("PKGProject", "GP5Created", "False")

                'Enable controls
                CurrentProjectTextBlock.Text = SFD.SelectedPath + "\" + NewProjectTitle + ".ini"
                SaveProjectButton.IsEnabled = True
                FinalizeButton.IsEnabled = True
                SetBackgroundButton.IsEnabled = True
                SetSoundtrackButton.IsEnabled = True
                SetURLButton.IsEnabled = True
                SetContentIDButton.IsEnabled = True
                SetTitleIDButton.IsEnabled = True
                SetVersionButton.IsEnabled = True
                GamesTextBlock.IsEnabled = True
                MediaTextBlock.IsEnabled = True
                TitleTextBlock.IsEnabled = True
                MainIconImageRectangle.IsEnabled = True
                BackgroundImage.IsEnabled = True

                CurrentProject = NewProject
                TitleTextBlock.Text = NewProjectTitle
            End If

        End If
    End Sub

    Private Sub LoadProjectButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadProjectButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select a saved PKG project", .Filter = "INI Files (*.ini)|*.ini", .Multiselect = False}
        If OFD.ShowDialog() = Windows.Forms.DialogResult.OK Then

            CurrentProjectTextBlock.Text = OFD.FileName

            Dim ProjectInfos As New PKGBuilderProject()
            Dim ProjectINI As New IniFile(OFD.FileName)
            If Not String.IsNullOrEmpty(ProjectINI.IniReadValue("PKGProject", "Title")) Then
                ProjectInfos.ProjectTitle = ProjectINI.IniReadValue("PKGProject", "Title")
                TitleTextBlock.Text = ProjectINI.IniReadValue("PKGProject", "Title")
            End If
            If Not String.IsNullOrEmpty(ProjectINI.IniReadValue("PKGProject", "Background")) Then
                Dim TempBitmapImage = New BitmapImage()
                TempBitmapImage.BeginInit()
                TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                TempBitmapImage.UriSource = New Uri(ProjectINI.IniReadValue("PKGProject", "Background"), UriKind.RelativeOrAbsolute)
                TempBitmapImage.EndInit()
                ProjectInfos.ProjectBackground = TempBitmapImage
                BackgroundImage.Source = TempBitmapImage
            End If
            If Not String.IsNullOrEmpty(ProjectINI.IniReadValue("PKGProject", "Category")) Then
                ProjectInfos.ProjectCategory = ProjectINI.IniReadValue("PKGProject", "Category")
                If Not ProjectINI.IniReadValue("PKGProject", "Category") = "Game" Then
                    MediaTextBlock.FontWeight = FontWeights.Bold
                    GamesTextBlock.FontWeight = FontWeights.Regular

                    If MediaTextBlock.Margin.Left = 175 Then
                        MediaTextBlock.BeginAnimation(MarginProperty, MoveRightToLeftAnimation)
                        GamesTextBlock.BeginAnimation(MarginProperty, MoveLeftToRightAnimation)
                    End If

                    PlayButtonTextBlock.Text = "Launch"
                End If
            End If
            If Not String.IsNullOrEmpty(ProjectINI.IniReadValue("PKGProject", "Icon")) Then
                Dim TempBitmapImage = New BitmapImage()
                TempBitmapImage.BeginInit()
                TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                TempBitmapImage.UriSource = New Uri(ProjectINI.IniReadValue("PKGProject", "Icon"), UriKind.RelativeOrAbsolute)
                TempBitmapImage.EndInit()

                ProjectInfos.ProjectIcon = TempBitmapImage
                MainIconImage.ImageSource = TempBitmapImage
            End If
            If Not String.IsNullOrEmpty(ProjectINI.IniReadValue("PKGProject", "Soundtrack")) Then
                ProjectInfos.ProjectSoundtrack = ProjectINI.IniReadValue("PKGProject", "Soundtrack")
            End If
            If Not String.IsNullOrEmpty(ProjectINI.IniReadValue("PKGProject", "URL")) Then
                ProjectInfos.ProjectURL = ProjectINI.IniReadValue("PKGProject", "URL")
            End If
            If Not String.IsNullOrEmpty(ProjectINI.IniReadValue("PKGProject", "Path")) Then
                ProjectInfos.ProjectPath = ProjectINI.IniReadValue("PKGProject", "Path")
            End If
            If Not String.IsNullOrEmpty(ProjectINI.IniReadValue("PKGProject", "GP5Created")) Then
                If ProjectINI.IniReadValue("PKGProject", "GP5Created") = "True" Then
                    If File.Exists(ProjectInfos.ProjectPath + "\" + ProjectInfos.ProjectTitle + ".gp5") Then
                        ProjectInfos.GP5Created = True
                        BuildProjectButton.IsEnabled = True
                    End If
                Else
                    ProjectInfos.GP5Created = False
                End If
            End If

            If Not String.IsNullOrEmpty(ProjectInfos.ProjectPath) Then
                SaveProjectButton.IsEnabled = True
                FinalizeButton.IsEnabled = True
                SetBackgroundButton.IsEnabled = True
                SetSoundtrackButton.IsEnabled = True
                SetURLButton.IsEnabled = True
                SetContentIDButton.IsEnabled = True
                SetTitleIDButton.IsEnabled = True
                SetVersionButton.IsEnabled = True
                GamesTextBlock.IsEnabled = True
                MediaTextBlock.IsEnabled = True
                TitleTextBlock.IsEnabled = True
                MainIconImageRectangle.IsEnabled = True
                BackgroundImage.IsEnabled = True

                CurrentProject = ProjectInfos
            Else
                MsgBox("Wrong INI format.", MsgBoxStyle.Exclamation, "Error reading project INI")
            End If

        End If
    End Sub

    Private Sub SaveProjectButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveProjectButton.Click
        If Not String.IsNullOrEmpty(CurrentProject.ProjectPath) Then

            Dim SFD As New FolderBrowserDialog() With {.Description = "Select a save path for your project", .ShowNewFolderButton = True}
            If SFD.ShowDialog() = Windows.Forms.DialogResult.OK Then

                'Save project settings
                Dim NewProjectINI As New IniFile(SFD.SelectedPath + "\" + CurrentProject.ProjectTitle + ".ini")
                NewProjectINI.IniWriteValue("PKGProject", "Title", CurrentProject.ProjectTitle)
                NewProjectINI.IniWriteValue("PKGProject", "Background", "")
                NewProjectINI.IniWriteValue("PKGProject", "Category", CurrentProject.ProjectCategory)
                NewProjectINI.IniWriteValue("PKGProject", "Icon", "")
                NewProjectINI.IniWriteValue("PKGProject", "Soundtrack", CurrentProject.ProjectSoundtrack)
                NewProjectINI.IniWriteValue("PKGProject", "URL", CurrentProject.ProjectURL)
                NewProjectINI.IniWriteValue("PKGProject", "Path", SFD.SelectedPath)
                NewProjectINI.IniWriteValue("PKGProject", "GP5Created", "False")
            End If
        Else
            MsgBox("No project loaded.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub GamesTextBlock_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles GamesTextBlock.MouseLeftButtonDown
        If Not String.IsNullOrEmpty(CurrentProject.ProjectPath) Then
            If File.Exists(CurrentProject.ProjectPath + "\sce_sys\param.json") Then
                ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "Category", "Game")
            Else
                CreateParam(CurrentProject.ProjectPath + "\sce_sys\param.json")
                ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "Category", "Game")
            End If

            MediaTextBlock.FontWeight = FontWeights.Regular
            GamesTextBlock.FontWeight = FontWeights.Bold

            If GamesTextBlock.Margin.Left = 175 Then
                GamesTextBlock.BeginAnimation(MarginProperty, MoveRightToLeftAnimation)
                MediaTextBlock.BeginAnimation(MarginProperty, MoveLeftToRightAnimation)
            End If

            PlayButtonTextBlock.Text = "Play Game"

            MsgBox("Category changed!", MsgBoxStyle.Information)
        Else
            MsgBox("No project loaded.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub MediaTextBlock_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles MediaTextBlock.MouseLeftButtonDown
        If Not String.IsNullOrEmpty(CurrentProject.ProjectPath) Then
            If File.Exists(CurrentProject.ProjectPath + "\sce_sys\param.json") Then
                ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "Category", "Media")
            Else
                CreateParam(CurrentProject.ProjectPath + "\sce_sys\param.json")
                ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "Category", "Media")
            End If

            MediaTextBlock.FontWeight = FontWeights.Bold
            GamesTextBlock.FontWeight = FontWeights.Regular

            If MediaTextBlock.Margin.Left = 175 Then
                MediaTextBlock.BeginAnimation(MarginProperty, MoveRightToLeftAnimation)
                GamesTextBlock.BeginAnimation(MarginProperty, MoveLeftToRightAnimation)
            End If

            PlayButtonTextBlock.Text = "Launch"

            MsgBox("Category changed!", MsgBoxStyle.Information)
        Else
            MsgBox("No project loaded.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub MainIconImageRectangle_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles MainIconImageRectangle.MouseLeftButtonDown
        If Not String.IsNullOrEmpty(CurrentProject.ProjectPath) Then
            Dim OFD As New OpenFileDialog() With {.Title = "Select a new icon", .Filter = "PNG Image (*.png)|*.png", .Multiselect = False}
            If OFD.ShowDialog() = Windows.Forms.DialogResult.OK Then
                'Show new icon
                Dispatcher.BeginInvoke(Sub()
                                           Dim TempBitmapImage = New BitmapImage()
                                           TempBitmapImage.BeginInit()
                                           TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                           TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                           TempBitmapImage.UriSource = New Uri(OFD.FileName, UriKind.RelativeOrAbsolute)
                                           TempBitmapImage.EndInit()
                                           CurrentProject.ProjectIcon = TempBitmapImage

                                           MainIconImageRectangle.BeginAnimation(OpacityProperty, HideAnimation)
                                           MainIconImage.ImageSource = TempBitmapImage
                                           MainIconImageRectangle.BeginAnimation(OpacityProperty, ShowAnimation)
                                       End Sub)

                'Copy file to project folder
                File.Copy(OFD.FileName, CurrentProject.ProjectPath + "\sce_sys\icon0.png", True)

                'Save project configuration file with new path
                If Not String.IsNullOrEmpty(CurrentProjectTextBlock.Text) Then
                    Dim ProjectINI As New IniFile(CurrentProjectTextBlock.Text)
                    ProjectINI.IniWriteValue("PKGProject", "Icon", CurrentProject.ProjectPath + "\sce_sys\icon0.png")
                End If

                MsgBox("Icon changed!", MsgBoxStyle.Information)
            End If
        Else
            MsgBox("No project loaded.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub BackgroundImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles BackgroundImage.MouseLeftButtonDown
        If Not String.IsNullOrEmpty(CurrentProject.ProjectPath) Then
            Dim OFD As New OpenFileDialog() With {.Title = "Select a new background image", .Filter = "PNG Image (*.png)|*.png", .Multiselect = False}
            If OFD.ShowDialog() = Windows.Forms.DialogResult.OK Then
                'Show new background
                Dispatcher.BeginInvoke(Sub()
                                           Dim TempBitmapImage = New BitmapImage()
                                           TempBitmapImage.BeginInit()
                                           TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                           TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                           TempBitmapImage.UriSource = New Uri(OFD.FileName, UriKind.RelativeOrAbsolute)
                                           TempBitmapImage.EndInit()
                                           CurrentProject.ProjectBackground = TempBitmapImage

                                           BackgroundImage.BeginAnimation(OpacityProperty, HideAnimation)
                                           BackgroundImage.Source = TempBitmapImage
                                           BackgroundImage.BeginAnimation(OpacityProperty, ShowAnimation)
                                       End Sub)

                'Copy file to project folder
                File.Copy(OFD.FileName, CurrentProject.ProjectPath + "\sce_sys\pic0.png", True)

                'Save project configuration file with new path
                If Not String.IsNullOrEmpty(CurrentProjectTextBlock.Text) Then
                    Dim ProjectINI As New IniFile(CurrentProjectTextBlock.Text)
                    ProjectINI.IniWriteValue("PKGProject", "Background", CurrentProject.ProjectPath + "\sce_sys\pic0.png")
                End If

                MsgBox("Background changed!", MsgBoxStyle.Information)
            End If
        Else
            MsgBox("No project loaded.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub TitleTextBlock_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles TitleTextBlock.MouseLeftButtonDown
        If Not String.IsNullOrEmpty(CurrentProject.ProjectPath) Then
            Dim NewTitle As String = InputBox("Enter a new title :", "Set Title", "")
            If Not String.IsNullOrEmpty(NewTitle) Then
                'Save changes in param.json
                If File.Exists(CurrentProject.ProjectPath + "\sce_sys\param.json") Then
                    ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "Title", NewTitle)
                Else
                    CreateParam(CurrentProject.ProjectPath + "\sce_sys\param.json")
                    ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "Title", NewTitle)
                End If

                'Save changes in manifest.json
                If File.Exists(CurrentProject.ProjectPath + "\manifest.json") Then
                    ChangeManifest(CurrentProject.ProjectPath + "\manifest.json", "ApplicationName", NewTitle)
                Else
                    CreateManifest(CurrentProject.ProjectPath + "\manifest.json")
                    ChangeManifest(CurrentProject.ProjectPath + "\manifest.json", "ApplicationName", NewTitle)
                End If

                TitleTextBlock.Text = NewTitle
                MsgBox("Title changed!", MsgBoxStyle.Information)
            End If
        Else
            MsgBox("No project loaded.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub SetBackgroundButton_Click(sender As Object, e As RoutedEventArgs) Handles SetBackgroundButton.Click
        If Not String.IsNullOrEmpty(CurrentProject.ProjectPath) Then
            Dim OFD As New OpenFileDialog() With {.Title = "Select a new background image", .Filter = "PNG Image (*.png)|*.png", .Multiselect = False}
            If OFD.ShowDialog() = Windows.Forms.DialogResult.OK Then
                'Show new background
                Dispatcher.BeginInvoke(Sub()
                                           Dim TempBitmapImage = New BitmapImage()
                                           TempBitmapImage.BeginInit()
                                           TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                           TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                           TempBitmapImage.UriSource = New Uri(OFD.FileName, UriKind.RelativeOrAbsolute)
                                           TempBitmapImage.EndInit()
                                           CurrentProject.ProjectBackground = TempBitmapImage

                                           BackgroundImage.BeginAnimation(OpacityProperty, HideAnimation)
                                           BackgroundImage.Source = TempBitmapImage
                                           BackgroundImage.BeginAnimation(OpacityProperty, ShowAnimation)
                                       End Sub)

                'Copy file to project folder
                File.Copy(OFD.FileName, CurrentProject.ProjectPath + "\sce_sys\pic0.png", True)

                'Save project configuration file with new path
                If Not String.IsNullOrEmpty(CurrentProjectTextBlock.Text) Then
                    Dim ProjectINI As New IniFile(CurrentProjectTextBlock.Text)
                    ProjectINI.IniWriteValue("PKGProject", "Background", CurrentProject.ProjectPath + "\sce_sys\pic0.png")
                End If

                MsgBox("Background changed!", MsgBoxStyle.Information)
            End If
        Else
            MsgBox("No project loaded.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub SetSoundtrackButton_Click(sender As Object, e As RoutedEventArgs) Handles SetSoundtrackButton.Click
        If Not String.IsNullOrEmpty(CurrentProject.ProjectPath) Then
            Dim OFD As New OpenFileDialog() With {.Title = "Select a new soundtrack", .Filter = "AT9 Audio (*.at9)|*.at9", .Multiselect = False}
            If OFD.ShowDialog() = Windows.Forms.DialogResult.OK Then
                'Copy file to project folder
                File.Copy(OFD.FileName, CurrentProject.ProjectPath + "\sce_sys\snd0.at9", True)

                'Save project configuration file
                If Not String.IsNullOrEmpty(CurrentProjectTextBlock.Text) Then
                    Dim ProjectINI As New IniFile(CurrentProjectTextBlock.Text)
                    ProjectINI.IniWriteValue("PKGProject", "Soundtrack", CurrentProject.ProjectPath + "\sce_sys\icon0.png")
                End If

                MsgBox("Soundtrack changed!", MsgBoxStyle.Information)
            End If
        Else
            MsgBox("No project loaded.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub SetURLButton_Click(sender As Object, e As RoutedEventArgs) Handles SetURLButton.Click
        If Not String.IsNullOrEmpty(CurrentProject.ProjectPath) Then
            Dim NewURL As String = InputBox("Enter an action or website URL :", "Set URL", "")
            If Not String.IsNullOrEmpty(NewURL) Then
                If File.Exists(CurrentProject.ProjectPath + "\sce_sys\param.json") Then
                    ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "URL", NewURL)
                Else
                    CreateParam(CurrentProject.ProjectPath + "\sce_sys\param.json")
                    ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "URL", NewURL)
                End If

                MsgBox("URL changed!", MsgBoxStyle.Information)
            End If
        Else
            MsgBox("No project loaded.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub SetContentIDButton_Click(sender As Object, e As RoutedEventArgs) Handles SetContentIDButton.Click
        If Not String.IsNullOrEmpty(CurrentProject.ProjectPath) Then
            Dim NewContentID As String = InputBox("Enter a new Content ID :", "Set Content ID", "IV9999-NPXS12345_00-XXXXXXXXXXXXXXXX")
            If Not String.IsNullOrEmpty(NewContentID) Then
                'Save changes in param.json
                If File.Exists(CurrentProject.ProjectPath + "\sce_sys\param.json") Then
                    ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "ContentID", NewContentID)
                Else
                    CreateParam(CurrentProject.ProjectPath + "\sce_sys\param.json")
                    ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "ContentID", NewContentID)
                End If

                MsgBox("Content ID changed!", MsgBoxStyle.Information)
            End If
        Else
            MsgBox("No project loaded.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub SetTitleIDButton_Click(sender As Object, e As RoutedEventArgs) Handles SetTitleIDButton.Click
        If Not String.IsNullOrEmpty(CurrentProject.ProjectPath) Then
            Dim NewTitleID As String = InputBox("Enter a new Title ID :", "Set Title ID", "NPXS12345")
            If Not String.IsNullOrEmpty(NewTitleID) Then
                'Save changes in param.json
                If File.Exists(CurrentProject.ProjectPath + "\sce_sys\param.json") Then
                    ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "TitleID", NewTitleID)
                Else
                    CreateParam(CurrentProject.ProjectPath + "\sce_sys\param.json")
                    ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "TitleID", NewTitleID)
                End If

                'Save changes in manifest.json
                If File.Exists(CurrentProject.ProjectPath + "\manifest.json") Then
                    ChangeManifest(CurrentProject.ProjectPath + "\manifest.json", "TitleID", NewTitleID)
                Else
                    CreateManifest(CurrentProject.ProjectPath + "\manifest.json")
                    ChangeManifest(CurrentProject.ProjectPath + "\manifest.json", "TitleID", NewTitleID)
                End If

                MsgBox("Title ID changed!", MsgBoxStyle.Information)
            End If
        Else
            MsgBox("No project loaded.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub SetVersionButton_Click(sender As Object, e As RoutedEventArgs) Handles SetVersionButton.Click
        If Not String.IsNullOrEmpty(CurrentProject.ProjectPath) Then
            Dim NewVersion As String = InputBox("Enter a new version like the default value :", "Set Version", "01.000.000")
            If Not String.IsNullOrEmpty(NewVersion) Then

                Dim SplittedValues As String() = NewVersion.Split("."c)
                Dim ApplicationMajorVersion As String = ""
                Dim ApplicationMinorVersion As String = ""
                Dim ApplicationPatchVersion As String = ""

                If SplittedValues(0).StartsWith("0") Then
                    ApplicationMajorVersion = SplittedValues(0).Remove(0, 1)
                End If
                If SplittedValues(0) = "00" Then
                    ApplicationMajorVersion = SplittedValues(0).Replace("00", "0")
                End If
                If String.IsNullOrEmpty(ApplicationMajorVersion) Then
                    ApplicationMajorVersion = SplittedValues(0)
                End If

                If SplittedValues(1) = "000" Then
                    ApplicationMinorVersion = SplittedValues(1).Replace("000", "0")
                End If
                If SplittedValues(1).Contains("00") Then
                    ApplicationMinorVersion = SplittedValues(1).Replace("00", "")
                End If
                If String.IsNullOrEmpty(ApplicationMinorVersion) Then
                    ApplicationMinorVersion = SplittedValues(1)
                End If

                If SplittedValues(2) = "000" Then
                    ApplicationPatchVersion = SplittedValues(2).Replace("000", "0")
                End If
                If SplittedValues(2).Contains("00") Then
                    ApplicationPatchVersion = SplittedValues(2).Replace("00", "")
                End If
                If String.IsNullOrEmpty(ApplicationPatchVersion) Then
                    ApplicationPatchVersion = SplittedValues(2)
                End If

                Dim ApplicationVersion As String = ApplicationMajorVersion + "." + ApplicationMinorVersion + "." + ApplicationPatchVersion + "+000"
                Dim ReactNativePlaystationVersion As String = ApplicationMajorVersion + "." + ApplicationMinorVersion + "." + ApplicationPatchVersion + "-000.0"
                Dim MasterVersion As String = SplittedValues(0) + "." + SplittedValues(1).Remove(0, 1)

                'Save changes in param.json
                If File.Exists(CurrentProject.ProjectPath + "\sce_sys\param.json") Then
                    If Not String.IsNullOrEmpty(NewVersion) AndAlso Not String.IsNullOrEmpty(MasterVersion) Then
                        ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "ContentVersion", NewVersion)
                        ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "MasterVersion", MasterVersion)
                    End If
                Else
                    CreateParam(CurrentProject.ProjectPath + "\sce_sys\param.json")

                    If Not String.IsNullOrEmpty(NewVersion) AndAlso Not String.IsNullOrEmpty(MasterVersion) Then
                        ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "ContentVersion", NewVersion)
                        ChangeParam(CurrentProject.ProjectPath + "\sce_sys\param.json", "MasterVersion", MasterVersion)
                    End If
                End If

                'Save changes in manifest.json
                If File.Exists(CurrentProject.ProjectPath + "\manifest.json") Then
                    If Not String.IsNullOrEmpty(ApplicationVersion) Then
                        ChangeManifest(CurrentProject.ProjectPath + "\manifest.json", "ApplicationVersion", ApplicationVersion)
                        ChangeManifest(CurrentProject.ProjectPath + "\manifest.json", "ReactNativePlaystationVersion", ReactNativePlaystationVersion)
                    End If
                Else
                    CreateManifest(CurrentProject.ProjectPath + "\manifest.json")

                    If Not String.IsNullOrEmpty(ApplicationVersion) Then
                        ChangeManifest(CurrentProject.ProjectPath + "\manifest.json", "ApplicationVersion", ApplicationVersion)
                        ChangeManifest(CurrentProject.ProjectPath + "\manifest.json", "ReactNativePlaystationVersion", ReactNativePlaystationVersion)
                    End If
                End If

                MsgBox("Version changed!", MsgBoxStyle.Information)
            End If
        Else
            MsgBox("No project loaded.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub FinalizeButton_Click(sender As Object, e As RoutedEventArgs) Handles FinalizeButton.Click
        If Not String.IsNullOrEmpty(CurrentProject.ProjectPath) AndAlso Not String.IsNullOrEmpty(CurrentProjectTextBlock.Text) Then
            If FinalizeProject(CurrentProjectTextBlock.Text) = True Then MsgBox("Project finalized! You can build the pkg now.", MsgBoxStyle.Information)
        Else
            MsgBox("No project loaded.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub BuildProjectButton_Click(sender As Object, e As RoutedEventArgs) Handles BuildProjectButton.Click
        If Not String.IsNullOrEmpty(CurrentProject.ProjectPath) AndAlso Not String.IsNullOrEmpty(CurrentProjectTextBlock.Text) Then

            Dim SFD As New SaveFileDialog() With {.Title = "Select a save path for the PKG file", .Filter = "PKG Files (*.pkg)|*.pkg", .DefaultExt = ".pkg"}
            If SFD.ShowDialog() = Windows.Forms.DialogResult.OK Then
                Dim PKGDestinationPath As String = SFD.FileName
                Dim GP5ProjectPath As String = CurrentProject.ProjectPath + "\" + CurrentProject.ProjectTitle + ".gp5"

                'Disable buttons
                SaveProjectButton.IsEnabled = False
                FinalizeButton.IsEnabled = False
                BuildProjectButton.IsEnabled = False
                SetBackgroundButton.IsEnabled = False
                SetSoundtrackButton.IsEnabled = False
                SetURLButton.IsEnabled = False
                SetContentIDButton.IsEnabled = False
                SetTitleIDButton.IsEnabled = False
                SetVersionButton.IsEnabled = False
                GamesTextBlock.IsEnabled = False
                MediaTextBlock.IsEnabled = False
                TitleTextBlock.IsEnabled = False
                MainIconImageRectangle.IsEnabled = False
                BackgroundImage.IsEnabled = False

                BuildPKG(GP5ProjectPath, PKGDestinationPath)
            End If

        End If
    End Sub


    Private Sub CreateParam(DestinationPath As String)
        Dim NewPS5Param As New PS5Param() With {
            .AgeLevel = New AgeLevel() With {
                .[Default] = 0, .US = 0},
            .ApplicationCategoryType = 0,
            .ApplicationDrmType = "free",
            .Attribute = 0,
            .Attribute2 = 0,
            .Attribute3 = 0,
            .ConceptId = "99999",
            .ContentBadgeType = 0,
            .ContentId = "IV9999-NPXS12345_00-XXXXXXXXXXXXXXXX",
            .ContentVersion = "01.000.000",
            .DownloadDataSize = 0,
            .LocalizedParameters = New LocalizedParameters() With {
                .DefaultLanguage = "en-US",
                .EnUS = New EnUS() With {.TitleName = "Title Name"},
                .ArAE = New ArAE() With {.TitleName = "Title Name"},
                .CsCZ = New CsCZ() With {.TitleName = "Title Name"},
                .DaDK = New DaDK() With {.TitleName = "Title Name"},
                .DeDE = New DeDE() With {.TitleName = "Title Name"},
                .ElGR = New ElGR() With {.TitleName = "Title Name"},
                .EnGB = New EnGB() With {.TitleName = "Title Name"},
                .Es419 = New Es419() With {.TitleName = "Title Name"},
                .EsES = New EsES() With {.TitleName = "Title Name"},
                .FiFI = New FiFI() With {.TitleName = "Title Name"},
                .FrCA = New FrCA() With {.TitleName = "Title Name"},
                .FrFR = New FrFR() With {.TitleName = "Title Name"},
                .HuHU = New HuHU() With {.TitleName = "Title Name"},
                .IdID = New IdID() With {.TitleName = "Title Name"},
                .ItIT = New ItIT() With {.TitleName = "Title Name"},
                .JaJP = New JaJP() With {.TitleName = "Title Name"},
                .KoKR = New KoKR() With {.TitleName = "Title Name"},
                .NlNL = New NlNL() With {.TitleName = "Title Name"},
                .NoNO = New NoNO() With {.TitleName = "Title Name"},
                .PlPL = New PlPL() With {.TitleName = "Title Name"},
                .PtBR = New PtBR() With {.TitleName = "Title Name"},
                .PtPT = New PtPT() With {.TitleName = "Title Name"},
                .RoRO = New RoRO() With {.TitleName = "Title Name"},
                .RuRU = New RuRU() With {.TitleName = "Title Name"},
                .SvSE = New SvSE() With {.TitleName = "Title Name"},
                .ThTH = New ThTH() With {.TitleName = "Title Name"},
                .TrTR = New TrTR() With {.TitleName = "Title Name"},
                .ViVN = New ViVN() With {.TitleName = "Title Name"},
                .ZhHans = New ZhHans() With {.TitleName = "Title Name"},
                .ZhHant = New ZhHant() With {.TitleName = "Title Name"}},
            .MasterVersion = "01.00",
            .TitleId = "NPXS12345"}

        Dim RawDataJSON As String = JsonConvert.SerializeObject(NewPS5Param, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
        File.WriteAllText(DestinationPath, RawDataJSON)
    End Sub

    Private Sub ChangeParam(DestinationPath As String, Param As String, NewValue As String)
        Dim JSONData As String = File.ReadAllText(DestinationPath)
        Try

            'Read values
            Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)

            'Change value
            Select Case Param
                Case "Category"
                    If NewValue = "Media" Then
                        ParamData.ApplicationCategoryType = 65536
                    Else
                        ParamData.ApplicationCategoryType = 0
                    End If
                Case "ContentID"
                    ParamData.ContentId = NewValue
                Case "ContentVersion"
                    ParamData.ContentVersion = NewValue
                Case "Title"
                    ParamData.LocalizedParameters = New LocalizedParameters() With {
                .DefaultLanguage = "en-US",
                .EnUS = New EnUS() With {.TitleName = NewValue},
                .ArAE = New ArAE() With {.TitleName = NewValue},
                .CsCZ = New CsCZ() With {.TitleName = NewValue},
                .DaDK = New DaDK() With {.TitleName = NewValue},
                .DeDE = New DeDE() With {.TitleName = NewValue},
                .ElGR = New ElGR() With {.TitleName = NewValue},
                .EnGB = New EnGB() With {.TitleName = NewValue},
                .Es419 = New Es419() With {.TitleName = NewValue},
                .EsES = New EsES() With {.TitleName = NewValue},
                .FiFI = New FiFI() With {.TitleName = NewValue},
                .FrCA = New FrCA() With {.TitleName = NewValue},
                .FrFR = New FrFR() With {.TitleName = NewValue},
                .HuHU = New HuHU() With {.TitleName = NewValue},
                .IdID = New IdID() With {.TitleName = NewValue},
                .ItIT = New ItIT() With {.TitleName = NewValue},
                .JaJP = New JaJP() With {.TitleName = NewValue},
                .KoKR = New KoKR() With {.TitleName = NewValue},
                .NlNL = New NlNL() With {.TitleName = NewValue},
                .NoNO = New NoNO() With {.TitleName = NewValue},
                .PlPL = New PlPL() With {.TitleName = NewValue},
                .PtBR = New PtBR() With {.TitleName = NewValue},
                .PtPT = New PtPT() With {.TitleName = NewValue},
                .RoRO = New RoRO() With {.TitleName = NewValue},
                .RuRU = New RuRU() With {.TitleName = NewValue},
                .SvSE = New SvSE() With {.TitleName = NewValue},
                .ThTH = New ThTH() With {.TitleName = NewValue},
                .TrTR = New TrTR() With {.TitleName = NewValue},
                .ViVN = New ViVN() With {.TitleName = NewValue},
                .ZhHans = New ZhHans() With {.TitleName = NewValue},
                .ZhHant = New ZhHant() With {.TitleName = NewValue}}
                Case "TitleID"
                    ParamData.TitleId = NewValue
                Case "MasterVersion"
                    ParamData.MasterVersion = NewValue
                Case "TitleId"
                    ParamData.TitleId = NewValue
                Case "URL"
                    ParamData.DeeplinkUri = NewValue
            End Select

            'Write back
            Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
            File.WriteAllText(DestinationPath, RawDataJSON)
        Catch ex As JsonSerializationException
            MsgBox("Could not parse the param.json file.", MsgBoxStyle.Critical, "Error")
        End Try
    End Sub

    Private Sub CreateManifest(DestinationPath As String)
        Dim NewPS5Manifest As New PS5Manifest() With {
            .applicationName = "No Title",
            .applicationVersion = "0.0.0+000",
            .bootAnimation = "default",
            .commitHash = "",
            .titleId = "NPXS12345",
            .repositoryUrl = "",
            .reactNativePlaystationVersion = "0.00.0-000.0",
            .applicationData = New ApplicationData() With {.branchType = "release"},
            .twinTurbo = True}

        Dim RawDataJSON As String = JsonConvert.SerializeObject(NewPS5Manifest, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
        File.WriteAllText(DestinationPath, RawDataJSON)
    End Sub

    Private Sub ChangeManifest(DestinationPath As String, Param As String, NewValue As String)
        Dim JSONData As String = File.ReadAllText(DestinationPath)
        Try

            'Read values
            Dim ManifestData As PS5Manifest = JsonConvert.DeserializeObject(Of PS5Manifest)(JSONData)

            'Set value
            Select Case Param
                Case "ApplicationName"
                    ManifestData.applicationName = NewValue
                Case "ApplicationVersion"
                    ManifestData.applicationVersion = NewValue
                Case "TitleID"
                    ManifestData.titleId = NewValue
                Case "ReactNativePlaystationVersion"
                    ManifestData.reactNativePlaystationVersion = NewValue
            End Select

            'Write back
            Dim RawDataJSON As String = JsonConvert.SerializeObject(ManifestData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
            File.WriteAllText(DestinationPath, RawDataJSON)
        Catch ex As JsonSerializationException
            MsgBox("Could not parse the manifest.json file.", MsgBoxStyle.Critical, "Error")
        End Try
    End Sub

    Private Function FinalizeProject(ProjectConfig As String) As Boolean
        If Not String.IsNullOrEmpty(ProjectConfig) AndAlso Not String.IsNullOrEmpty(CurrentProject.ProjectPath) Then

            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") Then

                Dim ProjectINI As New IniFile(ProjectConfig)
                Dim GP5ProjectPath As String = CurrentProject.ProjectPath + "\" + CurrentProject.ProjectTitle + ".gp5"

                PubToolsPath = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"

                If Not String.IsNullOrEmpty(CurrentProject.ProjectTitle) Then
                    Try
                        Dim DateAndTime As String = Date.Now.ToString("yyyy-MM-dd")
                        Dim ProjectSCESYSFolder As String = CurrentProject.ProjectPath + "\sce_sys"

                        'Create the gp5 project
                        Using PubCMD As New Process()
                            PubCMD.StartInfo.FileName = PubToolsPath
                            PubCMD.StartInfo.Arguments = "gp5_proj_create --volume_type prospero_app --passcode GvE6xCpZxd96scOUGuLPbuLp8O800B0s --c_date " + DateAndTime + " """ + GP5ProjectPath + """"
                            PubCMD.StartInfo.UseShellExecute = False
                            PubCMD.StartInfo.CreateNoWindow = True
                            PubCMD.Start()
                            PubCMD.WaitForExit()
                        End Using

                        'Create sce_sys folder inside gp5 project
                        Using PubCMD As New Process()
                            PubCMD.StartInfo.FileName = PubToolsPath
                            PubCMD.StartInfo.Arguments = "gp5_dir_add --src_path """ + ProjectSCESYSFolder + """ --dst_path sce_sys --chunk 0 """ + GP5ProjectPath + """"
                            PubCMD.StartInfo.UseShellExecute = False
                            PubCMD.StartInfo.CreateNoWindow = True
                            PubCMD.Start()
                            PubCMD.WaitForExit()
                        End Using

                        'Add manifest.json to gp5 project
                        If File.Exists(CurrentProject.ProjectPath + "\manifest.json") Then
                            Using PubCMD As New Process()
                                PubCMD.StartInfo.FileName = PubToolsPath
                                PubCMD.StartInfo.Arguments = "gp5_file_add --src_path """ + CurrentProject.ProjectPath + "\manifest.json" + """ --dst_path manifest.json --chunk 0 """ + GP5ProjectPath + """"
                                PubCMD.StartInfo.UseShellExecute = False
                                PubCMD.StartInfo.CreateNoWindow = True
                                PubCMD.Start()
                                PubCMD.WaitForExit()
                            End Using
                        End If

                        'Add param.json to gp5 project
                        If File.Exists(ProjectSCESYSFolder + "\param.json") Then
                            Using PubCMD As New Process()
                                PubCMD.StartInfo.FileName = PubToolsPath
                                PubCMD.StartInfo.Arguments = "gp5_file_add --src_path """ + ProjectSCESYSFolder + "\param.json" + """ --dst_path sce_sys\param.json --chunk 0 """ + GP5ProjectPath + """"
                                PubCMD.StartInfo.UseShellExecute = False
                                PubCMD.StartInfo.CreateNoWindow = True
                                PubCMD.Start()
                                PubCMD.WaitForExit()
                            End Using
                        End If

                        'Add icon to gp5 project
                        If Not String.IsNullOrEmpty(ProjectINI.IniReadValue("PKGProject", "Icon")) Then
                            If File.Exists(ProjectINI.IniReadValue("PKGProject", "Icon")) Then
                                Using PubCMD As New Process()
                                    PubCMD.StartInfo.FileName = PubToolsPath
                                    PubCMD.StartInfo.Arguments = "gp5_file_add --src_path """ + ProjectSCESYSFolder + "\icon0.png" + """ --dst_path sce_sys\icon0.png --chunk 0 """ + GP5ProjectPath + """"
                                    PubCMD.StartInfo.UseShellExecute = False
                                    PubCMD.StartInfo.CreateNoWindow = True
                                    PubCMD.Start()
                                    PubCMD.WaitForExit()
                                End Using
                            End If
                        End If

                        'Add background to gp5 project
                        If Not String.IsNullOrEmpty(ProjectINI.IniReadValue("PKGProject", "Background")) Then
                            If File.Exists(ProjectINI.IniReadValue("PKGProject", "Background")) Then
                                Using PubCMD As New Process()
                                    PubCMD.StartInfo.FileName = PubToolsPath
                                    PubCMD.StartInfo.Arguments = "gp5_file_add --src_path """ + ProjectSCESYSFolder + "\pic0.png" + """ --dst_path sce_sys\pic0.png --chunk 0 """ + GP5ProjectPath + """"
                                    PubCMD.StartInfo.UseShellExecute = False
                                    PubCMD.StartInfo.CreateNoWindow = True
                                    PubCMD.Start()
                                    PubCMD.WaitForExit()
                                End Using
                            End If
                        End If

                        'Add soundtrack to gp5 project
                        If Not String.IsNullOrEmpty(ProjectINI.IniReadValue("PKGProject", "Soundtrack")) Then
                            If File.Exists(ProjectINI.IniReadValue("PKGProject", "Soundtrack")) Then
                                Using PubCMD As New Process()
                                    PubCMD.StartInfo.FileName = PubToolsPath
                                    PubCMD.StartInfo.Arguments = "gp5_file_add --src_path """ + ProjectSCESYSFolder + "\snd0.at9" + """ --dst_path sce_sys\snd0.at9 --chunk 0 """ + GP5ProjectPath + """"
                                    PubCMD.StartInfo.UseShellExecute = False
                                    PubCMD.StartInfo.CreateNoWindow = True
                                    PubCMD.Start()
                                    PubCMD.WaitForExit()
                                End Using
                            End If
                        End If

                        CurrentProject.GP5Created = True
                        ProjectINI.IniWriteValue("PKGProject", "GP5Created", "True")
                        BuildProjectButton.IsEnabled = True

                        Return True
                    Catch ex As Exception
                        MsgBox("Could not create the gp5 project.", MsgBoxStyle.Critical, "Error")
                        Return False
                    End Try
                Else
                    Return False
                End If
            Else
                MsgBox("Cannot finalize project without publishing tools. You can get it in the settings.", MsgBoxStyle.Information)
                Return False
            End If
        Else
            MsgBox("No project loaded.", MsgBoxStyle.Information)
            Return False
        End If
    End Function

    Private Sub BuildPKG(ProjectPath As String, DestinationPath As String)
        PKGBuilder = New Process()
        PKGBuilder.StartInfo.FileName = PubToolsPath
        PKGBuilder.StartInfo.RedirectStandardOutput = True
        AddHandler PKGBuilder.OutputDataReceived, AddressOf OutputDataHandler
        PKGBuilder.StartInfo.UseShellExecute = False
        PKGBuilder.StartInfo.CreateNoWindow = True
        PKGBuilder.EnableRaisingEvents = True
        PKGBuilder.StartInfo.Arguments = "img_create --oformat nwonly """ + ProjectPath + """ """ + DestinationPath + """"
        PKGBuilder.Start()
        PKGBuilder.BeginOutputReadLine()
    End Sub

    Public Sub OutputDataHandler(sender As Object, e As DataReceivedEventArgs)
        If Not String.IsNullOrEmpty(e.Data) Then
            PKGBuilderOuput += e.Data + vbCrLf
        End If
    End Sub

    Private Sub PKGBuilder_Exited(sender As Object, e As EventArgs) Handles PKGBuilder.Exited
        PKGBuilder.CancelOutputRead()
        PKGBuilder = Nothing

        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub()
                                       'Re-enable buttons
                                       SaveProjectButton.IsEnabled = True
                                       FinalizeButton.IsEnabled = True
                                       BuildProjectButton.IsEnabled = True
                                       SetBackgroundButton.IsEnabled = True
                                       SetSoundtrackButton.IsEnabled = True
                                       SetURLButton.IsEnabled = True
                                       SetContentIDButton.IsEnabled = True
                                       SetTitleIDButton.IsEnabled = True
                                       SetVersionButton.IsEnabled = True
                                       GamesTextBlock.IsEnabled = True
                                       MediaTextBlock.IsEnabled = True
                                       TitleTextBlock.IsEnabled = True
                                       MainIconImageRectangle.IsEnabled = True
                                       BackgroundImage.IsEnabled = True
                                   End Sub)
        Else
            'Re-enable buttons
            SaveProjectButton.IsEnabled = True
            FinalizeButton.IsEnabled = True
            BuildProjectButton.IsEnabled = True
            SetBackgroundButton.IsEnabled = True
            SetSoundtrackButton.IsEnabled = True
            SetURLButton.IsEnabled = True
            SetContentIDButton.IsEnabled = True
            SetTitleIDButton.IsEnabled = True
            SetVersionButton.IsEnabled = True
            GamesTextBlock.IsEnabled = True
            MediaTextBlock.IsEnabled = True
            TitleTextBlock.IsEnabled = True
            MainIconImageRectangle.IsEnabled = True
            BackgroundImage.IsEnabled = True
        End If

        If PKGBuilderOuput.Contains("Create image Process finished with warning(s).") Then
            MsgBox("PKG created!", MsgBoxStyle.Information, "Success")
        Else
            MsgBox("Error while creating the PKG :" + vbCrLf + PKGBuilderOuput, MsgBoxStyle.Exclamation)
        End If

    End Sub

    Private Sub GP5PKGBuilder_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        ClockTextBlock.Text = Date.Now.ToString("HH:mm")
        ClockTimer = New DispatcherTimer With {.Interval = New TimeSpan(0, 1, 0)} 'Update every minute only
        ClockTimer.Start()
    End Sub

    Private Sub ClockTimer_Tick(sender As Object, e As EventArgs) Handles ClockTimer.Tick
        ClockTextBlock.Text = Date.Now.ToString("HH:mm")
    End Sub

End Class
