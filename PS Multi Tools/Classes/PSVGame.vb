Public Class PSVGame

    Private _GameTitle As String
    Private _GameID As String
    Private _GameSize As String
    Private _GameRegion As String
    Private _GameFilePath As String
    Private _GameFolderPath As String
    Private _GameFileType As GameFileTypes
    Private _GameCategory As String
    Private _GameRequiredFW As String
    Private _GameAppVer As String
    Private _ContentID As String
    Private _GameCoverSource As ImageSource
    Private _GameVer As String
    Private _GridWidth As Double
    Private _ImageHeight As Double
    Private _ImageWidth As Double
    Private _GridHeight As Double

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
        PKG
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

    Public Property GameVer As String
        Get
            Return _GameVer
        End Get
        Set
            _GameVer = Value
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

    Public Property ContentID As String
        Get
            Return _ContentID
        End Get
        Set
            _ContentID = Value
        End Set
    End Property

    Public Property GridWidth As Double
        Get
            Return _GridWidth
        End Get
        Set
            _GridWidth = Value
        End Set
    End Property

    Public Property GridHeight As Double
        Get
            Return _GridHeight
        End Get
        Set
            _GridHeight = Value
        End Set
    End Property

    Public Property ImageWidth As Double
        Get
            Return _ImageWidth
        End Get
        Set
            _ImageWidth = Value
        End Set
    End Property

    Public Property ImageHeight As Double
        Get
            Return _ImageHeight
        End Get
        Set
            _ImageHeight = Value
        End Set
    End Property

    Public Shared Function GetCategory(SFOCategory As String) As String
        Select Case SFOCategory
            Case "ac"
                Return "Additional Content"
            Case "gc"
                Return "Game Content"
            Case "gd"
                Return "Game Digital Application"
            Case "gda"
                Return "System Application"
            Case "gdb"
                Return "System Application"
            Case "gdc"
                Return "Non-Game Big Application"
            Case "gdd"
                Return "BG Application"
            Case "gp"
                Return "Game Patch"
            Case "gpc"
                Return "Non-Game Big App Patch"
            Case "gpd"
                Return "BG Application patch"
            Case "sd"
                Return "Save Data"
            Case Else
                Return "Unknown"
        End Select
    End Function

    Public Shared Function GetGameRegion(GameID As String) As String
        If GameID.StartsWith("PCSB") Then
            Return "Europe"
        ElseIf GameID.StartsWith("PCSF") Then
            Return "Europe"
        ElseIf GameID.StartsWith("PCSA") Then
            Return "USA"
        ElseIf GameID.StartsWith("PCSE") Then
            Return "USA"
        ElseIf GameID.StartsWith("PCSG") Then
            Return "Japan"
        ElseIf GameID.StartsWith("PCSC") Then
            Return "Japan"
        Else
            Return ""
        End If
    End Function

End Class
