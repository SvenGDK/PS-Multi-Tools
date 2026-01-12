using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PSMultiTools.PS2.Tools;

public partial class ELFWrapper : Window
{

    public ELFWrapper()
    {
        InitializeComponent();

        ForRetailCheckBox.IsCheckedChanged += ForRetailCheckBox_IsCheckedChanged;
        ForPSXCheckBox.IsCheckedChanged += ForPSXCheckBox_IsCheckedChanged;
        ForAllCheckBox.IsCheckedChanged += ForAllCheckBox_IsCheckedChanged;
    }

    private void ForAllCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (ForAllCheckBox.IsChecked == true)
        {
            ForPSXCheckBox.IsChecked = false;
            ForRetailCheckBox.IsChecked = false;
        }
    }

    private void ForPSXCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (ForPSXCheckBox.IsChecked == true)
        {
            ForRetailCheckBox.IsChecked = false;
            ForAllCheckBox.IsChecked = false;
        }
    }

    private void ForRetailCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (ForRetailCheckBox.IsChecked == true)
        {
            ForPSXCheckBox.IsChecked = false;
            ForAllCheckBox.IsChecked = false;
        }
    }

    private async void BrowseELFButton_Click(object? sender, RoutedEventArgs e)
    {
        var elfFileFilter = new FileDialogFilter
        {
            Name = "ELF File",
            Extensions = ["ELF"]
        };
        var OFD = new OpenFileDialog() { Filters = { elfFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedELFTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseOutputFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a folder where you want to save the KELF file" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            KELFOutputFolderTextBox.Text = FBDResult;
        }
    }

    private async void WrapButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedELFTextBox.Text))
        {
            if (!string.IsNullOrEmpty(KELFOutputFolderTextBox.Text))
            {

                string RetailKHNPath = Path.Combine(Environment.CurrentDirectory, "Tools", "KRYPTO(CEX).KHN");
                string PSXKHNPath = Path.Combine(Environment.CurrentDirectory, "Tools", "KRYPTO.KHN");
                string AllKHNPath = Path.Combine(Environment.CurrentDirectory, "Tools", "KRYPTO(All).KHN");

                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        LogTextBox.Clear();
                        Cursor = new Cursor(StandardCursorType.Wait);
                    });
                }
                else
                {
                    LogTextBox.Clear();
                    Cursor = new Cursor(StandardCursorType.Wait);
                }

                // Set SCEDoormat_NoME process properties
                Process SCEDoormat_NoME = new();
                SCEDoormat_NoME.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "SCEDoormat_NoME.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "SCEDoormat_NoME");

                if (ForRetailCheckBox.IsChecked == true)
                {
                    string args = $"\"{SelectedELFTextBox.Text}\" \"{Path.Combine(KELFOutputFolderTextBox.Text, "EXECUTE.KELF")}\" \"{RetailKHNPath}\"";
                    SCEDoormat_NoME.StartInfo.Arguments = args;
                }
                else if (ForPSXCheckBox.IsChecked == true)
                {
                    string args = $"\"{SelectedELFTextBox.Text}\" \"{Path.Combine(KELFOutputFolderTextBox.Text, "EXECUTE.KELF")}\" \"{PSXKHNPath}\"";
                    SCEDoormat_NoME.StartInfo.Arguments = args;
                }
                else if (ForAllCheckBox.IsChecked == true)
                {
                    string args = $"\"{SelectedELFTextBox.Text}\" \"{Path.Combine(KELFOutputFolderTextBox.Text, "EXECUTE.KELF")}\" \"{AllKHNPath}\"";
                    SCEDoormat_NoME.StartInfo.Arguments = args;
                }

                SCEDoormat_NoME.StartInfo.RedirectStandardOutput = true;
                SCEDoormat_NoME.StartInfo.RedirectStandardError = true;
                SCEDoormat_NoME.StartInfo.UseShellExecute = false;
                SCEDoormat_NoME.StartInfo.CreateNoWindow = true;
                SCEDoormat_NoME.EnableRaisingEvents = true;

                SCEDoormat_NoME.OutputDataReceived += (SenderProcess, DataArgs) =>
                {
                    if (!string.IsNullOrEmpty(DataArgs.Data))
                    {
                        // Append output log from SCEDoormat_NoME
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                LogTextBox.Text += DataArgs.Data + "\r\n";
                                ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                                LogTextBoxScrollViewer?.ScrollToEnd();
                            });
                        }
                        else
                        {
                            LogTextBox.Text += DataArgs.Data + "\r\n";
                            ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        }
                    }
                };

                SCEDoormat_NoME.ErrorDataReceived += (SenderProcess, DataArgs) =>
                {
                    if (!string.IsNullOrEmpty(DataArgs.Data))
                    {
                        // Append error log from SCEDoormat_NoME
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                LogTextBox.Text += DataArgs.Data + "\r\n";
                                ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                                LogTextBoxScrollViewer?.ScrollToEnd();
                            });
                        }
                        else
                        {
                            LogTextBox.Text += DataArgs.Data + "\r\n";
                            ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        }
                    }
                };

                SCEDoormat_NoME.Exited += (s, e) =>
                {
                    SCEDoormat_NoME.Dispose();

                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Cursor = new Cursor(StandardCursorType.Arrow);
                    }
                    else
                    {
                        Cursor = new Cursor(StandardCursorType.Arrow);
                    }
                };

                // Start SCEDoormat_NoME & read process output data
                SCEDoormat_NoME.Start();
                SCEDoormat_NoME.BeginOutputReadLine();
                SCEDoormat_NoME.BeginErrorReadLine();
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No output folder specified.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No ELF file selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

}