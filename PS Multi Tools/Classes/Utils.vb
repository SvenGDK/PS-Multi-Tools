Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.NetworkInformation
Imports System.Runtime.InteropServices
Imports System.Security.Principal
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports DiscUtils
Imports FluentFTP
Imports HtmlAgilityPack

Public Class Utils

    Public Shared ReadOnly ByBytes() As Byte = {&H62, &H79, &H20, &H53, &H76, &H65, &H6E, &H47, &H44, &H4B}
    Public Shared ReadOnly SpaceSeparator As String() = New String() {" "}

#Region "Library Music"

    <DllImport("winmm.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function PlaySound(<MarshalAs(UnmanagedType.LPWStr)> pszSound As String, hmod As IntPtr, fdwSound As Integer) As Boolean
    End Function
    <DllImport("winmm.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function PlaySound(<MarshalAs(UnmanagedType.LPArray)> pszSound As Byte(), hmod As IntPtr, fdwSound As PlaySoundFlags) As Boolean
    End Function

    Public Enum PlaySoundFlags As Integer
        SND_SYNC = 0
        SND_ASYNC = 1
        SND_NODEFAULT = 2
        SND_MEMORY = 4
        SND_LOOP = 8
        SND_NOSTOP = 16
        SND_NOWAIT = 8192
        SND_FILENAME = 131072
        SND_RESOURCE = 262148
    End Enum

    Public Shared Sub PlaySND(SoundFile As String)
        Dim SoundFileInfo As New FileInfo(SoundFile)
        PlaySound(SoundFileInfo.FullName, IntPtr.Zero, PlaySoundFlags.SND_ASYNC)
    End Sub

    Public Shared Sub PlaySND(SoundData As Byte())
        PlaySound(SoundData, IntPtr.Zero, PlaySoundFlags.SND_ASYNC Or PlaySoundFlags.SND_MEMORY)
    End Sub

    Public Shared Sub StopSND()
        'Set NULL to stop playing
        PlaySound(Nothing, New IntPtr(), PlaySoundFlags.SND_NODEFAULT)
    End Sub

    Public Shared Sub PlayGameSound(SoundFile As String)
        Using FFPlay As New Process()
            FFPlay.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\ffplay.exe"
            FFPlay.StartInfo.Arguments = "-nodisp -autoexit """ + SoundFile + """"
            FFPlay.StartInfo.UseShellExecute = False
            FFPlay.StartInfo.CreateNoWindow = True
            FFPlay.Start()
        End Using
    End Sub

    Public Shared Sub StopGameSound()
        For Each p As Process In Process.GetProcessesByName("ffplay")
            Try
                p.Kill()
                p.WaitForExit()
            Catch winException As Win32Exception
            Catch invalidException As InvalidOperationException
            End Try
        Next
    End Sub

#End Region

#Region "Converters"

    Public Shared Function HexStringToAscii(HexString As String, CleanEndOfString As Boolean) As String
        Dim ascii As String

        Try
            Dim str = ""

            While HexString.Length > 0
                str += Convert.ToChar(Convert.ToUInt32(HexString.Substring(0, 2), 16)).ToString()
                HexString = HexString.Substring(2)
            End While

            If CleanEndOfString Then str = str.Replace(vbNullChar, "")
            ascii = str
        Catch ex As Exception
            ascii = Nothing
        End Try

        Return ascii
    End Function

    Public Shared Function ByteArrayToAscii(ByteArray As Byte(), startPos As Integer, length As Integer, cleanEndOfString As Boolean) As String
        Dim numArray As Byte() = New Byte(length - 1 + 1 - 1) {}
        Array.Copy(ByteArray, startPos, numArray, 0, numArray.Length)
        Return HexStringToAscii(ByteArrayToHexString(numArray), cleanEndOfString)
    End Function

    Public Shared Function ByteArrayToHexString(ByteArray As Byte()) As String
        Dim hexString = ""
        Dim num As Integer = ByteArray.Length - 1
        Dim index = 0

        While index <= num
            hexString += ByteArray(index).ToString("X2")
            index += 1
        End While

        Return hexString
    End Function

    Public Shared Sub ConvertTo32bppAndDisposeOriginal(ByRef img As Bitmap)
        Dim bmp = New Bitmap(img.Width, img.Height, Imaging.PixelFormat.Format32bppArgb)

        Using gr = Graphics.FromImage(bmp)
            gr.DrawImage(img, New Rectangle(0, 0, 76, 108))
        End Using

        img.Dispose()
        img = bmp
    End Sub

    Public Shared Function ConvertTo24bppPNG(ImageToConvert As Image) As Bitmap
        Dim NewBitmap As New Bitmap(ImageToConvert.Width, ImageToConvert.Height, Imaging.PixelFormat.Format24bppRgb)
        Using NewGraphics As Graphics = Graphics.FromImage(NewBitmap)
            NewGraphics.DrawImage(ImageToConvert, New Rectangle(0, 0, ImageToConvert.Width, ImageToConvert.Height))
        End Using
        Return NewBitmap
    End Function

    Public Shared Function ToMemoryStream(BitmapImage As Bitmap) As MemoryStream
        Dim NewMemoryStream As New MemoryStream()
        BitmapImage.Save(NewMemoryStream, Imaging.ImageFormat.Png)
        Return NewMemoryStream
    End Function

#End Region

#Region "Image Processing"

    Public Shared Function BitmapSourceFromByteArray(buffer As Byte()) As BitmapSource
        Dim bitmap = New BitmapImage()

        Using stream = New MemoryStream(buffer)
            bitmap.BeginInit()
            bitmap.CacheOption = BitmapCacheOption.OnLoad
            bitmap.StreamSource = stream
            bitmap.EndInit()
        End Using

        bitmap.Freeze()
        Return bitmap
    End Function

    Public Shared Async Function GetResizedBitmap(ImageLocation As String, NewWidth As Integer, NewHeight As Integer) As Task(Of Bitmap)
        Using NewHttpClient As New HttpClient()
            Dim NewHttpResponseMessage As HttpResponseMessage = Await NewHttpClient.GetAsync(ImageLocation)
            If NewHttpResponseMessage.IsSuccessStatusCode Then
                Using NewStream As Stream = Await NewHttpResponseMessage.Content.ReadAsStreamAsync()
                    Dim OriginalBitmap As New Bitmap(NewStream)
                    Dim ResizedBitmap As New Bitmap(OriginalBitmap, New System.Drawing.Size(NewWidth, NewHeight))
                    Return ResizedBitmap
                End Using
            Else
                Return Nothing
            End If
        End Using
    End Function

    Public Shared Function ResizeAsImage(InputImage As Image, NewSizeX As Integer, NewSizeY As Integer) As Image
        Return New Bitmap(InputImage, New System.Drawing.Size(NewSizeX, NewSizeY))
    End Function

#End Region

#Region "PSX Related"

    Private Shared _ConnectedPSXHDD As Structures.MountedPSXDrive

    Public Shared Property ConnectedPSXHDD As Structures.MountedPSXDrive
        Get
            Return _ConnectedPSXHDD
        End Get
        Set
            _ConnectedPSXHDD = Value
        End Set
    End Property

    Public Enum DiscType
        CD
        DVD
    End Enum

    Public Shared Function GetDiscType(ISOFile As String) As DiscType
        Dim ISOFileSize As Double = New FileInfo(ISOFile).Length / 1048576

        If ISOFileSize > 700 Then
            Return DiscType.DVD
        Else
            Return DiscType.CD
        End If
    End Function

    Public Shared Function IsNBDConnected(WNBDClientPath As String) As String
        Dim ProcessOutput As String()
        Dim NBDDriveName As String = ""

        'List connected clients
        If Not String.IsNullOrEmpty(WNBDClientPath) Then
            Using WNBDClient As New Process()
                WNBDClient.StartInfo.FileName = WNBDClientPath
                WNBDClient.StartInfo.Arguments = "list"
                WNBDClient.StartInfo.RedirectStandardOutput = True
                WNBDClient.StartInfo.UseShellExecute = False
                WNBDClient.StartInfo.CreateNoWindow = True
                WNBDClient.Start()
                WNBDClient.WaitForExit()

                Dim OutputReader As StreamReader = WNBDClient.StandardOutput
                ProcessOutput = OutputReader.ReadToEnd().Split({vbCrLf}, StringSplitOptions.None)
            End Using

            For Each ReturnedLine As String In ProcessOutput
                If ReturnedLine.Contains("wnbd-client") Then
                    NBDDriveName = ReturnedLine.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries)(4).Trim()
                    Exit For
                End If
            Next
        End If

        If Not String.IsNullOrEmpty(NBDDriveName) Then
            Return NBDDriveName
        Else
            Return ""
        End If
    End Function

    Public Shared Function IsLocalHDDConnected() As String
        'Query the drives
        If File.Exists(Environment.CurrentDirectory + "\Tools\hdl_dump.exe") Then
            Using HDLDump As New Process()
                HDLDump.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\hdl_dump.exe"
                HDLDump.StartInfo.Arguments = "query"
                HDLDump.StartInfo.RedirectStandardOutput = True
                HDLDump.StartInfo.UseShellExecute = False
                HDLDump.StartInfo.CreateNoWindow = True
                HDLDump.Start()

                'Read the output
                Dim OutputReader As StreamReader = HDLDump.StandardOutput
                Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split({vbCrLf}, StringSplitOptions.None)

                Dim DriveHDLName As String = ""

                'Find the local drive
                For Each Line As String In ProcessOutput
                    If Not String.IsNullOrWhiteSpace(Line) Then
                        If Line.Contains("formatted Playstation 2 HDD") Then
                            'Set the found drive as mounted PSX drive
                            Dim DriveInfos As String() = Line.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries)
                            If DriveInfos(0) IsNot Nothing Then
                                DriveHDLName = DriveInfos(0).Trim()
                                Exit For
                            End If
                        End If
                    End If
                Next

                If Not String.IsNullOrWhiteSpace(DriveHDLName) Then
                    Return DriveHDLName
                Else
                    Return ""
                End If

            End Using
        Else
            Return ""
        End If
    End Function

    Public Shared Function GetConnectedNBDIP(WNBDClientPath As String, NBDDriveName As String) As String
        'Get the connected IP address
        If Not String.IsNullOrEmpty(WNBDClientPath) Then
            Dim ProcessOutput As String()
            Dim NBDIP As String = ""

            Using WNBDClient As New Process()
                WNBDClient.StartInfo.FileName = WNBDClientPath
                WNBDClient.StartInfo.Arguments = "show " + NBDDriveName
                WNBDClient.StartInfo.RedirectStandardOutput = True
                WNBDClient.StartInfo.UseShellExecute = False
                WNBDClient.StartInfo.CreateNoWindow = True
                WNBDClient.Start()
                WNBDClient.WaitForExit()

                Dim OutputReader As StreamReader = WNBDClient.StandardOutput
                ProcessOutput = OutputReader.ReadToEnd().Split({vbCrLf}, StringSplitOptions.None)
            End Using

            For Each ReturnedLine As String In ProcessOutput
                If ReturnedLine.Contains("Hostname") Then
                    NBDIP = ReturnedLine.Split(":"c)(1).Trim()
                    Exit For
                End If
            Next

            Return NBDIP
        Else
            Return ""
        End If
    End Function

    Public Shared Function GetHDLDriveName() As String
        If File.Exists(Environment.CurrentDirectory + "\Tools\hdl_dump.exe") Then
            Dim HDLDriveName As String = ""

            'Query the drives
            Using HDLDump As New Process()
                HDLDump.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\hdl_dump.exe"
                HDLDump.StartInfo.Arguments = "query"
                HDLDump.StartInfo.RedirectStandardOutput = True
                HDLDump.StartInfo.UseShellExecute = False
                HDLDump.StartInfo.CreateNoWindow = True
                HDLDump.Start()
                HDLDump.WaitForExit()

                'Read the output
                Dim OutputReader As StreamReader = HDLDump.StandardOutput
                Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split({vbCrLf}, StringSplitOptions.None)

                'Find the drive
                For Each Line As String In ProcessOutput
                    If Not String.IsNullOrWhiteSpace(Line) Then
                        If Line.Contains("formatted Playstation 2 HDD") Then
                            'Set the found drive as mounted PSX drive
                            Dim DriveInfos As String() = Line.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries)
                            HDLDriveName = DriveInfos(0).Trim()
                            Exit For
                        End If
                    End If
                Next
            End Using

            Return HDLDriveName
        Else
            Return ""
        End If
    End Function

    Public Shared Function GetHDDID() As String
        Dim DriveID As String = ""

        'Query the drives
        Using WMIC As New Process()
            WMIC.StartInfo.FileName = "wmic"
            WMIC.StartInfo.Arguments = "diskdrive get Caption,DeviceID"
            WMIC.StartInfo.RedirectStandardOutput = True
            WMIC.StartInfo.UseShellExecute = False
            WMIC.StartInfo.CreateNoWindow = True
            WMIC.Start()
            WMIC.WaitForExit()

            'Read the output
            Dim OutputReader As StreamReader = WMIC.StandardOutput
            Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split({vbCrLf}, StringSplitOptions.None)

            'Find the drive
            For Each Line As String In ProcessOutput
                If Not String.IsNullOrWhiteSpace(Line) Then
                    If Line.Contains("WNBD WNBD_DISK SCSI Disk Device") Then
                        DriveID = Line.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries)(5).Trim()
                        Exit For
                    ElseIf Line.Contains("Microsoft Virtual Disk") Then 'For testing with local VHD
                        DriveID = Line.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries)(3).Trim()
                        Exit For
                    End If
                End If
            Next
        End Using

        Return DriveID
    End Function

    Public Shared Sub ReloadProjects()
        For Each Win In System.Windows.Application.Current.Windows()
            If Win.ToString = "PS_Multi_Tools.PSXMainWindow" Then
                CType(Win, PSXMainWindow).ReloadProjects()
                Exit For
            End If
        Next
    End Sub

    Public Shared Sub ReloadPartitions()
        For Each Win In System.Windows.Application.Current.Windows()
            If Win.ToString = "PS_Multi_Tools.PSXPartitionManager" Then
                CType(Win, PSXPartitionManager).ReloadPartitions()
                Exit For
            End If
        Next
    End Sub

#End Region

    Public Shared Function IncrementArray(ByRef sourceArray As Byte(), position As Integer) As Boolean
        If sourceArray(position) = 255 Then
            If position <> 0 Then
                If IncrementArray(sourceArray, position - 1) Then
                    sourceArray(position) = 0
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If
        Else
            sourceArray(position) += CByte(1)
            Return True
        End If
    End Function

    Public Shared Function DirSize(sourceDir As String, recurse As Boolean) As Long
        Dim size As Long = 0
        Dim fileEntries As String() = Directory.GetFiles(sourceDir)

        For Each fileName As String In fileEntries
            Interlocked.Add(size, New FileInfo(fileName).Length)
        Next

        If recurse Then
            Dim subdirEntries As String() = Directory.GetDirectories(sourceDir)
            Parallel.[For](Of Long)(0, subdirEntries.Length, Function() 0, Function(i, [loop], subtotal)

                                                                               If (File.GetAttributes(subdirEntries(i)) And FileAttributes.ReparsePoint) <> FileAttributes.ReparsePoint Then
                                                                                   subtotal += DirSize(subdirEntries(i), True)
                                                                                   Return subtotal
                                                                               End If

                                                                               Return 0
                                                                           End Function, Function(x) Interlocked.Add(size, x))
        End If

        Return size
    End Function

    Public Shared Function GetFileSize(Size As Long) As String
        Dim DoubleBytes As Double
        Try
            Select Case Size
                Case Is >= 1099511627776
                    DoubleBytes = Size / 1099511627776 'TB
                    Return FormatNumber(DoubleBytes, 2) & " TB"
                Case 1073741824 To 1099511627775
                    DoubleBytes = Size / 1073741824 'GB
                    Return FormatNumber(DoubleBytes, 2) & " GB"
                Case 1048576 To 1073741823
                    DoubleBytes = Size / 1048576 'MB
                    Return FormatNumber(DoubleBytes, 2) & " MB"
                Case 1024 To 1048575
                    DoubleBytes = Size / 1024 'KB
                    Return FormatNumber(DoubleBytes, 2) & " KB"
                Case 0 To 1023
                    DoubleBytes = Size ' Bytes
                    Return FormatNumber(DoubleBytes, 2) & " Bytes"
                Case Else
                    Return ""
            End Select
        Catch
            Return ""
        End Try
    End Function

    Public Shared Function GetFileSizeAndDate(FileSize As String, TheDate As String) As Structures.PackageInfo
        Dim PKGSizeStr As Long
        Dim PKGDate As Date
        Dim NewPKGInfo As New Structures.PackageInfo()

        If Long.TryParse(FileSize.ToString.Trim, PKGSizeStr) Then
            NewPKGInfo.FileSize = GetFileSize(PKGSizeStr)
        Else
            NewPKGInfo.FileSize = FileSize
        End If
        If Date.TryParseExact(TheDate, "yyyy-MM-dd HH:mm:ss", Nothing, DateTimeStyles.None, PKGDate) Then
            NewPKGInfo.FileDate = CStr(PKGDate.Date)
        Else
            NewPKGInfo.FileDate = TheDate
        End If

        Return NewPKGInfo
    End Function

    Public Shared Function GetPKGTitleID(PKGFilePath As String) As String
        Dim PKGID As String = ""
        Try
            Dim NewStringBuilder As New StringBuilder
            If PKGFilePath.ToLower.EndsWith(".pkg") Then
                Using PKGBinaryReader As New BinaryReader(New StreamReader(PKGFilePath).BaseStream)
                    PKGBinaryReader.BaseStream.Position = &H30
                    Dim PKGBytes As Byte() = PKGBinaryReader.ReadBytes(36)
                    PKGBinaryReader.Close()
                    Dim str3 As String = Encoding.ASCII.GetString(PKGBytes)
                    If str3.Trim.Replace(ChrW(0), "").Length >= 7 Then
                        NewStringBuilder.AppendLine(str3.Substring(7, 9))
                    Else
                        NewStringBuilder.AppendLine("XXXX#####")
                    End If
                End Using
                Return NewStringBuilder.ToString.Trim()
            Else
                Return PKGID
            End If

        Catch ex As Exception
            Return PKGID
        End Try
    End Function

    Public Shared Sub ReCreateDirectoryStructure(SourceDirectory As String, TargetDirectory As String, Optional RootDirectory As String = "")
        If String.IsNullOrEmpty(RootDirectory) Then
            RootDirectory = SourceDirectory
        End If
        Dim AllFolders() As String = Directory.GetDirectories(SourceDirectory)
        For Each folder As String In AllFolders
            Directory.CreateDirectory(folder.Replace(RootDirectory, TargetDirectory))
            ReCreateDirectoryStructure(folder, TargetDirectory, RootDirectory)
        Next
    End Sub

    Public Shared Async Function IsURLValid(Url As String) As Task(Of Boolean)
        If NetworkInterface.GetIsNetworkAvailable Then
            Try
                Using client As New HttpClient()
                    Dim response As HttpResponseMessage = Await client.GetAsync(Url)

                    Select Case response.StatusCode
                        Case HttpStatusCode.OK, HttpStatusCode.Found
                            Return True
                        Case HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden,
                         HttpStatusCode.BadGateway, HttpStatusCode.BadRequest, HttpStatusCode.RequestTimeout,
                         HttpStatusCode.GatewayTimeout, HttpStatusCode.InternalServerError, HttpStatusCode.ServiceUnavailable
                            Return False
                        Case Else
                            Return False
                    End Select
                End Using
            Catch Ex As HttpRequestException
                Return False
            End Try
        Else
            Return False
        End If
    End Function

    Public Shared Function CleanTitle(Title As String) As String
        Return Title.Replace("¢", "").Replace("„", "").Replace("â", "").Replace("Â", "").Replace("Ô", "").Replace("Ê", "").Replace("ô", "").Replace("ê", "").Replace(",", "").Replace(";", "")
    End Function

    Public Shared Function FindScrollViewer(DepObj As DependencyObject) As ScrollViewer
        If TypeOf DepObj Is ScrollViewer Then Return TryCast(DepObj, ScrollViewer)

        For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(DepObj) - 1
            Dim FoundScrollViewer = FindScrollViewer(VisualTreeHelper.GetChild(DepObj, i))
            If FoundScrollViewer IsNot Nothing Then Return FoundScrollViewer
        Next

        Return Nothing
    End Function

    Public Shared Function IsRunningAsAdministrator() As Boolean
        Dim CurrentWindowsIdentity = WindowsIdentity.GetCurrent()
        Dim CurrentWindowsPrincipal = New WindowsPrincipal(CurrentWindowsIdentity)
        Return CurrentWindowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator)
    End Function

    Public Shared Sub RunAsAdministrator()
        Dim NewProcessStartInfo As New ProcessStartInfo With {
            .UseShellExecute = True,
            .WorkingDirectory = Environment.CurrentDirectory,
            .FileName = AppDomain.CurrentDomain.BaseDirectory + "PS Multi Tools.exe",
            .Verb = "runas"
        }

        Try
            Dim NewProcess As Process = Process.Start(NewProcessStartInfo)
        Catch ex As Win32Exception
            Return
        End Try

        System.Windows.Application.Current.Shutdown()
    End Sub

    Public Shared Function IsInt(Input As String) As Boolean
        Dim DigitOnly As New Regex("^\d+$")
        Return DigitOnly.Match(Input).Success
    End Function

    Public Shared Function IsHex(Input As String) As Boolean
        Return Regex.IsMatch(Input, "\A\b[0-9a-fA-F]+\b\Z")
    End Function

    Public Shared Sub UpdatePS5ParamEditor(UpdatedParams As PS5ParamClass.PS5Param)
        For Each OpenWin In System.Windows.Application.Current.Windows()
            If OpenWin.ToString = "PS_Multi_Tools.PS5ParamEditor" Then
                CType(OpenWin, PS5ParamEditor).CurrentParamJson = UpdatedParams
                Exit For
            End If
        Next
    End Sub

    Public Shared Sub UpdatePS5ManifestEditor(UpdatedParams As PS5ManifestClass.PS5Manifest)
        For Each OpenWin In System.Windows.Application.Current.Windows()
            If OpenWin.ToString = "PS_Multi_Tools.PS5ManifestEditor" Then
                CType(OpenWin, PS5ManifestEditor).CurrentManifestJson = UpdatedParams
                Exit For
            End If
        Next
    End Sub

    Public Shared Async Sub CheckForMissingFiles()
        If Not File.Exists("strings.exe") Then
            Try
                Using NewHttpClient As New HttpClient()
                    Dim NewHttpResponseMessage As HttpResponseMessage = Await NewHttpClient.GetAsync("http://X.X.X.X/strings.exe")
                    If NewHttpResponseMessage.IsSuccessStatusCode Then
                        Dim NewStream As Stream = Await NewHttpResponseMessage.Content.ReadAsStreamAsync()
                        Using NewFileStream As New FileStream("strings.exe", FileMode.Create)
                            NewStream.CopyTo(NewFileStream)
                        End Using
                    End If
                End Using
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
    End Sub

    Public Shared Function GetFilenameFromUrl(FileURL As Uri) As String
        Return FileURL.Segments(FileURL.Segments.Length - 1)
    End Function

    Public Shared Async Function WebFileSize(sURL As String) As Task(Of Double)
        Dim client As New HttpClient()
        Try
            Using request = New HttpRequestMessage(HttpMethod.Head, sURL)
                Using response = Await client.SendAsync(request)
                    If response.IsSuccessStatusCode AndAlso response.Content.Headers.ContentLength.HasValue Then
                        Return Math.Round(response.Content.Headers.ContentLength.Value / 1024 / 1024, 2)
                    Else
                        Return 0
                    End If
                End Using
            End Using
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Shared Async Function IsPSMultiToolsUpdateAvailable() As Task(Of Boolean)
        If Await IsURLValid("https://github.com/SvenGDK/PS-Multi-Tools/raw/main/LatestBuild.txt") Then
            Dim PSMTInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(Environment.CurrentDirectory + "\PS Multi Tools.exe")
            Dim CurrentPSMultiToolsVersion As String = PSMTInfo.FileVersion

            Using VerCheckClient As New HttpClient()
                Dim NewPSMultiToolsVersion As String = Await VerCheckClient.GetStringAsync("https://github.com/SvenGDK/PS-Multi-Tools/raw/main/LatestBuild.txt")

                If CurrentPSMultiToolsVersion < NewPSMultiToolsVersion Then
                    Return True
                Else
                    Return False
                End If
            End Using
        Else
            Return False
        End If
    End Function

    Public Shared Async Sub DownloadAndExecuteUpdater()
        If Not File.Exists(Environment.CurrentDirectory + "\PSMT-Update.exe") Then
            Using NewHttpClient As New HttpClient()
                Dim NewHttpResponseMessage As HttpResponseMessage = Await NewHttpClient.GetAsync("https://raw.githubusercontent.com/SvenGDK/PS-Multi-Tools/main/PSMT-Update.exe")
                If NewHttpResponseMessage.IsSuccessStatusCode Then

                    Dim NewStream As Stream = Await NewHttpResponseMessage.Content.ReadAsStreamAsync()
                    Using NewFileStream As New FileStream("PSMT-Update.exe", FileMode.Create)
                        NewStream.CopyTo(NewFileStream)
                    End Using

                    If MsgBox("Do you want to install the update now ?", MsgBoxStyle.YesNo, "Install Update") = MsgBoxResult.Yes Then
                        Process.Start(Environment.CurrentDirectory + "\PSMT-Update.exe")
                        System.Windows.Application.Current.Shutdown()
                    End If
                End If
            End Using
        Else
            Process.Start(Environment.CurrentDirectory + "\PSMT-Update.exe")
            System.Windows.Application.Current.Shutdown()
        End If
    End Sub

    Public Shared Async Function GetzRIF(PKGContentID As String) As Task(Of String)
        Dim DownloadsList As New List(Of Structures.Package)()
        If MsgBox("Load zRIF from the latest database ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Using NewWebClient As New HttpClient()
                Dim GamesList As String = Await NewWebClient.GetStringAsync("https://nopaystation.com/tsv/PSV_GAMES.tsv")
                Dim GamesListLines As String() = GamesList.Split(CChar(vbCrLf))
                For Each GameLine As String In GamesListLines.Skip(1)
                    Dim SplittedValues As String() = GameLine.Split(CChar(vbTab))
                    Dim AdditionalInfo As Structures.PackageInfo = GetFileSizeAndDate(SplittedValues(8).Trim(), SplittedValues(6).Trim())
                    Dim NewPackage As New Structures.Package() With {.PackageName = SplittedValues(2).Trim(),
                        .PackageURL = SplittedValues(3).Trim(),
                        .PackageTitleID = SplittedValues(0).Trim(),
                        .PackageContentID = SplittedValues(5).Trim(),
                        .PackagezRIF = SplittedValues(4).Trim(),
                        .PackageDate = AdditionalInfo.FileDate,
                        .PackageSize = AdditionalInfo.FileSize,
                        .PackageRegion = SplittedValues(1).Trim()}
                    If Not SplittedValues(3).Trim() = "MISSING" Then 'Only add available PKGs
                        DownloadsList.Add(NewPackage)
                    End If
                Next
            End Using
        Else 'Use local .tsv file
            If File.Exists(Environment.CurrentDirectory + "\Databases\PSV_GAMES.tsv") Then
                Dim FileReader As String() = File.ReadAllLines(Environment.CurrentDirectory + "\Databases\PSV_GAMES.tsv", Encoding.UTF8)
                For Each GameLine As String In FileReader.Skip(1) 'Skip 1st line in TSV
                    Dim SplittedValues As String() = GameLine.Split(CChar(vbTab))
                    Dim AdditionalInfo As Structures.PackageInfo = GetFileSizeAndDate(SplittedValues(8), SplittedValues(6))
                    Dim NewPackage As New Structures.Package() With {.PackageName = SplittedValues(2),
                        .PackageURL = SplittedValues(3),
                        .PackageTitleID = SplittedValues(0),
                        .PackageContentID = SplittedValues(5),
                        .PackagezRIF = SplittedValues(4),
                        .PackageDate = AdditionalInfo.FileDate,
                        .PackageSize = AdditionalInfo.FileSize,
                        .PackageRegion = SplittedValues(1)}
                    If Not SplittedValues(3) = "MISSING" Then 'Only add available PKGs
                        DownloadsList.Add(NewPackage)
                    End If
                Next
            Else
                MsgBox("Nothing available. Please add TSV files to the 'Databases' directory.", MsgBoxStyle.Exclamation, "Could not load list")
            End If
        End If

        Dim zRIFStr As String = ""
        'Check if we have a zRIF for the selected .pkg
        For Each AvailablePKG As Structures.Package In DownloadsList
            If AvailablePKG.PackageContentID = PKGContentID Then
                If AvailablePKG.PackagezRIF IsNot Nothing Then
                    zRIFStr = AvailablePKG.PackagezRIF
                    Exit For
                End If
            End If
        Next

        Return zRIFStr
    End Function

    Public Shared Function GetIntOnly(Value As String) As Integer
        Dim ReturnValue As String = String.Empty
        Dim MatchCol As MatchCollection = Regex.Matches(Value, "\d+")
        For Each m As Match In MatchCol
            ReturnValue += m.ToString()
        Next
        Return Convert.ToInt32(ReturnValue)
    End Function

    Public Shared Function GetScrollViewer(DepObj As DependencyObject) As DependencyObject
        If TypeOf DepObj Is ScrollViewer Then
            Return DepObj
        End If

        For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(DepObj) - 1
            Dim Child = VisualTreeHelper.GetChild(DepObj, i)
            Dim Result = GetScrollViewer(Child)

            If Result Is Nothing Then
                Continue For
            Else
                Return Result
            End If
        Next

        Return Nothing
    End Function

    Public Shared Function FindNextAvailableDriveLetter() As String
        Dim AlphabetCollection As New StringCollection()
        Dim LowerBound As Integer = Convert.ToInt16("a"c)
        Dim UpperBound As Integer = Convert.ToInt16("z"c)

        For i As Integer = LowerBound To UpperBound - 1
            Dim DriveLetter As Char = ChrW(i)
            AlphabetCollection.Add(DriveLetter.ToString())
        Next

        Dim Drives As DriveInfo() = DriveInfo.GetDrives()
        For Each Drive As DriveInfo In Drives
            AlphabetCollection.Remove(Drive.Name.Substring(0, 1).ToLower())
        Next

        If AlphabetCollection.Count > 0 Then
            Return AlphabetCollection(0)
        Else
            Throw New ApplicationException("No drive letter available.")
        End If
    End Function

    Public Shared Sub CopyDirectory(sourceDir As String, destinationDir As String, recursive As Boolean)
        Dim dir = New DirectoryInfo(sourceDir)
        If Not dir.Exists Then Throw New DirectoryNotFoundException($"Source directory not found: {dir.FullName}")
        Dim dirs As DirectoryInfo() = dir.GetDirectories()
        Directory.CreateDirectory(destinationDir)

        For Each file As FileInfo In dir.GetFiles()
            Dim targetFilePath As String = Path.Combine(destinationDir, file.Name)
            file.CopyTo(targetFilePath)
        Next

        If recursive Then
            For Each subDir As DirectoryInfo In dirs
                Dim newDestinationDir As String = Path.Combine(destinationDir, subDir.Name)
                CopyDirectory(subDir.FullName, newDestinationDir, True)
            Next
        End If
    End Sub

    Public Shared Function FileExistInISO(GameISOPath As String, FileToSearch As String) As Boolean
        Dim Exists As Boolean = False
        Try
            Using NewFileStream As New FileStream(GameISOPath, FileMode.Open, FileAccess.Read)
                Dim NewIso9660CDReader As New Iso9660.CDReader(NewFileStream, True)
                Try
                    NewIso9660CDReader.OpenFile(FileToSearch, FileMode.Open)
                    Exists = True
                Catch exception As Exception
                    Exists = False
                End Try
            End Using
        Catch exception1 As Exception
            Exists = False
        End Try
        Return Exists
    End Function

    Public Shared Function ExtractFileFromISO9660(ISOPath As String, FileName As String, DestinationPath As String) As String
        Dim OutputDestination As String = ""
        Dim DesinationDirectoryName As String = Path.GetDirectoryName(DestinationPath)
        If Not Directory.Exists(DesinationDirectoryName) Then
            Directory.CreateDirectory(DesinationDirectoryName)
        End If
        Try
            Using NewFileStream As New FileStream(ISOPath, FileMode.Open, FileAccess.Read)
                Dim NewIso9660CDReader As New Iso9660.CDReader(NewFileStream, True)
                Try
                    Dim NewSparseStream As Streams.SparseStream = NewIso9660CDReader.OpenFile(FileName, FileMode.Open)
                    Dim OutputFileStream As New FileStream(DestinationPath, FileMode.Create)
                    NewSparseStream.CopyTo(OutputFileStream)
                    OutputFileStream.Close()
                    OutputDestination = DestinationPath
                Catch ex As Exception
                    OutputDestination = ""
                End Try
            End Using
        Catch ex As Exception
            OutputDestination = ""
        End Try
        Return OutputDestination
    End Function

    Public Shared Function GetDKeyFromGameID(HTMLKeysDatabase As String, GameID As String) As String
        Dim NewHTMLDoc As New HtmlDocument()
        NewHTMLDoc.Load(HTMLKeysDatabase)

        Dim XPath As String = "//tr[td/a[normalize-space(text())='" & GameID & "']]"
        Dim FoundHtmlNode As HtmlNode = NewHTMLDoc.DocumentNode.SelectSingleNode(XPath)

        If FoundHtmlNode IsNot Nothing Then
            Dim AlltdNodes As HtmlNodeCollection = FoundHtmlNode.SelectNodes("td")
            If AlltdNodes IsNot Nothing AndAlso AlltdNodes.Count > 0 Then
                Dim GameDKey As String = AlltdNodes(AlltdNodes.Count - 1).InnerText.Trim()
                Return GameDKey
            Else
                Return String.Empty
            End If
        Else
            Return String.Empty
        End If
    End Function

    Public Shared Function GetRegionalTitleFromGameID(HTMLKeysDatabase As String, GameID As String) As String
        Dim NewHTMLDoc As New HtmlDocument()
        NewHTMLDoc.Load(HTMLKeysDatabase)

        Dim XPath As String = "//tr[td/a[normalize-space(text())='" & GameID & "']]"
        Dim FoundHtmlNode As HtmlNode = NewHTMLDoc.DocumentNode.SelectSingleNode(XPath)

        If FoundHtmlNode IsNot Nothing Then
            Dim tdNodes As HtmlNodeCollection = FoundHtmlNode.SelectNodes("td")
            If tdNodes IsNot Nothing AndAlso tdNodes.Count > 1 Then
                Dim FoundGameTitle As String = tdNodes(1).InnerText.Trim()
                Return Path.GetFileNameWithoutExtension(FoundGameTitle)
            Else
                Return String.Empty
            End If
        Else
            Return String.Empty
        End If
    End Function

    Public Shared Function RenameFolderUsingPowershell(InputFolderPath As String, NewFolderName As String) As Boolean
        Dim NewProcessStartInfo As New ProcessStartInfo("powershell.exe") With {
            .Arguments = $"-NoProfile -Command ""Rename-Item -Path '{InputFolderPath}' -NewName '{NewFolderName}'""",
            .Verb = "runas",
            .UseShellExecute = True
        }
        Dim NewPowershellProcess As Process = Process.Start(NewProcessStartInfo)

        If NewPowershellProcess Is Nothing Then
            Return False
        End If

        NewPowershellProcess.WaitForExit()

        Dim PowershellProcessExitCode As Integer = NewPowershellProcess.ExitCode
        Dim ParentInputFolder As String = Path.GetDirectoryName(InputFolderPath)
        Dim NewFolderPath As String = Path.Combine(ParentInputFolder, NewFolderName)
        Dim RenameSucceeded As Boolean = (PowershellProcessExitCode = 0) AndAlso (Not Directory.Exists(InputFolderPath)) AndAlso Directory.Exists(NewFolderPath)

        Return RenameSucceeded
    End Function

    Public Shared Function GetFTPDirectorySize(client As FtpClient, path As String) As Long
        Dim FolderSize As Long = 0

        For Each BackupFileEntry In client.GetListing(path)
            If BackupFileEntry.Type = FtpObjectType.File Then
                FolderSize += BackupFileEntry.Size
            ElseIf BackupFileEntry.Type = FtpObjectType.Directory Then
                FolderSize += GetFTPDirectorySize(client, BackupFileEntry.FullName)
            End If
        Next

        Return FolderSize
    End Function

End Class

Namespace INI

    ''' <summary>
    ''' Create a New INI file to store or load data
    ''' </summary>
    Public Class IniFile
        Public path As String

        <DllImport("kernel32", CharSet:=CharSet.Unicode)>
        Private Shared Function WritePrivateProfileString(section As String, key As String, val As String, filePath As String) As Long
        End Function

        <DllImport("kernel32", CharSet:=CharSet.Unicode)>
        Private Shared Function GetPrivateProfileString(section As String, key As String, def As String, retVal As StringBuilder, size As Integer, filePath As String) As Integer
        End Function

        ''' <summary>
        ''' INIFile Constructor.
        ''' </summary>
        ''' <PARAM name="INIPath"></PARAM>
        Public Sub New(INIPath As String)
            path = INIPath
        End Sub
        ''' <summary>
        ''' Write Data to the INI File
        ''' </summary>
        ''' <PARAM name="Section"></PARAM>
        ''' Section name
        ''' <PARAM name="Key"></PARAM>
        ''' Key Name
        ''' <PARAM name="Value"></PARAM>
        ''' Value Name
        Public Sub IniWriteValue(Section As String, Key As String, Value As String)
            WritePrivateProfileString(Section, Key, Value, path)
        End Sub

        ''' <summary>
        ''' Read Data Value From the Ini File
        ''' </summary>
        ''' <PARAM name="Section"></PARAM>
        ''' <PARAM name="Key"></PARAM>
        ''' <PARAM name="Path"></PARAM>
        ''' <returns></returns>
        Public Function IniReadValue(Section As String, Key As String) As String
            Dim temp As New StringBuilder(255)
            Dim i As Integer = GetPrivateProfileString(Section, Key, "", temp, 255, path)
            Return temp.ToString()

        End Function

    End Class

End Namespace