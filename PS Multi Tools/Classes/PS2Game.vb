Imports System.IO

Public Class PS2Game

    Private _GameTitle As String
    Private _GameID As String
    Private _GameSize As String
    Private _GameRegion As String
    Private _GameFilePath As String
    Private _GameFolderPath As String
    Private _GameCoverSource As ImageSource
    Private _GameGenre As String
    Private _GameDescription As String
    Private _GameReleaseDate As String
    Private _GamePublisher As String
    Private _GameDeveloper As String
    Private _GameBackupType As GameFileType
    Private _GameWebsite As String
    Private _GameCoverURL As String
    Private _AssignedPartitionDriveLetter As String
    Private _PartitionName As String

    Public Enum GameFileType
        ISO
        CSO
    End Enum

    Public Property GameTitle As String
        Get
            Return _GameTitle
        End Get
        Set
            _GameTitle = Value
        End Set
    End Property

    Public Property GameID As String
        Get
            Return _GameID
        End Get
        Set
            _GameID = Value
        End Set
    End Property

    Public Property GameSize As String
        Get
            Return _GameSize
        End Get
        Set
            _GameSize = Value
        End Set
    End Property

    Public Property GameRegion As String
        Get
            Return _GameRegion
        End Get
        Set
            _GameRegion = Value
        End Set
    End Property

    Public Property GameFilePath As String
        Get
            Return _GameFilePath
        End Get
        Set
            _GameFilePath = Value
        End Set
    End Property

    Public Property GameFolderPath As String
        Get
            Return _GameFolderPath
        End Get
        Set
            _GameFolderPath = Value
        End Set
    End Property

    Public Property GameCoverSource As ImageSource
        Get
            Return _GameCoverSource
        End Get
        Set
            _GameCoverSource = Value
        End Set
    End Property

    Public Property GameGenre As String
        Get
            Return _GameGenre
        End Get
        Set
            _GameGenre = Value
        End Set
    End Property

    Public Property GameDeveloper As String
        Get
            Return _GameDeveloper
        End Get
        Set
            _GameDeveloper = Value
        End Set
    End Property

    Public Property GamePublisher As String
        Get
            Return _GamePublisher
        End Get
        Set
            _GamePublisher = Value
        End Set
    End Property

    Public Property GameReleaseDate As String
        Get
            Return _GameReleaseDate
        End Get
        Set
            _GameReleaseDate = Value
        End Set
    End Property

    Public Property GameDescription As String
        Get
            Return _GameDescription
        End Get
        Set
            _GameDescription = Value
        End Set
    End Property

    Public Property GameBackupType As GameFileType
        Get
            Return _GameBackupType
        End Get
        Set
            _GameBackupType = Value
        End Set
    End Property

    Public Property GameWebsite As String
        Get
            Return _GameWebsite
        End Get
        Set
            _GameWebsite = Value
        End Set
    End Property

    Public Property GameCoverURL As String
        Get
            Return _GameCoverURL
        End Get
        Set
            _GameCoverURL = Value
        End Set
    End Property

    Public Property AssignedPartitionDriveLetter As String
        Get
            Return _AssignedPartitionDriveLetter
        End Get
        Set
            _AssignedPartitionDriveLetter = Value
        End Set
    End Property

    Public Property PartitionName As String
        Get
            Return _PartitionName
        End Get
        Set
            _PartitionName = Value
        End Set
    End Property

    Public Shared Function GetGameRegionByGameID(GameID As String) As String
        If GameID.StartsWith("SLES", StringComparison.OrdinalIgnoreCase) Then
            Return "E"
        ElseIf GameID.StartsWith("SCES", StringComparison.OrdinalIgnoreCase) Then
            Return "E"
        ElseIf GameID.StartsWith("SLUS", StringComparison.OrdinalIgnoreCase) Then
            Return "U"
        ElseIf GameID.StartsWith("SCUS", StringComparison.OrdinalIgnoreCase) Then
            Return "U"
        ElseIf GameID.StartsWith("SCPS", StringComparison.OrdinalIgnoreCase) Then
            Return "J"
        ElseIf GameID.StartsWith("SLPS", StringComparison.OrdinalIgnoreCase) Then
            Return "J"
        ElseIf GameID.StartsWith("SLPM", StringComparison.OrdinalIgnoreCase) Then
            Return "J"
        ElseIf GameID.StartsWith("SCCS", StringComparison.OrdinalIgnoreCase) Then
            Return "J"
        ElseIf GameID.StartsWith("SLKA", StringComparison.OrdinalIgnoreCase) Then
            Return "J"
        Else
            Return ""
        End If
    End Function

    Public Shared Function GetPS2GameID(GameISO As String) As String
        Dim GameID As String = ""

        Using SevenZip As New Process()
            SevenZip.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\7z.exe"
            SevenZip.StartInfo.Arguments = "l -ba """ + GameISO + """"
            SevenZip.StartInfo.RedirectStandardOutput = True
            SevenZip.StartInfo.UseShellExecute = False
            SevenZip.StartInfo.CreateNoWindow = True
            SevenZip.Start()

            'Read the output
            Dim OutputReader As StreamReader = SevenZip.StandardOutput
            Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.None)

            If ProcessOutput.Length > 0 Then
                For Each Line As String In ProcessOutput
                    If Line.Contains("SLES_") Or Line.Contains("SLUS_") Or Line.Contains("SCES_") Or Line.Contains("SCUS_") Then
                        If Line.Contains("Volume:") Then 'ID found in the ISO Header
                            If Line.Split(New String() {"Volume: "}, StringSplitOptions.RemoveEmptyEntries).Length > 0 Then
                                GameID = Line.Split(New String() {"Volume: "}, StringSplitOptions.RemoveEmptyEntries)(1)
                                Exit For
                            End If
                        Else 'ID found in the ISO files
                            If String.Join(" ", Line.Split(New Char() {}, StringSplitOptions.RemoveEmptyEntries)).Split(" "c).Length > 4 Then
                                GameID = String.Join(" ", Line.Split(New Char() {}, StringSplitOptions.RemoveEmptyEntries)).Split(" "c)(5).Trim()
                                Exit For
                            End If
                        End If
                    End If
                Next
            End If

        End Using

        Return GameID
    End Function

    Public Shared Function GetPS2GameTitleFromDatabaseList(GameID As String) As String
        Dim FoundGameTitle As String = ""
        GameID = GameID.Replace("-", "")

        For Each GameTitle As String In File.ReadLines(Environment.CurrentDirectory + "\Tools\ps2ids.txt")
            If GameTitle.Contains(GameID) Then
                FoundGameTitle = GameTitle.Split(";"c)(1)
                Exit For
            End If
        Next

        If String.IsNullOrEmpty(FoundGameTitle) Then
            Return "Unknown PS2 game"
        Else
            Return FoundGameTitle
        End If
    End Function

End Class
