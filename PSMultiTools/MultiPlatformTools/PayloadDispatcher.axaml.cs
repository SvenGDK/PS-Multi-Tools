using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PSMultiTools.MultiPlatformTools;

public partial class PayloadDispatcher : Window
{

    public string ConsoleIP = "";

    private readonly FileDialogFilter elfFileFilter = new() { Name = "ELF File", Extensions = ["elf"] };
    private readonly FileDialogFilter binFileFilter = new() { Name = "BIN File", Extensions = ["bin"] };
    private readonly FileDialogFilter jsFileFilter = new() { Name = "JavaScript File", Extensions = ["js"] };

    private CancellationTokenSource? SendingCTS;

    public PayloadDispatcher()
    {
        InitializeComponent();
        Loaded += PayloadDispatcher_Loaded;
    }

    private void PayloadDispatcher_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ConsoleIP))
        {
            IPAddressTextBox.Text = ConsoleIP;
        }
    }

    private async void BrowsePayload1Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var OFD = new OpenFileDialog() { Title = "Select an .elf, .bin or .js file", Filters = { elfFileFilter, binFileFilter, jsFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            Payload1PathTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowsePayload2Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var OFD = new OpenFileDialog() { Title = "Select an .elf, .bin or .js file", Filters = { elfFileFilter, binFileFilter, jsFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            Payload2PathTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowsePayload3Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var OFD = new OpenFileDialog() { Title = "Select an .elf, .bin or .js file", Filters = { elfFileFilter, binFileFilter, jsFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            Payload3PathTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowsePayload4Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var OFD = new OpenFileDialog() { Title = "Select an .elf, .bin or .js file", Filters = { elfFileFilter, binFileFilter, jsFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            Payload4PathTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowsePayload5Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var OFD = new OpenFileDialog() { Title = "Select an .elf, .bin or .js file", Filters = { elfFileFilter, binFileFilter, jsFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            Payload5PathTextBox.Text = OFDResult[0];
        }
    }

    private async void DispatchPayloadsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DispatchPayloadsButton.Content!.ToString() == "Dispatch Payloads")
        {
            await Dispatcher.UIThread.InvokeAsync(() => DispatchPayloadsButton.Content = "Stop dispatching");

            // Determine which payloads to send
            bool dispatchPayload1 = false;
            bool dispatchPayload2 = false;
            bool dispatchPayload3 = false;
            bool dispatchPayload4 = false;
            bool dispatchPayload5 = false;

            if (!string.IsNullOrEmpty(Payload1PathTextBox.Text) && !string.IsNullOrEmpty(DispatchPayload1InTextBox.Text) && !string.IsNullOrEmpty(DispatchPayload1OnPortTextBox.Text))
                dispatchPayload1 = true;
            if (!string.IsNullOrEmpty(Payload2PathTextBox.Text) && !string.IsNullOrEmpty(DispatchPayload2InTextBox.Text) && !string.IsNullOrEmpty(DispatchPayload2OnPortTextBox.Text))
                dispatchPayload2 = true;
            if (!string.IsNullOrEmpty(Payload3PathTextBox.Text) && !string.IsNullOrEmpty(DispatchPayload3InTextBox.Text) && !string.IsNullOrEmpty(DispatchPayload3OnPortTextBox.Text))
                dispatchPayload3 = true;
            if (!string.IsNullOrEmpty(Payload4PathTextBox.Text) && !string.IsNullOrEmpty(DispatchPayload4InTextBox.Text) && !string.IsNullOrEmpty(DispatchPayload4OnPortTextBox.Text))
                dispatchPayload4 = true;
            if (!string.IsNullOrEmpty(Payload5PathTextBox.Text) && !string.IsNullOrEmpty(DispatchPayload5InTextBox.Text) && !string.IsNullOrEmpty(DispatchPayload5OnPortTextBox.Text))
                dispatchPayload5 = true;

            if (!dispatchPayload1 && !dispatchPayload2 && !dispatchPayload3 && !dispatchPayload4 && !dispatchPayload5)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "No payload to dispatch. Please enter all values in order to send a payload.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                });
                return;
            }

            if (string.IsNullOrEmpty(IPAddressTextBox.Text))
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "No IP address!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                });
                return;
            }

            // Create a cancellation token
            SendingCTS?.Dispose();
            SendingCTS = new CancellationTokenSource();
            var SendingCTSToken = SendingCTS.Token;

            try
            {
                if (dispatchPayload1)
                {
                    Socket SenderSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = 3000 };
                    string payload1Filename = Path.GetFileName(Payload1PathTextBox.Text!);
                    // Output info to log
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LogTextBox.Text += $"Dispatching {payload1Filename} in {DispatchPayload1InTextBox.Text}sec ...\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    });

                    // Delay until sending the payload
                    await Task.Delay(TimeSpan.FromSeconds(int.Parse(DispatchPayload1InTextBox.Text!)));
                    // Connect
                    await SenderSocket.ConnectAsync(IPAddressTextBox.Text, int.Parse(DispatchPayload1OnPortTextBox.Text!), SendingCTSToken);
                    // Send payload
                    await SenderSocket.SendFileAsync(Payload1PathTextBox.Text, SendingCTSToken);
                    // Close the connection
                    SenderSocket.Close();

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LogTextBox.Text += $"{payload1Filename} has been sent!\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    });
                }

                if (dispatchPayload2)
                {
                    Socket SenderSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = 3000 };
                    string payload2Filename = Path.GetFileName(Payload2PathTextBox.Text!);
                    // Output info to log
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LogTextBox.Text += $"Dispatching {payload2Filename} in {DispatchPayload2InTextBox.Text}sec ...\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    });

                    // Delay until sending the payload
                    await Task.Delay(TimeSpan.FromSeconds(int.Parse(DispatchPayload2InTextBox.Text!)));
                    // Connect
                    await SenderSocket.ConnectAsync(IPAddressTextBox.Text, int.Parse(DispatchPayload2OnPortTextBox.Text!), SendingCTSToken);
                    // Send payload
                    await SenderSocket.SendFileAsync(Payload2PathTextBox.Text, SendingCTSToken);
                    // Close the connection
                    SenderSocket.Close();

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LogTextBox.Text += $"{payload2Filename} has been sent!\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    });
                }

                if (dispatchPayload3)
                {
                    Socket SenderSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = 3000 };
                    string payload3Filename = Path.GetFileName(Payload3PathTextBox.Text!);
                    // Output info to log
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LogTextBox.Text += $"Dispatching {payload3Filename} in {DispatchPayload3InTextBox.Text}sec ...\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    });

                    // Delay until sending the payload
                    await Task.Delay(TimeSpan.FromSeconds(int.Parse(DispatchPayload3InTextBox.Text!)));
                    // Connect
                    await SenderSocket.ConnectAsync(IPAddressTextBox.Text, int.Parse(DispatchPayload3OnPortTextBox.Text!), SendingCTSToken);
                    // Send payload
                    await SenderSocket.SendFileAsync(Payload3PathTextBox.Text, SendingCTSToken);
                    // Close the connection
                    SenderSocket.Close();

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LogTextBox.Text += $"{payload3Filename} has been sent!\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    });
                }

                if (dispatchPayload4)
                {
                    Socket SenderSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = 3000 };
                    string payload4Filename = Path.GetFileName(Payload4PathTextBox.Text!);
                    // Output info to log
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LogTextBox.Text += $"Dispatching {payload4Filename} in {DispatchPayload4InTextBox.Text}sec ...\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    });

                    // Delay until sending the payload
                    await Task.Delay(TimeSpan.FromSeconds(int.Parse(DispatchPayload4InTextBox.Text!)));
                    // Connect
                    await SenderSocket.ConnectAsync(IPAddressTextBox.Text, int.Parse(DispatchPayload4OnPortTextBox.Text!), SendingCTSToken);
                    // Send payload
                    await SenderSocket.SendFileAsync(Payload4PathTextBox.Text, SendingCTSToken);
                    // Close the connection
                    SenderSocket.Close();

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LogTextBox.Text += $"{payload4Filename} has been sent!\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    });
                }

                if (dispatchPayload5)
                {
                    Socket SenderSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = 3000 };
                    string payload5Filename = Path.GetFileName(Payload5PathTextBox.Text!);
                    // Output info to log
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LogTextBox.Text += $"Dispatching {payload5Filename} in {DispatchPayload5InTextBox.Text}sec ...\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    });

                    // Delay until sending the payload
                    await Task.Delay(TimeSpan.FromSeconds(int.Parse(DispatchPayload5InTextBox.Text!)));
                    // Connect
                    await SenderSocket.ConnectAsync(IPAddressTextBox.Text, int.Parse(DispatchPayload5OnPortTextBox.Text!), SendingCTSToken);
                    // Send payload
                    await SenderSocket.SendFileAsync(Payload5PathTextBox.Text, SendingCTSToken);
                    // Close the connection
                    SenderSocket.Close();

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LogTextBox.Text += $"{payload5Filename} has been sent!\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    });
                }
            }
            catch (OperationCanceledException)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LogTextBox.Text += "Dispatching cancelled.\n";
                    ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    LogTextBoxScrollViewer?.ScrollToEnd();
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LogTextBox.Text += $"Exception while dispatching {ex.Message}\n";
                    ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    LogTextBoxScrollViewer?.ScrollToEnd();
                });
            }
            finally
            {
                SendingCTS.Dispose();
                SendingCTS = null;
            }

            await Dispatcher.UIThread.Invoke(async () =>
            {
                DispatchPayloadsButton.Content = "Dispatch Payloads";
                var box = MessageBoxManager.GetMessageBoxStandard("Done", "All selected paylods have been sent!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                await box.ShowWindowAsync();
            });
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() => DispatchPayloadsButton.Content = "Dispatch Payloads");
            SendingCTS?.CancelAsync();
        }
    }

}