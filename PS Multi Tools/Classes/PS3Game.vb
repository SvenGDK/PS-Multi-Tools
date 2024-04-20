Imports System.Text

Public Class PS3Game

    Private _GameTitle As String
    Private _GameID As String
    Private _GameSize As String
    Private _GameRegion As String
    Private _GameFilePath As String
    Private _GameFolderPath As String
    Private _GameCategory As String
    Private _GameRequiredFW As String
    Private _GameAppVer As String
    Private _GameVer As String
    Private _GameCoverSource As ImageSource
    Private _GameBackgroundSource As ImageSource
    Private _GameBackgroundSoundFile As String
    Private _PKGType As String
    Private _ContentID As String
    Private _GameFileType As GameFileTypes
    Private _GameBackgroundSoundBytes As Byte()
    Private _GameResolution As String
    Private _GameSoundFormat As String
    Private _GameBackgroundPath As String
    Private _GridWidth As Double
    Private _GridHeight As Double
    Private _ImageWidth As Double
    Private _ImageHeight As Double
    Private _GameRootLocation As GameLocation

    Public Enum GameFileTypes
        Backup
        PKG
        PS3ISO
        PS2ISO
        PSXISO
        PSPISO
    End Enum

    Public Enum GameLocation
        WebMANMOD
        Local
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

    Public Property GameBackgroundSource As ImageSource
        Get
            Return _GameBackgroundSource
        End Get
        Set
            _GameBackgroundSource = Value
        End Set
    End Property

    Public Property GameBackgroundPath As String
        Get
            Return _GameBackgroundPath
        End Get
        Set
            _GameBackgroundPath = Value
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

    Public Property GameBackgroundSoundBytes As Byte()
        Get
            Return _GameBackgroundSoundBytes
        End Get
        Set
            _GameBackgroundSoundBytes = Value
        End Set
    End Property

    Public Property PKGType As String
        Get
            Return _PKGType
        End Get
        Set
            _PKGType = Value
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

    Public Property GameResolution As String
        Get
            Return _GameResolution
        End Get
        Set
            _GameResolution = Value
        End Set
    End Property

    Public Property GameSoundFormat As String
        Get
            Return _GameSoundFormat
        End Get
        Set
            _GameSoundFormat = Value
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

    Public Property GameRootLocation As GameLocation
        Get
            Return _GameRootLocation
        End Get
        Set
            _GameRootLocation = Value
        End Set
    End Property

#Region "Functions"

    Public Shared Function GetCategory(SFOCategory As String) As String
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

    Public Shared Function GetGameRegion(GameID As String) As String
        If GameID.StartsWith("BLES") Then
            Return "Europe"
        ElseIf GameID.StartsWith("BCES") Then
            Return "Europe"
        ElseIf GameID.StartsWith("NPEB") Then
            Return "Europe"
        ElseIf GameID.StartsWith("BLUS") Then
            Return "USA"
        ElseIf GameID.StartsWith("BCUS") Then
            Return "USA"
        ElseIf GameID.StartsWith("NPUB") Then
            Return "USA"
        ElseIf GameID.StartsWith("BCJS") Then
            Return "Japan"
        ElseIf GameID.StartsWith("BLJS") Then
            Return "Japan"
        ElseIf GameID.StartsWith("NPJB") Then
            Return "Japan"
        ElseIf GameID.StartsWith("BCAS") Then
            Return "Asia"
        ElseIf GameID.StartsWith("BLAS") Then
            Return "Asia"
        Else
            Return ""
        End If
    End Function

    Public Shared Function GetGameResolution(SFOResolution As String) As String
        Dim ResolutionValue As Long = CLng(Math.Round(Val(SFOResolution)))
        Dim SupportedResolutions As New List(Of String)()

        If (ResolutionValue And 1) = 1 Then
            SupportedResolutions.Add("480p")
        End If
        If (ResolutionValue And 2) = 2 Then
            SupportedResolutions.Add("576p")
        End If
        If (ResolutionValue And 4) = 4 Then
            SupportedResolutions.Add("720p")
        End If
        If (ResolutionValue And 8) = 8 Then
            SupportedResolutions.Add("1080p")
        End If
        If (ResolutionValue And 16) = 16 Then
            SupportedResolutions.Add("480p (16:9)")
        End If
        If (ResolutionValue And 32) = 32 Then
            SupportedResolutions.Add("576p (16:9)")
        End If
        If (ResolutionValue And 63) = 63 Then
            SupportedResolutions.Add("All video modes supported")
        End If

        Dim NewStringBuilder As New StringBuilder
        For Each SupportedResolution In SupportedResolutions
            NewStringBuilder.Append(SupportedResolution + vbCrLf)
        Next

        Return "Supported Resolutions: " + vbCrLf + NewStringBuilder.ToString()
    End Function

    Public Shared Function GetGameSoundFormat(SFOSoundFormat As String) As String
        Dim SoundValue As Long = CLng(Math.Round(Val(SFOSoundFormat)))
        Dim SupportedSoundFormats As New List(Of String)()

        If (SoundValue And 1) = 1 Then
            SupportedSoundFormats.Add("2.0 LPCM")
        End If
        If (SoundValue And 2) = 4 Then
            SupportedSoundFormats.Add("5.1 LPCM")
        End If
        If (SoundValue And 8) = 16 Then
            SupportedSoundFormats.Add("7.1 LPCM")
        End If
        If (SoundValue And 21) = 21 Then
            SupportedSoundFormats.Add("All LPCM modes")
        End If
        If (SoundValue And 256) = 256 Then
            SupportedSoundFormats.Add("Dolby Digital 5.1")
        End If
        If (SoundValue And 258) = 258 Then
            SupportedSoundFormats.Add("Only Dolby Digital 5.1")
        End If
        If (SoundValue And 279) = 279 Then
            SupportedSoundFormats.Add("All LPCM modes + Dolby Digital 5.1")
        End If
        If (SoundValue And 512) = 512 Then
            SupportedSoundFormats.Add("DTS 5.1")
        End If
        If (SoundValue And 514) = 514 Then
            SupportedSoundFormats.Add("Only DTS 5.1")
        End If
        If (SoundValue And 791) = 791 Then
            SupportedSoundFormats.Add("All sound modes")
        End If

        Dim NewStringBuilder As New StringBuilder
        For Each SupportedSoundFormat In SupportedSoundFormats
            NewStringBuilder.Append(SupportedSoundFormat + vbCrLf)
        Next

        Return "Supported Sound Formats: " + vbCrLf + NewStringBuilder.ToString()
    End Function

#End Region

End Class
