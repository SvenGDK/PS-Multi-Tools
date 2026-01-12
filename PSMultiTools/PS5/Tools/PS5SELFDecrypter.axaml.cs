using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PSMultiTools.PS5.Tools;

public partial class PS5SELFDecrypter : Window
{

    public string PayloadPath = "";
    public string PS5Host = "";
    public string PS5Port = "";

    public TcpClient NewTcpClient = new();

    public PS5SELFDecrypter()
    {
        InitializeComponent();
    }

    private async void ListenButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(IPAddressTextBox.Text) && !string.IsNullOrEmpty(PortTextBox.Text) && !string.IsNullOrEmpty(PayloadPathTextBox.Text))
        {
            if (ListenButton.Content!.ToString() == "Send Payload and Start Listening")
            {

                PayloadPath = PayloadPathTextBox.Text;
                PS5Host = IPAddressTextBox.Text;
                PS5Port = PortTextBox.Text;

                ListenButton.Content = "Stop Listening";

                await SendPayloadAndReceiveAsync();
            }
            else
            {
                if (NewTcpClient.Connected)
                {
                    try
                    {
                        NewTcpClient.Client.Shutdown(SocketShutdown.Both);
                        NewTcpClient.Close();
                    }
                    catch (Exception ex)
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }
                }

                ListenButton.Content = "Send Payload and Start Listening";
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please check your input.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async Task SendPayloadAndReceiveAsync()
    {
        try
        {
            await NewTcpClient.ConnectAsync(PS5Host, Convert.ToInt32(PS5Port));

            using var NewNetworkStream = NewTcpClient.GetStream();
            NewNetworkStream.ReadTimeout = 600000;
            NewNetworkStream.WriteTimeout = 600000;

            using (var NewFileStream = new FileStream(PayloadPath, FileMode.Open, FileAccess.Read))
            {
                var SendBuffer = new byte[8192];
                int BytesRead;
                do
                {
                    BytesRead = await NewFileStream.ReadAsync(SendBuffer);
                    if (BytesRead > 0)
                    {
                        await NewNetworkStream.WriteAsync(SendBuffer.AsMemory(0, BytesRead));
                    }
                }
                while (BytesRead != 0);
            }

            NewTcpClient.Client.Shutdown(SocketShutdown.Send);

            var ReceiveBuffer = new byte[8192];
            int BytesReceived;
            do
            {
                BytesReceived = await NewNetworkStream.ReadAsync(ReceiveBuffer);
                if (BytesReceived > 0)
                {
                    ListeningLogTextBox.Text += (Encoding.ASCII.GetString(ReceiveBuffer, 0, BytesReceived) + "\r\n");
                    ScrollViewer? LogTextBoxScrollViewer = ListeningLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    LogTextBoxScrollViewer?.ScrollToEnd();
                }
            }
            while (BytesReceived > 0);

            NewTcpClient.Client.Shutdown(SocketShutdown.Both);
            NewTcpClient.Close();

            await Dispatcher.UIThread.Invoke(async () =>
            {
                if (ListeningLogTextBox.Text != null && ListeningLogTextBox.Text.Contains("Dump Complete!"))
                {


                    ListenButton.Content = "Send Payload and Start Listening";
                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "Success! Dump completed.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            });
        }
        catch (Exception ex)
        {
            ListeningLogTextBox.Text += ("An error occurred: " + ex.Message);
        }
    }

    private async void BrowsePayloadButton_Click(object? sender, RoutedEventArgs e)
    {
        var elfFileFilter = new FileDialogFilter
        {
            Name = "ELF File",
            Extensions = ["elf"]
        };
        var OFD = new OpenFileDialog() { Filters = { elfFileFilter }, AllowMultiple = false, Title = "Select a ps5-self-decrypter payload" };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            PayloadPathTextBox.Text = OFDResult[0];
        }
    }

}