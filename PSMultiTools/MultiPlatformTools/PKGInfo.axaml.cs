using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using PS4_Tools;
using PSMultiTools.Classes;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static PSMultiTools.PS5.Tools.PS5PKGViewer;

namespace PSMultiTools.MultiPlatformTools;

public partial class PKGInfo : Window
{
    public string Console = "";
    public string SelectedPKG = "";

    private readonly BackgroundWorker PKGWorker = new();

    private byte[] PKGSoundBytes = [];

    private AnyBitmap? CurrentIcon0 = null;
    private AnyBitmap? CurrentPic0 = null;

    public PKGInfo()
    {
        InitializeComponent();

        Loaded += PKGInfo_Loaded;
        PKGWorker.DoWork += PKGWorker_DoWork;
    }

    private void PKGInfo_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedPKG))
        {
            //Try to detect the console/platform of the selected PKG if Console is not known
            if (string.IsNullOrEmpty(Console))
            {
                Console = TryPKGAsync(SelectedPKG);

                var box = MessageBoxManager.GetMessageBoxStandard("Info", Console, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                box.ShowWindowDialogAsync(this);
            }

            PKGWorker.RunWorkerAsync();
        }
    }

    private void PlayStopButton_Click(object? sender, RoutedEventArgs e)
    {
        if (PlayStopButton.Content!.ToString() == "Play Soundtrack" && PKGSoundBytes is not null)
        {
            PlayStopButton.Content = "Stop Soundtrack";
            Utils.StartAndStreamSoundBytes(PKGSoundBytes);
        }
        else
        {
            PlayStopButton.Content = "Play Soundtrack";
            Utils.StopGameSoundBytes();
        }
    }

    private void PKGWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
        switch (Console)
        {
            case "PS3":
                {
                    LoadPS3Info();
                    break;
                }
            case "PSP":
                {
                    LoadPSPInfo();
                    break;
                }
            case "PS4":
                {
                    LoadPS4Info();
                    break;
                }
            case "PS5":
                {
                    LoadPS5Info();
                    break;
                }
            case "PSV":
                {
                    LoadPSVInfo();
                    break;
                }
        }
    }

    private void LoadPS3Info()
    {
        try
        {
            var PKGFileInfo = new FileInfo(SelectedPKG);
            var NewPKGDecryptor = new PKGDecryptor();

            NewPKGDecryptor.ProcessPKGFile(SelectedPKG);

            Dispatcher.UIThread.Invoke(() => PKGConsoleTextBlock.Text = "PS3");
            Dispatcher.UIThread.Invoke(() => PKGSizeTextBlock.Text = Utils.HumanReadableBytes(PKGFileInfo.Length));

            if (NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.ICON0) is not null)
            {
                Dispatcher.UIThread.Invoke(() => GameBackground.Source = Utils.AnyBitmapToIImage(NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.ICON0)!));
                Dispatcher.UIThread.Invoke(() => SaveIconButton.IsEnabled = true);

                CurrentIcon0 = NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.ICON0);
            }
            if (NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC1) is not null)
            {
                Dispatcher.UIThread.Invoke(() => Background = Utils.AnyBitmapToImageBrush(NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC1)!));
                Dispatcher.UIThread.Invoke(() => SaveBackgroundButton.IsEnabled = true);

                CurrentPic0 = NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC1);
            }
            if (NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC2) is not null)
            {
                Dispatcher.UIThread.Invoke(() => GameIcon.Source = Utils.AnyBitmapToIImage(NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC2)!));
            }
            if (NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.SND0) is not null)
            {
                Dispatcher.UIThread.Invoke(() => PKGSoundBytes = NewPKGDecryptor.GetSND);
                Dispatcher.UIThread.Invoke(() => PlayStopButton.IsEnabled = true);
            }

            if (NewPKGDecryptor.GetPARAMSFO is not null)
            {
                var SFOKeys = SFONew.ReadSfo(NewPKGDecryptor.GetPARAMSFO);

                if (SFOKeys.TryGetValue("TITLE", out var TITLEValue))
                {
                    Dispatcher.UIThread.Invoke(() => Utils.CleanTitle(TITLEValue.ToString()!));
                }
                if (SFOKeys.TryGetValue("TITLE_ID", out var TITLEIDValue))
                {
                    Dispatcher.UIThread.Invoke(() => PKGTitleIDTextBlock.Text = TITLEIDValue.ToString());
                    Dispatcher.UIThread.Invoke(() => PKGRegionTextBlock.Text = GetPS3GameRegion(TITLEIDValue.ToString()!));
                }
                if (SFOKeys.TryGetValue("CATEGORY", out var CATEGORYValue))
                {
                    Dispatcher.UIThread.Invoke(() => PKGCategoryTextBlock.Text = GetPS3Category(CATEGORYValue.ToString()!));
                }
                if (SFOKeys.TryGetValue("CONTENT_ID", out var CONTENTIDValue))
                {
                    Dispatcher.UIThread.Invoke(() => PKGContentIDTextBlock.Text = CONTENTIDValue.ToString());
                }
                if (SFOKeys.TryGetValue("APP_TYPE", out var APPTYPEValue))
                {
                    Dispatcher.UIThread.Invoke(() => PKGTypeTextBlock.Text = APPTYPEValue.ToString());
                }
                if (SFOKeys.TryGetValue("APP_VER", out var APPVERValue))
                {
                    Dispatcher.UIThread.Invoke(() => PKGAppVerTextBlock.Text = APPVERValue.ToString());
                }
                if (SFOKeys.TryGetValue("PS3_SYSTEM_VER", out var PS3SYSTEMVERValue))
                {
                    Dispatcher.UIThread.Invoke(() => PKGFirmwareVersionTextBlock.Text = PS3SYSTEMVERValue.ToString());
                }
                if (SFOKeys.TryGetValue("VERSION", out var VERSIONValue))
                {
                    Dispatcher.UIThread.Invoke(() => PKGVersionTextBlock.Text = VERSIONValue.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Info", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            box.ShowWindowAsync();
        }
    }

    private void LoadPS4Info()
    {
        try
        {
            var GamePKG = PKG.SceneRelated.Read_PKG(SelectedPKG);

            if (!string.IsNullOrEmpty(GamePKG.BuildDate))
            {
                Dispatcher.UIThread.Invoke(() => PKGBuildDateTextBlock.Text = GamePKG.BuildDate);
            }

            if (!string.IsNullOrEmpty(GamePKG.Firmware_Version))
            {
                Dispatcher.UIThread.Invoke(() => PKGFirmwareVersionTextBlock.Text = GamePKG.Firmware_Version);
            }

            if (!string.IsNullOrEmpty(GamePKG.Size))
            {
                Dispatcher.UIThread.Invoke(() => PKGSizeTextBlock.Text = GamePKG.Size);
            }

            if (!string.IsNullOrEmpty(GamePKG.Content_ID))
            {
                Dispatcher.UIThread.Invoke(() => PKGContentIDTextBlock.Text = GamePKG.Content_ID);
            }
            else if (!string.IsNullOrEmpty(GamePKG.Param.ContentID))
            {
                Dispatcher.UIThread.Invoke(() => PKGContentIDTextBlock.Text = GamePKG.Param.ContentID);
            }

            if (!string.IsNullOrEmpty(GamePKG.Region))
            {
                Dispatcher.UIThread.Invoke(() => PKGRegionTextBlock.Text = GamePKG.Region);
            }

            if (GamePKG.Sound is not null && GamePKG.Sound.Length > 0)
            {
                Dispatcher.UIThread.Invoke(() => PKGSoundBytes = Media.Atrac9.LoadAt9(GamePKG.Sound));
                Dispatcher.UIThread.Invoke(() => PlayStopButton.IsEnabled = true);
            }
            if (GamePKG.Background is not null && GamePKG.Background.Length > 0)
            {
                Dispatcher.UIThread.Invoke(async () =>
                {
                    ImageBrush newBackgroundSource = Utils.ByteArrayToImageBrush(GamePKG.Background);
                    SaveBackgroundButton.IsEnabled = true;
                });

                CurrentPic0 = AnyBitmap.FromBytes(GamePKG.Background);

            }
            if (GamePKG.Icon is not null && GamePKG.Icon.Length > 0)
            {
                Dispatcher.UIThread.Invoke(async () =>
                {
                    GameIcon.Source = Utils.AnyBitmapToIImage(GamePKG.Icon);
                    SaveIconButton.IsEnabled = true;
                });

                CurrentIcon0 = AnyBitmap.FromBytes(GamePKG.Icon);
            }
            if (GamePKG.Image is not null && GamePKG.Image.Length > 0)
            {
                Dispatcher.UIThread.Invoke(() => GameBackground.Source = Utils.AnyBitmapToIImage(GamePKG.Image));
            }

            switch (GamePKG.PKGState)
            {
                case PKG.SceneRelated.PKG_State.Official:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGStateTextBlock.Text = "Official");
                        break;
                    }
                case PKG.SceneRelated.PKG_State.Officail_DP:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGStateTextBlock.Text = "Official DP");
                        break;
                    }
                case PKG.SceneRelated.PKG_State.Fake:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGStateTextBlock.Text = "Fake");
                        break;
                    }
                case PKG.SceneRelated.PKG_State.Unkown:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGStateTextBlock.Text = "Unknown");
                        break;
                    }

                default:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGStateTextBlock.Text = "Unknown");
                        break;
                    }
            }

            switch (GamePKG.PKG_Type)
            {
                case PKG.SceneRelated.PKGType.Addon_Theme:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGTypeTextBlock.Text = "Theme");
                        break;
                    }
                case PKG.SceneRelated.PKGType.App:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGTypeTextBlock.Text = "Application");
                        break;
                    }
                case PKG.SceneRelated.PKGType.Game:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGTypeTextBlock.Text = "Game");
                        break;
                    }
                case PKG.SceneRelated.PKGType.Patch:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGTypeTextBlock.Text = "Patch");
                        break;
                    }
                case PKG.SceneRelated.PKGType.Unknown:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGTypeTextBlock.Text = "Unknown");
                        break;
                    }

                default:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGTypeTextBlock.Text = "Unknown");
                        break;
                    }
            }

            switch (GamePKG.Param.PlaystationVersion)
            {
                case Param_SFO.PARAM_SFO.Playstation.ps4:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGConsoleTextBlock.Text = "PS4");
                        break;
                    }
                case Param_SFO.PARAM_SFO.Playstation.psp:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGConsoleTextBlock.Text = "PSP");
                        break;
                    }
                case Param_SFO.PARAM_SFO.Playstation.unknown:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGConsoleTextBlock.Text = "Unknown");
                        break;
                    }
            }

            Dispatcher.UIThread.Invoke(() => PKGAttributesTextBlock.Text = GamePKG.Param.Attribute);
            Dispatcher.UIThread.Invoke(() => PKGAppVerTextBlock.Text = GamePKG.Param.APP_VER);
            Dispatcher.UIThread.Invoke(() => PKGCategoryTextBlock.Text = GetPS4Category(GamePKG.Param.Category));
            Dispatcher.UIThread.Invoke(() => PKGTitleTextBlock.Text = GamePKG.Param.Title);

            switch (GamePKG.Param.DataType)
            {
                case Param_SFO.PARAM_SFO.DataTypes.DiscGame:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Disc Game");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.Additional_Content:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Additional Content");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.AppleTV:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "AppleTV");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.AppMusic:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Music App");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.AppPhoto:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Photo App");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.AppVideo:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Video App");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.AutoInstallRoot:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "AutoInstall Root");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.Blu_Ray_Disc:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Blu Ray Disc");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.BroadCastVideo:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Broadcast Video");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.CellBE:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "CellBE");
                        break;
                    }
                case var @case when @case == Param_SFO.PARAM_SFO.DataTypes.DiscGame:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Disc Game");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.DiscMovie:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Disc Movie");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.DiscPackage:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Disc Package");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.ExtraRoot:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Extra Root");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.GameContent:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Game Content");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.GameData:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Game Data");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.Game_Digital_Application:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Game Digital Application");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.GDE:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "GDE");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.HDDGame:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "HDD Game");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.Home:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Home");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.None:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "None");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.PS4_Game_Application_Patch:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Game Application Patch");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.PSN_Game:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "PSN Game");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.SaveData:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Save Data");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.StoreFronted:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Store Fronted");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.ThemeRoot:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Theme Root");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.VideoRoot:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Video Root");
                        break;
                    }
                case Param_SFO.PARAM_SFO.DataTypes.WebTV:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "WebTV");
                        break;
                    }

                default:
                    {
                        Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Unknown");
                        break;
                    }
            }

            foreach (Param_SFO.PARAM_SFO.Table TableEntry in GamePKG.Param.Tables.ToList())
            {
                if (TableEntry.Name == "TITLE_ID")
                {
                    Dispatcher.UIThread.Invoke(() => PKGTitleIDTextBlock.Text = TableEntry.Value.Trim());
                }
                if (TableEntry.Name == "VERSION")
                {
                    Dispatcher.UIThread.Invoke(() => PKGVersionTextBlock.Text = TableEntry.Value.Trim());
                }
            }
        }
        catch (Exception)
        {

        }
    }

    private void LoadPS5Info()
    {
        try
        {
            // Determine PS5 PKG
            string FirstString = "";
            sbyte Int8AtOffset5;
            bool IsSourcePKG = false;
            bool IsRetailPKG = false;

            string CurrentParamJSON = "";
            XDocument? CurrentConfigurationXML = null;

            Dispatcher.UIThread.Invoke(() => PKGConsoleTextBlock.Text = "PS5");

            using (var PKGReader = new FileStream(SelectedPKG, FileMode.Open, FileAccess.Read))
            {
                var BinReader = new BinaryReader(PKGReader);
                FirstString = BinReader.ReadString();
                PKGReader.Seek(5, SeekOrigin.Begin);
                Int8AtOffset5 = BinReader.ReadSByte();
                PKGReader.Close();
                BinReader.Close();
            }

            if (!string.IsNullOrEmpty(FirstString))
            {
                if (FirstString.Contains("CNT"))
                {
                    IsSourcePKG = true;
                    PKGTypeTextBlock.Text = "Source PKG";
                }
                else
                {
                    IsSourcePKG = false;
                }
            }

            if (Int8AtOffset5 == -128)
            {
                IsRetailPKG = true;
                PKGTypeTextBlock.Text = "Retail PKG";
            }

            if (IsRetailPKG | IsSourcePKG)
            {
                // Get only param.json
                byte[] startBytes = Encoding.UTF8.GetBytes("param.json");
                byte[] endBytes = Encoding.UTF8.GetBytes("version.xml");

                long startOffset = -1;
                long endOffset = -1;

                using (var PKGReader = new FileStream(SelectedPKG, FileMode.Open, FileAccess.Read))
                {
                    var buffer = new byte[4097];
                    long fileLength = PKGReader.Length;
                    long totalBytesRead = fileLength;

                    while (totalBytesRead > 0)
                    {
                        int bytesRead = (int)Math.Min(buffer.Length, totalBytesRead);
                        PKGReader.Seek(totalBytesRead - bytesRead, SeekOrigin.Begin);
                        PKGReader.ReadExactly(buffer, 0, bytesRead);
                        totalBytesRead -= bytesRead;

                        bool exitWhile = false;
                        for (int i = bytesRead - 1; i >= 0; i -= 1)
                        {
                            if (endOffset == -1 && MatchBytes(buffer, i, endBytes))
                            {
                                endOffset = totalBytesRead + i + endBytes.Length;
                            }

                            if (startOffset == -1 && MatchBytes(buffer, i, startBytes))
                            {
                                startOffset = totalBytesRead + i;
                            }

                            if (startOffset != -1 && endOffset != -1)
                            {
                                exitWhile = true;
                                break;
                            }
                        }

                        if (exitWhile)
                        {
                            break;
                        }
                    }
                }

                if (startOffset != -1 && endOffset != -1 && endOffset > startOffset)
                {
                    string FinalParamJSONString = "";
                    using (var ParamJSONFileStream = new FileStream(SelectedPKG, FileMode.Open, FileAccess.Read))
                    {
                        long ParamDataSize = endOffset - startOffset;
                        ParamJSONFileStream.Seek(startOffset, SeekOrigin.Begin);

                        var NewParamData = new byte[((int)ParamDataSize)];
                        ParamJSONFileStream.ReadExactly(NewParamData, 0, (int)ParamDataSize);

                        string ExtractedData = Encoding.UTF8.GetString(NewParamData);
                        var ParamJSONData = ExtractedData.Split(["\r\n"], StringSplitOptions.None).ToList();

                        // Adjust the output
                        ParamJSONData.RemoveAt(0);
                        ParamJSONData.Insert(0, "{");
                        ParamJSONData[^1] += "\"";
                        ParamJSONData.Add("}");

                        FinalParamJSONString = string.Join(Environment.NewLine, ParamJSONData);
                    }

                    if (!string.IsNullOrEmpty(FinalParamJSONString))
                    {
                        CurrentParamJSON = FinalParamJSONString;

                        // Display pkg information
                        var ParamData = JsonConvert.DeserializeObject<PS5ParamClass.PS5Param>(FinalParamJSONString);

                        if (ParamData is not null)
                        {

                            if (ParamData.TitleId is not null)
                            {
                                PKGTitleIDTextBlock.Text = "Title ID: " + ParamData.TitleId;
                                PKGRegionTextBlock.Text = "Region: " + PS5Game.GetGameRegion(ParamData.TitleId);
                            }

                            if (ParamData.LocalizedParameters!.EnUS is not null)
                            {
                                PKGTitleTextBlock.Text = ParamData.LocalizedParameters.EnUS.TitleName;
                            }

                            if (ParamData.ContentId is not null)
                            {
                                PKGContentIDTextBlock.Text = "Content ID: " + ParamData.ContentId;
                            }

                            if (ParamData.ApplicationCategoryType == 0)
                            {
                                PKGCategoryTextBlock.Text = "Type: PS5 Game";
                            }
                            else if (ParamData.ApplicationCategoryType == 65792)
                            {
                                PKGCategoryTextBlock.Text = "Type: RNPS Media App";
                            }
                            else if (ParamData.ApplicationCategoryType == 131328)
                            {
                                PKGCategoryTextBlock.Text = "Type: System Built-in App";
                            }
                            else if (ParamData.ApplicationCategoryType == 131584)
                            {
                                PKGCategoryTextBlock.Text = "Type: Big Daemon";
                            }
                            else if (ParamData.ApplicationCategoryType == 16777216)
                            {
                                PKGCategoryTextBlock.Text = "Type: ShellUI";
                            }
                            else if (ParamData.ApplicationCategoryType == 33554432)
                            {
                                PKGCategoryTextBlock.Text = "Type: Daemon";
                            }
                            else if (ParamData.ApplicationCategoryType == 67108864)
                            {
                                PKGCategoryTextBlock.Text = "Type: ShellApp";
                            }

                            long PS5GameSize = new FileInfo(SelectedPKG).Length;
                            PKGSizeTextBlock.Text = $"Size: {Utils.HumanReadableBytes(PS5GameSize)}";

                            if (ParamData.ContentVersion is not null)
                            {
                                PKGVersionTextBlock.Text = "Version: " + ParamData.ContentVersion;
                            }
                            if (ParamData.RequiredSystemSoftwareVersion is not null)
                            {
                                PKGFirmwareVersionTextBlock.Text = "Required Firmware: " + ParamData.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, ".").Insert(5, ".").Insert(8, ".").Remove(11, 8);
                            }

                        }
                    }

                }
            }
            else
            {
                string ExtractedPKGConfigurationData = "";
                string PKGConfigurationStartString = "<package-configuration version=\"1.0\" type=\"package-info\">";
                string PKGConfigurationEndString = "</package-configuration>";

                byte[] PKGConfigurationStartBytes = Encoding.UTF8.GetBytes(PKGConfigurationStartString);
                byte[] PKGConfigurationEndBytes = Encoding.UTF8.GetBytes(PKGConfigurationEndString);

                long PKGConfigurationStartOffset = -1;
                long PKGConfigurationEndOffset = -1;

                // Get the PKG configuration
                using (var PKGReader = new FileStream(SelectedPKG, FileMode.Open, FileAccess.Read))
                {
                    var buffer = new byte[4097];
                    long fileLength = PKGReader.Length;
                    long totalBytesRead = fileLength;

                    // Read backwards to find the end string first
                    while (totalBytesRead > 0)
                    {
                        int bytesRead = (int)Math.Min(buffer.Length, totalBytesRead);
                        PKGReader.Seek(totalBytesRead - bytesRead, SeekOrigin.Begin);
                        PKGReader.ReadExactly(buffer, 0, bytesRead);
                        totalBytesRead -= bytesRead;

                        // Check buffer from end to start
                        bool exitWhile1 = false;
                        for (int i = bytesRead - 1; i >= 0; i -= 1)
                        {
                            // Check for end string
                            if (PKGConfigurationEndOffset == -1 && MatchBytes(buffer, i, PKGConfigurationEndBytes))
                            {
                                PKGConfigurationEndOffset = totalBytesRead + i + PKGConfigurationEndBytes.Length;
                            }

                            // Check for start string
                            if (PKGConfigurationStartOffset == -1 && MatchBytes(buffer, i, PKGConfigurationStartBytes))
                            {
                                PKGConfigurationStartOffset = totalBytesRead + i;
                            }

                            if (PKGConfigurationStartOffset != -1 && PKGConfigurationEndOffset != -1)
                            {
                                exitWhile1 = true;
                                break;
                            }
                        }

                        if (exitWhile1)
                        {
                            break;
                        }
                    }

                    if (PKGConfigurationStartOffset != -1 & PKGConfigurationEndOffset != -1 && PKGConfigurationEndOffset > PKGConfigurationStartOffset)
                    {
                        PKGConfigurationStartOffset -= PKGConfigurationStartBytes.Length;
                        PKGConfigurationEndOffset -= PKGConfigurationEndBytes.Length - 1;

                        // Calculate the size of the pkg configuration data
                        long PKGConfigurationDataSize = PKGConfigurationEndOffset - PKGConfigurationStartOffset;

                        // Move to the start offset
                        PKGReader.Seek(PKGConfigurationStartOffset, SeekOrigin.Begin);

                        // Read the pkg configuration data
                        var data = new byte[((int)PKGConfigurationDataSize)];
                        PKGReader.ReadExactly(data, 0, (int)PKGConfigurationDataSize);

                        // Convert the data to a XML string
                        ExtractedPKGConfigurationData = Encoding.UTF8.GetString(data);
                        ExtractedPKGConfigurationData = string.Concat("<?xml version=\"1.0\" encoding=\"utf-8\"?>", ExtractedPKGConfigurationData);
                    }
                }

                // Process the retrieved PKG configuration data
                if (!string.IsNullOrEmpty(ExtractedPKGConfigurationData))
                {
                    // Load the XML file
                    XDocument PKGConfigurationXML = XDocument.Parse(ExtractedPKGConfigurationData);
                    CurrentConfigurationXML = PKGConfigurationXML;

                    if (PKGConfigurationXML != null)
                    {
                        // Get the PKG Mount Image Container Offset
                        string PKGMountImageContainerOffset = PKGConfigurationXML.Descendants("mount-image").First().Element("container-offset")!.Value;

                        // Extract param.json & icon0.png
                        using var PKGReader = new FileStream(SelectedPKG, FileMode.Open, FileAccess.Read);
                        // Seek from the end
                        PKGReader.Seek(0, SeekOrigin.End);

                        long ContainerOffsetDecValue = 0;
                        long EntryOffsetDecValue = 0;
                        int EntrySizeDecValue = 0;

                        long ParamJSONOffsetPosition = 0;
                        long Icon0OffsetPosition = 0;
                        long Pic0OffsetPosition = 0;

                        var ParamJsonPKGEntry = new PS5PKGEntry();
                        var Icon0PKGEntry = new PS5PKGEntry();
                        var Pic0PKGEntry = new PS5PKGEntry();

                        // Get the param.json and icon0.png PKG entry info
                        foreach (XElement PKGEntry in PKGConfigurationXML.Descendants("entries").Descendants("entry"))
                        {
                            if (PKGEntry is not null)
                            {
                                if (PKGEntry.Attribute("name")!.Value == "param.json")
                                {
                                    ParamJsonPKGEntry.EntryOffset = PKGEntry.Attribute("offset")!.Value;
                                    ParamJsonPKGEntry.EntrySize = PKGEntry.Attribute("size")!.Value;
                                    ParamJsonPKGEntry.EntryName = PKGEntry.Attribute("name")!.Value;
                                }
                                if (PKGEntry.Attribute("name")!.Value == "icon0.png")
                                {
                                    Icon0PKGEntry.EntryOffset = PKGEntry.Attribute("offset")!.Value;
                                    Icon0PKGEntry.EntrySize = PKGEntry.Attribute("size")!.Value;
                                    Icon0PKGEntry.EntryName = PKGEntry.Attribute("name")!.Value;
                                }
                                if (PKGEntry.Attribute("name")!.Value == "pic0.png")
                                {
                                    Pic0PKGEntry.EntryOffset = PKGEntry.Attribute("offset")!.Value;
                                    Pic0PKGEntry.EntrySize = PKGEntry.Attribute("size")!.Value;
                                    Pic0PKGEntry.EntryName = PKGEntry.Attribute("name")!.Value;
                                }
                            }
                        }

                        // PARAM.JSON
                        if (!string.IsNullOrEmpty(ParamJsonPKGEntry.EntryOffset) && !string.IsNullOrEmpty(ParamJsonPKGEntry.EntrySize))
                        {

                            // Get decimal offset values
                            if (!string.IsNullOrEmpty(PKGMountImageContainerOffset))
                            {
                                ContainerOffsetDecValue = Convert.ToInt64(PKGMountImageContainerOffset, 16);
                            }
                            if (!string.IsNullOrEmpty(ParamJsonPKGEntry.EntryOffset))
                            {
                                EntryOffsetDecValue = Convert.ToInt64(ParamJsonPKGEntry.EntryOffset, 16);
                            }
                            if (!string.IsNullOrEmpty(ParamJsonPKGEntry.EntrySize))
                            {
                                EntrySizeDecValue = Convert.ToInt32(ParamJsonPKGEntry.EntrySize, 16);
                            }
                            ParamJSONOffsetPosition = ContainerOffsetDecValue + EntryOffsetDecValue;

                            // Seek to the beginning of the param.json file and read
                            byte[] ParamFileBuffer = new byte[EntrySizeDecValue];
                            PKGReader.Seek(ParamJSONOffsetPosition, SeekOrigin.Begin);
                            PKGReader.ReadExactly(ParamFileBuffer);

                            if (!string.IsNullOrWhiteSpace(Encoding.UTF8.GetString(ParamFileBuffer)))
                            {
                                CurrentParamJSON = Encoding.UTF8.GetString(ParamFileBuffer);
                                var ParamData = JsonConvert.DeserializeObject<PS5ParamClass.PS5Param>(Encoding.UTF8.GetString(ParamFileBuffer));

                                if (ParamData is not null)
                                {

                                    PKGTypeTextBlock.Text = "Debug PKG";

                                    if (ParamData.TitleId is not null)
                                    {
                                        PKGTitleIDTextBlock.Text = "Title ID: " + ParamData.TitleId;
                                        PKGRegionTextBlock.Text = "Region: " + PS5Game.GetGameRegion(ParamData.TitleId);
                                    }

                                    if (ParamData.LocalizedParameters!.EnUS is not null)
                                    {
                                        PKGTitleTextBlock.Text = ParamData.LocalizedParameters.EnUS.TitleName;
                                    }

                                    if (ParamData.ContentId is not null)
                                    {
                                        PKGContentIDTextBlock.Text = "Content ID: " + ParamData.ContentId;
                                    }

                                    if (ParamData.ApplicationCategoryType == 0)
                                    {
                                        PKGCategoryTextBlock.Text = "Type: PS5 Game";
                                    }
                                    else if (ParamData.ApplicationCategoryType == 65792)
                                    {
                                        PKGCategoryTextBlock.Text = "Type: RNPS Media App";
                                    }
                                    else if (ParamData.ApplicationCategoryType == 131328)
                                    {
                                        PKGCategoryTextBlock.Text = "Type: System Built-in App";
                                    }
                                    else if (ParamData.ApplicationCategoryType == 131584)
                                    {
                                        PKGCategoryTextBlock.Text = "Type: Big Daemon";
                                    }
                                    else if (ParamData.ApplicationCategoryType == 16777216)
                                    {
                                        PKGCategoryTextBlock.Text = "Type: ShellUI";
                                    }
                                    else if (ParamData.ApplicationCategoryType == 33554432)
                                    {
                                        PKGCategoryTextBlock.Text = "Type: Daemon";
                                    }
                                    else if (ParamData.ApplicationCategoryType == 67108864)
                                    {
                                        PKGCategoryTextBlock.Text = "Type: ShellApp";
                                    }

                                    PKGSizeTextBlock.Text = $"Size: {Utils.HumanReadableBytes(PKGReader.Length)}";

                                    if (ParamData.ContentVersion is not null)
                                    {
                                        PKGVersionTextBlock.Text = "Version: " + ParamData.ContentVersion;
                                    }
                                    if (ParamData.RequiredSystemSoftwareVersion is not null)
                                    {
                                        PKGFirmwareVersionTextBlock.Text = "Required Firmware: " + ParamData.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, ".").Insert(5, ".").Insert(8, ".").Remove(11, 8);
                                    }

                                }
                            }

                        }

                        // ICON0.PNG
                        if (!string.IsNullOrEmpty(Icon0PKGEntry.EntryOffset) && !string.IsNullOrEmpty(Icon0PKGEntry.EntrySize))
                        {

                            // Get decimal offset values
                            if (!string.IsNullOrEmpty(PKGMountImageContainerOffset))
                            {
                                ContainerOffsetDecValue = Convert.ToInt64(PKGMountImageContainerOffset, 16);
                            }
                            if (!string.IsNullOrEmpty(Icon0PKGEntry.EntryOffset))
                            {
                                EntryOffsetDecValue = Convert.ToInt64(Icon0PKGEntry.EntryOffset, 16);
                            }
                            if (!string.IsNullOrEmpty(Icon0PKGEntry.EntrySize))
                            {
                                EntrySizeDecValue = Convert.ToInt32(Icon0PKGEntry.EntrySize, 16);
                            }
                            Icon0OffsetPosition = ContainerOffsetDecValue + EntryOffsetDecValue;

                            // Seek to the beginning of the icon0.png file and read
                            byte[] Icon0FileBuffer = new byte[EntrySizeDecValue];
                            PKGReader.Seek(Icon0OffsetPosition, SeekOrigin.Begin);
                            PKGReader.ReadExactly(Icon0FileBuffer);

                            // Check the buffer and display the icon
                            if (Icon0FileBuffer is not null)
                            {
                                Bitmap Icon0BitmapImage;
                                using (var Icon0MemoryStream = new MemoryStream(Icon0FileBuffer))
                                {
                                    Icon0BitmapImage = new Bitmap(Icon0MemoryStream);
                                    CurrentIcon0 = AnyBitmap.FromStream(Icon0MemoryStream);
                                }
                                GameIcon.Source = Icon0BitmapImage;
                                SaveIconButton.IsEnabled = true;
                            }
                        }

                        // PIC0.PNG
                        if (!string.IsNullOrEmpty(Pic0PKGEntry.EntryOffset) && !string.IsNullOrEmpty(Pic0PKGEntry.EntrySize))
                        {

                            // Get decimal offset values
                            if (!string.IsNullOrEmpty(PKGMountImageContainerOffset))
                            {
                                ContainerOffsetDecValue = Convert.ToInt64(PKGMountImageContainerOffset, 16);
                            }
                            if (!string.IsNullOrEmpty(Pic0PKGEntry.EntryOffset))
                            {
                                EntryOffsetDecValue = Convert.ToInt64(Pic0PKGEntry.EntryOffset, 16);
                            }
                            if (!string.IsNullOrEmpty(Pic0PKGEntry.EntrySize))
                            {
                                EntrySizeDecValue = Convert.ToInt32(Pic0PKGEntry.EntrySize, 16);
                            }
                            Pic0OffsetPosition = ContainerOffsetDecValue + EntryOffsetDecValue;

                            // Seek to the beginning of the icon0.png file and read
                            byte[] Pic0FileBuffer = new byte[EntrySizeDecValue];
                            PKGReader.Seek(Pic0OffsetPosition, SeekOrigin.Begin);
                            PKGReader.ReadExactly(Pic0FileBuffer);

                            // Check the buffer and display the icon
                            if (Pic0FileBuffer is not null)
                            {
                                Bitmap Pic0BitmapImage;
                                using (var Pic0MemoryStream = new MemoryStream(Pic0FileBuffer))
                                {
                                    Pic0BitmapImage = new Bitmap(Pic0MemoryStream);
                                    CurrentPic0 = AnyBitmap.FromStream(Pic0MemoryStream);
                                }
                                GameBackground.Source = Pic0BitmapImage;
                                SaveBackgroundButton.IsEnabled = true;
                            }
                        }

                        PKGReader.Close();
                    }
                }
            }

        }
        catch (Exception)
        {

        }
    }

    private void LoadPSPInfo()
    {
        try
        {
            var PKGFileInfo = new FileInfo(SelectedPKG);
            var NewPKGDecryptor = new PKGDecryptor();

            NewPKGDecryptor.ProcessPKGFile(SelectedPKG);

            Dispatcher.UIThread.Invoke(() => PKGConsoleTextBlock.Text = "PSP");
            Dispatcher.UIThread.Invoke(() => PKGSizeTextBlock.Text = Utils.HumanReadableBytes(PKGFileInfo.Length));

            if (NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.ICON0) is not null)
            {
                Dispatcher.UIThread.Invoke(() => GameBackground.Source = Utils.AnyBitmapToIImage(NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.ICON0)!));
                Dispatcher.UIThread.Invoke(() => SaveIconButton.IsEnabled = true);

                CurrentIcon0 = NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.ICON0);
            }
            if (NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC1) is not null)
            {
                Dispatcher.UIThread.Invoke(() => Background = Utils.AnyBitmapToImageBrush(NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC1)!));
                Dispatcher.UIThread.Invoke(() => SaveBackgroundButton.IsEnabled = true);

                CurrentPic0 = NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC1);
            }
            if (NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC2) is not null)
            {
                Dispatcher.UIThread.Invoke(() => GameIcon.Source = Utils.AnyBitmapToIImage(NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC2)!));
            }
            if (NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.SND0) is not null)
            {
                Dispatcher.UIThread.Invoke(() => PKGSoundBytes = NewPKGDecryptor.GetSND);
                Dispatcher.UIThread.Invoke(() => PlayStopButton.IsEnabled = true);
            }

            if (NewPKGDecryptor.GetPARAMSFO is not null)
            {
                var SFOKeys = SFONew.ReadSfo(NewPKGDecryptor.GetPARAMSFO);

                if (SFOKeys.TryGetValue("TITLE", out var TITLEValue))
                {
                    Dispatcher.UIThread.Invoke(() => Utils.CleanTitle(TITLEValue.ToString()!));
                }
                if (SFOKeys.TryGetValue("CATEGORY", out var CATEGORYValue))
                {
                    Dispatcher.UIThread.Invoke(() => PKGCategoryTextBlock.Text = GetPSPCategory(CATEGORYValue.ToString()!));
                }
                if (SFOKeys.TryGetValue("DISC_ID", out var CONTENTIDValue))
                {
                    Dispatcher.UIThread.Invoke(() => PKGContentIDTextBlock.Text = CONTENTIDValue.ToString());
                }
                if (SFOKeys.TryGetValue("REGION", out var REGIONValue))
                {
                    Dispatcher.UIThread.Invoke(() => PKGRegionTextBlock.Text = REGIONValue.ToString());
                }
                if (SFOKeys.TryGetValue("DISC_VERSION", out var DISCVERValue))
                {
                    Dispatcher.UIThread.Invoke(() => PKGAppVerTextBlock.Text = DISCVERValue.ToString());
                }
                if (SFOKeys.TryGetValue("PSP_SYSTEM_VER", out var PSPSYSTEMVERValue))
                {
                    Dispatcher.UIThread.Invoke(() => PKGFirmwareVersionTextBlock.Text = PSPSYSTEMVERValue.ToString());
                }
            }
        }
        catch (Exception)
        {

        }
    }

    private async void LoadPSVInfo()
    {
        try
        {
            var PKGFileInfo = new FileInfo(SelectedPKG);
            string PKGIconURL = string.Empty;
            string PKGTitleID = string.Empty;
            string PKGContentID = string.Empty;

            Dispatcher.UIThread.Invoke(() => PKGConsoleTextBlock.Text = "PS Vita");
            Dispatcher.UIThread.Invoke(() => PKGSizeTextBlock.Text = Utils.HumanReadableBytes(PKGFileInfo.Length));

            using (var SFOReader = new Process())
            {
                SFOReader.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "PSN_get_pkg_info.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "PSN_get_pkg_info");
                SFOReader.StartInfo.Arguments = "\"" + SelectedPKG + "\"";
                SFOReader.StartInfo.RedirectStandardOutput = true;
                SFOReader.StartInfo.UseShellExecute = false;
                SFOReader.StartInfo.CreateNoWindow = true;
                SFOReader.Start();

                var OutputReader = SFOReader.StandardOutput;
                string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                if (ProcessOutput.Length > 0)
                {
                    foreach (var Line in ProcessOutput)
                    {
                        if (Line.StartsWith("Title:"))
                        {
                            Dispatcher.UIThread.Invoke(() => PKGTitleTextBlock.Text = Line.Split(':')[1].Trim('"').Trim());
                        }
                        else if (Line.StartsWith("Title ID:"))
                        {
                            Dispatcher.UIThread.Invoke(() => PKGTitleIDTextBlock.Text = Line.Split(':')[1].Trim('"').Trim());
                            PKGTitleID = Line.Split(':')[1].Trim('"').Trim();
                        }
                        else if (Line.StartsWith("NPS Type:"))
                        {
                            Dispatcher.UIThread.Invoke(() => PKGTypeTextBlock.Text = Line.Split(':')[1].Trim('"').Trim());
                            Dispatcher.UIThread.Invoke(() => PKGCategoryTextBlock.Text = Line.Split(':')[1].Trim('"').Trim());
                        }
                        else if (Line.StartsWith("App Ver:"))
                        {
                            Dispatcher.UIThread.Invoke(() => PKGAppVerTextBlock.Text = Line.Split(':')[1].Trim('"'));
                        }
                        else if (Line.StartsWith("Min FW:"))
                        {
                            Dispatcher.UIThread.Invoke(() => PKGFirmwareVersionTextBlock.Text = Line.Split(':')[1].Trim('"'));
                        }
                        else if (Line.StartsWith("Version:"))
                        {
                            Dispatcher.UIThread.Invoke(() => PKGVersionTextBlock.Text = Line.Split(':')[1].Trim('"'));
                        }
                        else if (Line.StartsWith("Content ID:"))
                        {
                            Dispatcher.UIThread.Invoke(() => PKGContentIDTextBlock.Text = Line.Split(':')[1].Trim('"').Trim());
                            PKGContentID = Line.Split(':')[1].Trim('"').Trim();
                        }
                        else if (Line.StartsWith("Region:"))
                        {
                            Dispatcher.UIThread.Invoke(() => PKGRegionTextBlock.Text = Line.Split(':')[1].Trim('"').Trim());
                        }
                        else if (Line.StartsWith("c_date:"))
                        {
                            Dispatcher.UIThread.Invoke(() => PKGBuildDateTextBlock.Text = Line.Split(':')[1].Trim('"').Trim());
                        }
                    }

                    if (await Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + PKGTitleID + ".png"))
                    {
                        AnyBitmap GameCoverIcon = AnyBitmap.FromUri(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + PKGTitleID + ".png", UriKind.Absolute));

                        Dispatcher.UIThread.Invoke(() => GameIcon.Source = Utils.AnyBitmapToIImage(GameCoverIcon));
                        Dispatcher.UIThread.Invoke(() => SaveIconButton.IsEnabled = true);

                        CurrentIcon0 = GameCoverIcon;

                    }

                }
            }

            Dispatcher.UIThread.Invoke(() => PKGStateTextBlock.Text = "Not available");
            Dispatcher.UIThread.Invoke(() => PKGDataTypeTextBlock.Text = "Not available");
            Dispatcher.UIThread.Invoke(() => PKGAttributesTextBlock.Text = "Not available");
        }
        catch (Exception)
        {

        }
    }

    public static string GetPS4Category(string SFOCategory)
    {
        switch (SFOCategory ?? "")
        {
            case "ac":
                {
                    return "Additional Content";
                }
            case "bd":
                {
                    return "Blu-ray Disc";
                }
            case "gc":
                {
                    return "Game Content";
                }
            case "gd":
                {
                    return "Game Digital Application";
                }
            case "gda":
                {
                    return "System Application";
                }
            case "gdb":
                {
                    return "Unknown";
                }
            case "gdc":
                {
                    return "Non-Game Big Application";
                }
            case "gdd":
                {
                    return "BG Application";
                }
            case "gde":
                {
                    return "Non-Game Mini App / Video Service Native App";
                }
            case "gdk":
                {
                    return "Video Service Web App";
                }
            case "gdl":
                {
                    return "PS Cloud Beta App";
                }
            case "gdO":
                {
                    return "PS2 Classic";
                }
            case "gp":
                {
                    return "Game Application Patch";
                }
            case "gpc":
                {
                    return "Non-Game Big App Patch";
                }
            case "gpd":
                {
                    return "BG Application patch";
                }
            case "gpe":
                {
                    return "Non-Game Mini App Patch / Video Service Native App Patch";
                }
            case "gpk":
                {
                    return "Video Service Web App Patch";
                }
            case "gpl":
                {
                    return "PS Cloud Beta App Patch";
                }
            case "sd":
                {
                    return "Save Data";
                }
            case "la":
                {
                    return "Live Area";
                }
            case "wda":
                {
                    return "Unknown";
                }

            default:
                {
                    return "Unknown";
                }
        }
    }

    public static string GetPS3Category(string SFOCategory)
    {
        switch (SFOCategory ?? "")
        {
            case "DG":
                {
                    return "Disc Game";
                }
            case "AR":
                {
                    return "Autoinstall Root";
                }
            case "DP":
                {
                    return "Disc Packages";
                }
            case "IP":
                {
                    return "Install Package";
                }
            case "TR":
                {
                    return "Theme Root";
                }
            case "VR":
                {
                    return "Vide Root";
                }
            case "VI":
                {
                    return "Video Item";
                }
            case "XR":
                {
                    return "Extra Root";
                }
            case "DM":
                {
                    return "Disc Movie";
                }
            case "HG":
                {
                    return "HDD Game";
                }
            case "GD":
                {
                    return "Game Data";
                }
            case "SD":
                {
                    return "Save Data";
                }
            case "PP":
                {
                    return "PSP";
                }
            case "PE":
                {
                    return "PSP Emulator";
                }
            case "MN":
                {
                    return "PSP Minis";
                }
            case "1P":
                {
                    return "PS1 PSN";
                }
            case "2P":
                {
                    return "PS2 PSN";
                }

            default:
                {
                    return "Unknown";
                }
        }
    }

    public static string GetPS3GameRegion(string GameID)
    {
        if (GameID.StartsWith("BLES"))
        {
            return "Europe";
        }
        else if (GameID.StartsWith("BCES"))
        {
            return "Europe";
        }
        else if (GameID.StartsWith("NPEB"))
        {
            return "Europe";
        }
        else if (GameID.StartsWith("BLUS"))
        {
            return "USA";
        }
        else if (GameID.StartsWith("BCUS"))
        {
            return "USA";
        }
        else if (GameID.StartsWith("NPUB"))
        {
            return "USA";
        }
        else if (GameID.StartsWith("BCJS"))
        {
            return "Japan";
        }
        else if (GameID.StartsWith("BLJS"))
        {
            return "Japan";
        }
        else if (GameID.StartsWith("NPJB"))
        {
            return "Japan";
        }
        else if (GameID.StartsWith("BCAS"))
        {
            return "Asia";
        }
        else if (GameID.StartsWith("BLAS"))
        {
            return "Asia";
        }
        else
        {
            return "";
        }
    }

    public static string GetPSPCategory(string SFOCategory)
    {
        switch (SFOCategory ?? "")
        {
            case "UG":
                {
                    return "UMD Game";
                }
            case "PG":
                {
                    return "Game Update";
                }
            case "EG":
                {
                    return "PSP Remaster";
                }
            case "MA":
                {
                    return "App";
                }
            case "ME":
                {
                    return "Memory Stick Emulated Content";
                }
            case "MG":
                {
                    return "Memory Stick Game";
                }
            case "MS":
                {
                    return "Memory Stick Save Data";
                }
            default:
                {
                    return "Unknown";
                }
        }
    }

    private static string TryPKGAsync(string PKGPath)
    {
        string DetectedConsole = "";

        // Switch to main application path when coming from console
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        // Try as PS4 PKG
        try
        {
            PKG.SceneRelated.Unprotected_PKG? GamePKG = PKG.SceneRelated.Read_PKG(PKGPath);

            if (GamePKG.Param != null)
            {
                DetectedConsole = "PS4";
            }

            GamePKG = null;
            return DetectedConsole;
        }
        catch (Exception) { Trace.WriteLine("No PS4 PKG"); }

        // Try as Vita PKG & get package (sfo) name
        try
        {
            using var PKG2ZIP = new Process();
            PKG2ZIP.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "pkg2zip.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "pkg2zip");
            PKG2ZIP.StartInfo.Arguments = $"-l \"{PKGPath}\"";
            PKG2ZIP.StartInfo.RedirectStandardOutput = true;
            PKG2ZIP.StartInfo.RedirectStandardError = true;
            PKG2ZIP.StartInfo.CreateNoWindow = false;
            PKG2ZIP.Start();

            var OutputReader = PKG2ZIP.StandardOutput;
            var ErrorReader = PKG2ZIP.StandardError;
            string ProcessOutput = OutputReader.ReadToEnd();
            string ProcessErrors = ErrorReader.ReadToEnd();

            PKG2ZIP.WaitForExit();

            if (ProcessOutput != null && ProcessOutput.Length > 0)
            {
                if (!ProcessOutput.Contains("ERROR: not a pkg file"))
                {
                    DetectedConsole = "PSV";
                    return DetectedConsole;
                }
            }

            if (ProcessErrors != null && ProcessErrors.Length > 0)
            {
                if (!ProcessErrors.Contains("ERROR: not a pkg file"))
                {
                    DetectedConsole = "PSV";
                    return DetectedConsole;
                }
                else
                {
                    Trace.WriteLine("No PS Vita PKG");
                }
            }

        }
        catch (Exception) { Trace.WriteLine("No PS Vita PKG"); }

        //Try as PS3/PSP PKG
        try
        {
            FileInfo PKGFileInfo = new(PKGPath);
            PKGDecryptor NewPKGDecryptor = new();

            NewPKGDecryptor.ProcessPKGFile(PKGPath);

            if (NewPKGDecryptor.GetPKGPlatform == PKGDecryptor.PKGPlatform.PS3)
            {
                DetectedConsole = "PS3";
            }
            else if (NewPKGDecryptor.GetPKGPlatform == PKGDecryptor.PKGPlatform.PSP)
            {
                DetectedConsole = "PSP";
            }

            return DetectedConsole;
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Info", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            box.ShowWindowAsync();
            Trace.WriteLine("No PS3/PSP PKG");
        }

        // Try as PS5 PKG
        try
        {
            string FirstString = "";
            sbyte Int8AtOffset5;
            bool IsValidPKG = false;

            using var PKGReader = new FileStream(PKGPath, FileMode.Open, FileAccess.Read);
            var BinReader = new BinaryReader(PKGReader);
            FirstString = BinReader.ReadString();
            PKGReader.Seek(5, SeekOrigin.Begin);
            Int8AtOffset5 = BinReader.ReadSByte();

            PKGReader.Close();
            BinReader.Close();

            if (!string.IsNullOrEmpty(FirstString))
            {
                if (FirstString.Contains("CNT"))
                {
                    // _sc (Source) PKG
                    IsValidPKG = true;
                    DetectedConsole = "PS5";
                    return DetectedConsole;
                }
            }

            if (Int8AtOffset5 == -128)
            {
                // Retail PKG
                IsValidPKG = true;
                DetectedConsole = "PS5";
                return DetectedConsole;
            }

            // Final check if debug/self created PKG
            if (!IsValidPKG)
            {
                string ExtractedPKGConfigurationData = "";
                string PKGConfigurationStartString = "<package-configuration version=\"1.0\" type=\"package-info\">";
                string PKGConfigurationEndString = "</package-configuration>";

                byte[] PKGConfigurationStartBytes = Encoding.UTF8.GetBytes(PKGConfigurationStartString);
                byte[] PKGConfigurationEndBytes = Encoding.UTF8.GetBytes(PKGConfigurationEndString);

                long PKGConfigurationStartOffset = -1;
                long PKGConfigurationEndOffset = -1;

                // Get the PKG configuration
                using var NewPKGReader = new FileStream(PKGPath, FileMode.Open, FileAccess.Read);
                var buffer = new byte[4097];
                long fileLength = NewPKGReader.Length;
                long totalBytesRead = fileLength;

                // Read backwards to find the end string first
                while (totalBytesRead > 0)
                {
                    int bytesRead = (int)Math.Min(buffer.Length, totalBytesRead);
                    NewPKGReader.Seek(totalBytesRead - bytesRead, SeekOrigin.Begin);
                    NewPKGReader.ReadExactly(buffer, 0, bytesRead);
                    totalBytesRead -= bytesRead;

                    // Check buffer from end to start
                    bool exitWhile = false;
                    for (int i = bytesRead - 1; i >= 0; i -= 1)
                    {
                        // Check for end string
                        if (PKGConfigurationEndOffset == -1 && MatchBytes(buffer, i, PKGConfigurationEndBytes))
                        {
                            PKGConfigurationEndOffset = totalBytesRead + i + PKGConfigurationEndBytes.Length;
                        }

                        // Check for start string
                        if (PKGConfigurationStartOffset == -1 && MatchBytes(buffer, i, PKGConfigurationStartBytes))
                        {
                            PKGConfigurationStartOffset = totalBytesRead + i;
                        }

                        if (PKGConfigurationStartOffset != -1 && PKGConfigurationEndOffset != -1)
                        {
                            exitWhile = true;
                            break;
                        }
                    }

                    if (exitWhile)
                    {
                        break;
                    }
                }

                if (PKGConfigurationStartOffset != -1 & PKGConfigurationEndOffset != -1 && PKGConfigurationEndOffset > PKGConfigurationStartOffset)
                {
                    PKGConfigurationStartOffset -= PKGConfigurationStartBytes.Length;
                    PKGConfigurationEndOffset -= PKGConfigurationEndBytes.Length - 1;

                    // Calculate the size of the pkg configuration data
                    long PKGConfigurationDataSize = PKGConfigurationEndOffset - PKGConfigurationStartOffset;

                    // Move to the start offset
                    NewPKGReader.Seek(PKGConfigurationStartOffset, SeekOrigin.Begin);

                    // Read the pkg configuration data
                    var data = new byte[((int)PKGConfigurationDataSize)];
                    NewPKGReader.ReadExactly(data, 0, (int)PKGConfigurationDataSize);

                    // Convert the data to a XML string
                    ExtractedPKGConfigurationData = Encoding.UTF8.GetString(data);
                    ExtractedPKGConfigurationData = string.Concat("<?xml version=\"1.0\" encoding=\"utf-8\"?>", ExtractedPKGConfigurationData);
                }

                NewPKGReader.Close();

                if (!string.IsNullOrEmpty(ExtractedPKGConfigurationData))
                {
                    // Debug PKG
                    IsValidPKG = true;
                }
            }

            if (IsValidPKG)
            {
                DetectedConsole = "PS5";
                return DetectedConsole;
            }

        }
        catch (Exception) { Trace.WriteLine("No PS5 PKG"); }

        return DetectedConsole;
    }

    private static bool MatchBytes(byte[] buffer, int position, byte[] pattern)
    {
        if (position + 1 < pattern.Length)
        {
            return false;
        }

        for (int i = 0, loopTo = pattern.Length - 1; i <= loopTo; i++)
        {
            if (buffer[position - i] != pattern[pattern.Length - 1 - i])
            {
                return false;
            }
        }

        return true;
    }

    private async void SaveIconButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CurrentIcon0 is not null)
        {

            var newSaveFileDialog = new SaveFileDialog() { Title = "Select a save path", Filters = [new FileDialogFilter() { Name = "PNG files", Extensions = { "png" } }] };
            var selectedSaveFile = await newSaveFileDialog.ShowAsync(this);
            if (selectedSaveFile != null)
            {
                CurrentIcon0.TrySaveAs(selectedSaveFile, AnyBitmap.ImageFormat.Png);
            }
        }
    }

    private async void SaveBackgroundButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CurrentPic0 is not null)
        {
            var newSaveFileDialog = new SaveFileDialog() { Title = "Select a save path", Filters = [new FileDialogFilter() { Name = "PNG files", Extensions = { "png" } }] };
            var selectedSaveFile = await newSaveFileDialog.ShowAsync(this);
            if (selectedSaveFile != null)
            {
                CurrentPic0.TrySaveAs(selectedSaveFile, AnyBitmap.ImageFormat.Png);
            }
        }
    }

}