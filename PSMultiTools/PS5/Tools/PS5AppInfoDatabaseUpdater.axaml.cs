using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentFTP;
using Microsoft.Data.Sqlite;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.IO;
using System.Security.Authentication;

namespace PSMultiTools.PS5.Tools;

public partial class PS5AppInfoDatabaseUpdater : Window
{

    public string ConsoleIP = "";
    public string ConsolePort = "";

    public PS5AppInfoDatabaseUpdater()
    {
        InitializeComponent();

        // Init SQLite
        SQLitePCL.Batteries_V2.Init();
    }

    private async void UpdateLocalFile_Clicked(object? sender, RoutedEventArgs e)
    {
        var isoFileFilter = new FileDialogFilter
        {
            Name = "SQLite Database File",
            Extensions = ["db"]
        };
        var OFD = new OpenFileDialog() { Title = "Select an appinfo.db file", Filters = { isoFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            UpdateDBFile(OFDResult[0]);
        }
    }

    private async void UpdateUsingFTP_Clicked(object? sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Cache")))
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Cache"));

        LogTextBox.Text += "Connecting to PS5...";

        try
        {
            // Configurate the FTP connection
            using var conn = new FtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort));
            conn.Config.EncryptionMode = FtpEncryptionMode.None;
            conn.Config.SslProtocols = SslProtocols.None;
            conn.Config.DataConnectionEncryption = false;

            // Connect
            conn.Connect();

            LogTextBox.Text += "\nConnected! Getting /system_data/priv/mms/appinfo.db";

            // Get appinfo.db
            conn.DownloadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "appinfo.db"), "/system_data/priv/mms/appinfo.db", FtpLocalExists.Overwrite);

            // Disconnect
            conn.Disconnect();
        }
        catch (Exception)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not get the app.db, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }

        LogTextBox.Text += $"\nSaved {Path.Combine(Environment.CurrentDirectory, "Cache", "appinfo.db")}";

        try
        {
            string BackupFileName = "appinfo_backup.db";

            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "appinfo.db")))
            {
                LogTextBox.Text += "Error: appinfo.db file not found!";
                return;
            }

            // Create a backup
            LogTextBox.Text += "Creating backup...";

            File.Copy(Path.Combine(Environment.CurrentDirectory, "Cache", "appinfo.db"), Path.Combine(Environment.CurrentDirectory, "Cache", BackupFileName), true);
            LogTextBox.Text += $"\nBackup created: {Path.Combine(Environment.CurrentDirectory, "Cache", BackupFileName)}";

            // Update appinfo.db
            using (var conn = new SqliteConnection($"Data Source={Path.Combine(Environment.CurrentDirectory, "Cache", "appinfo.db")}"))
            {
                conn.Open();

                var SelectCommand = conn.CreateCommand();
                SelectCommand.CommandText = @"
                    SELECT key, val 
                    FROM tbl_appinfo 
                    WHERE titleId = 'PPSA01650' 
                    AND key IN ('CONTENT_VERSION', 'VERSION_FILE_URI')
                ";

                var SelectResults = new System.Collections.Generic.List<(string key, string val)>();
                using (var CommandReader = SelectCommand.ExecuteReader())
                {
                    while (CommandReader.Read())
                    {
                        SelectResults.Add((CommandReader.GetString(0), CommandReader.GetString(1)));
                    }
                }

                if (SelectResults.Count != 2)
                {
                    LogTextBox.Text += $"Error: Expected 2 keys but found {SelectResults.Count}";
                    LogTextBox.Text += "Required keys: CONTENT_VERSION, VERSION_FILE_URI";
                    LogTextBox.Text += "Found keys: [" + string.Join(", ", SelectResults.ConvertAll(r => r.key)) + "]";
                    conn.Close();
                }

                LogTextBox.Text += "All required keys found. Proceeding with updates...\n";

                SelectCommand.CommandText = @"
                    UPDATE tbl_appinfo 
                    SET val = '99.999.999'
                    WHERE titleId = 'PPSA01650' 
                    AND key = 'CONTENT_VERSION'
                ";
                int rowsAffected = SelectCommand.ExecuteNonQuery();
                LogTextBox.Text += $"Updated CONTENT_VERSION (rows affected: {rowsAffected})";

                SelectCommand.CommandText = @"
                    UPDATE tbl_appinfo 
                    SET val = 'http://127.0.0.2'
                    WHERE titleId = 'PPSA01650'
                    AND key = 'VERSION_FILE_URI'
                ";
                rowsAffected = SelectCommand.ExecuteNonQuery();
                LogTextBox.Text += $"Updated VERSION_FILE_URI (rows affected: {rowsAffected})";

                LogTextBox.Text += "\nVerifying changes...";
                SelectCommand.CommandText = @"
                    SELECT key, val 
                    FROM tbl_appinfo 
                    WHERE titleId = 'PPSA01650' 
                    AND key IN ('CONTENT_VERSION', 'VERSION_FILE_URI')
                ";

                using (var CommandReader = SelectCommand.ExecuteReader())
                {
                    while (CommandReader.Read())
                    {
                        LogTextBox.Text += $"  {CommandReader.GetString(0)}: {CommandReader.GetString(1)}";
                    }
                }

                // Close & dispose
                conn.Close();
                conn.Dispose();
                SelectCommand.Dispose();
                SqliteConnection.ClearAllPools();
            }
        }
        catch (SqliteException)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not modify appinfo.db.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }

        LogTextBox.Text += "\nChanges saved to appinfo.db";
        LogTextBox.Text += "\nOriginal backed up to appinfo_backup.db";
        LogTextBox.Text += "\nUploading new appinfo.db back to PS5...";

        // Upload back
        try
        {
            // Configurate the FTP connection
            using var conn = new FtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort));
            conn.Config.EncryptionMode = FtpEncryptionMode.None;
            conn.Config.SslProtocols = SslProtocols.None;
            conn.Config.DataConnectionEncryption = false;

            // Connect
            conn.Connect();

            // Remove the old appinfo.db
            conn.DeleteFile("/system_data/priv/mms/appinfo.db");

            // Get appinfo.db
            conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "appinfo.db"), "/system_data/priv/mms/appinfo.db", FtpRemoteExists.NoCheck);

            // Disconnect
            conn.Disconnect();
        }
        catch (Exception)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not update the appinfo.db on the PS5, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }

        LogTextBox.Text += "\nDone updating app.db on the PS5.";

        var box2 = MessageBoxManager.GetMessageBoxStandard("Success!", "Done updating app.db on the PS5! A backup can be found at " + Path.Combine(Environment.CurrentDirectory, "Cache", "appinfo_backup.db"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
        await box2.ShowWindowAsync();
    }

    private async void UpdateDBFile(string DatabaseFile)
    {
        string BackupFileName = "appinfo_backup.db";

        if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Cache")))
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Cache"));

        if (!File.Exists(DatabaseFile))
        {
            LogTextBox.Text += "Error: appinfo.db file not found!";
            return;
        }

        // Create a backup
        LogTextBox.Text += "Creating backup...";
        File.Copy(DatabaseFile, Path.Combine(Environment.CurrentDirectory, "Cache", BackupFileName), true);
        LogTextBox.Text += "Backup created: appinfo_backup.db";

        // Update appinfo.db
        using (var conn = new SqliteConnection($"Data Source={DatabaseFile}"))
        {
            conn.Open();

            var SelectCommand = conn.CreateCommand();
            SelectCommand.CommandText = @"
                    SELECT key, val 
                    FROM tbl_appinfo 
                    WHERE titleId = 'PPSA01650' 
                    AND key IN ('CONTENT_VERSION', 'VERSION_FILE_URI')
                ";

            var SelectResults = new System.Collections.Generic.List<(string key, string val)>();
            using (var CommandReader = SelectCommand.ExecuteReader())
            {
                while (CommandReader.Read())
                {
                    SelectResults.Add((CommandReader.GetString(0), CommandReader.GetString(1)));
                }
            }

            if (SelectResults.Count != 2)
            {
                LogTextBox.Text += $"Error: Expected 2 keys but found {SelectResults.Count}";
                LogTextBox.Text += "Required keys: CONTENT_VERSION, VERSION_FILE_URI";
                LogTextBox.Text += "Found keys: [" + string.Join(", ", SelectResults.ConvertAll(r => r.key)) + "]";
                conn.Close();
            }

            LogTextBox.Text += "All required keys found. Proceeding with updates...\n";

            SelectCommand.CommandText = @"
                    UPDATE tbl_appinfo 
                    SET val = '99.999.999'
                    WHERE titleId = 'PPSA01650' 
                    AND key = 'CONTENT_VERSION'
                ";
            int rowsAffected = SelectCommand.ExecuteNonQuery();
            LogTextBox.Text += $"Updated CONTENT_VERSION (rows affected: {rowsAffected})";

            SelectCommand.CommandText = @"
                    UPDATE tbl_appinfo 
                    SET val = 'http://127.0.0.2'
                    WHERE titleId = 'PPSA01650'
                    AND key = 'VERSION_FILE_URI'
                ";
            rowsAffected = SelectCommand.ExecuteNonQuery();
            LogTextBox.Text += $"Updated VERSION_FILE_URI (rows affected: {rowsAffected})";

            LogTextBox.Text += "\nVerifying changes...";
            SelectCommand.CommandText = @"
                    SELECT key, val 
                    FROM tbl_appinfo 
                    WHERE titleId = 'PPSA01650' 
                    AND key IN ('CONTENT_VERSION', 'VERSION_FILE_URI')
                ";

            using (var CommandReader = SelectCommand.ExecuteReader())
            {
                while (CommandReader.Read())
                {
                    LogTextBox.Text += $"  {CommandReader.GetString(0)}: {CommandReader.GetString(1)}";
                }
            }

            // Close & dispose
            conn.Close();
            conn.Dispose();
            SelectCommand.Dispose();
            SqliteConnection.ClearAllPools();
        }

        LogTextBox.Text += "\nChanges saved to appinfo.db";
        LogTextBox.Text += "Original backed up to appinfo_backup.db";

        var box = MessageBoxManager.GetMessageBoxStandard("Success!", "Done updating app.db. ", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
        await box.ShowWindowAsync();
    }

}