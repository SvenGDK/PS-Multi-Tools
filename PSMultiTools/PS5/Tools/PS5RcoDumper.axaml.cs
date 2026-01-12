using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FluentFTP;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.IO;
using System.Security.Authentication;

namespace PSMultiTools.PS5.Tools;

public partial class PS5RcoDumper : Window
{

    public string ConsoleIP = "";
    public string ConsolePort = "";

    public PS5RcoDumper()
    {
        InitializeComponent();
    }

    private async void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Directory = Environment.SpecialFolder.Desktop.ToString(), Title = "Select a folder where you want to dump the rco files" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SaveToTextBox.Text = FBDResult;
        }
    }

    private async void LoadFilesButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SaveToTextBox.Text))
        {
            if (!string.IsNullOrEmpty(ConsoleIP))
            {
                if (Directory.Exists(SaveToTextBox.Text))
                {

                    string SavePath = SaveToTextBox.Text;
                    Cursor = new Cursor(StandardCursorType.Wait);

                    try
                    {
                        using var conn = new FtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort));
                        // Configurate the FTP connection
                        conn.Config.EncryptionMode = FtpEncryptionMode.None;
                        conn.Config.SslProtocols = SslProtocols.None;
                        conn.Config.DataConnectionEncryption = false;

                        // Connect
                        conn.Connect();

                        // Enumerate .rco files in /system_ex/vsh_asset and download them
                        foreach (var FileInFTP in conn.GetListing("/system_ex/vsh_asset"))
                        {
                            if (FileInFTP.Name.EndsWith(".rco"))
                            {
                                conn.DownloadFile(Path.Combine(SavePath, FileInFTP.Name), FileInFTP.FullName, FtpLocalExists.Overwrite);
                            }
                        }

                        // Disconnect
                        conn.Disconnect();

                        Cursor = new Cursor(StandardCursorType.Arrow);

                        var box = MessageBoxManager.GetMessageBoxStandard("Success", "Files have been saved at " + SaveToTextBox.Text + "." + Environment.NewLine + "To extract them, please use the PS5 RCO Extractor.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box.ShowWindowAsync();
                    }
                    catch (Exception)
                    {
                        Cursor = new Cursor(StandardCursorType.Arrow);
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not dump .rco files, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }

                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please enter your IP and port in the settings before continuing.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please select a save directory before continuing.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

}