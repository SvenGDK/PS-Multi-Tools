Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports Microsoft.Web.WebView2.Core

Public Class PKGBrowser

    Public Console As String = ""
    Private CurrentListView As ListView
    Private DownloadsList As New List(Of NPSPKG)()
    Private TempDownloadsList As New List(Of NPSPKG)()

    Private WithEvents DownloadMenuItem As New MenuItem() With {.Header = "Download PKG", .Icon = New Image() With {.Source = New BitmapImage(New Uri("/Images/download.png", UriKind.Relative))}}
    Private WithEvents CreateRAPMenuItem As New MenuItem() With {.Header = "Create .rap file", .Icon = New Image() With {.Source = New BitmapImage(New Uri("/Images/create.png", UriKind.Relative))}}
    Private WithEvents ExtractPKGMenuItem As New MenuItem() With {.Header = "Extract .pkg file", .Icon = New Image() With {.Source = New BitmapImage(New Uri("/Images/extract.png", UriKind.Relative))}}

    Private WithEvents ShowDownloadMenuItem As New MenuItem() With {.Header = "Show download in folder"}
    Private WithEvents CancelDownloadMenuItem As New MenuItem() With {.Header = "Cancel download"}

    Dim URLs As New List(Of String)()
    Dim CurrentURL As Integer = 0

    Private Sub PKGBrowser_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        LoadListViewContextMenu()
        LoadDownloadsContextMenu()
    End Sub

    Private Sub MainTabControl_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles MainTabControl.MouseDown
        Select Case MainTabControl.SelectedIndex
            Case 0
                LoadGames()
            Case 1
                LoadDemos()
            Case 2
                LoadDLCs()
            Case 3
                LoadThemes()
            Case 4
                LoadAvatars()
        End Select
    End Sub

    Private Sub LoadGames()
        GamesListView.Items.Clear()

        Dim Result As MsgBoxResult = MsgBox("Do you want to load the latest database ?", MsgBoxStyle.YesNoCancel, "Loading game database")
        If Result = MsgBoxResult.Yes Then
            LoadDLList(Console + "_GAMES.tsv", True)
        ElseIf Result = MsgBoxResult.No Then
            LoadDLList(Console + "_GAMES.tsv", False)
        End If

        CurrentListView = GamesListView
    End Sub

    Private Sub LoadDemos()
        DemosListView.Items.Clear()

        Dim Result As MsgBoxResult = MsgBox("Do you want to load the latest database ?", MsgBoxStyle.YesNoCancel, "Loading demos database")
        If Result = MsgBoxResult.Yes Then
            LoadDLList(Console + "_DEMOS.tsv", True)
        ElseIf Result = MsgBoxResult.No Then
            LoadDLList(Console + "_DEMOS.tsv", False)
        End If

        CurrentListView = DemosListView
    End Sub

    Private Sub LoadDLCs()
        DLCsListView.Items.Clear()

        Dim Result As MsgBoxResult = MsgBox("Do you want to load the latest database ?", MsgBoxStyle.YesNoCancel, "Loading DLCs database")
        If Result = MsgBoxResult.Yes Then
            LoadDLList(Console + "_DLCS.tsv", True)
        ElseIf Result = MsgBoxResult.No Then
            LoadDLList(Console + "_DLCS.tsv", False)
        End If

        CurrentListView = DLCsListView
    End Sub

    Private Sub LoadThemes()
        ThemesListView.Items.Clear()

        Dim Result As MsgBoxResult = MsgBox("Do you want to load the latest database ?", MsgBoxStyle.YesNoCancel, "Loading themes database")
        If Result = MsgBoxResult.Yes Then
            LoadDLList(Console + "_THEMES.tsv", True)
        ElseIf Result = MsgBoxResult.No Then
            LoadDLList(Console + "_THEMES.tsv", False)
        End If

        CurrentListView = ThemesListView
    End Sub

    Private Sub LoadAvatars()
        AvatarsListView.Items.Clear()

        Dim Result As MsgBoxResult = MsgBox("Do you want to load the latest database ?", MsgBoxStyle.YesNoCancel, "Loading avatars database")
        If Result = MsgBoxResult.Yes Then
            LoadDLList(Console + "_AVATARS.tsv", True)
        ElseIf Result = MsgBoxResult.No Then
            LoadDLList(Console + "_AVATARS.tsv", False)
        End If

        CurrentListView = AvatarsListView
    End Sub

#Region "NPS Downloads"

    Private Sub TextSearch(TxtBox As TextBox)
        CurrentListView.Items.Clear()

        If TxtBox.Name = NameSearchTextBox.Name Then
            For Each item As NPSPKG In DownloadsList.Where(Function(lvi) lvi.PackageName.ToLower().Contains(TxtBox.Text.ToLower().Trim()))
                CurrentListView.Items.Add(item)
            Next
        ElseIf TxtBox.Name = TitleIDSearchTextBox.Name Then
            For Each item As NPSPKG In DownloadsList.Where(Function(lvi) lvi.PackageTitleID.ToLower().Contains(TxtBox.Text.ToLower().Trim()))
                CurrentListView.Items.Add(item)
            Next
        ElseIf TxtBox.Name = ContentIDSearchTextBox.Name Then
            For Each item As NPSPKG In DownloadsList.Where(Function(lvi) lvi.PackageContentID.ToLower().Contains(TxtBox.Text.ToLower().Trim()))
                CurrentListView.Items.Add(item)
            Next
        End If
    End Sub

    Private Async Sub LoadDLList(RequestedList As String, LoadLatest As Boolean)
        'Get the latest database from NPS
        If LoadLatest = True Then
            Using NewWebClient As New WebClient
                Dim GamesList As String = Await NewWebClient.DownloadStringTaskAsync(New Uri("https://nopaystation.com/tsv/" + RequestedList))
                Dim GamesListLines As String() = GamesList.Split(CChar(vbCrLf))
                For Each GameLine As String In GamesListLines.Skip(1)
                    Dim SplittedValues As String() = GameLine.Split(CChar(vbTab))
                    Dim AdditionalInfo As Structures.PackageInfo = Utils.GetFileSizeAndDate(SplittedValues(8).Trim(), SplittedValues(6).Trim())
                    Dim NewPackage As New NPSPKG() With {.PackageName = SplittedValues(2).Trim(),
                        .PackageURL = SplittedValues(3).Trim(),
                        .PackageTitleID = SplittedValues(0).Trim(),
                        .PackageContentID = SplittedValues(5).Trim(),
                        .PackageRAP = SplittedValues(4).Trim(),
                        .PackageDate = AdditionalInfo.FileDate,
                        .PackageSize = AdditionalInfo.FileSize,
                        .PackageRegion = SplittedValues(1).Trim()}

                    If Not SplittedValues(3).Trim() = "MISSING" Then

                        Select Case Console
                            Case "PS3"
                                If Not String.IsNullOrEmpty(NewPackage.PackageContentID) Then
                                    Dim TitleID As String = NewPackage.PackageTitleID
                                    Dim ContentID As String = NewPackage.PackageContentID.Split("-"c)(2)

                                    NewPackage.PackageCoverSource = "https://www.gametdb.com/PS3/" + TitleID
                                End If
                            Case "PSV"
                                If Not String.IsNullOrEmpty(NewPackage.PackageTitleID) Then
                                    Dim TitleID As String = NewPackage.PackageTitleID
                                    Dim ContentID As String = NewPackage.PackageContentID.Split("-"c)(2)

                                    NewPackage.PackageCoverSource = "https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + TitleID + ".png"
                                End If
                        End Select

                        DownloadsList.Add(NewPackage)
                    End If
                Next
            End Using
        Else 'Use local .tsv file
            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Databases\" + RequestedList) Then
                Dim FileReader As String() = File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\Databases\" + RequestedList, Text.Encoding.UTF8)
                For Each GameLine As String In FileReader.Skip(1) 'Skip 1st line in TSV
                    Dim SplittedValues As String() = GameLine.Split(CChar(vbTab))
                    Dim AdditionalInfo As Structures.PackageInfo = Utils.GetFileSizeAndDate(SplittedValues(8), SplittedValues(6))
                    Dim NewPackage As New NPSPKG() With {.PackageName = SplittedValues(2),
                        .PackageURL = SplittedValues(3),
                        .PackageTitleID = SplittedValues(0),
                        .PackageContentID = SplittedValues(5),
                        .PackageRAP = SplittedValues(4),
                        .PackageDate = AdditionalInfo.FileDate,
                        .PackageSize = AdditionalInfo.FileSize,
                        .PackageRegion = SplittedValues(1)}

                    If Not SplittedValues(3) = "MISSING" Then

                        Select Case Console
                            Case "PS3"
                                If Not String.IsNullOrEmpty(NewPackage.PackageContentID) Then
                                    Dim TitleID As String = NewPackage.PackageTitleID
                                    Dim ContentID As String = NewPackage.PackageContentID.Split("-"c)(2)

                                    NewPackage.PackageCoverSource = "https://www.gametdb.com/PS3/" + TitleID
                                End If
                            Case "PSV"
                                If Not String.IsNullOrEmpty(NewPackage.PackageTitleID) Then
                                    Dim TitleID As String = NewPackage.PackageTitleID
                                    Dim ContentID As String = NewPackage.PackageContentID.Split("-"c)(2)

                                    NewPackage.PackageCoverSource = "https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + TitleID + ".png"
                                End If
                        End Select

                        DownloadsList.Add(NewPackage)
                    End If
                Next
            Else
                MsgBox("No data available. Please add TSV files to the 'Databases' directory.", MsgBoxStyle.Exclamation, "Could not load list")
            End If
        End If

        'Add to the list
        For Each AvailablePKG In DownloadsList
            Select Case RequestedList.Split("."c)(0)
                Case "PS3_GAMES", "PSV_GAMES"
                    GamesListView.Items.Add(AvailablePKG)
                Case "PS3_DEMOS"
                    DemosListView.Items.Add(AvailablePKG)
                Case "PS3_DLCS", "PSV_DLCS"
                    DLCsListView.Items.Add(AvailablePKG)
                Case "PS3_THEMES", "PSV_THEMES"
                    ThemesListView.Items.Add(AvailablePKG)
                Case "PS3_AVATARS"
                    AvatarsListView.Items.Add(AvailablePKG)
            End Select
        Next
    End Sub

    Private Sub NameSearchTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles NameSearchTextBox.TextChanged
        If CurrentListView IsNot Nothing Then
            TextSearch(NameSearchTextBox)
        Else
            MsgBox("Please load the database first by clicking on the middle of the list or the left/right arrow.", MsgBoxStyle.Information, "Search unavailable")
        End If
    End Sub

    Private Sub TitleIDSearchTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TitleIDSearchTextBox.TextChanged
        If CurrentListView IsNot Nothing Then
            TextSearch(TitleIDSearchTextBox)
        Else
            MsgBox("Please load the database first by clicking on the middle of the list or the left/right arrow.", MsgBoxStyle.Information, "Search unavailable")
        End If
    End Sub

    Private Sub ContentIDSearchTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles ContentIDSearchTextBox.TextChanged
        If CurrentListView IsNot Nothing Then
            TextSearch(ContentIDSearchTextBox)
        Else
            MsgBox("Please load the database first by clicking on the middle of the list or the left/right arrow.", MsgBoxStyle.Information, "Search unavailable")
        End If
    End Sub

    Private Sub DownloadMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMenuItem.Click
        If CurrentListView.SelectedItem IsNot Nothing Then
            Dim SelectedPackage As NPSPKG = CType(CurrentListView.SelectedItem, NPSPKG)
            CreateNewDownload(SelectedPackage)
        End If
    End Sub

    Private Sub CreateNewDownload(NPSPKG As NPSPKG)

        If Not Directory.Exists(My.Computer.FileSystem.CurrentDirectory + "\Downloads\" + Console) Then
            Directory.CreateDirectory(My.Computer.FileSystem.CurrentDirectory + "\Downloads\" + Console + "\packages")
            Directory.CreateDirectory(My.Computer.FileSystem.CurrentDirectory + "\Downloads\PS3\exdata")
        End If

        Dim NewWebClient As New WebClient()

        Dim NewPKGDL As New PKGDownloadListViewItem() With {
            .AssociatedWebClient = NewWebClient,
            .PackageName = NPSPKG.PackageName,
            .PackageSize = NPSPKG.PackageSize,
            .PackageContentID = NPSPKG.PackageContentID,
            .PackageTitleID = NPSPKG.PackageTitleID,
            .PackageDownloadDestination = My.Computer.FileSystem.CurrentDirectory + "\Downloads\" + Console + "\packages\" + NPSPKG.PackageName + ".pkg",
            .PackageDownloadState = "Downloading"}

        DownloadsListView.Items.Add(NewPKGDL)
        DownloadsListView.Items.Refresh()

        If Not String.IsNullOrEmpty(NPSPKG.PackageURL) Then
            NewWebClient.DownloadFileAsync(New Uri(NPSPKG.PackageURL), My.Computer.FileSystem.CurrentDirectory + "\Downloads\" + Console + "\packages\" + NPSPKG.PackageName + ".pkg", Stopwatch.StartNew)

            AddHandler NewWebClient.DownloadProgressChanged, Sub(sender As Object, e As DownloadProgressChangedEventArgs)
                                                                 'Update values
                                                                 Dim ClientSender As WebClient = CType(sender, WebClient)

                                                                 For Each DownloadItem In DownloadsListView.Items
                                                                     Dim DLListViewItem As PKGDownloadListViewItem = CType(DownloadItem, PKGDownloadListViewItem)
                                                                     If DLListViewItem.AssociatedWebClient Is ClientSender Then
                                                                         DLListViewItem.PackageDownloadState = e.ProgressPercentage.ToString() + "% - " + (e.BytesReceived / (1024 * 1024)).ToString("0.000 MB") + "/" + (e.TotalBytesToReceive / (1024 * 1024)).ToString("0.000 MB")
                                                                     End If
                                                                 Next

                                                             End Sub

            AddHandler NewWebClient.DownloadFileCompleted, Sub(sender As Object, e As AsyncCompletedEventArgs)
                                                               Dim ClientSender As WebClient = CType(sender, WebClient)

                                                               'Update values
                                                               For Each DownloadItem In DownloadsListView.Items
                                                                   Dim DLListViewItem As PKGDownloadListViewItem = CType(DownloadItem, PKGDownloadListViewItem)
                                                                   If DLListViewItem.AssociatedWebClient Is ClientSender Then
                                                                       DLListViewItem.PackageDownloadState = "Download complete"
                                                                   End If
                                                               Next
                                                           End Sub
        End If
    End Sub

    Private Sub CreateRAPMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles CreateRAPMenuItem.Click
        If CurrentListView.SelectedItem IsNot Nothing Then
            Dim SelectedPackage As NPSPKG = CType(CurrentListView.SelectedItem, NPSPKG)
            CreateRAP(SelectedPackage.PackageContentID, SelectedPackage.PackageRAP)
        End If
    End Sub

    Public Sub LoadListViewContextMenu()
        Dim ListViewContextMenu As New ContextMenu()
        ListViewContextMenu.Items.Add(DownloadMenuItem)

        GamesListView.ContextMenu = ListViewContextMenu
        DemosListView.ContextMenu = ListViewContextMenu
        DLCsListView.ContextMenu = ListViewContextMenu
        ThemesListView.ContextMenu = ListViewContextMenu
        AvatarsListView.ContextMenu = ListViewContextMenu
    End Sub

    Private Sub LoadDownloadsContextMenu()
        DownloadsListView.ContextMenu = Nothing

        Dim DownloadsContextMenu As New ContextMenu()
        DownloadsContextMenu.Items.Add(ShowDownloadMenuItem)
        DownloadsContextMenu.Items.Add(CancelDownloadMenuItem)
        DownloadsContextMenu.Items.Add(New Separator())

        If Console = "PS3" Then
            DownloadsContextMenu.Items.Add(CreateRAPMenuItem)
        ElseIf Console = "PSV" Then
            DownloadsContextMenu.Items.Add(ExtractPKGMenuItem)
        End If

        DownloadsListView.ContextMenu = DownloadsContextMenu
    End Sub

    Private Sub CreateRAP(ContentID As String, RAP As String)
        Try
            If Not String.IsNullOrEmpty(ContentID) AndAlso RAP.Length Mod 2 = 0 Then

                'Create exdata folder if not exists
                If Not Directory.Exists(My.Computer.FileSystem.CurrentDirectory + "\Downloads\PS3\exdata") Then Directory.CreateDirectory(My.Computer.FileSystem.CurrentDirectory + "\Downloads\PS3\exdata")

                Dim bytes As Byte() = New Byte(CInt((RAP.Length / 2) - 1)) {}
                For index As Integer = 0 To CInt((RAP.Length / 2) - 1)
                    bytes(index) = Convert.ToByte(RAP.Substring(index * 2, 2), 16)
                Next
                File.WriteAllBytes(My.Computer.FileSystem.CurrentDirectory + "\Downloads\PS3\exdata\" + ContentID & ".rap", bytes)

                MsgBox(ContentID & ".rap file created!" & vbCrLf & "You can find it in the 'Downloads\PS3\exdata' folder.", MsgBoxStyle.Information)
            Else
                MsgBox("This package requires no .rap file. Simply activate it with ReactPSN.", MsgBoxStyle.Information)
            End If
        Catch ex As Exception
            MsgBox("Error creating RAP file: " & vbCrLf & ex.ToString)
        End Try
    End Sub

#End Region

    Private Async Sub ContentWebView_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs) Handles ContentWebView.NavigationCompleted
        If e.IsSuccess And ContentWebView.Source.ToString.StartsWith("https://www.gametdb.com") Then

            'Game ID
            Dim GameID As String = Await ContentWebView.ExecuteScriptAsync("document.getElementsByClassName('GameData')[0].getElementsByTagName('td')[1].innerText;")
            'Game Image
            Dim GameImageURL As String = Await ContentWebView.ExecuteScriptAsync("document.getElementsByClassName('frame lfloat')[0].getElementsByTagName('img')[0].src;")

            If Not GameImageURL = "null" AndAlso Not String.IsNullOrEmpty(GameID) Then

                GameID = GameID.Replace("""", "")
                GameImageURL = GameImageURL.Replace("""", "")

                For Each ItemInListView In CurrentListView.Items
                    Dim FoundGame As NPSPKG = CType(ItemInListView, NPSPKG)

                    If FoundGame.PackageTitleID.Contains(GameID) Or FoundGame.PackageTitleID = GameID Then

                        If Dispatcher.CheckAccess() = False Then
                            Await Dispatcher.BeginInvoke(Sub()
                                                             Dim TempBitmapImage = New BitmapImage()
                                                             TempBitmapImage.BeginInit()
                                                             TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                             TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                             TempBitmapImage.UriSource = New Uri(GameImageURL, UriKind.RelativeOrAbsolute)
                                                             TempBitmapImage.EndInit()
                                                             FoundGame.GameCoverSource = TempBitmapImage
                                                         End Sub)
                        Else
                            Dim TempBitmapImage = New BitmapImage()
                            TempBitmapImage.BeginInit()
                            TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                            TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                            TempBitmapImage.UriSource = New Uri(GameImageURL, UriKind.RelativeOrAbsolute)
                            TempBitmapImage.EndInit()
                            FoundGame.GameCoverSource = TempBitmapImage
                        End If

                        Exit For
                    End If
                Next

                If Dispatcher.CheckAccess() = False Then
                    Await Dispatcher.BeginInvoke(Sub() CurrentListView.Items.Refresh())
                Else
                    CurrentListView.Items.Refresh()
                End If
            End If
        End If
    End Sub

    Private Sub GamesListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles GamesListView.SelectionChanged
        If GamesListView.SelectedItem IsNot Nothing Then

            Dim SelectedGame As NPSPKG = CType(GamesListView.SelectedItem, NPSPKG)
            If Not String.IsNullOrEmpty(SelectedGame.PackageCoverSource) Then
                If Utils.IsURLValid(SelectedGame.PackageCoverSource) Then
                    Select Case Console
                        Case "PS3"
                            ContentWebView.CoreWebView2.Navigate(SelectedGame.PackageCoverSource)
                        Case "PSV"
                            If Dispatcher.CheckAccess() = False Then
                                Dispatcher.BeginInvoke(Sub()
                                                           Dim TempBitmapImage = New BitmapImage()
                                                           TempBitmapImage.BeginInit()
                                                           TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                           TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                           TempBitmapImage.UriSource = New Uri(SelectedGame.PackageCoverSource, UriKind.RelativeOrAbsolute)
                                                           TempBitmapImage.EndInit()
                                                           SelectedGame.GameCoverSource = TempBitmapImage
                                                       End Sub)
                            Else
                                Dim TempBitmapImage = New BitmapImage()
                                TempBitmapImage.BeginInit()
                                TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                TempBitmapImage.UriSource = New Uri(SelectedGame.PackageCoverSource, UriKind.RelativeOrAbsolute)
                                TempBitmapImage.EndInit()
                                SelectedGame.GameCoverSource = TempBitmapImage
                            End If
                    End Select
                End If
            End If

            PKGTitleTextBlock.Text = SelectedGame.PackageName
            TitleIDTextBlock.Text = "Title ID: " & SelectedGame.PackageTitleID
            ContentIDTextBlock.Text = "Content ID: " & SelectedGame.PackageContentID
            RegionTextBlock.Text = "Region: " & SelectedGame.PackageRegion
            SizeTextBlock.Text = "PKG Size: " & SelectedGame.PackageSize
        End If
    End Sub

    Private Sub ShowDownloadMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles ShowDownloadMenuItem.Click
        If DownloadsListView.SelectedItem IsNot Nothing Then
            Process.Start(My.Computer.FileSystem.CurrentDirectory + "\Downloads\" + Console + "\packages")
        End If
    End Sub

    Private Sub CancelDownloadMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles CancelDownloadMenuItem.Click
        If DownloadsListView.SelectedItem IsNot Nothing Then
            Dim SelectedDownload As PKGDownloadListViewItem = CType(DownloadsListView.SelectedItem, PKGDownloadListViewItem)

            If SelectedDownload.AssociatedWebClient IsNot Nothing Then
                If SelectedDownload.AssociatedWebClient.IsBusy Then
                    SelectedDownload.AssociatedWebClient.CancelAsync()
                End If
            End If
        End If
    End Sub

    Private Async Sub ExtractPKGMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles ExtractPKGMenuItem.Click
        If DownloadsListView.SelectedItem IsNot Nothing Then
            Dim SelectedDownload As PKGDownloadListViewItem = CType(DownloadsListView.SelectedItem, PKGDownloadListViewItem)
            Dim GameContentID As String = SelectedDownload.PackageContentID
            Dim GamezRIF As String = ""

            If Not String.IsNullOrEmpty(SelectedDownload.PackageDownloadDestination) Then
                If Not File.Exists(Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination) + "\pkg2zip.exe") Then
                    File.Copy(My.Computer.FileSystem.CurrentDirectory + "\Tools\pkg2zip.exe", Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination) + "\pkg2zip.exe", True)
                End If
            End If

            Dim DatabaseToLoad As String = ""
            Select Case CurrentListView.Name
                Case "GamesListView"
                    DatabaseToLoad = "PSV_GAMES.tsv"
                Case "DLCsListView"
                    DatabaseToLoad = "PSV_DLCS.tsv"
                Case "ThemesListView"
                    DatabaseToLoad = "PSV_THEMES.tsv"
            End Select

            If MsgBox("Load zRIF key from the latest database ?" + vbCrLf + "Selecting 'No' will use the local database file.", MsgBoxStyle.YesNoCancel) = MsgBoxResult.Yes Then
                Using NewWebClient As New WebClient
                    Dim GamesList As String = Await NewWebClient.DownloadStringTaskAsync(New Uri("https://nopaystation.com/tsv/" + DatabaseToLoad))
                    Dim GamesListLines As String() = GamesList.Split(CChar(vbCrLf))
                    For Each GameLine As String In GamesListLines.Skip(1)
                        Dim SplittedValues As String() = GameLine.Split(CChar(vbTab))
                        Dim AdditionalInfo As Structures.PackageInfo = Utils.GetFileSizeAndDate(SplittedValues(8).Trim(), SplittedValues(6).Trim())
                        Dim NewPackage As New NPSPKG() With {.PackageName = SplittedValues(2).Trim(),
                            .PackageURL = SplittedValues(3).Trim(),
                            .PackageTitleID = SplittedValues(0).Trim(),
                            .PackageContentID = SplittedValues(5).Trim(),
                            .PackageRAP = SplittedValues(4).Trim(),
                            .PackageDate = AdditionalInfo.FileDate,
                            .PackageSize = AdditionalInfo.FileSize,
                            .PackageRegion = SplittedValues(1).Trim()}
                        If Not SplittedValues(3).Trim() = "MISSING" Then 'Only add available PKGs
                            TempDownloadsList.Add(NewPackage)
                        End If
                    Next
                End Using
            Else 'Use local .tsv file
                If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Databases\" + DatabaseToLoad) Then
                    Dim FileReader As String() = File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\Databases\" + DatabaseToLoad, Text.Encoding.UTF8)
                    For Each GameLine As String In FileReader.Skip(1) 'Skip 1st line in TSV
                        Dim SplittedValues As String() = GameLine.Split(CChar(vbTab))
                        Dim AdditionalInfo As Structures.PackageInfo = Utils.GetFileSizeAndDate(SplittedValues(8), SplittedValues(6))
                        Dim NewPackage As New NPSPKG() With {.PackageName = SplittedValues(2),
                            .PackageURL = SplittedValues(3),
                            .PackageTitleID = SplittedValues(0),
                            .PackageContentID = SplittedValues(5),
                            .PackageRAP = SplittedValues(4),
                            .PackageDate = AdditionalInfo.FileDate,
                            .PackageSize = AdditionalInfo.FileSize,
                            .PackageRegion = SplittedValues(1)}
                        If Not SplittedValues(3) = "MISSING" Then 'Only add available PKGs
                            TempDownloadsList.Add(NewPackage)
                        End If
                    Next
                Else
                    MsgBox("Nothing database available. Please add PS Vita TSV files to the 'Databases' directory.", MsgBoxStyle.Exclamation, "Could not load list")
                End If
            End If

            For Each AvailablePKG As NPSPKG In TempDownloadsList
                If AvailablePKG.PackageContentID = GameContentID Then
                    If AvailablePKG.PackageRAP IsNot Nothing Then
                        GamezRIF = AvailablePKG.PackageRAP
                        Exit For
                    End If
                End If
            Next

            If Not String.IsNullOrEmpty(GamezRIF) Then
                Directory.SetCurrentDirectory(Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination)) 'Required to extract at pkg location

                If Not String.IsNullOrEmpty(SelectedDownload.PackageDownloadDestination) AndAlso File.Exists(SelectedDownload.PackageDownloadDestination) Then
                    Using PKG2ZIP As New Process()

                        PKG2ZIP.StartInfo.FileName = Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination) + "\pkg2zip.exe"
                        PKG2ZIP.StartInfo.Arguments = "-x """ + SelectedDownload.PackageDownloadDestination + """ """ + GamezRIF + """"
                        PKG2ZIP.StartInfo.RedirectStandardOutput = True
                        PKG2ZIP.StartInfo.RedirectStandardError = True
                        PKG2ZIP.StartInfo.UseShellExecute = False
                        PKG2ZIP.StartInfo.CreateNoWindow = True
                        PKG2ZIP.Start()

                        Dim OutputReader As StreamReader = PKG2ZIP.StandardOutput
                        Dim ProcessOutput As String = OutputReader.ReadToEnd()

                        If ProcessOutput.Contains("done!") Then
                            If MsgBox("PKG extracted! Do you want to open the folder containing the extracted folder ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                                Process.Start("explorer", Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination))
                            End If
                        Else
                            MsgBox("Could not extract the selected .pkg file.", MsgBoxStyle.Critical)
                        End If

                        If File.Exists(Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination) + "\pkg2zip.exe") Then
                            File.Delete(Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination) + "\pkg2zip.exe")
                        End If
                    End Using
                Else

                End If

                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory)
            Else
                MsgBox("No zRIF found.", MsgBoxStyle.Critical)
            End If

        End If
    End Sub

End Class
