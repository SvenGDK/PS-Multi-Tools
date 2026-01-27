using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace PSMultiTools.PS5.Tools;

public partial class PS5Sender : Window
{

    public struct WorkerArgs
    {
        public IPAddress DeviceIP { get; set; }

        public int DevicePort { get; set; }

        public string FileToSend { get; set; }

        public int ChunkSize { get; set; }
    }

    public struct DownloadedPayloadItem
    {
        public string PayloadPath { get; set; }

        public string PayloadName { get; set; }
    }

    public enum SendType
    {
        PAYLOAD,
        ISO,
        CONF
    }

    public string ConsoleIP = "";
    public string? ReceivedPayload;

    public BackgroundWorker DefaultSenderWorker = new() { WorkerReportsProgress = true };
    public BackgroundWorker SenderWorker = new() { WorkerReportsProgress = true };

    public readonly uint Magic = 0xEA6EU;
    public int TotalBytes = 0;
    public SendType CurrentType = new();
    public string SelectedISO = "";

    public PS5Sender()
    {
        InitializeComponent();

        Loaded += PS5Sender_Loaded;
        SenderWorker.DoWork += SenderWorker_DoWork;
        SenderWorker.RunWorkerCompleted += SenderWorker_RunWorkerCompleted;
        DefaultSenderWorker.DoWork += DefaultSenderWorker_DoWork;
        DefaultSenderWorker.RunWorkerCompleted += DefaultSenderWorker_RunWorkerCompleted;

        IPTextBox.TextInput += IPTextBox_TextInput;
        PortTextBox.TextInput += PortTextBox_TextInput;
    }

    private void PS5Sender_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedISO))
        {
            SelectedELFTextBox.Text = SelectedISO;
        }
        if (!string.IsNullOrEmpty(ConsoleIP))
        {
            IPTextBox.Text = ConsoleIP;
        }
        if (ReceivedPayload != null)
        {
            SelectedELFTextBox.Text = ReceivedPayload;
        }

        // Check for downloaded payloads and add to DownloadedPayloadsComboBox
        try
        {
            if (Directory.Exists(Utils.GetDownloadsFolderPath()))
            {
                var PayloadList = Directory.EnumerateFiles(Utils.GetDownloadsFolderPath(), "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".elf") || s.EndsWith(".bin") || s.EndsWith(".js"));
                foreach (var Payload in PayloadList)
                {
                    var NewDownloadedPayloadItem = new DownloadedPayloadItem() { PayloadName = Path.GetFileName(Payload), PayloadPath = Payload };
                    DownloadedPayloadsComboBox.Items.Add(NewDownloadedPayloadItem);
                }
            }
        }
        catch { }
    }

    private async void SendButton_Click(object? sender, RoutedEventArgs e)
    {
        // Check first if not both items are selected
        if (!string.IsNullOrEmpty(SelectedELFTextBox.Text) && DownloadedPayloadsComboBox.SelectedItem is not null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Info", "To prevent sending 2 payloads at the same time you need to mnually remove the selected payload or downloaded payload.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
        else
        {
            string SelectedPayload = "";
            if (!string.IsNullOrEmpty(SelectedELFTextBox.Text))
            {
                // Set payload to manually selected file
                SelectedPayload = SelectedELFTextBox.Text;
            }
            else if (DownloadedPayloadsComboBox.SelectedItem is not null)
            {
                // Set payload to selected downloaded file
                if (DownloadedPayloadsComboBox.SelectedItem is DownloadedPayloadItem item)
                {
                    SelectedPayload = item.PayloadPath;
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("No file selected", "Please select a file first.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
                return;
            }

            // Check if an IP address was entered
            if (!string.IsNullOrWhiteSpace(IPTextBox.Text))
            {
                IPAddress DeviceIP;
                try
                {
                    DeviceIP = IPAddress.Parse(IPTextBox.Text);
                }
                catch (FormatException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error sending payload", "Could not send selected payload. Please check your IP.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                    return;
                }

                var ELFFileInfo = new FileInfo(SelectedPayload);

                SendButton.IsEnabled = false;
                SendISOButton.IsEnabled = false;
                BrowseButton.IsEnabled = false;
                BrowseISOButton.IsEnabled = false;

                // Set the progress bar maximum and TotalBytes to send
                SendProgressBar.Value = 0d;
                SendProgressBar.Maximum = (double)ELFFileInfo.Length;
                TotalBytes = (int)ELFFileInfo.Length;

                // Start sending
                CurrentType = SendType.PAYLOAD;

                if (!string.IsNullOrEmpty(PortTextBox.Text))
                {
                    if (PortTextBox.Text == "9045") // Mast1c0re
                    {
                        SenderWorker.RunWorkerAsync(new WorkerArgs() { DeviceIP = DeviceIP, FileToSend = SelectedPayload, ChunkSize = 4096 });
                    }
                    else // Any other elf loader
                    {
                        int DevicePort = int.Parse(PortTextBox.Text);
                        DefaultSenderWorker.RunWorkerAsync(new WorkerArgs() { DeviceIP = DeviceIP, FileToSend = SelectedPayload, DevicePort = DevicePort });
                    }
                }

                // Reset selected combobox item
                DownloadedPayloadsComboBox.SelectedItem = null;
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("No IP address", "No IP address was entered." + Environment.NewLine + "Please enter an IP address on the main window and re-open the backup manager.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
    }

    private async void SendISOButton_Click(object? sender, RoutedEventArgs e)
    {
        // Check if a game is selected
        if (!string.IsNullOrEmpty(SelectedISOTextBox.Text))
        {
            // Check if an IP address was entered before
            if (!string.IsNullOrWhiteSpace(IPTextBox.Text))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Send " + SelectedISOTextBox.Text + " to the console ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                var boxresult = await box.ShowWindowDialogAsync(this);
                if (boxresult == ButtonResult.Yes)
                {
                    var DeviceIP = IPAddress.Parse(IPTextBox.Text);
                    var GameFileInfo = new FileInfo(SelectedISOTextBox.Text);

                    SendButton.IsEnabled = false;
                    SendISOButton.IsEnabled = false;
                    BrowseButton.IsEnabled = false;
                    BrowseISOButton.IsEnabled = false;

                    // Set the progress bar maximum and TotalBytes to send
                    SendProgressBar.Maximum = (double)GameFileInfo.Length;
                    TotalBytes = (int)GameFileInfo.Length;

                    // Start sending
                    var WorkArgs = new WorkerArgs() { DeviceIP = DeviceIP, FileToSend = SelectedISOTextBox.Text, ChunkSize = 63488 };
                    CurrentType = SendType.ISO;
                    SenderWorker.RunWorkerAsync(WorkArgs);
                }
            }

            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("No IP address", "No IP address was entered." + Environment.NewLine + "Please enter an IP address on the main window and re-open the backup manager.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("No game selected", "No game selected." + Environment.NewLine + "Please select a game first.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void SendConfigButton_Click(object? sender, RoutedEventArgs e)
    {
        // Open choose config dialog
        var confFileFilter = new FileDialogFilter
        {
            Name = "PS2 Config File",
            Extensions = ["conf"]
        };
        var OFD = new OpenFileDialog() { Title = "Select a .conf file", Filters = { confFileFilter } };
        var DeviceIP = IPAddress.Parse(IPTextBox.Text!);

        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {

            SendButton.IsEnabled = false;
            SendISOButton.IsEnabled = false;
            BrowseButton.IsEnabled = false;
            BrowseISOButton.IsEnabled = false;

            var ConfigFileInfo = new FileInfo(OFDResult[0]);
            string FilePath = Path.GetFullPath(OFDResult[0]);

            // Set the progress bar maximum and TotalBytes to send
            SendProgressBar.Value = 0d;
            SendProgressBar.Maximum = (double)ConfigFileInfo.Length;
            TotalBytes = (int)ConfigFileInfo.Length;

            // Start sending
            var WorkArgs = new WorkerArgs() { DeviceIP = DeviceIP, FileToSend = FilePath, ChunkSize = 10 };
            CurrentType = SendType.CONF;
            SenderWorker.RunWorkerAsync(WorkArgs);
        }
    }

    private void SenderWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
        WorkerArgs CurrentWorkerArgs = (WorkerArgs)e.Argument!;

        var FileInfos = new FileInfo(CurrentWorkerArgs.FileToSend);
        long FileSizeAsLong = FileInfos.Length;
        ulong FileSizeAsULong = (ulong)FileInfos.Length;

        byte[] MagicBytes = BytesConverter.ToLittleEndian(Magic);
        byte[] NewFileSizeBytes = BytesConverter.ToLittleEndian(FileSizeAsULong);

        using var SenderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = 3000 };

        SenderSocket.Connect(CurrentWorkerArgs.DeviceIP, 9045);

        SenderSocket.Send(MagicBytes);
        SenderSocket.Send(NewFileSizeBytes);

        int BytesRead = 0;
        int SendBytes = 0;
        var Buffer = new byte[CurrentWorkerArgs.ChunkSize];

        // Open the file and read
        using (var SenderFileStream = new FileStream(CurrentWorkerArgs.FileToSend, FileMode.Open, FileAccess.Read))
        {
            do
            {
                BytesRead = SenderFileStream.Read(Buffer, 0, Buffer.Length);

                if (BytesRead > 0)
                {
                    // Send bytes
                    SendBytes += SenderSocket.Send(Buffer, 0, BytesRead, SocketFlags.None);

                    // Update the status text
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => SendStatusTextBlock.Text = "Sending file: " + SendBytes.ToString() + " bytes of " + TotalBytes.ToString() + " bytes sent.");
                    }
                    else
                    {
                        SendStatusTextBlock.Text = "Sending file: " + SendBytes.ToString() + " of " + TotalBytes.ToString() + " sent.";
                    }

                    // Update the status progress bar
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => SendProgressBar.Value = (double)SendBytes);
                    }
                    else
                    {
                        SendProgressBar.Value = (double)SendBytes;
                    }

                }
            }
            while (BytesRead > 0);
        }

        // Close the connection
        SenderSocket.Close();
    }

    private async void SenderWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        SendStatusTextBlock.Text = "Status:";
        SendProgressBar.Value = 0;

        if (!e.Cancelled)
        {
            switch (CurrentType)
            {
                case SendType.PAYLOAD:
                    {
                        SendConfigButton.IsEnabled = true;
                        SendButton.IsEnabled = true;
                        SendISOButton.IsEnabled = true;
                        BrowseButton.IsEnabled = true;
                        BrowseISOButton.IsEnabled = true;

                        var box = MessageBoxManager.GetMessageBoxStandard("Success", "Payload successfully sent!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box.ShowWindowAsync();

                        break;
                    }
                case SendType.ISO:
                    {
                        SendConfigButton.IsEnabled = true;
                        SendButton.IsEnabled = true;
                        SendISOButton.IsEnabled = true;
                        BrowseButton.IsEnabled = true;
                        BrowseISOButton.IsEnabled = true;

                        var box = MessageBoxManager.GetMessageBoxStandard("Success", "Game successfully sent!" + Environment.NewLine + "You can now send a config file if you want to.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box.ShowWindowAsync();

                        break;
                    }
                case SendType.CONF:
                    {
                        SendConfigButton.IsEnabled = false;
                        SendButton.IsEnabled = true;
                        SendISOButton.IsEnabled = true;
                        BrowseButton.IsEnabled = true;
                        BrowseISOButton.IsEnabled = true;

                        var box = MessageBoxManager.GetMessageBoxStandard("Success", "Config successfully sent!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box.ShowWindowAsync();

                        break;
                    }
            }
        }
    }

    private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        var elfFileFilter = new FileDialogFilter
        {
            Name = "ELF File",
            Extensions = ["elf"]
        };
        var binFileFilter = new FileDialogFilter
        {
            Name = "BIN File",
            Extensions = ["bin"]
        };
        var jsFileFilter = new FileDialogFilter
        {
            Name = "JavaScript File",
            Extensions = ["js"]
        };

        var OFD = new OpenFileDialog() { Title = "Select an .elf, .bin or .js file", Filters = { elfFileFilter, binFileFilter, jsFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedELFTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseISOButton_Click(object? sender, RoutedEventArgs e)
    {
        var isoFileFilter = new FileDialogFilter
        {
            Name = "ISO File",
            Extensions = ["iso"]
        };
        var OFD = new OpenFileDialog() { Title = "Select an .iso file", Filters = { isoFileFilter } };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedISOTextBox.Text = OFDResult[0];
        }
    }

    private void DefaultSenderWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
        WorkerArgs CurrentWorkerArgs = (WorkerArgs)e.Argument!;

        using var SenderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = 3000 };
        // Connect
        SenderSocket.Connect(CurrentWorkerArgs.DeviceIP, CurrentWorkerArgs.DevicePort);
        // Send payload
        SenderSocket.SendFile(CurrentWorkerArgs.FileToSend);
        // Close the connection
        SenderSocket.Close();
    }

    private async void DefaultSenderWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess() == false)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                SendConfigButton.IsEnabled = true;
                SendButton.IsEnabled = true;
                SendISOButton.IsEnabled = true;
                BrowseButton.IsEnabled = true;
                BrowseISOButton.IsEnabled = true;

                var box = MessageBoxManager.GetMessageBoxStandard("Success", "Successfully sent!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            });
        }
        else
        {
            SendConfigButton.IsEnabled = true;
            SendButton.IsEnabled = true;
            SendISOButton.IsEnabled = true;
            BrowseButton.IsEnabled = true;
            BrowseISOButton.IsEnabled = true;

            var box = MessageBoxManager.GetMessageBoxStandard("Success", "Successfully sent!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private void PortTextBox_TextInput(object? sender, TextInputEventArgs e)
    {
        var numbersOnly = PortInputRegex();
        if (e.Text != null)
        {
            if (numbersOnly.IsMatch(e.Text))
                e.Handled = true;
        }
    }

    private void IPTextBox_TextInput(object? sender, TextInputEventArgs e)
    {
        var onlyNumbersAndDot = IPInputRegex();
        if (e.Text != null)
        {
            if (onlyNumbersAndDot.IsMatch(e.Text))
                e.Handled = true;
        }
    }

    [GeneratedRegex("[^0-9]+")]
    private static partial Regex PortInputRegex();
    [GeneratedRegex("[^0-9.]+")]
    private static partial Regex IPInputRegex();

}