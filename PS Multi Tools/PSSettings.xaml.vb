Imports System.IO
Imports System.Net
Imports PS_Multi_Tools.INI

Public Class PSSettings

    Private Sub ShowKlogButton_Click(sender As Object, e As RoutedEventArgs) Handles ShowKlogButton.Click
        Dim NewLogWindow As New PS5Log() With {.ShowActivated = True}
        If Not String.IsNullOrEmpty(PS5IPTextBox.Text) Then
            NewLogWindow.PS5IPTextBox.Text = PS5IPTextBox.Text
        End If
        NewLogWindow.Show()
    End Sub

    Private Sub LoadIconCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles LoadIconCheckBox.Checked
        Dim MainConfig As New IniFile(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini")
        MainConfig.IniWriteValue("PS5 Library", "LoadIcons", "True")
    End Sub

    Private Sub LoadBackgroundCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles LoadBackgroundCheckBox.Checked
        Dim MainConfig As New IniFile(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini")
        MainConfig.IniWriteValue("PS5 Library", "LoadBackgrounds", "True")
    End Sub

    Private Sub SkipFileCheckCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SkipFileCheckCheckBox.Checked
        Dim MainConfig As New IniFile(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini")
        MainConfig.IniWriteValue("PS5 Library", "SkipFileChecks", "True")
    End Sub

    Private Sub LoadIconCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles LoadIconCheckBox.Unchecked
        Dim MainConfig As New IniFile(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini")
        MainConfig.IniWriteValue("PS5 Library", "LoadIcons", "False")
    End Sub

    Private Sub LoadBackgroundCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles LoadBackgroundCheckBox.Unchecked
        Dim MainConfig As New IniFile(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini")
        MainConfig.IniWriteValue("PS5 Library", "LoadBackgrounds", "False")
    End Sub

    Private Sub SkipFileCheckCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SkipFileCheckCheckBox.Unchecked
        Dim MainConfig As New IniFile(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini")
        MainConfig.IniWriteValue("PS5 Library", "SkipFileChecks", "False")
    End Sub

    Private Sub SavePS5AddressButton_Click(sender As Object, e As RoutedEventArgs) Handles SavePS5AddressButton.Click
        If Not String.IsNullOrEmpty(PS5IPTextBox.Text) AndAlso Not String.IsNullOrEmpty(PS5FTPPortTextBox.Text) Then
            Dim MainConfig As New IniFile(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini")
            MainConfig.IniWriteValue("PS5 Tools", "IP", PS5IPTextBox.Text)
            MainConfig.IniWriteValue("PS5 Tools", "Port", PS5FTPPortTextBox.Text)
        End If
    End Sub

    Private Sub PSSettings_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Load existing config
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini") Then
            Dim MainConfig As New IniFile(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini")

            If Not String.IsNullOrEmpty(MainConfig.IniReadValue("PS5 Library", "LoadIcons")) Then
                If MainConfig.IniReadValue("PS5 Library", "LoadIcons") = "False" Then
                    LoadIconCheckBox.IsChecked = False
                Else
                    LoadIconCheckBox.IsChecked = True
                End If
            End If
            If Not String.IsNullOrEmpty(MainConfig.IniReadValue("PS5 Library", "LoadBackgrounds")) Then
                If MainConfig.IniReadValue("PS5 Library", "LoadBackgrounds") = "False" Then
                    LoadBackgroundCheckBox.IsChecked = False
                Else
                    LoadBackgroundCheckBox.IsChecked = True
                End If
            End If
            If Not String.IsNullOrEmpty(MainConfig.IniReadValue("PS5 Library", "SkipFileChecks")) Then
                If MainConfig.IniReadValue("PS5 Library", "SkipFileChecks") = "False" Then
                    SkipFileCheckCheckBox.IsChecked = False
                Else
                    SkipFileCheckCheckBox.IsChecked = True
                End If
            End If
            If Not String.IsNullOrEmpty(MainConfig.IniReadValue("PS5 Tools", "IP")) Then
                PS5IPTextBox.Text = MainConfig.IniReadValue("PS5 Tools", "IP")
            End If
            If Not String.IsNullOrEmpty(MainConfig.IniReadValue("PS5 Tools", "Port")) Then
                PS5FTPPortTextBox.Text = MainConfig.IniReadValue("PS5 Tools", "Port")
            End If
        End If
    End Sub

    Private Sub CheckForUpdateButton_Click(sender As Object, e As RoutedEventArgs) Handles CheckForUpdateButton.Click
        If Utils.IsURLValid("http://X.X.X.X/psmt-lib.txt") Then
            Dim LibraryInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(My.Computer.FileSystem.CurrentDirectory + "\psmt-lib.dll")
            Dim CurrentLibraryVersion As String = LibraryInfo.FileVersion

            Dim VerCheckClient As New WebClient()
            Dim NewLibraryVersion As String = VerCheckClient.DownloadString("http://X.X.X.X/psmt-lib.txt")
            Dim Changelog As String = VerCheckClient.DownloadString("http://X.X.X.X/changelog.txt")

            If CurrentLibraryVersion < NewLibraryVersion Then
                If MsgBox("A library update is available, do you want to install it now ?", MsgBoxStyle.YesNo, "Library Update found") = MsgBoxResult.Yes Then
                    Dim NewUpdater As New SyncLibrary() With {.ShowActivated = True}
                    NewUpdater.ShowDialog()
                End If
            Else
                MsgBox("PS Multi Tools is up to date!", MsgBoxStyle.Information, "No update found")
            End If
        Else
            MsgBox("Could not check for updates. No internet connection available.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub GetPubToolsButton_Click(sender As Object, e As RoutedEventArgs) Handles GetPubToolsButton.Click
        If MsgBox("Click on :" + vbCrLf + vbCrLf + "[<> Code] -> [Download ZIP]" + vbCrLf + vbCrLf + "Extract content of ZIP in the ""Tools\PS5"" folder where PS Multi Tools is located.", MsgBoxStyle.Information) = MsgBoxResult.Ok Then
            Dim NewProcessStartInfo As New ProcessStartInfo("https://github.com/florinsdistortedvision/prospero-publishing-tools")
            Process.Start(NewProcessStartInfo)
        End If
    End Sub

End Class
