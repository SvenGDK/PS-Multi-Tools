using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace PSMultiTools.PS2.Tools;

public partial class STARExtractor : Window
{
    public STARExtractor()
    {
        InitializeComponent();
    }

    private async void BrowseSTARFileButton_Click(object? sender, RoutedEventArgs e)
    {
        var OFD = new OpenFileDialog
        {
            AllowMultiple = false,
            Title = "Select STAR file",
            Filters =
            [
                new FileDialogFilter { Name = "STAR files", Extensions = { "star" } }
            ]
        };

        var result = await OFD.ShowAsync(this);
        if (result != null && result.Length > 0)
        {
            SelectedSTARFileTextBox.Text = result[0];
        }
    }

    private async void BrowseExtractionOutputFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog
        {
            Title = "Select a folder where you want to extract the STAR file",
            Directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        var result = await dlg.ShowAsync(this);
        if (!string.IsNullOrEmpty(result))
        {
            SelectedOutputFolderTextBox.Text = result;
        }
    }

    private async void ExtractSTARFileButton_Click(object? sender, RoutedEventArgs e)
    {
        var starPath = SelectedSTARFileTextBox.Text;
        if (string.IsNullOrWhiteSpace(starPath))
            return;

        Cursor = new Cursor(StandardCursorType.Wait);

        var exePath = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "stargazer.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "stargazer");
        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var outputFolder = SelectedOutputFolderTextBox.Text;
        if (!string.IsNullOrWhiteSpace(outputFolder))
        {
            psi.Arguments = $"\"{starPath}\" \"{outputFolder}\"";
        }
        else
        {
            psi.Arguments = $"\"{starPath}\"";
        }

        Process proc = new() { StartInfo = psi, EnableRaisingEvents = true };

        var tcs = new TaskCompletionSource<int>();
        proc.Exited += (s, ev) =>
        {
            try
            {
                tcs.TrySetResult(proc.ExitCode);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
            finally
            {
                proc.Dispose();
            }
        };

        try
        {
            proc.Start();
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() => Cursor = new Cursor(StandardCursorType.Arrow));
            var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Failed to start process: {ex.Message}", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
            return;
        }

        await tcs.Task;

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
            var box = MessageBoxManager.GetMessageBoxStandard("Done", "Done!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
            await box.ShowWindowDialogAsync(this);
        });
    }

}