using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentFTP;
using Microsoft.Data.Sqlite;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace PSMultiTools.PS5.Tools;

public partial class PS5WebBrowserAdder : Window
{

    public string ConsoleIP = "";
    public string ConsolePort = "";

    public List<string> DBTables = [];
    public bool LowFW = false;
    public bool RWAccess = false;

    public PS5WebBrowserAdder()
    {
        InitializeComponent();

        // Init SQLite
        SQLitePCL.Batteries_V2.Init();
    }

    private bool FTPS5_MountRW()
    {
        using var conn = new FtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort));
        // Configurate the FTP connection
        conn.Config.EncryptionMode = FtpEncryptionMode.None;
        conn.Config.SslProtocols = SslProtocols.None;
        conn.Config.DataConnectionEncryption = false;

        // Connect
        conn.Connect();

        // Mount /system & /system_ex with RW permission
        FtpReply RequestRWReply = default;
        RequestRWReply = conn.Execute("MTRW");

        // Disconnect
        conn.Disconnect();

        if (RequestRWReply.Success == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static async Task<bool> FilesAvailableAsync(string ConsoleIP, string ConsolePort)
    {
        try
        {
            // Configurate the FTP connection
            using var conn = new FtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort));
            conn.Config.EncryptionMode = FtpEncryptionMode.None;
            conn.Config.SslProtocols = SslProtocols.None;
            conn.Config.DataConnectionEncryption = false;

            int TotalDirs = 0;

            // Connect
            conn.Connect();

            // Change to /dev directory
            conn.SetWorkingDirectory("/dev");

            foreach (var FileInFTP in conn.GetListing())
                TotalDirs++;

            if (TotalDirs > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not connect, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return false;
        }
    }

    public async Task<bool> GetDatabaseOverFTPAsync()
    {
        if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Cache")))
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Cache"));

        try
        {
            // Configurate the FTP connection
            using var conn = new FtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort));
            conn.Config.EncryptionMode = FtpEncryptionMode.None;
            conn.Config.SslProtocols = SslProtocols.None;
            conn.Config.DataConnectionEncryption = false;

            // Connect
            conn.Connect();

            // Get app.db
            conn.DownloadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "app.db"), "/system_data/priv/mms/app.db", FtpLocalExists.Overwrite);

            // Disconnect
            conn.Disconnect();
        }
        catch (Exception)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not get the app.db, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return false;
        }

        try
        {
            using (var conn = new SqliteConnection("Data Source=" + Path.Combine(Environment.CurrentDirectory, "Cache", "app.db")))
            {
                conn.Open();

                var SelectCommand = conn.CreateCommand();
                SelectCommand.CommandText = "select name from sqlite_master where type='table' order by name";

                var DataReader = SelectCommand.ExecuteReader();

                // Get all tables from each profile
                while (DataReader.Read())
                {
                    if (DataReader["name"].ToString()!.StartsWith("tbl_addon"))
                    {
                        DBTables.Add(DataReader["name"].ToString()!);
                    }
                    else if (DataReader["name"].ToString()!.StartsWith("tbl_concepticoninfo"))
                    {
                        DBTables.Add(DataReader["name"].ToString()!);
                    }
                    else if (DataReader["name"].ToString()!.StartsWith("tbl_iconinfo"))
                    {
                        DBTables.Add(DataReader["name"].ToString()!);
                    }
                    else if (DataReader["name"].ToString()!.StartsWith("tbl_info"))
                    {
                        DBTables.Add(DataReader["name"].ToString()!);
                    }
                }

                DataReader.Close();

                // Check if the tbl_concepticoninfo contains column 'genreFlags' (Seems to be new on 4.00+)
                foreach (var Table in DBTables)
                {
                    if (Table.StartsWith("tbl_concepticoninfo"))
                    {
                        SelectCommand.CommandText = string.Format("PRAGMA table_info({0})", Table);

                        var reader = SelectCommand.ExecuteReader();
                        int nameIndex = reader.GetOrdinal("Name");

                        while (reader.Read())
                        {
                            if (reader.GetString(nameIndex).Equals("genreFlags"))
                            {
                                LowFW = false;
                            }
                            else
                            {
                                LowFW = true;
                            }
                        }

                        reader.Close();
                        break; // We only need to check once
                    }
                }

                // Close & dispose
                conn.Close();
                conn.Dispose();
                SelectCommand.Dispose();
            }

            // Completely close the SQLiteConnection and release access to app.db
            SqliteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (DBTables.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        catch (SqliteException)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not get the app.db, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return false;
        }
    }

    public async Task<bool> SendDatabaseOverFTPAsync()
    {
        try
        {
            using var conn = new FtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort));
            // Configurate the FTP connection
            conn.Config.EncryptionMode = FtpEncryptionMode.None;
            conn.Config.SslProtocols = SslProtocols.None;
            conn.Config.DataConnectionEncryption = false;

            // Connect
            conn.Connect();

            // Remove the old app.db (overwrite is not implemented yet) to upload the updated one
            conn.DeleteFile("/system_data/priv/mms/app.db");

            // Send the app.db back
            conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "app.db"), "/system_data/priv/mms/app.db", FtpRemoteExists.NoCheck);

            // Send the data/app folder
            conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Tools", "NPXS20102", "icon0.png"), "/data/apps/NPXS20102/icon0.png", FtpRemoteExists.NoCheck, true);
            conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Tools", "NPXS20102", "param.json"), "/data/apps/NPXS20102/param.json", FtpRemoteExists.NoCheck, true);

            // Disconnect
            conn.Disconnect();
            return true;
        }
        catch (Exception)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not get the app.db, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return false;
        }
    }

    private async void CheckButtonC(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ConsoleIP))
        {
            // Check if we can connect to the FTP server
            if (await FilesAvailableAsync(ConsoleIP, ConsolePort) == true)
            {
                // Check if the browser is still located in the Tools folder
                if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "NPXS20102")) == true)
                {
                    // Get the database and store temporarly in the Cache folder
                    if (await GetDatabaseOverFTPAsync() == true)
                    {

                        if (FTPS5_MountRW() == true)
                        {
                            RWAccess = true;
                        }

                        AddButton.IsEnabled = true;
                        var box = MessageBoxManager.GetMessageBoxStandard("Info", "You can install the Web Browser now.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box.ShowWindowAsync();
                    }
                    else
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not get the app.db, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }
                }
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please enter your console's FTP IP address in the settings before continuing.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void AddButtonC(object? sender, RoutedEventArgs e)
    {
        if (RWAccess == true)
        {
            // Upload the Web Browser directly
            try
            {
                using (var conn = new FtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort)))
                {
                    // Configurate the FTP connection
                    conn.Config.EncryptionMode = FtpEncryptionMode.None;
                    conn.Config.SslProtocols = SslProtocols.None;
                    conn.Config.DataConnectionEncryption = false;

                    // Connect
                    conn.Connect();

                    // Send the content of NPXS40169
                    conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Tools", "NPXS40169", "license.txt"), "/system_ex/rnps/apps/NPXS40169/license.txt", FtpRemoteExists.NoCheck, true);
                    conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Tools", "NPXS40169", "manifest.json"), "/system_ex/rnps/apps/NPXS40169/manifest.json", FtpRemoteExists.NoCheck, true);

                    conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Tools", "NPXS40169", "appdb", "NPXS40169", "icon0.png"), "/system_ex/rnps/apps/NPXS40169/appdb/NPXS40169/icon0.png", FtpRemoteExists.NoCheck, true);
                    conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Tools", "NPXS40169", "appdb", "NPXS40169", "ignore_devkit.dat"), "/system_ex/rnps/apps/NPXS40169/appdb/NPXS40169/ignore_devkit.dat", FtpRemoteExists.NoCheck, true);
                    conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Tools", "NPXS40169", "appdb", "NPXS40169", "ignore_testkit.dat"), "/system_ex/rnps/apps/NPXS40169/appdb/NPXS40169/ignore_testkit.dat", FtpRemoteExists.NoCheck, true);
                    conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Tools", "NPXS40169", "appdb", "NPXS40169", "param.json"), "/system_ex/rnps/apps/NPXS40169/appdb/NPXS40169/param.json", FtpRemoteExists.NoCheck, true);

                    // Disconnect
                    conn.Disconnect();
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "The Web Browser has been successfully installed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
            catch (Exception)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not upload the Web Browser, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }

        else
        {
            // Add the values to app.db
            bool AppDBUdated = false;

            try
            {
                // Update the app.db
                using (var conn = new SqliteConnection("Data Source=" + Path.Combine(Environment.CurrentDirectory, "Cache", "app.db")))
                {
                    conn.Open();

                    // Insert required values
                    var SelectCommand = conn.CreateCommand();
                    int TableRowsAffected = 0;

                    SelectCommand.CommandText = @"INSERT or REPLACE INTO tbl_contentinfo VALUES ('NPXS20102','IV9999-NPXS20102_00-XXXXXXXXXXXXXXXX','Internet browser','/system_ex/app/NPXS20102','1994-12-03 00:00:00.110',0,2,NULL,100,0,22,146,0,NULL,0,0,'2022-06-15 09:58:16.693','1994-12-03 00:00:00.110','game',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,0,'2022-10-03 09:08:36.703',0,1,67108864,NULL,0,0,0,'cid:local:NPXS20102','https://cthugha.exploit.menu/','pshome:gamehub?titleId=NPXS20102',0,0,NULL,'/user/data/apps/NPXS20102/icon0.png?ts=315532800',NULL,NULL,0,NULL,0,196611,NULL,NULL,4,0,0,NULL,0,'{""field_list"":[{""data"":22,""key"":""#_access_index"",""size"":8,""type"":0},{""data"":22,""key"":""#_access_index_1356e418"",""size"":8,""type"":0},{""data"":0,""key"":""#_contents_status"",""size"":8,""type"":0},{""data"":""1994-12-03 00:00:00.110"",""key"":""#_install_time"",""size"":23,""type"":2},{""data"":""2022-06-15 10:00:35.132"",""key"":""#_last_access_time"",""size"":23,""type"":2},{""data"":""2022-06-15 10:00:35.132"",""key"":""#_last_access_time_1356e418"",""size"":23,""type"":2},{""data"":""2022-10-03 09:08:36.703"",""key"":""#_mtime"",""size"":23,""type"":2},{""data"":""2022-06-15 09:58:16.693"",""key"":""#_promote_time"",""size"":23,""type"":2},{""data"":0,""key"":""#_size"",""size"":8,""type"":0},{""data"":0,""key"":""#exit_type"",""size"":8,""type"":0},{""data"":0,""key"":""APP_TYPE"",""size"":8,""type"":0},{""data"":0,""key"":""ATTRIBUTE"",""size"":8,""type"":0},{""data"":0,""key"":""ATTRIBUTE2"",""size"":8,""type"":0},{""data"":0,""key"":""ATTRIBUTE3"",""size"":8,""type"":0},{""data"":0,""key"":""ATTRIBUTE_EXE"",""size"":8,""type"":0},{""data"":0,""key"":""ATTRIBUTE_INTERNAL"",""size"":8,""type"":0},{""data"":67108864,""key"":""CATEGORY_TYPE"",""size"":8,""type"":0},{""data"":0,""key"":""CONTENT_BADGE_TYPE"",""size"":8,""type"":0},{""data"":""IV9999-NPXS40047_00-XXXXXXXXXXXXXXXX"",""key"":""CONTENT_ID"",""size"":36,""type"":2},{""data"":146,""key"":""DISPLAYLOCATION"",""size"":8,""type"":0},{""data"":0,""key"":""DISP_LOCATION_1"",""size"":8,""type"":0},{""data"":0,""key"":""DISP_LOCATION_2"",""size"":8,""type"":0},{""data"":0,""key"":""DOWNLOAD_DATA_SIZE"",""size"":8,""type"":0},{""data"":""pshome:gamestore"",""key"":""HUBAPP_URI"",""size"":16,""type"":2},{""data"":0,""key"":""MASS_SIZE"",""size"":8,""type"":0},{""data"":""prior:shellapp"",""key"":""METADATA_ID"",""size"":14,""type"":2},{""data"":0,""key"":""NOTICE_SCREEN_VERSION"",""size"":8,""type"":0},{""data"":0,""key"":""PARENTAL_LEVEL"",""size"":8,""type"":0},{""data"":0,""key"":""PUBTOOL_VERSION"",""size"":8,""type"":0},{""data"":0,""key"":""SERVICE_LAUNCH_BUTTON_KEY_CODE"",""size"":8,""type"":0},{""data"":""PlayStation Store"",""key"":""TITLE"",""size"":17,""type"":2},{""data"":""PlayStation Store"",""key"":""TITLE_01"",""size"":17,""type"":2},{""data"":""NPXS40047"",""key"":""TITLE_ID"",""size"":9,""type"":2},{""data"":0,""key"":""USER_DEFINED_PARAM_1"",""size"":8,""type"":0},{""data"":0,""key"":""USER_DEFINED_PARAM_2"",""size"":8,""type"":0},{""data"":0,""key"":""USER_DEFINED_PARAM_3"",""size"":8,""type"":0},{""data"":0,""key"":""USER_DEFINED_PARAM_4"",""size"":8,""type"":0},{""data"":1,""key"":""_app_format_type"",""size"":8,""type"":0},{""data"":0,""key"":""_contents_ext_type"",""size"":8,""type"":0},{""data"":2,""key"":""_contents_location"",""size"":8,""type"":0},{""data"":0,""key"":""_current_slot"",""size"":8,""type"":0},{""data"":0,""key"":""_disable_live_detail"",""size"":8,""type"":0},{""data"":0,""key"":""_external_hdd_app_status"",""size"":8,""type"":0},{""data"":0,""key"":""_hdd_location"",""size"":8,""type"":0},{""data"":0,""key"":""_install_status"",""size"":8,""type"":0},{""data"":1,""key"":""_install_sub_status"",""size"":8,""type"":0},{""data"":289074801081843713,""key"":""_install_version"",""size"":8,""type"":0},{""data"":""cid:local:NPXS40047"",""key"":""_local_concept_id"",""size"":19,""type"":2},{""data"":0,""key"":""_m2_device_id"",""size"":8,""type"":0},{""data"":""\/system_ex\/rnps\/apps\/NPXS40047\/appdb\/default"",""key"":""_metadata_path"",""size"":44,""type"":2},{""data"":0,""key"":""_not_install_sub_status"",""size"":8,""type"":0},{""data"":""\/system_ex\/rnps\/apps\/NPXS40047"",""key"":""_org_path"",""size"":30,""type"":2},{""data"":0,""key"":""_path_changeinfo_info"",""size"":8,""type"":0},{""data"":-8070450532247928832,""key"":""_path_icon0_info"",""size"":8,""type"":0},{""data"":315532800,""key"":""_path_icon0_info_time_stamp"",""size"":8,""type"":0},{""data"":0,""key"":""_path_info"",""size"":8,""type"":0},{""data"":0,""key"":""_path_info_2"",""size"":8,""type"":0},{""data"":0,""key"":""_path_pic0_info"",""size"":8,""type"":0},{""data"":0,""key"":""_path_pic0_info_time_stamp"",""size"":8,""type"":0},{""data"":0,""key"":""_path_pic1_info"",""size"":8,""type"":0},{""data"":0,""key"":""_path_pic1_info_time_stamp"",""size"":8,""type"":0},{""data"":0,""key"":""_path_promotion0_info"",""size"":8,""type"":0},{""data"":196611,""key"":""_primary_title_sort"",""size"":8,""type"":0},{""data"":0,""key"":""_ps_platform"",""size"":8,""type"":0},{""data"":0,""key"":""_size_other_hdd"",""size"":8,""type"":0},{""data"":6,""key"":""_sort_priority"",""size"":8,""type"":0},{""data"":0,""key"":""_uninstallable"",""size"":8,""type"":0},{""data"":0,""key"":""_view_category"",""size"":8,""type"":0},{""data"":195,""key"":""sync_index"",""size"":8,""type"":0}],""mimeType"":0,""mimeTypeFormatVersion"":0,""parserId"":0,""promoterFormatVersion"":0}');";
                    TableRowsAffected = SelectCommand.ExecuteNonQuery();

                    if (TableRowsAffected == 1)
                    {
                        SelectCommand.CommandText = "INSERT or REPLACE INTO tbl_conceptmetadata VALUES ('cid:local:NPXS20102','NPXS20102','Internet browser','/system_ex/app/NPXS20102/sce_sys','2022-06-15 10:00:35.132',847,5,0,0,146,0,1,1,2,66049,'pssettings:play?mode=settings&function=update','pshome:gamehub?titleId=NPXS20102','/user/data/apps/NPXS20102/icon0.png?ts=315532800',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL);";
                        TableRowsAffected = SelectCommand.ExecuteNonQuery();
                        if (TableRowsAffected == 1)
                        {
                            // Insert required values into each profile
                            foreach (var Table in DBTables)
                            {
                                if (Table.StartsWith("tbl_info"))
                                {
                                    SelectCommand.CommandText = "INSERT or REPLACE INTO " + Table + @" VALUES ('init_order_list','{""title_list"":[{""titleId"":""PPSA01325""},{""titleId"":""NPXS40075""}]}'),
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
 ('nsx_content_flag_CUSA00556','{""deleted"":true,""pushed"":false}');";
                                    TableRowsAffected = SelectCommand.ExecuteNonQuery();
                                }
                                else if (Table.StartsWith("tbl_iconinfo"))
                                {

                                    if (LowFW == true)
                                    {
                                        SelectCommand.CommandText = "INSERT or REPLACE INTO " + Table + " VALUES ('NPXS20102','Internet browser','2022-10-12 13:16:03.571',847,NULL,NULL,NULL,NULL,'1994-12-03 00:00:00.110','1994-12-03 00:00:00.110',146,1,0,0,'pssettings:play?mode=settings&function=update','pshome:gamehub?titleId=NPXS20102',0,0,0,0,0,4295163907,'IV9999-NPXS20102_00-XXXXXXXXXXXXXXXX',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL);";
                                    }
                                    else
                                    {
                                        SelectCommand.CommandText = "INSERT or REPLACE INTO " + Table + " VALUES ('NPXS20102','Internet browser','cid:local:NPXS20102',0,'2022-10-12 13:16:03.571',847,NULL,NULL,NULL,NULL,'1994-12-03 00:00:00.110','1994-12-03 00:00:00.110',2,146,1,0,0,'pssettings:play?mode=settings&function=update','pshome:gamehub?titleId=NPXS20102',0,0,0,0,0,4295163907,'IV9999-NPXS20102_00-XXXXXXXXXXXXXXXX',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL);";
                                    }

                                    TableRowsAffected = SelectCommand.ExecuteNonQuery();
                                }
                                else if (Table.StartsWith("tbl_concepticoninfo"))
                                {

                                    if (LowFW == true)
                                    {
                                        SelectCommand.CommandText = "INSERT or REPLACE INTO " + Table + " VALUES ('cid:local:NPXS20102',1,0,'Internet browser','2022-10-12 13:16:03.571',847,NULL,NULL,0,NULL,'1994-12-03 00:00:00.110','NPXS20102','Internet browser',0,NULL,'2022-10-12 13:16:03.571',2194,1,1,0,0,1,0,NULL,'2022-06-15 09:58:16.693','1994-12-03 00:00:00.110',0,'pssettings:play?mode=settings&function=update','pshome:gamehub?titleId=NPXS20102','/system_ex/app/NPXS20102','/user/data/apps/NPXS20102/icon0.png?ts=315532800',NULL,NULL,1,0,NULL,NULL,NULL,NULL,0,'0',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL)";
                                    }
                                    else
                                    {
                                        SelectCommand.CommandText = "INSERT or REPLACE INTO " + Table + " VALUES ('cid:local:NPXS20102',1,0,'Internet browser','2022-10-12 13:16:03.571',847,NULL,NULL,0,NULL,'1994-12-03 00:00:00.110','NPXS20102','Internet browser',0,NULL,'2022-10-12 13:16:03.571',2194,1,1,0,0,1,0,NULL,'2022-06-15 09:58:16.693','1994-12-03 00:00:00.110',0,'pssettings:play?mode=settings&function=update','pshome:gamehub?titleId=NPXS20102','/system_ex/app/NPXS20102','/user/data/apps/NPXS20102/icon0.png?ts=315532800',NULL,NULL,1,0,NULL,NULL,NULL,NULL,0,0,'0',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL)";
                                    }

                                    TableRowsAffected = SelectCommand.ExecuteNonQuery();
                                }
                            }

                            // Update done
                            AppDBUdated = true;
                        }
                    }
                    else
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not update the database.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }

                    // Close & dispose
                    conn.Close();
                    conn.Dispose();
                    SelectCommand.Dispose();
                }

                // Completely close the SQLiteConnection and release access to app.db
                SqliteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Now upload the updated app.db and files
                if (AppDBUdated == true)
                {
                    if (await SendDatabaseOverFTPAsync() == true)
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Info", "The Web Browser has been successfully installed!" + Environment.NewLine + "If it doesn't show up on the home menu then restart the console.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box.ShowWindowAsync();
                    }
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not update the database.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
            catch (Exception)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not update app.db, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }

        }
    }

}