Imports System.IO
Imports PS_Multi_Tools.INI

Public Class PSSettings

    Dim MainConfig As New IniFile(Environment.CurrentDirectory + "\psmt-config.ini")

    Private Sub LoadIconCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles LoadIconCheckBox.Checked
        MainConfig.IniWriteValue("PS5 Library", "LoadIcons", "True")
    End Sub

    Private Sub LoadBackgroundCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles LoadBackgroundCheckBox.Checked
        MainConfig.IniWriteValue("PS5 Library", "LoadBackgrounds", "True")
    End Sub

    Private Sub SkipFileCheckCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SkipFileCheckCheckBox.Checked
        MainConfig.IniWriteValue("PS5 Library", "SkipFileChecks", "True")
    End Sub

    Private Sub LoadIconCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles LoadIconCheckBox.Unchecked
        MainConfig.IniWriteValue("PS5 Library", "LoadIcons", "False")
    End Sub

    Private Sub LoadBackgroundCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles LoadBackgroundCheckBox.Unchecked
        MainConfig.IniWriteValue("PS5 Library", "LoadBackgrounds", "False")
    End Sub

    Private Sub SkipFileCheckCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SkipFileCheckCheckBox.Unchecked
        MainConfig.IniWriteValue("PS5 Library", "SkipFileChecks", "False")
    End Sub

    Private Sub SaveConfigButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveConfigButton.Click
        If Not String.IsNullOrEmpty(PS5IPTextBox.Text) Then
            MainConfig.IniWriteValue("PS5 Tools", "IP", PS5IPTextBox.Text)
        End If
        If Not String.IsNullOrEmpty(PS5FTPPortTextBox.Text) Then
            MainConfig.IniWriteValue("PS5 Tools", "FTPPort", PS5FTPPortTextBox.Text)
        End If
        If Not String.IsNullOrEmpty(PS5PayloadPortTextBox.Text) Then
            MainConfig.IniWriteValue("PS5 Tools", "PayloadPort", PS5PayloadPortTextBox.Text)
        End If
    End Sub

    Private Sub PSSettings_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Load existing config
        If File.Exists(Environment.CurrentDirectory + "\psmt-config.ini") Then
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
            If Not String.IsNullOrEmpty(MainConfig.IniReadValue("PS5 Tools", "FTPPort")) Then
                PS5FTPPortTextBox.Text = MainConfig.IniReadValue("PS5 Tools", "FTPPort")
            End If
            If Not String.IsNullOrEmpty(MainConfig.IniReadValue("PS5 Tools", "PayloadPort")) Then
                PS5PayloadPortTextBox.Text = MainConfig.IniReadValue("PS5 Tools", "PayloadPort")
            End If
        End If
    End Sub

End Class
