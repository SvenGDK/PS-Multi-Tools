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

    Public Enum LoadType
        LocalFolder
        FTP
        PKGs
    End Enum

    Public Structure GameLoaderArgs
        Private _Type As LoadType
        Private _FolderPath As String

        Public Property Type As LoadType
            Get
                Return _Type
            End Get
            Set
                _Type = Value
            End Set
        End Property

        Public Property FolderPath As String
            Get
                Return _FolderPath
            End Get
            Set
                _FolderPath = Value
            End Set
        End Property
    End Structure

    Public Structure MountedPSXDrive
        Private _HDLDriveName As String
        Private _NBDDriveName As String
        Private _DriveID As String

        Public Property DriveID As String
            Get
                Return _DriveID
            End Get
            Set
                _DriveID = Value
            End Set
        End Property

        Public Property HDLDriveName As String
            Get
                Return _HDLDriveName
            End Get
            Set
                _HDLDriveName = Value
            End Set
        End Property

        Public Property NBDDriveName As String
            Get
                Return _NBDDriveName
            End Get
            Set
                _NBDDriveName = Value
            End Set
        End Property
    End Structure

    Public Structure Partition
        Private _Type As String
        Private _Start As String
        Private _Parts As String
        Private _Size As String
        Private _Name As String

        Public Property Type As String
            Get
                Return _Type
            End Get
            Set
                _Type = Value
            End Set
        End Property

        Public Property Start As String
            Get
                Return _Start
            End Get
            Set
                _Start = Value
            End Set
        End Property

        Public Property Parts As String
            Get
                Return _Parts
            End Get
            Set
                _Parts = Value
            End Set
        End Property

        Public Property Size As String
            Get
                Return _Size
            End Get
            Set
                _Size = Value
            End Set
        End Property

        Public Property Name As String
            Get
                Return _Name
            End Get
            Set
                _Name = Value
            End Set
        End Property
    End Structure

    Public Structure GamePartition
        Private _Type As String
        Private _Size As String
        Private _Name As String
        Private _Flags As String
        Private _DMA As String
        Private _Startup As String

        Public Property Type As String
            Get
                Return _Type
            End Get
            Set
                _Type = Value
            End Set
        End Property

        Public Property Size As String
            Get
                Return _Size
            End Get
            Set
                _Size = Value
            End Set
        End Property

        Public Property Flags As String
            Get
                Return _Flags
            End Get
            Set
                _Flags = Value
            End Set
        End Property

        Public Property DMA As String
            Get
                Return _DMA
            End Get
            Set
                _DMA = Value
            End Set
        End Property

        Public Property Startup As String
            Get
                Return _Startup
            End Get
            Set
                _Startup = Value
            End Set
        End Property

        Public Property Name As String
            Get
                Return _Name
            End Get
            Set
                _Name = Value
            End Set
        End Property

    End Structure

End Class
