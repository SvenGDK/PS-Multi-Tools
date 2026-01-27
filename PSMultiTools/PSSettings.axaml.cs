using Avalonia.Controls;
using Avalonia.Interactivity;
using IniParser;
using IniParser.Model;
using System;
using System.IO;

namespace PSMultiTools;

public partial class PSSettings : Window
{

    FileIniDataParser? PSMTConfigParser;
    IniData? PSMTConfigData;

    public PSSettings()
    {
        InitializeComponent();

        Loaded += PSSettings_Loaded;
        Closing += PSSettings_Closing;
    }

    private void PSSettings_Loaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini")))
            {
                try
                {
                    PSMTConfigParser = new FileIniDataParser();
                    PSMTConfigData = PSMTConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"));

                    // General
                    if (!string.IsNullOrEmpty(PSMTConfigData["General"]["AutoLibraryMusic"]))
                    {
                        if (PSMTConfigData["General"]["AutoLibraryMusic"] == "False")
                        {
                            LibraryMusicCheckBox.IsChecked = false;
                        }
                        else
                        {
                            LibraryMusicCheckBox.IsChecked = true;
                        }
                    }

                    // PS5 Library Local
                    if (!string.IsNullOrEmpty(PSMTConfigData["PS5 Library"]["LoadIcons"]))
                    {
                        if (PSMTConfigData["PS5 Library"]["LoadIcons"] == "False")
                        {
                            LoadIconCheckBox.IsChecked = false;
                        }
                        else
                        {
                            LoadIconCheckBox.IsChecked = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(PSMTConfigData["PS5 Library"]["LoadBackgrounds"]))
                    {
                        if (PSMTConfigData["PS5 Library"]["LoadBackgrounds"] == "False")
                        {
                            LoadBackgroundCheckBox.IsChecked = false;
                        }
                        else
                        {
                            LoadBackgroundCheckBox.IsChecked = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(PSMTConfigData["PS5 Library"]["SkipFileChecks"]))
                    {
                        if (PSMTConfigData["PS5 Library"]["SkipFileChecks"] == "False")
                        {
                            SkipFileCheckCheckBox.IsChecked = false;
                        }
                        else
                        {
                            SkipFileCheckCheckBox.IsChecked = true;
                        }
                    }

                    // PS5 Library FTP
                    if (!string.IsNullOrEmpty(PSMTConfigData["PS5 Library"]["FTPLoadIcons"]))
                    {
                        if (PSMTConfigData["PS5 Library"]["FTPLoadIcons"] == "False")
                        {
                            LoadFTPIconsCheckBox.IsChecked = false;
                        }
                        else
                        {
                            LoadFTPIconsCheckBox.IsChecked = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(PSMTConfigData["PS5 Library"]["FTPLoadBackgrounds"]))
                    {
                        if (PSMTConfigData["PS5 Library"]["FTPLoadBackgrounds"] == "False")
                        {
                            LoadFTPBackgroundsCheckBox.IsChecked = false;
                        }
                        else
                        {
                            LoadFTPBackgroundsCheckBox.IsChecked = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(PSMTConfigData["PS5 Library"]["FTPScanAllUSB"]))
                    {
                        if (PSMTConfigData["PS5 Library"]["FTPScanAllUSB"] == "False")
                        {
                            ScanAllUSBCheckBox.IsChecked = false;
                        }
                        else
                        {
                            ScanAllUSBCheckBox.IsChecked = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(PSMTConfigData["PS5 Library"]["FTPScanext0"]))
                    {
                        if (PSMTConfigData["PS5 Library"]["FTPScanext0"] == "False")
                        {
                            Scanext0CheckBox.IsChecked = false;
                        }
                        else
                        {
                            Scanext0CheckBox.IsChecked = true;
                        }
                    }

                    // PS5 Tools
                    if (!string.IsNullOrEmpty(PSMTConfigData["PS5 Tools"]["IP"]))
                    {
                        PS5IPTextBox.Text = PSMTConfigData["PS5 Tools"]["IP"];
                    }
                    if (!string.IsNullOrEmpty(PSMTConfigData["PS5 Tools"]["FTPPort"]))
                    {
                        PS5FTPPortTextBox.Text = PSMTConfigData["PS5 Tools"]["FTPPort"];
                    }
                    if (!string.IsNullOrEmpty(PSMTConfigData["PS5 Tools"]["PayloadPort"]))
                    {
                        PS5PayloadPortTextBox.Text = PSMTConfigData["PS5 Tools"]["PayloadPort"];
                    }
                    if (!string.IsNullOrEmpty(PSMTConfigData["PS5 Tools"]["ScanThreads"]))
                    {
                        ScanThreadsCount.Value = Convert.ToInt32(PSMTConfigData["PS5 Tools"]["ScanThreads"]);
                    }

                    // PS3 Tools
                    if (!string.IsNullOrEmpty(PSMTConfigData["PS3 Tools"]["IP"]))
                    {
                        PS3IPTextBox.Text = PSMTConfigData["PS3 Tools"]["IP"];
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                // Create new config
                File.Create(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"));
                PSMTConfigParser = new FileIniDataParser();
                PSMTConfigData = PSMTConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"));
            }
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
    }

    private void PSSettings_Closing(object? sender, WindowClosingEventArgs e)
    {
        // Save on close
        if (PSMTConfigData != null && PSMTConfigParser != null)
        {
            try
            {
                // General
                if (LibraryMusicCheckBox.IsChecked == true)
                {
                    PSMTConfigData["General"]["AutoLibraryMusic"] = "True";
                }
                else
                {
                    PSMTConfigData["General"]["AutoLibraryMusic"] = "False";
                }

                // PS5 Library FTP
                if (LoadFTPIconsCheckBox.IsChecked == true)
                {
                    PSMTConfigData["PS5 Library"]["FTPLoadIcons"] = "True";
                }
                else
                {
                    PSMTConfigData["PS5 Library"]["FTPLoadIcons"] = "False";
                }
                if (LoadFTPBackgroundsCheckBox.IsChecked == true)
                {
                    PSMTConfigData["PS5 Library"]["FTPLoadBackgrounds"] = "True";
                }
                else
                {
                    PSMTConfigData["PS5 Library"]["FTPLoadBackgrounds"] = "False";
                }
                if (ScanAllUSBCheckBox.IsChecked == true)
                {
                    PSMTConfigData["PS5 Library"]["FTPScanAllUSB"] = "True";
                }
                else
                {
                    PSMTConfigData["PS5 Library"]["FTPScanAllUSB"] = "False";
                }
                if (Scanext0CheckBox.IsChecked == true)
                {
                    PSMTConfigData["PS5 Library"]["FTPScanext0"] = "True";
                }
                else
                {
                    PSMTConfigData["PS5 Library"]["FTPScanext0"] = "False";
                }

                // PS5 Library Local
                if (LoadIconCheckBox.IsChecked == true)
                {
                    PSMTConfigData["PS5 Library"]["LoadIcons"] = "True";
                }
                else
                {
                    PSMTConfigData["PS5 Library"]["LoadIcons"] = "False";
                }
                if (LoadBackgroundCheckBox.IsChecked == true)
                {
                    PSMTConfigData["PS5 Library"]["LoadBackgrounds"] = "True";
                }
                else
                {
                    PSMTConfigData["PS5 Library"]["LoadBackgrounds"] = "False";
                }
                if (SkipFileCheckCheckBox.IsChecked == true)
                {
                    PSMTConfigData["PS5 Library"]["SkipFileChecks"] = "True";
                }
                else
                {
                    PSMTConfigData["PS5 Library"]["SkipFileChecks"] = "False";
                }
                if (ScanThreadsCount.Value >= 1)
                {
                    PSMTConfigData["PS5 Library"]["ScanThreads"] = ScanThreadsCount.Value.ToString();
                }
                else
                {
                    PSMTConfigData["PS5 Library"]["ScanThreads"] = "8";
                }

                // PS5 Tools
                if (!string.IsNullOrEmpty(PS5IPTextBox.Text))
                {
                    PSMTConfigData["PS5 Tools"]["IP"] = PS5IPTextBox.Text;
                }
                if (!string.IsNullOrEmpty(PS5FTPPortTextBox.Text))
                {
                    PSMTConfigData["PS5 Tools"]["FTPPort"] = PS5FTPPortTextBox.Text;
                }
                if (!string.IsNullOrEmpty(PS5PayloadPortTextBox.Text))
                {
                    PSMTConfigData["PS5 Tools"]["PayloadPort"] = PS5PayloadPortTextBox.Text;
                }

                // PS3 Library
                if (!string.IsNullOrEmpty(PS3IPTextBox.Text))
                {
                    PSMTConfigData["PS3 Tools"]["IP"] = PS3IPTextBox.Text;
                }

                PSMTConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"), PSMTConfigData);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }

}