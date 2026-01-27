using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PSMultiTools.PS5.Tools;

public partial class PS5Backporter : Window
{

    private const uint PT_SCE_PROCPARAM = 0x61000001;
    private const uint PT_SCE_MODULE_PARAM = 0x61000002;

    private const uint SCE_PROCESS_PARAM_MAGIC = 0x4942524F;
    private const uint SCE_MODULE_PARAM_MAGIC = 0x3C13F4BF;
    private const long SCE_PARAM_MAGIC_OFFSET = 0x8;
    private const int SCE_PARAM_MAGIC_SIZE = 0x4;
    private const long SCE_PARAM_PS4_SDK_OFFSET = 0x10;
    private const long SCE_PARAM_PS5_SDK_OFFSET = 0x14;
    private const int SCE_PARAM_PS_VERSION_SIZE = 0x4;

    private const long PHT_OFFSET_OFFSET = 0x20;
    private const int PHT_OFFSET_SIZE = 0x8;
    private const long PHT_COUNT_OFFSET = 0x38;
    private const int PHT_COUNT_SIZE = 0x2;

    private const long PHDR_ENTRY_SIZE = 0x38;
    private const long PHDR_TYPE_OFFSET = 0x0;
    private const int PHDR_TYPE_SIZE = 0x4;
    private const long PHDR_OFFSET_OFFSET = 0x8;
    private const int PHDR_OFFSET_SIZE = 0x8;

    private readonly byte[] ELF_MAGIC = [0x7F, (byte)'E', (byte)'L', (byte)'F'];
    private readonly byte[] PS4_FSELF_MAGIC = [0x4F, 0x15, 0x3D, 0x1D];
    private readonly byte[] PS5_FSELF_MAGIC = [0x54, 0x14, 0xF5, 0xEE];

    private readonly Dictionary<int, (uint ps5, uint ps4)> sdk_version_pairs = new() {
        {1,  (0x01000050, 0x07590001)},
        {2,  (0x02000009, 0x08050001)},
        {3,  (0x03000027, 0x08540001)},
        {4,  (0x04000031, 0x09040001)},
        {5,  (0x05000033, 0x09590001)},
        {6,  (0x06000038, 0x10090001)},
        {7,  (0x07000038, 0x10590001)},
        {8,  (0x08000041, 0x11090001)},
        {9,  (0x09000040, 0x11590001)},
        {10, (0x10000040, 0x12090001)},
    };

    public PS5Backporter()
    {
        InitializeComponent();
    }

    private async void BrowseFolderButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a game backup or a folder containing decrypted ELF files" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedDirectoryTextBox.Text = FBDResult;
        }
    }

    private async void BackportButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedDirectoryTextBox.Text) && !string.IsNullOrEmpty(TargetSDKVersionComboBox.Text))
        {
            // Collect all files to backport
            var FilesToBackport = Directory.EnumerateFiles(SelectedDirectoryTextBox.Text, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".prx") || s.EndsWith(".sprx") || s.EndsWith(".elf") || s.EndsWith(".self") || s.EndsWith(".bin"));
            uint ps5_sdk_version = 0;
            uint ps4_version = 0;

            // Set SDK
            try
            {
                var TargetSDKs = sdk_version_pairs.TryGetValue(int.Parse(TargetSDKVersionComboBox.Text), out (uint ps5, uint ps4) SDKs);
                ps5_sdk_version = SDKs.ps5;
                ps4_version = SDKs.ps4;
            }
            catch
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Invalid or unsupported SDK version.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                });
            }

            if (ps5_sdk_version != 0 && ps4_version != 0)
            {
                // Downgrade SDK on each file
                foreach (string FileToBackport in FilesToBackport)
                {
                    try
                    {
                        ProcessFile(FileToBackport, CreateBackupCheckBox.IsChecked == true, ps5_sdk_version, ps4_version);
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            LogTextBox.Text += $"Exception: {ex.Message}\n";
                            ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                }

                // Apply backport patches & fake sign if selected
                if (AutoPatchAndSignCheckBox.IsChecked == true)
                {
                    string patchesPath = Path.Combine(Environment.CurrentDirectory, "Tools", "PS5", "patches");
                    string destinationFakelibPath = Path.Combine(SelectedDirectoryTextBox.Text, "fakelib");

                    // Create fakelib folder
                    try
                    {
                        if (!Directory.Exists(destinationFakelibPath))
                            Directory.CreateDirectory(destinationFakelibPath);
                    }
                    catch { }

                    // Check if a patch exists for any of the decrypted libraries
                    foreach (string FileToPatch in FilesToBackport)
                    {
                        string libraryName = Path.GetFileNameWithoutExtension(FileToPatch);

                        // Check if any patch exists for selected SDK & library
                        if (TargetSDKVersionComboBox.Text == "6")
                        {
                            if (Directory.EnumerateFiles(Path.Combine(patchesPath, "6xx"), libraryName + ".*").Any())
                            {
                                // Get the patch and apply it
                                string foundPatch = Directory.EnumerateFiles(Path.Combine(patchesPath, "6xx"), libraryName + ".*").FirstOrDefault()!;
                                if (foundPatch != null)
                                {
                                    string outputPath = Path.Combine(destinationFakelibPath, Path.GetFileName(FileToPatch));

                                    // Patch prx file with bps patch
                                    try
                                    {
                                        await Dispatcher.UIThread.InvokeAsync(() =>
                                        {
                                            LogTextBox.Text += "Reading PRX library...\n";
                                        }
                                        );
                                        byte[] source = File.ReadAllBytes(FileToPatch);
                                        await Dispatcher.UIThread.InvokeAsync(() =>
                                        {
                                            LogTextBox.Text += "PRX library is valid.\n";
                                        });

                                        await Dispatcher.UIThread.InvokeAsync(() =>
                                        {
                                            LogTextBox.Text += "Reading BPS patch...\n";
                                        });
                                        byte[] patch = File.ReadAllBytes(foundPatch);
                                        await Dispatcher.UIThread.InvokeAsync(() =>
                                        {
                                            LogTextBox.Text += "BPS patch is valid.\n";
                                        });

                                        await Dispatcher.UIThread.InvokeAsync(() =>
                                        {
                                            LogTextBox.Text += "Applying patch...\n";
                                        });

                                        // Switch to rom-patcher directory
                                        Directory.SetCurrentDirectory(Path.Combine(Environment.CurrentDirectory, "Tools", "rom-patcher-js"));

                                        // Run node with rom-patcher an patch the S/PRX with the found patch
                                        var NewPSI = new ProcessStartInfo
                                        {
                                            FileName = "node",
                                            Arguments = $"index.js patch \"{FileToPatch}\" \"{foundPatch}\" -o \"{outputPath}\"",
                                            UseShellExecute = false,
                                            CreateNoWindow = true
                                        };
                                        var NewProcess = new Process { StartInfo = NewPSI };
                                        NewProcess.Start();
                                        await NewProcess.WaitForExitAsync();
                                        if (NewProcess.ExitCode != 0)
                                        {
                                            await Dispatcher.UIThread.InvokeAsync(() =>
                                            {
                                                LogTextBox.Text += $"Patching failed!";
                                            });
                                        }
                                        else
                                        {
                                            await Dispatcher.UIThread.InvokeAsync(() =>
                                            {
                                                LogTextBox.Text += "Patched PRX library successfully created!\nOutput saved to: " + outputPath;
                                            });
                                        }
                                        NewProcess.Close();

                                        // Switch back to PS Multi Tools location
                                        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                                    }
                                    catch (Exception ex)
                                    {
                                        await Dispatcher.UIThread.InvokeAsync(() => { LogTextBox.Text += "Patching Error: " + ex.Message; });
                                    }
                                }
                            }
                        }
                        else if (TargetSDKVersionComboBox.Text == "7")
                        {
                            if (Directory.EnumerateFiles(Path.Combine(patchesPath, "7xx"), libraryName + ".*").Any())
                            {
                                // Get the patch and apply it
                                string foundPatch = Directory.EnumerateFiles(Path.Combine(patchesPath, "7xx"), libraryName + ".*").FirstOrDefault()!;
                                if (foundPatch != null)
                                {
                                    string outputPath = Path.Combine(destinationFakelibPath, Path.GetFileName(FileToPatch));

                                    // Patch prx file with bps patch
                                    try
                                    {
                                        await Dispatcher.UIThread.InvokeAsync(() =>
                                        {
                                            LogTextBox.Text += "Reading PRX library...\n";
                                        }
                                        );
                                        byte[] source = File.ReadAllBytes(FileToPatch);
                                        await Dispatcher.UIThread.InvokeAsync(() =>
                                        {
                                            LogTextBox.Text += "PRX library is valid.\n";
                                        });

                                        await Dispatcher.UIThread.InvokeAsync(() =>
                                        {
                                            LogTextBox.Text += "Reading BPS patch...\n";
                                        });
                                        byte[] patch = File.ReadAllBytes(foundPatch);
                                        await Dispatcher.UIThread.InvokeAsync(() =>
                                        {
                                            LogTextBox.Text += "BPS patch is valid.\n";
                                        });

                                        await Dispatcher.UIThread.InvokeAsync(() =>
                                        {
                                            LogTextBox.Text += "Applying patch...\n";
                                        });

                                        // Switch to rom-patcher directory
                                        Directory.SetCurrentDirectory(Path.Combine(Environment.CurrentDirectory, "Tools", "rom-patcher-js"));

                                        // Run node with rom-patcher an patch the S/PRX with the found patch
                                        var NewPSI = new ProcessStartInfo
                                        {
                                            FileName = "node",
                                            Arguments = $"index.js patch \"{FileToPatch}\" \"{foundPatch}\" -o \"{outputPath}\"",
                                            UseShellExecute = false,
                                            CreateNoWindow = true
                                        };
                                        var NewProcess = new Process { StartInfo = NewPSI };
                                        NewProcess.Start();
                                        await NewProcess.WaitForExitAsync();
                                        if (NewProcess.ExitCode != 0)
                                        {
                                            await Dispatcher.UIThread.InvokeAsync(() => { LogTextBox.Text += $"Patching failed!"; });            
                                        }
                                        else
                                        {
                                            await Dispatcher.UIThread.InvokeAsync(() => { LogTextBox.Text += "Patched PRX library successfully created!\nOutput saved to: " + outputPath; });
                                        }
                                        NewProcess.Close();

                                        // Switch back to PS Multi Tools location
                                        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                                    }
                                    catch (Exception ex)
                                    {
                                        await Dispatcher.UIThread.InvokeAsync(() => { LogTextBox.Text += "Patching Error: " + ex.Message; });
                                    }
                                }
                            }
                        }
                        else { break; }
                    }

                    // Fake sign the patched fakelibs
                    var FakelibsToSign = Directory.EnumerateFiles(destinationFakelibPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".prx") || s.EndsWith(".sprx"));
                    foreach (var FileToSign in FakelibsToSign)
                    {
                        string FileToSignDirectory = Path.GetDirectoryName(FileToSign)!;
                        string FileToSignFileName = Path.GetFileName(FileToSign);
                        string FileToSignTempFileName = FileToSignFileName + ".temp";
                        string BackupFileName = FileToSignFileName + ".bak";
                        string FullTempFilePath = Path.Combine(FileToSignDirectory, FileToSignTempFileName);
                        string BackupFilePath = Path.Combine(FileToSignDirectory, BackupFileName);

                        using (var Make_fSELF = new Process())
                        {
                            Make_fSELF.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "make_fself_ps5.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "make_fself_ps5");
                            Make_fSELF.StartInfo.Arguments = $"\"{FileToSign}\" \"{FullTempFilePath}\"";
                            Make_fSELF.StartInfo.RedirectStandardOutput = true;
                            Make_fSELF.StartInfo.UseShellExecute = false;
                            Make_fSELF.StartInfo.CreateNoWindow = true;
                            Make_fSELF.Start();

                            var OutputReader = Make_fSELF.StandardOutput;
                            string ProcessOutput = OutputReader.ReadToEnd();

                            await Make_fSELF.WaitForExitAsync();
                            Make_fSELF.Close();

                            if (!string.IsNullOrEmpty(ProcessOutput))
                            {
                                if (Dispatcher.UIThread.CheckAccess() == false)
                                {
                                    Dispatcher.UIThread.Invoke(() =>
                                    {
                                        LogTextBox.Text += ProcessOutput + "\n";
                                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                                        LogTextBoxScrollViewer?.ScrollToEnd();
                                    });
                                }
                                else
                                {
                                    LogTextBox.Text += ProcessOutput + "\n";
                                    ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                                    LogTextBoxScrollViewer?.ScrollToEnd();
                                }
                            }
                        }

                        // Backup original file
                        File.Move(FileToSign, BackupFilePath);
                    }
                }

                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "Success! All files processed.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                    await box.ShowWindowAsync();
                });
            }
        }
        else
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please check your input.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            });    
        }
    }

    private static ulong ReadLeInt(FileStream fs, long offset, int size)
    {
        if (size != 1 && size != 2 && size != 4 && size != 8)
            throw new ArgumentException($"Unsupported size: {size}. Must be 1, 2, 4, or 8 bytes.");

        byte[] buffer = new byte[size];
        fs.Seek(offset, SeekOrigin.Begin);
        int read = fs.Read(buffer, 0, size);
        if (read != size) throw new EndOfStreamException("Unexpected end of file while reading integer.");

        if (!BitConverter.IsLittleEndian && size > 1)
            Array.Reverse(buffer);

        return size switch
        {
            1 => buffer[0],
            2 => BitConverter.ToUInt16(buffer, 0),
            4 => BitConverter.ToUInt32(buffer, 0),
            8 => BitConverter.ToUInt64(buffer, 0),
            _ => throw new ArgumentException("Unsupported size")
        };
    }

    private static uint ReadUInt32LE(FileStream fs, long offset)
    {
        byte[] buf = new byte[4];
        fs.Seek(offset, SeekOrigin.Begin);
        int r = fs.Read(buf, 0, 4);
        if (r != 4) throw new EndOfStreamException("Unable to read 4 bytes for uint32.");
        if (!BitConverter.IsLittleEndian) Array.Reverse(buf);
        return BitConverter.ToUInt32(buf, 0);
    }

    private static void WriteUInt32LE(FileStream fs, long offset, uint value)
    {
        byte[] buf = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian) Array.Reverse(buf);
        fs.Seek(offset, SeekOrigin.Begin);
        fs.Write(buf, 0, 4);
        fs.Flush();
    }

    private async Task<bool> PatchFileAsync(FileStream fs, uint ps5_sdk_version, uint ps4_version)
    {
        ulong segment_count = ReadLeInt(fs, PHT_COUNT_OFFSET, PHT_COUNT_SIZE);
        ulong pht_offset = ReadLeInt(fs, PHT_OFFSET_OFFSET, PHT_OFFSET_SIZE);

        for (ulong i = 0; i < segment_count; i++)
        {
            long entryBase = (long)(pht_offset + i * (ulong)PHDR_ENTRY_SIZE);
            uint segmentType = (uint)ReadLeInt(fs, entryBase + PHDR_TYPE_OFFSET, PHDR_TYPE_SIZE);
            if (segmentType != PT_SCE_PROCPARAM && segmentType != PT_SCE_MODULE_PARAM)
                continue;

            ulong segmentOffset = ReadLeInt(fs, entryBase + PHDR_OFFSET_OFFSET, PHDR_OFFSET_SIZE);

            uint paramStructSize = (uint)ReadLeInt(fs, (long)segmentOffset, 4);
            if (paramStructSize == 0 && segmentType == PT_SCE_MODULE_PARAM)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LogTextBox.Text += $"Module file has no param, skipping '{Path.GetFileName(fs.Name)}'\n";
                    ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    LogTextBoxScrollViewer?.ScrollToEnd();
                });
                return true;
            }

            if (paramStructSize < SCE_PARAM_PS5_SDK_OFFSET + SCE_PARAM_PS_VERSION_SIZE)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LogTextBox.Text += "Unexpected param struct size 0x" + paramStructSize.ToString("X") + " for file '{Path.GetFileName(fs.Name)}', aborting\n";
                    ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    LogTextBoxScrollViewer?.ScrollToEnd();
                });
                return false;
            }

            uint magic = (uint)ReadLeInt(fs, (long)segmentOffset + SCE_PARAM_MAGIC_OFFSET, SCE_PARAM_MAGIC_SIZE);
            if ((segmentType == PT_SCE_PROCPARAM && magic != SCE_PROCESS_PARAM_MAGIC) ||
                (segmentType == PT_SCE_MODULE_PARAM && magic != SCE_MODULE_PARAM_MAGIC))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LogTextBox.Text += $"Invalid param magic 0x" + magic.ToString("X8") + " for file '{Path.GetFileName(fs.Name)}', aborting\n";
                    ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    LogTextBoxScrollViewer?.ScrollToEnd();
                });
                return false;
            }

            long ps5Offset = (long)segmentOffset + SCE_PARAM_PS5_SDK_OFFSET;
            long ps4Offset = (long)segmentOffset + SCE_PARAM_PS4_SDK_OFFSET;

            uint og_ps5_sdk_version = ReadUInt32LE(fs, ps5Offset);
            uint og_ps4_sdk_version = ReadUInt32LE(fs, ps4Offset);

            WriteUInt32LE(fs, ps5Offset, ps5_sdk_version);
            WriteUInt32LE(fs, ps4Offset, ps4_version);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                LogTextBox.Text +=
                $"Patched file '{Path.GetFileName(fs.Name)}':\n" +
                "PS5 SDK version 0x" + og_ps5_sdk_version.ToString("X8") + " -> 0x" + ps5_sdk_version.ToString("X8") + "\n" +
                "PS4 version 0x" + og_ps4_sdk_version.ToString("X8") + " -> 0x" + ps4_version.ToString("X8") + "\n\n";

                ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                LogTextBoxScrollViewer?.ScrollToEnd();
            });
            return true;
        }

        return false;
    }

    private async void ProcessFile(string filePath, bool createBackup, uint ps5_sdk_version, uint ps4_version)
    {
        using FileStream fs = new(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        fs.Seek(0, SeekOrigin.Begin);
        byte[] magic = new byte[4];
        int r = fs.Read(magic, 0, 4);
        if (r != 4) throw new EndOfStreamException("Unable to read file magic.");

        if (!ByteArrayEquals(magic, ELF_MAGIC))
        {
            if (ByteArrayEquals(magic, PS4_FSELF_MAGIC) || ByteArrayEquals(magic, PS5_FSELF_MAGIC))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LogTextBox.Text += $"Aborting, file '{filePath}' is a signed file.\n";
                    ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    LogTextBoxScrollViewer?.ScrollToEnd();
                });
            }
            return;
        }

        fs.Seek(0, SeekOrigin.Begin);

        if (createBackup && !File.Exists(filePath + ".bak"))
        {
            fs.Close();
            File.Copy(filePath, filePath + ".bak");

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                LogTextBox.Text += $"Backup created for '{filePath}'\n";
                ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                LogTextBoxScrollViewer?.ScrollToEnd();
            });

            using FileStream reopened = new(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            if (!await PatchFileAsync(reopened, ps5_sdk_version, ps4_version))
            {
                return;
            }
            return;
        }

        if (!await PatchFileAsync(fs, ps5_sdk_version, ps4_version))
        {
            return;
        }
    }

    private static bool ByteArrayEquals(byte[] a, byte[] b)
    {
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;
        return true;
    }

}