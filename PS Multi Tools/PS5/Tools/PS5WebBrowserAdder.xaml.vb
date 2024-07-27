Imports System.Data.SQLite
Imports System.IO
Imports System.Security.Authentication
Imports FluentFTP
Imports WinSCP

Public Class PS5WebBrowserAdder

    Public ConsoleIP As String
    Dim DBTables As New List(Of String)()
    Dim LowFW As Boolean = False
    Dim RWAccess As Boolean = False

    Private Function FTPS5_MountRW() As Boolean
        Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", 1337)
            'Configurate the FTP connection
            conn.Config.EncryptionMode = FtpEncryptionMode.None
            conn.Config.SslProtocols = SslProtocols.None
            conn.Config.DataConnectionEncryption = False

            'Connect
            conn.Connect()

            'Mount /system & /system_ex with RW permission
            Dim RequestRWReply As FtpReply = Nothing
            RequestRWReply = conn.Execute("MTRW")

            'Disconnect
            conn.Disconnect()

            If RequestRWReply.Success() = True Then
                Return True
            Else
                Return False
            End If
        End Using
    End Function

    Public Function FilesAvailable(ConsoleIP As String) As Boolean
        Try
            Dim sessionOptions As New SessionOptions
            With sessionOptions
                .Protocol = Protocol.Ftp
                .HostName = ConsoleIP
                .UserName = "anonymous"
                .Password = "anonymous"
                .PortNumber = 1337
            End With

            Dim NewSession As New Session()

            'Try to connect
            NewSession.Open(sessionOptions)

            Dim TotalDirs As Integer = 0

            'Enumerate a folder to check if we have access
            For Each FileInFTP In NewSession.EnumerateRemoteFiles("/dev", "", EnumerationOptions.AllDirectories)
                TotalDirs += 1
            Next

            If TotalDirs > 0 Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            MsgBox("Could not connect, please verify your connection.", MsgBoxStyle.Exclamation)
            Return False
        End Try
    End Function

    Public Function GetDatabaseOverFTP() As Boolean
        If Not Directory.Exists(My.Computer.FileSystem.CurrentDirectory + "\Cache") Then Directory.CreateDirectory(My.Computer.FileSystem.CurrentDirectory + "\Cache")

        Try
            Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", 1337)
                'Configurate the FTP connection
                conn.Config.EncryptionMode = FtpEncryptionMode.None
                conn.Config.SslProtocols = SslProtocols.None
                conn.Config.DataConnectionEncryption = False

                'Connect
                conn.Connect()

                'Get app.db
                conn.DownloadFile(My.Computer.FileSystem.CurrentDirectory + "\Cache\app.db", "/system_data/priv/mms/app.db", FtpLocalExists.Overwrite)

                'Disconnect
                conn.Disconnect()
            End Using
        Catch ex As Exception
            MsgBox("Could not get the app.db, please verify your connection.", MsgBoxStyle.Exclamation)
            Return False
        End Try

        Try
            Dim RecordsCount As Integer = 0

            Using conn As New SQLiteConnection("Data Source=" + My.Computer.FileSystem.CurrentDirectory + "\Cache\app.db")
                conn.Open()

                Dim SelectCommand = conn.CreateCommand()
                SelectCommand.CommandText = "select name from sqlite_master where type='table' order by name"

                Dim DataReader As SQLiteDataReader = SelectCommand.ExecuteReader()

                'Get all tables from each profile
                While DataReader.Read
                    If DataReader("name").ToString.StartsWith("tbl_addon") Then
                        DBTables.Add(DataReader("name").ToString)
                    ElseIf DataReader("name").ToString.StartsWith("tbl_concepticoninfo") Then
                        DBTables.Add(DataReader("name").ToString)
                    ElseIf DataReader("name").ToString.StartsWith("tbl_iconinfo") Then
                        DBTables.Add(DataReader("name").ToString)
                    ElseIf DataReader("name").ToString.StartsWith("tbl_info") Then
                        DBTables.Add(DataReader("name").ToString)
                    End If
                End While

                DataReader.Close()

                'Check if the tbl_concepticoninfo contains column 'genreFlags' (Seems to be new on 4.00+)
                For Each Table In DBTables
                    If Table.StartsWith("tbl_concepticoninfo") Then
                        SelectCommand.CommandText = String.Format("PRAGMA table_info({0})", Table)

                        Dim reader = SelectCommand.ExecuteReader()
                        Dim nameIndex As Integer = reader.GetOrdinal("Name")

                        While reader.Read()
                            If reader.GetString(nameIndex).Equals("genreFlags") Then
                                LowFW = False
                            Else
                                LowFW = True
                            End If
                        End While

                        reader.Close()
                        Exit For 'We only need to check once
                    End If
                Next

                'Close & dispose
                conn.Close()
                conn.Dispose()
                SelectCommand.Dispose()
            End Using

            'Completely close the SQLiteConnection and release access to app.db
            SQLiteConnection.ClearAllPools()
            GC.Collect()
            GC.WaitForPendingFinalizers()

            If DBTables.Count = 0 Then
                Return False
            Else
                Return True
            End If
        Catch ex As SQLiteException
            MsgBox(ex.Message)
            MsgBox("Could not read app.db, please verify your connection.", MsgBoxStyle.Exclamation)
            Return False
        End Try
    End Function

    Public Function SendDatabaseOverFTP() As Boolean
        Try
            Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", 1337)
                'Configurate the FTP connection
                conn.Config.EncryptionMode = FtpEncryptionMode.None
                conn.Config.SslProtocols = SslProtocols.None
                conn.Config.DataConnectionEncryption = False

                'Connect
                conn.Connect()

                'Remove the old app.db (overwrite is not implemented yet) to upload the updated one
                conn.DeleteFile("/system_data/priv/mms/app.db")

                'Send the app.db back
                conn.UploadFile(My.Computer.FileSystem.CurrentDirectory + "\Cache\app.db", "/system_data/priv/mms/app.db", FtpRemoteExists.NoCheck)

                'Send the data/app folder
                conn.UploadFile(My.Computer.FileSystem.CurrentDirectory + "\Tools\NPXS20102\icon0.png", "/data/apps/NPXS20102/icon0.png", FtpRemoteExists.NoCheck, True)
                conn.UploadFile(My.Computer.FileSystem.CurrentDirectory + "\Tools\NPXS20102\param.json", "/data/apps/NPXS20102/param.json", FtpRemoteExists.NoCheck, True)

                'Disconnect
                conn.Disconnect()
                Return True
            End Using
        Catch ex As Exception
            MsgBox("Could not upload app.db, please verify your connection.", MsgBoxStyle.Exclamation)
            MsgBox(ex.Message)
            MsgBox(ex.InnerException.Message)
            Return False
        End Try
    End Function

    Private Sub CheckButtonC(sender As Object, e As RoutedEventArgs) Handles CheckButton.Click
        If Not String.IsNullOrEmpty(ConsoleIP) Then
            'Check if we can connect to the FTP server
            If FilesAvailable(ConsoleIP) = True Then
                'Check if the browser is still located in the Tools folder
                If Directory.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\NPXS20102") = True Then
                    'Get the database and store temporarly in the Cache folder
                    If GetDatabaseOverFTP() = True Then

                        If FTPS5_MountRW() = True Then
                            RWAccess = True
                        End If

                        AddButton.IsEnabled = True
                        MsgBox("You can install the Web Browser now.", MsgBoxStyle.Information)
                    Else
                        MsgBox("Could not read the app.db file.", MsgBoxStyle.Exclamation)
                    End If
                End If
            End If
        Else
            MsgBox("Please enter your console's FTP IP address in the settings before continuing.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub AddButtonC(sender As Object, e As RoutedEventArgs) Handles AddButton.Click
        If RWAccess = True Then
            'Upload the Web Browser directly
            Try
                Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", 1337)
                    'Configurate the FTP connection
                    conn.Config.EncryptionMode = FtpEncryptionMode.None
                    conn.Config.SslProtocols = SslProtocols.None
                    conn.Config.DataConnectionEncryption = False

                    'Connect
                    conn.Connect()

                    'Send the content of NPXS40169
                    conn.UploadFile(My.Computer.FileSystem.CurrentDirectory + "\Tools\NPXS40169\license.txt", "/system_ex/rnps/apps/NPXS40169/license.txt", FtpRemoteExists.NoCheck, True)
                    conn.UploadFile(My.Computer.FileSystem.CurrentDirectory + "\Tools\NPXS40169\manifest.json", "/system_ex/rnps/apps/NPXS40169/manifest.json", FtpRemoteExists.NoCheck, True)

                    conn.UploadFile(My.Computer.FileSystem.CurrentDirectory + "\Tools\NPXS40169\appdb\NPXS40169\icon0.png", "/system_ex/rnps/apps/NPXS40169/appdb/NPXS40169/icon0.png", FtpRemoteExists.NoCheck, True)
                    conn.UploadFile(My.Computer.FileSystem.CurrentDirectory + "\Tools\NPXS40169\appdb\NPXS40169\ignore_devkit.dat", "/system_ex/rnps/apps/NPXS40169/appdb/NPXS40169/ignore_devkit.dat", FtpRemoteExists.NoCheck, True)
                    conn.UploadFile(My.Computer.FileSystem.CurrentDirectory + "\Tools\NPXS40169\appdb\NPXS40169\ignore_testkit.dat", "/system_ex/rnps/apps/NPXS40169/appdb/NPXS40169/ignore_testkit.dat", FtpRemoteExists.NoCheck, True)
                    conn.UploadFile(My.Computer.FileSystem.CurrentDirectory + "\Tools\NPXS40169\appdb\NPXS40169\param.json", "/system_ex/rnps/apps/NPXS40169/appdb/NPXS40169/param.json", FtpRemoteExists.NoCheck, True)

                    'Disconnect
                    conn.Disconnect()
                End Using

                MsgBox("The Web Browser has been successfully installed!", MsgBoxStyle.Information)
            Catch ex As Exception
                MsgBox("Could not upload the Web Browser, please verify your connection.", MsgBoxStyle.Exclamation)
                MsgBox(ex.Message)
                MsgBox(ex.InnerException.Message)
            End Try

        Else
            'Add the values to app.db
            Dim AppDBUdated As Boolean = False

            Try
                'Update the app.db
                Using conn As New SQLiteConnection("Data Source=" + My.Computer.FileSystem.CurrentDirectory + "\Cache\app.db")
                    conn.Open()

                    'Insert required values
                    Dim SelectCommand = conn.CreateCommand()
                    Dim TableRowsAffected As Integer = 0

                    SelectCommand.CommandText = "INSERT or REPLACE INTO tbl_contentinfo VALUES ('NPXS20102','IV9999-NPXS20102_00-XXXXXXXXXXXXXXXX','Internet browser','/system_ex/app/NPXS20102','1994-12-03 00:00:00.110',0,2,NULL,100,0,22,146,0,NULL,0,0,'2022-06-15 09:58:16.693','1994-12-03 00:00:00.110','game',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,0,'2022-10-03 09:08:36.703',0,1,67108864,NULL,0,0,0,'cid:local:NPXS20102','https://cthugha.exploit.menu/','pshome:gamehub?titleId=NPXS20102',0,0,NULL,'/user/data/apps/NPXS20102/icon0.png?ts=315532800',NULL,NULL,0,NULL,0,196611,NULL,NULL,4,0,0,NULL,0,'{""field_list"":[{""data"":22,""key"":""#_access_index"",""size"":8,""type"":0},{""data"":22,""key"":""#_access_index_1356e418"",""size"":8,""type"":0},{""data"":0,""key"":""#_contents_status"",""size"":8,""type"":0},{""data"":""1994-12-03 00:00:00.110"",""key"":""#_install_time"",""size"":23,""type"":2},{""data"":""2022-06-15 10:00:35.132"",""key"":""#_last_access_time"",""size"":23,""type"":2},{""data"":""2022-06-15 10:00:35.132"",""key"":""#_last_access_time_1356e418"",""size"":23,""type"":2},{""data"":""2022-10-03 09:08:36.703"",""key"":""#_mtime"",""size"":23,""type"":2},{""data"":""2022-06-15 09:58:16.693"",""key"":""#_promote_time"",""size"":23,""type"":2},{""data"":0,""key"":""#_size"",""size"":8,""type"":0},{""data"":0,""key"":""#exit_type"",""size"":8,""type"":0},{""data"":0,""key"":""APP_TYPE"",""size"":8,""type"":0},{""data"":0,""key"":""ATTRIBUTE"",""size"":8,""type"":0},{""data"":0,""key"":""ATTRIBUTE2"",""size"":8,""type"":0},{""data"":0,""key"":""ATTRIBUTE3"",""size"":8,""type"":0},{""data"":0,""key"":""ATTRIBUTE_EXE"",""size"":8,""type"":0},{""data"":0,""key"":""ATTRIBUTE_INTERNAL"",""size"":8,""type"":0},{""data"":67108864,""key"":""CATEGORY_TYPE"",""size"":8,""type"":0},{""data"":0,""key"":""CONTENT_BADGE_TYPE"",""size"":8,""type"":0},{""data"":""IV9999-NPXS40047_00-XXXXXXXXXXXXXXXX"",""key"":""CONTENT_ID"",""size"":36,""type"":2},{""data"":146,""key"":""DISPLAYLOCATION"",""size"":8,""type"":0},{""data"":0,""key"":""DISP_LOCATION_1"",""size"":8,""type"":0},{""data"":0,""key"":""DISP_LOCATION_2"",""size"":8,""type"":0},{""data"":0,""key"":""DOWNLOAD_DATA_SIZE"",""size"":8,""type"":0},{""data"":""pshome:gamestore"",""key"":""HUBAPP_URI"",""size"":16,""type"":2},{""data"":0,""key"":""MASS_SIZE"",""size"":8,""type"":0},{""data"":""prior:shellapp"",""key"":""METADATA_ID"",""size"":14,""type"":2},{""data"":0,""key"":""NOTICE_SCREEN_VERSION"",""size"":8,""type"":0},{""data"":0,""key"":""PARENTAL_LEVEL"",""size"":8,""type"":0},{""data"":0,""key"":""PUBTOOL_VERSION"",""size"":8,""type"":0},{""data"":0,""key"":""SERVICE_LAUNCH_BUTTON_KEY_CODE"",""size"":8,""type"":0},{""data"":""PlayStation Store"",""key"":""TITLE"",""size"":17,""type"":2},{""data"":""PlayStation Store"",""key"":""TITLE_01"",""size"":17,""type"":2},{""data"":""NPXS40047"",""key"":""TITLE_ID"",""size"":9,""type"":2},{""data"":0,""key"":""USER_DEFINED_PARAM_1"",""size"":8,""type"":0},{""data"":0,""key"":""USER_DEFINED_PARAM_2"",""size"":8,""type"":0},{""data"":0,""key"":""USER_DEFINED_PARAM_3"",""size"":8,""type"":0},{""data"":0,""key"":""USER_DEFINED_PARAM_4"",""size"":8,""type"":0},{""data"":1,""key"":""_app_format_type"",""size"":8,""type"":0},{""data"":0,""key"":""_contents_ext_type"",""size"":8,""type"":0},{""data"":2,""key"":""_contents_location"",""size"":8,""type"":0},{""data"":0,""key"":""_current_slot"",""size"":8,""type"":0},{""data"":0,""key"":""_disable_live_detail"",""size"":8,""type"":0},{""data"":0,""key"":""_external_hdd_app_status"",""size"":8,""type"":0},{""data"":0,""key"":""_hdd_location"",""size"":8,""type"":0},{""data"":0,""key"":""_install_status"",""size"":8,""type"":0},{""data"":1,""key"":""_install_sub_status"",""size"":8,""type"":0},{""data"":289074801081843713,""key"":""_install_version"",""size"":8,""type"":0},{""data"":""cid:local:NPXS40047"",""key"":""_local_concept_id"",""size"":19,""type"":2},{""data"":0,""key"":""_m2_device_id"",""size"":8,""type"":0},{""data"":""\/system_ex\/rnps\/apps\/NPXS40047\/appdb\/default"",""key"":""_metadata_path"",""size"":44,""type"":2},{""data"":0,""key"":""_not_install_sub_status"",""size"":8,""type"":0},{""data"":""\/system_ex\/rnps\/apps\/NPXS40047"",""key"":""_org_path"",""size"":30,""type"":2},{""data"":0,""key"":""_path_changeinfo_info"",""size"":8,""type"":0},{""data"":-8070450532247928832,""key"":""_path_icon0_info"",""size"":8,""type"":0},{""data"":315532800,""key"":""_path_icon0_info_time_stamp"",""size"":8,""type"":0},{""data"":0,""key"":""_path_info"",""size"":8,""type"":0},{""data"":0,""key"":""_path_info_2"",""size"":8,""type"":0},{""data"":0,""key"":""_path_pic0_info"",""size"":8,""type"":0},{""data"":0,""key"":""_path_pic0_info_time_stamp"",""size"":8,""type"":0},{""data"":0,""key"":""_path_pic1_info"",""size"":8,""type"":0},{""data"":0,""key"":""_path_pic1_info_time_stamp"",""size"":8,""type"":0},{""data"":0,""key"":""_path_promotion0_info"",""size"":8,""type"":0},{""data"":196611,""key"":""_primary_title_sort"",""size"":8,""type"":0},{""data"":0,""key"":""_ps_platform"",""size"":8,""type"":0},{""data"":0,""key"":""_size_other_hdd"",""size"":8,""type"":0},{""data"":6,""key"":""_sort_priority"",""size"":8,""type"":0},{""data"":0,""key"":""_uninstallable"",""size"":8,""type"":0},{""data"":0,""key"":""_view_category"",""size"":8,""type"":0},{""data"":195,""key"":""sync_index"",""size"":8,""type"":0}],""mimeType"":0,""mimeTypeFormatVersion"":0,""parserId"":0,""promoterFormatVersion"":0}');"
                    TableRowsAffected = SelectCommand.ExecuteNonQuery()

                    If TableRowsAffected = 1 Then
                        SelectCommand.CommandText = "INSERT or REPLACE INTO tbl_conceptmetadata VALUES ('cid:local:NPXS20102','NPXS20102','Internet browser','/system_ex/app/NPXS20102/sce_sys','2022-06-15 10:00:35.132',847,5,0,0,146,0,1,1,2,66049,'pssettings:play?mode=settings&function=update','pshome:gamehub?titleId=NPXS20102','/user/data/apps/NPXS20102/icon0.png?ts=315532800',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL);"
                        TableRowsAffected = SelectCommand.ExecuteNonQuery()
                        If TableRowsAffected = 1 Then
                            'Insert required values into each profile
                            For Each Table In DBTables
                                If Table.StartsWith("tbl_info") Then
                                    SelectCommand.CommandText = "INSERT or REPLACE INTO " + Table + " VALUES ('init_order_list','{""title_list"":[{""titleId"":""PPSA01325""},{""titleId"":""NPXS40075""}]}'),
 ('nsx_content_flag_NPXS40075','{""deleted"":false,""pushed"":true}'),
 ('nsx_content_flag_PPSA01325','{""deleted"":false,""pushed"":true}'),
 ('go_west_by_user','1'),
 ('nsx_content_flag_CUSA01127','{""deleted"":true,""pushed"":false}'),
 ('nsx_content_flag_CUSA01114','{""deleted"":true,""pushed"":false}'),
 ('nsx_content_flag_CUSA00001','{""deleted"":true,""pushed"":false}'),
 ('nsx_content_flag_NPXS29005','{""deleted"":true,""pushed"":false}'),
 ('nsx_content_flag_CUSA02012','{""deleted"":true,""pushed"":false}'),
 ('nsx_content_flag_CUSA18278','{""deleted"":true,""pushed"":false}'),
 ('nsx_content_flag_CUSA10249','{""deleted"":true,""pushed"":false}'),
 ('nsx_content_flag_CUSA00556','{""deleted"":true,""pushed"":false}');"
                                    TableRowsAffected = SelectCommand.ExecuteNonQuery()
                                ElseIf Table.StartsWith("tbl_iconinfo") Then

                                    If LowFW = True Then
                                        SelectCommand.CommandText = "INSERT or REPLACE INTO " + Table + " VALUES ('NPXS20102','Internet browser','2022-10-12 13:16:03.571',847,NULL,NULL,NULL,NULL,'1994-12-03 00:00:00.110','1994-12-03 00:00:00.110',146,1,0,0,'pssettings:play?mode=settings&function=update','pshome:gamehub?titleId=NPXS20102',0,0,0,0,0,4295163907,'IV9999-NPXS20102_00-XXXXXXXXXXXXXXXX',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL);"
                                    Else
                                        SelectCommand.CommandText = "INSERT or REPLACE INTO " + Table + " VALUES ('NPXS20102','Internet browser','cid:local:NPXS20102',0,'2022-10-12 13:16:03.571',847,NULL,NULL,NULL,NULL,'1994-12-03 00:00:00.110','1994-12-03 00:00:00.110',2,146,1,0,0,'pssettings:play?mode=settings&function=update','pshome:gamehub?titleId=NPXS20102',0,0,0,0,0,4295163907,'IV9999-NPXS20102_00-XXXXXXXXXXXXXXXX',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL);"
                                    End If

                                    TableRowsAffected = SelectCommand.ExecuteNonQuery()
                                ElseIf Table.StartsWith("tbl_concepticoninfo") Then

                                    If LowFW = True Then
                                        SelectCommand.CommandText = "INSERT or REPLACE INTO " + Table + " VALUES ('cid:local:NPXS20102',1,0,'Internet browser','2022-10-12 13:16:03.571',847,NULL,NULL,0,NULL,'1994-12-03 00:00:00.110','NPXS20102','Internet browser',0,NULL,'2022-10-12 13:16:03.571',2194,1,1,0,0,1,0,NULL,'2022-06-15 09:58:16.693','1994-12-03 00:00:00.110',0,'pssettings:play?mode=settings&function=update','pshome:gamehub?titleId=NPXS20102','/system_ex/app/NPXS20102','/user/data/apps/NPXS20102/icon0.png?ts=315532800',NULL,NULL,1,0,NULL,NULL,NULL,NULL,0,'0',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL)"
                                    Else
                                        SelectCommand.CommandText = "INSERT or REPLACE INTO " + Table + " VALUES ('cid:local:NPXS20102',1,0,'Internet browser','2022-10-12 13:16:03.571',847,NULL,NULL,0,NULL,'1994-12-03 00:00:00.110','NPXS20102','Internet browser',0,NULL,'2022-10-12 13:16:03.571',2194,1,1,0,0,1,0,NULL,'2022-06-15 09:58:16.693','1994-12-03 00:00:00.110',0,'pssettings:play?mode=settings&function=update','pshome:gamehub?titleId=NPXS20102','/system_ex/app/NPXS20102','/user/data/apps/NPXS20102/icon0.png?ts=315532800',NULL,NULL,1,0,NULL,NULL,NULL,NULL,0,0,'0',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL)"
                                    End If

                                    TableRowsAffected = SelectCommand.ExecuteNonQuery()
                                End If
                            Next

                            'Update done
                            AppDBUdated = True
                        End If
                    Else
                        MsgBox("Could not update the database.", MsgBoxStyle.Exclamation)
                    End If

                    'Close & dispose
                    conn.Close()
                    conn.Dispose()
                    SelectCommand.Dispose()
                End Using

                'Completely close the SQLiteConnection and release access to app.db
                SQLiteConnection.ClearAllPools()
                GC.Collect()
                GC.WaitForPendingFinalizers()

                'Now upload the updated app.db and files
                If AppDBUdated = True Then
                    If SendDatabaseOverFTP() = True Then
                        MsgBox("The Web Browser has been successfully installed!" + vbCrLf + "If it doesn't show up on the home menu then restart the console.", MsgBoxStyle.Information)
                    End If
                Else
                    MsgBox("Could not update the database.", MsgBoxStyle.Exclamation)
                End If
            Catch ex As Exception
                MsgBox("Could not update app.db, please verify your connection.", MsgBoxStyle.Exclamation)
                MsgBox(ex.Message)
                MsgBox(ex.InnerException.Message)
            End Try

        End If
    End Sub

End Class
