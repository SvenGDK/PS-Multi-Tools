Public Class PS5Game

    Private _GameTitle As String
    Private _GameID As String
    Private _GameSize As String
    Private _GameRegion As String
    Private _GameFileOrFolderPath As String
    Private _GameCoverSource As ImageSource
    Private _GameBGSource As ImageSource
    Private _GameContentID As String
    Private _GameCategory As String
    Private _GameVersion As String
    Private _GameRequiredFirmware As String
    Private _DEGameTitle As String
    Private _JPGameTitle As String
    Private _ESGameTitle As String
    Private _ITGameTitle As String
    Private _FRGameTitle As String
    Private _GameBackgroundImageBrush As ImageBrush
    Private _GameSoundFile As String
    Private _IsCompatibleFW As String 'Not used yet
    Private _DecFilesIncluded As String

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

    Public Property GameFileOrFolderPath As String
        Get
            Return _GameFileOrFolderPath
        End Get
        Set
            _GameFileOrFolderPath = Value
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

    Public Property GameBGSource As ImageSource
        Get
            Return _GameBGSource
        End Get
        Set
            _GameBGSource = Value
        End Set
    End Property

    Public Property GameBackgroundImageBrush As ImageBrush
        Get
            Return _GameBackgroundImageBrush
        End Get
        Set
            _GameBackgroundImageBrush = Value
        End Set
    End Property

    Public Property GameContentID As String
        Get
            Return _GameContentID
        End Get
        Set
            _GameContentID = Value
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

    Public Property GameVersion As String
        Get
            Return _GameVersion
        End Get
        Set
            _GameVersion = Value
        End Set
    End Property

    Public Property GameRequiredFirmware As String
        Get
            Return _GameRequiredFirmware
        End Get
        Set
            _GameRequiredFirmware = Value
        End Set
    End Property

    Public Property GameSoundFile As String
        Get
            Return _GameSoundFile
        End Get
        Set
            _GameSoundFile = Value
        End Set
    End Property

    Public Property DEGameTitle As String
        Get
            Return _DEGameTitle
        End Get
        Set
            _DEGameTitle = Value
        End Set
    End Property

    Public Property FRGameTitle As String
        Get
            Return _FRGameTitle
        End Get
        Set
            _FRGameTitle = Value
        End Set
    End Property

    Public Property ITGameTitle As String
        Get
            Return _ITGameTitle
        End Get
        Set
            _ITGameTitle = Value
        End Set
    End Property

    Public Property ESGameTitle As String
        Get
            Return _ESGameTitle
        End Get
        Set
            _ESGameTitle = Value
        End Set
    End Property

    Public Property JPGameTitle As String
        Get
            Return _JPGameTitle
        End Get
        Set
            _JPGameTitle = Value
        End Set
    End Property

    Public Property IsCompatibleFW As String
        Get
            Return _IsCompatibleFW
        End Get
        Set
            _IsCompatibleFW = Value
        End Set
    End Property

    Public Property DecFilesIncluded As String
        Get
            Return _DecFilesIncluded
        End Get
        Set
            _DecFilesIncluded = Value
        End Set
    End Property

    Public Shared Function GetGameRegion(GameID As String) As String
        If GameID.StartsWith("PPSA") Then
            Return "NA / Europe"
        ElseIf GameID.StartsWith("ECAS") Then
            Return "Asia"
        ElseIf GameID.StartsWith("ELAS") Then
            Return "Asia"
        ElseIf GameID.StartsWith("ELJM") Then
            Return "Japan"
        Else
            Return ""
        End If
    End Function

End Class
