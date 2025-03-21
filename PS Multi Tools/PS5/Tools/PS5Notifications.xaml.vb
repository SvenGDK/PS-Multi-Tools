Imports System.Data
Imports System.IO
Imports System.Security.Authentication
Imports FluentFTP
Imports Microsoft.Data.Sqlite
Imports Newtonsoft.Json

Public Class PS5Notifications

    Dim NotificationsCount As Integer = 0
    Dim UserProfiles As New List(Of String)()
    Dim SelectedUserProfile As String = ""

    Dim OtherFW As Boolean = False 'title_icon seems not to be presented in all fws
    Dim InstallOnAllProfiles As Boolean = False

    'Shortcuts that will be available
    Dim WebBrowserJSONActions As New List(Of Action)()

    Private Sub ConnectButton_Click(sender As Object, e As RoutedEventArgs) Handles ConnectButton.Click
        If Not Directory.Exists(Environment.CurrentDirectory + "\Cache") Then Directory.CreateDirectory(Environment.CurrentDirectory + "\Cache")

        UserProfilesComboBox.Items.Clear()

        If Not String.IsNullOrEmpty(IPTextBox.Text) Then
            Try
                Using conn As New FtpClient(IPTextBox.Text, "anonymous", "anonymous", 1337)
                    'Configurate the FTP connection
                    conn.Config.EncryptionMode = FtpEncryptionMode.None
                    conn.Config.SslProtocols = SslProtocols.None
                    conn.Config.DataConnectionEncryption = False

                    'Connect
                    conn.Connect()

                    'Get notification2.db
                    conn.DownloadFile(Environment.CurrentDirectory + "\Cache\notification2.db", "/system_data/priv/mms/notification2.db", FtpLocalExists.Overwrite)

                    'Also get notification.db in case we can't find any user_id
                    conn.DownloadFile(Environment.CurrentDirectory + "\Cache\notification.db", "/system_data/priv/mms/notification.db", FtpLocalExists.Overwrite)

                    'Disconnect
                    conn.Disconnect()
                End Using
            Catch ex As Exception
                MsgBox("Could not get the notification2.db, please verify your connection.", MsgBoxStyle.Exclamation)
            End Try

            'Create a backup that will not be overwritten
            If File.Exists(Environment.CurrentDirectory + "\Cache\notification2.db") Then
                If Not File.Exists(Environment.CurrentDirectory + "\Cache\notification2-backup.db") Then
                    File.Copy(Environment.CurrentDirectory + "\Cache\notification2.db", Environment.CurrentDirectory + "\Cache\notification2-backup.db", False)
                End If
            End If

            'Load values
            Try
                Using conn As New SqliteConnection("Data Source=" + Environment.CurrentDirectory + "\Cache\notification2.db")
                    conn.Open()

                    Dim SelectCommand = conn.CreateCommand()
                    SelectCommand.CommandText = "SELECT DISTINCT user_id FROM notification" 'No duplicates

                    'Create a new DataTable and load the values into it
                    Using DataReader = SelectCommand.ExecuteReader()
                        Dim NewDataTable As New DataTable()
                        NewDataTable.Load(DataReader)
                        If Not NewDataTable.Rows.Count = 0 Then
                            'Add each user profile to the UserProfiles list
                            For Each Record As DataRow In NewDataTable.Rows
                                UserProfiles.Add(Record(0).ToString)
                            Next
                        End If
                    End Using

                    'Get the last notification ID so we can add more
                    SelectCommand.CommandText = "SELECT id FROM notification ORDER BY id DESC LIMIT 1;" 'Get the last notification ID
                    Using DataReader = SelectCommand.ExecuteReader()
                        Dim NewDataTable As New DataTable()
                        NewDataTable.Load(DataReader)
                        If Not NewDataTable.Rows.Count = 0 Then
                            NotificationsCount = CInt(NewDataTable.Rows(0).Item(0).ToString)
                        End If
                    End Using

                    'Check if the table 'notification' contains the column 'title_icon' (Seems to be new on not presented on all fws)
                    SelectCommand.CommandText = String.Format("PRAGMA table_info({0})", "notification")
                    Dim NewDataReader = SelectCommand.ExecuteReader()
                    Dim NameIndex As Integer = NewDataReader.GetOrdinal("Name")

                    While NewDataReader.Read()
                        If NewDataReader.GetString(NameIndex).Equals("title_icon") Then
                            OtherFW = True
                        Else
                            OtherFW = False
                        End If
                    End While
                    NewDataReader.Close()

                    'Close & dispose
                    conn.Close()
                    conn.Dispose()
                    SelectCommand.Dispose()
                End Using

                'Try to get user profiles from notification.db if we can't find them inside notification2.db
                If UserProfiles.Count = 0 Then
                    Using conn As New SQLiteConnection("Data Source=" + Environment.CurrentDirectory + "\Cache\notification.db")
                        conn.Open()

                        Dim SelectCommand = conn.CreateCommand()
                        SelectCommand.CommandText = "SELECT DISTINCT user_id FROM notification" 'No duplicates

                        Using DataReader = SelectCommand.ExecuteReader()
                            Dim NewDataTable As New DataTable()
                            NewDataTable.Load(DataReader)
                            If Not NewDataTable.Rows.Count = 0 Then
                                'Add each user profile to the UserProfiles list
                                For Each Record As DataRow In NewDataTable.Rows
                                    UserProfiles.Add(Record(0).ToString)
                                Next
                            End If
                        End Using

                        'Close & dispose
                        conn.Close()
                        conn.Dispose()
                        SelectCommand.Dispose()
                    End Using
                End If

                'Completely close the SQLiteConnection and release access to notification2.db
                SQLiteConnection.ClearAllPools()
                GC.Collect()
                GC.WaitForPendingFinalizers()

                MsgBox("Users and notifications loaded successfully." + vbCrLf + "Remember to upload the changes after you're done.", MsgBoxStyle.Information, "Success")
            Catch ex As SQLiteException
                MsgBox(ex.Message)
                MsgBox("Could not read notification2.db, please verify your connection.", MsgBoxStyle.Exclamation)
            End Try

            'Add profiles to the UserProfilesComboBox
            For Each UserProfile In UserProfiles
                UserProfilesComboBox.Items.Add(UserProfile)
            Next

            UploadButton.IsEnabled = True
        End If

    End Sub

    Private Sub AllUserProfilesCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles AllUserProfilesCheckBox.Checked
        InstallOnAllProfiles = True
        UserProfilesComboBox.IsEnabled = False
    End Sub

    Private Sub AllUserProfilesCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles AllUserProfilesCheckBox.Unchecked
        InstallOnAllProfiles = False
        UserProfilesComboBox.IsEnabled = True
    End Sub

    Private Sub AddDebugSettingsButton_Click(sender As Object, e As RoutedEventArgs) Handles AddDebugSettingsButton.Click

        'Add to the notification2.db
        If File.Exists(Environment.CurrentDirectory + "\Cache\notification2.db") Then
            Try
                Using conn As New SQLiteConnection("Data Source=" + Environment.CurrentDirectory + "\Cache\notification2.db")
                    conn.Open()

                    'Insert required values
                    Dim SelectCommand = conn.CreateCommand()
                    Dim TableRowsAffected As Integer = 0
                    Dim AffectedRows As Integer = 0

                    If InstallOnAllProfiles = True Then

                        For Each UserProfile In UserProfiles

                            If OtherFW = True Then
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""pssettings:play?function=debug_settings""}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""★Debug Settings""}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL,'')"
                            Else
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""pssettings:play?function=debug_settings""}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""★Debug Settings""}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL)"
                            End If

                            TableRowsAffected = SelectCommand.ExecuteNonQuery()

                            If TableRowsAffected = 1 Then
                                NotificationsCount += 1
                                AffectedRows += 1
                            End If
                        Next

                        If AffectedRows = UserProfiles.Count Then
                            MsgBox("The Debug Menu has been added to the notifications for all users." + vbCrLf + "Remember to upload the changes after you're done.", MsgBoxStyle.Information, "Success")
                        Else
                            MsgBox("Could not add the Debug Menu for every user.", MsgBoxStyle.Exclamation, "Error")
                        End If
                    Else

                        If OtherFW = True Then
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""pssettings:play?function=debug_settings""}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""★Debug Settings""}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL,'')"
                        Else
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""pssettings:play?function=debug_settings""}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""★Debug Settings""}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL)"
                        End If

                        TableRowsAffected = SelectCommand.ExecuteNonQuery()

                        If TableRowsAffected = 1 Then
                            NotificationsCount += 1
                            MsgBox("The Debug Menu has been added to the notifications." + vbCrLf + "Remember to upload the changes after you're done.", MsgBoxStyle.Information, "Success")
                        Else
                            MsgBox("Could not add the Debug Menu.", MsgBoxStyle.Exclamation, "Error")
                        End If
                    End If

                    'Close & dispose
                    conn.Close()
                    conn.Dispose()
                    SelectCommand.Dispose()
                End Using

                'Completely close the SQLiteConnection and release access to notification2.db
                SQLiteConnection.ClearAllPools()
                GC.Collect()
                GC.WaitForPendingFinalizers()

            Catch ex As SQLiteException
                MsgBox(ex.Message)
                MsgBox("Could not read notification2.db, please verify your connection.", MsgBoxStyle.Exclamation)
            End Try
        End If

    End Sub

    Private Sub AddWebBrowserButton_Click(sender As Object, e As RoutedEventArgs) Handles AddWebBrowserButton.Click

        'Create the notification rawData JSON
        Dim NewPS5Notification As New PS5Notification() With {
            .bundleName = "download",
            .channelType = "Downloads",
            .isAnonymous = True,
            .isImmediate = True,
            .platformViews = New PlatformViews() With {.previewDisabled = New PreviewDisabled() With {.viewData = New ViewData() With {.icon = New Icon() With {.parameters = New Parameters() With {.icon = "download"}, .type = ""}, .message = New Message() With {.body = ""}}}},
            .priority = 1,
            .toastOverwriteType = "No",
            .useCaseId = "NUC249",
            .viewData = New ViewData() With {.actions = WebBrowserJSONActions, .icon = New Icon() With {.parameters = New Parameters() With {.icon = "localasset_system_software_default"}, .type = "Predefined"}, .message = New Message() With {.body = ""}, .subMessage = New SubMessage() With {.body = "Web browser"}},
            .viewTemplateType = "InteractiveToastTemplateB"
        }

        'Serialize PS5Notification to JSON
        Dim rawDataJSON As String = JsonConvert.SerializeObject(NewPS5Notification)

        'Add to the notification2.db
        If File.Exists(Environment.CurrentDirectory + "\Cache\notification2.db") Then
            Try
                Using conn As New SQLiteConnection("Data Source=" + Environment.CurrentDirectory + "\Cache\notification2.db")
                    conn.Open()

                    'Insert required values
                    Dim SelectCommand = conn.CreateCommand()
                    Dim TableRowsAffected As Integer = 0
                    Dim AffectedRows As Integer = 0

                    If InstallOnAllProfiles = True Then

                        For Each UserProfile In UserProfiles

                            If OtherFW = True Then
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + UserProfile + ",0,1,NULL,NULL,'2871025089','','xxx','','','','" + rawDataJSON + "',NULL,'{""titleInfos"":[{""icon"":"""",""name"":"""",""titleKey"":""{\""objectType\"":\""TitleByTitle\"",\""titleIds\"":[\""\""]}""}],""userInfos"":[],""completed"":true,""updatedDateTime"":""""}',NULL,NULL)"
                            Else
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + UserProfile + ",0,1,NULL,NULL,'2871025089','','xxx','','','','" + rawDataJSON + "',NULL,'{""titleInfos"":[{""icon"":"""",""name"":"""",""titleKey"":""{\""objectType\"":\""TitleByTitle\"",\""titleIds\"":[\""\""]}""}],""userInfos"":[],""completed"":true,""updatedDateTime"":""""}',NULL)"
                            End If

                            MsgBox(SelectCommand.CommandText)

                            TableRowsAffected = SelectCommand.ExecuteNonQuery()

                            If TableRowsAffected = 1 Then
                                NotificationsCount += 1
                                AffectedRows += 1
                            End If
                        Next

                        If AffectedRows = UserProfiles.Count Then
                            MsgBox("The Web Browser has been added to the notifications for all users." + vbCrLf + "Remember to upload the changes after you're done.", MsgBoxStyle.Information, "Success")
                        Else
                            MsgBox("Could not add the Web Browser for every user.", MsgBoxStyle.Exclamation, "Error")
                        End If

                    Else

                        If OtherFW = True Then
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + SelectedUserProfile + ",0,1,NULL,NULL,'2871025089','','xxx','','','','" + rawDataJSON + "',NULL,'{""titleInfos"":[{""icon"":"""",""name"":"""",""titleKey"":""{\""objectType\"":\""TitleByTitle\"",\""titleIds\"":[\""\""]}""}],""userInfos"":[],""completed"":true,""updatedDateTime"":""""}',NULL,NULL)"
                        Else
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + SelectedUserProfile + ",0,1,NULL,NULL,'2871025089','','xxx','','','','" + rawDataJSON + "',NULL,'{""titleInfos"":[{""icon"":"""",""name"":"""",""titleKey"":""{\""objectType\"":\""TitleByTitle\"",\""titleIds\"":[\""\""]}""}],""userInfos"":[],""completed"":true,""updatedDateTime"":""""}',NULL)"
                        End If

                        TableRowsAffected = SelectCommand.ExecuteNonQuery()

                        If TableRowsAffected = 1 Then
                            MsgBox("The Web Browser has been added to the notifications." + vbCrLf + "Remember to upload the changes after you're done.", MsgBoxStyle.Information, "Success")
                        Else
                            MsgBox("Could not add the Web Browser.", MsgBoxStyle.Exclamation, "Error")
                        End If
                    End If

                    'Close & dispose
                    conn.Close()
                    conn.Dispose()
                    SelectCommand.Dispose()
                End Using

                'Completely close the SQLiteConnection and release access to notification2.db
                SQLiteConnection.ClearAllPools()
                GC.Collect()
                GC.WaitForPendingFinalizers()

            Catch ex As SQLiteException
                MsgBox(ex.Message)
                MsgBox("Could not read notification2.db, please verify your connection.", MsgBoxStyle.Exclamation)
            End Try
        End If

    End Sub

    Private Sub AddSaveDataManagerButton_Click(sender As Object, e As RoutedEventArgs) Handles AddSaveDataManagerButton.Click

        Dim CustomActionURL As String = "pssettings:play?function=savedata"
        Dim CustomActionName As String = "★Saved Data Management"

        'Add to the notification2.db
        If File.Exists(Environment.CurrentDirectory + "\Cache\notification2.db") Then
            Try
                Using conn As New SQLiteConnection("Data Source=" + Environment.CurrentDirectory + "\Cache\notification2.db")
                    conn.Open()

                    'Insert required values
                    Dim SelectCommand = conn.CreateCommand()
                    Dim TableRowsAffected As Integer = 0
                    Dim AffectedRows As Integer = 0

                    If InstallOnAllProfiles = True Then

                        For Each UserProfile In UserProfiles

                            If OtherFW = True Then
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""" + CustomActionURL + """}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""" + CustomActionName + """}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL,'')"
                            Else
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""" + CustomActionURL + """}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""" + CustomActionName + """}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL)"
                            End If

                            TableRowsAffected = SelectCommand.ExecuteNonQuery()
                            If TableRowsAffected = 1 Then
                                NotificationsCount += 1
                                AffectedRows += 1
                            End If
                        Next

                        If AffectedRows = UserProfiles.Count Then
                            MsgBox("The custom action " + CustomActionName + " has been added to the notifications for all users." + vbCrLf + "Remember to upload the changes after you're done.", MsgBoxStyle.Information, "Success")
                        Else
                            MsgBox("Could not add the custom action " + CustomActionName + " for every user.", MsgBoxStyle.Exclamation, "Error")
                        End If
                    Else

                        If OtherFW = True Then
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""" + CustomActionURL + """}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""" + CustomActionName + """}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL,'')"
                        Else
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""" + CustomActionURL + """}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""" + CustomActionName + """}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL)"
                        End If

                        TableRowsAffected = SelectCommand.ExecuteNonQuery()

                        If TableRowsAffected = 1 Then
                            NotificationsCount += 1
                            MsgBox("The custom action " + CustomActionName + " has been added to the notifications." + vbCrLf + "Remember to upload the changes after you're done.", MsgBoxStyle.Information, "Success")
                        Else
                            MsgBox("Could not add the custom action " + CustomActionName + ".", MsgBoxStyle.Exclamation, "Error")
                        End If
                    End If

                    'Close & dispose
                    conn.Close()
                    conn.Dispose()
                    SelectCommand.Dispose()
                End Using

                'Completely close the SQLiteConnection and release access to notification2.db
                SQLiteConnection.ClearAllPools()
                GC.Collect()
                GC.WaitForPendingFinalizers()

            Catch ex As SQLiteException
                MsgBox(ex.Message)
                MsgBox("Could not read notification2.db, please verify your connection.", MsgBoxStyle.Exclamation)
            End Try
        End If

    End Sub

    Private Sub AddSaveDataManagerPS4Button_Click(sender As Object, e As RoutedEventArgs) Handles AddSaveDataManagerPS4Button.Click

        Dim CustomActionURL As String = "pssettings:play?function=savedata_ps4"
        Dim CustomActionName As String = "★Saved Data Management PS4"

        'Add to the notification2.db
        If File.Exists(Environment.CurrentDirectory + "\Cache\notification2.db") Then
            Try
                Using conn As New SQLiteConnection("Data Source=" + Environment.CurrentDirectory + "\Cache\notification2.db")
                    conn.Open()

                    'Insert required values
                    Dim SelectCommand = conn.CreateCommand()
                    Dim TableRowsAffected As Integer = 0
                    Dim AffectedRows As Integer = 0

                    If InstallOnAllProfiles = True Then

                        For Each UserProfile In UserProfiles

                            If OtherFW = True Then
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""" + CustomActionURL + """}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""" + CustomActionName + """}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL,'')"
                            Else
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""" + CustomActionURL + """}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""" + CustomActionName + """}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL)"
                            End If

                            TableRowsAffected = SelectCommand.ExecuteNonQuery()
                            If TableRowsAffected = 1 Then
                                NotificationsCount += 1
                                AffectedRows += 1
                            End If
                        Next

                        If AffectedRows = UserProfiles.Count Then
                            MsgBox("The custom action " + CustomActionName + " has been added to the notifications for all users." + vbCrLf + "Remember to upload the changes after you're done.", MsgBoxStyle.Information, "Success")
                        Else
                            MsgBox("Could not add the custom action " + CustomActionName + " for every user.", MsgBoxStyle.Exclamation, "Error")
                        End If
                    Else

                        If OtherFW = True Then
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""" + CustomActionURL + """}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""" + CustomActionName + """}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL,'')"
                        Else
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""" + CustomActionURL + """}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""" + CustomActionName + """}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL)"
                        End If

                        TableRowsAffected = SelectCommand.ExecuteNonQuery()

                        If TableRowsAffected = 1 Then
                            NotificationsCount += 1
                            MsgBox("The custom action " + CustomActionName + " has been added to the notifications." + vbCrLf + "Remember to upload the changes after you're done.", MsgBoxStyle.Information, "Success")
                        Else
                            MsgBox("Could not add the custom action " + CustomActionName + ".", MsgBoxStyle.Exclamation, "Error")
                        End If
                    End If

                    'Close & dispose
                    conn.Close()
                    conn.Dispose()
                    SelectCommand.Dispose()
                End Using

                'Completely close the SQLiteConnection and release access to notification2.db
                SQLiteConnection.ClearAllPools()
                GC.Collect()
                GC.WaitForPendingFinalizers()

            Catch ex As SQLiteException
                MsgBox(ex.Message)
                MsgBox("Could not read notification2.db, please verify your connection.", MsgBoxStyle.Exclamation)
            End Try
        End If

    End Sub

    Private Sub UserProfilesComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles UserProfilesComboBox.SelectionChanged
        If UserProfilesComboBox.SelectedItem IsNot Nothing Then
            SelectedUserProfile = UserProfilesComboBox.SelectedItem.ToString
        End If
    End Sub

    Private Sub OtherActionsComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles OtherActionsComboBox.SelectionChanged
        If OtherActionsComboBox.SelectedItem IsNot Nothing Then
            Dim SelectedComboBoxItem As ComboBoxItem = CType(OtherActionsComboBox.SelectedItem, ComboBoxItem)
            CustomDefinedActionTextBox.Text = SelectedComboBoxItem.Content.ToString()
        End If
    End Sub

    Private Sub PS5Notifications_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        'Add pre-defined bookmarks to the list
        WebBrowserJSONActions.Add(New Action() With {.actionName = "PS Multi Tools Host",
                        .actionType = "DeepLink",
                        .defaultFocus = True,
                        .parameters = New Parameters() With {.actionUrl = "http://X.X.X.X/ps5ex/"}})

        WebBrowserJSONActions.Add(New Action() With {.actionName = "Al-Azif",
                        .actionType = "DeepLink",
                        .defaultFocus = True,
                        .parameters = New Parameters() With {.actionUrl = "https://ithaqua.exploit.menu/"}})

        WebBrowserJSONActions.Add(New Action() With {.actionName = "@SvenGDK",
                        .actionType = "DeepLink",
                        .defaultFocus = True,
                        .parameters = New Parameters() With {.actionUrl = "https://twitter.com/SvenGDK"}})

        For Each WebBrowserAction In WebBrowserJSONActions
            WebBrowserShorcutsComboBox.Items.Add(WebBrowserAction)
        Next

        'Show the action name
        WebBrowserShorcutsComboBox.DisplayMemberPath = "actionName"
        WebBrowserShorcutsComboBox.SelectedIndex = 0

    End Sub

    Private Sub RemoveShortcutButton_Click(sender As Object, e As RoutedEventArgs) Handles RemoveShortcutButton.Click
        If WebBrowserShorcutsComboBox.SelectedItem IsNot Nothing Then
            WebBrowserJSONActions.Remove(CType(WebBrowserShorcutsComboBox.SelectedItem, Action))
            WebBrowserShorcutsComboBox.Items.Remove(WebBrowserShorcutsComboBox.SelectedItem)
        End If
    End Sub

    Private Sub AddShortcutButton_Click(sender As Object, e As RoutedEventArgs) Handles AddShortcutButton.Click
        If Not String.IsNullOrEmpty(ShortcutNameTextBox.Text) And Not String.IsNullOrEmpty(ShortcutLinkTextBox.Text) Then
            WebBrowserJSONActions.Add(New Action() With {.actionName = ShortcutNameTextBox.Text,
                        .actionType = "DeepLink",
                        .defaultFocus = True,
                        .parameters = New Parameters() With {.actionUrl = ShortcutLinkTextBox.Text}})
            MsgBox("New shortcut for the Web Browser has been added." + vbCrLf + "Remember to upload the changes after you're done.", MsgBoxStyle.Information)
        Else
            MsgBox("Please fill in the required informations.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub AddCustomActionButton_Click(sender As Object, e As RoutedEventArgs) Handles AddCustomActionButton.Click

        Dim CustomActionURL As String = CustomDefinedActionTextBox.Text
        Dim CustomActionName As String = CustomActionNameTextBox.Text

        'Add to the notification2.db
        If File.Exists(Environment.CurrentDirectory + "\Cache\notification2.db") Then
            Try
                Using conn As New SQLiteConnection("Data Source=" + Environment.CurrentDirectory + "\Cache\notification2.db")
                    conn.Open()

                    'Insert required values
                    Dim SelectCommand = conn.CreateCommand()
                    Dim TableRowsAffected As Integer = 0
                    Dim AffectedRows As Integer = 0

                    If InstallOnAllProfiles = True Then

                        For Each UserProfile In UserProfiles

                            If OtherFW = True Then
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""" + CustomActionURL + """}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""" + CustomActionName + """}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL,'')"
                            Else
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""" + CustomActionURL + """}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""" + CustomActionName + """}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL)"
                            End If

                            TableRowsAffected = SelectCommand.ExecuteNonQuery()
                            If TableRowsAffected = 1 Then
                                NotificationsCount += 1
                                AffectedRows += 1
                            End If
                        Next

                        If AffectedRows = UserProfiles.Count Then
                            MsgBox("The custom action " + CustomActionName + " has been added to the notifications for all users." + vbCrLf + "Remember to upload the changes after you're done.", MsgBoxStyle.Information, "Success")
                        Else
                            MsgBox("Could not add the custom action " + CustomActionName + " for every user.", MsgBoxStyle.Exclamation, "Error")
                        End If
                    Else

                        If OtherFW = True Then
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""" + CustomActionURL + """}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""" + CustomActionName + """}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL,'')"
                        Else
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{""bundleName"":"""",""channelType"":""Downloads"",""isAnonymous"":true,""isImmediate"":false,""platformViews"":{""previewDisabled"":{""viewData"":{""icon"":{""parameters"":{""icon"":""download""},""type"":""Predefined""},""message"":{""body"":""""}}}},""priority"":1,""toastOverwriteType"":""No"",""useCaseId"":""NUC249"",""viewData"":{""actions"":[{""actionName"":""Enter Debug Menu"",""actionType"":""DeepLink"",""defaultFocus"":true,""parameters"":{""actionUrl"":""" + CustomActionURL + """}}],""icon"":{""parameters"":{""icon"":""localasset_system_software_default""},""type"":""Predefined""},""message"":{""body"":""""},""subMessage"":{""body"":""" + CustomActionName + """}},""viewTemplateType"":""InteractiveToastTemplateB""}',NULL,'{""titleInfos"":[],""userInfos"":[],""completed"":true,""updatedDateTime"":""2022-10-08T17:16:03.512Z""}',NULL)"
                        End If

                        TableRowsAffected = SelectCommand.ExecuteNonQuery()

                        If TableRowsAffected = 1 Then
                            NotificationsCount += 1
                            MsgBox("The custom action " + CustomActionName + " has been added to the notifications." + vbCrLf + "Remember to upload the changes after you're done.", MsgBoxStyle.Information, "Success")
                        Else
                            MsgBox("Could not add the custom action " + CustomActionName + ".", MsgBoxStyle.Exclamation, "Error")
                        End If
                    End If

                    'Close & dispose
                    conn.Close()
                    conn.Dispose()
                    SelectCommand.Dispose()
                End Using

                'Completely close the SQLiteConnection and release access to notification2.db
                SQLiteConnection.ClearAllPools()
                GC.Collect()
                GC.WaitForPendingFinalizers()

            Catch ex As SQLiteException
                MsgBox(ex.Message)
                MsgBox("Could not read notification2.db, please verify your connection.", MsgBoxStyle.Exclamation)
            End Try
        End If

    End Sub

    Private Sub UploadButton_Click(sender As Object, e As RoutedEventArgs) Handles UploadButton.Click
        Try
            Using conn As New FtpClient(IPTextBox.Text, "anonymous", "anonymous", 1337)
                'Configurate the FTP connection
                conn.Config.EncryptionMode = FtpEncryptionMode.None
                conn.Config.SslProtocols = SslProtocols.None
                conn.Config.DataConnectionEncryption = False

                'Connect
                conn.Connect()

                'Remove the old notification2.db (overwrite is not implemented yet) to upload the updated one
                conn.DeleteFile("/system_data/priv/mms/notification2.db")

                'Upload updated notification2.db
                Dim NewFTPStatus As FtpStatus = conn.UploadFile(Environment.CurrentDirectory + "\Cache\notification2.db", "/system_data/priv/mms/notification2.db", FtpRemoteExists.NoCheck)

                If NewFTPStatus = FtpStatus.Success Then
                    MsgBox("Notifications on the console have been updated." + vbCrLf + "Check your notifications on the PS5.", MsgBoxStyle.Information)
                ElseIf NewFTPStatus = FtpStatus.Failed Then
                    MsgBox("Could not update the notifications on the console." + vbCrLf + "Please verify your connection.", MsgBoxStyle.Information)
                End If

                'Disconnect
                conn.Disconnect()
            End Using
        Catch ex As Exception
            MsgBox("Could not upload the notification2.db, please verify your connection.", MsgBoxStyle.Exclamation)
        End Try
    End Sub

End Class
