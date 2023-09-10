Imports Newtonsoft.Json

Public Class PS5Game

    Private _GameTitle As String
    Private _GameID As String
    Private _GameSize As String
    Private _GameRegion As String
    Private _GameFilePath As String
    Private _GameCoverSource As ImageSource
    Private _GameBGSource As ImageSource
    Private _GameContentID As String
    Private _GameCategory As String
    Private _GameVersion As String
    Private _GameRequiredFirmware As String

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

Public Class PS5PKGParamJSON

    Public Class Addcont

        <JsonProperty("serviceIdForSharing")>
        Public Property ServiceIdForSharing As String()
    End Class

    Public Class AgeLevel

        <JsonProperty("AE")>
        Public Property AE As Integer

        <JsonProperty("AR")>
        Public Property AR As Integer

        <JsonProperty("AT")>
        Public Property AT As Integer

        <JsonProperty("AU")>
        Public Property AU As Integer

        <JsonProperty("BE")>
        Public Property BE As Integer

        <JsonProperty("BG")>
        Public Property BG As Integer

        <JsonProperty("BH")>
        Public Property BH As Integer

        <JsonProperty("BO")>
        Public Property BO As Integer

        <JsonProperty("BR")>
        Public Property BR As Integer

        <JsonProperty("CA")>
        Public Property CA As Integer

        <JsonProperty("CH")>
        Public Property CH As Integer

        <JsonProperty("CL")>
        Public Property CL As Integer

        <JsonProperty("CN")>
        Public Property CN As Integer

        <JsonProperty("CO")>
        Public Property CO As Integer

        <JsonProperty("CR")>
        Public Property CR As Integer

        <JsonProperty("CY")>
        Public Property CY As Integer

        <JsonProperty("CZ")>
        Public Property CZ As Integer

        <JsonProperty("DE")>
        Public Property DE As Integer

        <JsonProperty("DK")>
        Public Property DK As Integer

        <JsonProperty("EC")>
        Public Property EC As Integer

        <JsonProperty("ES")>
        Public Property ES As Integer

        <JsonProperty("FI")>
        Public Property FI As Integer

        <JsonProperty("FR")>
        Public Property FR As Integer

        <JsonProperty("GB")>
        Public Property GB As Integer

        <JsonProperty("GR")>
        Public Property GR As Integer

        <JsonProperty("GT")>
        Public Property GT As Integer

        <JsonProperty("HK")>
        Public Property HK As Integer

        <JsonProperty("HN")>
        Public Property HN As Integer

        <JsonProperty("HR")>
        Public Property HR As Integer

        <JsonProperty("HU")>
        Public Property HU As Integer

        <JsonProperty("ID")>
        Public Property ID As Integer

        <JsonProperty("IE")>
        Public Property IE As Integer

        <JsonProperty("IL")>
        Public Property IL As Integer

        <JsonProperty("IN")>
        Public Property Indonesia As Integer

        <JsonProperty("IS")>
        Public Property Island As Integer

        <JsonProperty("IT")>
        Public Property IT As Integer

        <JsonProperty("JP")>
        Public Property JP As Integer

        <JsonProperty("KR")>
        Public Property KR As Integer

        <JsonProperty("KW")>
        Public Property KW As Integer

        <JsonProperty("LB")>
        Public Property LB As Integer

        <JsonProperty("LU")>
        Public Property LU As Integer

        <JsonProperty("MT")>
        Public Property MT As Integer

        <JsonProperty("MX")>
        Public Property MX As Integer

        <JsonProperty("MY")>
        Public Property MY As Integer

        <JsonProperty("NI")>
        Public Property NI As Integer

        <JsonProperty("NL")>
        Public Property NL As Integer

        <JsonProperty("NO")>
        Public Property NO As Integer

        <JsonProperty("NZ")>
        Public Property NZ As Integer

        <JsonProperty("OM")>
        Public Property OM As Integer

        <JsonProperty("PA")>
        Public Property PA As Integer

        <JsonProperty("PE")>
        Public Property PE As Integer

        <JsonProperty("PL")>
        Public Property PL As Integer

        <JsonProperty("PT")>
        Public Property PT As Integer

        <JsonProperty("PY")>
        Public Property PY As Integer

        <JsonProperty("QA")>
        Public Property QA As Integer

        <JsonProperty("RO")>
        Public Property RO As Integer

        <JsonProperty("RU")>
        Public Property RU As Integer

        <JsonProperty("SA")>
        Public Property SA As Integer

        <JsonProperty("SE")>
        Public Property SE As Integer

        <JsonProperty("SG")>
        Public Property SG As Integer

        <JsonProperty("SI")>
        Public Property SI As Integer

        <JsonProperty("SK")>
        Public Property SK As Integer

        <JsonProperty("SV")>
        Public Property SV As Integer

        <JsonProperty("TH")>
        Public Property TH As Integer

        <JsonProperty("TR")>
        Public Property TR As Integer

        <JsonProperty("TW")>
        Public Property TW As Integer

        <JsonProperty("UA")>
        Public Property UA As Integer

        <JsonProperty("US")>
        Public Property US As Integer

        <JsonProperty("UY")>
        Public Property UY As Integer

        <JsonProperty("ZA")>
        Public Property ZA As Integer

        <JsonProperty("default")>
        Public Property [Default] As Integer
    End Class

    Public Class PermittedIntent

        <JsonProperty("intentType")>
        Public Property IntentType As String
    End Class

    Public Class GameIntent

        <JsonProperty("permittedIntents")>
        Public Property PermittedIntents As PermittedIntent()
    End Class

    Public Class EnUS

        <JsonProperty("titleName")>
        Public Property TitleName As String
    End Class

    Public Class JaJP

        <JsonProperty("titleName")>
        Public Property TitleName As String
    End Class

    Public Class KoKR

        <JsonProperty("titleName")>
        Public Property TitleName As String
    End Class

    Public Class ZhHans

        <JsonProperty("titleName")>
        Public Property TitleName As String
    End Class

    Public Class ZhHant

        <JsonProperty("titleName")>
        Public Property TitleName As String
    End Class

    Public Class LocalizedParameters

        <JsonProperty("defaultLanguage")>
        Public Property DefaultLanguage As String

        <JsonProperty("en-US")>
        Public Property EnUS As EnUS

        <JsonProperty("ja-JP")>
        Public Property JaJP As JaJP

        <JsonProperty("ko-KR")>
        Public Property KoKR As KoKR

        <JsonProperty("zh-Hans")>
        Public Property ZhHans As ZhHans

        <JsonProperty("zh-Hant")>
        Public Property ZhHant As ZhHant
    End Class

    Public Class Pubtools

        <JsonProperty("creationDate")>
        Public Property CreationDate As String

        <JsonProperty("loudnessSnd0")>
        Public Property LoudnessSnd0 As String

        <JsonProperty("submission")>
        Public Property Submission As Boolean

        <JsonProperty("toolVersion")>
        Public Property ToolVersion As String
    End Class

    Public Class Savedata

        <JsonProperty("titleIdForTransferringPs4")>
        Public Property TitleIdForTransferringPs4 As String()
    End Class

    Public Class Share

        <JsonProperty("overlay_position")>
        Public Property OverlayPosition As String
    End Class

    Public Class PS5PKGParam

        <JsonProperty("addcont")>
        Public Property Addcont As Addcont

        <JsonProperty("ageLevel")>
        Public Property AgeLevel As AgeLevel

        <JsonProperty("applicationCategoryType")>
        Public Property ApplicationCategoryType As Integer

        <JsonProperty("applicationDrmType")>
        Public Property ApplicationDrmType As String

        <JsonProperty("attribute")>
        Public Property Attribute As Integer

        <JsonProperty("attribute2")>
        Public Property Attribute2 As Integer

        <JsonProperty("attribute3")>
        Public Property Attribute3 As Integer

        <JsonProperty("conceptId")>
        Public Property ConceptId As String

        <JsonProperty("contentBadgeType")>
        Public Property ContentBadgeType As Integer

        <JsonProperty("contentId")>
        Public Property ContentId As String

        <JsonProperty("contentVersion")>
        Public Property ContentVersion As String

        <JsonProperty("downloadDataSize")>
        Public Property DownloadDataSize As Integer

        <JsonProperty("gameIntent")>
        Public Property GameIntent As GameIntent

        <JsonProperty("localizedParameters")>
        Public Property LocalizedParameters As LocalizedParameters

        <JsonProperty("masterVersion")>
        Public Property MasterVersion As String

        <JsonProperty("originContentVersion")>
        Public Property OriginContentVersion As String

        <JsonProperty("pubtools")>
        Public Property Pubtools As Pubtools

        <JsonProperty("requiredSystemSoftwareVersion")>
        Public Property RequiredSystemSoftwareVersion As String

        <JsonProperty("savedata")>
        Public Property Savedata As Savedata

        <JsonProperty("sdkVersion")>
        Public Property SdkVersion As String

        <JsonProperty("share")>
        Public Property Share As Share

        <JsonProperty("targetContentVersion")>
        Public Property TargetContentVersion As String

        <JsonProperty("titleId")>
        Public Property TitleId As String

        <JsonProperty("userDefinedParam1")>
        Public Property UserDefinedParam1 As Integer

        <JsonProperty("userDefinedParam2")>
        Public Property UserDefinedParam2 As Integer

        <JsonProperty("userDefinedParam3")>
        Public Property UserDefinedParam3 As Integer

        <JsonProperty("userDefinedParam4")>
        Public Property UserDefinedParam4 As Integer

        <JsonProperty("versionFileUri")>
        Public Property VersionFileUri As String
    End Class

End Class