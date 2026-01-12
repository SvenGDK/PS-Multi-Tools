using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentFTP;
using FluentFTP.Exceptions;
using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace PSMultiTools.PS5.Tools.WebSrv;

public partial class WebSrvHomebrewManager : Window
{

    private HomebrewListViewItem SelectedHomebrewItem = default;
    private int PS5Port = 0;
    private readonly FtpConfig NewFtpConfig = new() { EncryptionMode = FtpEncryptionMode.None, SslProtocols = SslProtocols.None, DataConnectionEncryption = false, ValidateAnyCertificate = true };

    private struct HomebrewListViewItem
    {
        public string HomebrewPath { get; set; }
    }

    public WebSrvHomebrewManager()
    {
        InitializeComponent();

        HomebrewImage.PointerPressed += HomebrewImage_PointerPressed;
        InstalledHomebrewListBox.SelectionChanged += InstalledHomebrewListView_SelectionChanged;
    }

    private async void ConnectButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PS5IPTextBox.Text))
        {

            var NewFTPConfig = new FtpConfig() { ConnectTimeout = 3000, RetryAttempts = 1 };

            if (ConnectButton.Content!.ToString() == "Connect")
            {
                try
                {
                    using (var conn = new FtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", 1337, NewFTPConfig))
                    {
                        // Check connection on port 1337
                        conn.Connect(false);
                        PS5Port = 1337;
                        conn.Disconnect();
                    }

                    ListHomebrew();
                    ConnectButton.Content = "Disconnect";
                }
                catch (TimeoutException)
                {
                    try
                    {
                        using (var conn = new FtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", 2121, NewFTPConfig))
                        {
                            // Check connection on port 2121
                            conn.Connect(false);
                            PS5Port = 2121;
                            conn.Disconnect();
                        }

                        ListHomebrew();
                        ConnectButton.Content = "Disconnect";
                    }
                    catch (TimeoutException)
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error connecting to the PS5", "Could not connect to the PS5's FTP server. Please verify your IP and if the FTP server is running on the PS5.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }
                }
            }
            else
            {
                // Disconnect
                try
                {
                    using (var conn = new FtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", PS5Port))
                    {
                        if (conn.IsConnected)
                        {
                            conn.Disconnect();
                        }
                    }
                    ConnectButton.Content = "Connect";
                }
                catch (FtpException ex)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Ignorable Error", "Could not disconnect from the FTP server." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void ListHomebrew()
    {
        InstalledHomebrewListBox.Items.Clear();

        try
        {
            using var NewFtpClient = new AsyncFtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", PS5Port, NewFtpConfig);

            // Connect
            await NewFtpClient.Connect();

            // List directory
            foreach (var FTPItem in await NewFtpClient.GetListing("/data/homebrew"))
            {
                if (FTPItem.FullName.EndsWith("homebrew.js"))
                {
                    var NewHomebrewListViewItem = new HomebrewListViewItem() { HomebrewPath = Path.GetDirectoryName(FTPItem.FullName)! };
                    InstalledHomebrewListBox.Items.Add(NewHomebrewListViewItem);
                }
            }

            // Disonnect
            await NewFtpClient.Disconnect();
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private async void GetHomebrewInfo(string HomebrewFullPath)
    {

        using var NewFtpClient = new AsyncFtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", PS5Port, NewFtpConfig);

        // Connect
        await NewFtpClient.Connect();

        if (await NewFtpClient.FileExists(HomebrewFullPath + "/homebrew.js"))
        {

            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js")))
            {
                File.Delete(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js"));
            }

            // Get the homebrew.js file
            await NewFtpClient.DownloadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js"), HomebrewFullPath + "/homebrew.js", FtpLocalExists.Overwrite);

            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js")))
            {

                string PayloadFilePath = "";
                string PayloadArgs = "";
                string PayloadEnvVars = "";
                string PayloadROMDir = "";
                string PayloadMediaDir = "";
                string PayloadName = "";
                string PayloadDescription = "";

                // Get homebrew info
                foreach (string Line in File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js")))
                {
                    if (Line.Contains("const PAYLOAD"))
                    {
                        PayloadFilePath = Line.Split('+')[1].Replace("'", "").Replace(";", "").Trim();
                    }
                    if (Line.Contains("const ARGS"))
                    {
                        PayloadArgs = Line.Split('=')[1].Replace("[", "").Replace("]", "").Replace(";", "").Trim();
                    }
                    if (Line.Contains("const ENVVARS"))
                    {
                        PayloadEnvVars = Line.Split('=')[1].Replace(";", "").Trim();
                    }
                    if (Line.Contains("const ROMDIR"))
                    {
                        PayloadROMDir = Line.Split('+')[1].Replace("'", "").Replace(";", "").Trim();
                    }
                    if (Line.Contains("const MEDIADIR"))
                    {
                        PayloadMediaDir = Line.Split('+')[1].Replace("'", "").Replace(";", "").Trim();
                    }

                    if (Line.Contains("mainText:"))
                    {
                        PayloadName = Line.Split(':')[1].Replace("\"", "").Replace(",", "").Trim();
                    }
                    if (Line.Contains("secondaryText:"))
                    {
                        PayloadDescription = Line.Split(':')[1].Replace("'", "").Replace(",", "").Trim();
                    }
                }

                // Show info in TextBoxes
                HomebrewPayloadPathTextBox.Text = PayloadFilePath;
                HomebrewPayloadArgumentsTextBox.Text = PayloadArgs;
                HomebrewPayloadEnvironmentVariablesTextBox.Text = PayloadEnvVars;
                HomebrewPayloadROMDirectoryTextBox.Text = PayloadROMDir;
                HomebrewPayloadMediaDirectoryTextBox.Text = PayloadMediaDir;
                HomebrewPayloadNameTextBox.Text = PayloadName;
                HomebrewPayloadDescriptionTextBox.Text = PayloadDescription;

                // Show or hide buttons
                if (!string.IsNullOrEmpty(PayloadROMDir))
                {
                    ManageROMsButton.IsVisible = true;
                }
                else
                {
                    ManageROMsButton.IsVisible = false;
                }
                if (!string.IsNullOrEmpty(PayloadMediaDir))
                {
                    ManageMediaButton.IsVisible = true;
                }
                else
                {
                    ManageMediaButton.IsVisible = false;
                }

            }

        }

        if (await NewFtpClient.FileExists(HomebrewFullPath + "/sce_sys/icon0.png"))
        {
            try
            {
                // Set homebrew icon
                var TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("ftp://" + PS5IPTextBox.Text + ":" + PS5Port.ToString() + HomebrewFullPath + "/sce_sys/icon0.png"));
                HomebrewImage.Source = Utils.AnyBitmapToIImage(TempBitmapImage);
            }
            catch (Exception)
            {
            }
        }
        else
        {
            HomebrewImage.Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/nothing.png")));
        }

        // Disonnect
        await NewFtpClient.Disconnect();
    }

    private void InstalledHomebrewListView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (InstalledHomebrewListBox.SelectedItem is not null && !string.IsNullOrEmpty(PS5IPTextBox.Text))
        {
            SelectedHomebrewItem = (HomebrewListViewItem)InstalledHomebrewListBox.SelectedItem;
            GetHomebrewInfo(SelectedHomebrewItem.HomebrewPath);
        }
    }

    private async void SaveChangesButton_Click(object? sender, RoutedEventArgs e)
    {

        var NewJSLines = new List<string>();

        // Payload File Path
        if (!string.IsNullOrEmpty(HomebrewPayloadPathTextBox.Text))
        {
            foreach (string Line in File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js")))
            {
                if (Line.Contains("const PAYLOAD"))
                {
                    NewJSLines.Add("const PAYLOAD = window.workingDir + '" + HomebrewPayloadPathTextBox.Text + "';");
                }
                else
                {
                    NewJSLines.Add(Line);
                }
            }
            // Save
            File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js"), [.. NewJSLines], Encoding.UTF8);
            NewJSLines = [];
        }

        // Payload Title
        if (!string.IsNullOrEmpty(HomebrewPayloadNameTextBox.Text))
        {
            foreach (string Line in File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js")))
            {
                if (Line.Contains("mainText:"))
                {
                    NewJSLines.Add("mainText: " + "\"" + HomebrewPayloadNameTextBox.Text + "\",");
                }
                else
                {
                    NewJSLines.Add(Line);
                }
            }
            // Save
            File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js"), [.. NewJSLines], Encoding.UTF8);
            NewJSLines = [];
        }

        // Payload Description
        if (!string.IsNullOrEmpty(HomebrewPayloadDescriptionTextBox.Text))
        {
            foreach (string Line in File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js")))
            {
                if (Line.Contains("secondaryText:"))
                {
                    NewJSLines.Add("secondaryText: " + "'" + HomebrewPayloadDescriptionTextBox.Text + "',");
                }
                else
                {
                    NewJSLines.Add(Line);
                }
            }
            // Save
            File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js"), [.. NewJSLines], Encoding.UTF8);
            NewJSLines = [];
        }

        // Payload Arguments
        if (!string.IsNullOrEmpty(HomebrewPayloadArgumentsTextBox.Text))
        {
            foreach (string Line in File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js")))
            {
                if (Line.Contains("const ARGS"))
                {
                    NewJSLines.Add("const ARGS = [" + HomebrewPayloadArgumentsTextBox.Text + "]"); // Missing ";" ?
                }
                else
                {
                    NewJSLines.Add(Line);
                }
            }
            // Save
            File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js"), [.. NewJSLines], Encoding.UTF8);
            NewJSLines = [];
        }

        // Payload Enviroment Variables
        if (!string.IsNullOrEmpty(HomebrewPayloadEnvironmentVariablesTextBox.Text))
        {
            foreach (string Line in File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js")))
            {
                if (Line.Contains("const ENVVARS"))
                {
                    NewJSLines.Add("const ENVVARS = " + HomebrewPayloadEnvironmentVariablesTextBox.Text + ";");
                }
                else
                {
                    NewJSLines.Add(Line);
                }
            }
            // Save
            File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js"), [.. NewJSLines], Encoding.UTF8);
            NewJSLines = [];
        }

        // Payload ROMDIR
        if (!string.IsNullOrEmpty(HomebrewPayloadROMDirectoryTextBox.Text))
        {
            foreach (string Line in File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js")))
            {
                if (Line.Contains("const ROMDIR"))
                {
                    NewJSLines.Add("const ROMDIR = window.workingDir + '" + HomebrewPayloadROMDirectoryTextBox.Text + "';");
                }
                else
                {
                    NewJSLines.Add(Line);
                }
            }
            // Save
            File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js"), [.. NewJSLines], Encoding.UTF8);
            NewJSLines = [];
        }

        // Payload MEDIADIR
        if (!string.IsNullOrEmpty(HomebrewPayloadMediaDirectoryTextBox.Text))
        {
            foreach (string Line in File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js")))
            {
                if (Line.Contains("const MEDIADIR"))
                {
                    NewJSLines.Add("const MEDIADIR = window.workingDir + '" + HomebrewPayloadMediaDirectoryTextBox.Text + "';");
                }
                else
                {
                    NewJSLines.Add(Line);
                }
            }
            // Save
            File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js"), [.. NewJSLines], Encoding.UTF8);
            NewJSLines = [];
        }

        // Upload changes
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js")) && !string.IsNullOrEmpty(SelectedHomebrewItem.HomebrewPath))
        {
            try
            {
                using var NewFtpClient = new AsyncFtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", PS5Port, NewFtpConfig);

                // Connect
                await NewFtpClient.Connect();

                await NewFtpClient.UploadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js"), SelectedHomebrewItem.HomebrewPath, FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None);

                // Disconnect
                await NewFtpClient.Disconnect();
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error uploading changes", "Could not upload any changes to the PS5." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error uploading changes", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Cache", "homebrew.js"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }

        var box2 = MessageBoxManager.GetMessageBoxStandard("Info", "Homebrew information updated & saved!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
        await box2.ShowWindowAsync();
    }

    private void ManageROMsButton_Click(object? sender, RoutedEventArgs e)
    {
        if (InstalledHomebrewListBox.SelectedItem is not null && !string.IsNullOrEmpty(HomebrewPayloadROMDirectoryTextBox.Text) && !string.IsNullOrEmpty(PS5IPTextBox.Text))
        {
            SelectedHomebrewItem = (HomebrewListViewItem)InstalledHomebrewListBox.SelectedItem;
            string ROMFullPath = SelectedHomebrewItem.HomebrewPath + HomebrewPayloadROMDirectoryTextBox.Text;
            var NewROMManager = new WebSrvROMManager() { ShowActivated = true, Title = "PS5 WebSrv Game ROM Manager - " + ROMFullPath, ROMPath = ROMFullPath, PS5IP = PS5IPTextBox.Text, PS5PORT = PS5Port };
            NewROMManager.Show();
        }
    }

    private void ManageMediaButton_Click(object? sender, RoutedEventArgs e)
    {
        if (InstalledHomebrewListBox.SelectedItem is not null && !string.IsNullOrEmpty(HomebrewPayloadMediaDirectoryTextBox.Text))
        {
            SelectedHomebrewItem = (HomebrewListViewItem)InstalledHomebrewListBox.SelectedItem;
            string MediaFullPath = SelectedHomebrewItem.HomebrewPath + HomebrewPayloadMediaDirectoryTextBox.Text;
            var NewROMManager = new WebSrvMediaManager() { ShowActivated = true, Title = "PS5 WebSrv Media Content Manager - " + MediaFullPath, MediaPath = MediaFullPath, PS5IP = PS5IPTextBox.Text!, PS5PORT = PS5Port };
            NewROMManager.Show();
        }
    }

    private async void AddHomebrewButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PS5IPTextBox.Text))
        {

            var FBD = new OpenFolderDialog() { Title = "Select a compatible homebrew folder" };
            var FBDResult = await FBD.ShowAsync(this);

            if (FBDResult != null)
            {

                if (File.Exists(Path.Combine(FBDResult, "homebrew.js")))
                {

                    var DirInfo = new DirectoryInfo(FBDResult);

                    try
                    {
                        using var NewFtpClient = new AsyncFtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", PS5Port, NewFtpConfig);

                        // Connect
                        await NewFtpClient.Connect();

                        // Delete
                        if (!await NewFtpClient.FileExists("/data/homebrew/" + DirInfo.Name))
                        {
                            // Create the directory on the PS5
                            await NewFtpClient.CreateDirectory("/data/homebrew/" + DirInfo.Name);

                            // Upload files recursively
                            await UploadFolderAsync(FBDResult, "/data/homebrew/" + DirInfo.Name);

                            // Disconnect
                            await NewFtpClient.Disconnect();

                            var box = MessageBoxManager.GetMessageBoxStandard("Info", "Homebrew has been uploaded to the WebSrv.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowAsync();
                        }
                        else
                        {
                            // Update folder with new files
                            await UploadFolderAsync(FBDResult, "/data/homebrew/" + DirInfo.Name);

                            // Disconnect
                            await NewFtpClient.Disconnect();

                            var box2 = MessageBoxManager.GetMessageBoxStandard("Info", "Homebrew updated on the WebSrv.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box2.ShowWindowAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not add the selected homebrew." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }

                    // Refresh
                    ListHomebrew();
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Incompatible homebrew", "This folder is missing a homebrew.js file that is required by the WebSrv and cannot be added.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }

            }

        }
    }

    private async void RemoveHomebrewButton_Click(object? sender, RoutedEventArgs e)
    {
        if (InstalledHomebrewListBox.SelectedItem is not null && !string.IsNullOrEmpty(PS5IPTextBox.Text))
        {
            SelectedHomebrewItem = (HomebrewListViewItem)InstalledHomebrewListBox.SelectedItem;

            var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Do you really want to delete the selected homebrew from the WebSrv ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);
            if (boxresult == ButtonResult.Yes)
            {

                try
                {
                    using var NewFtpClient = new AsyncFtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", PS5Port, NewFtpConfig);

                    // Connect
                    await NewFtpClient.Connect();

                    // Delete
                    await NewFtpClient.DeleteDirectory(SelectedHomebrewItem.HomebrewPath);

                    // Disconnect
                    await NewFtpClient.Disconnect();
                }
                catch (Exception ex)
                {
                    var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "Could not delete the selected homebrew." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box2.ShowWindowAsync();
                }

                var box3 = MessageBoxManager.GetMessageBoxStandard("Info", "Homebrew removed from the WebSrv.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box3.ShowWindowAsync();

                // Clear homebrew information
                HomebrewPayloadPathTextBox.Text = "";
                HomebrewPayloadArgumentsTextBox.Text = "";
                HomebrewPayloadEnvironmentVariablesTextBox.Text = "";
                HomebrewPayloadROMDirectoryTextBox.Text = "";
                HomebrewPayloadMediaDirectoryTextBox.Text = "";
                HomebrewPayloadNameTextBox.Text = "";
                HomebrewPayloadDescriptionTextBox.Text = "";
                HomebrewImage.Source = null;
                ManageROMsButton.IsVisible = false;
                ManageMediaButton.IsVisible = false;

                // Refresh
                ListHomebrew();
            }

        }
    }

    private async void HomebrewImage_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (InstalledHomebrewListBox.SelectedItem is not null && !string.IsNullOrEmpty(PS5IPTextBox.Text))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Do you want to replace the icon for the selected homebrew ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);
            if (boxresult == ButtonResult.Yes)
            {
                var pngFileFilter = new FileDialogFilter
                {
                    Name = "PNG Image File",
                    Extensions = ["png"]
                };
                var OFD = new OpenFileDialog() { Title = "Select a new icon0.png file.", Filters = { pngFileFilter }, AllowMultiple = false };
                var OFDResult = await OFD.ShowAsync(this);

                if (OFDResult != null && OFDResult.Length > 0)
                {
                    try
                    {
                        using var NewFtpClient = new AsyncFtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", PS5Port, NewFtpConfig);

                        // Connect
                        await NewFtpClient.Connect();

                        // Replace
                        await NewFtpClient.UploadFile(OFDResult[0], SelectedHomebrewItem.HomebrewPath + "/sce_sys/icon0.png", FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None);

                        // Disconnect
                        await NewFtpClient.Disconnect();

                        var box2 = MessageBoxManager.GetMessageBoxStandard("Info", "Icon replaced!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box2.ShowWindowAsync();
                    }
                    catch (Exception ex)
                    {
                        var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "Could not upload the selected icon." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box2.ShowWindowAsync();
                    }

                    // Refresh icon
                    GetHomebrewInfo(SelectedHomebrewItem.HomebrewPath);
                }
            }
        }
    }

    private async Task<bool> UploadFolderAsync(string LocalDirectoryPath, string RemoteDestinationPath)
    {
        try
        {
            using var NewFtpClient = new AsyncFtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", PS5Port, NewFtpConfig);
            // Connect
            await NewFtpClient.Connect();

            // Upload
            await NewFtpClient.UploadDirectory(LocalDirectoryPath, RemoteDestinationPath, FtpFolderSyncMode.Update, FtpRemoteExists.OverwriteInPlace, FtpVerify.None, null);

            // Disconnect
            await NewFtpClient.Disconnect();
            return true;
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not upload the selected folder." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return false;
        }
    }

}