Public Class PS5Game

    Private _GameTitle As String
    Private _GameID As String
    Private _GameSize As String
    Private _GameRegion As String
    Private _GameFilePath As String
    Private _GameCoverSource As ImageSource
    Private _GameBGSource As ImageSource
    Private _GameContentID As String
    Private _GameCatergory As String

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

    Public Property GameContentID As String
        Get
            Return _GameContentID
        End Get
        Set
            _GameContentID = Value
        End Set
    End Property

    Public Property GameCatergory As String
        Get
            Return _GameCatergory
        End Get
        Set
            _GameCatergory = Value
        End Set
    End Property

End Class
