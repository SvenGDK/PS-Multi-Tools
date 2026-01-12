using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PSMultiTools.PS5.Tools;

public partial class PS5Log : Window
{
    public string SavedIP = "";
    public int DefaultPort = 9081;

    public bool Highlighting = false;
    public List<HighlightItem> HighlightItems = [];

    private CancellationTokenSource? NewCancellationTokenSource;
    private LogTextMarker? NewMarkerService;

    public struct HighlightItem
    {
        public string ItemWord { get; set; }
        public string ItemColor { get; set; }
    }

    public PS5Log()
    {
        InitializeComponent();

        NewMarkerService = new LogTextMarker(LogTextEditor.Document);
        LogTextEditor.TextArea.TextView.BackgroundRenderers.Add(NewMarkerService);
        LogTextEditor.TextArea.TextView.LineTransformers.Add(NewMarkerService);
    }

    private async void ReconnectButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ReconnectButton.Content?.ToString() == "Disconnect")
        {
            NewCancellationTokenSource?.Cancel();
            ReconnectButton.Content = "Connect";
        }
        else
        {
            if (!string.IsNullOrEmpty(PS5IPTextBox.Text))
                SavedIP = PS5IPTextBox.Text;
            if (!string.IsNullOrEmpty(PS5KlogPortTextBox.Text) && int.TryParse(PS5KlogPortTextBox.Text, out var p))
                DefaultPort = p;

            ReconnectButton.Content = "Disconnect";
            NewCancellationTokenSource = new CancellationTokenSource();
            try
            {
                await Task.Run(() => KernelReadAsync(NewCancellationTokenSource.Token));
            }
            catch (OperationCanceledException) { }
            finally
            {
                ReconnectButton.Content = "Connect";
            }
        }
    }

    private async Task KernelReadAsync(CancellationToken token)
    {
        var endpoint = new IPEndPoint(IPAddress.Parse(SavedIP), DefaultPort);
        using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(endpoint);

        var buffer = new byte[4096];
        int read;
        while (!token.IsCancellationRequested && (read = socket.Receive(buffer)) > 0)
        {
            var received = Encoding.UTF8.GetString(buffer, 0, read);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (Highlighting)
                    AppendFormattedText(received);
                else
                    AppendPlainText(received);

                LogTextEditor.ScrollToEnd();
            });
        }

        socket.Close();
    }

    private void AppendPlainText(string text)
    {
        var doc = LogTextEditor.Document;
        var offset = doc.TextLength;
        doc.Insert(offset, text);
    }

    private void ClearButton_Click(object? sender, RoutedEventArgs e)
    {
        if (LogTextEditor.Document != null && NewMarkerService != null)
        {
            LogTextEditor.Document.Text = string.Empty;
            NewMarkerService.RemoveAll();
        }
    }

    public async void AppendFormattedText(string receivedString)
    {
        if (LogTextEditor.Document != null && NewMarkerService != null)
        {
            receivedString = receivedString.Replace("<78>", "").Replace("<80>", "").Replace("<118>", "");

            var doc = LogTextEditor.Document;
            var startOffset = doc.TextLength;
            doc.Insert(startOffset, receivedString);

            foreach (var hi in HighlightItems)
            {
                if (string.IsNullOrEmpty(hi.ItemWord))
                    continue;

                var pattern = Regex.Escape(hi.ItemWord);
                var matches = Regex.Matches(receivedString, pattern);
                foreach (Match m in matches)
                {
                    var absoluteStart = startOffset + m.Index;
                    var length = m.Length;

                    try
                    {
                        var brush = Brush.Parse(hi.ItemColor);
                        NewMarkerService.Create(absoluteStart, length, foreground: brush);
                    }
                    catch
                    {
                        var box2 = MessageBoxManager.GetMessageBoxStandard("Log", "Invalid color", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box2.ShowWindowDialogAsync(this);
                    }
                }
            }
        }
    }

    private async void AddHightlightButton_Click(object? sender, RoutedEventArgs e)
    {
        if (HighlightSelectionComboBox.SelectedItem != null && !string.IsNullOrEmpty(ColorCodeTextBox.Text))
        {
            try
            {
                var newItem = new HighlightItem
                {
                    ItemWord = HighlightSelectionComboBox.Text!,
                    ItemColor = ColorCodeTextBox.Text
                };
                HighlightItems.Add(newItem);
                Highlighting = true;
            }
            catch
            {
                var box2 = MessageBoxManager.GetMessageBoxStandard("Log", "Invalid color", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box2.ShowWindowDialogAsync(this);
            }
        }
    }

    private void ClearHighlightsButton_Click(object? sender, RoutedEventArgs e)
    {
        Highlighting = false;
        HighlightItems.Clear();

        if (LogTextEditor.Document != null && NewMarkerService != null)
        {
            NewMarkerService.RemoveAll();
        }
    }

    private async void ApplyBGColorButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(BackgroundColorTextBox.Text))
        {
            try
            {
                IBrush NewBackgroundBrush = Brush.Parse(BackgroundColorTextBox.Text);
                if (NewBackgroundBrush is not null)
                {
                    LogTextEditor.Background = NewBackgroundBrush;
                }
            }
            catch (Exception)
            {
                var box2 = MessageBoxManager.GetMessageBoxStandard("Log", "Invalid color", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box2.ShowWindowDialogAsync(this);
            }
        }
    }

    private async void ApplyTextColorButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(FontColorTextBox.Text))
        {
            try
            {
                IBrush NewBackgroundBrush = Brush.Parse(FontColorTextBox.Text);
                if (NewBackgroundBrush is not null)
                {
                    LogTextEditor.Foreground = NewBackgroundBrush;
                }
            }
            catch (NotSupportedException)
            {
                var box2 = MessageBoxManager.GetMessageBoxStandard("Log", "Invalid color", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box2.ShowWindowDialogAsync(this);
            }
        }
    }

    private void ResetColorButton_Click(object? sender, RoutedEventArgs e)
    {
        IBrush NewBackgroundBrush = Brush.Parse("#FF474747");
        IBrush NewTextBrush = Brush.Parse("#FFFFFFFF");

        LogTextEditor.Background = NewBackgroundBrush;
        LogTextEditor.Foreground = NewTextBrush;
    }

}