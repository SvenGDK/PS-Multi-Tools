using Avalonia.Controls;
using Avalonia.VisualTree;
using FluentFTP;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using static PSMultiTools.Classes.PS5ParamClass;

namespace PSMultiTools.PS5.Tools;

public partial class PS5YT2JBToolbox : Window
{

    public string ConsoleIP = "";
    public string ConsoleFTPPort = "";
    public string ConsolePayloadPort = "";

    private ScrollViewer? LogTextBoxScrollViewer;

    public PS5YT2JBToolbox()
    {
        InitializeComponent();

        Loaded += PS5YT2JBToolbox_Loaded;

        // Init SQLite
        SQLitePCL.Batteries_V2.Init();
    }

    private void PS5YT2JBToolbox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();

        if (!string.IsNullOrEmpty(ConsoleIP))
            PS5IPTextBox.Text = ConsoleIP;

        if (!string.IsNullOrEmpty(ConsoleFTPPort))
            PS5FTPPortTextBox.Text = ConsoleFTPPort;

        if (!string.IsNullOrEmpty(ConsolePayloadPort))
            PS5PayloadPortTextBox.Text = ConsolePayloadPort;
    }

    private async void PatchLocalFiles_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a folder that contains app.db, appinfo.db and param.json" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            int totalRows = 0;
            LogTextBox.Clear();

            try
            {
                if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Cache")))
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Cache"));
            }
            catch (Exception ex) { LogTextBox.Text += $"Error while creating the Cache folder!\n{ex.Message}\nTask stopped."; return; }

            if (File.Exists(Path.Combine(FBDResult, "appinfo.db")))
            {
                LogTextBox.Text += "Creating a backup of appinfo.db ...\n";
                File.Copy(Path.Combine(FBDResult, "appinfo.db"), Path.Combine(Environment.CurrentDirectory, "Cache", "appinfo_backup.db"), true);
                LogTextBox.Text += "Backup of appinfo.db done!\n";

                LogTextBox.Text += "Starting to patch appinfo.db ...";
                LogTextBoxScrollViewer?.ScrollToEnd();

                // Patch appinfo.db
                using var connection = new SqliteConnection($"Data Source={Path.Combine(FBDResult, "appinfo.db")}");
                connection.Open();

                using var transaction = connection.BeginTransaction();
                try
                {
                    // Update tbl_appinfo columns
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = transaction;

                        cmd.CommandText = @"
                    UPDATE tbl_appinfo 
                    SET val = $contentVersion 
                    WHERE titleId = $titleId 
                    AND key = 'CONTENT_VERSION';
                ";
                        cmd.Parameters.AddWithValue("$contentVersion", "99.999.999");
                        cmd.Parameters.AddWithValue("$titleId", "PPSA01650");

                        totalRows += cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();

                        cmd.CommandText = @"
                    UPDATE tbl_appinfo 
                    SET val = $versionFileUri 
                    WHERE titleId = $titleId 
                    AND key = 'VERSION_FILE_URI';
                ";
                        cmd.Parameters.AddWithValue("$versionFileUri", "http://127.0.0.2");
                        cmd.Parameters.AddWithValue("$titleId", "PPSA01650");

                        totalRows += cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        LogTextBox.Text += "An error occured, rolling back changes made to appinfo.db\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();
                        transaction.Rollback();
                    }
                    catch
                    {
                        LogTextBox.Text += "Failed to rollback appinfo.db changes!\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }
                }

                try
                {
                    // Dispose transaction & connection & make sure access to file is released
                    transaction.Dispose();
                    connection.Close();
                    SqliteConnection.ClearPool(connection);
                    connection.Dispose();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch (Exception ex) { LogTextBox.Text += $"WARNING: Could not close the appinfo.db connection.\n{ex.Message}\n"; }

                LogTextBox.Text += $"appinfo.db patched - {totalRows} changes done.\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }
            else
            {
                LogTextBox.Text += "File: appinfo.db not found and will not be patched!\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }

            totalRows = 0;

            if (File.Exists(Path.Combine(FBDResult, "app.db")))
            {
                LogTextBox.Text += "Creating a backup of app.db ...\n";
                File.Copy(Path.Combine(FBDResult, "app.db"), Path.Combine(Environment.CurrentDirectory, "Cache", "app_backup.db"), true);
                LogTextBox.Text += "Backup of app.db done!\n";

                LogTextBox.Text += "Starting to patch app.db ...\n";
                LogTextBoxScrollViewer?.ScrollToEnd();

                // Patch app.db
                using var NewConnection = new SqliteConnection($"Data Source={Path.Combine(FBDResult, "app.db")}");
                NewConnection.Open();

                using var NewTransaction = NewConnection.BeginTransaction();
                try
                {
                    // Update JSON inside tbl_contentinfo.AppInfoJson using json_set
                    using (var NewCMD = NewConnection.CreateCommand())
                    {
                        NewCMD.Transaction = NewTransaction;
                        NewCMD.CommandText = @"
                    UPDATE tbl_contentinfo
                    SET AppInfoJson = json_set(
                        AppInfoJson,
                        '$.CONTENT_VERSION', $contentVersion,
                        '$.VERSION_FILE_URI', $versionFileUri
                    )
                    WHERE titleId = $titleId;
                ";
                        NewCMD.Parameters.AddWithValue("$contentVersion", "99.999.999");
                        NewCMD.Parameters.AddWithValue("$versionFileUri", "http://127.0.0.2");
                        NewCMD.Parameters.AddWithValue("$titleId", "PPSA01650");

                        totalRows += NewCMD.ExecuteNonQuery();
                    }

                    NewTransaction.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        LogTextBox.Text += "An error occured, rolling back changes made to app.db\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();
                        NewTransaction.Rollback();
                    }
                    catch
                    {
                        LogTextBox.Text += "Failed to rollback app.db changes!\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }
                }

                try
                {
                    // Dispose transaction & connection & make sure access to file is released
                    NewTransaction.Dispose();
                    NewConnection.Close();
                    SqliteConnection.ClearPool(NewConnection);
                    NewConnection.Dispose();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch (Exception ex) { LogTextBox.Text += $"WARNING: Could not close the app.db connection.\n{ex.Message}\n"; }

                LogTextBox.Text += $"app.db patched - {totalRows} changes done.\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }
            else
            {
                LogTextBox.Text += "File: app.db not found and will not be patched!\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }

            // Patch param.json
            if (File.Exists(Path.Combine(FBDResult, "param.json")))
            {
                LogTextBox.Text += "Creating a backup of param.json ...\n";
                File.Copy(Path.Combine(FBDResult, "param.json"), Path.Combine(Environment.CurrentDirectory, "Cache", "param_backup.json"), true);
                LogTextBox.Text += "Backup of param.json done!\n";

                LogTextBox.Text += "Starting to patch param.json ...\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
                try
                {
                    string JSONData = File.ReadAllText(Path.Combine(FBDResult, "param.json"));
                    if (JSONData != null)
                    {
                        PS5Param ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData)!;

                        ParamData.ContentVersion = "99.999.999";
                        ParamData.VersionFileUri = "http://127.0.0.2";

                        string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                        File.WriteAllText(Path.Combine(FBDResult, "param.json"), RawDataJSON);
                    }
                    else
                    {
                        LogTextBox.Text += "Could not read param.json!\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }
                }
                catch
                {
                    LogTextBox.Text += "Failed to modify param.json!\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();
                }
            }
            else
            {
                LogTextBox.Text += "File: param.json not found and will not be patched!\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }

            LogTextBox.Text += "Done! All found files have been patched and ready to use.\n";
            LogTextBoxScrollViewer?.ScrollToEnd();
        }
        else
        {
            LogTextBox.Text += "No folder selected.\n";
            LogTextBoxScrollViewer?.ScrollToEnd();
        }
    }

    private async void PatchFTPFiles_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PS5IPTextBox.Text) && !string.IsNullOrEmpty(PS5FTPPortTextBox.Text))
        {
            LogTextBox.Clear();

            LogTextBox.Text += "Getting :\n";
            LogTextBox.Text += "/system_data/priv/mms/appinfo.db\n";
            LogTextBox.Text += "/system_data/priv/mms/app.db\n";
            LogTextBox.Text += "/user/appmeta/PPSA01650/param.json\n";
            LogTextBox.Text += "Please wait ...\n";
            LogTextBoxScrollViewer?.ScrollToEnd();

            try
            {
                if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Cache")))
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Cache"));
            }
            catch (Exception ex) { LogTextBox.Text += $"Error while creating the Cache folder!\n{ex.Message}\nTask stopped."; return; }

            try
            {
                IEnumerable<string> RemoteFiles = [
                    "/system_data/priv/mms/appinfo.db",
                        "/system_data/priv/mms/app.db",
                        "/user/appmeta/PPSA01650/param.json"
                    ];

                // Configurate AsyncFtpClient
                using var conn = new AsyncFtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", int.Parse(PS5FTPPortTextBox.Text));
                conn.Config.EncryptionMode = FtpEncryptionMode.None;
                conn.Config.SslProtocols = SslProtocols.None;
                conn.Config.DataConnectionEncryption = false;

                // Connect
                await conn.Connect();

                // Get required files
                await conn.DownloadFiles(Path.Combine(Environment.CurrentDirectory, "Cache"), RemoteFiles, FtpLocalExists.Overwrite, FtpVerify.None, FtpError.None);

                // Temporary disconnect
                await conn.Disconnect();
            }
            catch (Exception)
            {
                LogTextBox.Text += "Could not download any files from PS5, please verify your connection.\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }

            LogTextBox.Text += $"All files saved to. {Path.Combine(Environment.CurrentDirectory, "Cache")}\n";
            LogTextBox.Text += "Creating a backup of all files ...\n";
            LogTextBoxScrollViewer?.ScrollToEnd();

            File.Copy(Path.Combine(Environment.CurrentDirectory, "Cache", "appinfo.db"), Path.Combine(Environment.CurrentDirectory, "Cache", "appinfo_backup.db"), true);
            File.Copy(Path.Combine(Environment.CurrentDirectory, "Cache", "app.db"), Path.Combine(Environment.CurrentDirectory, "Cache", "app_backup.db"), true);
            File.Copy(Path.Combine(Environment.CurrentDirectory, "Cache", "param.json"), Path.Combine(Environment.CurrentDirectory, "Cache", "param_backup.json"), true);

            LogTextBox.Text += "Backup done!\n";
            LogTextBox.Text += "Starting to patch all files ...\n";
            LogTextBoxScrollViewer?.ScrollToEnd();

            int totalRows = 0;

            LogTextBox.Text += "Starting to patch appinfo.db ...";
            LogTextBoxScrollViewer?.ScrollToEnd();

            // Patch appinfo.db
            using var connection = new SqliteConnection($"Data Source={Path.Combine(Environment.CurrentDirectory, "Cache", "appinfo.db")}");
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                // Update tbl_appinfo columns
                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = transaction;

                    cmd.CommandText = @"
                    UPDATE tbl_appinfo 
                    SET val = $contentVersion 
                    WHERE titleId = $titleId 
                    AND key = 'CONTENT_VERSION'
                ";
                    cmd.Parameters.AddWithValue("$contentVersion", "99.999.999");
                    cmd.Parameters.AddWithValue("$titleId", "PPSA01650");

                    totalRows += cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();

                    cmd.CommandText = @"
                    UPDATE tbl_appinfo 
                    SET val = $versionFileUri 
                    WHERE titleId = $titleId 
                    AND key = 'VERSION_FILE_URI'
                ";
                    cmd.Parameters.AddWithValue("$versionFileUri", "http://127.0.0.2");
                    cmd.Parameters.AddWithValue("$titleId", "PPSA01650");

                    totalRows += cmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                try
                {
                    LogTextBox.Text += "An error occured, rolling back changes made to appinfo.db\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();
                    transaction.Rollback();
                }
                catch
                {
                    LogTextBox.Text += "Failed to rollback appinfo.db changes!\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();
                }
            }

            try
            {
                // Dispose transaction & connection & make sure access to file is released
                transaction.Dispose();
                connection.Close();
                SqliteConnection.ClearPool(connection);
                connection.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex) { LogTextBox.Text += $"WARNING: Could not close the appinfo.db connection.\n{ex.Message}\n"; }

            LogTextBox.Text += $"appinfo.db patched - {totalRows} changes done.\n";
            LogTextBoxScrollViewer?.ScrollToEnd();

            totalRows = 0;

            LogTextBox.Text += "Starting to patch app.db ...\n";
            LogTextBoxScrollViewer?.ScrollToEnd();

            // Patch app.db
            using var NewConnection = new SqliteConnection($"Data Source={Path.Combine(Environment.CurrentDirectory, "Cache", "app.db")}");
            NewConnection.Open();

            using var NewTransaction = NewConnection.BeginTransaction();
            try
            {
                // Update JSON inside tbl_contentinfo.AppInfoJson using json_set
                using (var NewCMD = NewConnection.CreateCommand())
                {
                    NewCMD.Transaction = NewTransaction;
                    NewCMD.CommandText = @"
                    UPDATE tbl_contentinfo
                    SET AppInfoJson = json_set(
                        AppInfoJson,
                        '$.CONTENT_VERSION', $contentVersion,
                        '$.VERSION_FILE_URI', $versionFileUri
                    )
                    WHERE titleId = $titleId;
                ";
                    NewCMD.Parameters.AddWithValue("$contentVersion", "99.999.999");
                    NewCMD.Parameters.AddWithValue("$versionFileUri", "http://127.0.0.2");
                    NewCMD.Parameters.AddWithValue("$titleId", "PPSA01650");

                    totalRows += NewCMD.ExecuteNonQuery();
                }

                NewTransaction.Commit();
            }
            catch (Exception)
            {
                try
                {
                    LogTextBox.Text += "An error occured, rolling back changes made to app.db\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();
                    NewTransaction.Rollback();
                }
                catch
                {
                    LogTextBox.Text += "Failed to rollback app.db changes!\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();
                }
            }

            LogTextBox.Text += $"app.db patched - {totalRows} changes done.\n";
            LogTextBoxScrollViewer?.ScrollToEnd();

            try
            {
                // Dispose transaction & connection & make sure access to file is released
                NewTransaction.Dispose();
                NewConnection.Close();
                SqliteConnection.ClearPool(NewConnection);
                NewConnection.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex) { LogTextBox.Text += $"WARNING: Could not close the app.db connection.\n{ex.Message}\n"; }

            LogTextBox.Text += "Starting to patch param.json ...\n";
            LogTextBoxScrollViewer?.ScrollToEnd();

            // Patch param.json
            try
            {
                string JSONData = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Cache", "param.json"));
                if (JSONData != null)
                {
                    PS5Param ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData)!;

                    ParamData.ContentVersion = "99.999.999";
                    ParamData.VersionFileUri = "http://127.0.0.2";

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "Cache", "param.json"), RawDataJSON);
                }
                else
                {
                    LogTextBox.Text += "Could not read param.json!\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();
                }
            }
            catch
            {
                LogTextBox.Text += "Failed to modify param.json!\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }

            LogTextBox.Text += "All files patched successfully!\n";

            LogTextBox.Text += "Now uploading all files back ...\n";
            LogTextBoxScrollViewer?.ScrollToEnd();

            // Upload back
            try
            {
                using var conn = new AsyncFtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", int.Parse(PS5FTPPortTextBox.Text));
                conn.Config.EncryptionMode = FtpEncryptionMode.None;
                conn.Config.SslProtocols = SslProtocols.None;
                conn.Config.DataConnectionEncryption = false;

                // Connect
                await conn.Connect();

                try
                {
                    // Upload and replace all files
                    await conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "appinfo.db"), "/system_data/priv/mms/appinfo.db", FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None);
                    await conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "app.db"), "/system_data/priv/mms/app.db", FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None);
                    await conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "param.json"), "/system_data/priv/appmeta/PPSA01650/param.json", FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None);
                    await conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "param.json"), "/user/appmeta/PPSA01650/param.json", FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None);
                }
                catch (Exception)
                {
                    // Fallback to old method if OverwriteInPlace fails
                    // Delete first
                    await conn.DeleteFile("/system_data/priv/mms/appinfo.db");
                    await conn.DeleteFile("/system_data/priv/mms/app.db");
                    await conn.DeleteFile("/system_data/priv/appmeta/PPSA01650/param.json");
                    await conn.DeleteFile("/user/appmeta/PPSA01650/param.json");
                    // Upload new files
                    await conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "appinfo.db"), "/system_data/priv/mms/appinfo.db", FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None);
                    await conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "app.db"), "/system_data/priv/mms/app.db", FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None);
                    await conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "param.json"), "/system_data/priv/appmeta/PPSA01650/param.json", FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None);
                    await conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "param.json"), "/user/appmeta/PPSA01650/param.json", FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None);
                }

                // Disconnect
                await conn.Disconnect();
            }
            catch (Exception)
            {
                LogTextBox.Text += "Could not upload the files back to the PS5, please verify your connection.\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }

            LogTextBox.Text += "Done updating all files on the PS5.\nPress the PS button on the controller and reboot !";
            LogTextBoxScrollViewer?.ScrollToEnd();
        }
        else
        {
            LogTextBox.Text += "Please enter an IP Address and FTP Port first.\n";
            LogTextBoxScrollViewer?.ScrollToEnd();
        }
    }

    private async void AutoReplaceDownload0dat_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PS5IPTextBox.Text) && !string.IsNullOrEmpty(PS5FTPPortTextBox.Text))
        {
            LogTextBox.Clear();
            LogTextBox.Text += "Getting latest download0.dat ...\n";
            LogTextBoxScrollViewer?.ScrollToEnd();

            try
            {
                if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Cache")))
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Cache"));
            }
            catch (Exception ex) { LogTextBox.Text += $"Error while creating the Cache folder!\n{ex.Message}\nTask stopped."; return; }

            try
            {
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "download0.dat")))
                    File.Delete(Path.Combine(Environment.CurrentDirectory, "Cache", "download0.dat"));
            }
            catch (Exception ex) { LogTextBox.Text += $"Error while deleting {Path.Combine(Environment.CurrentDirectory, "Cache", "download0.dat")}!\n{ex.Message}\nTask stopped."; return; }

            try
            {
                // Download download0.dat file (v1.3)
                using (var http = new HttpClient())
                using (var response = await http.GetAsync("http://87.106.5.21/ps5/hb/download0.dat", HttpCompletionOption.ResponseHeadersRead, default))
                {
                    response.EnsureSuccessStatusCode();

                    using var sourceStream = await response.Content.ReadAsStreamAsync(default);
                    using var destinationStream = File.Create(Path.Combine(Environment.CurrentDirectory, "Cache", "download0.dat"));
                    await sourceStream.CopyToAsync(destinationStream, 81920, default);
                }
            }
            catch (Exception ex) { LogTextBox.Text += $"Error while downloading download0.dat !\n{ex.Message}\nTask stopped."; return; }

            LogTextBox.Text += "Retrieved download0.dat. Now replacing on the PS5 ...\n";
            LogTextBoxScrollViewer?.ScrollToEnd();

            try
            {
                // Connect to FTP server and replace
                using var conn = new AsyncFtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", int.Parse(PS5FTPPortTextBox.Text));
                conn.Config.EncryptionMode = FtpEncryptionMode.None;
                conn.Config.SslProtocols = SslProtocols.None;
                conn.Config.DataConnectionEncryption = false;

                // Connect
                await conn.Connect();

                // Check if download0.dat still exists and pass download0datDidNotExist for conn.UploadFile's createRemoteDir = true if not
                bool download0datDidNotExist = true;
                if (await conn.GetObjectInfo("/user/download/PPSA01650/download0.dat") is not null)
                {
                    // Remove the old download0.dat
                    await conn.DeleteFile("/user/download/PPSA01650/download0.dat");
                    download0datDidNotExist = false;
                }

                try
                {
                    // Upload new download0.dat file
                    await conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "download0.dat"), "/user/download/PPSA01650/download0.dat", FtpRemoteExists.OverwriteInPlace, download0datDidNotExist, FtpVerify.None);
                }
                catch (Exception)
                {
                    // Try old method if OverwriteInPlace failed
                    await conn.DeleteFile("/user/download/PPSA01650/download0.dat");
                    await conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "download0.dat"), "/user/download/PPSA01650/download0.dat", FtpRemoteExists.NoCheck, false, FtpVerify.None);
                }

                // Disconnect
                await conn.Disconnect();
            }
            catch (Exception ex)
            {
                LogTextBox.Text += $"Failed to upload & replace download0.dat on the PS5!\n{ex.Message}\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }

            LogTextBox.Text += "Replacing download0.dat succeeded!\n";
            LogTextBoxScrollViewer?.ScrollToEnd();
        }
        else
        {
            LogTextBox.Text += "Please enter an IP Address and FTP Port first.\n";
            LogTextBoxScrollViewer?.ScrollToEnd();
        }
    }

    private async void UploadDownload0dat_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PS5IPTextBox.Text) && !string.IsNullOrEmpty(PS5FTPPortTextBox.Text))
        {
            var datFileFilter = new FileDialogFilter
            {
                Name = "dat File",
                Extensions = ["dat"]
            };
            var OFD = new OpenFileDialog() { Title = "Select a download0.dat file", Filters = { datFileFilter }, AllowMultiple = false };
            var OFDResult = await OFD.ShowAsync(this);

            if (OFDResult != null && OFDResult.Length > 0)
            {
                LogTextBox.Clear();
                LogTextBox.Text += $"Uploading {OFDResult[0]} to PS5, please wait ...\n";
                LogTextBoxScrollViewer?.ScrollToEnd();

                try
                {
                    // Connect to FTP server and replace
                    using var conn = new AsyncFtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", int.Parse(PS5FTPPortTextBox.Text));
                    conn.Config.EncryptionMode = FtpEncryptionMode.None;
                    conn.Config.SslProtocols = SslProtocols.None;
                    conn.Config.DataConnectionEncryption = false;

                    // Connect
                    await conn.Connect();

                    // Check if download0.dat still exists and pass download0datDidNotExist for conn.UploadFile's createRemoteDir = true if not
                    bool download0datDidNotExist = true;
                    if (await conn.GetObjectInfo("/user/download/PPSA01650/download0.dat") is not null)
                    {
                        // Remove the old download0.dat
                        await conn.DeleteFile("/user/download/PPSA01650/download0.dat");
                        download0datDidNotExist = false;
                    }

                    try
                    {
                        // Upload new download0.dat file
                        await conn.UploadFile(OFDResult[0], "/user/download/PPSA01650/download0.dat", FtpRemoteExists.OverwriteInPlace, download0datDidNotExist, FtpVerify.None);
                    }
                    catch (Exception)
                    {
                        // Try old method if OverwriteInPlace failed
                        await conn.DeleteFile("/user/download/PPSA01650/download0.dat");
                        await conn.UploadFile(OFDResult[0], "/user/download/PPSA01650/download0.dat", FtpRemoteExists.NoCheck, false, FtpVerify.None);
                    }

                    // Disconnect
                    await conn.Disconnect();
                }
                catch (Exception ex)
                {
                    LogTextBox.Text += $"Failed to replace download0.dat on the PS5!\n{ex.Message}\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();
                }

                LogTextBox.Text += "Replacing download0.dat succeeded!\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }
        }
        else
        {
            LogTextBox.Text += "Please enter an IP Address and FTP Port first.\n";
            LogTextBoxScrollViewer?.ScrollToEnd();
        }
    }

    private async void InstallYouTubePKG_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PS5IPTextBox.Text))
        {
            // Check if Direct Package Installer V2 is service is active
            bool isActive = false;
            using var NewTcpClient = new TcpClient();
            try
            {
                var NewIAsyncResult = NewTcpClient.BeginConnect(PS5IPTextBox.Text, 12800, null, null);
                bool PortOpen = NewIAsyncResult.AsyncWaitHandle.WaitOne(1000);

                if (!PortOpen)
                {
                    isActive = false;
                }

                NewTcpClient.EndConnect(NewIAsyncResult);
                isActive = true;
            }
            catch (Exception)
            {
                isActive = false;
            }

            // Send PKG if active
            if (isActive)
            {
                LogTextBox.Clear();
                LogTextBox.Text += "YouTube PKG has been send to the PS5.\n";
                LogTextBoxScrollViewer?.ScrollToEnd();

                try
                {
                    string YTPKGURL = "http://87.106.5.21/ps5/hb/UP4381-PPSA01650_00-YOUTUBESIEA00000.pkg";
                    using var NewHttpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
                    var PS5RequestURL = $"http://{PS5IPTextBox.Text}:12800/upload";
                    var Boundary = "----DirectPackageInstallerBoundary";
                    using var NewMultipartFormDataContent = new MultipartFormDataContent(Boundary) { { new StringContent(string.Empty), "\"file\"", "\"\"" } };

                    using var NewMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(YTPKGURL));
                    NewMultipartFormDataContent.Add(new StreamContent(NewMemoryStream), "\"url\"");

                    var Response = await NewHttpClient.PostAsync(PS5RequestURL, NewMultipartFormDataContent);

                    using var NewMS = new MemoryStream();
                    await Response.Content.CopyToAsync(NewMS);

                    var Result = Encoding.UTF8.GetString(NewMS.ToArray());

                    if (Result.Contains("SUCCESS:"))
                    {
                        LogTextBox.Text += "Success! YouTube PKG is now installing.\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }
                    else
                    {
                        LogTextBox.Text += "Failed to install the YouTube PKG!\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }
                }
                catch (Exception ex) { LogTextBox.Text += $"Error while sending the YouTube PKG!\n{ex.Message}\nTask stopped."; return; }
            }
            else
            {
                LogTextBox.Text += "Please enable the Direct Package Installer V2 Service first in etaHEN.\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }
        }
        else
        {
            LogTextBox.Text += "Please enter an IP Address first.\n";
            LogTextBoxScrollViewer?.ScrollToEnd();
        }
    }

    private async void SendetaHENPayload_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PS5IPTextBox.Text) && !string.IsNullOrEmpty(PS5PayloadPortTextBox.Text))
        {
            // Download latest etaHEN to Cache
            LogTextBox.Clear();

            try
            {
                if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Cache")))
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Cache"));
            }
            catch (Exception ex) { LogTextBox.Text += $"Error while creating the Cache folder!\n{ex.Message}\nTask stopped."; return; }

            // Download latest etaHEN-2.5B.bin file
            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "etaHEN-2.5B.bin")))
            {
                LogTextBox.Text += "Getting latest etaHEN v2.5B ...\n";
                LogTextBoxScrollViewer?.ScrollToEnd();

                try
                {
                    using var http = new HttpClient();
                    using var response = await http.GetAsync("https://github.com/etaHEN/etaHEN/releases/download/2.5B/etaHEN-2.5B.bin", HttpCompletionOption.ResponseHeadersRead, default);
                    response.EnsureSuccessStatusCode();

                    using var sourceStream = await response.Content.ReadAsStreamAsync(default);
                    using var destinationStream = File.Create(Path.Combine(Environment.CurrentDirectory, "Cache", "etaHEN-2.5B.bin"));
                    await sourceStream.CopyToAsync(destinationStream, 81920, default);
                }
                catch (Exception ex)
                {
                    LogTextBox.Text += $"An error occured while downloading the etaHEN-2.5B.bin payload.\n{ex.Message}\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();
                }
            }

            // Send etaHEN payload to PS5
            try
            {
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "etaHEN-2.5B.bin")))
                {
                    LogTextBox.Text += "Sending etaHEN-2.5B.bin to the PS5 ...\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();

                    Socket SenderSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        ReceiveTimeout = 3000,
                        SendTimeout = 3000
                    };

                    await SenderSocket.ConnectAsync(new IPEndPoint(IPAddress.Parse(PS5IPTextBox.Text), int.Parse(PS5PayloadPortTextBox.Text)));
                    await SenderSocket.SendFileAsync(Path.Combine(Environment.CurrentDirectory, "Cache", "etaHEN-2.5B.bin"));
                    SenderSocket.Close();

                    LogTextBox.Text += "Payload etaHEN-2.5B.bin send successfully!\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();
                }
                else
                {
                    LogTextBox.Text += "Could not find the downloaded etaHEN-2.5B.bin payload!\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();
                }
            }
            catch (Exception ex)
            {
                LogTextBox.Text += $"An error occured while sending the etaHEN payload.\n{ex.Message}\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }
        }
        else
        {
            LogTextBox.Text += "Please enter an IP Address and Payload Port first.\n";
            LogTextBoxScrollViewer?.ScrollToEnd();
        }
    }

    private async void ReplaceAutoloader_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select an USB drive" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            string Y2JBUpdateLink = "https://github.com/itsPLK/ps5_y2jb_autoloader/releases/latest/download/y2jb_update.zip";
            string DestinationPath = Path.Combine(FBDResult, "y2jb_update.zip");

            try
            {
                using var NewHttpClient = new HttpClient();
                using var NewHttpResponseMessage = await NewHttpClient.GetAsync(Y2JBUpdateLink, HttpCompletionOption.ResponseHeadersRead);
                NewHttpResponseMessage.EnsureSuccessStatusCode();
                using var NewFileStream = new FileStream(DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await NewHttpResponseMessage.Content.CopyToAsync(NewFileStream);
            }
            catch (Exception ex)
            {
                LogTextBox.Text += $"An error occured while downloading {Y2JBUpdateLink} to {DestinationPath}.\n{ex.Message}\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }

            LogTextBox.Text += $"Downloading y2jb_update.zip to {DestinationPath} succeeded!\n";
            LogTextBoxScrollViewer?.ScrollToEnd();
        }
    }

    private async void SendCustomPayload_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PS5IPTextBox.Text) && !string.IsNullOrEmpty(PS5PayloadPortTextBox.Text))
        {
            var binFileFilter = new FileDialogFilter
            {
                Name = "BIN File",
                Extensions = ["bin"]
            };
            var elfFileFilter = new FileDialogFilter
            {
                Name = "ELF File",
                Extensions = ["elf"]
            };
            var jsFileFilter = new FileDialogFilter
            {
                Name = "JavaScript File",
                Extensions = ["js"]
            };

            var OFD = new OpenFileDialog() { Title = "Select a payload", Filters = { binFileFilter, elfFileFilter, jsFileFilter }, AllowMultiple = false };
            var OFDResult = await OFD.ShowAsync(this);

            if (OFDResult != null && OFDResult.Length > 0)
            {
                LogTextBox.Text += $"Sending {OFDResult[0]} to the PS5 ...\n";
                LogTextBoxScrollViewer?.ScrollToEnd();

                Socket SenderSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveTimeout = 3000,
                    SendTimeout = 3000
                };

                await SenderSocket.ConnectAsync(new IPEndPoint(IPAddress.Parse(PS5IPTextBox.Text), int.Parse(PS5PayloadPortTextBox.Text)));
                await SenderSocket.SendFileAsync(OFDResult[0]);
                SenderSocket.Close();

                LogTextBox.Text += $"Payload {OFDResult[0]}  send successfully!\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }
        }
        else
        {
            LogTextBox.Text += "Please enter an IP Address and Payload Port first.\n";
            LogTextBoxScrollViewer?.ScrollToEnd();
        }
    }

    private async void SendLapse_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PS5IPTextBox.Text))
        {
            // Send Lapse payload to PS5
            try
            {
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "PS5", "lapse.js")))
                {
                    LogTextBox.Text += "Sending lapse.js to the PS5 ...\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();

                    Socket SenderSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        ReceiveTimeout = 3000,
                        SendTimeout = 3000
                    };

                    await SenderSocket.ConnectAsync(new IPEndPoint(IPAddress.Parse(PS5IPTextBox.Text), 50000));
                    await SenderSocket.SendFileAsync(Path.Combine(Environment.CurrentDirectory, "Tools", "PS5", "lapse.js"));
                    SenderSocket.Close();

                    LogTextBox.Text += "Payload lapse.js send successfully!\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();
                }
                else
                {
                    LogTextBox.Text += "Could not find the lapse.js payload!\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();
                }
            }
            catch (Exception ex)
            {
                LogTextBox.Text += $"An error occured while sending the lapse.js payload.\n{ex.Message}\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            }
        }
        else
        {
            LogTextBox.Text += "Please enter an IP Address first.\n";
            LogTextBoxScrollViewer?.ScrollToEnd();
        }
    }

}