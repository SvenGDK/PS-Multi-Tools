Imports Newtonsoft.Json

Public Class Structures

    Public Structure PS3PKG
        Public FilePath As String
        Public SFOPath As String
        Public PKGType As String
        Public TitleID As String
        Public ContentID As String
    End Structure

    Public Structure PS3DLGame
        Public Property TitleID As String
        Public Property Region As String
        Public Property Name As String
        Public Property PKGLink As String
        Public Property RAP As String
        Public Property ContentID As String
        Public Property LastModificationDate As String
        Public Property RAPFFileRequired As String
        Public Property FileSize As String
        Public Property SHA256 As String
    End Structure

    Public Structure Package
        Private _PackageName As String
        Private _PackageDescription As String
        Private _PackageURL As String
        Private _PackageContentID As String
        Private _PackageRAP As String
        Private _PackageRegion As String
        Private _PackageDate As String
        Private _PackageDLCs As String
        Private _PackageSize As String
        Private _IsSelected As Boolean
        Private _PackagezRIF As String
        Private _PackageReqFW As String
        Private _PackageTitleID As String

        Public Property PackageName As String
            Get
                Return _PackageName
            End Get
            Set
                _PackageName = Value
            End Set
        End Property

        Public Property PackageDescription As String
            Get
                Return _PackageDescription
            End Get
            Set
                _PackageDescription = Value
            End Set
        End Property

        Public Property PackageURL As String
            Get
                Return _PackageURL
            End Get
            Set
                _PackageURL = Value
            End Set
        End Property

        Public Property PackageTitleID As String
            Get
                Return _PackageTitleID
            End Get
            Set
                _PackageTitleID = Value
            End Set
        End Property

        Public Property PackageContentID As String
            Get
                Return _PackageContentID
            End Get
            Set
                _PackageContentID = Value
            End Set
        End Property

        Public Property PackageRAP As String
            Get
                Return _PackageRAP
            End Get
            Set
                _PackageRAP = Value
            End Set
        End Property

        Public Property PackagezRIF As String
            Get
                Return _PackagezRIF
            End Get
            Set
                _PackagezRIF = Value
            End Set
        End Property

        Public Property PackageReqFW As String
            Get
                Return _PackageReqFW
            End Get
            Set
                _PackageReqFW = Value
            End Set
        End Property

        Public Property PackageRegion As String
            Get
                Return _PackageRegion
            End Get
            Set
                _PackageRegion = Value
            End Set
        End Property

        Public Property PackageDate As String
            Get
                Return _PackageDate
            End Get
            Set
                _PackageDate = Value
            End Set
        End Property

        Public Property PackageDLCs As String
            Get
                Return _PackageDLCs
            End Get
            Set
                _PackageDLCs = Value
            End Set
        End Property

        Public Property PackageSize As String
            Get
                Return _PackageSize
            End Get
            Set
                _PackageSize = Value
            End Set
        End Property

        Public Property IsSelected As Boolean
            Get
                Return _IsSelected
            End Get
            Set
                _IsSelected = Value
            End Set
        End Property

    End Structure

    Public Structure PackageInfo
        Public Property FileSize As String
        Public Property FileDate As String
    End Structure

    Public Structure CopyItem
        Public Property FileName As String
        Public Property FileSize As String
    End Structure

    Public Structure BackupFolders
        Public Property IsGAMESPresent As Boolean
        Public Property IsGAMEZPresent As Boolean
        Public Property IsexdataPresent As Boolean
        Public Property IspackagesPresent As Boolean
    End Structure

    Public Structure ExtractionPKGProcess
        Public Property DecryptedPKGFileName As String
        Public Property EncryptedPKGFileName As String
        Public Property PKGFileName As String
    End Structure

    Public Structure ExtractionWorkerProgress
        Public Property FileCount As Integer
        Public Property FileName As String
    End Structure

    Public Structure ParamSFOContent
        Public Property ParamName As String
        Public Property ParamValue As Object
        Public Property ParamLenght As Integer
        Public Property ParamType As Integer
        Public Property ParamLocation As Integer
    End Structure

    Public Structure StorePageInfos

        Private _name As String
        Private _category As String
        Private _description As String
        Private _sku As String
        Private _image As String
        Private _price As String
        Private _priceCurrency As String

        Public Property name As String
            Get
                Return _name
            End Get
            Set
                _name = Value
            End Set
        End Property

        Public Property category As String
            Get
                Return _category
            End Get
            Set
                _category = Value
            End Set
        End Property

        Public Property description As String
            Get
                Return _description
            End Get
            Set
                _description = Value
            End Set
        End Property

        Public Property sku As String
            Get
                Return _sku
            End Get
            Set
                _sku = Value
            End Set
        End Property

        Public Property image As String
            Get
                Return _image
            End Get
            Set
                _image = Value
            End Set
        End Property

    End Structure

    Public Class Addcont

        <JsonProperty("serviceIdForSharing")>
        Public Property ServiceIdForSharing As String()
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

    Public Class EnGB

        <JsonProperty("titleName")>
        Public Property TitleName As String
    End Class

    Public Class DeDE

        <JsonProperty("titleName")>
        Public Property TitleName As String
    End Class

    Public Class FrFR

        <JsonProperty("titleName")>
        Public Property TitleName As String
    End Class

    Public Class LocalizedParameters

        <JsonProperty("defaultLanguage")>
        Public Property DefaultLanguage As String

        <JsonProperty("en-US")>
        Public Property EnUS As EnUS

        <JsonProperty("en-GB")>
        Public Property EnGB As EnGB

        <JsonProperty("de-DE")>
        Public Property DeDE As DeDE

        <JsonProperty("fr-FR")>
        Public Property FrFR As FrFR
    End Class

    Public Class Pubtools

        <JsonProperty("creationDate")>
        Public Property CreationDate As String

        <JsonProperty("submission")>
        Public Property Submission As Boolean

        <JsonProperty("toolVersion")>
        Public Property ToolVersion As String
    End Class

    Public Class PS5GameParamJson

        <JsonProperty("addcont")>
        Public Property Addcont As Addcont

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

        <JsonProperty("pubtools")>
        Public Property Pubtools As Pubtools

        <JsonProperty("requiredSystemSoftwareVersion")>
        Public Property RequiredSystemSoftwareVersion As String

        <JsonProperty("sdkVersion")>
        Public Property SdkVersion As String

        <JsonProperty("titleId")>
        Public Property TitleId As String

        <JsonProperty("userDefinedParam1")>
        Public Property UserDefinedParam1 As Integer

        <JsonProperty("versionFileUri")>
        Public Property VersionFileUri As String
    End Class

    Public Class AppParamJson

        <JsonProperty("applicationCategoryType")>
        Public Property ApplicationCategoryType As Integer

        <JsonProperty("contentId")>
        Public Property ContentId As String

        <JsonProperty("titleId")>
        Public Property TitleId As String

        <JsonProperty("attribute")>
        Public Property Attribute As Integer

        <JsonProperty("attribute2")>
        Public Property Attribute2 As Integer

        <JsonProperty("attribute3")>
        Public Property Attribute3 As Integer

        <JsonProperty("downloadDataSize")>
        Public Property DownloadDataSize As Integer

        <JsonProperty("localizedParameters")>
        Public Property LocalizedParameters As LocalizedParameters
    End Class

    Public Enum AssetType
        Video
        Audio
        Image
        Font
    End Enum

    Public Structure AssetListViewItem
        Private _AssetFileName As String
        Private _AssetFilePath As String
        Private _Type As AssetType
        Private _Icon As ImageSource

        Public Property AssetFileName As String
            Get
                Return _AssetFileName
            End Get
            Set
                _AssetFileName = Value
            End Set
        End Property

        Public Property AssetFilePath As String
            Get
                Return _AssetFilePath
            End Get
            Set
                _AssetFilePath = Value
            End Set
        End Property

        Public Property Type As AssetType
            Get
                Return _Type
            End Get
            Set
                _Type = Value
            End Set
        End Property

        Public Property Icon As ImageSource
            Get
                Return _Icon
            End Get
            Set
                _Icon = Value
            End Set
        End Property

    End Structure

End Class
