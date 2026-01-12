using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Diagnostics;
using System.IO;

namespace PSMultiTools.PS5.Tools;

public partial class PS5AT9Converter : Window
{
    public PS5AT9Converter()
    {
        InitializeComponent();

        Loaded += PS5AT9Converter_Loaded;
    }

    private async void PS5AT9Converter_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!OperatingSystem.IsWindows())
        {
            //  Check if a wine prefix exists
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c")))
            {
                var WineNotInstalledMessage = MessageBoxManager.GetMessageBoxStandard("Wine installation not complete", "A wine prefix will be created, please close the Wine Configuration Tool when the initialization finished.", ButtonEnum.Ok);
                await WineNotInstalledMessage.ShowAsync();

                // Check if winetricks is updated if previously installed
                Process BashProcess = new();
                BashProcess.StartInfo.FileName = OperatingSystem.IsLinux() ? "/bin/bash" : "/bin/sh";
                BashProcess.StartInfo.Arguments = $"-c \"winecfg\"";
                BashProcess.StartInfo.RedirectStandardOutput = true;
                BashProcess.StartInfo.RedirectStandardError = true;
                BashProcess.StartInfo.UseShellExecute = false;
                BashProcess.StartInfo.CreateNoWindow = false;
                BashProcess.Start();
                BashProcess.WaitForExit();
            }

            // Check if wine prefix is 64bit
            if (!Directory.Exists(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "windows", "syswow64"))))
            {
                var Wine64NotInstalledMessage = MessageBoxManager.GetMessageBoxStandard("Wine prefix mismatch", "Current default wine prefix is 32bit only, please change to 64bit mode before continuing.", ButtonEnum.Ok);
                await Wine64NotInstalledMessage.ShowAsync();
            }
        }
    }

    private async void BrowseWavFileButton_Click(object? sender, RoutedEventArgs e)
    {
        var wavFileFilter = new FileDialogFilter
        {
            Name = "WAV File",
            Extensions = ["wav"]
        };
        var OFD = new OpenFileDialog() { Title = "Select a saved WAV file", Filters = { wavFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            InputWavTextBox.Text = OFDResult[0];

            var WAVFileInfos = new WAVFile();
            using (var reader = new BinaryReader(File.Open(OFDResult[0], FileMode.Open, FileAccess.Read, FileShare.Read)))
            {

                // Read the wave file header from the buffer.
                WAVFileInfos.RIFF = reader.ReadInt32();
                WAVFileInfos.TotalLength = reader.ReadInt32();
                WAVFileInfos.Wave = reader.ReadInt32();
                WAVFileInfos.FormatChunkMarker = reader.ReadInt32();
                WAVFileInfos.Subchunk1Size = reader.ReadInt32();
                WAVFileInfos.AudioFormat = reader.ReadInt16();
                WAVFileInfos.Channels = reader.ReadInt16();
                WAVFileInfos.SampleRate = reader.ReadInt32();
                WAVFileInfos.ByteRate = reader.ReadInt32();
                WAVFileInfos.BlockAlign = reader.ReadInt16();
                WAVFileInfos.BitsPerSample = reader.ReadInt16();

                short[] ExtraPars = [];
                if (WAVFileInfos.Subchunk1Size != 16)
                {
                    short NoOfEPs = reader.ReadInt16();
                    ExtraPars = new short[NoOfEPs];

                    for (int i = 0, loopTo = NoOfEPs - 1; i <= loopTo; i++)
                        ExtraPars[i] = reader.ReadInt16();
                }

                WAVFileInfos.Data = reader.ReadInt32();
                WAVFileInfos.DataLength = reader.ReadInt32();

                byte[] NewByteArray = new byte[WAVFileInfos.DataLength];
                NewByteArray = reader.ReadBytes(WAVFileInfos.DataLength);
            }

            WAVInfoTextBlock.Text = "WAV File Info - Format: " + WAVFileInfos.BitsPerSample.ToString() + "bit PCM / " + "Sample Rate: " + WAVFileInfos.SampleRate.ToString() + "Hz";
        }
    }

    private async void BrowseAt9FileButton_Click(object? sender, RoutedEventArgs e)
    {
        var at9FileFilter = new FileDialogFilter
        {
            Name = "AT9 File",
            Extensions = ["at9"]
        };
        var OFD = new OpenFileDialog() { Title = "Select an AT9 file", Filters = { at9FileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            InputAt9TextBox.Text = OFDResult[0];
        }
    }

    private async void ConvertToAt9Button_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(InputWavTextBox.Text))
        {
            string EncodingBitRate = string.Empty;
            switch (EncodeBitrateComboBox.Text ?? "")
            {
                case "1ch:72kbps":
                    {
                        EncodingBitRate = "-br 72";
                        break;
                    }
                case "2ch:144kbps":
                    {
                        EncodingBitRate = "-br 144";
                        break;
                    }
                case "4.0ch:240kbps":
                    {
                        EncodingBitRate = "-br 240";
                        break;
                    }
                case "5.1ch:300kbps":
                    {
                        EncodingBitRate = "-br 300";
                        break;
                    }
                case "7.1ch:420kbps":
                    {
                        EncodingBitRate = "-br 420";
                        break;
                    }
                case "Vibration 1ch: 24kbps":
                    {
                        EncodingBitRate = "-br 24";
                        break;
                    }
                case "Vibration 2ch: 48kbps":
                    {
                        EncodingBitRate = "-br 48";
                        break;
                    }
            }

            string EncodingSampleRate = string.Empty;
            switch (EncodeSamplingRateComboBox.Text ?? "")
            {
                case "12000":
                    {
                        EncodingSampleRate = "-fs 12000";
                        break;
                    }
                case "24000":
                    {
                        EncodingSampleRate = "-fs 24000";
                        break;
                    }
                case "48000":
                    {
                        EncodingSampleRate = "-fs 48000";
                        break;
                    }
            }

            var EncodingOptions = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(EncodingBitRate))
            {
                EncodingOptions.Append(EncodingBitRate + " ");
            }
            if (!string.IsNullOrEmpty(EncodingSampleRate))
            {
                EncodingOptions.Append(EncodingSampleRate + " ");
            }

            string NewFileName = Path.GetFileNameWithoutExtension(InputWavTextBox.Text) + ".at9";
            string NewConvertedFilePath = Path.Combine(Path.GetDirectoryName(InputWavTextBox.Text)!, NewFileName);

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    using var NewProc = new Process();
                    NewProc.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Tools", "PS5", "at9tool.exe");

                    if (!string.IsNullOrEmpty(EncodingOptions.ToString()))
                    {
                        NewProc.StartInfo.Arguments = $"-e \"{EncodingOptions.ToString()}\" \"{InputWavTextBox.Text}\" \"{NewConvertedFilePath}\"";
                    }
                    else
                    {
                        NewProc.StartInfo.Arguments = $"-e \"{InputWavTextBox.Text}\" \"{NewConvertedFilePath}\"";
                    }

                    NewProc.StartInfo.UseShellExecute = false;
                    NewProc.StartInfo.CreateNoWindow = true;
                    NewProc.Start();
                }
                else
                {
                    // Set destination AT9 path to wine's C:\ drive
                    NewConvertedFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", NewFileName);

                    // Check & copy tools
                    string WinePubToolsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "PS5");
                    if (!Directory.Exists(WinePubToolsPath))
                    {
                        Utils.CopyFilesRecursively(Path.Combine(Environment.CurrentDirectory, "Tools", "PS5"), WinePubToolsPath);
                    }

                    // Copy track to the wine C:\ drive
                    string DestinationCPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", InputWavTextBox.Text);
                    if (!File.Exists(DestinationCPath))
                    {
                        File.Copy(InputWavTextBox.Text, DestinationCPath, true);
                    }

                    // Start converting
                    string ConvertCMD = "";
                    if (!string.IsNullOrEmpty(EncodingOptions.ToString()))
                    {
                        ConvertCMD = $"-e {EncodingOptions.ToString()} \"{InputWavTextBox.Text}\" \"{NewConvertedFilePath}\"";
                    }
                    else
                    {
                        ConvertCMD = $"-e \"{InputWavTextBox.Text}\" \"{NewConvertedFilePath}\"";
                    }

                    string FullConvertCMD = $"wine \"c:\\PS5\\at9tool.exe\" {ConvertCMD}";
                    var EscapedArgs = FullConvertCMD.Replace("\"", "\\\"");

                    // DEBUG: output everywhere
                    Console.WriteLine("Original: " + FullConvertCMD);
                    Console.WriteLine("Escaped: " + EscapedArgs);
                    Trace.WriteLine("Original: " + FullConvertCMD);
                    Trace.WriteLine("Escaped: " + EscapedArgs);
                    Debug.WriteLine("Original: " + FullConvertCMD);
                    Debug.WriteLine("Escaped: " + EscapedArgs);

                    Process AT9Tool = new();
                    AT9Tool.StartInfo.FileName = "/bin/bash";
                    AT9Tool.StartInfo.Arguments = $"-c \"{EscapedArgs}\"";
                    AT9Tool.StartInfo.RedirectStandardOutput = true;
                    AT9Tool.StartInfo.UseShellExecute = false;
                    AT9Tool.StartInfo.CreateNoWindow = true;
                    AT9Tool.EnableRaisingEvents = true;
                    AT9Tool.Exited += async (s, e) =>
                    {
                        AT9Tool.Dispose();
                    };

                    AT9Tool.Start();

                    // Move converted file from wine's C:\ drive to the final destination
                    if (File.Exists(NewConvertedFilePath))
                    {
                        File.Move(NewConvertedFilePath, Path.Combine(Path.GetDirectoryName(InputWavTextBox.Text)!, NewFileName), true);
                    }
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Success", "WAV file converted to AT9. The file can be found in the same directory.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowDialogAsync(this);
            }
            catch (Exception)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not convert selected WAV file", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowDialogAsync(this);
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No WAV file specified.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void ConvertToWavButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(InputAt9TextBox.Text))
        {
            string DecodeFormat = string.Empty;
            switch (EncodeSamplingRateComboBox.Text ?? "")
            {
                case "16bit Integer PCM":
                    {
                        DecodeFormat = "-int16";
                        break;
                    }
                case "24bit Integer PCM":
                    {
                        DecodeFormat = "-int24";
                        break;
                    }
                case "IEEE float PCM":
                    {
                        DecodeFormat = "-float";
                        break;
                    }
            }

            string NewFileName = Path.GetFileNameWithoutExtension(InputAt9TextBox.Text) + ".wav";
            string NewConvertedFilePath = Path.Combine(Path.GetDirectoryName(InputAt9TextBox.Text)!, NewFileName);

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    Process NewProc = new();
                    NewProc.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Tools", "PS5", "at9tool.exe");

                    if (!string.IsNullOrEmpty(DecodeFormat))
                    {
                        NewProc.StartInfo.Arguments = $"-d {DecodeFormat} \"{InputAt9TextBox.Text}\" \"{NewConvertedFilePath}\"";
                    }
                    else
                    {
                        NewProc.StartInfo.Arguments = $"-d \"{InputAt9TextBox.Text}\" \"{NewConvertedFilePath}\"";
                    }

                    NewProc.StartInfo.UseShellExecute = false;
                    NewProc.StartInfo.CreateNoWindow = true;
                    NewProc.Start();
                }
                else if (OperatingSystem.IsLinux())
                {
                    // Set destination WAV path to wine's C:\ drive
                    NewConvertedFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", NewFileName);

                    // Check & copy tools
                    string WinePubToolsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "PS5");
                    if (!Directory.Exists(WinePubToolsPath))
                    {
                        Utils.CopyFilesRecursively(Path.Combine(Environment.CurrentDirectory, "Tools", "PS5"), WinePubToolsPath);
                    }

                    // Copy track to the wine C:\ drive
                    string DestinationCPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", InputAt9TextBox.Text);
                    if (!File.Exists(DestinationCPath))
                    {
                        File.Copy(InputAt9TextBox.Text, DestinationCPath, true);
                    }

                    // Start converting
                    string ConvertCMD = "";
                    if (!string.IsNullOrEmpty(DecodeFormat))
                    {
                        ConvertCMD = $"-d {DecodeFormat} \"{DestinationCPath}\" \"{NewConvertedFilePath}\"";
                    }
                    else
                    {
                        ConvertCMD = $"-d \"{DestinationCPath}\" \"{NewConvertedFilePath}\"";
                    }
                    string FullConvertCMD = $"wine \"c:\\PS5\\at9tool.exe\" {ConvertCMD}";
                    var EscapedArgs = FullConvertCMD.Replace("\"", "\\\"");

                    // DEBUG: output everywhere
                    Console.WriteLine("Original: " + FullConvertCMD);
                    Console.WriteLine("Escaped: " + EscapedArgs);
                    Trace.WriteLine("Original: " + FullConvertCMD);
                    Trace.WriteLine("Escaped: " + EscapedArgs);
                    Debug.WriteLine("Original: " + FullConvertCMD);
                    Debug.WriteLine("Escaped: " + EscapedArgs);

                    Process AT9Tool = new();
                    AT9Tool.StartInfo.FileName = "/bin/bash";
                    AT9Tool.StartInfo.Arguments = $"-c \"{EscapedArgs}\"";
                    AT9Tool.StartInfo.RedirectStandardOutput = true;
                    AT9Tool.StartInfo.UseShellExecute = false;
                    AT9Tool.StartInfo.CreateNoWindow = true;
                    AT9Tool.EnableRaisingEvents = true;
                    AT9Tool.Exited += async (s, e) =>
                    {
                        AT9Tool.Dispose();
                    };

                    AT9Tool.Start();

                    // Move converted file from wine's C:\ drive to the final destination
                    if (File.Exists(NewConvertedFilePath))
                    {
                        File.Move(NewConvertedFilePath, Path.Combine(Path.GetDirectoryName(InputAt9TextBox.Text)!, NewFileName), true);
                    }
                }
                else if (OperatingSystem.IsMacOS())
                {

                }

                var box = MessageBoxManager.GetMessageBoxStandard("Success", "AT9 file converted to WAV. The file can be found in the same directory.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowDialogAsync(this);
            }
            catch (Exception)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not convert selected AT9 file", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowDialogAsync(this);
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No AT9 file specified.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

}