Imports System.IO
Imports System.Security.Authentication
Imports FluentFTP
Imports PS_Multi_Tools.INI

Public Class PS5etaHENConfigurator

    Public ConsoleIP As String

    Private Sub GetConfigButton_Click(sender As Object, e As RoutedEventArgs) Handles GetConfigButton.Click
        If Not Directory.Exists(Environment.CurrentDirectory + "\Cache") Then Directory.CreateDirectory(Environment.CurrentDirectory + "\Cache")

        If Not String.IsNullOrEmpty(ConsoleIP) Then
            Try
                Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", 1337)
                    'Configurate the FTP connection
                    conn.Config.EncryptionMode = FtpEncryptionMode.None
                    conn.Config.SslProtocols = SslProtocols.None
                    conn.Config.DataConnectionEncryption = False

                    'Connect
                    conn.Connect()

                    'Get config.ini
                    conn.DownloadFile(Environment.CurrentDirectory + "\Cache\config.ini", "/data/etaHEN/config.ini", FtpLocalExists.Overwrite)

                    'Disconnect
                    conn.Disconnect()
                End Using

                'Load config after download
                If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
                    Dim ConfigInfo As New FileInfo(Environment.CurrentDirectory + "\Cache\config.ini")
                    Dim ConfigDateTime As Date = ConfigInfo.LastWriteTime
                    ConfigStatusTextBlock.Text = "etaHEN config found" + " - " + ConfigDateTime.ToString()

                    Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
                    If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "PS5Debug")) Then
                        If etaHENConfig.IniReadValue("Settings", "PS5Debug") = "0" Then
                            PS5DebugAutoLoadCheckBox.IsChecked = False
                        Else
                            PS5DebugAutoLoadCheckBox.IsChecked = True
                        End If
                    End If
                    If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "FTP")) Then
                        If etaHENConfig.IniReadValue("Settings", "FTP") = "0" Then
                            FTPCheckBox.IsChecked = False
                        Else
                            FTPCheckBox.IsChecked = True
                        End If
                    End If
                    If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "discord_rpc")) Then
                        If etaHENConfig.IniReadValue("Settings", "discord_rpc") = "0" Then
                            DiscordCheckBox.IsChecked = False
                        Else
                            DiscordCheckBox.IsChecked = True
                        End If
                    End If
                    If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "testkit")) Then
                        If etaHENConfig.IniReadValue("Settings", "testkit") = "0" Then
                            TestkitCheckBox.IsChecked = False
                        Else
                            TestkitCheckBox.IsChecked = True
                        End If
                    End If
                    If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "Allow_data_in_sandbox")) Then
                        If etaHENConfig.IniReadValue("Settings", "Allow_data_in_sandbox") = "0" Then
                            AllowDataInSandboxCheckBox.IsChecked = False
                        Else
                            AllowDataInSandboxCheckBox.IsChecked = True
                        End If
                    End If
                    If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "DPI")) Then
                        If etaHENConfig.IniReadValue("Settings", "DPI") = "0" Then
                            DPIServiceCheckBox.IsChecked = False
                        Else
                            DPIServiceCheckBox.IsChecked = True
                        End If
                    End If
                    If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "Klog")) Then
                        If etaHENConfig.IniReadValue("Settings", "Klog") = "0" Then
                            KernelLogCheckBox.IsChecked = False
                        Else
                            KernelLogCheckBox.IsChecked = True
                        End If
                    End If
                    If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "ALLOW_FTP_DEV_ACCESS")) Then
                        If etaHENConfig.IniReadValue("Settings", "ALLOW_FTP_DEV_ACCESS") = "0" Then
                            FTPDevAccessCheckBox.IsChecked = False
                        Else
                            FTPDevAccessCheckBox.IsChecked = True
                        End If
                    End If
                    If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "Util_rest_kill")) Then
                        If etaHENConfig.IniReadValue("Settings", "Util_rest_kill") = "0" Then
                            KillUtilDaemonCheckBox.IsChecked = False
                        Else
                            KillUtilDaemonCheckBox.IsChecked = True
                        End If
                    End If
                    If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "Game_rest_kill")) Then
                        If etaHENConfig.IniReadValue("Settings", "Game_rest_kill") = "0" Then
                            KillOpenGameCheckBox.IsChecked = False
                        Else
                            KillOpenGameCheckBox.IsChecked = True
                        End If
                    End If

                    If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "StartOption")) Then
                        Select Case etaHENConfig.IniReadValue("Settings", "StartOption")
                            Case "0"
                                StartupOptionComboBox.SelectedIndex = 0
                            Case "1"
                                StartupOptionComboBox.SelectedIndex = 1
                            Case "2"
                                StartupOptionComboBox.SelectedIndex = 2
                            Case "3"
                                StartupOptionComboBox.SelectedIndex = 3
                            Case "4"
                                StartupOptionComboBox.SelectedIndex = 4
                        End Select
                    End If
                    If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "Rest_Mode_Delay_Seconds")) Then
                        ShellUIPatchDelayTextBox.Text = etaHENConfig.IniReadValue("Settings", "Rest_Mode_Delay_Seconds")
                    End If

                End If

                MsgBox("Config file retrieved!" + vbCrLf + "You can change the checkboxes now and reupload the config.ini", MsgBoxStyle.Information)
            Catch ex As Exception
                MsgBox("Could not get the config.ini, please verify your connection.", MsgBoxStyle.Exclamation)
            End Try
        End If
    End Sub

    Private Sub SaveAndUploadButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveAndUploadButton.Click
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Try
                Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", 1337)
                    'Configurate the FTP connection
                    conn.Config.EncryptionMode = FtpEncryptionMode.None
                    conn.Config.SslProtocols = SslProtocols.None
                    conn.Config.DataConnectionEncryption = False

                    'Connect
                    conn.Connect()

                    'Upload config.ini
                    conn.UploadFile(Environment.CurrentDirectory + "\Cache\config.ini", "/data/etaHEN/config.ini", FtpRemoteExists.NoCheck)

                    'Disconnect
                    conn.Disconnect()
                End Using

                MsgBox("etaHEN config uploaded & updated on the PS5!", MsgBoxStyle.Information)
            Catch ex As Exception
                MsgBox("Could upload the config.ini, please verify your connection.", MsgBoxStyle.Exclamation)
            End Try
        Else
            MsgBox("No local config.ini file found.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub PS5etaHENConfigurator_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Load existing config
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            Dim ConfigInfo As New FileInfo(Environment.CurrentDirectory + "\Cache\config.ini")
            Dim ConfigDateTime As Date = ConfigInfo.LastWriteTime
            ConfigStatusTextBlock.Text = "etaHEN config found" + " - " + ConfigDateTime.ToString()

            If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "PS5Debug")) Then
                If etaHENConfig.IniReadValue("Settings", "PS5Debug") = "0" Then
                    PS5DebugAutoLoadCheckBox.IsChecked = False
                Else
                    PS5DebugAutoLoadCheckBox.IsChecked = True
                End If
            End If
            If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "FTP")) Then
                If etaHENConfig.IniReadValue("Settings", "FTP") = "0" Then
                    FTPCheckBox.IsChecked = False
                Else
                    FTPCheckBox.IsChecked = True
                End If
            End If
            If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "discord_rpc")) Then
                If etaHENConfig.IniReadValue("Settings", "discord_rpc") = "0" Then
                    DiscordCheckBox.IsChecked = False
                Else
                    DiscordCheckBox.IsChecked = True
                End If
            End If
            If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "testkit")) Then
                If etaHENConfig.IniReadValue("Settings", "testkit") = "0" Then
                    TestkitCheckBox.IsChecked = False
                Else
                    TestkitCheckBox.IsChecked = True
                End If
            End If
            If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "Allow_data_in_sandbox")) Then
                If etaHENConfig.IniReadValue("Settings", "Allow_data_in_sandbox") = "0" Then
                    AllowDataInSandboxCheckBox.IsChecked = False
                Else
                    AllowDataInSandboxCheckBox.IsChecked = True
                End If
            End If
            If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "DPI")) Then
                If etaHENConfig.IniReadValue("Settings", "DPI") = "0" Then
                    DPIServiceCheckBox.IsChecked = False
                Else
                    DPIServiceCheckBox.IsChecked = True
                End If
            End If
            If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "Klog")) Then
                If etaHENConfig.IniReadValue("Settings", "Klog") = "0" Then
                    KernelLogCheckBox.IsChecked = False
                Else
                    KernelLogCheckBox.IsChecked = True
                End If
            End If
            If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "ALLOW_FTP_DEV_ACCESS")) Then
                If etaHENConfig.IniReadValue("Settings", "ALLOW_FTP_DEV_ACCESS") = "0" Then
                    FTPDevAccessCheckBox.IsChecked = False
                Else
                    FTPDevAccessCheckBox.IsChecked = True
                End If
            End If
            If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "Util_rest_kill")) Then
                If etaHENConfig.IniReadValue("Settings", "Util_rest_kill") = "0" Then
                    KillUtilDaemonCheckBox.IsChecked = False
                Else
                    KillUtilDaemonCheckBox.IsChecked = True
                End If
            End If
            If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "Game_rest_kill")) Then
                If etaHENConfig.IniReadValue("Settings", "Game_rest_kill") = "0" Then
                    KillOpenGameCheckBox.IsChecked = False
                Else
                    KillOpenGameCheckBox.IsChecked = True
                End If
            End If

            If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "StartOption")) Then
                Select Case etaHENConfig.IniReadValue("Settings", "StartOption")
                    Case "0"
                        StartupOptionComboBox.SelectedIndex = 0
                    Case "1"
                        StartupOptionComboBox.SelectedIndex = 1
                    Case "2"
                        StartupOptionComboBox.SelectedIndex = 2
                    Case "3"
                        StartupOptionComboBox.SelectedIndex = 3
                    Case "4"
                        StartupOptionComboBox.SelectedIndex = 4
                End Select
            End If
            If Not String.IsNullOrEmpty(etaHENConfig.IniReadValue("Settings", "Rest_Mode_Delay_Seconds")) Then
                ShellUIPatchDelayTextBox.Text = etaHENConfig.IniReadValue("Settings", "Rest_Mode_Delay_Seconds")
            End If
        End If
    End Sub

#Region "Check changes"

    Private Sub DiscordCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles DiscordCheckBox.Checked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "discord_rpc", "1")
        End If
    End Sub

    Private Sub DiscordCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles DiscordCheckBox.Unchecked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "discord_rpc", "0")
        End If
    End Sub

    Private Sub FTPCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles FTPCheckBox.Checked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "FTP", "1")
        End If
    End Sub

    Private Sub FTPCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles FTPCheckBox.Unchecked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "FTP", "0")
        End If
    End Sub

    Private Sub TestkitCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles TestkitCheckBox.Checked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "testkit", "1")
        End If
    End Sub

    Private Sub TestkitCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles TestkitCheckBox.Unchecked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "testkit", "0")
        End If
    End Sub

    Private Sub PS5DebugAutoLoadCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles PS5DebugAutoLoadCheckBox.Checked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "PS5Debug", "1")
        End If
    End Sub

    Private Sub PS5DebugAutoLoadCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles PS5DebugAutoLoadCheckBox.Unchecked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "PS5Debug", "0")
        End If
    End Sub

    Private Sub AllowDataInSandboxCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles AllowDataInSandboxCheckBox.Checked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "Allow_data_in_sandbox", "1")
        End If
    End Sub

    Private Sub AllowDataInSandboxCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles AllowDataInSandboxCheckBox.Unchecked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "Allow_data_in_sandbox", "0")
        End If
    End Sub

    Private Sub DPIServiceCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles DPIServiceCheckBox.Checked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "DPI", "1")
        End If
    End Sub

    Private Sub DPIServiceCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles DPIServiceCheckBox.Unchecked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "DPI", "0")
        End If
    End Sub

    Private Sub KernelLogCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles KernelLogCheckBox.Checked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "Klog", "1")
        End If
    End Sub

    Private Sub KernelLogCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles KernelLogCheckBox.Unchecked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "Klog", "0")
        End If
    End Sub

    Private Sub KillUtilDaemonCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles KillUtilDaemonCheckBox.Checked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "Util_rest_kill", "1")
        End If
    End Sub

    Private Sub KillUtilDaemonCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles KillUtilDaemonCheckBox.Unchecked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "Util_rest_kill", "0")
        End If
    End Sub

    Private Sub KillOpenGameCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles KillOpenGameCheckBox.Checked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "Game_rest_kill", "1")
        End If
    End Sub

    Private Sub KillOpenGameCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles KillOpenGameCheckBox.Unchecked
        If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
            Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
            etaHENConfig.IniWriteValue("Settings", "Game_rest_kill", "0")
        End If
    End Sub

    Private Sub StartupOptionComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles StartupOptionComboBox.SelectionChanged
        If e.AddedItems IsNot Nothing Then
            Dim SelectedCBItem As ComboBoxItem = CType(e.AddedItems(0), ComboBoxItem)
            Select Case SelectedCBItem.Content.ToString()
                Case "None"
                    If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
                        Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
                        etaHENConfig.IniWriteValue("Settings", "StartOption", "0")
                    End If
                Case "Home Menu"
                    If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
                        Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
                        etaHENConfig.IniWriteValue("Settings", "StartOption", "1")
                    End If
                Case "Settings"
                    If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
                        Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
                        etaHENConfig.IniWriteValue("Settings", "StartOption", "2")
                    End If
                Case "Toolbox"
                    If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
                        Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
                        etaHENConfig.IniWriteValue("Settings", "StartOption", "3")
                    End If
                Case "Itemzflow"
                    If File.Exists(Environment.CurrentDirectory + "\Cache\config.ini") Then
                        Dim etaHENConfig As New IniFile(Environment.CurrentDirectory + "\Cache\config.ini")
                        etaHENConfig.IniWriteValue("Settings", "StartOption", "4")
                    End If
            End Select
        End If
    End Sub

#End Region

End Class
