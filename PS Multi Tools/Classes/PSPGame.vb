Public Class PSPGame

    Private _GameTitle As String
    Private _GameID As String
    Private _GameSize As String
    Private _GameRegion As String
    Private _GameFilePath As String
    Private _GameFolderPath As String
    Private _GameFileType As GameFileTypes
    Private _GameAppVer As String
    Private _GameRequiredFW As String
    Private _GameCategory As String
    Private _GameCoverSource As ImageSource
    Private _GameBackgroundSource As ImageSource
    Private _GameBackgroundSoundFile As String

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

    Enum GameFileTypes
        Backup
        ISO
    End Enum

    Public Property GameFileType As GameFileTypes
        Get
            Return _GameFileType
        End Get
        Set
            _GameFileType = Value
        End Set
    End Property

    Public Property GameCategory As String
        Get
            Return _GameCategory
        End Get
        Set
            _GameCategory = Value
        End Set
    End Property

    Public Property GameRequiredFW As String
        Get
            Return _GameRequiredFW
        End Get
        Set
            _GameRequiredFW = Value
        End Set
    End Property

    Public Property GameAppVer As String
        Get
            Return _GameAppVer
        End Get
        Set
            _GameAppVer = Value
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

    Public Property GameBackgroundSource As ImageSource
        Get
            Return _GameBackgroundSource
        End Get
        Set
            _GameBackgroundSource = Value
        End Set
    End Property

    Public Property GameBackgroundSoundFile As String
        Get
            Return _GameBackgroundSoundFile
        End Get
        Set
            _GameBackgroundSoundFile = Value
        End Set
    End Property

    Public Shared Function GetCategory(SFOCategory As String) As String
        Select Case SFOCategory
            Case "UG"
                Return "UMD Disc Game"
            Case "PG"
                Return "Game Update"
            Case "EG"
                Return "PSP Remaster"
            Case "MA"
                Return "App"
            Case "ME"
                Return "PS1 Classic"
            Case "MS"
                Return "MemoryStick Save for Game&Apps"
            Case Else
                Return "Unknown"
        End Select
    End Function

    Public Shared Function GetGameRegion(GameID As String) As String
        If GameID.StartsWith("UCES") Then
            Return "Europe"
        ElseIf GameID.StartsWith("ULES") Then
            Return "Europe"
        ElseIf GameID.StartsWith("SLES") Then
            Return "Europe"
        ElseIf GameID.StartsWith("UCUS") Then
            Return "USA"
        ElseIf GameID.StartsWith("ULUS") Then
            Return "USA"
        ElseIf GameID.StartsWith("NPUG") Then
            Return "USA"
        ElseIf GameID.StartsWith("SLUS") Then
            Return "USA"
        ElseIf GameID.StartsWith("ULJS") Then
            Return "Japan"
        ElseIf GameID.StartsWith("UCJS") Then
            Return "Japan"
        Else
            Return ""
        End If
    End Function

End Class
