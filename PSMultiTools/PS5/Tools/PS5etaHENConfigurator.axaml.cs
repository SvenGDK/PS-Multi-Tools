using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentFTP;
using IniParser;
using IniParser.Model;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.IO;
using System.Security.Authentication;

namespace PSMultiTools.PS5.Tools;

public partial class PS5etaHENConfigurator : Window
{

    public string ConsoleIP = "";
    public string ConsolePort = "";

    public PS5etaHENConfigurator()
    {
        InitializeComponent();
        Loaded += PS5etaHENConfigurator_Loaded;

        DiscordCheckBox.IsCheckedChanged += DiscordCheckBox_IsCheckedChanged;
        FTPCheckBox.IsCheckedChanged += FTPCheckBox_IsCheckedChanged;
        TestkitCheckBox.IsCheckedChanged += TestkitCheckBox_IsCheckedChanged;
        PS5DebugAutoLoadCheckBox.IsCheckedChanged += PS5DebugAutoLoadCheckBox_IsCheckedChanged;
        AllowDataInSandboxCheckBox.IsCheckedChanged += AllowDataInSandboxCheckBox_IsCheckedChanged;
        DPIServiceCheckBox.IsCheckedChanged += DPIServiceCheckBox_IsCheckedChanged;
        KernelLogCheckBox.IsCheckedChanged += KernelLogCheckBox_IsCheckedChanged;
        KillUtilDaemonCheckBox.IsCheckedChanged += KillUtilDaemonCheckBox_IsCheckedChanged;
        KillOpenGameCheckBox.IsCheckedChanged += KillOpenGameCheckBox_IsCheckedChanged;

        StartupOptionComboBox.SelectionChanged += StartupOptionComboBox_SelectionChanged;
    }

    private async void GetConfigButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Cache")))
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Cache"));

        if (!string.IsNullOrEmpty(ConsoleIP))
        {
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

                    // Get config.ini
                    conn.DownloadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), "/data/etaHEN/config.ini", FtpLocalExists.Overwrite);

                    // Disconnect
                    conn.Disconnect();
                }

                // Load config after download
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
                {
                    var ConfigInfo = new FileInfo(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                    var ConfigDateTime = ConfigInfo.LastWriteTime;
                    ConfigStatusTextBlock.Text = "etaHEN config found" + " - " + ConfigDateTime.ToString();

                    var etaHENConfigParser = new FileIniDataParser();
                    IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["PS5Debug"]))
                    {
                        if (etaHENConfigData["Settings"]["PS5Debug"] == "0")
                        {
                            PS5DebugAutoLoadCheckBox.IsChecked = false;
                        }
                        else
                        {
                            PS5DebugAutoLoadCheckBox.IsChecked = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["FTP"]))
                    {
                        if (etaHENConfigData["Settings"]["FTP"] == "0")
                        {
                            FTPCheckBox.IsChecked = false;
                        }
                        else
                        {
                            FTPCheckBox.IsChecked = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["discord_rpc"]))
                    {
                        if (etaHENConfigData["Settings"]["discord_rpc"] == "0")
                        {
                            DiscordCheckBox.IsChecked = false;
                        }
                        else
                        {
                            DiscordCheckBox.IsChecked = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["testkit"]))
                    {
                        if (etaHENConfigData["Settings"]["testkit"] == "0")
                        {
                            TestkitCheckBox.IsChecked = false;
                        }
                        else
                        {
                            TestkitCheckBox.IsChecked = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Allow_data_in_sandbox"]))
                    {
                        if (etaHENConfigData["Settings"]["Allow_data_in_sandbox"] == "0")
                        {
                            AllowDataInSandboxCheckBox.IsChecked = false;
                        }
                        else
                        {
                            AllowDataInSandboxCheckBox.IsChecked = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["DPI"]))
                    {
                        if (etaHENConfigData["Settings"]["DPI"] == "0")
                        {
                            DPIServiceCheckBox.IsChecked = false;
                        }
                        else
                        {
                            DPIServiceCheckBox.IsChecked = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Klog"]))
                    {
                        if (etaHENConfigData["Settings"]["Klog"] == "0")
                        {
                            KernelLogCheckBox.IsChecked = false;
                        }
                        else
                        {
                            KernelLogCheckBox.IsChecked = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["ALLOW_FTP_DEV_ACCESS"]))
                    {
                        if (etaHENConfigData["Settings"]["ALLOW_FTP_DEV_ACCESS"] == "0")
                        {
                            FTPDevAccessCheckBox.IsChecked = false;
                        }
                        else
                        {
                            FTPDevAccessCheckBox.IsChecked = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Util_rest_kill"]))
                    {
                        if (etaHENConfigData["Settings"]["Util_rest_kill"] == "0")
                        {
                            KillUtilDaemonCheckBox.IsChecked = false;
                        }
                        else
                        {
                            KillUtilDaemonCheckBox.IsChecked = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Game_rest_kill"]))
                    {
                        if (etaHENConfigData["Settings"]["Game_rest_kill"] == "0")
                        {
                            KillOpenGameCheckBox.IsChecked = false;
                        }
                        else
                        {
                            KillOpenGameCheckBox.IsChecked = true;
                        }
                    }

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["StartOption"]))
                    {
                        switch (etaHENConfigData["Settings"]["StartOption"] ?? "")
                        {
                            case "0":
                                {
                                    StartupOptionComboBox.SelectedIndex = 0;
                                    break;
                                }
                            case "1":
                                {
                                    StartupOptionComboBox.SelectedIndex = 1;
                                    break;
                                }
                            case "2":
                                {
                                    StartupOptionComboBox.SelectedIndex = 2;
                                    break;
                                }
                            case "3":
                                {
                                    StartupOptionComboBox.SelectedIndex = 3;
                                    break;
                                }
                            case "4":
                                {
                                    StartupOptionComboBox.SelectedIndex = 4;
                                    break;
                                }
                        }
                    }
                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Rest_Mode_Delay_Seconds"]))
                    {
                        ShellUIPatchDelayTextBox.Text = etaHENConfigData["Settings"]["Rest_Mode_Delay_Seconds"];
                    }

                }

                var box = MessageBoxManager.GetMessageBoxStandard("Done", "Config file retrieved!" + Environment.NewLine + "You can change the checkboxes now and reupload the config.ini", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
            catch (Exception)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not get the config.ini file, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
    }

    private async void SaveAndUploadButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
        {
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

                    // Upload config.ini
                    conn.UploadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), "/data/etaHEN/config.ini", FtpRemoteExists.NoCheck);

                    // Disconnect
                    conn.Disconnect();
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Done", "etaHEN config uploaded & updated on the PS5!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
            catch (Exception)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not upload the config.ini, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No local config.ini file found.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private void PS5etaHENConfigurator_Loaded(object? sender, RoutedEventArgs e)
    {
        // Load existing config
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
        {
            var etaHENConfigParser = new FileIniDataParser();
            IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));

            var ConfigInfo = new FileInfo(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
            var ConfigDateTime = ConfigInfo.LastWriteTime;
            ConfigStatusTextBlock.Text = "etaHEN config found" + " - " + ConfigDateTime.ToString();

            if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["PS5Debug"]))
            {
                if (etaHENConfigData["Settings"]["PS5Debug"] == "0")
                {
                    PS5DebugAutoLoadCheckBox.IsChecked = false;
                }
                else
                {
                    PS5DebugAutoLoadCheckBox.IsChecked = true;
                }
            }
            if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["FTP"]))
            {
                if (etaHENConfigData["Settings"]["FTP"] == "0")
                {
                    FTPCheckBox.IsChecked = false;
                }
                else
                {
                    FTPCheckBox.IsChecked = true;
                }
            }
            if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["discord_rpc"]))
            {
                if (etaHENConfigData["Settings"]["discord_rpc"] == "0")
                {
                    DiscordCheckBox.IsChecked = false;
                }
                else
                {
                    DiscordCheckBox.IsChecked = true;
                }
            }
            if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["testkit"]))
            {
                if (etaHENConfigData["Settings"]["testkit"] == "0")
                {
                    TestkitCheckBox.IsChecked = false;
                }
                else
                {
                    TestkitCheckBox.IsChecked = true;
                }
            }
            if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Allow_data_in_sandbox"]))
            {
                if (etaHENConfigData["Settings"]["Allow_data_in_sandbox"] == "0")
                {
                    AllowDataInSandboxCheckBox.IsChecked = false;
                }
                else
                {
                    AllowDataInSandboxCheckBox.IsChecked = true;
                }
            }
            if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["DPI"]))
            {
                if (etaHENConfigData["Settings"]["DPI"] == "0")
                {
                    DPIServiceCheckBox.IsChecked = false;
                }
                else
                {
                    DPIServiceCheckBox.IsChecked = true;
                }
            }
            if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Klog"]))
            {
                if (etaHENConfigData["Settings"]["Klog"] == "0")
                {
                    KernelLogCheckBox.IsChecked = false;
                }
                else
                {
                    KernelLogCheckBox.IsChecked = true;
                }
            }
            if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["ALLOW_FTP_DEV_ACCESS"]))
            {
                if (etaHENConfigData["Settings"]["ALLOW_FTP_DEV_ACCESS"] == "0")
                {
                    FTPDevAccessCheckBox.IsChecked = false;
                }
                else
                {
                    FTPDevAccessCheckBox.IsChecked = true;
                }
            }
            if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Util_rest_kill"]))
            {
                if (etaHENConfigData["Settings"]["Util_rest_kill"] == "0")
                {
                    KillUtilDaemonCheckBox.IsChecked = false;
                }
                else
                {
                    KillUtilDaemonCheckBox.IsChecked = true;
                }
            }
            if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Game_rest_kill"]))
            {
                if (etaHENConfigData["Settings"]["Game_rest_kill"] == "0")
                {
                    KillOpenGameCheckBox.IsChecked = false;
                }
                else
                {
                    KillOpenGameCheckBox.IsChecked = true;
                }
            }

            if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["StartOption"]))
            {
                switch (etaHENConfigData["Settings"]["StartOption"] ?? "")
                {
                    case "0":
                        {
                            StartupOptionComboBox.SelectedIndex = 0;
                            break;
                        }
                    case "1":
                        {
                            StartupOptionComboBox.SelectedIndex = 1;
                            break;
                        }
                    case "2":
                        {
                            StartupOptionComboBox.SelectedIndex = 2;
                            break;
                        }
                    case "3":
                        {
                            StartupOptionComboBox.SelectedIndex = 3;
                            break;
                        }
                    case "4":
                        {
                            StartupOptionComboBox.SelectedIndex = 4;
                            break;
                        }
                }
            }
            if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Rest_Mode_Delay_Seconds"]))
            {
                ShellUIPatchDelayTextBox.Text = etaHENConfigData["Settings"]["Rest_Mode_Delay_Seconds"];
            }
        }
    }

    #region Check changes

    private void KillOpenGameCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (KillOpenGameCheckBox.IsChecked == true)
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["Game_rest_kill"] = "1";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
        else
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["Game_rest_kill"] = "0";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
    }

    private void KillUtilDaemonCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (KillUtilDaemonCheckBox.IsChecked == true)
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["Util_rest_kill"] = "1";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
        else
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["Util_rest_kill"] = "0";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
    }

    private void KernelLogCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (KernelLogCheckBox.IsChecked == true)
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["Klog"] = "1";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
        else
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["Klog"] = "0";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
    }

    private void DPIServiceCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (DPIServiceCheckBox.IsChecked == true)
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["DPI"] = "1";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
        else
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["DPI"] = "0";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
    }

    private void AllowDataInSandboxCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (AllowDataInSandboxCheckBox.IsChecked == true)
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["Allow_data_in_sandbox"] = "1";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
        else
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["Allow_data_in_sandbox"] = "0";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
    }

    private void PS5DebugAutoLoadCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (PS5DebugAutoLoadCheckBox.IsChecked == true)
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["PS5Debug"] = "1";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
        else
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["PS5Debug"] = "0";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
    }

    private void TestkitCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (TestkitCheckBox.IsChecked == true)
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["testkit"] = "1";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
        else
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["testkit"] = "0";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
    }

    private void FTPCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (FTPCheckBox.IsChecked == true)
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["FTP"] = "1";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
        else
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["FTP"] = "0";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
    }

    private void DiscordCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (DiscordCheckBox.IsChecked == true)
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["discord_rpc"] = "1";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
        else
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
            {
                var etaHENConfigParser = new FileIniDataParser();
                IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                etaHENConfigData["Settings"]["discord_rpc"] = "0";
                etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
            }
        }
    }

    private void StartupOptionComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not null)
        {

            ComboBoxItem SelectedCBItem = (ComboBoxItem)e.AddedItems[0]!;
            switch (SelectedCBItem.Content!.ToString() ?? "")
            {
                case "None":
                    {
                        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
                        {
                            var etaHENConfigParser = new FileIniDataParser();
                            IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                            etaHENConfigData["Settings"]["StartOption"] = "0";
                            etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
                        }

                        break;
                    }
                case "Home Menu":
                    {
                        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
                        {
                            var etaHENConfigParser = new FileIniDataParser();
                            IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                            etaHENConfigData["Settings"]["StartOption"] = "1";
                            etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
                        }

                        break;
                    }
                case "Settings":
                    {
                        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
                        {
                            var etaHENConfigParser = new FileIniDataParser();
                            IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                            etaHENConfigData["Settings"]["StartOption"] = "2";
                            etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
                        }

                        break;
                    }
                case "Toolbox":
                    {
                        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
                        {
                            var etaHENConfigParser = new FileIniDataParser();
                            IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                            etaHENConfigData["Settings"]["StartOption"] = "3";
                            etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
                        }

                        break;
                    }
                case "Itemzflow":
                    {
                        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
                        {
                            var etaHENConfigParser = new FileIniDataParser();
                            IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                            etaHENConfigData["Settings"]["StartOption"] = "4";
                            etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);
                        }

                        break;
                    }
            }
        }
    }

    #endregion

}