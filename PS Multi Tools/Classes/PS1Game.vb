Public Class PS1Game

    Private _GameTitle As String
    Private _GameID As String
    Private _GameSize As String
    Private _GameRegion As String
    Private _GameFilePath As String
    Private _GameFolderPath As String
    Private _GameCoverSource As ImageSource

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

End Class
