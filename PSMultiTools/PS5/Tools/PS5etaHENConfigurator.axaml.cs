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
    }

    private void PS5etaHENConfigurator_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ConsoleIP))
        {
            IPAddressTextBox.Text = ConsoleIP;
        }
        if (!string.IsNullOrEmpty(ConsolePort))
        {
            FTPPortTextBox.Text = ConsolePort;
        }

        // Load existing config
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
        {
            var etaHENConfigParser = new FileIniDataParser();
            IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));

            var ConfigInfo = new FileInfo(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
            var ConfigDateTime = ConfigInfo.LastWriteTime;
            //ConfigStatusTextBlock.Text = "etaHEN config found" + " - " + ConfigDateTime.ToString();

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
                    conn.DownloadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), "/data/etaHEN/config.ini", FtpLocalExists.Overwrite, FtpVerify.None);

                    // Disconnect
                    conn.Disconnect();
                }

                // Load config after download
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini")))
                {
                    var ConfigInfo = new FileInfo(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
                    var etaHENConfigParser = new FileIniDataParser();
                    IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["PS5Debug"]))
                        PS5DebugAutoLoadCheckBox.IsChecked = etaHENConfigData["Settings"]["PS5Debug"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["FTP"]))
                        FTPCheckBox.IsChecked = etaHENConfigData["Settings"]["FTP"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["launch_itemzflow"]))
                        AutostartItemzflowCheckBox.IsChecked = etaHENConfigData["Settings"]["launch_itemzflow"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["discord_rpc"]))
                        DiscordCheckBox.IsChecked = etaHENConfigData["Settings"]["discord_rpc"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["testkit"]))
                        TestkitCheckBox.IsChecked = etaHENConfigData["Settings"]["testkit"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Klog"]))
                        KernelLogCheckBox.IsChecked = etaHENConfigData["Settings"]["Klog"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["DPI"]))
                        DPIServiceCheckBox.IsChecked = etaHENConfigData["Settings"]["DPI"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Allow_data_in_sandbox"]))
                        AllowDataInSandboxCheckBox.IsChecked = etaHENConfigData["Settings"]["Allow_data_in_sandbox"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["ALLOW_FTP_DEV_ACCESS"]))
                        FTPDevAccessCheckBox.IsChecked = etaHENConfigData["Settings"]["ALLOW_FTP_DEV_ACCESS"] != "0";

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
                        ShellUIPatchDelayTextBox.Text = etaHENConfigData["Settings"]["Rest_Mode_Delay_Seconds"];

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Util_rest_kill"]))
                        KillUtilDaemonCheckBox.IsChecked = etaHENConfigData["Settings"]["Util_rest_kill"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Game_rest_kill"]))
                        KillOpenGameCheckBox.IsChecked = etaHENConfigData["Settings"]["Game_rest_kill"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["toolbox_auto_start"]))
                        ToolboxAutostartCheckBox.IsChecked = etaHENConfigData["Settings"]["toolbox_auto_start"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["DPI_v2"]))
                        DPIv2ServiceCheckBox.IsChecked = etaHENConfigData["Settings"]["DPI_v2"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["disable_toolbox_auto_start_for_rest_mode"]))
                        DisableToolboxAutostartforRestModeCheckBox.IsChecked = etaHENConfigData["Settings"]["disable_toolbox_auto_start_for_rest_mode"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Display_tids"]))
                        DisplayTitleIDsCheckBox.IsChecked = etaHENConfigData["Settings"]["Display_tids"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["APP_JB_Debug_Msg"]))
                        AppJailbreakDebugMessagesCheckBox.IsChecked = etaHENConfigData["Settings"]["APP_JB_Debug_Msg"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["etaHEN_Game_Options"]))
                        etaHENGameOptionsCheckBox.IsChecked = etaHENConfigData["Settings"]["etaHEN_Game_Options"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["auto_eject_disc"]))
                        AutoEjectDiscCheckBox.IsChecked = etaHENConfigData["Settings"]["auto_eject_disc"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["overlay_ram"]))
                        OverlayRAMCheckBox.IsChecked = etaHENConfigData["Settings"]["overlay_ram"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["overlay_cpu"]))
                        OverlayCPUCheckBox.IsChecked = etaHENConfigData["Settings"]["overlay_cpu"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["overlay_gpu"]))
                        OverlayGPUCheckBox.IsChecked = etaHENConfigData["Settings"]["overlay_gpu"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["overlay_fps"]))
                        OverlayFPSCheckBox.IsChecked = etaHENConfigData["Settings"]["overlay_fps"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["overlay_ip"]))
                        OverlayIPCheckBox.IsChecked = etaHENConfigData["Settings"]["overlay_ip"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["overlay_kstuff"]))
                        OverlayKstuffCheckBox.IsChecked = etaHENConfigData["Settings"]["overlay_kstuff"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["enable_kstuff_on_close"]))
                        KstuffOnCloseCheckBox.IsChecked = etaHENConfigData["Settings"]["enable_kstuff_on_close"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["pause_kstuff_on_open"]))
                        PauseKstuffOnOpenCheckBox.IsChecked = etaHENConfigData["Settings"]["pause_kstuff_on_open"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["enable_fan_speed"]))
                        EnableFanSpeedCheckBox.IsChecked = etaHENConfigData["Settings"]["enable_fan_speed"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["fan_threshold"]))
                        FanTresholdTextBox.Text = etaHENConfigData["Settings"]["fan_threshold"];

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["pause_kstuff_on_open_secs"]))
                        PauseKstuffOnTextBox.Text = etaHENConfigData["Settings"]["pause_kstuff_on_open_secs"];

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Cheats_shortcut_opt"]))
                        CheatsShortcutCheckBox.IsChecked = etaHENConfigData["Settings"]["Cheats_shortcut_opt"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Toolbox_shortcut_opt"]))
                        ToolboxShortcutCheckBox.IsChecked = etaHENConfigData["Settings"]["Toolbox_shortcut_opt"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Games_shortcut_opt"]))
                        GamesShortcutCheckBox.IsChecked = etaHENConfigData["Settings"]["Games_shortcut_opt"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Kstuff_shortcut_opt"]))
                        KstuffShortcutCheckBox.IsChecked = etaHENConfigData["Settings"]["Kstuff_shortcut_opt"] != "0";

                    if (!string.IsNullOrEmpty(etaHENConfigData["Settings"]["Overlay_pos"]))
                        OverlayPositionTextBox.Text = etaHENConfigData["Settings"]["Overlay_pos"];
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
            var ConfigInfo = new FileInfo(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));
            var etaHENConfigParser = new FileIniDataParser();
            IniData etaHENConfigData = etaHENConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"));

            // Update values
            if (!string.IsNullOrEmpty(OverlayPositionTextBox.Text))
                etaHENConfigData["Settings"]["Overlay_pos"] = OverlayPositionTextBox.Text;

            if (KstuffShortcutCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["Kstuff_shortcut_opt"] = "1";
            else
                etaHENConfigData["Settings"]["Kstuff_shortcut_opt"] = "0";

            if (GamesShortcutCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["Games_shortcut_opt"] = "1";
            else
                etaHENConfigData["Settings"]["Games_shortcut_opt"] = "0";

            if (ToolboxShortcutCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["Toolbox_shortcut_opt"] = "1";
            else
                etaHENConfigData["Settings"]["Toolbox_shortcut_opt"] = "0";

            if (CheatsShortcutCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["Cheats_shortcut_opt"] = "1";
            else
                etaHENConfigData["Settings"]["Cheats_shortcut_opt"] = "0";

            if (!string.IsNullOrEmpty(PauseKstuffOnTextBox.Text))
                etaHENConfigData["Settings"]["pause_kstuff_on_open_secs"] = PauseKstuffOnTextBox.Text;

            if (!string.IsNullOrEmpty(FanTresholdTextBox.Text))
                etaHENConfigData["Settings"]["fan_threshold"] = FanTresholdTextBox.Text;

            if (EnableFanSpeedCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["enable_fan_speed"] = "1";
            else
                etaHENConfigData["Settings"]["enable_fan_speed"] = "0";

            if (PauseKstuffOnOpenCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["pause_kstuff_on_open"] = "1";
            else
                etaHENConfigData["Settings"]["pause_kstuff_on_open"] = "0";

            if (KstuffOnCloseCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["enable_kstuff_on_close"] = "1";
            else
                etaHENConfigData["Settings"]["enable_kstuff_on_close"] = "0";

            if (OverlayKstuffCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["overlay_kstuff"] = "1";
            else
                etaHENConfigData["Settings"]["overlay_kstuff"] = "0";

            if (OverlayIPCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["overlay_ip"] = "1";
            else
                etaHENConfigData["Settings"]["overlay_ip"] = "0";

            if (OverlayFPSCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["overlay_fps"] = "1";
            else
                etaHENConfigData["Settings"]["overlay_fps"] = "0";

            if (OverlayGPUCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["overlay_gpu"] = "1";
            else
                etaHENConfigData["Settings"]["overlay_gpu"] = "0";

            if (OverlayCPUCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["overlay_cpu"] = "1";
            else
                etaHENConfigData["Settings"]["overlay_cpu"] = "0";

            if (OverlayRAMCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["overlay_ram"] = "1";
            else
                etaHENConfigData["Settings"]["overlay_ram"] = "0";

            if (AutoEjectDiscCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["auto_eject_disc"] = "1";
            else
                etaHENConfigData["Settings"]["auto_eject_disc"] = "0";

            if (etaHENGameOptionsCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["etaHEN_Game_Options"] = "1";
            else
                etaHENConfigData["Settings"]["etaHEN_Game_Options"] = "0";

            if (AppJailbreakDebugMessagesCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["APP_JB_Debug_Msg"] = "1";
            else
                etaHENConfigData["Settings"]["APP_JB_Debug_Msg"] = "0";

            if (DisplayTitleIDsCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["Display_tids"] = "1";
            else
                etaHENConfigData["Settings"]["Display_tids"] = "0";

            if (DisableToolboxAutostartforRestModeCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["disable_toolbox_auto_start_for_rest_mode"] = "1";
            else
                etaHENConfigData["Settings"]["disable_toolbox_auto_start_for_rest_mode"] = "0";

            if (DPIv2ServiceCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["DPI_v2"] = "1";
            else
                etaHENConfigData["Settings"]["DPI_v2"] = "0";

            if (ToolboxAutostartCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["toolbox_auto_start"] = "1";
            else
                etaHENConfigData["Settings"]["toolbox_auto_start"] = "0";

            if (KillOpenGameCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["Game_rest_kill"] = "1";
            else
                etaHENConfigData["Settings"]["Game_rest_kill"] = "0";

            if (KillUtilDaemonCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["Util_rest_kill"] = "1";
            else
                etaHENConfigData["Settings"]["Util_rest_kill"] = "0";

            if (!string.IsNullOrEmpty(ShellUIPatchDelayTextBox.Text))
                etaHENConfigData["Settings"]["Rest_Mode_Delay_Seconds"] = ShellUIPatchDelayTextBox.Text;

            if (StartupOptionComboBox.SelectedItem != null)
            {
                if (StartupOptionComboBox.SelectedIndex == 0)
                {
                    etaHENConfigData["Settings"]["StartOption"] = "0";
                }
                else if (StartupOptionComboBox.SelectedIndex == 1)
                {
                    etaHENConfigData["Settings"]["StartOption"] = "1";
                }
                else if (StartupOptionComboBox.SelectedIndex == 2)
                {
                    etaHENConfigData["Settings"]["StartOption"] = "2";
                }
                else if (StartupOptionComboBox.SelectedIndex == 3)
                {
                    etaHENConfigData["Settings"]["StartOption"] = "3";
                }
                else if (StartupOptionComboBox.SelectedIndex == 4)
                {
                    etaHENConfigData["Settings"]["StartOption"] = "4";
                }
            }

            if (FTPDevAccessCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["ALLOW_FTP_DEV_ACCESS"] = "1";
            else
                etaHENConfigData["Settings"]["ALLOW_FTP_DEV_ACCESS"] = "0";

            if (AllowDataInSandboxCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["Allow_data_in_sandbox"] = "1";
            else
                etaHENConfigData["Settings"]["Allow_data_in_sandbox"] = "0";

            if (DPIServiceCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["DPI"] = "1";
            else
                etaHENConfigData["Settings"]["DPI"] = "0";

            if (KernelLogCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["Klog"] = "1";
            else
                etaHENConfigData["Settings"]["Klog"] = "0";

            if (TestkitCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["testkit"] = "1";
            else
                etaHENConfigData["Settings"]["testkit"] = "0";

            if (DiscordCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["discord_rpc"] = "1";
            else
                etaHENConfigData["Settings"]["discord_rpc"] = "0";

            if (AutostartItemzflowCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["launch_itemzflow"] = "1";
            else
                etaHENConfigData["Settings"]["launch_itemzflow"] = "0";

            if (FTPCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["FTP"] = "1";
            else
                etaHENConfigData["Settings"]["FTP"] = "0";

            if (PS5DebugAutoLoadCheckBox.IsChecked == true)
                etaHENConfigData["Settings"]["PS5Debug"] = "1";
            else
                etaHENConfigData["Settings"]["PS5Debug"] = "0";

            // Write to config
            etaHENConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "Cache", "config.ini"), etaHENConfigData);

            // Re-upload
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

                    // Delete old config.ini
                    conn.DeleteFile("/data/etaHEN/config.ini");

                    // Upload new config.ini
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
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please load the etaHEN configuration from the PS5 first.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

}