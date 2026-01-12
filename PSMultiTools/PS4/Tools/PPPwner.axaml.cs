using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;

namespace PSMultiTools.PS4.Tools;

public partial class PPPwner : Window
{

    public BackgroundWorker PPPwnWoker = new();
    bool PPPwnRunning = false;

    private struct ComboBoxEthernetDevice
    {
        public string AdapterDescription { get; set; }

        public string AdapterID { get; set; }

        public string AdapterName { get; set; }

        public string DisplayValue { get; set; }
    }

    public PPPwner()
    {
        InitializeComponent();

        PPPwnWoker.DoWork += PPPwnWoker_DoWork;
        Loaded += PPPwner_Loaded;
    }

    private void PPPwner_Loaded(object? sender, RoutedEventArgs e)
    {
        // List Ethernet Adapters
        var NewListOfEthernetAdapter = new List<ComboBoxEthernetDevice>();
        foreach (NetworkInterface AvailableNetworkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            switch (AvailableNetworkInterface.NetworkInterfaceType)
            {
                // Show only Ethernet Interfaces
                case NetworkInterfaceType.Ethernet:
                case NetworkInterfaceType.FastEthernetT:
                case NetworkInterfaceType.GigabitEthernet:
                    {
                        var NewComboBoxEhternetDevice = new ComboBoxEthernetDevice()
                        {
                            AdapterDescription = AvailableNetworkInterface.Description,
                            AdapterID = AvailableNetworkInterface.Id,
                            AdapterName = AvailableNetworkInterface.Name,
                            DisplayValue = "Name: " + AvailableNetworkInterface.Name + Environment.NewLine + "Description: " + AvailableNetworkInterface.Description + Environment.NewLine + "ID: " + AvailableNetworkInterface.Id
                        };
                        NewListOfEthernetAdapter.Add(NewComboBoxEhternetDevice);
                        break;
                    }
            }
        }

        // Set EthernetInterfacesComboBox properties
        EthernetInterfacesComboBox.ItemsSource = NewListOfEthernetAdapter;
    }

    private async void StartPPPwnButton_Click(object? sender, RoutedEventArgs e)
    {
        if (StartPPPwnButton.Content!.ToString() == "Stop PPPwn")
        {
            if (PPPwnRunning == true)
            {
                // Stop PPPwn
                foreach (var p in Process.GetProcessesByName("pppwn"))
                {
                    try
                    {
                        if (!p.CloseMainWindow())
                        {
                            p.Kill();
                        }
                        p.WaitForExit(2500);

                        PPPwnRunning = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to stop pppwn - {p.Id}: {ex.Message}");
                        Trace.WriteLine($"Failed to stop pppwn - {p.Id}: {ex.Message}");
                    }
                    finally
                    {
                        p.Dispose();
                    }
                }

                // Update Button
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        StartPPPwnButton.IsEnabled = false;
                        StartPPPwnButton.Content = "Start PPPWn";
                    });
                }
                else
                {
                    StartPPPwnButton.IsEnabled = false;
                    StartPPPwnButton.Content = "Start PPPWn";
                }
            }
        }
        else if (EthernetInterfacesComboBox.SelectedItem is not null && FirmwaresComboBox.SelectedItem is not null)
        {
            if (File.Exists(OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "pppwn.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "pppwn")))
            {
                // Get selected Ethernet interface
                ComboBoxEthernetDevice SelectedEthernetInterfaceInComboBox = (ComboBoxEthernetDevice)EthernetInterfacesComboBox.SelectedItem;
                string SelectedEthernetInterface = @"\Device\NPF_" + SelectedEthernetInterfaceInComboBox.AdapterID;

                // Set firmware
                string SelectedFirmware = "";
                switch (FirmwaresComboBox.Text ?? "")
                {
                    case "7.50 / 7.51 / 7.55":
                        {
                            SelectedFirmware = "750";
                            break;
                        }
                    case "8.00 / 8.01 / 8.03":
                        {
                            SelectedFirmware = "800";
                            break;
                        }
                    case "8.50 / 8.52":
                        {
                            SelectedFirmware = "850";
                            break;
                        }
                    case "9.00":
                        {
                            SelectedFirmware = "900";
                            break;
                        }
                    case "9.03 / 9.04":
                        {
                            SelectedFirmware = "903";
                            break;
                        }
                    case "9.50 / 9.51 / 9.60":
                        {
                            SelectedFirmware = "950";
                            break;
                        }
                    case "10.00 / 10.01":
                        {
                            SelectedFirmware = "1000";
                            break;
                        }
                    case "10.50 / 10.70 / 10.71":
                        {
                            SelectedFirmware = "1050";
                            break;
                        }
                    case "11.00":
                        {
                            SelectedFirmware = "1100";
                            break;
                        }
                }

                // Set the files for stage1 & stage2
                string Stage1File = "";
                string Stage2File = "";
                if (UseCustomStageFilesCheckBox.IsChecked == true)
                {
                    Stage1File = CustomStage1PayloadTextBox.Text!;
                    Stage2File = CustomStage2PayloadTextBox.Text!;
                }
                else
                {
                    switch (SelectedFirmware ?? "")
                    {
                        case "750":
                            {
                                Stage1File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage1", "ToF-stage1-750.bin");
                                Stage2File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage2", "ToF-stage2-750.bin");
                                break;
                            }
                        case "800":
                            {
                                Stage1File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage1", "ToF-stage1-800.bin");
                                Stage2File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage2", "ToF-stage2-800.bin");
                                break;
                            }
                        case "850":
                            {
                                Stage1File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage1", "ToF-stage1-850.bin");
                                Stage2File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage2", "ToF-stage2-850.bin");
                                break;
                            }
                        case "900":
                            {
                                Stage1File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage1", "SiS-stage1-900.bin");
                                Stage2File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage2", "SiS-stage2-900.bin");
                                break;
                            }
                        case "903":
                            {
                                Stage1File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage1", "ToF-stage1-903.bin");
                                Stage2File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage2", "ToF-stage2-903.bin");
                                break;
                            }
                        case "950":
                            {
                                Stage1File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage1", "SiS-stage1-950.bin");
                                Stage2File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage2", "SiS-stage2-950.bin");
                                break;
                            }
                        case "1000":
                            {
                                Stage1File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage1", "SiS-stage1-1000.bin");
                                Stage2File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage2", "SiS-stage2-1000.bin");
                                break;
                            }
                        case "1050":
                            {
                                Stage1File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage1", "ToF-stage1-1050.bin");
                                Stage2File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage2", "ToF-stage2-1050.bin");
                                break;
                            }
                        case "1100":
                            {
                                Stage1File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage1", "SiS-stage1-1100.bin");
                                Stage2File = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "stage2", "SiS-stage2-1100.bin");
                                break;
                            }
                    }
                }

                // Build the arguments string
                var NewStringBuilder = new StringBuilder();
                NewStringBuilder.Append($"--interface \"{SelectedEthernetInterface}\" --fw {SelectedFirmware} --stage1 \"{Stage1File}\" --stage2 \"{Stage2File}\"");

                if (UseResponseTimeoutCheckBox.IsChecked == true)
                {
                    if (!string.IsNullOrEmpty(ResponseTimeoutValueTextBox.Text) && Utils.IsInt(ResponseTimeoutValueTextBox.Text))
                    {
                        NewStringBuilder.Append(" --timeout " + ResponseTimeoutValueTextBox.Text);
                    }
                }
                if (UsePinningWaitingTimeCheckBox.IsChecked == true)
                {
                    if (!string.IsNullOrEmpty(PinningWaitingTimeValueTextBox.Text) && Utils.IsInt(PinningWaitingTimeValueTextBox.Text))
                    {
                        NewStringBuilder.Append(" --wait-after-pin " + PinningWaitingTimeValueTextBox.Text);
                    }
                }
                if (UseGroomDelayCheckBox.IsChecked == true)
                {
                    if (!string.IsNullOrEmpty(GroomDelayValueTextBox.Text) && Utils.IsInt(GroomDelayValueTextBox.Text))
                    {
                        NewStringBuilder.Append(" --groom-delay " + GroomDelayValueTextBox.Text);
                    }

                }
                if (SpecifyPCAPBufferSizeCheckBox.IsChecked == true)
                {
                    if (!string.IsNullOrEmpty(PCAPBufferSizeValueTextBox.Text) && Utils.IsInt(PCAPBufferSizeValueTextBox.Text))
                    {
                        NewStringBuilder.Append(" --buffer-size " + PCAPBufferSizeValueTextBox.Text);
                    }
                }

                if (AutoRetryCheckBox.IsChecked == true)
                {
                    NewStringBuilder.Append(" --auto-retry");
                }
                if (DontWaitPADICheckBox.IsChecked == true)
                {
                    NewStringBuilder.Append(" --no-wait-padi");
                }
                if (UseCPUCheckBox.IsChecked == true)
                {
                    NewStringBuilder.Append(" --real-sleep");
                }

                PPPwnRunning = true;

                // Run PPPwn
                PPPwnWoker.RunWorkerAsync(NewStringBuilder.ToString());

                // Update button
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        EthernetInterfacesComboBox.IsEnabled = false;
                        FirmwaresComboBox.IsEnabled = false;
                        AutoRetryCheckBox.IsEnabled = false;
                        UseResponseTimeoutCheckBox.IsEnabled = false;
                        UsePinningWaitingTimeCheckBox.IsEnabled = false;
                        UseGroomDelayCheckBox.IsEnabled = false;
                        SpecifyPCAPBufferSizeCheckBox.IsEnabled = false;
                        DontWaitPADICheckBox.IsEnabled = false;
                        UseCPUCheckBox.IsEnabled = false;
                        ResponseTimeoutValueTextBox.IsEnabled = false;
                        PinningWaitingTimeValueTextBox.IsEnabled = false;
                        GroomDelayValueTextBox.IsEnabled = false;
                        PCAPBufferSizeValueTextBox.IsEnabled = false;

                        StartPPPwnButton.Content = "Stop PPPWn";
                    });
                }
                else
                {
                    EthernetInterfacesComboBox.IsEnabled = false;
                    FirmwaresComboBox.IsEnabled = false;
                    AutoRetryCheckBox.IsEnabled = false;
                    UseResponseTimeoutCheckBox.IsEnabled = false;
                    UsePinningWaitingTimeCheckBox.IsEnabled = false;
                    UseGroomDelayCheckBox.IsEnabled = false;
                    SpecifyPCAPBufferSizeCheckBox.IsEnabled = false;
                    DontWaitPADICheckBox.IsEnabled = false;
                    UseCPUCheckBox.IsEnabled = false;
                    ResponseTimeoutValueTextBox.IsEnabled = false;
                    PinningWaitingTimeValueTextBox.IsEnabled = false;
                    GroomDelayValueTextBox.IsEnabled = false;
                    PCAPBufferSizeValueTextBox.IsEnabled = false;

                    StartPPPwnButton.Content = "Stop PPPWn";
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find PPPwn", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please select your Ethernet interface, PS4 firmware and Payload.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void BrowseStage1PayloadButton_Click(object? sender, RoutedEventArgs e)
    {
        var binFileFilter = new FileDialogFilter
        {
            Name = "BIN File",
            Extensions = ["bin"]
        };
        var OFD = new OpenFileDialog() { Title = "Select a stage1 payload", Filters = { binFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            CustomStage1PayloadTextBox.Text = OFDResult[0];
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No file selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void BrowseStage2PayloadButton_Click(object? sender, RoutedEventArgs e)
    {
        var binFileFilter = new FileDialogFilter
        {
            Name = "BIN File",
            Extensions = ["bin"]
        };
        var OFD = new OpenFileDialog() { Title = "Select a stage2 payload", Filters = { binFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            CustomStage2PayloadTextBox.Text = OFDResult[0];
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No file selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private void PPPwnWoker_DoWork(object? sender, DoWorkEventArgs e)
    {
        // Set PPPwn process properties
        Process PPPwn = new();
        PPPwn.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "pppwn.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "pppwn");
        PPPwn.StartInfo.Arguments = e.Argument!.ToString();
        PPPwn.StartInfo.RedirectStandardOutput = true;
        PPPwn.StartInfo.RedirectStandardError = true;
        PPPwn.StartInfo.UseShellExecute = false;
        PPPwn.StartInfo.CreateNoWindow = true;
        PPPwn.EnableRaisingEvents = true;

        PPPwn.OutputDataReceived += (SenderProcess, DataArgs) =>
        {
            if (!string.IsNullOrEmpty(DataArgs.Data))
            {
                // Append output log from PPPWn
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        LogTextBox.Text += (DataArgs.Data + "\r\n");
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    });
                }
                else
                {
                    LogTextBox.Text += (DataArgs.Data + "\r\n");
                    ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    LogTextBoxScrollViewer?.ScrollToEnd();
                }
            }
        };

        PPPwn.ErrorDataReceived += (SenderProcess, DataArgs) =>
        {
            if (!string.IsNullOrEmpty(DataArgs.Data))
            {
                // Append error log from PPPWn
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        LogTextBox.Text += (DataArgs.Data + "\r\n");
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    });
                }
                else
                {
                    LogTextBox.Text += (DataArgs.Data + "\r\n");
                    ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    LogTextBoxScrollViewer?.ScrollToEnd();
                }
            }
        };

        PPPwn.Exited += (s, e) =>
        {
            PPPwn.Dispose();

            // Update button on exit
            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    EthernetInterfacesComboBox.IsEnabled = true;
                    FirmwaresComboBox.IsEnabled = true;
                    AutoRetryCheckBox.IsEnabled = true;
                    UseResponseTimeoutCheckBox.IsEnabled = true;
                    UsePinningWaitingTimeCheckBox.IsEnabled = true;
                    UseGroomDelayCheckBox.IsEnabled = true;
                    SpecifyPCAPBufferSizeCheckBox.IsEnabled = true;
                    DontWaitPADICheckBox.IsEnabled = true;
                    UseCPUCheckBox.IsEnabled = true;
                    ResponseTimeoutValueTextBox.IsEnabled = true;
                    PinningWaitingTimeValueTextBox.IsEnabled = true;
                    GroomDelayValueTextBox.IsEnabled = true;
                    PCAPBufferSizeValueTextBox.IsEnabled = true;

                    StartPPPwnButton.Content = "Start PPPWn";
                });
            }
            else
            {
                EthernetInterfacesComboBox.IsEnabled = true;
                FirmwaresComboBox.IsEnabled = true;
                AutoRetryCheckBox.IsEnabled = true;
                UseResponseTimeoutCheckBox.IsEnabled = true;
                UsePinningWaitingTimeCheckBox.IsEnabled = true;
                UseGroomDelayCheckBox.IsEnabled = true;
                SpecifyPCAPBufferSizeCheckBox.IsEnabled = true;
                DontWaitPADICheckBox.IsEnabled = true;
                UseCPUCheckBox.IsEnabled = true;
                ResponseTimeoutValueTextBox.IsEnabled = true;
                PinningWaitingTimeValueTextBox.IsEnabled = true;
                GroomDelayValueTextBox.IsEnabled = true;
                PCAPBufferSizeValueTextBox.IsEnabled = true;

                StartPPPwnButton.Content = "Start PPPWn";
            }
        };

        // Start PPPwn & read process output data
        PPPwn.Start();
        PPPwn.BeginOutputReadLine();
        PPPwn.BeginErrorReadLine();
    }

    private async void CopyGoldHENButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select an USB drive" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "goldhen.bin")))
            {
                try
                {
                    File.Copy(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "goldhen.bin"), Path.Combine(FBDResult, "goldhen.bin"), true);

                    var box = MessageBoxManager.GetMessageBoxStandard("Info", "Copy done!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (Exception)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not copy GoldHEN to selected USB drive.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find goldhen.bin", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No USB drive selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void DownloadGoldHENButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select an USB drive" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            try
            {
                if (await Utils.IsURLValid("http://X.X.X.X/ps4/ex/goldhen_v2.4b18.7.bin"))
                {

                    string GoldHENURL = "http://X.X.X.X/ps4/ex/goldhen_v2.4b18.7.bin";
                    string DestinationPath = Path.Combine(FBDResult, "goldhen.bin");

                    using (var NewHttpClient = new HttpClient())
                    {
                        using var NewHttpResponseMessage = await NewHttpClient.GetAsync(GoldHENURL, HttpCompletionOption.ResponseHeadersRead);
                        NewHttpResponseMessage.EnsureSuccessStatusCode();
                        using var NewFileStream = new FileStream(DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
                        await NewHttpResponseMessage.Content.CopyToAsync(NewFileStream);
                    }

                    var box = MessageBoxManager.GetMessageBoxStandard("Info", "GoldHEN downloaded to : " + FBDResult + "goldhen.bin", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download GoldHEN to : " + FBDResult + "goldhen.bin" + Environment.NewLine + "File is not available.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
            catch (Exception)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not copy GoldHEN to selected USB drive.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
    }

}