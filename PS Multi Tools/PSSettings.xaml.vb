Imports System.IO
Imports PS_Multi_Tools.INI

Public Class PSSettings

    Private Sub LoadIconCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles LoadIconCheckBox.Checked
        Dim MainConfig As New IniFile(Environment.CurrentDirectory + "\psmt-config.ini")
        MainConfig.IniWriteValue("PS5 Library", "LoadIcons", "True")
    End Sub

    Private Sub LoadBackgroundCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles LoadBackgroundCheckBox.Checked
        Dim MainConfig As New IniFile(Environment.CurrentDirectory + "\psmt-config.ini")
        MainConfig.IniWriteValue("PS5 Library", "LoadBackgrounds", "True")
    End Sub

    Private Sub SkipFileCheckCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SkipFileCheckCheckBox.Checked
        Dim MainConfig As New IniFile(Environment.CurrentDirectory + "\psmt-config.ini")
        MainConfig.IniWriteValue("PS5 Library", "SkipFileChecks", "True")
    End Sub

    Private Sub LoadIconCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles LoadIconCheckBox.Unchecked
        Dim MainConfig As New IniFile(Environment.CurrentDirectory + "\psmt-config.ini")
        MainConfig.IniWriteValue("PS5 Library", "LoadIcons", "False")
    End Sub

    Private Sub LoadBackgroundCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles LoadBackgroundCheckBox.Unchecked
        Dim MainConfig As New IniFile(Environment.CurrentDirectory + "\psmt-config.ini")
        MainConfig.IniWriteValue("PS5 Library", "LoadBackgrounds", "False")
    End Sub

    Private Sub SkipFileCheckCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SkipFileCheckCheckBox.Unchecked
        Dim MainConfig As New IniFile(Environment.CurrentDirectory + "\psmt-config.ini")
        MainConfig.IniWriteValue("PS5 Library", "SkipFileChecks", "False")
    End Sub

    Private Sub SavePS5AddressButton_Click(sender As Object, e As RoutedEventArgs) Handles SavePS5AddressButton.Click
        If Not String.IsNullOrEmpty(PS5IPTextBox.Text) AndAlso Not String.IsNullOrEmpty(PS5FTPPortTextBox.Text) Then
            Dim MainConfig As New IniFile(Environment.CurrentDirectory + "\psmt-config.ini")
            MainConfig.IniWriteValue("PS5 Tools", "IP", PS5IPTextBox.Text)
            MainConfig.IniWriteValue("PS5 Tools", "Port", PS5FTPPortTextBox.Text)
        End If
    End Sub

    Private Sub PSSettings_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Load existing config
        If File.Exists(Environment.CurrentDirectory + "\psmt-config.ini") Then
            Dim MainConfig As New IniFile(Environment.CurrentDirectory + "\psmt-config.ini")

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

End Class
