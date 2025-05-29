Imports Newtonsoft.Json

Public Class Structures

    Public Structure PackageInfo
        Public Property FileSize As String
        Public Property FileDate As String
    End Structure

    Public Structure CopyItem
        Public Property FileName As String
        Public Property FileSize As String
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

    Public Enum AssetType
        Video
        Audio
        Image
        Font
        DIC
        XML
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

    Public Structure GP5ChunkFilesFolderListViewItem
        Private _SourcePath As String
        Private _DestinationPath As String
        Private _ChunkType As String

        Public Property ChunkType As String
            Get
                Return _ChunkType
            End Get
            Set
                _ChunkType = Value
            End Set
        End Property

        Public Property SourcePath As String
            Get
                Return _SourcePath
            End Get
            Set
                _SourcePath = Value
            End Set
        End Property

        Public Property DestinationPath As String
            Get
                Return _DestinationPath
            End Get
            Set
                _DestinationPath = Value
            End Set
        End Property
    End Structure

    Public Structure PKGBuilderProject
        Private _ProjectPath As String
        Private _ProjectCategory As String
        Private _ProjectIcon As ImageSource
        Private _ProjectTitle As String
        Private _ProjectBackground As ImageSource
        Private _ProjectSoundtrack As String
        Private _ProjectURL As String
        Private _GP5Created As Boolean

        Public Property ProjectPath As String
            Get
                Return _ProjectPath
            End Get
            Set
                _ProjectPath = Value
            End Set
        End Property

        Public Property ProjectCategory As String
            Get
                Return _ProjectCategory
            End Get
            Set
                _ProjectCategory = Value
            End Set
        End Property

        Public Property ProjectIcon As ImageSource
            Get
                Return _ProjectIcon
            End Get
            Set
                _ProjectIcon = Value
            End Set
        End Property

        Public Property ProjectTitle As String
            Get
                Return _ProjectTitle
            End Get
            Set
                _ProjectTitle = Value
            End Set
        End Property

        Public Property ProjectBackground As ImageSource
            Get
                Return _ProjectBackground
            End Get
            Set
                _ProjectBackground = Value
            End Set
        End Property

        Public Property ProjectSoundtrack As String
            Get
                Return _ProjectSoundtrack
            End Get
            Set
                _ProjectSoundtrack = Value
            End Set
        End Property

        Public Property ProjectURL As String
            Get
                Return _ProjectURL
            End Get
            Set
                _ProjectURL = Value
            End Set
        End Property

        Public Property GP5Created As Boolean
            Get
                Return _GP5Created
            End Get
            Set
                _GP5Created = Value
            End Set
        End Property
    End Structure

    Public Structure PS5GameLoaderArgs
        Private _FolderPath As String
        Private _LoadIcons As Boolean
        Private _LoadBackgrounds As Boolean
        Private _SkipFileChecks As Boolean

        Public Property FolderPath As String
            Get
                Return _FolderPath
            End Get
            Set
                _FolderPath = Value
            End Set
        End Property

        Public Property LoadIcons As Boolean
            Get
                Return _LoadIcons
            End Get
            Set
                _LoadIcons = Value
            End Set
        End Property

        Public Property LoadBackgrounds As Boolean
            Get
                Return _LoadBackgrounds
            End Get
            Set
                _LoadBackgrounds = Value
            End Set
        End Property

        Public Property SkipFileChecks As Boolean
            Get
                Return _SkipFileChecks
            End Get
            Set
                _SkipFileChecks = Value
            End Set
        End Property
    End Structure

    Public Structure FTPListViewItem
        Private _FileOrDirName As String
        Private _FileOrDirType As String
        Private _FileOrDirLastModified As String
        Private _FileOrDirPermissions As String
        Private _FileOrDirOwner As String
        Private _FileOrDirSize As String

        Public Property FileOrDirName As String
            Get
                Return _FileOrDirName
            End Get
            Set
                _FileOrDirName = Value
            End Set
        End Property

        Public Property FileOrDirType As String
            Get
                Return _FileOrDirType
            End Get
            Set
                _FileOrDirType = Value
            End Set
        End Property

        Public Property FileOrDirSize As String
            Get
                Return _FileOrDirSize
            End Get
            Set
                _FileOrDirSize = Value
            End Set
        End Property

        Public Property FileOrDirLastModified As String
            Get
                Return _FileOrDirLastModified
            End Get
            Set
                _FileOrDirLastModified = Value
            End Set
        End Property

        Public Property FileOrDirPermissions As String
            Get
                Return _FileOrDirPermissions
            End Get
            Set
                _FileOrDirPermissions = Value
            End Set
        End Property

        Public Property FileOrDirOwner As String
            Get
                Return _FileOrDirOwner
            End Get
            Set
                _FileOrDirOwner = Value
            End Set
        End Property
    End Structure

End Class

Public Class PS5ParamClass
    Public Class AgeLevel
        Private _US As Integer
        Private _Default As Integer
        Private _AE As Integer
        Private _AR As Integer
        Private _AT As Integer
        Private _AU As Integer
        Private _BE As Integer
        Private _BG As Integer
        Private _BH As Integer
        Private _BO As Integer
        Private _BR As Integer
        Private _CA As Integer
        Private _CH As Integer
        Private _CL As Integer
        Private _CN As Integer
        Private _CO As Integer
        Private _CR As Integer
        Private _CY As Integer
        Private _CZ As Integer
        Private _DE As Integer
        Private _DK As Integer
        Private _EC As Integer
        Private _ES As Integer
        Private _FI As Integer
        Private _FR As Integer
        Private _GB As Integer
        Private _GR As Integer
        Private _GT As Integer
        Private _HK As Integer
        Private _HN As Integer
        Private _HR As Integer
        Private _HU As Integer
        Private _ID As Integer
        Private _IE As Integer
        Private _IL As Integer
        Private _IT As Integer
        Private _JP As Integer
        Private _KR As Integer
        Private _KW As Integer
        Private _LB As Integer
        Private _LU As Integer
        Private _MT As Integer
        Private _MX As Integer
        Private _MY As Integer
        Private _NI As Integer
        Private _NL As Integer
        Private _NO As Integer
        Private _NZ As Integer
        Private _OM As Integer
        Private _PA As Integer
        Private _PE As Integer
        Private _PL As Integer
        Private _PT As Integer
        Private _PY As Integer
        Private _QA As Integer
        Private _RO As Integer
        Private _RU As Integer
        Private _SA As Integer
        Private _SE As Integer
        Private _SG As Integer
        Private _SI As Integer
        Private _SK As Integer
        Private _SV As Integer
        Private _TH As Integer
        Private _TR As Integer
        Private _TW As Integer
        Private _UA As Integer
        Private _UY As Integer
        Private _ZA As Integer
        Private _India As Integer
        Private _Iceland As Integer

        <JsonProperty("US")>
        Public Property US As Integer
            Get
                Return _US
            End Get
            Set
                _US = Value
            End Set
        End Property

        <JsonProperty("AE")>
        Public Property AE As Integer
            Get
                Return _AE
            End Get
            Set
                _AE = Value
            End Set
        End Property

        <JsonProperty("AR")>
        Public Property AR As Integer
            Get
                Return _AR
            End Get
            Set
                _AR = Value
            End Set
        End Property

        <JsonProperty("AT")>
        Public Property AT As Integer
            Get
                Return _AT
            End Get
            Set
                _AT = Value
            End Set
        End Property

        <JsonProperty("AU")>
        Public Property AU As Integer
            Get
                Return _AU
            End Get
            Set
                _AU = Value
            End Set
        End Property

        <JsonProperty("BE")>
        Public Property BE As Integer
            Get
                Return _BE
            End Get
            Set
                _BE = Value
            End Set
        End Property

        <JsonProperty("BG")>
        Public Property BG As Integer
            Get
                Return _BG
            End Get
            Set
                _BG = Value
            End Set
        End Property

        <JsonProperty("BH")>
        Public Property BH As Integer
            Get
                Return _BH
            End Get
            Set
                _BH = Value
            End Set
        End Property

        <JsonProperty("BO")>
        Public Property BO As Integer
            Get
                Return _BO
            End Get
            Set
                _BO = Value
            End Set
        End Property

        <JsonProperty("BR")>
        Public Property BR As Integer
            Get
                Return _BR
            End Get
            Set
                _BR = Value
            End Set
        End Property

        <JsonProperty("CA")>
        Public Property CA As Integer
            Get
                Return _CA
            End Get
            Set
                _CA = Value
            End Set
        End Property

        <JsonProperty("CH")>
        Public Property CH As Integer
            Get
                Return _CH
            End Get
            Set
                _CH = Value
            End Set
        End Property

        <JsonProperty("CL")>
        Public Property CL As Integer
            Get
                Return _CL
            End Get
            Set
                _CL = Value
            End Set
        End Property

        <JsonProperty("CN")>
        Public Property CN As Integer
            Get
                Return _CN
            End Get
            Set
                _CN = Value
            End Set
        End Property

        <JsonProperty("CO")>
        Public Property CO As Integer
            Get
                Return _CO
            End Get
            Set
                _CO = Value
            End Set
        End Property

        <JsonProperty("CR")>
        Public Property CR As Integer
            Get
                Return _CR
            End Get
            Set
                _CR = Value
            End Set
        End Property

        <JsonProperty("CY")>
        Public Property CY As Integer
            Get
                Return _CY
            End Get
            Set
                _CY = Value
            End Set
        End Property

        <JsonProperty("CZ")>
        Public Property CZ As Integer
            Get
                Return _CZ
            End Get
            Set
                _CZ = Value
            End Set
        End Property

        <JsonProperty("DE")>
        Public Property DE As Integer
            Get
                Return _DE
            End Get
            Set
                _DE = Value
            End Set
        End Property

        <JsonProperty("DK")>
        Public Property DK As Integer
            Get
                Return _DK
            End Get
            Set
                _DK = Value
            End Set
        End Property

        <JsonProperty("EC")>
        Public Property EC As Integer
            Get
                Return _EC
            End Get
            Set
                _EC = Value
            End Set
        End Property

        <JsonProperty("ES")>
        Public Property ES As Integer
            Get
                Return _ES
            End Get
            Set
                _ES = Value
            End Set
        End Property

        <JsonProperty("FI")>
        Public Property FI As Integer
            Get
                Return _FI
            End Get
            Set
                _FI = Value
            End Set
        End Property

        <JsonProperty("FR")>
        Public Property FR As Integer
            Get
                Return _FR
            End Get
            Set
                _FR = Value
            End Set
        End Property

        <JsonProperty("GB")>
        Public Property GB As Integer
            Get
                Return _GB
            End Get
            Set
                _GB = Value
            End Set
        End Property

        <JsonProperty("GR")>
        Public Property GR As Integer
            Get
                Return _GR
            End Get
            Set
                _GR = Value
            End Set
        End Property

        <JsonProperty("GT")>
        Public Property GT As Integer
            Get
                Return _GT
            End Get
            Set
                _GT = Value
            End Set
        End Property

        <JsonProperty("HK")>
        Public Property HK As Integer
            Get
                Return _HK
            End Get
            Set
                _HK = Value
            End Set
        End Property

        <JsonProperty("HN")>
        Public Property HN As Integer
            Get
                Return _HN
            End Get
            Set
                _HN = Value
            End Set
        End Property

        <JsonProperty("HR")>
        Public Property HR As Integer
            Get
                Return _HR
            End Get
            Set
                _HR = Value
            End Set
        End Property

        <JsonProperty("HU")>
        Public Property HU As Integer
            Get
                Return _HU
            End Get
            Set
                _HU = Value
            End Set
        End Property

        <JsonProperty("ID")>
        Public Property ID As Integer
            Get
                Return _ID
            End Get
            Set
                _ID = Value
            End Set
        End Property

        <JsonProperty("IE")>
        Public Property IE As Integer
            Get
                Return _IE
            End Get
            Set
                _IE = Value
            End Set
        End Property

        <JsonProperty("IL")>
        Public Property IL As Integer
            Get
                Return _IL
            End Get
            Set
                _IL = Value
            End Set
        End Property

        <JsonProperty("IN")>
        Public Property India As Integer
            Get
                Return _India
            End Get
            Set
                _India = Value
            End Set
        End Property

        <JsonProperty("IS")>
        Public Property Iceland As Integer
            Get
                Return _Iceland
            End Get
            Set
                _Iceland = Value
            End Set
        End Property

        <JsonProperty("IT")>
        Public Property IT As Integer
            Get
                Return _IT
            End Get
            Set
                _IT = Value
            End Set
        End Property

        <JsonProperty("JP")>
        Public Property JP As Integer
            Get
                Return _JP
            End Get
            Set
                _JP = Value
            End Set
        End Property

        <JsonProperty("KR")>
        Public Property KR As Integer
            Get
                Return _KR
            End Get
            Set
                _KR = Value
            End Set
        End Property

        <JsonProperty("KW")>
        Public Property KW As Integer
            Get
                Return _KW
            End Get
            Set
                _KW = Value
            End Set
        End Property

        <JsonProperty("LB")>
        Public Property LB As Integer
            Get
                Return _LB
            End Get
            Set
                _LB = Value
            End Set
        End Property

        <JsonProperty("LU")>
        Public Property LU As Integer
            Get
                Return _LU
            End Get
            Set
                _LU = Value
            End Set
        End Property

        <JsonProperty("MT")>
        Public Property MT As Integer
            Get
                Return _MT
            End Get
            Set
                _MT = Value
            End Set
        End Property

        <JsonProperty("MX")>
        Public Property MX As Integer
            Get
                Return _MX
            End Get
            Set
                _MX = Value
            End Set
        End Property

        <JsonProperty("MY")>
        Public Property MY As Integer
            Get
                Return _MY
            End Get
            Set
                _MY = Value
            End Set
        End Property

        <JsonProperty("NI")>
        Public Property NI As Integer
            Get
                Return _NI
            End Get
            Set
                _NI = Value
            End Set
        End Property

        <JsonProperty("NL")>
        Public Property NL As Integer
            Get
                Return _NL
            End Get
            Set
                _NL = Value
            End Set
        End Property

        <JsonProperty("NO")>
        Public Property NO As Integer
            Get
                Return _NO
            End Get
            Set
                _NO = Value
            End Set
        End Property

        <JsonProperty("NZ")>
        Public Property NZ As Integer
            Get
                Return _NZ
            End Get
            Set
                _NZ = Value
            End Set
        End Property

        <JsonProperty("OM")>
        Public Property OM As Integer
            Get
                Return _OM
            End Get
            Set
                _OM = Value
            End Set
        End Property

        <JsonProperty("PA")>
        Public Property PA As Integer
            Get
                Return _PA
            End Get
            Set
                _PA = Value
            End Set
        End Property

        <JsonProperty("PE")>
        Public Property PE As Integer
            Get
                Return _PE
            End Get
            Set
                _PE = Value
            End Set
        End Property

        <JsonProperty("PL")>
        Public Property PL As Integer
            Get
                Return _PL
            End Get
            Set
                _PL = Value
            End Set
        End Property

        <JsonProperty("PT")>
        Public Property PT As Integer
            Get
                Return _PT
            End Get
            Set
                _PT = Value
            End Set
        End Property

        <JsonProperty("PY")>
        Public Property PY As Integer
            Get
                Return _PY
            End Get
            Set
                _PY = Value
            End Set
        End Property

        <JsonProperty("QA")>
        Public Property QA As Integer
            Get
                Return _QA
            End Get
            Set
                _QA = Value
            End Set
        End Property

        <JsonProperty("RO")>
        Public Property RO As Integer
            Get
                Return _RO
            End Get
            Set
                _RO = Value
            End Set
        End Property

        <JsonProperty("RU")>
        Public Property RU As Integer
            Get
                Return _RU
            End Get
            Set
                _RU = Value
            End Set
        End Property

        <JsonProperty("SA")>
        Public Property SA As Integer
            Get
                Return _SA
            End Get
            Set
                _SA = Value
            End Set
        End Property

        <JsonProperty("SE")>
        Public Property SE As Integer
            Get
                Return _SE
            End Get
            Set
                _SE = Value
            End Set
        End Property

        <JsonProperty("SG")>
        Public Property SG As Integer
            Get
                Return _SG
            End Get
            Set
                _SG = Value
            End Set
        End Property

        <JsonProperty("SI")>
        Public Property SI As Integer
            Get
                Return _SI
            End Get
            Set
                _SI = Value
            End Set
        End Property

        <JsonProperty("SK")>
        Public Property SK As Integer
            Get
                Return _SK
            End Get
            Set
                _SK = Value
            End Set
        End Property

        <JsonProperty("SV")>
        Public Property SV As Integer
            Get
                Return _SV
            End Get
            Set
                _SV = Value
            End Set
        End Property

        <JsonProperty("TH")>
        Public Property TH As Integer
            Get
                Return _TH
            End Get
            Set
                _TH = Value
            End Set
        End Property

        <JsonProperty("TR")>
        Public Property TR As Integer
            Get
                Return _TR
            End Get
            Set
                _TR = Value
            End Set
        End Property

        <JsonProperty("TW")>
        Public Property TW As Integer
            Get
                Return _TW
            End Get
            Set
                _TW = Value
            End Set
        End Property

        <JsonProperty("UA")>
        Public Property UA As Integer
            Get
                Return _UA
            End Get
            Set
                _UA = Value
            End Set
        End Property

        <JsonProperty("UY")>
        Public Property UY As Integer
            Get
                Return _UY
            End Get
            Set
                _UY = Value
            End Set
        End Property

        <JsonProperty("ZA")>
        Public Property ZA As Integer
            Get
                Return _ZA
            End Get
            Set
                _ZA = Value
            End Set
        End Property

        <JsonProperty("default")>
        Public Property [Default] As Integer
            Get
                Return _Default
            End Get
            Set
                _Default = Value
            End Set
        End Property
    End Class

    Public Class ArAE
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class CsCZ
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class DaDK
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class DeDE
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class ElGR
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class EnGB
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class EnUS
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class Es419
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class EsES
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class FiFI
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class FrCA
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class FrFR
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class HuHU
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class IdID
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class ItIT
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class JaJP
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class KoKR
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class NlNL
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class NoNO
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class PlPL
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class PtBR
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class PtPT
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class RoRO
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class RuRU
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class SvSE
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class ThTH
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class TrTR
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class ViVN
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class ZhHans
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class ZhHant
        Private _TitleName As String

        <JsonProperty("titleName")>
        Public Property TitleName As String
            Get
                Return _TitleName
            End Get
            Set
                _TitleName = Value
            End Set
        End Property
    End Class

    Public Class LocalizedParameters
        Private _ArAE As ArAE
        Private _CsCZ As CsCZ
        Private _DaDK As DaDK
        Private _DeDE As DeDE
        Private _DefaultLanguage As String
        Private _ElGR As ElGR
        Private _FrCA As FrCA
        Private _FiFI As FiFI
        Private _EsES As EsES
        Private _Es419 As Es419
        Private _EnUS As EnUS
        Private _EnGB As EnGB
        Private _PtBR As PtBR
        Private _PlPL As PlPL
        Private _NoNO As NoNO
        Private _NlNL As NlNL
        Private _KoKR As KoKR
        Private _JaJP As JaJP
        Private _ItIT As ItIT
        Private _IdID As IdID
        Private _HuHU As HuHU
        Private _FrFR As FrFR
        Private _ZhHant As ZhHant
        Private _ZhHans As ZhHans
        Private _ViVN As ViVN
        Private _TrTR As TrTR
        Private _ThTH As ThTH
        Private _SvSE As SvSE
        Private _RuRU As RuRU
        Private _RoRO As RoRO
        Private _PtPT As PtPT

        <JsonProperty("ar-AE")>
        Public Property ArAE As ArAE
            Get
                Return _ArAE
            End Get
            Set
                _ArAE = Value
            End Set
        End Property

        <JsonProperty("cs-CZ")>
        Public Property CsCZ As CsCZ
            Get
                Return _CsCZ
            End Get
            Set
                _CsCZ = Value
            End Set
        End Property

        <JsonProperty("da-DK")>
        Public Property DaDK As DaDK
            Get
                Return _DaDK
            End Get
            Set
                _DaDK = Value
            End Set
        End Property

        <JsonProperty("de-DE")>
        Public Property DeDE As DeDE
            Get
                Return _DeDE
            End Get
            Set
                _DeDE = Value
            End Set
        End Property

        <JsonProperty("defaultLanguage")>
        Public Property DefaultLanguage As String
            Get
                Return _DefaultLanguage
            End Get
            Set
                _DefaultLanguage = Value
            End Set
        End Property

        <JsonProperty("el-GR")>
        Public Property ElGR As ElGR
            Get
                Return _ElGR
            End Get
            Set
                _ElGR = Value
            End Set
        End Property

        <JsonProperty("en-GB")>
        Public Property EnGB As EnGB
            Get
                Return _EnGB
            End Get
            Set
                _EnGB = Value
            End Set
        End Property

        <JsonProperty("en-US")>
        Public Property EnUS As EnUS
            Get
                Return _EnUS
            End Get
            Set
                _EnUS = Value
            End Set
        End Property

        <JsonProperty("es-419")>
        Public Property Es419 As Es419
            Get
                Return _Es419
            End Get
            Set
                _Es419 = Value
            End Set
        End Property

        <JsonProperty("es-ES")>
        Public Property EsES As EsES
            Get
                Return _EsES
            End Get
            Set
                _EsES = Value
            End Set
        End Property

        <JsonProperty("fi-FI")>
        Public Property FiFI As FiFI
            Get
                Return _FiFI
            End Get
            Set
                _FiFI = Value
            End Set
        End Property

        <JsonProperty("fr-CA")>
        Public Property FrCA As FrCA
            Get
                Return _FrCA
            End Get
            Set
                _FrCA = Value
            End Set
        End Property

        <JsonProperty("fr-FR")>
        Public Property FrFR As FrFR
            Get
                Return _FrFR
            End Get
            Set
                _FrFR = Value
            End Set
        End Property

        <JsonProperty("hu-HU")>
        Public Property HuHU As HuHU
            Get
                Return _HuHU
            End Get
            Set
                _HuHU = Value
            End Set
        End Property

        <JsonProperty("id-ID")>
        Public Property IdID As IdID
            Get
                Return _IdID
            End Get
            Set
                _IdID = Value
            End Set
        End Property

        <JsonProperty("it-IT")>
        Public Property ItIT As ItIT
            Get
                Return _ItIT
            End Get
            Set
                _ItIT = Value
            End Set
        End Property

        <JsonProperty("ja-JP")>
        Public Property JaJP As JaJP
            Get
                Return _JaJP
            End Get
            Set
                _JaJP = Value
            End Set
        End Property

        <JsonProperty("ko-KR")>
        Public Property KoKR As KoKR
            Get
                Return _KoKR
            End Get
            Set
                _KoKR = Value
            End Set
        End Property

        <JsonProperty("nl-NL")>
        Public Property NlNL As NlNL
            Get
                Return _NlNL
            End Get
            Set
                _NlNL = Value
            End Set
        End Property

        <JsonProperty("no-NO")>
        Public Property NoNO As NoNO
            Get
                Return _NoNO
            End Get
            Set
                _NoNO = Value
            End Set
        End Property

        <JsonProperty("pl-PL")>
        Public Property PlPL As PlPL
            Get
                Return _PlPL
            End Get
            Set
                _PlPL = Value
            End Set
        End Property

        <JsonProperty("pt-BR")>
        Public Property PtBR As PtBR
            Get
                Return _PtBR
            End Get
            Set
                _PtBR = Value
            End Set
        End Property

        <JsonProperty("pt-PT")>
        Public Property PtPT As PtPT
            Get
                Return _PtPT
            End Get
            Set
                _PtPT = Value
            End Set
        End Property

        <JsonProperty("ro-RO")>
        Public Property RoRO As RoRO
            Get
                Return _RoRO
            End Get
            Set
                _RoRO = Value
            End Set
        End Property

        <JsonProperty("ru-RU")>
        Public Property RuRU As RuRU
            Get
                Return _RuRU
            End Get
            Set
                _RuRU = Value
            End Set
        End Property

        <JsonProperty("sv-SE")>
        Public Property SvSE As SvSE
            Get
                Return _SvSE
            End Get
            Set
                _SvSE = Value
            End Set
        End Property

        <JsonProperty("th-TH")>
        Public Property ThTH As ThTH
            Get
                Return _ThTH
            End Get
            Set
                _ThTH = Value
            End Set
        End Property

        <JsonProperty("tr-TR")>
        Public Property TrTR As TrTR
            Get
                Return _TrTR
            End Get
            Set
                _TrTR = Value
            End Set
        End Property

        <JsonProperty("vi-VN")>
        Public Property ViVN As ViVN
            Get
                Return _ViVN
            End Get
            Set
                _ViVN = Value
            End Set
        End Property

        <JsonProperty("zh-Hans")>
        Public Property ZhHans As ZhHans
            Get
                Return _ZhHans
            End Get
            Set
                _ZhHans = Value
            End Set
        End Property

        <JsonProperty("zh-Hant")>
        Public Property ZhHant As ZhHant
            Get
                Return _ZhHant
            End Get
            Set
                _ZhHant = Value
            End Set
        End Property
    End Class

    Public Class Savedata
        Private _TitleIdForTransferringPs4 As String()

        <JsonProperty("titleIdForTransferringPs4")>
        Public Property TitleIdForTransferringPs4 As String()
            Get
                Return _TitleIdForTransferringPs4
            End Get
            Set
                _TitleIdForTransferringPs4 = Value
            End Set
        End Property
    End Class

    Public Class Code
        Private _Asa10 As String

        <JsonProperty("asa10")>
        Public Property Asa10 As String
            Get
                Return _Asa10
            End Get
            Set
                _Asa10 = Value
            End Set
        End Property
    End Class

    Public Class Asa
        Private _Code As Code
        Private _Sign As String()

        <JsonProperty("code")>
        Public Property Code As Code
            Get
                Return _Code
            End Get
            Set
                _Code = Value
            End Set
        End Property

        <JsonProperty("sign")>
        Public Property Sign As String()
            Get
                Return _Sign
            End Get
            Set
                _Sign = Value
            End Set
        End Property
    End Class

    Public Class Kernel
        Private _CpuPageTableSize As Integer
        Private _FlexibleMemorySize As Integer
        Private _GpuPageTableSize As Integer

        <JsonProperty("cpuPageTableSize")>
        Public Property CpuPageTableSize As Integer
            Get
                Return _CpuPageTableSize
            End Get
            Set
                _CpuPageTableSize = Value
            End Set
        End Property

        <JsonProperty("flexibleMemorySize")>
        Public Property FlexibleMemorySize As Integer
            Get
                Return _FlexibleMemorySize
            End Get
            Set
                _FlexibleMemorySize = Value
            End Set
        End Property

        <JsonProperty("gpuPageTableSize")>
        Public Property GpuPageTableSize As Integer
            Get
                Return _GpuPageTableSize
            End Get
            Set
                _GpuPageTableSize = Value
            End Set
        End Property
    End Class

    Public Class Pubtools
        Private _CreationDate As String
        Private _LoudnessSnd0 As String
        Private _Submission As Boolean
        Private _ToolVersion As String

        <JsonProperty("creationDate")>
        Public Property CreationDate As String
            Get
                Return _CreationDate
            End Get
            Set
                _CreationDate = Value
            End Set
        End Property

        <JsonProperty("loudnessSnd0")>
        Public Property LoudnessSnd0 As String
            Get
                Return _LoudnessSnd0
            End Get
            Set
                _LoudnessSnd0 = Value
            End Set
        End Property

        <JsonProperty("submission")>
        Public Property Submission As Boolean
            Get
                Return _Submission
            End Get
            Set
                _Submission = Value
            End Set
        End Property

        <JsonProperty("toolVersion")>
        Public Property ToolVersion As String
            Get
                Return _ToolVersion
            End Get
            Set
                _ToolVersion = Value
            End Set
        End Property
    End Class

    Public Class PS5Param
        Private _AgeLevel As AgeLevel
        Private _ApplicationCategoryType As Integer
        Private _ApplicationDrmType As String
        Private _Asa As Asa
        Private _Attribute As Integer
        Private _Attribute2 As Integer
        Private _Attribute3 As Integer
        Private _BackgroundBasematType As String
        Private _ConceptId As String
        Private _ContentBadgeType As Integer
        Private _ContentId As String
        Private _ContentVersion As String
        Private _DeeplinkUri As String
        Private _DownloadDataSize As Integer
        Private _Kernel As Kernel
        Private _LocalizedParameters As LocalizedParameters
        Private _MasterVersion As String
        Private _OriginContentVersion As String
        Private _Pubtools As Pubtools
        Private _RequiredSystemSoftwareVersion As String
        Private _Savedata As Savedata
        Private _SdkVersion As String
        Private _TargetContentVersion As String
        Private _TitleId As String
        Private _VersionFileUri As String

        <JsonProperty("ageLevel")>
        Public Property AgeLevel As AgeLevel
            Get
                Return _AgeLevel
            End Get
            Set
                _AgeLevel = Value
            End Set
        End Property

        <JsonProperty("applicationCategoryType")>
        Public Property ApplicationCategoryType As Integer
            Get
                Return _ApplicationCategoryType
            End Get
            Set
                _ApplicationCategoryType = Value
            End Set
        End Property

        <JsonProperty("applicationDrmType")>
        Public Property ApplicationDrmType As String
            Get
                Return _ApplicationDrmType
            End Get
            Set
                _ApplicationDrmType = Value
            End Set
        End Property

        <JsonProperty("asa")>
        Public Property Asa As Asa
            Get
                Return _Asa
            End Get
            Set
                _Asa = Value
            End Set
        End Property

        <JsonProperty("attribute")>
        Public Property Attribute As Integer
            Get
                Return _Attribute
            End Get
            Set
                _Attribute = Value
            End Set
        End Property

        <JsonProperty("attribute2")>
        Public Property Attribute2 As Integer
            Get
                Return _Attribute2
            End Get
            Set
                _Attribute2 = Value
            End Set
        End Property

        <JsonProperty("attribute3")>
        Public Property Attribute3 As Integer
            Get
                Return _Attribute3
            End Get
            Set
                _Attribute3 = Value
            End Set
        End Property

        <JsonProperty("backgroundBasematType")>
        Public Property BackgroundBasematType As String
            Get
                Return _BackgroundBasematType
            End Get
            Set
                _BackgroundBasematType = Value
            End Set
        End Property

        <JsonProperty("conceptId")>
        Public Property ConceptId As String
            Get
                Return _ConceptId
            End Get
            Set
                _ConceptId = Value
            End Set
        End Property

        <JsonProperty("contentBadgeType")>
        Public Property ContentBadgeType As Integer
            Get
                Return _ContentBadgeType
            End Get
            Set
                _ContentBadgeType = Value
            End Set
        End Property

        <JsonProperty("contentId")>
        Public Property ContentId As String
            Get
                Return _ContentId
            End Get
            Set
                _ContentId = Value
            End Set
        End Property

        <JsonProperty("contentVersion")>
        Public Property ContentVersion As String
            Get
                Return _ContentVersion
            End Get
            Set
                _ContentVersion = Value
            End Set
        End Property

        <JsonProperty("downloadDataSize")>
        Public Property DownloadDataSize As Integer
            Get
                Return _DownloadDataSize
            End Get
            Set
                _DownloadDataSize = Value
            End Set
        End Property

        <JsonProperty("deeplinkUri")>
        Public Property DeeplinkUri As String
            Get
                Return _DeeplinkUri
            End Get
            Set
                _DeeplinkUri = Value
            End Set
        End Property

        <JsonProperty("kernel")>
        Public Property Kernel As Kernel
            Get
                Return _Kernel
            End Get
            Set
                _Kernel = Value
            End Set
        End Property

        <JsonProperty("localizedParameters")>
        Public Property LocalizedParameters As LocalizedParameters
            Get
                Return _LocalizedParameters
            End Get
            Set
                _LocalizedParameters = Value
            End Set
        End Property

        <JsonProperty("masterVersion")>
        Public Property MasterVersion As String
            Get
                Return _MasterVersion
            End Get
            Set
                _MasterVersion = Value
            End Set
        End Property

        <JsonProperty("originContentVersion")>
        Public Property OriginContentVersion As String
            Get
                Return _OriginContentVersion
            End Get
            Set
                _OriginContentVersion = Value
            End Set
        End Property

        <JsonProperty("pubtools")>
        Public Property Pubtools As Pubtools
            Get
                Return _Pubtools
            End Get
            Set
                _Pubtools = Value
            End Set
        End Property

        <JsonProperty("requiredSystemSoftwareVersion")>
        Public Property RequiredSystemSoftwareVersion As String
            Get
                Return _RequiredSystemSoftwareVersion
            End Get
            Set
                _RequiredSystemSoftwareVersion = Value
            End Set
        End Property

        <JsonProperty("savedata")>
        Public Property Savedata As Savedata
            Get
                Return _Savedata
            End Get
            Set
                _Savedata = Value
            End Set
        End Property

        <JsonProperty("sdkVersion")>
        Public Property SdkVersion As String
            Get
                Return _SdkVersion
            End Get
            Set
                _SdkVersion = Value
            End Set
        End Property

        <JsonProperty("targetContentVersion")>
        Public Property TargetContentVersion As String
            Get
                Return _TargetContentVersion
            End Get
            Set
                _TargetContentVersion = Value
            End Set
        End Property

        <JsonProperty("titleId")>
        Public Property TitleId As String
            Get
                Return _TitleId
            End Get
            Set
                _TitleId = Value
            End Set
        End Property

        <JsonProperty("versionFileUri")>
        Public Property VersionFileUri As String
            Get
                Return _VersionFileUri
            End Get
            Set
                _VersionFileUri = Value
            End Set
        End Property
    End Class

    Public Class ParamListViewItem
        Private _ParamName As String
        Private _ParamType As String
        Private _ParamValue As String

        Public Property ParamName As String
            Get
                Return _ParamName
            End Get
            Set
                _ParamName = Value
            End Set
        End Property

        Public Property ParamType As String
            Get
                Return _ParamType
            End Get
            Set
                _ParamType = Value
            End Set
        End Property

        Public Property ParamValue As String
            Get
                Return _ParamValue
            End Get
            Set
                _ParamValue = Value
            End Set
        End Property
    End Class

End Class

Public Class PS5ManifestClass

    Public Class ApplicationData
        Private _branchType As String

        Public Property branchType As String
            Get
                Return _branchType
            End Get
            Set
                _branchType = Value
            End Set
        End Property
    End Class

    Public Class PS5Manifest
        Private _applicationData As ApplicationData
        Private _applicationName As String
        Private _applicationVersion As String
        Private _bootAnimation As String
        Private _commitHash As String
        Private _enableAccessibility As String()
        Private _enableHttpCache As Boolean
        Private _repositoryUrl As String
        Private _reactNativePlaystationVersion As String
        Private _titleId As String
        Private _twinTurbo As Boolean

        Public Property applicationData As ApplicationData
            Get
                Return _applicationData
            End Get
            Set
                _applicationData = Value
            End Set
        End Property

        Public Property applicationName As String
            Get
                Return _applicationName
            End Get
            Set
                _applicationName = Value
            End Set
        End Property

        Public Property applicationVersion As String
            Get
                Return _applicationVersion
            End Get
            Set
                _applicationVersion = Value
            End Set
        End Property

        Public Property bootAnimation As String
            Get
                Return _bootAnimation
            End Get
            Set
                _bootAnimation = Value
            End Set
        End Property

        Public Property commitHash As String
            Get
                Return _commitHash
            End Get
            Set
                _commitHash = Value
            End Set
        End Property

        Public Property enableAccessibility As String()
            Get
                Return _enableAccessibility
            End Get
            Set
                _enableAccessibility = Value
            End Set
        End Property

        Public Property enableHttpCache As Boolean
            Get
                Return _enableHttpCache
            End Get
            Set
                _enableHttpCache = Value
            End Set
        End Property

        Public Property repositoryUrl As String
            Get
                Return _repositoryUrl
            End Get
            Set
                _repositoryUrl = Value
            End Set
        End Property

        Public Property reactNativePlaystationVersion As String
            Get
                Return _reactNativePlaystationVersion
            End Get
            Set
                _reactNativePlaystationVersion = Value
            End Set
        End Property

        Public Property titleId As String
            Get
                Return _titleId
            End Get
            Set
                _titleId = Value
            End Set
        End Property

        Public Property twinTurbo As Boolean
            Get
                Return _twinTurbo
            End Get
            Set
                _twinTurbo = Value
            End Set
        End Property
    End Class

End Class

Public Class WAVFile
    Private _RIFF As Integer
    Private _TotalLength As Integer
    Private _Wave As Integer
    Private _ByteRate As Integer
    Private _FormatChunkMarker As Integer
    Private _Subchunk1Size As Integer
    Private _AudioFormat As Integer
    Private _Channels As Integer
    Private _SampleRate As Integer
    Private _BitsPerSample As Integer
    Private _DataLength As Integer
    Private _Data As Integer
    Private _BlockAlign As Integer

    Public Property RIFF As Integer
        Get
            Return _RIFF
        End Get
        Set
            _RIFF = Value
        End Set
    End Property

    Public Property TotalLength As Integer
        Get
            Return _TotalLength
        End Get
        Set
            _TotalLength = Value
        End Set
    End Property

    Public Property Wave As Integer
        Get
            Return _Wave
        End Get
        Set
            _Wave = Value
        End Set
    End Property

    Public Property FormatChunkMarker As Integer
        Get
            Return _FormatChunkMarker
        End Get
        Set
            _FormatChunkMarker = Value
        End Set
    End Property

    Public Property Subchunk1Size As Integer
        Get
            Return _Subchunk1Size
        End Get
        Set
            _Subchunk1Size = Value
        End Set
    End Property

    Public Property AudioFormat As Integer
        Get
            Return _AudioFormat
        End Get
        Set
            _AudioFormat = Value
        End Set
    End Property

    Public Property Channels As Integer
        Get
            Return _Channels
        End Get
        Set
            _Channels = Value
        End Set
    End Property

    Public Property SampleRate As Integer
        Get
            Return _SampleRate
        End Get
        Set
            _SampleRate = Value
        End Set
    End Property

    Public Property ByteRate As Integer
        Get
            Return _ByteRate
        End Get
        Set
            _ByteRate = Value
        End Set
    End Property

    Public Property BlockAlign As Integer
        Get
            Return _BlockAlign
        End Get
        Set
            _BlockAlign = Value
        End Set
    End Property

    Public Property BitsPerSample As Integer
        Get
            Return _BitsPerSample
        End Get
        Set
            _BitsPerSample = Value
        End Set
    End Property

    Public Property Data As Integer
        Get
            Return _Data
        End Get
        Set
            _Data = Value
        End Set
    End Property

    Public Property DataLength As Integer
        Get
            Return _DataLength
        End Get
        Set
            _DataLength = Value
        End Set
    End Property

End Class

Public Class Translations
    Public Class DetectedLanguage
        Private _confidence As Double
        Private _language As String

        Public Property confidence As Double
            Get
                Return _confidence
            End Get
            Set
                _confidence = Value
            End Set
        End Property

        Public Property language As String
            Get
                Return _language
            End Get
            Set
                _language = Value
            End Set
        End Property
    End Class

    Public Class ReceivedTranslation
        Private _detectedLanguage As DetectedLanguage
        Private _translatedText As String

        Public Property detectedLanguage As DetectedLanguage
            Get
                Return _detectedLanguage
            End Get
            Set
                _detectedLanguage = Value
            End Set
        End Property

        Public Property translatedText As String
            Get
                Return _translatedText
            End Get
            Set
                _translatedText = Value
            End Set
        End Property
    End Class
End Class

Public Class Offers

    Private _Type As String
    Private _Price As Double
    Private _PriceCurrency As String

    <JsonProperty("@type")>
    Public Property Type As String
        Get
            Return _Type
        End Get
        Set
            _Type = Value
        End Set
    End Property

    <JsonProperty("price")>
    Public Property Price As Double
        Get
            Return _Price
        End Get
        Set
            _Price = Value
        End Set
    End Property

    <JsonProperty("priceCurrency")>
    Public Property PriceCurrency As String
        Get
            Return _PriceCurrency
        End Get
        Set
            _PriceCurrency = Value
        End Set
    End Property

End Class

Public Class StorePageInfos

    Private _Context As String
    Private _Type As String
    Private _Name As String
    Private _Category As String
    Private _Description As String
    Private _Sku As String
    Private _Image As String
    Private _Offers As Offers

    <JsonProperty("@context")>
    Public Property Context As String
        Get
            Return _Context
        End Get
        Set
            _Context = Value
        End Set
    End Property

    <JsonProperty("@type")>
    Public Property Type As String
        Get
            Return _Type
        End Get
        Set
            _Type = Value
        End Set
    End Property

    <JsonProperty("name")>
    Public Property Name As String
        Get
            Return _Name
        End Get
        Set
            _Name = Value
        End Set
    End Property

    <JsonProperty("category")>
    Public Property Category As String
        Get
            Return _Category
        End Get
        Set
            _Category = Value
        End Set
    End Property

    <JsonProperty("description")>
    Public Property Description As String
        Get
            Return _Description
        End Get
        Set
            _Description = Value
        End Set
    End Property

    <JsonProperty("sku")>
    Public Property Sku As String
        Get
            Return _Sku
        End Get
        Set
            _Sku = Value
        End Set
    End Property

    <JsonProperty("image")>
    Public Property Image As String
        Get
            Return _Image
        End Get
        Set
            _Image = Value
        End Set
    End Property

    <JsonProperty("offers")>
    Public Property Offers As Offers
        Get
            Return _Offers
        End Get
        Set
            _Offers = Value
        End Set
    End Property

End Class

Public Class PS5DiscParamClass

    Public Class Disc
        Private _MasterDataId As String
        Private _Role As String

        <JsonProperty("masterDataId")>
        Public Property MasterDataId As String
            Get
                Return _MasterDataId
            End Get
            Set
                _MasterDataId = Value
            End Set
        End Property

        <JsonProperty("role")>
        Public Property Role As String
            Get
                Return _Role
            End Get
            Set
                _Role = Value
            End Set
        End Property
    End Class

    Public Class PS5DiscParam
        Private _RequiredSystemSoftwareVersion As String
        Private _PubtoolsVersion As String
        Private _MasterVersion As String
        Private _DiscTotal As Integer
        Private _DiscNumber As Integer
        Private _Disc As Disc()

        <JsonProperty("disc")>
        Public Property Disc As Disc()
            Get
                Return _Disc
            End Get
            Set
                _Disc = Value
            End Set
        End Property

        <JsonProperty("discNumber")>
        Public Property DiscNumber As Integer
            Get
                Return _DiscNumber
            End Get
            Set
                _DiscNumber = Value
            End Set
        End Property

        <JsonProperty("discTotal")>
        Public Property DiscTotal As Integer
            Get
                Return _DiscTotal
            End Get
            Set
                _DiscTotal = Value
            End Set
        End Property

        <JsonProperty("masterVersion")>
        Public Property MasterVersion As String
            Get
                Return _MasterVersion
            End Get
            Set
                _MasterVersion = Value
            End Set
        End Property

        <JsonProperty("pubtoolsVersion")>
        Public Property PubtoolsVersion As String
            Get
                Return _PubtoolsVersion
            End Get
            Set
                _PubtoolsVersion = Value
            End Set
        End Property

        <JsonProperty("requiredSystemSoftwareVersion")>
        Public Property RequiredSystemSoftwareVersion As String
            Get
                Return _RequiredSystemSoftwareVersion
            End Get
            Set
                _RequiredSystemSoftwareVersion = Value
            End Set
        End Property
    End Class

End Class
