Imports FluentFTP
Imports Microsoft.Web.WebView2.Core
Imports MS.Internal
Imports Newtonsoft.Json
Imports System.ComponentModel
Imports System.IO
Imports System.Security.Authentication
Imports System.Text

Public Class PS5Library

    Dim WithEvents GameLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PS5 files", .ShowActivated = True}

    Public CurrentPath As String = ""
    Dim ConsoleIP As String = ""
    Dim ConsolePort As String = ""

    Dim PKGCount As Integer = 0
    Dim URLs As New List(Of String)
    Dim CurrentURL As Integer = 0
    Dim TotalSize As Long = 0

    'Supplemental library menu items
    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load installed games and apps over FTP"}
    Dim WithEvents LoadPKGFolderMenuItem As New Controls.MenuItem() With {.Header = "Load patches PKG folder"}
    Dim WithEvents LoadDLFolderMenuItem As New Controls.MenuItem() With {.Header = "Open Downloads folder"}

    Private Sub PS5Library_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Will set the console IP and port when changing the console address in the settings
        AddHandler NewPS5Menu.IPTextChanged, AddressOf PS5MenuIPTextChanged

        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As Controls.MenuItem = CType(NewPS5Menu.Items(0), Controls.MenuItem)
        LibraryMenuItem.Items.Add(LoadPKGFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem)
    End Sub

#Region "Game Loader"

    Public Enum LoadType
        FTP
        PKGs
    End Enum

    Public Structure GameLoaderArgs
        Private _Type As LoadType
        Private _FolderPath As String

        Public Property Type As LoadType
            Get
                Return _Type
            End Get
            Set
                _Type = Value
            End Set
        End Property

        Public Property FolderPath As String
            Get
                Return _FolderPath
            End Get
            Set
                _FolderPath = Value
            End Set
        End Property
    End Structure

    Private Sub GameLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles GameLoaderWorker.DoWork

        Dim WorkerArgs As GameLoaderArgs = CType(e.Argument, GameLoaderArgs)

        If WorkerArgs.Type = LoadType.FTP Then

            'Get installed games and apps over FTP
            Dim CPort As Integer = Integer.Parse(ConsolePort)
            Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", CPort)
                'Configurate the FTP connection
                conn.Config.EncryptionMode = FtpEncryptionMode.None
                conn.Config.SslProtocols = SslProtocols.None
                conn.Config.DataConnectionEncryption = False

                'Connect
                conn.Connect()

                'List appmeta directory for the games
                For Each item In conn.GetListing("/user/appmeta")

                    Dim PS5GameLVItem As New PS5Game()

                    If item.Type = FtpObjectType.Directory Then

                        If item.Name = "push_resource" Or item.Name = "addcont" Or item.Name = "external" Then

                        Else
                            PS5GameLVItem.GameID = item.Name

                            'Check for icon0.png
                            If conn.GetObjectInfo(item.FullName + "/icon0.png") IsNot Nothing Then
                                Dim Icon0Bytes As Byte() = Nothing
                                If conn.DownloadBytes(Icon0Bytes, item.FullName + "/icon0.png") Then
                                    PS5GameLVItem.GameCoverSource = Utils.BitmapSourceFromByteArray(Icon0Bytes)
                                End If
                            End If

                            'Check for pic0.png (currently not used - can take also some more time)
                            'If conn.GetObjectInfo(item.FullName + "/pic0.png") IsNot Nothing Then
                            '    Dim Pic0Bytes As Byte() = Nothing
                            '    If conn.DownloadBytes(Pic0Bytes, item.FullName + "/pic0.png") Then
                            '        PS5GameLVItem.GameCoverSource = Utils.BitmapSourceFromByteArray(Pic0Bytes)
                            '    End If
                            'End If

                            'Check for param.json
                            If conn.GetObjectInfo(item.FullName + "/param.json") IsNot Nothing Then
                                Dim ParamBytes As Byte() = Nothing
                                If conn.DownloadBytes(ParamBytes, item.FullName + "/param.json") Then
                                    Dim ParamBytesAsString = Encoding.UTF8.GetString(ParamBytes)
                                    Dim GameInfos As Structures.PS5GameParamJson = JsonConvert.DeserializeObject(Of Structures.PS5GameParamJson)(ParamBytesAsString)

                                    PS5GameLVItem.GameTitle = GameInfos.LocalizedParameters.EnUS.TitleName
                                    PS5GameLVItem.GameContentID = GameInfos.ContentId
                                    PS5GameLVItem.GameRegion = PS4Game.GetGameRegion(GameInfos.ContentId)
                                End If
                            End If

                            'Check for game pkg info
                            If conn.GetObjectInfo("/user/app/" + item.Name + "/app.pkg") IsNot Nothing Then
                                Dim PKG As FtpListItem = conn.GetObjectInfo("/user/app/" + item.Name + "/app.pkg")
                                PS5GameLVItem.GameSize = FormatNumber(PKG.Size / 1048576, 2) + " MB"
                            End If

                            'Add to the GamesListView
                            If Dispatcher.CheckAccess() = False Then
                                Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(PS5GameLVItem))
                            Else
                                GamesListView.Items.Add(PS5GameLVItem)
                            End If
                        End If

                    End If

                Next

                'List installed system apps 
                For Each item In conn.GetListing("/system_ex/app")

                    Dim PS5GameLVItem As New PS5Game()

                    If item.Type = FtpObjectType.Directory Then

                        PS5GameLVItem.GameID = item.Name

                        'Check for pic0.png (currently not used - can take also some more time)
                        'If conn.GetObjectInfo(item.FullName + "/pic0.png") IsNot Nothing Then
                        '    Dim Pic0Bytes As Byte() = Nothing
                        '    If conn.DownloadBytes(Pic0Bytes, item.FullName + "/pic0.png") Then
                        '        PS5GameLVItem.GameCoverSource = Utils.BitmapSourceFromByteArray(Pic0Bytes)
                        '    End If
                        'End If

                        'Check for param.json
                        If conn.GetObjectInfo(item.FullName + "/sce_sys/param.json") IsNot Nothing Then
                            Dim ParamBytes As Byte() = Nothing
                            If conn.DownloadBytes(ParamBytes, item.FullName + "/sce_sys/param.json") Then
                                Dim ParamBytesAsString = Encoding.UTF8.GetString(ParamBytes)
                                Dim AppInfos As Structures.AppParamJson = JsonConvert.DeserializeObject(Of Structures.AppParamJson)(ParamBytesAsString)

                                PS5GameLVItem.GameTitle = AppInfos.LocalizedParameters.EnUS.TitleName
                                'PS5GameLVItem.GameContentID = AppInfos.ContentId
                                'PS5GameLVItem.GameRegion = PS4Game.GetGameRegion(AppInfos.ContentId)
                            End If
                        End If

                        'Add to the AppsListView
                        If Dispatcher.CheckAccess() = False Then
                            Dispatcher.BeginInvoke(Sub() AppsListView.Items.Add(PS5GameLVItem))
                        Else
                            AppsListView.Items.Add(PS5GameLVItem)
                        End If
                    End If

                Next

                'Disconnect
                conn.Disconnect()
            End Using

        ElseIf WorkerArgs.Type = LoadType.PKGs Then

            'PS5 Patch Source pkgs
            For Each PatchSCPKG In Directory.GetFiles(WorkerArgs.FolderPath, "*_sc.pkg", SearchOption.AllDirectories)

                Dim NewPS5Game As New PS5Game()
                Dim PKGFileInfo As New FileInfo(PatchSCPKG)

                TotalSize = 0

                Using SFOReader As New Process()
                    SFOReader.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\ps5_pkg.exe"
                    SFOReader.StartInfo.Arguments = "--psmtparam file:""" + PatchSCPKG + """"
                    SFOReader.StartInfo.RedirectStandardOutput = True
                    SFOReader.StartInfo.UseShellExecute = False
                    SFOReader.StartInfo.CreateNoWindow = True
                    SFOReader.Start()

                    Dim OutputReader As StreamReader = SFOReader.StandardOutput
                    Dim ProcessOutput As String = OutputReader.ReadToEnd()

                    If ProcessOutput.Count > 0 Then
                        Dim ParamData = JsonConvert.DeserializeObject(Of PS5PKGParamJSON.PS5PKGParam)(ProcessOutput)

                        If ParamData IsNot Nothing Then
                            NewPS5Game.GameID = ParamData.TitleId
                            NewPS5Game.GameTitle = ParamData.LocalizedParameters.EnUS.TitleName
                            NewPS5Game.GameContentID = ParamData.ContentId
                            NewPS5Game.GameRegion = PS5Game.GetGameRegion(ParamData.TitleId)
                            NewPS5Game.GameCategory = "Game Patch"
                            NewPS5Game.GameSize = FormatNumber(GetDirSize(PKGFileInfo.DirectoryName) / 1073741824, 2) + " GB" 'Will only display correct if all PKG parts are present.
                            NewPS5Game.GameVersion = "Patch: " + ParamData.ContentVersion
                            NewPS5Game.GameRequiredFirmware = "Req.FW: " + ParamData.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, "."c).Insert(5, "."c).Insert(8, "."c).Remove(11, 8)

                            If File.Exists(Path.GetDirectoryName(PatchSCPKG) + "\icon0.png") Then
                                Dispatcher.BeginInvoke(Sub() NewPS5Game.GameCoverSource = New BitmapImage(New Uri(Path.GetDirectoryName(PatchSCPKG) + "\icon0.png", UriKind.RelativeOrAbsolute)))
                            Else
                                If Utils.IsURLValid("https://prosperopatches.com/" + ParamData.TitleId.Trim()) Then
                                    URLs.Add("https://prosperopatches.com/" + ParamData.TitleId.Trim()) 'Get the image from prosperopatches
                                End If
                            End If

                        End If

                        Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                        Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading PKG " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + PKGCount.ToString())

                        'Add to the ListView
                        If PatchesListView.Dispatcher.CheckAccess() = False Then
                            PatchesListView.Dispatcher.BeginInvoke(Sub() PatchesListView.Items.Add(NewPS5Game))
                        Else
                            PatchesListView.Items.Add(NewPS5Game)
                        End If

                    End If

                End Using

            Next

        End If

    End Sub

    Private Sub GameLoaderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles GameLoaderWorker.RunWorkerCompleted
        If URLs.Count > 0 Then
            NewLoadingWindow.LoadStatusTextBlock.Text = "Getting " + URLs.Count.ToString() + " available covers"
            NewLoadingWindow.LoadProgressBar.Value = 0
            NewLoadingWindow.LoadProgressBar.Maximum = URLs.Count

            ContentWebView.Source = New Uri(URLs.Item(0))
        Else
            NewLoadingWindow.Close()
        End If
    End Sub

#End Region

    Private Sub LoadFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadFolderMenuItem.Click
        If Not String.IsNullOrEmpty(ConsoleIP) Then
            'Show the loading progress window
            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS5 files", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.IsIndeterminate = True
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading files, please wait ..."
            NewLoadingWindow.Show()

            'Load the files
            GameLoaderWorker.RunWorkerAsync(New GameLoaderArgs() With {.Type = LoadType.FTP, .FolderPath = String.Empty})
        Else
            MsgBox("Please enter your console's FTP IP address in the settings before continuing.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub LoadPKGFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadPKGFolderMenuItem.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Select your PS5 patches folder"}

        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            PKGCount = Directory.GetFiles(FBD.SelectedPath, "*_sc.pkg", SearchOption.AllDirectories).Count

            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS5 patches", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = PKGCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + PKGCount.ToString()
            NewLoadingWindow.Show()

            GameLoaderWorker.RunWorkerAsync(New GameLoaderArgs() With {.Type = LoadType.PKGs, .FolderPath = FBD.SelectedPath})
        End If
    End Sub

    Private Sub PS5MenuIPTextChanged(sender As Object, e As RoutedEventArgs)
        ConsoleIP = NewPS5Menu.SharedConsoleAddress.Split(":"c)(0)
        ConsolePort = NewPS5Menu.SharedConsoleAddress.Split(":"c)(1)
    End Sub

    Private Async Sub ContentWebView_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs) Handles ContentWebView.NavigationCompleted

        Dim GameCoverSource As String = String.Empty

        If e.IsSuccess And ContentWebView.Source.ToString.StartsWith("https://prosperopatches.com/") Then
            'Game ID
            Dim GameID As String = Await ContentWebView.ExecuteScriptAsync("document.getElementsByClassName('bd-links-group py-2')[0].innerText;")

            'Game Image
            Dim GameImageURL As String = Await ContentWebView.ExecuteScriptAsync("document.getElementsByClassName('game-icon secondary')[0].outerHTML;")
            Dim SplittedGameImageURL As String() = GameImageURL.Split(New String() {"(", ")"}, StringSplitOptions.None)

            If SplittedGameImageURL.Count > 0 And Not String.IsNullOrEmpty(GameID) Then
                GameID = GameID.Split(New String() {"\n"}, StringSplitOptions.None)(1).Replace("""", "")
                GameCoverSource = SplittedGameImageURL(1)
            End If

            If Not String.IsNullOrEmpty(GameCoverSource) And Not String.IsNullOrEmpty(GameID) Then
                For Each GamePatch In PatchesListView.Items
                    Dim FoundGamePatch As PS5Game = CType(GamePatch, PS5Game)

                    If FoundGamePatch.GameID.Contains(GameID) Or FoundGamePatch.GameID = GameID Then
                        FoundGamePatch.GameCoverSource = New BitmapImage(New Uri(GameCoverSource))
                        Exit For
                    End If
                Next
            End If

            If CurrentURL < URLs.Count Then
                ContentWebView.CoreWebView2.Navigate(URLs.Item(CurrentURL))
                CurrentURL += 1
                NewLoadingWindow.LoadProgressBar.Value = CurrentURL
            Else
                CurrentURL = 0
                URLs.Clear()
                NewLoadingWindow.Close()
                PatchesListView.Items.Refresh()
            End If
        End If

    End Sub

    Public Function GetDirSize(RootFolder As String) As Long
        Dim FolderInfo = New DirectoryInfo(RootFolder)
        For Each File In FolderInfo.GetFiles
            If Not File.Name.EndsWith("-merged.pkg") Then 'skip merged .pkg
                TotalSize += File.Length
            End If
        Next
        For Each SubFolderInfo In FolderInfo.GetDirectories : GetDirSize(SubFolderInfo.FullName)
        Next
        Return TotalSize
    End Function

End Class
