Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.NetworkInformation
Imports System.Runtime.InteropServices
Imports System.Security.Policy
Imports System.Security.Principal
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading

Public Class Utils

    Public Shared ReadOnly ByBytes() As Byte = {&H62, &H79, &H20, &H53, &H76, &H65, &H6E, &H47, &H44, &H4B}
    Public Shared ConnectedPSXHDD As Structures.MountedPSXDrive

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

    Public Shared Function HexStringToAscii(HexString As String, CleanEndOfString As Boolean) As String
        Dim ascii As String

        Try
            Dim str = ""

            While HexString.Length > 0
                str += Convert.ToChar(Convert.ToUInt32(HexString.Substring(0, 2), 16)).ToString()
                HexString = HexString.Substring(2, HexString.Length - 2)
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

    Public Shared Sub CreateWorkingDirectories()
        If Not Directory.Exists(".\Downloads") Then
            With Directory.CreateDirectory(".\Downloads")
                .CreateSubdirectory(".\exdata")
                .CreateSubdirectory(".\pkgs")
            End With
        End If
        If Not Directory.Exists(".\Extractions") Then
            Directory.CreateDirectory(".\Extractions")
        End If
        If Not Directory.Exists(".\Decryptions") Then
            Directory.CreateDirectory(".\Decryptions")
        End If
    End Sub

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
        Long.TryParse(FileSize.ToString.Trim, PKGSizeStr)

        Dim PKGDate As Date
        Date.TryParseExact(TheDate, "yyyy-MM-dd HH:mm:ss", Nothing, DateTimeStyles.None, PKGDate)

        Return New Structures.PackageInfo With {.FileSize = GetFileSize(PKGSizeStr), .FileDate = CStr(PKGDate.Date)}
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

    Public Shared Function GetBackupFolders(DestinationPath As String) As Structures.BackupFolders
        Dim DestinationBackupStructure As New Structures.BackupFolders()

        If Directory.Exists(DestinationPath + "GAMES") Then
            DestinationBackupStructure.IsGAMESPresent = True
        End If
        If Directory.Exists(DestinationPath + "GAMEZ") Then
            DestinationBackupStructure.IsGAMEZPresent = True
        End If
        If Directory.Exists(DestinationPath + "packages") Then
            DestinationBackupStructure.IspackagesPresent = True
        End If
        If Directory.Exists(DestinationPath + "exdata") Then
            DestinationBackupStructure.IsexdataPresent = True
        End If

        Return DestinationBackupStructure
    End Function

    Public Shared Function IsWindowOpen(WindowName As String) As Boolean
        Dim WinFound As Boolean = False
        For Each OpenWin In System.Windows.Application.Current.Windows()
            If OpenWin.ToString = "PS_Multi_Tools." + WindowName Then
                WinFound = True
                Return True
                Exit For
            Else
                WinFound = False
                Return False
            End If
        Next
        Return WinFound
    End Function

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

    Public Shared Function GetScaledImage(InputImage As Image, NewWidth As Integer, NewHeight As Integer) As Bitmap
        Dim ScaledImage As New Bitmap(NewWidth, NewHeight)

        Using gr As Graphics = Graphics.FromImage(ScaledImage)
            gr.SmoothingMode = SmoothingMode.HighQuality
            gr.InterpolationMode = InterpolationMode.HighQualityBicubic
            gr.PixelOffsetMode = PixelOffsetMode.HighQuality
            gr.DrawImage(InputImage, New Rectangle(0, 0, NewWidth, NewHeight))
        End Using

        Return ScaledImage
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
            Using NewHttpClient As New HttpClient()
                Dim NewHttpResponseMessage As HttpResponseMessage = Await NewHttpClient.GetAsync("http://X.X.X.X/strings.exe")
                If NewHttpResponseMessage.IsSuccessStatusCode Then
                    Dim NewStream As Stream = Await NewHttpResponseMessage.Content.ReadAsStreamAsync()
                    Using NewFileStream As New FileStream("strings.exe", FileMode.Create)
                        NewStream.CopyTo(NewFileStream)
                    End Using
                End If
            End Using
        End If
    End Sub

    Public Shared Function GetFilenameFromUrl(FileURL As Uri) As String
        Return FileURL.Segments(FileURL.Segments.Length - 1)
    End Function

    Public Shared Async Function WebFileSize(sURL As String) As Task(Of Double)
        Dim client As New HttpClient()
        Using request = New HttpRequestMessage(HttpMethod.Head, sURL)
            Using response = Await client.SendAsync(request)
                If response.IsSuccessStatusCode AndAlso response.Content.Headers.ContentLength.HasValue Then
                    Return Math.Round(response.Content.Headers.ContentLength.Value / 1024 / 1024, 2)
                Else
                    Return 0
                End If
            End Using
        End Using
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
                    NBDDriveName = ReturnedLine.Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries)(4).Trim()
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
                            Dim DriveInfos As String() = Line.Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries)
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
                            Dim DriveInfos As String() = Line.Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries)
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
                        DriveID = Line.Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries)(5).Trim()
                        Exit For
                    ElseIf Line.Contains("Microsoft Virtual Disk") Then 'For testing with local VHD
                        DriveID = Line.Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries)(3).Trim()
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

    Public Shared Function GetIntOnly(Value As String) As Integer
        Dim ReturnValue As String = String.Empty
        Dim MatchCol As MatchCollection = Regex.Matches(Value, "\d+")
        For Each m As Match In MatchCol
            ReturnValue += m.ToString()
        Next
        Return Convert.ToInt32(ReturnValue)
    End Function

    Public Shared Async Function GetResizedBitmap(ImageLocation As String, NewWidth As Integer, NewHeight As Integer) As Task(Of Bitmap)
        Using NewHttpClient As New HttpClient()
            Dim NewHttpResponseMessage As HttpResponseMessage = Await NewHttpClient.GetAsync(ImageLocation)
            If NewHttpResponseMessage.IsSuccessStatusCode Then
                Using NewStream As Stream = Await NewHttpResponseMessage.Content.ReadAsStreamAsync()
                    Dim OriginalBitmap As New Bitmap(NewStream)
                    Dim ResizedBitmap As New Bitmap(OriginalBitmap, New Size(NewWidth, NewHeight))
                    Return ResizedBitmap
                End Using
            Else
                Return Nothing
            End If
        End Using
    End Function

    Public Shared Sub ConvertTo32bppAndDisposeOriginal(ByRef img As Bitmap)
        Dim bmp = New Bitmap(img.Width, img.Height, Imaging.PixelFormat.Format32bppArgb)

        Using gr = Graphics.FromImage(bmp)
            gr.DrawImage(img, New Rectangle(0, 0, 76, 108))
        End Using

        img.Dispose()
        img = bmp
    End Sub

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

    Public Shared Function ResizeAsImage(InputImage As Image, NewSizeX As Integer, NewSizeY As Integer) As Image
        Return New Bitmap(InputImage, New Size(NewSizeX, NewSizeY))
    End Function

    Public Shared Function ConvertTo24bppPNG(ImageToConvert As Image) As Bitmap
        Dim NewBitmap As New Bitmap(ImageToConvert.Width, ImageToConvert.Height, Imaging.PixelFormat.Format24bppRgb)
        Using NewGraphics As Graphics = Graphics.FromImage(NewBitmap)
            NewGraphics.DrawImage(ImageToConvert, New Rectangle(0, 0, ImageToConvert.Width, ImageToConvert.Height))
        End Using
        Return NewBitmap
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

    Public Shared Function ToMemoryStream(BitmapImage As Bitmap) As MemoryStream
        Dim NewMemoryStream As New MemoryStream()
        BitmapImage.Save(NewMemoryStream, Imaging.ImageFormat.Png)
        Return NewMemoryStream
    End Function

End Class

Namespace INI

    ''' <summary>
    ''' Create a New INI file to store or load data
    ''' </summary>
    Public Class IniFile
        Public path As String

        <DllImport("kernel32")>
        Private Shared Function WritePrivateProfileString(section As String, key As String, val As String, filePath As String) As Long
        End Function

        <DllImport("kernel32")>
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