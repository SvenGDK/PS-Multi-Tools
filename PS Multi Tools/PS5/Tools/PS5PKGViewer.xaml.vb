Imports Newtonsoft.Json
Imports System.IO
Imports System.Text

Public Class PS5PKGViewer

    Dim PFSImageRootFiles As New List(Of PS5PKGRootFile)()
    Dim PFSImageRootDirectories As New List(Of PS5PKGRootDirectory)()
    Dim PFSImageURootFiles As New List(Of PS5PKGRootFile)()
    Dim NestedImageRootFiles As New List(Of PS5PKGRootFile)()
    Dim NestedImageRootDirectories As New List(Of PS5PKGRootDirectory)()
    Dim NestedImageURootFiles As New List(Of PS5PKGRootFile)()

    Dim IsSourcePKG As Boolean = False
    Dim IsRetailPKG As Boolean = False

    Dim CurrentParamJSON As String = ""
    Dim CurrentConfigurationXML As XDocument = Nothing
    Dim CurrentIcon0 As BitmapImage = Nothing
    Dim CurrentPic0 As BitmapImage = Nothing

    Private Sub BrowsePKGFileButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePKGFileButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Filter = "PKG files (*.pkg)|*.pkg"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then

            SelectedPKGFileTextBox.Text = OFD.FileName

            'Clear previous ListView items and lists
            PKGContentListView.Items.Clear()
            PKGScenariosListView.Items.Clear()
            PKGChunksListView.Items.Clear()
            PKGOutersListView.Items.Clear()
            PFSImageRootFiles.Clear()
            PFSImageRootDirectories.Clear()
            PFSImageURootFiles.Clear()
            NestedImageRootFiles.Clear()
            NestedImageRootDirectories.Clear()
            NestedImageURootFiles.Clear()

            'Reset
            PKGIconImage.Source = Nothing
            IsSourcePKG = False
            IsRetailPKG = False
            CurrentParamJSON = ""
            CurrentConfigurationXML = Nothing
            CurrentIcon0 = Nothing
            CurrentPic0 = Nothing

            'Determine PS5 PKG
            Dim FirstString As String = ""
            Dim Int8AtOffset5 As SByte
            Using PKGReader As New FileStream(OFD.FileName, FileMode.Open, FileAccess.Read)
                Dim BinReader As New BinaryReader(PKGReader)
                FirstString = BinReader.ReadString()
                PKGReader.Seek(5, SeekOrigin.Begin)
                Int8AtOffset5 = BinReader.ReadSByte()
                PKGReader.Close()
            End Using

            If Not String.IsNullOrEmpty(FirstString) Then
                If FirstString.Contains("CNT") Then
                    IsSourcePKG = True
                Else
                    IsSourcePKG = False
                End If
            End If

            If Int8AtOffset5 = -128 Then
                IsRetailPKG = True
            End If

            If IsRetailPKG Or IsSourcePKG Then
                'Get only param.json
                Dim startBytes As Byte() = Encoding.UTF8.GetBytes("param.json")
                Dim endBytes As Byte() = Encoding.UTF8.GetBytes("version.xml")

                Dim startOffset As Long = -1
                Dim endOffset As Long = -1

                Using PKGReader As New FileStream(OFD.FileName, FileMode.Open, FileAccess.Read)
                    Dim buffer(4096) As Byte
                    Dim fileLength As Long = PKGReader.Length
                    Dim totalBytesRead As Long = fileLength

                    While totalBytesRead > 0
                        Dim bytesRead As Integer = CInt(Math.Min(buffer.Length, totalBytesRead))
                        PKGReader.Seek(totalBytesRead - bytesRead, SeekOrigin.Begin)
                        PKGReader.Read(buffer, 0, bytesRead)
                        totalBytesRead -= bytesRead

                        For i As Integer = bytesRead - 1 To 0 Step -1
                            If endOffset = -1 AndAlso MatchBytes(buffer, i, endBytes) Then
                                endOffset = totalBytesRead + i + endBytes.Length
                            End If

                            If startOffset = -1 AndAlso MatchBytes(buffer, i, startBytes) Then
                                startOffset = totalBytesRead + i
                            End If

                            If startOffset <> -1 AndAlso endOffset <> -1 Then
                                Exit While
                            End If
                        Next
                    End While
                End Using

                If startOffset <> -1 AndAlso endOffset <> -1 AndAlso endOffset > startOffset Then
                    Dim FinalParamJSONString As String = ""
                    Using ParamJSONFileStream As New FileStream(OFD.FileName, FileMode.Open, FileAccess.Read)
                        Dim ParamDataSize As Long = endOffset - startOffset
                        ParamJSONFileStream.Seek(startOffset, SeekOrigin.Begin)

                        Dim NewParamData(CInt(ParamDataSize) - 1) As Byte
                        ParamJSONFileStream.Read(NewParamData, 0, CInt(ParamDataSize))

                        Dim ExtractedData As String = Encoding.UTF8.GetString(NewParamData)
                        Dim ParamJSONData As List(Of String) = ExtractedData.Split(New String() {vbCrLf}, StringSplitOptions.None).ToList()

                        'Adjust the output
                        ParamJSONData.RemoveAt(0)
                        ParamJSONData.Insert(0, "{")
                        ParamJSONData(ParamJSONData.Count - 1) &= """"
                        ParamJSONData.Add("}")

                        FinalParamJSONString = String.Join(Environment.NewLine, ParamJSONData)
                    End Using

                    If Not String.IsNullOrEmpty(FinalParamJSONString) Then
                        CurrentParamJSON = FinalParamJSONString

                        'Display pkg information
                        Dim ParamData As PS5ParamClass.PS5Param = JsonConvert.DeserializeObject(Of PS5ParamClass.PS5Param)(FinalParamJSONString)
                        Dim NewPS5Game As New PS5Game With {.GameBackupType = "PKG"}
                        If ParamData IsNot Nothing Then
                            If ParamData.TitleId IsNot Nothing Then
                                NewPS5Game.GameID = "Title ID: " + ParamData.TitleId
                                NewPS5Game.GameRegion = "Region: " + PS5Game.GetGameRegion(ParamData.TitleId)
                            End If

                            If ParamData.LocalizedParameters.EnUS IsNot Nothing Then
                                NewPS5Game.GameTitle = ParamData.LocalizedParameters.EnUS.TitleName
                            End If
                            If ParamData.LocalizedParameters.DeDE IsNot Nothing Then
                                NewPS5Game.DEGameTitle = ParamData.LocalizedParameters.DeDE.TitleName
                            End If
                            If ParamData.LocalizedParameters.FrFR IsNot Nothing Then
                                NewPS5Game.FRGameTitle = ParamData.LocalizedParameters.FrFR.TitleName
                            End If
                            If ParamData.LocalizedParameters.ItIT IsNot Nothing Then
                                NewPS5Game.ITGameTitle = ParamData.LocalizedParameters.ItIT.TitleName
                            End If
                            If ParamData.LocalizedParameters.EsES IsNot Nothing Then
                                NewPS5Game.ESGameTitle = ParamData.LocalizedParameters.EsES.TitleName
                            End If
                            If ParamData.LocalizedParameters.JaJP IsNot Nothing Then
                                NewPS5Game.JPGameTitle = ParamData.LocalizedParameters.JaJP.TitleName
                            End If

                            If ParamData.ContentId IsNot Nothing Then
                                NewPS5Game.GameContentID = "Content ID: " + ParamData.ContentId
                            End If

                            If ParamData.ApplicationCategoryType = 0 Then
                                NewPS5Game.GameCategory = "Type: PS5 Game"
                            ElseIf ParamData.ApplicationCategoryType = 65792 Then
                                NewPS5Game.GameCategory = "Type: RNPS Media App"
                            ElseIf ParamData.ApplicationCategoryType = 131328 Then
                                NewPS5Game.GameCategory = "Type: System Built-in App"
                            ElseIf ParamData.ApplicationCategoryType = 131584 Then
                                NewPS5Game.GameCategory = "Type: Big Daemon"
                            ElseIf ParamData.ApplicationCategoryType = 16777216 Then
                                NewPS5Game.GameCategory = "Type: ShellUI"
                            ElseIf ParamData.ApplicationCategoryType = 33554432 Then
                                NewPS5Game.GameCategory = "Type: Daemon"
                            ElseIf ParamData.ApplicationCategoryType = 67108864 Then
                                NewPS5Game.GameCategory = "Type: ShellApp"
                            End If

                            NewPS5Game.GameSize = "Size: " + FormatNumber(New FileInfo(OFD.FileName).Length / 1073741824, 2) + " GB"

                            If ParamData.ContentVersion IsNot Nothing Then
                                NewPS5Game.GameVersion = "Version: " + ParamData.ContentVersion
                            End If
                            If ParamData.RequiredSystemSoftwareVersion IsNot Nothing Then
                                NewPS5Game.GameRequiredFirmware = "Required Firmware: " + ParamData.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, "."c).Insert(5, "."c).Insert(8, "."c).Remove(11, 8)
                            End If

                            GameTitleTextBlock.Visibility = Visibility.Visible
                            GameIDTextBlock.Visibility = Visibility.Visible
                            GameRegionTextBlock.Visibility = Visibility.Visible
                            GameVersionTextBlock.Visibility = Visibility.Visible
                            GameContentIDTextBlock.Visibility = Visibility.Visible
                            GameCategoryTextBlock.Visibility = Visibility.Visible
                            GameSizeTextBlock.Visibility = Visibility.Visible
                            GameRequiredFirmwareTextBlock.Visibility = Visibility.Visible

                            GameTitleTextBlock.Text = NewPS5Game.GameTitle
                            GameIDTextBlock.Text = NewPS5Game.GameID
                            GameRegionTextBlock.Text = NewPS5Game.GameRegion
                            GameVersionTextBlock.Text = NewPS5Game.GameVersion
                            GameContentIDTextBlock.Text = NewPS5Game.GameContentID
                            GameCategoryTextBlock.Text = NewPS5Game.GameCategory
                            GameSizeTextBlock.Text = NewPS5Game.GameSize
                            GameRequiredFirmwareTextBlock.Text = NewPS5Game.GameRequiredFirmware
                        End If
                    End If

                End If

                Exit Sub
            End If

            'Probably a self created PKG that contains a package configuration

            Dim ExtractedPKGConfigurationData As String = ""
            Dim PKGConfigurationStartString As String = "<package-configuration version=""1.0"" type=""package-info"">"
            Dim PKGConfigurationEndString As String = "</package-configuration>"

            Dim PKGConfigurationStartBytes As Byte() = Encoding.UTF8.GetBytes(PKGConfigurationStartString)
            Dim PKGConfigurationEndBytes As Byte() = Encoding.UTF8.GetBytes(PKGConfigurationEndString)

            Dim PKGConfigurationStartOffset As Long = -1
            Dim PKGConfigurationEndOffset As Long = -1

            '1. Get the PKG configuration
            Using PKGReader As New FileStream(OFD.FileName, FileMode.Open, FileAccess.Read)
                Dim buffer(4096) As Byte
                Dim fileLength As Long = PKGReader.Length
                Dim totalBytesRead As Long = fileLength

                ' Read backwards to find the end string first
                While totalBytesRead > 0
                    Dim bytesRead As Integer = CInt(Math.Min(buffer.Length, totalBytesRead))
                    PKGReader.Seek(totalBytesRead - bytesRead, SeekOrigin.Begin)
                    PKGReader.Read(buffer, 0, bytesRead)
                    totalBytesRead -= bytesRead

                    ' Check buffer from end to start
                    For i As Integer = bytesRead - 1 To 0 Step -1
                        ' Check for end string
                        If PKGConfigurationEndOffset = -1 AndAlso MatchBytes(buffer, i, PKGConfigurationEndBytes) Then
                            PKGConfigurationEndOffset = totalBytesRead + i + PKGConfigurationEndBytes.Length
                        End If

                        ' Check for start string
                        If PKGConfigurationStartOffset = -1 AndAlso MatchBytes(buffer, i, PKGConfigurationStartBytes) Then
                            PKGConfigurationStartOffset = totalBytesRead + i
                        End If

                        If PKGConfigurationStartOffset <> -1 AndAlso PKGConfigurationEndOffset <> -1 Then
                            Exit While
                        End If
                    Next
                End While

                If PKGConfigurationStartOffset <> -1 And PKGConfigurationEndOffset <> -1 AndAlso PKGConfigurationEndOffset > PKGConfigurationStartOffset Then
                    PKGConfigurationStartOffset -= PKGConfigurationStartBytes.Length
                    PKGConfigurationEndOffset -= PKGConfigurationEndBytes.Length - 1

                    ' Calculate the size of the pkg configuration data
                    Dim PKGConfigurationDataSize As Long = PKGConfigurationEndOffset - PKGConfigurationStartOffset

                    ' Move to the start offset
                    PKGReader.Seek(PKGConfigurationStartOffset, SeekOrigin.Begin)

                    ' Read the pkg configuration data
                    Dim data(CInt(PKGConfigurationDataSize) - 1) As Byte
                    PKGReader.Read(data, 0, CInt(PKGConfigurationDataSize))

                    ' Convert the data to a XML string
                    ExtractedPKGConfigurationData = Encoding.UTF8.GetString(data)
                    ExtractedPKGConfigurationData = String.Concat("<?xml version=""1.0"" encoding=""utf-8""?>", ExtractedPKGConfigurationData)
                End If
            End Using

            '2. Process the retrieved PKG configuration data
            If Not String.IsNullOrEmpty(ExtractedPKGConfigurationData) Then
                'Load the XML file
                Dim PKGConfigurationXML As XDocument = XDocument.Parse(ExtractedPKGConfigurationData)
                CurrentConfigurationXML = PKGConfigurationXML

                'Get the PKG config values
                Dim PKGConfig As XElement = PKGConfigurationXML.Element("config")
                If PKGConfig IsNot Nothing Then
                    Dim PKGConfigVersion As String = PKGConfig.Attribute("version")?.Value
                    Dim PKGConfigMetadata As String = PKGConfig.Attribute("metadata")?.Value
                    Dim PKGConfigPrimary As String = PKGConfig.Attribute("primary")?.Value
                End If
                Dim PKGConfigContentID As String = PKGConfigurationXML.Descendants("config").First().Element("content-id").Value
                Dim PKGConfigPrimaryID As String = PKGConfigurationXML.Descendants("config").First().Element("primary-id").Value
                Dim PKGConfigLongName As String = PKGConfigurationXML.Descendants("config").First().Element("longname").Value
                Dim PKGConfigRequiredSystemVersion As String = PKGConfigurationXML.Descendants("config").First().Element("required-system-version").Value
                Dim PKGConfigDRMType As String = PKGConfigurationXML.Descendants("config").First().Element("drm-type").Value
                Dim PKGConfigContentType As String = PKGConfigurationXML.Descendants("config").First().Element("content-type").Value
                Dim PKGConfigApplicationType As String = PKGConfigurationXML.Descendants("config").First().Element("application-type").Value
                Dim PKGConfigNumberOfImages As String = PKGConfigurationXML.Descendants("config").First().Element("num-of-images").Value
                Dim PKGConfigSize As String = PKGConfigurationXML.Descendants("config").First().Element("package-size").Value
                Dim PKGConfigVersionDate As String = PKGConfigurationXML.Descendants("config").First().Element("version-date").Value
                Dim PKGConfigVersionHash As String = PKGConfigurationXML.Descendants("config").First().Element("version-hash").Value

                'Get the PKG digests
                Dim PKGDigests As XElement = PKGConfigurationXML.Element("digests")
                If PKGDigests IsNot Nothing Then
                    Dim PKGDigestsVersion As String = PKGDigests.Attribute("version")?.Value
                    Dim PKGDigestsMajorParamVersion As String = PKGDigests.Attribute("major-param-version")?.Value
                End If
                Dim PKGContentDigest As String = PKGConfigurationXML.Descendants("digests").First().Element("content-digest").Value
                Dim PKGGameDigest As String = PKGConfigurationXML.Descendants("digests").First().Element("game-digest").Value
                Dim PKGHeaderDigest As String = PKGConfigurationXML.Descendants("digests").First().Element("header-digest").Value
                Dim PKGSystemDigest As String = PKGConfigurationXML.Descendants("digests").First().Element("system-digest").Value
                Dim PKGParamDigest As String = PKGConfigurationXML.Descendants("digests").First().Element("param-digest").Value
                Dim PKGDigest As String = PKGConfigurationXML.Descendants("digests").First().Element("package-digest").Value

                'Get the PKG params
                Dim PKGParamApplicationDRMType As String = PKGConfigurationXML.Descendants("params").First().Element("applicationDrmType").Value
                Dim PKGParamContentID As String = PKGConfigurationXML.Descendants("params").First().Element("contentId").Value
                Dim PKGParamContentVersion As String = PKGConfigurationXML.Descendants("params").First().Element("contentVersion").Value
                Dim PKGParamMasterVersion As String = PKGConfigurationXML.Descendants("params").First().Element("masterVersion").Value
                Dim PKGParamRequiredSystemVersion As String = PKGConfigurationXML.Descendants("params").First().Element("requiredSystemSoftwareVersion").Value
                Dim PKGParamSDKVersion As String = PKGConfigurationXML.Descendants("params").First().Element("sdkVersion").Value
                Dim PKGParamTitleName As String = PKGConfigurationXML.Descendants("params").First().Element("titleName").Value

                'Get the PKG container information
                Dim PKGContainerSize As String = PKGConfigurationXML.Descendants("container").First().Element("container-size").Value
                Dim PKGContainerMandatorySize As String = PKGConfigurationXML.Descendants("container").First().Element("mandatory-size").Value
                Dim PKGContainerBodyOffset As String = PKGConfigurationXML.Descendants("container").First().Element("body-offset").Value
                Dim PKGContainerBodySize As String = PKGConfigurationXML.Descendants("container").First().Element("body-size").Value
                Dim PKGContainerBodyDigest As String = PKGConfigurationXML.Descendants("container").First().Element("body-digest").Value
                Dim PKGContainerPromoteSize As String = PKGConfigurationXML.Descendants("container").First().Element("promote-size").Value

                'Get the PKG mount image
                Dim PKGMountImagePFSOffsetAlign As String = PKGConfigurationXML.Descendants("mount-image").First().Element("pfs-offset-align").Value
                Dim PKGMountImagePFSSizeAlign As String = PKGConfigurationXML.Descendants("mount-image").First().Element("pfs-size-align").Value
                Dim PKGMountImagePFSImageOffset As String = PKGConfigurationXML.Descendants("mount-image").First().Element("pfs-image-offset").Value
                Dim PKGMountImagePFSImageSize As String = PKGConfigurationXML.Descendants("mount-image").First().Element("pfs-image-size").Value
                Dim PKGMountImageFixedInfoSize As String = PKGConfigurationXML.Descendants("mount-image").First().Element("fixed-info-size").Value
                Dim PKGMountImagePFSImageSeed As String = PKGConfigurationXML.Descendants("mount-image").First().Element("pfs-image-seed").Value
                Dim PKGMountImageSBlockDigest As String = PKGConfigurationXML.Descendants("mount-image").First().Element("sblock-digest").Value
                Dim PKGMountImageFixedInfoDigest As String = PKGConfigurationXML.Descendants("mount-image").First().Element("fixed-info-digest").Value
                Dim PKGMountImageOffset As String = PKGConfigurationXML.Descendants("mount-image").First().Element("mount-image-offset").Value
                Dim PKGMountImageSize As String = PKGConfigurationXML.Descendants("mount-image").First().Element("mount-image-size").Value
                Dim PKGMountImageContainerOffset As String = PKGConfigurationXML.Descendants("mount-image").First().Element("container-offset").Value
                Dim PKGMountImageSupplementalOffset As String = PKGConfigurationXML.Descendants("mount-image").First().Element("supplemental-offset").Value

                'Get the PKG entries and add to PKGContentListView
                Dim PKGEntries As IEnumerable(Of XElement) = PKGConfigurationXML.Descendants("entries").Descendants("entry")
                For Each PKGEntry As XElement In PKGEntries
                    Dim NewPS5PKGEntry As New PS5PKGEntry() With {.EntryOffset = PKGEntry.Attribute("offset").Value, .EntrySize = PKGEntry.Attribute("size").Value, .EntryName = PKGEntry.Attribute("name").Value}
                    PKGContentListView.Items.Add(NewPS5PKGEntry)
                Next
                PKGContentListView.Items.Refresh()

                'Get the PKG chunkinfo
                Dim PKGChunkInfo As XElement = PKGConfigurationXML.Element("chunkinfo")
                If PKGChunkInfo IsNot Nothing Then
                    Dim PKGChunkInfoSize As String = PKGChunkInfo.Attribute("size")?.Value
                    Dim PKGChunkInfoNested As String = PKGChunkInfo.Attribute("nested")?.Value
                    Dim PKGChunkInfoSDK As String = PKGChunkInfo.Attribute("sdk")?.Value
                    Dim PKGChunkInfoDisps As String = PKGChunkInfo.Attribute("disps")?.Value
                End If
                Dim PKGChunkInfoContentID As String = PKGConfigurationXML.Descendants("chunkinfo").First().Element("contentid").Value
                Dim PKGChunkInfoLanguages As String = PKGConfigurationXML.Descendants("chunkinfo").First().Element("languages").Value

                'Get the PKG chunkinfo scenarios
                Dim PKGChunkInfoScenarios As IEnumerable(Of XElement) = PKGConfigurationXML.Descendants("chunkinfo").Descendants("scenarios").Descendants("scenario")
                For Each PKGChunkInfoScenario As XElement In PKGChunkInfoScenarios
                    Dim NewPS5PKGChunkInfoScenario As New PS5PKGScenario() With {
                        .ScenarioID = PKGChunkInfoScenario.Attribute("id").Value,
                        .ScenarioType = PKGChunkInfoScenario.Attribute("type").Value,
                        .ScenarioName = PKGChunkInfoScenario.Attribute("name").Value}
                    PKGScenariosListView.Items.Add(NewPS5PKGChunkInfoScenario)
                Next
                PKGScenariosListView.Items.Refresh()

                'Get the PKG chunkinfo chunks
                Dim PKGChunkInfoChunks As XElement = PKGConfigurationXML.Element("chunks")
                If PKGChunkInfoChunks IsNot Nothing Then
                    Dim PKGChunkInfoChunksNum As String = PKGChunkInfoChunks.Attribute("num")?.Value
                    Dim PKGChunkInfoChunksDefault As String = PKGChunkInfoChunks.Attribute("default")?.Value
                End If
                Dim PKGChunkInfoChunksList As IEnumerable(Of XElement) = PKGConfigurationXML.Descendants("chunkinfo").Descendants("chunks").Descendants("chunk")
                For Each PKGChunkInfoChunk As XElement In PKGChunkInfoChunksList
                    Dim NewPS5PKGChunkInfoChunk As New PS5PKGChunk() With {
                        .ChunkID = PKGChunkInfoChunk.Attribute("id").Value,
                        .ChunkFlag = PKGChunkInfoChunk.Attribute("flag").Value,
                        .ChunkLocus = PKGChunkInfoChunk.Attribute("locus").Value,
                        .ChunkLanguage = PKGChunkInfoChunk.Attribute("language").Value,
                        .ChunkDisps = PKGChunkInfoChunk.Attribute("disps").Value,
                        .ChunkNum = PKGChunkInfoChunk.Attribute("num").Value,
                        .ChunkSize = PKGChunkInfoChunk.Attribute("size").Value,
                        .ChunkName = PKGChunkInfoChunk.Attribute("name").Value,
                        .ChunkValue = PKGChunkInfoChunk.Value}
                    PKGChunksListView.Items.Add(NewPS5PKGChunkInfoChunk)
                Next
                PKGChunksListView.Items.Refresh()

                'Get the PKG chunkinfo outers
                Dim PKGChunkInfoOuters As XElement = PKGConfigurationXML.Element("outers")
                If PKGChunkInfoOuters IsNot Nothing Then
                    Dim PKGChunkInfoOutersNum As String = PKGChunkInfoOuters.Attribute("num")?.Value
                    Dim PKGChunkInfoOutersOverlapped As String = PKGChunkInfoOuters.Attribute("overlapped")?.Value
                    Dim PKGChunkInfoOutersLanguageOverlapped As String = PKGChunkInfoOuters.Attribute("language-overlapped")?.Value
                End If
                Dim PKGChunkInfoOutersList As IEnumerable(Of XElement) = PKGConfigurationXML.Descendants("chunkinfo").Descendants("outers").Descendants("outer")
                For Each PKGChunkInfoOuter As XElement In PKGChunkInfoOutersList
                    Dim NewPS5PKGOuter As New PS5PKGOuter() With {
                        .OuterID = PKGChunkInfoOuter.Attribute("id").Value,
                        .OuterImage = PKGChunkInfoOuter.Attribute("image").Value,
                        .OuterOffset = PKGChunkInfoOuter.Attribute("offset").Value,
                        .OuterSize = PKGChunkInfoOuter.Attribute("size").Value,
                        .OuterChunks = PKGChunkInfoOuter.Attribute("chunks").Value}
                    PKGOutersListView.Items.Add(NewPS5PKGOuter)
                Next
                PKGOutersListView.Items.Refresh()

                'Get the PKG pfs image info
                Dim PKGPFSImage As XElement = PKGConfigurationXML.Element("pfs-image")
                If PKGPFSImage IsNot Nothing Then
                    Dim PKGPFSImageVersion As String = PKGPFSImage.Attribute("version")?.Value
                    Dim PKGPFSImageReadOnly As String = PKGPFSImage.Attribute("readonly")?.Value
                    Dim PKGPFSImageOffset As String = PKGPFSImage.Attribute("offset")?.Value
                    Dim PKGPFSImageMetadata As String = PKGPFSImage.Attribute("metadata")?.Value
                End If

                'Get the PKG pfs image sblock info
                Dim PKGPFSImageSBlock As XElement = PKGConfigurationXML.Descendants("sblock").FirstOrDefault()
                If PKGPFSImageSBlock IsNot Nothing Then
                    Dim PKGPFSImageSBlockSigned As String = PKGPFSImageSBlock.Attribute("signed")?.Value
                    Dim PKGPFSImageSBlockEncrypted As String = PKGPFSImageSBlock.Attribute("encrypted")?.Value
                    Dim PKGPFSImageSBlockIgnoreCase As String = PKGPFSImageSBlock.Attribute("ignore-case")?.Value
                    Dim PKGPFSImageSBlockIndexSize As String = PKGPFSImageSBlock.Attribute("index-size")?.Value
                    Dim PKGPFSImageSBlockBlocks As String = PKGPFSImageSBlock.Attribute("blocks")?.Value
                    Dim PKGPFSImageSBlockBackups As String = PKGPFSImageSBlock.Attribute("backups")?.Value
                End If
                Dim PKGPFSImageSBlockImageSize As XElement = PKGConfigurationXML.Descendants("sblock").FirstOrDefault().Element("image-size")
                If PKGPFSImageSBlockImageSize IsNot Nothing Then
                    Dim PKGPFSImageSBlockImageSizeBlockSize As String = PKGPFSImageSBlockImageSize.Attribute("block-size")?.Value
                    Dim PKGPFSImageSBlockImageSizeNum As String = PKGPFSImageSBlockImageSize.Attribute("num")?.Value
                    Dim PKGPFSImageSBlockImageSizeValue As String = PKGPFSImageSBlockImageSize.Value
                End If
                Dim PKGPFSImageSBlockSuperInode As XElement = PKGConfigurationXML.Descendants("sblock").FirstOrDefault().Element("super-inode")
                If PKGPFSImageSBlockSuperInode IsNot Nothing Then
                    Dim PKGPFSImageSBlockSuperInodeBlocks As String = PKGPFSImageSBlockSuperInode.Attribute("blocks")?.Value
                    Dim PKGPFSImageSBlockSuperInodeInodes As String = PKGPFSImageSBlockSuperInode.Attribute("inodes")?.Value
                    Dim PKGPFSImageSBlockSuperInodeRoot As String = PKGPFSImageSBlockSuperInode.Attribute("root")?.Value
                End If
                Dim PKGPFSImageSBlockInode As XElement = PKGConfigurationXML.Descendants("sblock").FirstOrDefault().Descendants("super-inode").FirstOrDefault().Element("inode")
                If PKGPFSImageSBlockInode IsNot Nothing Then
                    Dim PKGPFSImageSBlockInodeSize As String = PKGPFSImageSBlockInode.Attribute("size")?.Value
                    Dim PKGPFSImageSBlockInodeLinks As String = PKGPFSImageSBlockInode.Attribute("links")?.Value
                    Dim PKGPFSImageSBlockInodeMode As String = PKGPFSImageSBlockInode.Attribute("mode")?.Value
                    Dim PKGPFSImageSBlockInodeIMode As String = PKGPFSImageSBlockInode.Attribute("imode")?.Value
                    Dim PKGPFSImageSBlockInodeIndex As String = PKGPFSImageSBlockInode.Attribute("index")?.Value
                End If
                Dim PKGPFSImageSBlockSeed As String = PKGConfigurationXML.Descendants("sblock").FirstOrDefault().Element("seed").Value
                Dim PKGPFSImageSBlockICV As String = PKGConfigurationXML.Descendants("sblock").FirstOrDefault().Element("icv").Value

                'Get the PKG pfs image root info
                Dim PKGPFSImageRoot As XElement = PKGConfigurationXML.Descendants("pfs-image").FirstOrDefault().Element("root")
                If PKGPFSImageRoot IsNot Nothing Then
                    Dim PKGPFSImageRootSize As String = PKGPFSImageRoot.Attribute("size")?.Value
                    Dim PKGPFSImageRootLinks As String = PKGPFSImageRoot.Attribute("links")?.Value
                    Dim PKGPFSImageRootIMode As String = PKGPFSImageRoot.Attribute("imode")?.Value
                    Dim PKGPFSImageRootIndex As String = PKGPFSImageRoot.Attribute("index")?.Value
                    Dim PKGPFSImageRootINode As String = PKGPFSImageRoot.Attribute("inode")?.Value
                    Dim PKGPFSImageRootName As String = PKGPFSImageRoot.Attribute("name")?.Value
                End If
                'Get the files in root
                Dim PKGPFSImageRootFiles As IEnumerable(Of XElement) = PKGConfigurationXML.Descendants("pfs-image").FirstOrDefault().Descendants("root").FirstOrDefault().Descendants("file")
                For Each PKGPFSImageRootFile As XElement In PKGPFSImageRootFiles
                    Dim NewPS5PKGRootFile As New PS5PKGRootFile() With {
                        .FileSize = PKGPFSImageRootFile.Attribute("size")?.Value,
                        .FilePlain = PKGPFSImageRootFile.Attribute("plain")?.Value,
                        .FileCompression = PKGPFSImageRootFile.Attribute("comp")?.Value,
                        .FileIMode = PKGPFSImageRootFile.Attribute("imode")?.Value,
                        .FileIndex = PKGPFSImageRootFile.Attribute("index")?.Value,
                        .FileINode = PKGPFSImageRootFile.Attribute("inode")?.Value,
                        .FileName = PKGPFSImageRootFile.Attribute("name")?.Value}
                    PFSImageRootFiles.Add(NewPS5PKGRootFile)
                Next
                'Get the directories in root
                Dim PKGPFSImageRootDirectories As IEnumerable(Of XElement) = PKGConfigurationXML.Descendants("pfs-image").FirstOrDefault().Descendants("root").FirstOrDefault().Descendants("dir")
                For Each PKGPFSImageRootDirectory As XElement In PKGPFSImageRootDirectories
                    Dim NewPS5PKGRootDirectory As New PS5PKGRootDirectory() With {
                        .DirectorySize = PKGPFSImageRootDirectory.Attribute("size")?.Value,
                        .DirectoryLinks = PKGPFSImageRootDirectory.Attribute("links")?.Value,
                        .DirectoryIMode = PKGPFSImageRootDirectory.Attribute("imode")?.Value,
                        .DirectoryIndex = PKGPFSImageRootDirectory.Attribute("index")?.Value,
                        .DirectoryINode = PKGPFSImageRootDirectory.Attribute("inode")?.Value,
                        .DirectoryName = PKGPFSImageRootDirectory.Attribute("name")?.Value}
                    PFSImageRootDirectories.Add(NewPS5PKGRootDirectory)
                Next
                'Get the files in uroot
                Dim PKGPFSImageURootFiles As IEnumerable(Of XElement) = PKGConfigurationXML.Descendants("pfs-image").FirstOrDefault().Descendants("root").FirstOrDefault().Descendants("dir").FirstOrDefault().Descendants("file")
                For Each PKGPFSImageURootFile As XElement In PKGPFSImageURootFiles
                    Dim NewPS5PKGURootFile As New PS5PKGRootFile() With {
                        .FileSize = PKGPFSImageURootFile.Attribute("size")?.Value,
                        .FilePlain = PKGPFSImageURootFile.Attribute("plain")?.Value,
                        .FileCompression = PKGPFSImageURootFile.Attribute("comp")?.Value,
                        .FileIMode = PKGPFSImageURootFile.Attribute("imode")?.Value,
                        .FileIndex = PKGPFSImageURootFile.Attribute("index")?.Value,
                        .FileINode = PKGPFSImageURootFile.Attribute("inode")?.Value,
                        .FileName = PKGPFSImageURootFile.Attribute("name")?.Value}
                    PFSImageURootFiles.Add(NewPS5PKGURootFile)
                Next

                'Get the PKG nested image info
                Dim PKGNestedImage As XElement = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault()
                If PKGNestedImage IsNot Nothing Then
                    Dim PKGNestedImageVersion As String = PKGNestedImage.Attribute("version")?.Value
                    Dim PKGNestedImageReadOnly As String = PKGNestedImage.Attribute("readonly")?.Value
                    Dim PKGNestedImageOffset As String = PKGNestedImage.Attribute("offset")?.Value
                End If
                'Get the PKG nested image sblock info
                Dim PKGNestedImageSBlock As XElement = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault().Descendants("sblock").FirstOrDefault()
                If PKGNestedImageSBlock IsNot Nothing Then
                    Dim PKGPFSImageSBlockSigned As String = PKGNestedImageSBlock.Attribute("signed")?.Value
                    Dim PKGPFSImageSBlockEncrypted As String = PKGNestedImageSBlock.Attribute("encrypted")?.Value
                    Dim PKGPFSImageSBlockIgnoreCase As String = PKGNestedImageSBlock.Attribute("ignore-case")?.Value
                    Dim PKGPFSImageSBlockIndexSize As String = PKGNestedImageSBlock.Attribute("index-size")?.Value
                    Dim PKGPFSImageSBlockBlocks As String = PKGNestedImageSBlock.Attribute("blocks")?.Value
                    Dim PKGPFSImageSBlockBackups As String = PKGNestedImageSBlock.Attribute("backups")?.Value
                End If
                Dim PKGNestedImageSBlockImageSize As XElement = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault().Descendants("sblock").FirstOrDefault().Element("image-size")
                If PKGNestedImageSBlockImageSize IsNot Nothing Then
                    Dim PKGPFSImageSBlockImageSizeBlockSize As String = PKGNestedImageSBlockImageSize.Attribute("block-size")?.Value
                    Dim PKGPFSImageSBlockImageSizeNum As String = PKGNestedImageSBlockImageSize.Attribute("num")?.Value
                    Dim PKGPFSImageSBlockImageSizeValue As String = PKGNestedImageSBlockImageSize.Value
                End If
                Dim PKGNestedImageSBlockSuperInode As XElement = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault().Descendants("sblock").FirstOrDefault().Element("super-inode")
                If PKGNestedImageSBlockSuperInode IsNot Nothing Then
                    Dim PKGPFSImageSBlockSuperInodeBlocks As String = PKGNestedImageSBlockSuperInode.Attribute("blocks")?.Value
                    Dim PKGPFSImageSBlockSuperInodeInodes As String = PKGNestedImageSBlockSuperInode.Attribute("inodes")?.Value
                    Dim PKGPFSImageSBlockSuperInodeRoot As String = PKGNestedImageSBlockSuperInode.Attribute("root")?.Value
                End If
                Dim PKGNestedImageSBlockInode As XElement = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault().Descendants("sblock").FirstOrDefault().Descendants("super-inode").FirstOrDefault().Element("inode")
                If PKGNestedImageSBlockInode IsNot Nothing Then
                    Dim PKGPFSImageSBlockInodeSize As String = PKGNestedImageSBlockInode.Attribute("size")?.Value
                    Dim PKGPFSImageSBlockInodeLinks As String = PKGNestedImageSBlockInode.Attribute("links")?.Value
                    Dim PKGPFSImageSBlockInodeMode As String = PKGNestedImageSBlockInode.Attribute("mode")?.Value
                    Dim PKGPFSImageSBlockInodeIMode As String = PKGNestedImageSBlockInode.Attribute("imode")?.Value
                    Dim PKGPFSImageSBlockInodeIndex As String = PKGNestedImageSBlockInode.Attribute("index")?.Value
                End If
                'Get the PKG nested image metadata
                Dim PKGNestedImageMetadata As XElement = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault().Descendants("metadata").FirstOrDefault()
                If PKGNestedImageMetadata IsNot Nothing Then
                    Dim PKGNestedImageMetadataSize As String = PKGNestedImageMetadata.Attribute("size")?.Value
                    Dim PKGNestedImageMetadataPlain As String = PKGNestedImageMetadata.Attribute("plain")?.Value
                    Dim PKGNestedImageMetadataCompression As String = PKGNestedImageMetadata.Attribute("comp")?.Value
                    Dim PKGNestedImageMetadataOffset As String = PKGNestedImageMetadata.Attribute("offset")?.Value
                    Dim PKGNestedImageMetadataPOffset As String = PKGNestedImageMetadata.Attribute("poffset")?.Value
                    Dim PKGNestedImageMetadataAfid As String = PKGNestedImageMetadata.Attribute("afid")?.Value
                End If

                'Get the PKG nested image root info
                Dim PKGNestedImageRoot As XElement = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault().Element("root")
                If PKGNestedImageRoot IsNot Nothing Then
                    Dim PKGPFSImageRootSize As String = PKGNestedImageRoot.Attribute("size")?.Value
                    Dim PKGPFSImageRootLinks As String = PKGNestedImageRoot.Attribute("links")?.Value
                    Dim PKGPFSImageRootIMode As String = PKGNestedImageRoot.Attribute("imode")?.Value
                    Dim PKGPFSImageRootIndex As String = PKGNestedImageRoot.Attribute("index")?.Value
                    Dim PKGPFSImageRootINode As String = PKGNestedImageRoot.Attribute("inode")?.Value
                    Dim PKGPFSImageRootName As String = PKGNestedImageRoot.Attribute("name")?.Value
                End If
                'Get the files in root
                Dim PKGNestedImageRootFiles As IEnumerable(Of XElement) = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault().Descendants("root").FirstOrDefault().Descendants("file")
                For Each PKGNestedImageRootFile As XElement In PKGNestedImageRootFiles
                    Dim NewPS5PKGRootFile As New PS5PKGRootFile() With {
                        .FileSize = PKGNestedImageRootFile.Attribute("size")?.Value,
                        .FilePlain = PKGNestedImageRootFile.Attribute("plain")?.Value,
                        .FileCompression = PKGNestedImageRootFile.Attribute("comp")?.Value,
                        .FileIMode = PKGNestedImageRootFile.Attribute("imode")?.Value,
                        .FileIndex = PKGNestedImageRootFile.Attribute("index")?.Value,
                        .FileINode = PKGNestedImageRootFile.Attribute("inode")?.Value,
                        .FileName = PKGNestedImageRootFile.Attribute("name")?.Value}
                    NestedImageRootFiles.Add(NewPS5PKGRootFile)
                Next
                'Get the directories in root
                Dim PKGNestedImageRootDirectories As IEnumerable(Of XElement) = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault().Descendants("root").FirstOrDefault().Descendants("dir")
                For Each PKGNestedImageRootDirectory As XElement In PKGNestedImageRootDirectories
                    Dim NewPS5PKGRootDirectory As New PS5PKGRootDirectory() With {
                        .DirectorySize = PKGNestedImageRootDirectory.Attribute("size")?.Value,
                        .DirectoryLinks = PKGNestedImageRootDirectory.Attribute("links")?.Value,
                        .DirectoryIMode = PKGNestedImageRootDirectory.Attribute("imode")?.Value,
                        .DirectoryIndex = PKGNestedImageRootDirectory.Attribute("index")?.Value,
                        .DirectoryINode = PKGNestedImageRootDirectory.Attribute("inode")?.Value,
                        .DirectoryName = PKGNestedImageRootDirectory.Attribute("name")?.Value}
                    NestedImageRootDirectories.Add(NewPS5PKGRootDirectory)
                Next
                'Get the files in uroot
                Dim PKGNestedImageURootFiles As IEnumerable(Of XElement) = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault().Descendants("root").FirstOrDefault().Descendants("dir").FirstOrDefault().Descendants("file")
                For Each PKGNestedImageURootFile As XElement In PKGNestedImageURootFiles
                    Dim NewPS5PKGURootFile As New PS5PKGRootFile() With {
                        .FileSize = PKGNestedImageURootFile.Attribute("size")?.Value,
                        .FilePlain = PKGNestedImageURootFile.Attribute("plain")?.Value,
                        .FileCompression = PKGNestedImageURootFile.Attribute("comp")?.Value,
                        .FileIMode = PKGNestedImageURootFile.Attribute("imode")?.Value,
                        .FileIndex = PKGNestedImageURootFile.Attribute("index")?.Value,
                        .FileINode = PKGNestedImageURootFile.Attribute("inode")?.Value,
                        .FileName = PKGNestedImageURootFile.Attribute("name")?.Value}
                    NestedImageURootFiles.Add(NewPS5PKGURootFile)
                Next
                'Get the directories in uroot
                Dim PKGNestedImageURootDirectories As IEnumerable(Of XElement) = PKGConfigurationXML.Descendants("nested-image").FirstOrDefault().Descendants("root").FirstOrDefault().Descendants("dir").FirstOrDefault().Descendants("dir")
                For Each PKGNestedImageURootDirectory As XElement In PKGNestedImageURootDirectories
                    Dim NewPS5PKGRootDirectory As New PS5PKGRootDirectory() With {
                        .DirectorySize = PKGNestedImageURootDirectory.Attribute("size")?.Value,
                        .DirectoryLinks = PKGNestedImageURootDirectory.Attribute("links")?.Value,
                        .DirectoryIMode = PKGNestedImageURootDirectory.Attribute("imode")?.Value,
                        .DirectoryIndex = PKGNestedImageURootDirectory.Attribute("index")?.Value,
                        .DirectoryINode = PKGNestedImageURootDirectory.Attribute("inode")?.Value,
                        .DirectoryName = PKGNestedImageURootDirectory.Attribute("name")?.Value}
                    NestedImageRootDirectories.Add(NewPS5PKGRootDirectory)
                Next

                ' Extract param.json & icon0.png
                Using PKGReader As New FileStream(OFD.FileName, FileMode.Open, FileAccess.Read)
                    'Seek from the end
                    PKGReader.Seek(0, SeekOrigin.End)

                    Dim ContainerOffsetDecValue As Long = 0
                    Dim EntryOffsetDecValue As Long = 0
                    Dim EntrySizeDecValue As Integer = 0

                    Dim ParamJSONOffsetPosition As Long = 0
                    Dim Icon0OffsetPosition As Long = 0
                    Dim Pic0OffsetPosition As Long = 0

                    Dim PKGFileLength As Long = PKGReader.Length

                    Dim ParamJsonPKGEntry As New PS5PKGEntry()
                    Dim Icon0PKGEntry As New PS5PKGEntry()
                    Dim Pic0PKGEntry As New PS5PKGEntry()

                    'Get the param.json and icon0.png PKG entry info
                    For Each PKGEntry As XElement In PKGConfigurationXML.Descendants("entries").Descendants("entry")
                        If PKGEntry.Attribute("name").Value = "param.json" Then
                            ParamJsonPKGEntry.EntryOffset = PKGEntry.Attribute("offset").Value
                            ParamJsonPKGEntry.EntrySize = PKGEntry.Attribute("size").Value
                            ParamJsonPKGEntry.EntryName = PKGEntry.Attribute("name").Value
                        End If
                        If PKGEntry.Attribute("name").Value = "icon0.png" Then
                            Icon0PKGEntry.EntryOffset = PKGEntry.Attribute("offset").Value
                            Icon0PKGEntry.EntrySize = PKGEntry.Attribute("size").Value
                            Icon0PKGEntry.EntryName = PKGEntry.Attribute("name").Value
                        End If
                        If PKGEntry.Attribute("name").Value = "pic0.png" Then
                            Pic0PKGEntry.EntryOffset = PKGEntry.Attribute("offset").Value
                            Pic0PKGEntry.EntrySize = PKGEntry.Attribute("size").Value
                            Pic0PKGEntry.EntryName = PKGEntry.Attribute("name").Value
                        End If
                    Next

                    'PARAM.JSON
                    If Not String.IsNullOrEmpty(ParamJsonPKGEntry.EntryOffset) AndAlso Not String.IsNullOrEmpty(ParamJsonPKGEntry.EntrySize) Then

                        'Get decimal offset values
                        If Not String.IsNullOrEmpty(PKGMountImageContainerOffset) Then
                            ContainerOffsetDecValue = Convert.ToInt64(PKGMountImageContainerOffset, 16)
                        End If
                        If Not String.IsNullOrEmpty(ParamJsonPKGEntry.EntryOffset) Then
                            EntryOffsetDecValue = Convert.ToInt64(ParamJsonPKGEntry.EntryOffset, 16)
                        End If
                        If Not String.IsNullOrEmpty(ParamJsonPKGEntry.EntrySize) Then
                            EntrySizeDecValue = Convert.ToInt32(ParamJsonPKGEntry.EntrySize, 16)
                        End If
                        ParamJSONOffsetPosition = ContainerOffsetDecValue + EntryOffsetDecValue

                        'Seek to the beginning of the param.json file and read
                        Dim ParamFileBuffer() As Byte = New Byte(EntrySizeDecValue - 1) {}
                        PKGReader.Seek(ParamJSONOffsetPosition, SeekOrigin.Begin)
                        PKGReader.Read(ParamFileBuffer, 0, ParamFileBuffer.Length)

                        If Not String.IsNullOrWhiteSpace(Encoding.UTF8.GetString(ParamFileBuffer)) Then
                            CurrentParamJSON = Encoding.UTF8.GetString(ParamFileBuffer)
                            Dim ParamData = JsonConvert.DeserializeObject(Of PS5ParamClass.PS5Param)(Encoding.UTF8.GetString(ParamFileBuffer))
                            Dim NewPS5Game As New PS5Game With {.GameBackupType = "PKG"}

                            If ParamData IsNot Nothing Then
                                If ParamData.TitleId IsNot Nothing Then
                                    NewPS5Game.GameID = "Title ID: " + ParamData.TitleId
                                    NewPS5Game.GameRegion = "Region: " + PS5Game.GetGameRegion(ParamData.TitleId)
                                End If

                                If ParamData.LocalizedParameters.EnUS IsNot Nothing Then
                                    NewPS5Game.GameTitle = ParamData.LocalizedParameters.EnUS.TitleName
                                End If
                                If ParamData.LocalizedParameters.DeDE IsNot Nothing Then
                                    NewPS5Game.DEGameTitle = ParamData.LocalizedParameters.DeDE.TitleName
                                End If
                                If ParamData.LocalizedParameters.FrFR IsNot Nothing Then
                                    NewPS5Game.FRGameTitle = ParamData.LocalizedParameters.FrFR.TitleName
                                End If
                                If ParamData.LocalizedParameters.ItIT IsNot Nothing Then
                                    NewPS5Game.ITGameTitle = ParamData.LocalizedParameters.ItIT.TitleName
                                End If
                                If ParamData.LocalizedParameters.EsES IsNot Nothing Then
                                    NewPS5Game.ESGameTitle = ParamData.LocalizedParameters.EsES.TitleName
                                End If
                                If ParamData.LocalizedParameters.JaJP IsNot Nothing Then
                                    NewPS5Game.JPGameTitle = ParamData.LocalizedParameters.JaJP.TitleName
                                End If

                                If ParamData.ContentId IsNot Nothing Then
                                    NewPS5Game.GameContentID = "Content ID: " + ParamData.ContentId
                                End If

                                If ParamData.ApplicationCategoryType = 0 Then
                                    NewPS5Game.GameCategory = "Type: PS5 Game"
                                ElseIf ParamData.ApplicationCategoryType = 65792 Then
                                    NewPS5Game.GameCategory = "Type: RNPS Media App"
                                ElseIf ParamData.ApplicationCategoryType = 131328 Then
                                    NewPS5Game.GameCategory = "Type: System Built-in App"
                                ElseIf ParamData.ApplicationCategoryType = 131584 Then
                                    NewPS5Game.GameCategory = "Type: Big Daemon"
                                ElseIf ParamData.ApplicationCategoryType = 16777216 Then
                                    NewPS5Game.GameCategory = "Type: ShellUI"
                                ElseIf ParamData.ApplicationCategoryType = 33554432 Then
                                    NewPS5Game.GameCategory = "Type: Daemon"
                                ElseIf ParamData.ApplicationCategoryType = 67108864 Then
                                    NewPS5Game.GameCategory = "Type: ShellApp"
                                End If

                                NewPS5Game.GameSize = "Size: " + FormatNumber(PKGFileLength / 1073741824, 2) + " GB"

                                If ParamData.ContentVersion IsNot Nothing Then
                                    NewPS5Game.GameVersion = "Version: " + ParamData.ContentVersion
                                End If
                                If ParamData.RequiredSystemSoftwareVersion IsNot Nothing Then
                                    NewPS5Game.GameRequiredFirmware = "Required Firmware: " + ParamData.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, "."c).Insert(5, "."c).Insert(8, "."c).Remove(11, 8)
                                End If

                                GameTitleTextBlock.Visibility = Visibility.Visible
                                GameIDTextBlock.Visibility = Visibility.Visible
                                GameRegionTextBlock.Visibility = Visibility.Visible
                                GameVersionTextBlock.Visibility = Visibility.Visible
                                GameContentIDTextBlock.Visibility = Visibility.Visible
                                GameCategoryTextBlock.Visibility = Visibility.Visible
                                GameSizeTextBlock.Visibility = Visibility.Visible
                                GameRequiredFirmwareTextBlock.Visibility = Visibility.Visible

                                GameTitleTextBlock.Text = NewPS5Game.GameTitle
                                GameIDTextBlock.Text = NewPS5Game.GameID
                                GameRegionTextBlock.Text = NewPS5Game.GameRegion
                                GameVersionTextBlock.Text = NewPS5Game.GameVersion
                                GameContentIDTextBlock.Text = NewPS5Game.GameContentID
                                GameCategoryTextBlock.Text = NewPS5Game.GameCategory
                                GameSizeTextBlock.Text = NewPS5Game.GameSize
                                GameRequiredFirmwareTextBlock.Text = NewPS5Game.GameRequiredFirmware
                            End If
                        End If

                    End If

                    'ICON0.PNG
                    If Not String.IsNullOrEmpty(Icon0PKGEntry.EntryOffset) AndAlso Not String.IsNullOrEmpty(Icon0PKGEntry.EntrySize) Then

                        'Get decimal offset values
                        If Not String.IsNullOrEmpty(PKGMountImageContainerOffset) Then
                            ContainerOffsetDecValue = Convert.ToInt64(PKGMountImageContainerOffset, 16)
                        End If
                        If Not String.IsNullOrEmpty(Icon0PKGEntry.EntryOffset) Then
                            EntryOffsetDecValue = Convert.ToInt64(Icon0PKGEntry.EntryOffset, 16)
                        End If
                        If Not String.IsNullOrEmpty(Icon0PKGEntry.EntrySize) Then
                            EntrySizeDecValue = Convert.ToInt32(Icon0PKGEntry.EntrySize, 16)
                        End If
                        Icon0OffsetPosition = ContainerOffsetDecValue + EntryOffsetDecValue

                        'Seek to the beginning of the icon0.png file and read
                        Dim Icon0FileBuffer() As Byte = New Byte(EntrySizeDecValue - 1) {}
                        PKGReader.Seek(Icon0OffsetPosition, SeekOrigin.Begin)
                        PKGReader.Read(Icon0FileBuffer, 0, Icon0FileBuffer.Length)

                        'Check the buffer and display the icon
                        If Icon0FileBuffer IsNot Nothing Then
                            Dim Icon0BitmapImage As New BitmapImage()
                            Using Icon0MemoryStream As New MemoryStream(Icon0FileBuffer)
                                Icon0BitmapImage.BeginInit()
                                Icon0BitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                Icon0BitmapImage.StreamSource = Icon0MemoryStream
                                Icon0BitmapImage.EndInit()
                            End Using
                            PKGIconImage.Source = Icon0BitmapImage
                            CurrentIcon0 = Icon0BitmapImage
                        End If
                    End If

                    'PIC0.PNG
                    If Not String.IsNullOrEmpty(Pic0PKGEntry.EntryOffset) AndAlso Not String.IsNullOrEmpty(Pic0PKGEntry.EntrySize) Then

                        'Get decimal offset values
                        If Not String.IsNullOrEmpty(PKGMountImageContainerOffset) Then
                            ContainerOffsetDecValue = Convert.ToInt64(PKGMountImageContainerOffset, 16)
                        End If
                        If Not String.IsNullOrEmpty(Pic0PKGEntry.EntryOffset) Then
                            EntryOffsetDecValue = Convert.ToInt64(Pic0PKGEntry.EntryOffset, 16)
                        End If
                        If Not String.IsNullOrEmpty(Pic0PKGEntry.EntrySize) Then
                            EntrySizeDecValue = Convert.ToInt32(Pic0PKGEntry.EntrySize, 16)
                        End If
                        Pic0OffsetPosition = ContainerOffsetDecValue + EntryOffsetDecValue

                        'Seek to the beginning of the icon0.png file and read
                        Dim Pic0FileBuffer() As Byte = New Byte(EntrySizeDecValue - 1) {}
                        PKGReader.Seek(Pic0OffsetPosition, SeekOrigin.Begin)
                        PKGReader.Read(Pic0FileBuffer, 0, Pic0FileBuffer.Length)

                        'Check the buffer and display the icon
                        If Pic0FileBuffer IsNot Nothing Then
                            Dim Pic0BitmapImage As New BitmapImage()
                            Using Pic0MemoryStream As New MemoryStream(Pic0FileBuffer)
                                Pic0BitmapImage.BeginInit()
                                Pic0BitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                Pic0BitmapImage.StreamSource = Pic0MemoryStream
                                Pic0BitmapImage.EndInit()
                            End Using
                            CurrentPic0 = Pic0BitmapImage
                        End If
                    End If

                    PKGReader.Close()
                End Using
            End If
        End If
    End Sub

    Private Function MatchBytes(buffer As Byte(), position As Integer, pattern As Byte()) As Boolean
        If position + 1 < pattern.Length Then
            Return False
        End If

        For i As Integer = 0 To pattern.Length - 1
            If buffer(position - i) <> pattern(pattern.Length - 1 - i) Then
                Return False
            End If
        Next

        Return True
    End Function

    Public Structure PS5PKGEntry
        Private _EntryOffset As String
        Private _EntrySize As String
        Private _EntryName As String

        Public Property EntryOffset As String
            Get
                Return _EntryOffset
            End Get
            Set
                _EntryOffset = Value
            End Set
        End Property

        Public Property EntrySize As String
            Get
                Return _EntrySize
            End Get
            Set
                _EntrySize = Value
            End Set
        End Property

        Public Property EntryName As String
            Get
                Return _EntryName
            End Get
            Set
                _EntryName = Value
            End Set
        End Property
    End Structure

    Public Structure PS5PKGScenario
        Private _ScenarioName As String
        Private _ScenarioType As String
        Private _ScenarioID As String

        Public Property ScenarioID As String
            Get
                Return _ScenarioID
            End Get
            Set
                _ScenarioID = Value
            End Set
        End Property

        Public Property ScenarioType As String
            Get
                Return _ScenarioType
            End Get
            Set
                _ScenarioType = Value
            End Set
        End Property

        Public Property ScenarioName As String
            Get
                Return _ScenarioName
            End Get
            Set
                _ScenarioName = Value
            End Set
        End Property
    End Structure

    Public Structure PS5PKGChunk
        Private _ChunkID As String
        Private _ChunkFlag As String
        Private _ChunkLocus As String
        Private _ChunkName As String
        Private _ChunkSize As String
        Private _ChunkNum As String
        Private _ChunkDisps As String
        Private _ChunkLanguage As String
        Private _ChunkValue As String

        Public Property ChunkID As String
            Get
                Return _ChunkID
            End Get
            Set
                _ChunkID = Value
            End Set
        End Property

        Public Property ChunkFlag As String
            Get
                Return _ChunkFlag
            End Get
            Set
                _ChunkFlag = Value
            End Set
        End Property

        Public Property ChunkLocus As String
            Get
                Return _ChunkLocus
            End Get
            Set
                _ChunkLocus = Value
            End Set
        End Property

        Public Property ChunkLanguage As String
            Get
                Return _ChunkLanguage
            End Get
            Set
                _ChunkLanguage = Value
            End Set
        End Property

        Public Property ChunkDisps As String
            Get
                Return _ChunkDisps
            End Get
            Set
                _ChunkDisps = Value
            End Set
        End Property

        Public Property ChunkNum As String
            Get
                Return _ChunkNum
            End Get
            Set
                _ChunkNum = Value
            End Set
        End Property

        Public Property ChunkSize As String
            Get
                Return _ChunkSize
            End Get
            Set
                _ChunkSize = Value
            End Set
        End Property

        Public Property ChunkName As String
            Get
                Return _ChunkName
            End Get
            Set
                _ChunkName = Value
            End Set
        End Property

        Public Property ChunkValue As String
            Get
                Return _ChunkValue
            End Get
            Set
                _ChunkValue = Value
            End Set
        End Property
    End Structure

    Public Structure PS5PKGOuter
        Private _OuterChunks As String
        Private _OuterSize As String
        Private _OuterOffset As String
        Private _OuterImage As String
        Private _OuterID As String

        Public Property OuterID As String
            Get
                Return _OuterID
            End Get
            Set
                _OuterID = Value
            End Set
        End Property

        Public Property OuterImage As String
            Get
                Return _OuterImage
            End Get
            Set
                _OuterImage = Value
            End Set
        End Property

        Public Property OuterOffset As String
            Get
                Return _OuterOffset
            End Get
            Set
                _OuterOffset = Value
            End Set
        End Property

        Public Property OuterSize As String
            Get
                Return _OuterSize
            End Get
            Set
                _OuterSize = Value
            End Set
        End Property

        Public Property OuterChunks As String
            Get
                Return _OuterChunks
            End Get
            Set
                _OuterChunks = Value
            End Set
        End Property
    End Structure

    Public Structure PS5PKGRootDirectory
        Private _DirectoryName As String
        Private _DirectoryINode As String
        Private _DirectoryIndex As String
        Private _DirectoryIMode As String
        Private _DirectoryLinks As String
        Private _DirectorySize As String

        Public Property DirectorySize As String
            Get
                Return _DirectorySize
            End Get
            Set
                _DirectorySize = Value
            End Set
        End Property

        Public Property DirectoryLinks As String
            Get
                Return _DirectoryLinks
            End Get
            Set
                _DirectoryLinks = Value
            End Set
        End Property

        Public Property DirectoryIMode As String
            Get
                Return _DirectoryIMode
            End Get
            Set
                _DirectoryIMode = Value
            End Set
        End Property

        Public Property DirectoryIndex As String
            Get
                Return _DirectoryIndex
            End Get
            Set
                _DirectoryIndex = Value
            End Set
        End Property

        Public Property DirectoryINode As String
            Get
                Return _DirectoryINode
            End Get
            Set
                _DirectoryINode = Value
            End Set
        End Property

        Public Property DirectoryName As String
            Get
                Return _DirectoryName
            End Get
            Set
                _DirectoryName = Value
            End Set
        End Property
    End Structure

    Public Structure PS5PKGRootFile
        Private _FileName As String
        Private _FileINode As String
        Private _FileIndex As String
        Private _FileIMode As String
        Private _FileCompression As String
        Private _FilePlain As String
        Private _FileSize As String

        Public Property FileSize As String
            Get
                Return _FileSize
            End Get
            Set
                _FileSize = Value
            End Set
        End Property

        Public Property FilePlain As String
            Get
                Return _FilePlain
            End Get
            Set
                _FilePlain = Value
            End Set
        End Property

        Public Property FileCompression As String
            Get
                Return _FileCompression
            End Get
            Set
                _FileCompression = Value
            End Set
        End Property

        Public Property FileIMode As String
            Get
                Return _FileIMode
            End Get
            Set
                _FileIMode = Value
            End Set
        End Property

        Public Property FileIndex As String
            Get
                Return _FileIndex
            End Get
            Set
                _FileIndex = Value
            End Set
        End Property

        Public Property FileINode As String
            Get
                Return _FileINode
            End Get
            Set
                _FileINode = Value
            End Set
        End Property

        Public Property FileName As String
            Get
                Return _FileName
            End Get
            Set
                _FileName = Value
            End Set
        End Property
    End Structure

    Private Sub HideListViews()
        PKGContentListView.Visibility = Visibility.Hidden
        PKGScenariosListView.Visibility = Visibility.Hidden
        PKGChunksListView.Visibility = Visibility.Hidden
        PKGOutersListView.Visibility = Visibility.Hidden
        PKGImageFilesListView.Visibility = Visibility.Hidden
        PKGImageDirectoriesListView.Visibility = Visibility.Hidden
    End Sub

    Private Sub ShowPKGPFSImageFilesButton_Click(sender As Object, e As RoutedEventArgs) Handles ShowPKGPFSImageFilesButton.Click
        CurrentListViewTitleTextBlock.Text = "PKG PFS Image Files :"

        HideListViews()
        PKGImageFilesListView.Visibility = Visibility.Visible
        PKGImageFilesListView.Items.Clear()

        If PFSImageRootFiles IsNot Nothing AndAlso PFSImageRootFiles.Count > 0 Then
            For Each PFSImageRootFile In PFSImageRootFiles
                PKGImageFilesListView.Items.Add(PFSImageRootFile)
            Next
        End If
    End Sub

    Private Sub ShowPKGPFSImageDirectoriesButton_Click(sender As Object, e As RoutedEventArgs) Handles ShowPKGPFSImageDirectoriesButton.Click
        CurrentListViewTitleTextBlock.Text = "PKG PFS Image Directories :"

        HideListViews()
        PKGImageDirectoriesListView.Visibility = Visibility.Visible
        PKGImageDirectoriesListView.Items.Clear()

        If PFSImageRootDirectories IsNot Nothing AndAlso PFSImageRootDirectories.Count > 0 Then
            For Each PFSImageRootDirectory In PFSImageRootDirectories
                PKGImageDirectoriesListView.Items.Add(PFSImageRootDirectory)
            Next
        End If
    End Sub

    Private Sub ShowPKGNestedImageFilesButton_Click(sender As Object, e As RoutedEventArgs) Handles ShowPKGNestedImageFilesButton.Click
        CurrentListViewTitleTextBlock.Text = "PKG Nested Image Files :"

        HideListViews()
        PKGImageFilesListView.Visibility = Visibility.Visible
        PKGImageFilesListView.Items.Clear()

        If NestedImageRootFiles IsNot Nothing AndAlso NestedImageRootFiles.Count > 0 Then
            For Each NestedImageRootFile In NestedImageRootFiles
                PKGImageFilesListView.Items.Add(NestedImageRootFile)
            Next
        End If
    End Sub

    Private Sub ShowPKGNestedImageDirectoriesButton_Click(sender As Object, e As RoutedEventArgs) Handles ShowPKGNestedImageDirectoriesButton.Click
        CurrentListViewTitleTextBlock.Text = "PKG Nested Image Directories :"

        HideListViews()
        PKGImageDirectoriesListView.Visibility = Visibility.Visible
        PKGImageDirectoriesListView.Items.Clear()

        If NestedImageRootDirectories IsNot Nothing AndAlso NestedImageRootDirectories.Count > 0 Then
            For Each NestedImageRootDirectory In NestedImageRootDirectories
                PKGImageDirectoriesListView.Items.Add(NestedImageRootDirectory)
            Next
        End If
    End Sub

    Private Sub ShowPKGScenariosButton_Click(sender As Object, e As RoutedEventArgs) Handles ShowPKGScenariosButton.Click
        CurrentListViewTitleTextBlock.Text = "PKG Scenarios :"

        HideListViews()
        PKGScenariosListView.Visibility = Visibility.Visible
    End Sub

    Private Sub ShowPKGChunksButton_Click(sender As Object, e As RoutedEventArgs) Handles ShowPKGChunksButton.Click
        CurrentListViewTitleTextBlock.Text = "PKG Chunks :"

        HideListViews()
        PKGChunksListView.Visibility = Visibility.Visible
    End Sub

    Private Sub ShowPKGOutersButton_Click(sender As Object, e As RoutedEventArgs) Handles ShowPKGOutersButton.Click
        CurrentListViewTitleTextBlock.Text = "PKG Outers :"

        HideListViews()
        PKGOutersListView.Visibility = Visibility.Visible
    End Sub

    Private Sub ShowPKGEntriesButton_Click(sender As Object, e As RoutedEventArgs) Handles ShowPKGEntriesButton.Click
        CurrentListViewTitleTextBlock.Text = "PKG Entries :"

        HideListViews()
        PKGContentListView.Visibility = Visibility.Visible
    End Sub

    Private Sub ExportConfigurationXMLButton_Click(sender As Object, e As RoutedEventArgs) Handles ExportConfigurationXMLButton.Click
        If CurrentConfigurationXML IsNot Nothing Then
            Dim SFD As New Forms.SaveFileDialog() With {.Title = "Select a save path", .Filter = "XML files (*.xml)|*.xml", .FileName = "package-configuration.xml"}
            If SFD.ShowDialog() = Forms.DialogResult.OK Then
                CurrentConfigurationXML.Save(SFD.FileName)
            End If
        End If
    End Sub

    Private Sub ExportParamJSONButton_Click(sender As Object, e As RoutedEventArgs) Handles ExportParamJSONButton.Click
        If Not String.IsNullOrEmpty(CurrentParamJSON) Then
            Dim SFD As New Forms.SaveFileDialog() With {.Title = "Select a save path", .Filter = "JSON files (*.json)|*.json", .FileName = "param.json"}
            If SFD.ShowDialog() = Forms.DialogResult.OK Then
                File.WriteAllText(SFD.FileName, CurrentParamJSON)
            End If
        End If
    End Sub

    Private Sub ExportIcon0PNGButton_Click(sender As Object, e As RoutedEventArgs) Handles ExportIcon0PNGButton.Click
        If CurrentIcon0 IsNot Nothing Then
            Dim SFD As New Forms.SaveFileDialog() With {.Title = "Select a save path", .Filter = "PNG files (*.png)|*.png", .FileName = "icon0.png"}
            If SFD.ShowDialog() = Forms.DialogResult.OK Then
                Dim NewPngBitmapEncoder As New PngBitmapEncoder()
                NewPngBitmapEncoder.Frames.Add(BitmapFrame.Create(CurrentIcon0))

                Using Icon0FileStream As New FileStream(SFD.FileName, FileMode.Create)
                    NewPngBitmapEncoder.Save(Icon0FileStream)
                End Using
            End If
        End If
    End Sub

    Private Sub ExportPic0Button_Click(sender As Object, e As RoutedEventArgs) Handles ExportPic0Button.Click
        If CurrentPic0 IsNot Nothing Then
            Dim SFD As New Forms.SaveFileDialog() With {.Title = "Select a save path", .Filter = "PNG files (*.png)|*.png", .FileName = "pic0.png"}
            If SFD.ShowDialog() = Forms.DialogResult.OK Then
                Dim NewPngBitmapEncoder As New PngBitmapEncoder()
                NewPngBitmapEncoder.Frames.Add(BitmapFrame.Create(CurrentPic0))

                Using Pic0FileStream As New FileStream(SFD.FileName, FileMode.Create)
                    NewPngBitmapEncoder.Save(Pic0FileStream)
                End Using
            End If
        End If
    End Sub

End Class
