Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Runtime.InteropServices
Imports System.Security.Principal
Imports System.Text
Imports System.Threading

Public Class Utils

    Public Declare Auto Function PlaySound Lib "winmm.dll" (pszSound As String, hmod As IntPtr, fdwSound As Integer) As Boolean
    Public Declare Auto Function PlaySound Lib "winmm.dll" (pszSound As Byte(), hmod As IntPtr, fdwSound As PlaySoundFlags) As Boolean

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
            FFPlay.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\ffplay.exe"
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
        For Each OpenWin In Windows.Application.Current.Windows()
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

    Public Shared Function IsURLValid(Url As String) As Boolean
        If NetworkInterface.GetIsNetworkAvailable Then
            Try
                Dim request As HttpWebRequest = CType(WebRequest.Create(Url), HttpWebRequest)
                Using response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
                    If response.StatusCode = HttpStatusCode.OK Then
                        Return True
                    ElseIf response.StatusCode = HttpStatusCode.Found Then
                        Return True
                    ElseIf response.StatusCode = HttpStatusCode.NotFound Then
                        Return False
                    ElseIf response.StatusCode = HttpStatusCode.Unauthorized Then
                        Return False
                    ElseIf response.StatusCode = HttpStatusCode.Forbidden Then
                        Return False
                    ElseIf response.StatusCode = HttpStatusCode.BadGateway Then
                        Return False
                    ElseIf response.StatusCode = HttpStatusCode.BadRequest Then
                        Return False
                    ElseIf response.StatusCode = HttpStatusCode.RequestTimeout Then
                        Return False
                    ElseIf response.StatusCode = HttpStatusCode.GatewayTimeout Then
                        Return False
                    ElseIf response.StatusCode = HttpStatusCode.InternalServerError Then
                        Return False
                    ElseIf response.StatusCode = HttpStatusCode.ServiceUnavailable Then
                        Return False
                    Else
                        Return False
                    End If
                End Using
            Catch Ex As WebException
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

        Windows.Application.Current.Shutdown()
    End Sub

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