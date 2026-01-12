using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Net.Sockets;

namespace PSMultiTools.PS5.Tools;

public partial class PS5PortChecker : Window
{

    public string PS5Host = "";

    public PS5PortChecker()
    {
        InitializeComponent();
        Loaded += PS5PortChecker_Loaded;
    }

    private void PS5PortChecker_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PS5Host))
        {
            IPAddressTextBox.Text = PS5Host;

            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() => Cursor = new Cursor(StandardCursorType.Wait));
            }
            else
            {
                Cursor = new Cursor(StandardCursorType.Wait);
            }

            CheckAllPorts();
        }
    }

    private async void CheckAllPorts()
    {
        if (!string.IsNullOrEmpty(IPAddressTextBox.Text))
        {

            // Payload Loader
            if (CheckPort(IPAddressTextBox.Text, 9020, 1000))
            {
                Port9020TextBlock.Foreground = Brushes.Green;
            }
            else
            {
                Port9020TextBlock.Foreground = Brushes.Red;
            }
            if (CheckPort(IPAddressTextBox.Text, 9021, 1000))
            {
                Port9021TextBlock.Foreground = Brushes.Green;
            }
            else
            {
                Port9021TextBlock.Foreground = Brushes.Red;
            }

            // FTP
            if (CheckPort(IPAddressTextBox.Text, 1337, 1000))
            {
                Port1337TextBlock.Foreground = Brushes.Green;
            }
            else
            {
                Port1337TextBlock.Foreground = Brushes.Red;
            }
            if (CheckPort(IPAddressTextBox.Text, 2121, 1000))
            {
                Port2121TextBlock.Foreground = Brushes.Green;
            }
            else
            {
                Port2121TextBlock.Foreground = Brushes.Red;
            }

            // Klog
            if (CheckPort(IPAddressTextBox.Text, 3232, 1000))
            {
                Port3232TextBlock.Foreground = Brushes.Green;
            }
            else
            {
                Port3232TextBlock.Foreground = Brushes.Red;
            }
            if (CheckPort(IPAddressTextBox.Text, 9081, 1000))
            {
                Port9081TextBlock.Foreground = Brushes.Green;
            }
            else
            {
                Port9081TextBlock.Foreground = Brushes.Red;
            }

            // Discord RPC server
            if (CheckPort(IPAddressTextBox.Text, 8000, 1000))
            {
                Port8000TextBlock.Foreground = Brushes.Green;
            }
            else
            {
                Port8000TextBlock.Foreground = Brushes.Red;
            }

            // Direct PKG installer services
            if (CheckPort(IPAddressTextBox.Text, 9090, 1000))
            {
                Port9090TextBlock.Foreground = Brushes.Green;
            }
            else
            {
                Port9090TextBlock.Foreground = Brushes.Red;
            }
            if (CheckPort(IPAddressTextBox.Text, 12800, 1000))
            {
                Port12800TextBlock.Foreground = Brushes.Green;
            }
            else
            {
                Port12800TextBlock.Foreground = Brushes.Red;
            }

            // WebSrv
            if (CheckPort(IPAddressTextBox.Text, 8080, 1000))
            {
                Port8080TextBlock.Foreground = Brushes.Green;
            }
            else
            {
                Port8080TextBlock.Foreground = Brushes.Red;
            }

            // SHSrv
            if (CheckPort(IPAddressTextBox.Text, 2323, 1000))
            {
                Port2323TextBlock.Foreground = Brushes.Green;
            }
            else
            {
                Port2323TextBlock.Foreground = Brushes.Red;
            }

            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() => Cursor = new Cursor(StandardCursorType.Arrow));
            }
            else
            {
                Cursor = new Cursor(StandardCursorType.Arrow);
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please enter your PS5 IP address.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private static bool CheckPort(string ConsoleIP, int PortToCheck, int CheckTimeout)
    {
        using var NewTcpClient = new TcpClient();
        try
        {
            var NewIAsyncResult = NewTcpClient.BeginConnect(ConsoleIP, PortToCheck, null, null);
            bool PortOpen = NewIAsyncResult.AsyncWaitHandle.WaitOne(CheckTimeout);

            if (!PortOpen)
            {
                return false;
            }

            NewTcpClient.EndConnect(NewIAsyncResult);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async void CheckPortButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(IPAddressTextBox.Text) && !string.IsNullOrEmpty(PortTextBox.Text))
        {
            if (CheckPort(IPAddressTextBox.Text, Convert.ToInt32(PortTextBox.Text), 1000))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Port Checker", $"Port {PortTextBox.Text} is open!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Port Checker", $"Port {PortTextBox.Text} is not open!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
    }

    private async void ReloadButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(IPAddressTextBox.Text))
        {
            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() => Cursor = new Cursor(StandardCursorType.Wait));
            }
            else
            {
                Cursor = new Cursor(StandardCursorType.Wait);
            }

            CheckAllPorts();
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please enter your PS5 IP address.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }

    }


}