Public Class PS4Game

    Private _GameTitle As String
    Private _GameID As String
    Private _GameSize As String
    Private _GameRegion As String
    Private _GameFilePath As String
    Private _GameFolderPath As String
    Private _GameCategory As String
    Private _IsGameSelected As Visibility
    Private _GameCoverSource As ImageSource
    Private _GameAppVer As String
    Private _GameVer As String
    Private _GameRequiredFW As String
    Private _GameContentID As String
    Private _GameSoundtrackBytes As Byte()

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

    Public Property GameContentID As String
        Get
            Return _GameContentID
        End Get
        Set
            _GameContentID = Value
        End Set
    End Property

    Public Property IsGameSelected As Visibility
        Get
            Return _IsGameSelected
        End Get
        Set
            _IsGameSelected = Value
        End Set
    End Property

    Public Property GameSoundtrackBytes As Byte()
        Get
            Return _GameSoundtrackBytes
        End Get
        Set
            _GameSoundtrackBytes = Value
        End Set
    End Property

    Public Shared Function GetCategory(SFOCategory As String) As String
        Select Case SFOCategory
            Case "ac"
                Return "Additional Content"
            Case "bd"
                Return "Blu-ray Disc"
            Case "gc"
                Return "Game Content"
            Case "gd"
                Return "Game Digital Application"
            Case "gda"
                Return "System Application"
            Case "gdb"
                Return "Unknown"
            Case "gdc"
                Return "Non-Game Big Application"
            Case "gdd"
                Return "BG Application"
            Case "gde"
                Return "Non-Game Mini App / Video Service Native App"
            Case "gdk"
                Return "Video Service Web App"
            Case "gdl"
                Return "PS Cloud Beta App"
            Case "gdO"
                Return "PS2 Classic"
            Case "gp"
                Return "Game Application Patch"
            Case "gpc"
                Return "Non-Game Big App Patch"
            Case "gpd"
                Return "BG Application patch"
            Case "gpe"
                Return "Non-Game Mini App Patch / Video Service Native App Patch"
            Case "gpk"
                Return "Video Service Web App Patch"
            Case "gpl"
                Return "PS Cloud Beta App Patch"
            Case "sd"
                Return "Save Data"
            Case "la"
                Return "Live Area"
            Case "wda"
                Return "Unknown"
            Case Else
                Return "Unknown"
        End Select
    End Function

    Public Shared Function GetGameRegion(ContentID As String) As String
        If ContentID.StartsWith("EP") Then
            Return "Europe"
        ElseIf ContentID.StartsWith("UP") Then
            Return "USA"
        ElseIf ContentID.StartsWith("JP") Then
            Return "Japan"
        Else
            Return ""
        End If
    End Function

End Class
