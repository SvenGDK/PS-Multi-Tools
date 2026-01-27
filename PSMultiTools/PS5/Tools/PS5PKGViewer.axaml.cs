using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using PSMultiTools.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static PSMultiTools.Classes.PS5ParamClass;

namespace PSMultiTools.PS5.Tools;

public partial class PS5PKGViewer : Window
{
    public PS5PKGViewer()
    {
        InitializeComponent();
        DataContext = this;
    }

    public List<PS5PKGRootFile> PFSImageRootFiles = [];
    public List<PS5PKGRootDirectory> PFSImageRootDirectories = [];
    public List<PS5PKGRootFile> PFSImageURootFiles = [];
    public List<PS5PKGRootFile> NestedImageRootFiles = [];
    public List<PS5PKGRootDirectory> NestedImageRootDirectories = [];
    public List<PS5PKGRootFile> NestedImageURootFiles = [];

    private bool IsSourcePKG = false;
    private bool IsRetailPKG = false;

    private string CurrentParamJSON = "";
    private XDocument? CurrentConfigurationXML = null;
    private Bitmap? CurrentIcon0 = null;
    private Bitmap? CurrentPic0 = null;

    private async void BrowsePKGFileButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
            return;

        var newOpenFileDialog = new OpenFileDialog() { Title = "Select a PS5 PKG file :", AllowMultiple = false, Filters = [new FileDialogFilter() { Name = "PKG files", Extensions = { "pkg" } }] };
        var selectedFile = await newOpenFileDialog.ShowAsync(window);

        if (selectedFile is null || selectedFile.Length == 0)
            return;
        if (selectedFile[0] is not null)
        {
            SelectedPKGFileTextBox.Text = selectedFile[0];

            // Clear previous ListBox items and lists
            PKGContentListBox.Items.Clear();
            PKGScenariosListBox.Items.Clear();
            PKGChunksListBox.Items.Clear();
            PKGOutersListBox.Items.Clear();

            PFSImageRootFiles.Clear();
            PFSImageRootDirectories.Clear();
            PFSImageURootFiles.Clear();
            NestedImageRootFiles.Clear();
            NestedImageRootDirectories.Clear();
            NestedImageURootFiles.Clear();

            // Reset
            PKGIconImage.Source = null;
            IsSourcePKG = false;
            IsRetailPKG = false;
            CurrentParamJSON = "";
            CurrentConfigurationXML = null;
            CurrentIcon0 = null;
            CurrentPic0 = null;

            // Determine PS5 PKG
            string FirstString = "";
            sbyte Int8AtOffset5;
            using (var PKGReader = new FileStream(selectedFile[0], FileMode.Open, FileAccess.Read))
            {
                var BinReader = new BinaryReader(PKGReader);
                FirstString = BinReader.ReadString();
                PKGReader.Seek(5, SeekOrigin.Begin);
                Int8AtOffset5 = BinReader.ReadSByte();
                PKGReader.Close();
            }

            if (!string.IsNullOrEmpty(FirstString))
            {
                if (FirstString.Contains("CNT"))
                {
                    IsSourcePKG = true;
                }
                else
                {
                    IsSourcePKG = false;
                }
            }

            if (Int8AtOffset5 == -128)
            {
                IsRetailPKG = true;
            }

            if (IsRetailPKG | IsSourcePKG)
            {
                // Get only param.json
                byte[] startBytes = Encoding.UTF8.GetBytes("param.json");
                byte[] endBytes = Encoding.UTF8.GetBytes("version.xml");

                long startOffset = -1;
                long endOffset = -1;

                using (var PKGReader = new FileStream(selectedFile[0], FileMode.Open, FileAccess.Read))
                {
                    var buffer = new byte[4096];
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

                // A problem here is that the found offsets values are around the actual param.json because the PKG is not propperly read. However, with some fixes it will actually parse.
                if (startOffset != -1 && endOffset != -1 && endOffset > startOffset)
                {
                    string FinalParamJSONString = "";
                    using (var ParamJSONFileStream = new FileStream(selectedFile[0], FileMode.Open, FileAccess.Read))
                    {
                        long ParamDataSize = endOffset - startOffset;
                        ParamJSONFileStream.Seek(startOffset, SeekOrigin.Begin);

                        var NewParamData = new byte[((int)ParamDataSize)];
                        ParamJSONFileStream.ReadExactly(NewParamData, 0, (int)ParamDataSize);

                        var NewUTF8Encoding = new UTF8Encoding(false, false);
                        string ExtractedData = NewUTF8Encoding.GetString(NewParamData);
                        List<string> ParamJSONData = [.. ExtractedData.Split(["\r\n"], StringSplitOptions.None)];

                        if (ParamDataSize > 5000) // Occurs in PKGs that have multiple param.json - get only 1
                        {
                            // Pick only the first param.json
                            string versionFileUriMark = "versionFileUri";
                            int versionFileUriLine = ParamJSONData.FindIndex(line => line != null && line.Contains(versionFileUriMark, StringComparison.OrdinalIgnoreCase));
                            if (versionFileUriLine >= 0)
                            {
                                int ClosingLineIndex = ParamJSONData.FindIndex(versionFileUriLine, line => line != null && line.Trim() == "}");
                                ParamJSONData = [.. ParamJSONData.Take(ClosingLineIndex + 1)];
                            }
                        }

                        //Remove any unwanted unicode characters
                        //for (int i = 0; i < ParamJSONData.Count; i++)
                        //{
                        //    ParamJSONData[i] = Regex.Replace(ParamJSONData[i], @"[^\t\r\n -~]", "");
                        //}

                        if (ParamDataSize > 5000)
                        {
                            //Adjust the output
                            ParamJSONData.RemoveAt(0);
                            ParamJSONData.Insert(0, "{");
                            ParamJSONData[^1] += "\"";

                            // Additional cleanup for previously cutted param.json
                            string versionFileUriMark = "versionFileUri";
                            int RecheckedversionFileUriLine = ParamJSONData.FindIndex(line => line != null && line.Contains(versionFileUriMark, StringComparison.OrdinalIgnoreCase));
                            if (RecheckedversionFileUriLine >= 0)
                            {
                                int ClosingLineIndex = ParamJSONData.FindIndex(RecheckedversionFileUriLine, line => line != null && line.Trim() == "}");
                                ParamJSONData[RecheckedversionFileUriLine] = ParamJSONData[RecheckedversionFileUriLine].Replace("\"\"", "\"");
                            }
                        }
                        else
                        {
                            //Adjust the output
                            ParamJSONData.RemoveAt(0);
                            ParamJSONData.Insert(0, "{");
                            ParamJSONData[^1] += "\"";
                            ParamJSONData.Add("}");
                        }

                        FinalParamJSONString = string.Join("\n", ParamJSONData);
                    }

                    if (!string.IsNullOrEmpty(FinalParamJSONString))
                    {
                        // Extra JSON cleanup
                        FinalParamJSONString = FinalParamJSONString.Trim();
                        int lastBrace = FinalParamJSONString.LastIndexOf('}');
                        if (lastBrace > 0)
                            FinalParamJSONString = FinalParamJSONString[..(lastBrace + 1)].Trim();

                        CurrentParamJSON = FinalParamJSONString;

                        // Display pkg information
                        var ParamData = JsonConvert.DeserializeObject<PS5Param>(FinalParamJSONString);
                        var NewPS5Game = new PS5Game() { GameBackupType = PS5Game.BackupType.LocalPKG };
                        if (ParamData is not null)
                        {
                            if (ParamData.TitleId is not null)
                            {
                                NewPS5Game.GameID = "Title ID: " + ParamData.TitleId;
                                NewPS5Game.GameRegion = "Region: " + PS5Game.GetGameRegion(ParamData.TitleId);
                            }

                            bool NonENUSTitle = false;
                            if (ParamData.LocalizedParameters!.EnUS is not null)
                            {
                                NewPS5Game.GameTitle = ParamData.LocalizedParameters.EnUS.TitleName;
                            }
                            else
                            {
                                NonENUSTitle = true;
                            }
                            if (ParamData.LocalizedParameters.DeDE is not null)
                            {
                                NewPS5Game.DEGameTitle = ParamData.LocalizedParameters.DeDE.TitleName;
                            }
                            if (ParamData.LocalizedParameters.FrFR is not null)
                            {
                                NewPS5Game.FRGameTitle = ParamData.LocalizedParameters.FrFR.TitleName;
                            }
                            if (ParamData.LocalizedParameters.ItIT is not null)
                            {
                                NewPS5Game.ITGameTitle = ParamData.LocalizedParameters.ItIT.TitleName;
                            }
                            if (ParamData.LocalizedParameters.EsES is not null)
                            {
                                NewPS5Game.ESGameTitle = ParamData.LocalizedParameters.EsES.TitleName;
                            }
                            if (ParamData.LocalizedParameters.JaJP is not null)
                            {
                                NewPS5Game.JPGameTitle = ParamData.LocalizedParameters.JaJP.TitleName;
                            }
                            if (NonENUSTitle)
                            {
                                NewPS5Game.GameTitle = GetAlternativeGameTitle(ParamData);
                            }

                            if (ParamData.ContentId is not null)
                            {
                                NewPS5Game.GameContentID = "Content ID: " + ParamData.ContentId;
                            }

                            if (ParamData.ApplicationCategoryType == 0)
                            {
                                NewPS5Game.GameCategory = "Type: PS5 Game";
                            }
                            else if (ParamData.ApplicationCategoryType == 65792)
                            {
                                NewPS5Game.GameCategory = "Type: RNPS Media App";
                            }
                            else if (ParamData.ApplicationCategoryType == 131328)
                            {
                                NewPS5Game.GameCategory = "Type: System Built-in App";
                            }
                            else if (ParamData.ApplicationCategoryType == 131584)
                            {
                                NewPS5Game.GameCategory = "Type: Big Daemon";
                            }
                            else if (ParamData.ApplicationCategoryType == 16777216)
                            {
                                NewPS5Game.GameCategory = "Type: ShellUI";
                            }
                            else if (ParamData.ApplicationCategoryType == 33554432)
                            {
                                NewPS5Game.GameCategory = "Type: Daemon";
                            }
                            else if (ParamData.ApplicationCategoryType == 67108864)
                            {
                                NewPS5Game.GameCategory = "Type: ShellApp";
                            }

                            long PS5GameSize = new FileInfo(selectedFile[0]).Length;
                            NewPS5Game.GameSize = $"Size: {Utils.HumanReadableBytes(PS5GameSize)}";

                            if (ParamData.ContentVersion is not null)
                            {
                                NewPS5Game.GameVersion = "Version: " + ParamData.ContentVersion;
                            }
                            if (ParamData.RequiredSystemSoftwareVersion is not null)
                            {
                                NewPS5Game.GameRequiredFirmware = "Required Firmware: " + ParamData.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, ".").Insert(5, ".").Insert(8, ".").Remove(11, 8);
                            }

                            GameTitleTextBlock.IsVisible = true;
                            GameIDTextBlock.IsVisible = true;
                            GameRegionTextBlock.IsVisible = true;
                            GameVersionTextBlock.IsVisible = true;
                            GameContentIDTextBlock.IsVisible = true;
                            GameCategoryTextBlock.IsVisible = true;
                            GameSizeTextBlock.IsVisible = true;
                            GameRequiredFirmwareTextBlock.IsVisible = true;

                            GameTitleTextBlock.Text = NewPS5Game.GameTitle;
                            GameIDTextBlock.Text = NewPS5Game.GameID;
                            GameRegionTextBlock.Text = NewPS5Game.GameRegion;
                            GameVersionTextBlock.Text = NewPS5Game.GameVersion;
                            GameContentIDTextBlock.Text = NewPS5Game.GameContentID;
                            GameCategoryTextBlock.Text = NewPS5Game.GameCategory;
                            GameSizeTextBlock.Text = NewPS5Game.GameSize;
                            GameRequiredFirmwareTextBlock.Text = NewPS5Game.GameRequiredFirmware;
                        }
                    }
                }
                return;
            }

            // Probably a self created PKG that contains a package configuration
            string ExtractedPKGConfigurationData = "";
            string PKGConfigurationStartString = "<package-configuration version=\"1.0\" type=\"package-info\">";
            string PKGConfigurationEndString = "</package-configuration>";

            byte[] PKGConfigurationStartBytes = Encoding.UTF8.GetBytes(PKGConfigurationStartString);
            byte[] PKGConfigurationEndBytes = Encoding.UTF8.GetBytes(PKGConfigurationEndString);

            long PKGConfigurationStartOffset = -1;
            long PKGConfigurationEndOffset = -1;

            // 1. Get the PKG configuration
            using (var PKGReader = new FileStream(selectedFile[0], FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[4097];
                long fileLength = PKGReader.Length;
                long totalBytesRead = fileLength;

                // Read backwards to find the end string first
                while (totalBytesRead > 0L)
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

            // 2. Process the retrieved PKG configuration data
            if (!string.IsNullOrEmpty(ExtractedPKGConfigurationData))
            {
                // Load the XML file
                XDocument PKGConfigurationXML = XDocument.Parse(ExtractedPKGConfigurationData);
                CurrentConfigurationXML = PKGConfigurationXML;

                if (PKGConfigurationXML != null)
                {
                    // Get the PKG config values
                    var PKGConfig = PKGConfigurationXML.Element("config");
                    if (PKGConfig is not null)
                    {
                        string PKGConfigVersion = PKGConfig.Attribute("version")!.Value;
                        string PKGConfigMetadata = PKGConfig.Attribute("metadata")!.Value;
                        string PKGConfigPrimary = PKGConfig.Attribute("primary")!.Value;
                    }
                    string PKGConfigContentID = PKGConfigurationXML.Descendants("config").First().Element("content-id")!.Value;
                    string PKGConfigPrimaryID = PKGConfigurationXML.Descendants("config").First().Element("primary-id")!.Value;
                    string PKGConfigLongName = PKGConfigurationXML.Descendants("config").First().Element("longname")!.Value;
                    string PKGConfigRequiredSystemVersion = PKGConfigurationXML.Descendants("config").First().Element("required-system-version")!.Value;
                    string PKGConfigDRMType = PKGConfigurationXML.Descendants("config").First().Element("drm-type")!.Value;
                    string PKGConfigContentType = PKGConfigurationXML.Descendants("config").First().Element("content-type")!.Value;
                    string PKGConfigApplicationType = PKGConfigurationXML.Descendants("config").First().Element("application-type")!.Value;
                    string PKGConfigNumberOfImages = PKGConfigurationXML.Descendants("config").First().Element("num-of-images")!.Value;
                    string PKGConfigSize = PKGConfigurationXML.Descendants("config").First().Element("package-size")!.Value;
                    string PKGConfigVersionDate = PKGConfigurationXML.Descendants("config").First().Element("version-date")!.Value;
                    string PKGConfigVersionHash = PKGConfigurationXML.Descendants("config").First().Element("version-hash")!.Value;

                    // Get the PKG digests
                    var PKGDigests = PKGConfigurationXML.Element("digests");
                    if (PKGDigests is not null)
                    {
                        string PKGDigestsVersion = PKGDigests.Attribute("version")!.Value;
                        string PKGDigestsMajorParamVersion = PKGDigests.Attribute("major-param-version")!.Value;
                    }
                    string PKGContentDigest = PKGConfigurationXML.Descendants("digests").First().Element("content-digest")!.Value;
                    string PKGGameDigest = PKGConfigurationXML.Descendants("digests").First().Element("game-digest")!.Value;
                    string PKGHeaderDigest = PKGConfigurationXML.Descendants("digests").First().Element("header-digest")!.Value;
                    string PKGSystemDigest = PKGConfigurationXML.Descendants("digests").First().Element("system-digest")!.Value;
                    string PKGParamDigest = PKGConfigurationXML.Descendants("digests").First().Element("param-digest")!.Value;
                    string PKGDigest = PKGConfigurationXML.Descendants("digests").First().Element("package-digest")!.Value;

                    // Get the PKG params
                    string PKGParamApplicationDRMType = PKGConfigurationXML.Descendants("params").First().Element("applicationDrmType")!.Value;
                    string PKGParamContentID = PKGConfigurationXML.Descendants("params").First().Element("contentId")!.Value;
                    string PKGParamContentVersion = PKGConfigurationXML.Descendants("params").First().Element("contentVersion")!.Value;
                    string PKGParamMasterVersion = PKGConfigurationXML.Descendants("params").First().Element("masterVersion")!.Value;
                    string PKGParamRequiredSystemVersion = PKGConfigurationXML.Descendants("params").First().Element("requiredSystemSoftwareVersion")!.Value;
                    string PKGParamSDKVersion = PKGConfigurationXML.Descendants("params").First().Element("sdkVersion")!.Value;
                    string PKGParamTitleName = PKGConfigurationXML.Descendants("params").First().Element("titleName")!.Value;

                    // Get the PKG container information
                    string PKGContainerSize = PKGConfigurationXML.Descendants("container").First().Element("container-size")!.Value;
                    string PKGContainerMandatorySize = PKGConfigurationXML.Descendants("container").First().Element("mandatory-size")!.Value;
                    string PKGContainerBodyOffset = PKGConfigurationXML.Descendants("container").First().Element("body-offset")!.Value;
                    string PKGContainerBodySize = PKGConfigurationXML.Descendants("container").First().Element("body-size")!.Value;
                    string PKGContainerBodyDigest = PKGConfigurationXML.Descendants("container").First().Element("body-digest")!.Value;
                    string PKGContainerPromoteSize = PKGConfigurationXML.Descendants("container").First().Element("promote-size")!.Value;

                    // Get the PKG mount image
                    string PKGMountImagePFSOffsetAlign = PKGConfigurationXML.Descendants("mount-image").First().Element("pfs-offset-align")!.Value;
                    string PKGMountImagePFSSizeAlign = PKGConfigurationXML.Descendants("mount-image").First().Element("pfs-size-align")!.Value;
                    string PKGMountImagePFSImageOffset = PKGConfigurationXML.Descendants("mount-image").First().Element("pfs-image-offset")!.Value;
                    string PKGMountImagePFSImageSize = PKGConfigurationXML.Descendants("mount-image").First().Element("pfs-image-size")!.Value;
                    string PKGMountImageFixedInfoSize = PKGConfigurationXML.Descendants("mount-image").First().Element("fixed-info-size")!.Value;
                    string PKGMountImagePFSImageSeed = PKGConfigurationXML.Descendants("mount-image").First().Element("pfs-image-seed")!.Value;
                    string PKGMountImageSBlockDigest = PKGConfigurationXML.Descendants("mount-image").First().Element("sblock-digest")!.Value;
                    string PKGMountImageFixedInfoDigest = PKGConfigurationXML.Descendants("mount-image").First().Element("fixed-info-digest")!.Value;
                    string PKGMountImageOffset = PKGConfigurationXML.Descendants("mount-image").First().Element("mount-image-offset")!.Value;
                    string PKGMountImageSize = PKGConfigurationXML.Descendants("mount-image").First().Element("mount-image-size")!.Value;
                    string PKGMountImageContainerOffset = PKGConfigurationXML.Descendants("mount-image").First().Element("container-offset")!.Value;
                    string PKGMountImageSupplementalOffset = PKGConfigurationXML.Descendants("mount-image").First().Element("supplemental-offset")!.Value;

                    // Get the PKG entries and add to PKGContentListView
                    var PKGEntries = PKGConfigurationXML.Descendants("entries").Descendants("entry");
                    foreach (XElement PKGEntry in PKGEntries)
                    {
                        var NewPS5PKGEntry = new PS5PKGEntry() { EntryOffset = PKGEntry.Attribute("offset")!.Value, EntrySize = PKGEntry.Attribute("size")!.Value, EntryName = PKGEntry.Attribute("name")!.Value };
                        PKGContentListBox.Items.Add(NewPS5PKGEntry);
                    }

                    // Get the PKG chunkinfo
                    var PKGChunkInfo = PKGConfigurationXML.Element("chunkinfo");
                    if (PKGChunkInfo is not null)
                    {
                        string PKGChunkInfoSize = PKGChunkInfo.Attribute("size")!.Value;
                        string PKGChunkInfoNested = PKGChunkInfo.Attribute("nested")!.Value;
                        string PKGChunkInfoSDK = PKGChunkInfo.Attribute("sdk")!.Value;
                        string PKGChunkInfoDisps = PKGChunkInfo.Attribute("disps")!.Value;
                    }
                    string PKGChunkInfoContentID = PKGConfigurationXML.Descendants("chunkinfo").First().Element("contentid")!.Value;
                    string PKGChunkInfoLanguages = PKGConfigurationXML.Descendants("chunkinfo").First().Element("languages")!.Value;

                    // Get the PKG chunkinfo scenarios
                    var PKGChunkInfoScenarios = PKGConfigurationXML.Descendants("chunkinfo").Descendants("scenarios").Descendants("scenario");
                    foreach (XElement PKGChunkInfoScenario in PKGChunkInfoScenarios)
                    {
                        var NewPS5PKGChunkInfoScenario = new PS5PKGScenario()
                        {
                            ScenarioID = PKGChunkInfoScenario.Attribute("id")!.Value,
                            ScenarioType = PKGChunkInfoScenario.Attribute("type")!.Value,
                            ScenarioName = PKGChunkInfoScenario.Attribute("name")!.Value
                        };
                        PKGScenariosListBox.Items.Add(NewPS5PKGChunkInfoScenario);
                    }

                    // Get the PKG chunkinfo chunks
                    var PKGChunkInfoChunks = PKGConfigurationXML.Element("chunks");
                    if (PKGChunkInfoChunks is not null)
                    {
                        string PKGChunkInfoChunksNum = PKGChunkInfoChunks.Attribute("num")!.Value;
                        string PKGChunkInfoChunksDefault = PKGChunkInfoChunks.Attribute("default")!.Value;
                    }
                    var PKGChunkInfoChunksList = PKGConfigurationXML.Descendants("chunkinfo").Descendants("chunks").Descendants("chunk");
                    foreach (XElement PKGChunkInfoChunk in PKGChunkInfoChunksList)
                    {
                        var NewPS5PKGChunkInfoChunk = new PS5PKGChunk()
                        {
                            ChunkID = PKGChunkInfoChunk.Attribute("id")!.Value,
                            ChunkFlag = PKGChunkInfoChunk.Attribute("flag")!.Value,
                            ChunkLocus = PKGChunkInfoChunk.Attribute("locus")!.Value,
                            ChunkLanguage = PKGChunkInfoChunk.Attribute("language")!.Value,
                            ChunkDisps = PKGChunkInfoChunk.Attribute("disps")!.Value,
                            ChunkNum = PKGChunkInfoChunk.Attribute("num")!.Value,
                            ChunkSize = PKGChunkInfoChunk.Attribute("size")!.Value,
                            ChunkName = PKGChunkInfoChunk.Attribute("name")!.Value,
                            ChunkValue = PKGChunkInfoChunk.Value
                        };
                        PKGChunksListBox.Items.Add(NewPS5PKGChunkInfoChunk);
                    }

                    // Get the PKG chunkinfo outers
                    var PKGChunkInfoOuters = PKGConfigurationXML.Element("outers");
                    if (PKGChunkInfoOuters is not null)
                    {
                        string PKGChunkInfoOutersNum = PKGChunkInfoOuters.Attribute("num")!.Value;
                        string PKGChunkInfoOutersOverlapped = PKGChunkInfoOuters.Attribute("overlapped")!.Value;
                        string PKGChunkInfoOutersLanguageOverlapped = PKGChunkInfoOuters.Attribute("language-overlapped")!.Value;
                    }
                    var PKGChunkInfoOutersList = PKGConfigurationXML.Descendants("chunkinfo").Descendants("outers").Descendants("outer");
                    foreach (XElement PKGChunkInfoOuter in PKGChunkInfoOutersList)
                    {
                        var NewPS5PKGOuter = new PS5PKGOuter()
                        {
                            OuterID = PKGChunkInfoOuter.Attribute("id")!.Value,
                            OuterImage = PKGChunkInfoOuter.Attribute("image")!.Value,
                            OuterOffset = PKGChunkInfoOuter.Attribute("offset")!.Value,
                            OuterSize = PKGChunkInfoOuter.Attribute("size")!.Value,
                            OuterChunks = PKGChunkInfoOuter.Attribute("chunks")!.Value
                        };
                        PKGOutersListBox.Items.Add(NewPS5PKGOuter);
                    }

                    // Get the PKG pfs image info
                    var PKGPFSImage = PKGConfigurationXML.Element("pfs-image");
                    if (PKGPFSImage is not null)
                    {
                        string PKGPFSImageVersion = PKGPFSImage.Attribute("version")!.Value;
                        string PKGPFSImageReadOnly = PKGPFSImage.Attribute("readonly")!.Value;
                        string PKGPFSImageOffset = PKGPFSImage.Attribute("offset")!.Value;
                        string PKGPFSImageMetadata = PKGPFSImage.Attribute("metadata")!.Value;
                    }

                    // Get the PKG pfs image sblock info
                    var PKGPFSImageSBlock = PKGConfigurationXML.Descendants("sblock").FirstOrDefault();
                    if (PKGPFSImageSBlock is not null)
                    {
                        string PKGPFSImageSBlockSigned = PKGPFSImageSBlock.Attribute("signed")!.Value;
                        string PKGPFSImageSBlockEncrypted = PKGPFSImageSBlock.Attribute("encrypted")!.Value;
                        string PKGPFSImageSBlockIgnoreCase = PKGPFSImageSBlock.Attribute("ignore-case")!.Value;
                        string PKGPFSImageSBlockIndexSize = PKGPFSImageSBlock.Attribute("index-size")!.Value;
                        string PKGPFSImageSBlockBlocks = PKGPFSImageSBlock.Attribute("blocks")!.Value;
                        string PKGPFSImageSBlockBackups = PKGPFSImageSBlock.Attribute("backups")!.Value;
                    }
                    var PKGPFSImageSBlockImageSize = PKGConfigurationXML.Descendants("sblock").FirstOrDefault()!.Element("image-size");
                    if (PKGPFSImageSBlockImageSize is not null)
                    {
                        string PKGPFSImageSBlockImageSizeBlockSize = PKGPFSImageSBlockImageSize.Attribute("block-size")!.Value;
                        string PKGPFSImageSBlockImageSizeNum = PKGPFSImageSBlockImageSize.Attribute("num")!.Value;
                        string PKGPFSImageSBlockImageSizeValue = PKGPFSImageSBlockImageSize.Value;
                    }
                    var PKGPFSImageSBlockSuperInode = PKGConfigurationXML.Descendants("sblock").FirstOrDefault()!.Element("super-inode");
                    if (PKGPFSImageSBlockSuperInode is not null)
                    {
                        string PKGPFSImageSBlockSuperInodeBlocks = PKGPFSImageSBlockSuperInode.Attribute("blocks")!.Value;
                        string PKGPFSImageSBlockSuperInodeInodes = PKGPFSImageSBlockSuperInode.Attribute("inodes")!.Value;
                        string PKGPFSImageSBlockSuperInodeRoot = PKGPFSImageSBlockSuperInode.Attribute("root")!.Value;
                    }
                    var PKGPFSImageSBlockInode = PKGConfigurationXML.Descendants("sblock").FirstOrDefault()!.Descendants("super-inode").FirstOrDefault()!.Element("inode");
                    if (PKGPFSImageSBlockInode is not null)
                    {
                        string PKGPFSImageSBlockInodeSize = PKGPFSImageSBlockInode.Attribute("size")!.Value;
                        string PKGPFSImageSBlockInodeLinks = PKGPFSImageSBlockInode.Attribute("links")!.Value;
                        string PKGPFSImageSBlockInodeMode = PKGPFSImageSBlockInode.Attribute("mode")!.Value;
                        string PKGPFSImageSBlockInodeIMode = PKGPFSImageSBlockInode.Attribute("imode")!.Value;
                        string PKGPFSImageSBlockInodeIndex = PKGPFSImageSBlockInode.Attribute("index")!.Value;
                    }
                    string PKGPFSImageSBlockSeed = PKGConfigurationXML.Descendants("sblock").FirstOrDefault()!.Element("seed")!.Value;
                    string PKGPFSImageSBlockICV = PKGConfigurationXML.Descendants("sblock").FirstOrDefault()!.Element("icv")!.Value;

                    // Get the PKG pfs image root info
                    var PKGPFSImageRoot = PKGConfigurationXML.Descendants("pfs-image").FirstOrDefault()!.Element("root");
                    if (PKGPFSImageRoot is not null)
                    {
                        string PKGPFSImageRootSize = PKGPFSImageRoot.Attribute("size")!.Value;
                        string PKGPFSImageRootLinks = PKGPFSImageRoot.Attribute("links")!.Value;
                        string PKGPFSImageRootIMode = PKGPFSImageRoot.Attribute("imode")!.Value;
                        string PKGPFSImageRootIndex = PKGPFSImageRoot.Attribute("index")!.Value;
                        string PKGPFSImageRootINode = PKGPFSImageRoot.Attribute("inode")!.Value;
                        string PKGPFSImageRootName = PKGPFSImageRoot.Attribute("name")!.Value;
                    }
                    // Get the files in root
                    var PKGPFSImageRootFiles = PKGConfigurationXML.Descendants("pfs-image").FirstOrDefault()!.Descendants("root").FirstOrDefault()!.Descendants("file");
                    foreach (XElement PKGPFSImageRootFile in PKGPFSImageRootFiles)
                    {
                        PS5PKGRootFile @init = new();
                        var NewPS5PKGRootFile = (@init.FileSize = PKGPFSImageRootFile.Attribute("size")!.Value, @init.FilePlain = PKGPFSImageRootFile.Attribute("plain")!.Value, @init.FileCompression = PKGPFSImageRootFile.Attribute("comp")!.Value, @init.FileIMode = PKGPFSImageRootFile.Attribute("imode")!.Value, @init.FileIndex = PKGPFSImageRootFile.Attribute("index")!.Value, @init.FileINode = PKGPFSImageRootFile.Attribute("inode")!.Value, @init.FileName = PKGPFSImageRootFile.Attribute("name")!.Value, @init).@init;
                        PFSImageRootFiles.Add(NewPS5PKGRootFile);
                    }
                    // Get the directories in root
                    var PKGPFSImageRootDirectories = PKGConfigurationXML.Descendants("pfs-image").FirstOrDefault()!.Descendants("root").FirstOrDefault()!.Descendants("dir");
                    foreach (XElement PKGPFSImageRootDirectory in PKGPFSImageRootDirectories)
                    {
                        PS5PKGRootDirectory @init1 = new();
                        var NewPS5PKGRootDirectory = (@init1.DirectorySize = PKGPFSImageRootDirectory.Attribute("size")!.Value, @init1.DirectoryLinks = PKGPFSImageRootDirectory.Attribute("links")!.Value, @init1.DirectoryIMode = PKGPFSImageRootDirectory.Attribute("imode")!.Value, @init1.DirectoryIndex = PKGPFSImageRootDirectory.Attribute("index")!.Value, @init1.DirectoryINode = PKGPFSImageRootDirectory.Attribute("inode")!.Value, @init1.DirectoryName = PKGPFSImageRootDirectory.Attribute("name")!.Value, @init1).@init1;
                        PFSImageRootDirectories.Add(NewPS5PKGRootDirectory);
                    }
                    // Get the files in uroot
                    var PKGPFSImageURootFiles = PKGConfigurationXML.Descendants("pfs-image").FirstOrDefault()!.Descendants("root").FirstOrDefault()!.Descendants("dir").FirstOrDefault()!.Descendants("file");
                    foreach (XElement PKGPFSImageURootFile in PKGPFSImageURootFiles)
                    {
                        PS5PKGRootFile @init2 = new();
                        var NewPS5PKGURootFile = (@init2.FileSize = PKGPFSImageURootFile.Attribute("size")!.Value, @init2.FilePlain = PKGPFSImageURootFile.Attribute("plain")!.Value, @init2.FileCompression = PKGPFSImageURootFile.Attribute("comp")!.Value, @init2.FileIMode = PKGPFSImageURootFile.Attribute("imode")!.Value, @init2.FileIndex = PKGPFSImageURootFile.Attribute("index")!.Value, @init2.FileINode = PKGPFSImageURootFile.Attribute("inode")!.Value, @init2.FileName = PKGPFSImageURootFile.Attribute("name")!.Value, @init2).@init2;
                        PFSImageURootFiles.Add(NewPS5PKGURootFile);
                    }

                    // Get the PKG nested image info
                    var PKGNestedImage = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault();
                    if (PKGNestedImage is not null)
                    {
                        string PKGNestedImageVersion = PKGNestedImage.Attribute("version")!.Value;
                        string PKGNestedImageReadOnly = PKGNestedImage.Attribute("readonly")!.Value;
                        string PKGNestedImageOffset = PKGNestedImage.Attribute("offset")!.Value;
                    }
                    // Get the PKG nested image sblock info
                    var PKGNestedImageSBlock = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault()!.Descendants("sblock").FirstOrDefault();
                    if (PKGNestedImageSBlock is not null)
                    {
                        string PKGPFSImageSBlockSigned = PKGNestedImageSBlock.Attribute("signed")!.Value;
                        string PKGPFSImageSBlockEncrypted = PKGNestedImageSBlock.Attribute("encrypted")!.Value;
                        string PKGPFSImageSBlockIgnoreCase = PKGNestedImageSBlock.Attribute("ignore-case")!.Value;
                        string PKGPFSImageSBlockIndexSize = PKGNestedImageSBlock.Attribute("index-size")!.Value;
                        string PKGPFSImageSBlockBlocks = PKGNestedImageSBlock.Attribute("blocks")!.Value;
                        string PKGPFSImageSBlockBackups = PKGNestedImageSBlock.Attribute("backups")!.Value;
                    }
                    var PKGNestedImageSBlockImageSize = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault()!.Descendants("sblock").FirstOrDefault()!.Element("image-size");
                    if (PKGNestedImageSBlockImageSize is not null)
                    {
                        string PKGPFSImageSBlockImageSizeBlockSize = PKGNestedImageSBlockImageSize.Attribute("block-size")!.Value;
                        string PKGPFSImageSBlockImageSizeNum = PKGNestedImageSBlockImageSize.Attribute("num")!.Value;
                        string PKGPFSImageSBlockImageSizeValue = PKGNestedImageSBlockImageSize.Value;
                    }
                    var PKGNestedImageSBlockSuperInode = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault()!.Descendants("sblock").FirstOrDefault()!.Element("super-inode");
                    if (PKGNestedImageSBlockSuperInode is not null)
                    {
                        string PKGPFSImageSBlockSuperInodeBlocks = PKGNestedImageSBlockSuperInode.Attribute("blocks")!.Value;
                        string PKGPFSImageSBlockSuperInodeInodes = PKGNestedImageSBlockSuperInode.Attribute("inodes")!.Value;
                        string PKGPFSImageSBlockSuperInodeRoot = PKGNestedImageSBlockSuperInode.Attribute("root")!.Value;
                    }
                    var PKGNestedImageSBlockInode = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault()!.Descendants("sblock").FirstOrDefault()!.Descendants("super-inode").FirstOrDefault()!.Element("inode");
                    if (PKGNestedImageSBlockInode is not null)
                    {
                        string PKGPFSImageSBlockInodeSize = PKGNestedImageSBlockInode.Attribute("size")!.Value;
                        string PKGPFSImageSBlockInodeLinks = PKGNestedImageSBlockInode.Attribute("links")!.Value;
                        string PKGPFSImageSBlockInodeMode = PKGNestedImageSBlockInode.Attribute("mode")!.Value;
                        string PKGPFSImageSBlockInodeIMode = PKGNestedImageSBlockInode.Attribute("imode")!.Value;
                        string PKGPFSImageSBlockInodeIndex = PKGNestedImageSBlockInode.Attribute("index")!.Value;
                    }
                    // Get the PKG nested image metadata
                    var PKGNestedImageMetadata = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault()!.Descendants("metadata").FirstOrDefault();
                    if (PKGNestedImageMetadata is not null)
                    {
                        string PKGNestedImageMetadataSize = PKGNestedImageMetadata.Attribute("size")!.Value;
                        string PKGNestedImageMetadataPlain = PKGNestedImageMetadata.Attribute("plain")!.Value;
                        string PKGNestedImageMetadataCompression = PKGNestedImageMetadata.Attribute("comp")!.Value;
                        string PKGNestedImageMetadataOffset = PKGNestedImageMetadata.Attribute("offset")!.Value;
                        string PKGNestedImageMetadataPOffset = PKGNestedImageMetadata.Attribute("poffset")!.Value;
                        string PKGNestedImageMetadataAfid = PKGNestedImageMetadata.Attribute("afid")!.Value;
                    }

                    // Get the PKG nested image root info
                    var PKGNestedImageRoot = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault()!.Element("root");
                    if (PKGNestedImageRoot is not null)
                    {
                        string PKGPFSImageRootSize = PKGNestedImageRoot.Attribute("size")!.Value;
                        string PKGPFSImageRootLinks = PKGNestedImageRoot.Attribute("links")!.Value;
                        string PKGPFSImageRootIMode = PKGNestedImageRoot.Attribute("imode")!.Value;
                        string PKGPFSImageRootIndex = PKGNestedImageRoot.Attribute("index")!.Value;
                        string PKGPFSImageRootINode = PKGNestedImageRoot.Attribute("inode")!.Value;
                        string PKGPFSImageRootName = PKGNestedImageRoot.Attribute("name")!.Value;
                    }
                    // Get the files in root
                    var PKGNestedImageRootFiles = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault()!.Descendants("root").FirstOrDefault()!.Descendants("file");
                    foreach (XElement PKGNestedImageRootFile in PKGNestedImageRootFiles)
                    {
                        PS5PKGRootFile @init3 = new();
                        var NewPS5PKGRootFile = (@init3.FileSize = PKGNestedImageRootFile.Attribute("size")!.Value, @init3.FilePlain = PKGNestedImageRootFile.Attribute("plain")!.Value, @init3.FileCompression = PKGNestedImageRootFile.Attribute("comp")!.Value, @init3.FileIMode = PKGNestedImageRootFile.Attribute("imode")!.Value, @init3.FileIndex = PKGNestedImageRootFile.Attribute("index")!.Value, @init3.FileINode = PKGNestedImageRootFile.Attribute("inode")!.Value, @init3.FileName = PKGNestedImageRootFile.Attribute("name")!.Value, @init3).@init3;
                        NestedImageRootFiles.Add(NewPS5PKGRootFile);
                    }
                    // Get the directories in root
                    var PKGNestedImageRootDirectories = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault()!.Descendants("root").FirstOrDefault()!.Descendants("dir");
                    foreach (XElement PKGNestedImageRootDirectory in PKGNestedImageRootDirectories)
                    {
                        PS5PKGRootDirectory @init4 = new();
                        var NewPS5PKGRootDirectory = (@init4.DirectorySize = PKGNestedImageRootDirectory.Attribute("size")!.Value, @init4.DirectoryLinks = PKGNestedImageRootDirectory.Attribute("links")!.Value, @init4.DirectoryIMode = PKGNestedImageRootDirectory.Attribute("imode")!.Value, @init4.DirectoryIndex = PKGNestedImageRootDirectory.Attribute("index")!.Value, @init4.DirectoryINode = PKGNestedImageRootDirectory.Attribute("inode")!.Value, @init4.DirectoryName = PKGNestedImageRootDirectory.Attribute("name")!.Value, @init4).@init4;
                        NestedImageRootDirectories.Add(NewPS5PKGRootDirectory);
                    }
                    // Get the files in uroot
                    var PKGNestedImageURootFiles = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault()!.Descendants("root").FirstOrDefault()!.Descendants("dir").FirstOrDefault()!.Descendants("file");
                    foreach (XElement PKGNestedImageURootFile in PKGNestedImageURootFiles)
                    {
                        PS5PKGRootFile @init5 = new();
                        var NewPS5PKGURootFile = (@init5.FileSize = PKGNestedImageURootFile.Attribute("size")!.Value, @init5.FilePlain = PKGNestedImageURootFile.Attribute("plain")!.Value, @init5.FileCompression = PKGNestedImageURootFile.Attribute("comp")!.Value, @init5.FileIMode = PKGNestedImageURootFile.Attribute("imode")!.Value, @init5.FileIndex = PKGNestedImageURootFile.Attribute("index")!.Value, @init5.FileINode = PKGNestedImageURootFile.Attribute("inode")!.Value, @init5.FileName = PKGNestedImageURootFile.Attribute("name")!.Value, @init5).@init5;
                        NestedImageURootFiles.Add(NewPS5PKGURootFile);
                    }
                    // Get the directories in uroot
                    var PKGNestedImageURootDirectories = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault()!.Descendants("root").FirstOrDefault()!.Descendants("dir").FirstOrDefault()!.Descendants("dir");
                    foreach (XElement PKGNestedImageURootDirectory in PKGNestedImageURootDirectories)
                    {
                        PS5PKGRootDirectory @init6 = new();
                        var NewPS5PKGRootDirectory = (@init6.DirectorySize = PKGNestedImageURootDirectory.Attribute("size")!.Value, @init6.DirectoryLinks = PKGNestedImageURootDirectory.Attribute("links")!.Value, @init6.DirectoryIMode = PKGNestedImageURootDirectory.Attribute("imode")!.Value, @init6.DirectoryIndex = PKGNestedImageURootDirectory.Attribute("index")!.Value, @init6.DirectoryINode = PKGNestedImageURootDirectory.Attribute("inode")!.Value, @init6.DirectoryName = PKGNestedImageURootDirectory.Attribute("name")!.Value, @init6).@init6;
                        NestedImageRootDirectories.Add(NewPS5PKGRootDirectory);
                    }

                    // Extract param.json & icon0.png
                    using var PKGReader = new FileStream(selectedFile[0], FileMode.Open, FileAccess.Read);
                    // Seek from the end
                    PKGReader.Seek(0L, SeekOrigin.End);

                    long ContainerOffsetDecValue = 0L;
                    long EntryOffsetDecValue = 0L;
                    int EntrySizeDecValue = 0;

                    long ParamJSONOffsetPosition = 0L;
                    long Icon0OffsetPosition = 0L;
                    long Pic0OffsetPosition = 0L;

                    long PKGFileLength = PKGReader.Length;

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
                            var ParamData = JsonConvert.DeserializeObject<PS5Param>(Encoding.UTF8.GetString(ParamFileBuffer));
                            var NewPS5Game = new PS5Game() { GameBackupType = PS5Game.BackupType.LocalPKG };

                            if (ParamData is not null)
                            {
                                if (ParamData.TitleId is not null)
                                {
                                    NewPS5Game.GameID = "Title ID: " + ParamData.TitleId;
                                    NewPS5Game.GameRegion = "Region: " + PS5Game.GetGameRegion(ParamData.TitleId);
                                }

                                if (ParamData.LocalizedParameters!.EnUS is not null)
                                {
                                    NewPS5Game.GameTitle = ParamData.LocalizedParameters.EnUS.TitleName;
                                }
                                if (ParamData.LocalizedParameters.DeDE is not null)
                                {
                                    NewPS5Game.DEGameTitle = ParamData.LocalizedParameters.DeDE.TitleName;
                                }
                                if (ParamData.LocalizedParameters.FrFR is not null)
                                {
                                    NewPS5Game.FRGameTitle = ParamData.LocalizedParameters.FrFR.TitleName;
                                }
                                if (ParamData.LocalizedParameters.ItIT is not null)
                                {
                                    NewPS5Game.ITGameTitle = ParamData.LocalizedParameters.ItIT.TitleName;
                                }
                                if (ParamData.LocalizedParameters.EsES is not null)
                                {
                                    NewPS5Game.ESGameTitle = ParamData.LocalizedParameters.EsES.TitleName;
                                }
                                if (ParamData.LocalizedParameters.JaJP is not null)
                                {
                                    NewPS5Game.JPGameTitle = ParamData.LocalizedParameters.JaJP.TitleName;
                                }

                                if (ParamData.ContentId is not null)
                                {
                                    NewPS5Game.GameContentID = "Content ID: " + ParamData.ContentId;
                                }

                                if (ParamData.ApplicationCategoryType == 0)
                                {
                                    NewPS5Game.GameCategory = "Type: PS5 Game";
                                }
                                else if (ParamData.ApplicationCategoryType == 65792)
                                {
                                    NewPS5Game.GameCategory = "Type: RNPS Media App";
                                }
                                else if (ParamData.ApplicationCategoryType == 131328)
                                {
                                    NewPS5Game.GameCategory = "Type: System Built-in App";
                                }
                                else if (ParamData.ApplicationCategoryType == 131584)
                                {
                                    NewPS5Game.GameCategory = "Type: Big Daemon";
                                }
                                else if (ParamData.ApplicationCategoryType == 16777216)
                                {
                                    NewPS5Game.GameCategory = "Type: ShellUI";
                                }
                                else if (ParamData.ApplicationCategoryType == 33554432)
                                {
                                    NewPS5Game.GameCategory = "Type: Daemon";
                                }
                                else if (ParamData.ApplicationCategoryType == 67108864)
                                {
                                    NewPS5Game.GameCategory = "Type: ShellApp";
                                }

                                long PS5GameSize = PKGFileLength;
                                NewPS5Game.GameSize = $"Size: {Utils.HumanReadableBytes(PS5GameSize)}";

                                if (ParamData.ContentVersion is not null)
                                {
                                    NewPS5Game.GameVersion = "Version: " + ParamData.ContentVersion;
                                }
                                if (ParamData.RequiredSystemSoftwareVersion is not null)
                                {
                                    NewPS5Game.GameRequiredFirmware = "Required Firmware: " + ParamData.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, ".").Insert(5, ".").Insert(8, ".").Remove(11, 8);
                                }

                                GameTitleTextBlock.IsVisible = true;
                                GameIDTextBlock.IsVisible = true;
                                GameRegionTextBlock.IsVisible = true;
                                GameVersionTextBlock.IsVisible = true;
                                GameContentIDTextBlock.IsVisible = true;
                                GameCategoryTextBlock.IsVisible = true;
                                GameSizeTextBlock.IsVisible = true;
                                GameRequiredFirmwareTextBlock.IsVisible = true;

                                GameTitleTextBlock.Text = NewPS5Game.GameTitle;
                                GameIDTextBlock.Text = NewPS5Game.GameID;
                                GameRegionTextBlock.Text = NewPS5Game.GameRegion;
                                GameVersionTextBlock.Text = NewPS5Game.GameVersion;
                                GameContentIDTextBlock.Text = NewPS5Game.GameContentID;
                                GameCategoryTextBlock.Text = NewPS5Game.GameCategory;
                                GameSizeTextBlock.Text = NewPS5Game.GameSize;
                                GameRequiredFirmwareTextBlock.Text = NewPS5Game.GameRequiredFirmware;
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
                            }
                            PKGIconImage.Source = Icon0BitmapImage;
                            CurrentIcon0 = Icon0BitmapImage;
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
                            }
                            CurrentPic0 = Pic0BitmapImage;
                        }
                    }

                    PKGReader.Close();
                }
            }
        }
    }

    private void ShowPKGPFSImageFilesButton_Click(object sender, RoutedEventArgs e)
    {
        CurrentListViewTitleTextBlock.Text = "PKG PFS Image Files :";

        HideListBoxes();
        PKGImageFilesListBox.IsVisible = true;
        PKGImageFilesListBox.Items.Clear();

        if (PFSImageRootFiles is not null && PFSImageRootFiles.Count > 0)
        {
            foreach (var PFSImageRootFile in PFSImageRootFiles)
                PKGImageFilesListBox.Items.Add(PFSImageRootFile);
        }
    }

    private void ShowPKGPFSImageDirectoriesButton_Click(object sender, RoutedEventArgs e)
    {
        CurrentListViewTitleTextBlock.Text = "PKG PFS Image Directories :";

        HideListBoxes();
        PKGImageDirectoriesListBox.IsVisible = true;
        PKGImageDirectoriesListBox.Items.Clear();

        if (PFSImageRootDirectories is not null && PFSImageRootDirectories.Count > 0)
        {
            foreach (var PFSImageRootDirectory in PFSImageRootDirectories)
                PKGImageDirectoriesListBox.Items.Add(PFSImageRootDirectory);
        }
    }

    private void ShowPKGNestedImageFilesButton_Click(object sender, RoutedEventArgs e)
    {
        CurrentListViewTitleTextBlock.Text = "PKG Nested Image Files :";

        HideListBoxes();
        PKGImageFilesListBox.IsVisible = true;
        PKGImageFilesListBox.Items.Clear();

        if (NestedImageRootFiles is not null && NestedImageRootFiles.Count > 0)
        {
            foreach (var NestedImageRootFile in NestedImageRootFiles)
                PKGImageFilesListBox.Items.Add(NestedImageRootFile);
        }
    }

    private void ShowPKGNestedImageDirectoriesButton_Click(object sender, RoutedEventArgs e)
    {
        CurrentListViewTitleTextBlock.Text = "PKG Nested Image Directories :";

        HideListBoxes();
        PKGImageDirectoriesListBox.IsVisible = true;
        PKGImageDirectoriesListBox.Items.Clear();

        if (NestedImageRootDirectories is not null && NestedImageRootDirectories.Count > 0)
        {
            foreach (var NestedImageRootDirectory in NestedImageRootDirectories)
                PKGImageDirectoriesListBox.Items.Add(NestedImageRootDirectory);
        }
    }

    private void ShowPKGScenariosButton_Click(object sender, RoutedEventArgs e)
    {
        CurrentListViewTitleTextBlock.Text = "PKG Scenarios :";

        HideListBoxes();
        PKGScenariosListBox.IsVisible = true;
    }

    private void ShowPKGChunksButton_Click(object sender, RoutedEventArgs e)
    {
        CurrentListViewTitleTextBlock.Text = "PKG Chunks :";

        HideListBoxes();
        PKGChunksListBox.IsVisible = true;
    }

    private void ShowPKGOutersButton_Click(object sender, RoutedEventArgs e)
    {
        CurrentListViewTitleTextBlock.Text = "PKG Outers :";

        HideListBoxes();
        PKGOutersListBox.IsVisible = true;
    }

    private void ShowPKGEntriesButton_Click(object sender, RoutedEventArgs e)
    {
        CurrentListViewTitleTextBlock.Text = "PKG Entries :";

        HideListBoxes();
        PKGContentListBox.IsVisible = true;
    }

    private async void ExportConfigurationXMLButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentConfigurationXML is not null)
        {
            if (VisualRoot is not Window window)
                return;

            var newSaveFileDialog = new SaveFileDialog() { Title = "Select a save path", Filters = [new FileDialogFilter() { Name = "XML files", Extensions = { "xml" } }] };
            var selectedSaveFile = await newSaveFileDialog.ShowAsync(window);
            if (selectedSaveFile != null)
            {
                CurrentConfigurationXML.Save(selectedSaveFile);
            }
        }
    }

    private async void ExportParamJSONButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentParamJSON))
        {
            if (VisualRoot is not Window window)
                return;

            var newSaveFileDialog = new SaveFileDialog() { Title = "Select a save path", Filters = [new FileDialogFilter() { Name = "JSON files", Extensions = { "json" } }] };
            var selectedSaveFile = await newSaveFileDialog.ShowAsync(window);
            if (selectedSaveFile != null)
            {
                File.WriteAllText(selectedSaveFile, CurrentParamJSON);
            }
        }
    }

    private async void ExportIcon0PNGButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentIcon0 is not null)
        {
            if (VisualRoot is not Window window)
                return;

            var newSaveFileDialog = new SaveFileDialog() { Title = "Select a save path", Filters = [new FileDialogFilter() { Name = "PNG files", Extensions = { "png" } }] };
            var selectedSaveFile = await newSaveFileDialog.ShowAsync(window);
            if (selectedSaveFile != null)
            {
                CurrentIcon0.Save(selectedSaveFile);
            }
        }
    }

    private async void ExportPic0Button_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPic0 is not null)
        {
            if (VisualRoot is not Window window)
                return;

            var newSaveFileDialog = new SaveFileDialog() { Title = "Select a save path", Filters = [new FileDialogFilter() { Name = "PNG files", Extensions = { "png" } }] };
            var selectedSaveFile = await newSaveFileDialog.ShowAsync(window);
            if (selectedSaveFile != null)
            {
                CurrentPic0.Save(selectedSaveFile);
            }
        }
    }

    private void HideListBoxes()
    {
        PKGContentListBox.IsVisible = false;
        PKGScenariosListBox.IsVisible = false;
        PKGChunksListBox.IsVisible = false;
        PKGOutersListBox.IsVisible = false;
        PKGImageFilesListBox.IsVisible = false;
        PKGImageDirectoriesListBox.IsVisible = false;
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

    private static string? GetAlternativeGameTitle(PS5Param ParamFile)
    {
        var Languages = ParamFile.LocalizedParameters;
        if (Languages == null) return null;

        if (Languages.EnGB != null) return Languages.EnGB.TitleName;
        if (Languages.JaJP != null) return Languages.JaJP.TitleName;
        if (Languages.KoKR != null) return Languages.KoKR.TitleName;
        if (Languages.ZhHant != null) return Languages.ZhHant.TitleName;
        if (Languages.ZhHans != null) return Languages.ZhHans.TitleName;
        if (Languages.ArAE != null) return Languages.ArAE.TitleName;

        return null;
    }

    #region Structures & Classes

    public struct PS5PKGEntry
    {
        public string EntryOffset { get; set; }

        public string EntrySize { get; set; }

        public string EntryName { get; set; }
    }

    public struct PS5PKGScenario
    {
        public string ScenarioID { get; set; }

        public string ScenarioType { get; set; }

        public string ScenarioName { get; set; }
    }

    public struct PS5PKGChunk
    {
        public string ChunkID { get; set; }

        public string ChunkFlag { get; set; }

        public string ChunkLocus { get; set; }

        public string ChunkLanguage { get; set; }

        public string ChunkDisps { get; set; }

        public string ChunkNum { get; set; }

        public string ChunkSize { get; set; }

        public string ChunkName { get; set; }

        public string ChunkValue { get; set; }
    }

    public struct PS5PKGOuter
    {
        public string OuterID { get; set; }

        public string OuterImage { get; set; }

        public string OuterOffset { get; set; }

        public string OuterSize { get; set; }

        public string OuterChunks { get; set; }
    }

    public struct PS5PKGRootDirectory
    {
        public string DirectorySize { get; set; }

        public string DirectoryLinks { get; set; }

        public string DirectoryIMode { get; set; }

        public string DirectoryIndex { get; set; }

        public string DirectoryINode { get; set; }

        public string DirectoryName { get; set; }
    }

    public struct PS5PKGRootFile
    {
        public string FileSize { get; set; }

        public string FilePlain { get; set; }

        public string FileCompression { get; set; }

        public string FileIMode { get; set; }

        public string FileIndex { get; set; }

        public string FileINode { get; set; }

        public string FileName { get; set; }
    }

    #endregion

}