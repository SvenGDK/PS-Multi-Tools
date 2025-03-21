Imports System.ComponentModel
Imports System.IO
Imports PS4_Tools

Public Class PKGInfo

    Public Console As String = String.Empty
    Public SelectedPKG As String = String.Empty

    Dim WithEvents PKGWorker As New BackgroundWorker()

    Dim PKGSoundBytes As Byte() = Nothing
    Dim IsSoundPlaying As Boolean = False

    Private Sub PKGInfo_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Not String.IsNullOrEmpty(SelectedPKG) AndAlso Not String.IsNullOrEmpty(Console) Then
            PKGWorker.RunWorkerAsync()
        End If
    End Sub

    Private Sub PlayStopButton_Click(sender As Object, e As RoutedEventArgs) Handles PlayStopButton.Click
        If PlayStopButton.Content.ToString = "Play Soundtrack" AndAlso PKGSoundBytes IsNot Nothing Then
            PlayStopButton.Content = "Stop Soundtrack"
            IsSoundPlaying = True
            Utils.PlaySND(PKGSoundBytes)
        Else
            PlayStopButton.Content = "Play Soundtrack"
            IsSoundPlaying = False
            Utils.StopSND()
        End If
    End Sub

    Private Sub PKGWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles PKGWorker.DoWork
        Select Case Console
            Case "PS3"
                LoadPS3Info()
            Case "PS4"
                LoadPS4Info()
            Case "PSV"
                LoadPSVInfo()
        End Select
    End Sub

    Private Sub LoadPS4Info()

        Dim GamePKG As PKG.SceneRelated.Unprotected_PKG = PKG.SceneRelated.Read_PKG(SelectedPKG)

        If Not String.IsNullOrEmpty(GamePKG.BuildDate) Then
            PKGBuildDateTextBlock.Dispatcher.BeginInvoke(Sub() PKGBuildDateTextBlock.Text = GamePKG.BuildDate)
        End If

        If Not String.IsNullOrEmpty(GamePKG.Firmware_Version) Then
            PKGFirmwareVersionTextBlock.Dispatcher.BeginInvoke(Sub() PKGFirmwareVersionTextBlock.Text = GamePKG.Firmware_Version)
        End If

        If Not String.IsNullOrEmpty(GamePKG.Size) Then
            PKGSizeTextBlock.Dispatcher.BeginInvoke(Sub() PKGSizeTextBlock.Text = GamePKG.Size)
        End If

        If Not String.IsNullOrEmpty(GamePKG.Content_ID) Then
            PKGContentIDTextBlock.Dispatcher.BeginInvoke(Sub() PKGContentIDTextBlock.Text = GamePKG.Content_ID)
        Else
            If Not String.IsNullOrEmpty(GamePKG.Param.ContentID) Then
                PKGContentIDTextBlock.Dispatcher.BeginInvoke(Sub() PKGContentIDTextBlock.Text = GamePKG.Param.ContentID)
            End If
        End If

        If Not String.IsNullOrEmpty(GamePKG.Region) Then
            PKGRegionTextBlock.Dispatcher.BeginInvoke(Sub() PKGRegionTextBlock.Text = GamePKG.Region)
        End If

        If GamePKG.Sound IsNot Nothing AndAlso GamePKG.Sound.Length > 0 Then
            Dispatcher.BeginInvoke(Sub() PKGSoundBytes = Media.Atrac9.LoadAt9(GamePKG.Sound))
            PlayStopButton.Dispatcher.BeginInvoke(Sub() PlayStopButton.IsEnabled = True)
        End If
        If GamePKG.Background IsNot Nothing AndAlso GamePKG.Background.Length > 0 Then
            Dispatcher.BeginInvoke(Sub() Background = New ImageBrush(Utils.BitmapSourceFromByteArray(GamePKG.Background)))
        End If
        If GamePKG.Icon IsNot Nothing AndAlso GamePKG.Icon.Length > 0 Then
            GameIcon.Dispatcher.BeginInvoke(Sub() GameIcon.Source = Utils.BitmapSourceFromByteArray(GamePKG.Icon))
        End If
        If GamePKG.Image IsNot Nothing AndAlso GamePKG.Image.Length > 0 Then
            GameImage.Dispatcher.BeginInvoke(Sub() GameImage.Source = Utils.BitmapSourceFromByteArray(GamePKG.Image))
        End If

        Select Case GamePKG.PKGState
            Case PKG.SceneRelated.PKG_State.Official
                PKGStateTextBlock.Dispatcher.BeginInvoke(Sub() PKGStateTextBlock.Text = "Official")
            Case PKG.SceneRelated.PKG_State.Officail_DP
                PKGStateTextBlock.Dispatcher.BeginInvoke(Sub() PKGStateTextBlock.Text = "Official DP")
            Case PKG.SceneRelated.PKG_State.Fake
                PKGStateTextBlock.Dispatcher.BeginInvoke(Sub() PKGStateTextBlock.Text = "Fake")
            Case PKG.SceneRelated.PKG_State.Unkown
                PKGStateTextBlock.Dispatcher.BeginInvoke(Sub() PKGStateTextBlock.Text = "Unknown")
            Case Else
                PKGStateTextBlock.Dispatcher.BeginInvoke(Sub() PKGStateTextBlock.Text = "Unknown")
        End Select

        Select Case GamePKG.PKG_Type
            Case PKG.SceneRelated.PKGType.Addon_Theme
                PKGTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGTypeTextBlock.Text = "Theme")
            Case PKG.SceneRelated.PKGType.App
                PKGTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGTypeTextBlock.Text = "Application")
            Case PKG.SceneRelated.PKGType.Game
                PKGTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGTypeTextBlock.Text = "Game")
            Case PKG.SceneRelated.PKGType.Patch
                PKGTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGTypeTextBlock.Text = "Patch")
            Case PKG.SceneRelated.PKGType.Unknown
                PKGTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGTypeTextBlock.Text = "Unknown")
            Case Else
                PKGTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGTypeTextBlock.Text = "Unknown")
        End Select

        Select Case GamePKG.Param.PlaystationVersion
            Case Param_SFO.PARAM_SFO.Playstation.ps4
                PKGConsoleTextBlock.Dispatcher.BeginInvoke(Sub() PKGConsoleTextBlock.Text = "PS4")
            Case Param_SFO.PARAM_SFO.Playstation.psp
                PKGConsoleTextBlock.Dispatcher.BeginInvoke(Sub() PKGConsoleTextBlock.Text = "PSP")
            Case Param_SFO.PARAM_SFO.Playstation.unknown
                PKGConsoleTextBlock.Dispatcher.BeginInvoke(Sub() PKGConsoleTextBlock.Text = "Unknown")
        End Select

        PKGAttributesTextBlock.Dispatcher.BeginInvoke(Sub() PKGAttributesTextBlock.Text = GamePKG.Param.Attribute)
        PKGAppVerTextBlock.Dispatcher.BeginInvoke(Sub() PKGAppVerTextBlock.Text = GamePKG.Param.APP_VER)
        PKGCategoryTextBlock.Dispatcher.BeginInvoke(Sub() PKGCategoryTextBlock.Text = GetPS4Category(GamePKG.Param.Category))
        PKGTitleTextBlock.Dispatcher.BeginInvoke(Sub() PKGTitleTextBlock.Text = GamePKG.Param.Title)

        Select Case GamePKG.Param.DataType
            Case Param_SFO.PARAM_SFO.DataTypes.DiscGame
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Disc Game")
            Case Param_SFO.PARAM_SFO.DataTypes.Additional_Content
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Additional Content")
            Case Param_SFO.PARAM_SFO.DataTypes.AppleTV
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "AppleTV")
            Case Param_SFO.PARAM_SFO.DataTypes.AppMusic
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Music App")
            Case Param_SFO.PARAM_SFO.DataTypes.AppPhoto
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Photo App")
            Case Param_SFO.PARAM_SFO.DataTypes.AppVideo
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Video App")
            Case Param_SFO.PARAM_SFO.DataTypes.AutoInstallRoot
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "AutoInstall Root")
            Case Param_SFO.PARAM_SFO.DataTypes.Blu_Ray_Disc
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Blu Ray Disc")
            Case Param_SFO.PARAM_SFO.DataTypes.BroadCastVideo
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Broadcast Video")
            Case Param_SFO.PARAM_SFO.DataTypes.CellBE
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "CellBE")
            Case Param_SFO.PARAM_SFO.DataTypes.DiscGame
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Disc Game")
            Case Param_SFO.PARAM_SFO.DataTypes.DiscMovie
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Disc Movie")
            Case Param_SFO.PARAM_SFO.DataTypes.DiscPackage
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Disc Package")
            Case Param_SFO.PARAM_SFO.DataTypes.ExtraRoot
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Extra Root")
            Case Param_SFO.PARAM_SFO.DataTypes.GameContent
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Game Content")
            Case Param_SFO.PARAM_SFO.DataTypes.GameData
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Game Data")
            Case Param_SFO.PARAM_SFO.DataTypes.Game_Digital_Application
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Game Digital Application")
            Case Param_SFO.PARAM_SFO.DataTypes.GDE
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "GDE")
            Case Param_SFO.PARAM_SFO.DataTypes.HDDGame
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "HDD Game")
            Case Param_SFO.PARAM_SFO.DataTypes.Home
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Home")
            Case Param_SFO.PARAM_SFO.DataTypes.None
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "None")
            Case Param_SFO.PARAM_SFO.DataTypes.PS4_Game_Application_Patch
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Game Application Patch")
            Case Param_SFO.PARAM_SFO.DataTypes.PSN_Game
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "PSN Game")
            Case Param_SFO.PARAM_SFO.DataTypes.SaveData
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Save Data")
            Case Param_SFO.PARAM_SFO.DataTypes.StoreFronted
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Store Fronted")
            Case Param_SFO.PARAM_SFO.DataTypes.ThemeRoot
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Theme Root")
            Case Param_SFO.PARAM_SFO.DataTypes.VideoRoot
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Video Root")
            Case Param_SFO.PARAM_SFO.DataTypes.WebTV
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "WebTV")
            Case Else
                PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Unknown")
        End Select

        For Each TableEntry As Param_SFO.PARAM_SFO.Table In GamePKG.Param.Tables.ToList()
            If TableEntry.Name = "TITLE_ID" Then
                PKGTitleDTextBlock.Dispatcher.BeginInvoke(Sub() PKGTitleDTextBlock.Text = TableEntry.Value.Trim())
            End If
            If TableEntry.Name = "VERSION" Then
                PKGVersionTextBlock.Dispatcher.BeginInvoke(Sub() PKGVersionTextBlock.Text = TableEntry.Value.Trim())
            End If
        Next

    End Sub

    Private Sub LoadPS3Info()

        Dim PKGFileInfo As New FileInfo(SelectedPKG)
        Dim NewPKGDecryptor As New PKGDecryptor()

        NewPKGDecryptor.ProcessPKGFile(SelectedPKG)

        PKGConsoleTextBlock.Dispatcher.BeginInvoke(Sub() PKGConsoleTextBlock.Text = "PS3")
        PKGSizeTextBlock.Dispatcher.BeginInvoke(Sub() PKGSizeTextBlock.Text = FormatNumber(PKGFileInfo.Length / 1073741824, 2) + " GB")

        If NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.ICON0) IsNot Nothing Then
            GameImage.Dispatcher.BeginInvoke(Sub() GameImage.Source = NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.ICON0))
        End If
        If NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC1) IsNot Nothing Then
            Dispatcher.BeginInvoke(Sub() Background = New ImageBrush(NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC1)))
        End If
        If NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC2) IsNot Nothing Then
            GameIcon.Dispatcher.BeginInvoke(Sub() GameIcon.Source = NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC2))
        End If
        If NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.SND0) IsNot Nothing Then
            Dispatcher.BeginInvoke(Sub() PKGSoundBytes = NewPKGDecryptor.GetSND())
        End If

        If NewPKGDecryptor.GetPARAMSFO IsNot Nothing Then
            Dim SFOKeys As Dictionary(Of String, Object) = SFONew.ReadSFO(NewPKGDecryptor.GetPARAMSFO)
            If SFOKeys.ContainsKey("TITLE") Then
                PKGTitleTextBlock.Dispatcher.BeginInvoke(Sub() Utils.CleanTitle(SFOKeys("TITLE").ToString))
            End If
            If SFOKeys.ContainsKey("TITLE_ID") Then
                PKGTitleDTextBlock.Dispatcher.BeginInvoke(Sub() PKGTitleDTextBlock.Text = SFOKeys("TITLE_ID").ToString)
                PKGRegionTextBlock.Dispatcher.BeginInvoke(Sub() PKGRegionTextBlock.Text = GetPS3GameRegion(SFOKeys("TITLE_ID").ToString))
            End If
            If SFOKeys.ContainsKey("CATEGORY") Then
                PKGCategoryTextBlock.Dispatcher.BeginInvoke(Sub() PKGCategoryTextBlock.Text = GetPS3Category(SFOKeys("CATEGORY").ToString))
            End If
            If SFOKeys.ContainsKey("CONTENT_ID") Then
                PKGContentIDTextBlock.Dispatcher.BeginInvoke(Sub() PKGContentIDTextBlock.Text = SFOKeys("CONTENT_ID").ToString)
            End If
            If SFOKeys.ContainsKey("APP_TYPE") Then
                PKGTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGTypeTextBlock.Text = SFOKeys("APP_TYPE").ToString)
            End If
            If SFOKeys.ContainsKey("APP_VER") Then
                PKGAppVerTextBlock.Dispatcher.BeginInvoke(Sub() PKGAppVerTextBlock.Text = FormatNumber(SFOKeys("APP_VER").ToString, 2))
            End If
            If SFOKeys.ContainsKey("PS3_SYSTEM_VER") Then
                PKGFirmwareVersionTextBlock.Dispatcher.BeginInvoke(Sub() PKGFirmwareVersionTextBlock.Text = FormatNumber(SFOKeys("PS3_SYSTEM_VER").ToString))
            End If
            If SFOKeys.ContainsKey("VERSION") Then
                PKGVersionTextBlock.Dispatcher.BeginInvoke(Sub() PKGVersionTextBlock.Text = SFOKeys("VERSION").ToString)
            End If
        End If

    End Sub

    Private Sub LoadPSVInfo()
        Dim PKGFileInfo As New FileInfo(SelectedPKG)
        Dim PKGIconURL As String = String.Empty
        Dim PKGTitleID As String = String.Empty
        Dim PKGContentID As String = String.Empty

        PKGConsoleTextBlock.Dispatcher.BeginInvoke(Sub() PKGConsoleTextBlock.Text = "PS Vita")
        PKGSizeTextBlock.Dispatcher.BeginInvoke(Sub() PKGSizeTextBlock.Text = FormatNumber(PKGFileInfo.Length / 1073741824, 2) + " GB")

        Using SFOReader As New Process()
            SFOReader.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\PSN_get_pkg_info.exe"
            SFOReader.StartInfo.Arguments = """" + SelectedPKG + """"
            SFOReader.StartInfo.RedirectStandardOutput = True
            SFOReader.StartInfo.UseShellExecute = False
            SFOReader.StartInfo.CreateNoWindow = True
            SFOReader.Start()

            Dim OutputReader As StreamReader = SFOReader.StandardOutput
            Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

            If ProcessOutput.Count > 0 Then
                For Each Line In ProcessOutput
                    If Line.StartsWith("Title:") Then
                        PKGTitleTextBlock.Dispatcher.BeginInvoke(Sub() PKGTitleTextBlock.Text = Line.Split(":"c)(1).Trim(""""c).Trim())
                    ElseIf Line.StartsWith("Title ID:") Then
                        PKGTitleDTextBlock.Dispatcher.BeginInvoke(Sub() PKGTitleDTextBlock.Text = Line.Split(":"c)(1).Trim(""""c).Trim())
                        PKGTitleID = Line.Split(":"c)(1).Trim(""""c).Trim()
                    ElseIf Line.StartsWith("NPS Type:") Then
                        PKGTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGTypeTextBlock.Text = Line.Split(":"c)(1).Trim(""""c).Trim())
                        PKGCategoryTextBlock.Dispatcher.BeginInvoke(Sub() PKGCategoryTextBlock.Text = Line.Split(":"c)(1).Trim(""""c).Trim())
                    ElseIf Line.StartsWith("App Ver:") Then
                        PKGAppVerTextBlock.Dispatcher.BeginInvoke(Sub() PKGAppVerTextBlock.Text = FormatNumber(Line.Split(":"c)(1).Trim(""""c), 2))
                    ElseIf Line.StartsWith("Min FW:") Then
                        PKGFirmwareVersionTextBlock.Dispatcher.BeginInvoke(Sub() PKGFirmwareVersionTextBlock.Text = FormatNumber(Line.Split(":"c)(1).Trim(""""c), 2))
                    ElseIf Line.StartsWith("Version:") Then
                        PKGVersionTextBlock.Dispatcher.BeginInvoke(Sub() PKGVersionTextBlock.Text = FormatNumber(Line.Split(":"c)(1).Trim(""""c), 2))
                    ElseIf Line.StartsWith("Content ID:") Then
                        PKGContentIDTextBlock.Dispatcher.BeginInvoke(Sub() PKGContentIDTextBlock.Text = Line.Split(":"c)(1).Trim(""""c).Trim())
                        PKGContentID = Line.Split(":"c)(1).Trim(""""c).Trim()
                    ElseIf Line.StartsWith("Region:") Then
                        PKGRegionTextBlock.Dispatcher.BeginInvoke(Sub() PKGRegionTextBlock.Text = Line.Split(":"c)(1).Trim(""""c).Trim())
                    ElseIf Line.StartsWith("c_date:") Then
                        PKGBuildDateTextBlock.Dispatcher.BeginInvoke(Sub() PKGBuildDateTextBlock.Text = Line.Split(":"c)(1).Trim(""""c).Trim())
                    End If
                Next

                If Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + PKGTitleID + ".png") Then
                    GameIcon.Source = New BitmapImage(New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + PKGTitleID + ".png", UriKind.RelativeOrAbsolute))
                End If

            End If
        End Using

        PKGStateTextBlock.Dispatcher.BeginInvoke(Sub() PKGStateTextBlock.Text = "Not available")
        PKGDataTypeTextBlock.Dispatcher.BeginInvoke(Sub() PKGDataTypeTextBlock.Text = "Not available")
        PKGAttributesTextBlock.Dispatcher.BeginInvoke(Sub() PKGAttributesTextBlock.Text = "Not available")
    End Sub

    Public Shared Function GetPS4Category(SFOCategory As String) As String
        Select Case SFOCategory
            Case "ac"
                Return "Additional Content"
            Case "bd"
                Return "Blu-ray Disc"
            Case "gc"
                Return "Game Content"
            Case "gd"
                Return "Game Digital Application"
            Case "gda"
                Return "System Application"
            Case "gdb"
                Return "Unknown"
            Case "gdc"
                Return "Non-Game Big Application"
            Case "gdd"
                Return "BG Application"
            Case "gde"
                Return "Non-Game Mini App / Video Service Native App"
            Case "gdk"
                Return "Video Service Web App"
            Case "gdl"
                Return "PS Cloud Beta App"
            Case "gdO"
                Return "PS2 Classic"
            Case "gp"
                Return "Game Application Patch"
            Case "gpc"
                Return "Non-Game Big App Patch"
            Case "gpd"
                Return "BG Application patch"
            Case "gpe"
                Return "Non-Game Mini App Patch / Video Service Native App Patch"
            Case "gpk"
                Return "Video Service Web App Patch"
            Case "gpl"
                Return "PS Cloud Beta App Patch"
            Case "sd"
                Return "Save Data"
            Case "la"
                Return "Live Area"
            Case "wda"
                Return "Unknown"
            Case Else
                Return "Unknown"
        End Select
    End Function

    Public Shared Function GetPS3Category(SFOCategory As String) As String
        Select Case SFOCategory
            Case "DG"
                Return "Disc Game"
            Case "AR"
                Return "Autoinstall Root"
            Case "DP"
                Return "Disc Packages"
            Case "IP"
                Return "Install Package"
            Case "TR"
                Return "Theme Root"
            Case "VR"
                Return "Vide Root"
            Case "VI"
                Return "Video Item"
            Case "XR"
                Return "Extra Root"
            Case "DM"
                Return "Disc Movie"
            Case "HG"
                Return "HDD Game"
            Case "GD"
                Return "Game Data"
            Case "SD"
                Return "Save Data"
            Case "PP"
                Return "PSP"
            Case "PE"
                Return "PSP Emulator"
            Case "MN"
                Return "PSP Minis"
            Case "1P"
                Return "PS1 PSN"
            Case "2P"
                Return "PS2 PSN"
            Case Else
                Return "Unknown"
        End Select
    End Function

    Public Shared Function GetPS3GameRegion(GameID As String) As String
        If GameID.StartsWith("BLES") Then
            Return "Europe"
        ElseIf GameID.StartsWith("BCES") Then
            Return "Europe"
        ElseIf GameID.StartsWith("NPEB") Then
            Return "Europe"
        ElseIf GameID.StartsWith("BLUS") Then
            Return "USA"
        ElseIf GameID.StartsWith("BCUS") Then
            Return "USA"
        ElseIf GameID.StartsWith("NPUB") Then
            Return "USA"
        ElseIf GameID.StartsWith("BCJS") Then
            Return "Japan"
        ElseIf GameID.StartsWith("BLJS") Then
            Return "Japan"
        ElseIf GameID.StartsWith("NPJB") Then
            Return "Japan"
        ElseIf GameID.StartsWith("BCAS") Then
            Return "Asia"
        ElseIf GameID.StartsWith("BLAS") Then
            Return "Asia"
        Else
            Return ""
        End If
    End Function

End Class
