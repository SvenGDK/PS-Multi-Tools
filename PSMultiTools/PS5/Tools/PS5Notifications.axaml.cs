using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentFTP;
using Microsoft.Data.Sqlite;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using PSMultiTools.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Authentication;

namespace PSMultiTools.PS5.Tools;

public partial class PS5Notifications : Window
{

    public int NotificationsCount = 0;
    public List<string> UserProfiles = [];
    public string SelectedUserProfile = "";

    public bool OtherFW = false; // title_icon seems not to be presented in all fws
    public bool InstallOnAllProfiles = false;

    private readonly string NotificationsDBPath = Path.Combine(Environment.CurrentDirectory, "Cache", "notification.db");
    private readonly string NotificationsDB2Path = Path.Combine(Environment.CurrentDirectory, "Cache", "notification2.db");

    // Shortcuts that will be available
    public List<Classes.Action> WebBrowserJSONActions = [];

    public PS5Notifications()
    {
        InitializeComponent();

        Loaded += PS5Notifications_Loaded;

        // Init SQLite
        SQLitePCL.Batteries_V2.Init();
    }

    private void PS5Notifications_Loaded(object? sender, RoutedEventArgs e)
    {
        // Add pre-defined bookmarks to the list
        WebBrowserJSONActions.Add(new Classes.Action()
        {
            actionName = "PS Multi Tools Host",
            actionType = "DeepLink",
            defaultFocus = true,
            parameters = new Parameters() { actionUrl = "http://X.X.X.X/ps5ex/" }
        });

        WebBrowserJSONActions.Add(new Classes.Action()
        {
            actionName = "Al-Azif",
            actionType = "DeepLink",
            defaultFocus = true,
            parameters = new Parameters() { actionUrl = "https://ithaqua.exploit.menu/" }
        });

        WebBrowserJSONActions.Add(new Classes.Action()
        {
            actionName = "@SvenGDK",
            actionType = "DeepLink",
            defaultFocus = true,
            parameters = new Parameters() { actionUrl = "https://twitter.com/SvenGDK" }
        });

        foreach (var WebBrowserAction in WebBrowserJSONActions)
            WebBrowserShorcutsComboBox.Items.Add(WebBrowserAction);

        // Show the action name
        WebBrowserShorcutsComboBox.SelectedIndex = 0;
    }

    private async void ConnectButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Cache")))
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Cache"));

        UserProfilesComboBox.Items.Clear();

        if (!string.IsNullOrEmpty(IPTextBox.Text) && !string.IsNullOrEmpty(PortTextBox.Text))
        {
            try
            {
                using var conn = new FtpClient(IPTextBox.Text, "anonymous", "anonymous", Convert.ToInt32(PortTextBox.Text));
                // Configurate the FTP connection
                conn.Config.EncryptionMode = FtpEncryptionMode.None;
                conn.Config.SslProtocols = SslProtocols.None;
                conn.Config.DataConnectionEncryption = false;

                // Connect
                conn.Connect();

                // Get notification2.db
                conn.DownloadFile(NotificationsDB2Path, "/system_data/priv/mms/notification2.db", FtpLocalExists.Overwrite);

                // Also get notification.db in case we can't find any user_id
                conn.DownloadFile(NotificationsDBPath, "/system_data/priv/mms/notification.db", FtpLocalExists.Overwrite);

                // Disconnect
                conn.Disconnect();
            }
            catch (Exception)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not get the notification2.db, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }

            // Create a backup that will not be overwritten
            if (File.Exists(NotificationsDB2Path))
            {
                if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "notification2-backup.db")))
                {
                    try
                    {
                        File.Copy(NotificationsDB2Path, Path.Combine(Environment.CurrentDirectory, "Cache", "notification2-backup.db"), false);
                    }
                    catch { }
                }
            }

            // Load values
            try
            {
                using (var conn = new SqliteConnection("Data Source=" + NotificationsDB2Path))
                {
                    conn.Open();

                    var SelectCommand = conn.CreateCommand();
                    SelectCommand.CommandText = "SELECT DISTINCT user_id FROM notification"; // No duplicates

                    // Create a new DataTable and load the values into it
                    using (var DataReader = SelectCommand.ExecuteReader())
                    {
                        var NewDataTable = new DataTable();
                        NewDataTable.Load(DataReader);
                        if (!(NewDataTable.Rows.Count == 0))
                        {
                            // Add each user profile to the UserProfiles list
                            foreach (DataRow Record in NewDataTable.Rows)
                                UserProfiles.Add(Record[0].ToString()!);
                        }
                    }

                    // Get the last notification ID so we can add more
                    SelectCommand.CommandText = "SELECT id FROM notification ORDER BY id DESC LIMIT 1;"; // Get the last notification ID
                    using (var DataReader = SelectCommand.ExecuteReader())
                    {
                        var NewDataTable = new DataTable();
                        NewDataTable.Load(DataReader);
                        if (!(NewDataTable.Rows.Count == 0))
                        {
                            NotificationsCount = Convert.ToInt32(NewDataTable.Rows[0][0].ToString());
                        }
                    }

                    // Check if the table 'notification' contains the column 'title_icon' (Seems to be new on not presented on all fws)
                    SelectCommand.CommandText = string.Format("PRAGMA table_info({0})", "notification");
                    var NewDataReader = SelectCommand.ExecuteReader();
                    int NameIndex = NewDataReader.GetOrdinal("Name");

                    while (NewDataReader.Read())
                    {
                        if (NewDataReader.GetString(NameIndex).Equals("title_icon"))
                        {
                            OtherFW = true;
                        }
                        else
                        {
                            OtherFW = false;
                        }
                    }
                    NewDataReader.Close();

                    // Close & dispose
                    conn.Close();
                    conn.Dispose();
                    SelectCommand.Dispose();
                }

                // Try to get user profiles from notification.db if we can't find them inside notification2.db
                if (UserProfiles.Count == 0)
                {
                    using var conn = new SqliteConnection("Data Source=" + NotificationsDBPath);
                    conn.Open();

                    var SelectCommand = conn.CreateCommand();
                    SelectCommand.CommandText = "SELECT DISTINCT user_id FROM notification"; // No duplicates

                    using (var DataReader = SelectCommand.ExecuteReader())
                    {
                        var NewDataTable = new DataTable();
                        NewDataTable.Load(DataReader);
                        if (!(NewDataTable.Rows.Count == 0))
                        {
                            // Add each user profile to the UserProfiles list
                            foreach (DataRow Record in NewDataTable.Rows)
                                UserProfiles.Add(Record[0].ToString()!);
                        }
                    }

                    // Close & dispose
                    conn.Close();
                    conn.Dispose();
                    SelectCommand.Dispose();
                }

                // Completely close the SQLiteConnection and release access to notification2.db
                SqliteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                var box = MessageBoxManager.GetMessageBoxStandard("Success", "Users and notifications loaded successfully." + Environment.NewLine + "Remember to upload the changes after you're done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
            catch (SqliteException)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not read notification2.db, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }

            // Add profiles to the UserProfilesComboBox
            foreach (var UserProfile in UserProfiles)
                UserProfilesComboBox.Items.Add(UserProfile);

            UploadButton.IsEnabled = true;
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please check your IP and Port", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }

    }

    private void AllUserProfilesCheckBox_Checked(object? sender, RoutedEventArgs e)
    {
        InstallOnAllProfiles = true;
        UserProfilesComboBox.IsEnabled = false;
    }

    private void AllUserProfilesCheckBox_Unchecked(object? sender, RoutedEventArgs e)
    {
        InstallOnAllProfiles = false;
        UserProfilesComboBox.IsEnabled = true;
    }

    private async void AddDebugSettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        // Add to the notification2.db
        if (File.Exists(NotificationsDB2Path))
        {
            try
            {
                using (var conn = new SqliteConnection("Data Source=" + NotificationsDB2Path))
                {
                    conn.Open();

                    // Insert required values
                    var SelectCommand = conn.CreateCommand();
                    int TableRowsAffected = 0;
                    int AffectedRows = 0;

                    if (InstallOnAllProfiles == true)
                    {

                        foreach (var UserProfile in UserProfiles)
                        {

                            if (OtherFW == true)
                            {
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"pssettings:play?function=debug_settings\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"Debug Settings\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL,'')";
                            }
                            else
                            {
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"pssettings:play?function=debug_settings\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"Debug Settings\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL)";
                            }

                            TableRowsAffected = SelectCommand.ExecuteNonQuery();

                            if (TableRowsAffected == 1)
                            {
                                NotificationsCount += 1;
                                AffectedRows += 1;
                            }
                        }

                        if (AffectedRows == UserProfiles.Count)
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "The Debug Menu has been added to the notifications for all users." + Environment.NewLine + "Remember to upload the changes after you're done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowAsync();
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not add the Debug Menu for every user.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }
                    else
                    {

                        if (OtherFW == true)
                        {
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"pssettings:play?function=debug_settings\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"Debug Settings\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL,'')";
                        }
                        else
                        {
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"pssettings:play?function=debug_settings\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"Debug Settings\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL)";
                        }

                        TableRowsAffected = SelectCommand.ExecuteNonQuery();

                        if (TableRowsAffected == 1)
                        {
                            NotificationsCount += 1;
                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "The Debug Menu has been added to the notifications." + Environment.NewLine + "Remember to upload the changes after you're done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowAsync();
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not add the Debug Menu.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }

                    // Close & dispose
                    conn.Close();
                    conn.Dispose();
                    SelectCommand.Dispose();
                }

                // Completely close the SQLiteConnection and release access to notification2.db
                SqliteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            catch (SqliteException)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not read notification2.db, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }

    }

    private async void AddWebBrowserButton_Click(object? sender, RoutedEventArgs e)
    {

        // Create the notification rawData JSON
        var NewPS5Notification = new PS5Notification()
        {
            bundleName = "download",
            channelType = "Downloads",
            isAnonymous = true,
            isImmediate = true,
            platformViews = new PlatformViews() { previewDisabled = new PreviewDisabled() { viewData = new ViewData() { icon = new Classes.Icon() { parameters = new Parameters() { icon = "download" }, @type = "" }, message = new Message() { body = "" } } } },
            priority = 1,
            toastOverwriteType = "No",
            useCaseId = "NUC249",
            viewData = new ViewData() { actions = WebBrowserJSONActions, icon = new Classes.Icon() { parameters = new Parameters() { icon = "localasset_system_software_default" }, @type = "Predefined" }, message = new Message() { body = "" }, subMessage = new SubMessage() { body = "Web browser" } },
            viewTemplateType = "InteractiveToastTemplateB"
        };

        // Serialize PS5Notification to JSON
        string rawDataJSON = JsonConvert.SerializeObject(NewPS5Notification);

        // Add to the notification2.db
        if (File.Exists(NotificationsDB2Path))
        {
            try
            {
                using (var conn = new SqliteConnection("Data Source=" + NotificationsDB2Path))
                {
                    conn.Open();

                    // Insert required values
                    var SelectCommand = conn.CreateCommand();
                    int TableRowsAffected = 0;
                    int AffectedRows = 0;

                    if (InstallOnAllProfiles == true)
                    {

                        foreach (var UserProfile in UserProfiles)
                        {

                            if (OtherFW == true)
                            {
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + UserProfile + ",0,1,NULL,NULL,'2871025089','','xxx','','','','" + rawDataJSON + @"',NULL,'{""titleInfos"":[{""icon"":"""",""name"":"""",""titleKey"":""{\""objectType\"":\""TitleByTitle\"",\""titleIds\"":[\""\""]}""}],""userInfos"":[],""completed"":true,""updatedDateTime"":""""}',NULL,NULL)";
                            }
                            else
                            {
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + UserProfile + ",0,1,NULL,NULL,'2871025089','','xxx','','','','" + rawDataJSON + @"',NULL,'{""titleInfos"":[{""icon"":"""",""name"":"""",""titleKey"":""{\""objectType\"":\""TitleByTitle\"",\""titleIds\"":[\""\""]}""}],""userInfos"":[],""completed"":true,""updatedDateTime"":""""}',NULL)";
                            }

                            TableRowsAffected = SelectCommand.ExecuteNonQuery();

                            if (TableRowsAffected == 1)
                            {
                                NotificationsCount += 1;
                                AffectedRows += 1;
                            }
                        }

                        if (AffectedRows == UserProfiles.Count)
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "The Web Browser has been added to the notifications for all users." + Environment.NewLine + "Remember to upload the changes after you're done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowAsync();
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not add the Web Browser for every user.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }

                    else
                    {

                        if (OtherFW == true)
                        {
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + SelectedUserProfile + ",0,1,NULL,NULL,'2871025089','','xxx','','','','" + rawDataJSON + @"',NULL,'{""titleInfos"":[{""icon"":"""",""name"":"""",""titleKey"":""{\""objectType\"":\""TitleByTitle\"",\""titleIds\"":[\""\""]}""}],""userInfos"":[],""completed"":true,""updatedDateTime"":""""}',NULL,NULL)";
                        }
                        else
                        {
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + SelectedUserProfile + ",0,1,NULL,NULL,'2871025089','','xxx','','','','" + rawDataJSON + @"',NULL,'{""titleInfos"":[{""icon"":"""",""name"":"""",""titleKey"":""{\""objectType\"":\""TitleByTitle\"",\""titleIds\"":[\""\""]}""}],""userInfos"":[],""completed"":true,""updatedDateTime"":""""}',NULL)";
                        }

                        TableRowsAffected = SelectCommand.ExecuteNonQuery();

                        if (TableRowsAffected == 1)
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "The Web Browser has been added to the notifications." + Environment.NewLine + "Remember to upload the changes after you're done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowAsync();
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not add the Web Browser.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }

                    // Close & dispose
                    conn.Close();
                    conn.Dispose();
                    SelectCommand.Dispose();
                }

                // Completely close the SQLiteConnection and release access to notification2.db
                SqliteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            catch (SqliteException)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not read notification2.db, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }

    }

    private async void AddSaveDataManagerButton_Click(object? sender, RoutedEventArgs e)
    {

        string CustomActionURL = "pssettings:play?function=savedata";
        string CustomActionName = "Saved Data Management";

        // Add to the notification2.db
        if (File.Exists(NotificationsDB2Path))
        {
            try
            {
                using (var conn = new SqliteConnection("Data Source=" + NotificationsDB2Path))
                {
                    conn.Open();

                    // Insert required values
                    var SelectCommand = conn.CreateCommand();
                    int TableRowsAffected = 0;
                    int AffectedRows = 0;

                    if (InstallOnAllProfiles == true)
                    {

                        foreach (var UserProfile in UserProfiles)
                        {

                            if (OtherFW == true)
                            {
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"" + CustomActionURL + "\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"" + CustomActionName + "\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL,'')";
                            }
                            else
                            {
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"" + CustomActionURL + "\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"" + CustomActionName + "\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL)";
                            }

                            TableRowsAffected = SelectCommand.ExecuteNonQuery();
                            if (TableRowsAffected == 1)
                            {
                                NotificationsCount += 1;
                                AffectedRows += 1;
                            }
                        }

                        if (AffectedRows == UserProfiles.Count)
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "The custom action " + CustomActionName + " has been added to the notifications for all users." + Environment.NewLine + "Remember to upload the changes after you're done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowAsync();
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not add the custom action " + CustomActionName + " for every user.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }
                    else
                    {

                        if (OtherFW == true)
                        {
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"" + CustomActionURL + "\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"" + CustomActionName + "\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL,'')";
                        }
                        else
                        {
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"" + CustomActionURL + "\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"" + CustomActionName + "\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL)";
                        }

                        TableRowsAffected = SelectCommand.ExecuteNonQuery();

                        if (TableRowsAffected == 1)
                        {
                            NotificationsCount += 1;
                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "The custom action " + CustomActionName + " has been added to the notifications." + Environment.NewLine + "Remember to upload the changes after you're done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowAsync();
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not add the custom action " + CustomActionName + ".", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }

                    // Close & dispose
                    conn.Close();
                    conn.Dispose();
                    SelectCommand.Dispose();
                }

                // Completely close the SQLiteConnection and release access to notification2.db
                SqliteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            catch (SqliteException)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not read notification2.db, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }

    }

    private async void AddSaveDataManagerPS4Button_Click(object? sender, RoutedEventArgs e)
    {

        string CustomActionURL = "pssettings:play?function=savedata_ps4";
        string CustomActionName = "Saved Data Management PS4";

        // Add to the notification2.db
        if (File.Exists(NotificationsDB2Path))
        {
            try
            {
                using (var conn = new SqliteConnection("Data Source=" + NotificationsDB2Path))
                {
                    conn.Open();

                    // Insert required values
                    var SelectCommand = conn.CreateCommand();
                    int TableRowsAffected = 0;
                    int AffectedRows = 0;

                    if (InstallOnAllProfiles == true)
                    {

                        foreach (var UserProfile in UserProfiles)
                        {

                            if (OtherFW == true)
                            {
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"" + CustomActionURL + "\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"" + CustomActionName + "\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL,'')";
                            }
                            else
                            {
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"" + CustomActionURL + "\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"" + CustomActionName + "\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL)";
                            }

                            TableRowsAffected = SelectCommand.ExecuteNonQuery();
                            if (TableRowsAffected == 1)
                            {
                                NotificationsCount += 1;
                                AffectedRows += 1;
                            }
                        }

                        if (AffectedRows == UserProfiles.Count)
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "The custom action " + CustomActionName + " has been added to the notifications for all users." + Environment.NewLine + "Remember to upload the changes after you're done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowAsync();
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not add the custom action " + CustomActionName + " for every user.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }
                    else
                    {

                        if (OtherFW == true)
                        {
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"" + CustomActionURL + "\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"" + CustomActionName + "\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL,'')";
                        }
                        else
                        {
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"" + CustomActionURL + "\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"" + CustomActionName + "\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL)";
                        }

                        TableRowsAffected = SelectCommand.ExecuteNonQuery();

                        if (TableRowsAffected == 1)
                        {
                            NotificationsCount += 1;
                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "The custom action " + CustomActionName + " has been added to the notifications." + Environment.NewLine + "Remember to upload the changes after you're done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowAsync();
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not add the custom action " + CustomActionName + ".", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }

                    // Close & dispose
                    conn.Close();
                    conn.Dispose();
                    SelectCommand.Dispose();
                }

                // Completely close the SQLiteConnection and release access to notification2.db
                SqliteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            catch (SqliteException)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not read notification2.db, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }

    }

    private void UserProfilesComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (UserProfilesComboBox.SelectedItem is not null)
        {
            SelectedUserProfile = UserProfilesComboBox.SelectedItem.ToString()!;
        }
    }

    private void OtherActionsComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (OtherActionsComboBox.SelectedItem is not null)
        {
            ComboBoxItem SelectedComboBoxItem = (ComboBoxItem)OtherActionsComboBox.SelectedItem;
            CustomDefinedActionTextBox.Text = SelectedComboBoxItem.Content!.ToString();
        }
    }

    private void RemoveShortcutButton_Click(object? sender, RoutedEventArgs e)
    {
        if (WebBrowserShorcutsComboBox.SelectedItem is not null)
        {
            WebBrowserJSONActions.Remove((Classes.Action)WebBrowserShorcutsComboBox.SelectedItem);
            WebBrowserShorcutsComboBox.Items.Remove(WebBrowserShorcutsComboBox.SelectedItem);
        }
    }

    private async void AddShortcutButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ShortcutNameTextBox.Text) & !string.IsNullOrEmpty(ShortcutLinkTextBox.Text))
        {
            WebBrowserJSONActions.Add(new Classes.Action()
            {
                actionName = ShortcutNameTextBox.Text,
                actionType = "DeepLink",
                defaultFocus = true,
                parameters = new Parameters() { actionUrl = ShortcutLinkTextBox.Text }
            });
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "New shortcut for the Web Browser has been added." + Environment.NewLine + "Remember to upload the changes after you're done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please fill in the required informations.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void AddCustomActionButton_Click(object? sender, RoutedEventArgs e)
    {

        string CustomActionURL = CustomDefinedActionTextBox.Text!;
        string CustomActionName = CustomActionNameTextBox.Text!;

        // Add to the notification2.db
        if (File.Exists(NotificationsDB2Path))
        {
            try
            {
                using (var conn = new SqliteConnection("Data Source=" + NotificationsDB2Path))
                {
                    conn.Open();

                    // Insert required values
                    var SelectCommand = conn.CreateCommand();
                    int TableRowsAffected = 0;
                    int AffectedRows = 0;

                    if (InstallOnAllProfiles == true)
                    {

                        foreach (var UserProfile in UserProfiles)
                        {

                            if (OtherFW == true)
                            {
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"" + CustomActionURL + "\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"" + CustomActionName + "\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL,'')";
                            }
                            else
                            {
                                SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + UserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"" + CustomActionURL + "\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"" + CustomActionName + "\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL)";
                            }

                            TableRowsAffected = SelectCommand.ExecuteNonQuery();
                            if (TableRowsAffected == 1)
                            {
                                NotificationsCount += 1;
                                AffectedRows += 1;
                            }
                        }

                        if (AffectedRows == UserProfiles.Count)
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "The custom action " + CustomActionName + " has been added to the notifications for all users." + Environment.NewLine + "Remember to upload the changes after you're done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowAsync();
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not add the custom action " + CustomActionName + " for every user.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }
                    else
                    {

                        if (OtherFW == true)
                        {
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"" + CustomActionURL + "\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"" + CustomActionName + "\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL,'')";
                        }
                        else
                        {
                            SelectCommand.CommandText = "INSERT or REPLACE INTO notification VALUES (" + (NotificationsCount + 1).ToString() + "," + SelectedUserProfile + ",0,1,NULL,NULL,'1740177210','','xxx','','','','{\"bundleName\":\"\",\"channelType\":\"Downloads\",\"isAnonymous\":true,\"isImmediate\":false,\"platformViews\":{\"previewDisabled\":{\"viewData\":{\"icon\":{\"parameters\":{\"icon\":\"download\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"}}}},\"priority\":1,\"toastOverwriteType\":\"No\",\"useCaseId\":\"NUC249\",\"viewData\":{\"actions\":[{\"actionName\":\"Enter Debug Menu\",\"actionType\":\"DeepLink\",\"defaultFocus\":true,\"parameters\":{\"actionUrl\":\"" + CustomActionURL + "\"}}],\"icon\":{\"parameters\":{\"icon\":\"localasset_system_software_default\"},\"type\":\"Predefined\"},\"message\":{\"body\":\"\"},\"subMessage\":{\"body\":\"" + CustomActionName + "\"}},\"viewTemplateType\":\"InteractiveToastTemplateB\"}',NULL,'{\"titleInfos\":[],\"userInfos\":[],\"completed\":true,\"updatedDateTime\":\"2022-10-08T17:16:03.512Z\"}',NULL)";
                        }

                        TableRowsAffected = SelectCommand.ExecuteNonQuery();

                        if (TableRowsAffected == 1)
                        {
                            NotificationsCount += 1;
                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "The custom action " + CustomActionName + " has been added to the notifications." + Environment.NewLine + "Remember to upload the changes after you're done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowAsync();
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not add the custom action " + CustomActionName + ".", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }

                    // Close & dispose
                    conn.Close();
                    conn.Dispose();
                    SelectCommand.Dispose();
                }

                // Completely close the SQLiteConnection and release access to notification2.db
                SqliteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            catch (SqliteException)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not read notification2.db, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }

    }

    private async void UploadButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(IPTextBox.Text) && !string.IsNullOrEmpty(PortTextBox.Text))
        {
            try
            {
                using var conn = new FtpClient(IPTextBox.Text, "anonymous", "anonymous", Convert.ToInt32(PortTextBox.Text));
                // Configurate the FTP connection
                conn.Config.EncryptionMode = FtpEncryptionMode.None;
                conn.Config.SslProtocols = SslProtocols.None;
                conn.Config.DataConnectionEncryption = false;

                // Connect
                conn.Connect();

                // Remove the old notification2.db (overwrite is not implemented yet) to upload the updated one
                conn.DeleteFile("/system_data/priv/mms/notification2.db");

                // Upload updated notification2.db
                var NewFTPStatus = conn.UploadFile(NotificationsDB2Path, "/system_data/priv/mms/notification2.db", FtpRemoteExists.NoCheck);

                if (NewFTPStatus == FtpStatus.Success)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Info", "Notifications on the console have been updated." + Environment.NewLine + "Check your notifications on the PS5.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                else if (NewFTPStatus == FtpStatus.Failed)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not update the notifications on the console." + Environment.NewLine + "Please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }

                // Disconnect
                conn.Disconnect();
            }
            catch (Exception)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not upload the notification2.db, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please check your IP and Port", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

}