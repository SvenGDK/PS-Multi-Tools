using Avalonia.Media.Imaging;
using IronSoftware.Drawing;
using Newtonsoft.Json;

namespace PSMultiTools.Classes
{

    public class Structures
    {

        public struct PackageInfo
        {
            public string FileSize { get; set; }
            public string FileDate { get; set; }
        }

        public struct CopyItem
        {
            public string FileName { get; set; }
            public string FileSize { get; set; }
        }

        public struct ExtractionPKGProcess
        {
            public string DecryptedPKGFileName { get; set; }
            public string EncryptedPKGFileName { get; set; }
            public string PKGFileName { get; set; }
        }

        public struct ExtractionWorkerProgress
        {
            public int FileCount { get; set; }
            public string FileName { get; set; }
        }

        public struct ParamSFOContent
        {
            public string ParamName { get; set; }
            public object ParamValue { get; set; }
            public int ParamLenght { get; set; }
            public int ParamType { get; set; }
            public int ParamLocation { get; set; }
        }

        public enum AssetType
        {
            Video,
            Audio,
            Image,
            Font,
            DIC,
            XML
        }

        public struct AssetListViewItem
        {
            public string AssetFileName { get; set; }

            public string AssetFilePath { get; set; }

            public AssetType Type { get; set; }

            public Bitmap Icon { get; set; }
        }

        public struct MountedPSXDrive
        {
            public string DriveID { get; set; }

            public string HDLDriveName { get; set; }

            public string NBDDriveName { get; set; }
        }

        public struct Partition
        {
            public string Type { get; set; }

            public string Start { get; set; }

            public string Parts { get; set; }

            public string Size { get; set; }

            public string Name { get; set; }
        }

        public struct GamePartition
        {
            public string Type { get; set; }

            public string Size { get; set; }

            public string Flags { get; set; }

            public string DMA { get; set; }

            public string Startup { get; set; }

            public string Name { get; set; }
        }

        public struct Package
        {
            public string PackageName { get; set; }

            public string PackageDescription { get; set; }

            public string PackageURL { get; set; }

            public string PackageTitleID { get; set; }

            public string PackageContentID { get; set; }

            public string PackageRAP { get; set; }

            public string PackagezRIF { get; set; }

            public string PackageReqFW { get; set; }

            public string PackageRegion { get; set; }

            public string PackageDate { get; set; }

            public string PackageDLCs { get; set; }

            public string PackageSize { get; set; }

            public bool IsSelected { get; set; }

        }

        public struct GP5ChunkFilesFolderListViewItem
        {
            public string ChunkType { get; set; }

            public string SourcePath { get; set; }

            public string DestinationPath { get; set; }
        }

        public struct PKGBuilderProject
        {
            public string ProjectPath { get; set; }

            public string ProjectCategory { get; set; }

            public AnyBitmap? ProjectIcon { get; set; }

            public string ProjectTitle { get; set; }

            public AnyBitmap? ProjectBackground { get; set; }

            public string ProjectSoundtrack { get; set; }

            public string ProjectURL { get; set; }

            public bool GP5Created { get; set; }
        }

        public struct PS5GameLoaderArgs
        {
            public string FolderPath { get; set; }

            public bool LoadIcons { get; set; }

            public bool LoadBackgrounds { get; set; }

            public bool SkipFileChecks { get; set; }
        }

        public struct FTPListViewItem
        {
            public string FileOrDirName { get; set; }

            public string FileOrDirType { get; set; }

            public string FileOrDirSize { get; set; }

            public string FileOrDirLastModified { get; set; }

            public string FileOrDirPermissions { get; set; }

            public string FileOrDirOwner { get; set; }
        }

    }

    public class PS5ParamClass
    {
        public class AgeLevel
        {
            [JsonProperty("US")]
            public int US { get; set; }

            [JsonProperty("AE")]
            public int AE { get; set; }

            [JsonProperty("AR")]
            public int AR { get; set; }

            [JsonProperty("AT")]
            public int AT { get; set; }

            [JsonProperty("AU")]
            public int AU { get; set; }

            [JsonProperty("BE")]
            public int BE { get; set; }

            [JsonProperty("BG")]
            public int BG { get; set; }

            [JsonProperty("BH")]
            public int BH { get; set; }

            [JsonProperty("BO")]
            public int BO { get; set; }

            [JsonProperty("BR")]
            public int BR { get; set; }

            [JsonProperty("CA")]
            public int CA { get; set; }

            [JsonProperty("CH")]
            public int CH { get; set; }

            [JsonProperty("CL")]
            public int CL { get; set; }

            [JsonProperty("CN")]
            public int CN { get; set; }

            [JsonProperty("CO")]
            public int CO { get; set; }

            [JsonProperty("CR")]
            public int CR { get; set; }

            [JsonProperty("CY")]
            public int CY { get; set; }

            [JsonProperty("CZ")]
            public int CZ { get; set; }

            [JsonProperty("DE")]
            public int DE { get; set; }

            [JsonProperty("DK")]
            public int DK { get; set; }

            [JsonProperty("EC")]
            public int EC { get; set; }

            [JsonProperty("ES")]
            public int ES { get; set; }

            [JsonProperty("FI")]
            public int FI { get; set; }

            [JsonProperty("FR")]
            public int FR { get; set; }

            [JsonProperty("GB")]
            public int GB { get; set; }

            [JsonProperty("GR")]
            public int GR { get; set; }

            [JsonProperty("GT")]
            public int GT { get; set; }

            [JsonProperty("HK")]
            public int HK { get; set; }

            [JsonProperty("HN")]
            public int HN { get; set; }

            [JsonProperty("HR")]
            public int HR { get; set; }

            [JsonProperty("HU")]
            public int HU { get; set; }

            [JsonProperty("ID")]
            public int ID { get; set; }

            [JsonProperty("IE")]
            public int IE { get; set; }

            [JsonProperty("IL")]
            public int IL { get; set; }

            [JsonProperty("IN")]
            public int India { get; set; }

            [JsonProperty("IS")]
            public int Iceland { get; set; }

            [JsonProperty("IT")]
            public int IT { get; set; }

            [JsonProperty("JP")]
            public int JP { get; set; }

            [JsonProperty("KR")]
            public int KR { get; set; }

            [JsonProperty("KW")]
            public int KW { get; set; }

            [JsonProperty("LB")]
            public int LB { get; set; }

            [JsonProperty("LU")]
            public int LU { get; set; }

            [JsonProperty("MT")]
            public int MT { get; set; }

            [JsonProperty("MX")]
            public int MX { get; set; }

            [JsonProperty("MY")]
            public int MY { get; set; }

            [JsonProperty("NI")]
            public int NI { get; set; }

            [JsonProperty("NL")]
            public int NL { get; set; }

            [JsonProperty("NO")]
            public int NO { get; set; }

            [JsonProperty("NZ")]
            public int NZ { get; set; }

            [JsonProperty("OM")]
            public int OM { get; set; }

            [JsonProperty("PA")]
            public int PA { get; set; }

            [JsonProperty("PE")]
            public int PE { get; set; }

            [JsonProperty("PL")]
            public int PL { get; set; }

            [JsonProperty("PT")]
            public int PT { get; set; }

            [JsonProperty("PY")]
            public int PY { get; set; }

            [JsonProperty("QA")]
            public int QA { get; set; }

            [JsonProperty("RO")]
            public int RO { get; set; }

            [JsonProperty("RU")]
            public int RU { get; set; }

            [JsonProperty("SA")]
            public int SA { get; set; }

            [JsonProperty("SE")]
            public int SE { get; set; }

            [JsonProperty("SG")]
            public int SG { get; set; }

            [JsonProperty("SI")]
            public int SI { get; set; }

            [JsonProperty("SK")]
            public int SK { get; set; }

            [JsonProperty("SV")]
            public int SV { get; set; }

            [JsonProperty("TH")]
            public int TH { get; set; }

            [JsonProperty("TR")]
            public int TR { get; set; }

            [JsonProperty("TW")]
            public int TW { get; set; }

            [JsonProperty("UA")]
            public int UA { get; set; }

            [JsonProperty("UY")]
            public int UY { get; set; }

            [JsonProperty("ZA")]
            public int ZA { get; set; }

            [JsonProperty("default")]
            public int Default { get; set; }
        }

        public class ArAE
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class CsCZ
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class DaDK
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class DeDE
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class ElGR
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class EnGB
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class EnUS
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class Es419
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class EsES
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class FiFI
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class FrCA
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class FrFR
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class HuHU
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class IdID
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class ItIT
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class JaJP
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class KoKR
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class NlNL
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class NoNO
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class PlPL
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class PtBR
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class PtPT
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class RoRO
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class RuRU
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class SvSE
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class ThTH
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class TrTR
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class ViVN
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class ZhHans
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class ZhHant
        {
            [JsonProperty("titleName")]
            public string? TitleName { get; set; }
        }

        public class LocalizedParameters
        {
            [JsonProperty("ar-AE")]
            public ArAE? ArAE { get; set; }

            [JsonProperty("cs-CZ")]
            public CsCZ? CsCZ { get; set; }

            [JsonProperty("da-DK")]
            public DaDK? DaDK { get; set; }

            [JsonProperty("de-DE")]
            public DeDE? DeDE { get; set; }

            [JsonProperty("defaultLanguage")]
            public string? DefaultLanguage { get; set; }

            [JsonProperty("el-GR")]
            public ElGR? ElGR { get; set; }

            [JsonProperty("en-GB")]
            public EnGB? EnGB { get; set; }

            [JsonProperty("en-US")]
            public EnUS? EnUS { get; set; }

            [JsonProperty("es-419")]
            public Es419? Es419 { get; set; }

            [JsonProperty("es-ES")]
            public EsES? EsES { get; set; }

            [JsonProperty("fi-FI")]
            public FiFI? FiFI { get; set; }

            [JsonProperty("fr-CA")]
            public FrCA? FrCA { get; set; }

            [JsonProperty("fr-FR")]
            public FrFR? FrFR { get; set; }

            [JsonProperty("hu-HU")]
            public HuHU? HuHU { get; set; }

            [JsonProperty("id-ID")]
            public IdID? IdID { get; set; }

            [JsonProperty("it-IT")]
            public ItIT? ItIT { get; set; }

            [JsonProperty("ja-JP")]
            public JaJP? JaJP { get; set; }

            [JsonProperty("ko-KR")]
            public KoKR? KoKR { get; set; }

            [JsonProperty("nl-NL")]
            public NlNL? NlNL { get; set; }

            [JsonProperty("no-NO")]
            public NoNO? NoNO { get; set; }

            [JsonProperty("pl-PL")]
            public PlPL? PlPL { get; set; }

            [JsonProperty("pt-BR")]
            public PtBR? PtBR { get; set; }

            [JsonProperty("pt-PT")]
            public PtPT? PtPT { get; set; }

            [JsonProperty("ro-RO")]
            public RoRO? RoRO { get; set; }

            [JsonProperty("ru-RU")]
            public RuRU? RuRU { get; set; }

            [JsonProperty("sv-SE")]
            public SvSE? SvSE { get; set; }

            [JsonProperty("th-TH")]
            public ThTH? ThTH { get; set; }

            [JsonProperty("tr-TR")]
            public TrTR? TrTR { get; set; }

            [JsonProperty("vi-VN")]
            public ViVN? ViVN { get; set; }

            [JsonProperty("zh-Hans")]
            public ZhHans? ZhHans { get; set; }

            [JsonProperty("zh-Hant")]
            public ZhHant? ZhHant { get; set; }
        }

        public class Savedata
        {
            [JsonProperty("titleIdForTransferringPs4")]
            public string[]? TitleIdForTransferringPs4 { get; set; }
        }

        public class Code
        {
            [JsonProperty("asa10")]
            public string? Asa10 { get; set; }
        }

        public class Asa
        {
            [JsonProperty("code")]
            public Code? Code { get; set; }

            [JsonProperty("sign")]
            public string[]? Sign { get; set; }
        }

        public class Kernel
        {
            [JsonProperty("cpuPageTableSize")]
            public int CpuPageTableSize { get; set; }

            [JsonProperty("flexibleMemorySize")]
            public int FlexibleMemorySize { get; set; }

            [JsonProperty("gpuPageTableSize")]
            public int GpuPageTableSize { get; set; }
        }

        public class Pubtools
        {
            [JsonProperty("creationDate")]
            public string? CreationDate { get; set; }

            [JsonProperty("loudnessSnd0")]
            public string? LoudnessSnd0 { get; set; }

            [JsonProperty("submission")]
            public bool Submission { get; set; }

            [JsonProperty("toolVersion")]
            public string? ToolVersion { get; set; }
        }

        public class PS5Param
        {
            [JsonProperty("ageLevel")]
            public AgeLevel? AgeLevel { get; set; }

            [JsonProperty("applicationCategoryType")]
            public int ApplicationCategoryType { get; set; }

            [JsonProperty("applicationDrmType")]
            public string? ApplicationDrmType { get; set; }

            [JsonProperty("asa")]
            public Asa? Asa { get; set; }

            [JsonProperty("attribute")]
            public int Attribute { get; set; }

            [JsonProperty("attribute2")]
            public int Attribute2 { get; set; }

            [JsonProperty("attribute3")]
            public int Attribute3 { get; set; }

            [JsonProperty("backgroundBasematType")]
            public string? BackgroundBasematType { get; set; }

            [JsonProperty("conceptId")]
            public string? ConceptId { get; set; }

            [JsonProperty("contentBadgeType")]
            public int ContentBadgeType { get; set; }

            [JsonProperty("contentId")]
            public string? ContentId { get; set; }

            [JsonProperty("contentVersion")]
            public string? ContentVersion { get; set; }

            [JsonProperty("downloadDataSize")]
            public int DownloadDataSize { get; set; }

            [JsonProperty("deeplinkUri")]
            public string? DeeplinkUri { get; set; }

            [JsonProperty("kernel")]
            public Kernel? Kernel { get; set; }

            [JsonProperty("localizedParameters")]
            public LocalizedParameters? LocalizedParameters { get; set; }

            [JsonProperty("masterVersion")]
            public string? MasterVersion { get; set; }

            [JsonProperty("originContentVersion")]
            public string? OriginContentVersion { get; set; }

            [JsonProperty("pubtools")]
            public Pubtools? Pubtools { get; set; }

            [JsonProperty("requiredSystemSoftwareVersion")]
            public string? RequiredSystemSoftwareVersion { get; set; }

            [JsonProperty("savedata")]
            public Savedata? Savedata { get; set; }

            [JsonProperty("sdkVersion")]
            public string? SdkVersion { get; set; }

            [JsonProperty("targetContentVersion")]
            public string? TargetContentVersion { get; set; }

            [JsonProperty("titleId")]
            public string? TitleId { get; set; }

            [JsonProperty("versionFileUri")]
            public string? VersionFileUri { get; set; }
        }

        public class ParamListViewItem
        {
            public string? ParamName { get; set; }

            public string? ParamType { get; set; }

            public string? ParamValue { get; set; }
        }

    }

    public class PS5ManifestClass
    {

        public class ApplicationData
        {
            public string? branchType { get; set; }
        }

        public class PS5Manifest
        {
            public ApplicationData? applicationData { get; set; }

            public string? applicationName { get; set; }

            public string? applicationVersion { get; set; }

            public string? bootAnimation { get; set; }

            public string? commitHash { get; set; }

            public string[]? enableAccessibility { get; set; }

            public bool enableHttpCache { get; set; }

            public string? repositoryUrl { get; set; }

            public string? reactNativePlaystationVersion { get; set; }

            public string? titleId { get; set; }

            public bool twinTurbo { get; set; }
        }

    }

    public class WAVFile
    {
        public int RIFF { get; set; }

        public int TotalLength { get; set; }

        public int Wave { get; set; }

        public int FormatChunkMarker { get; set; }

        public int Subchunk1Size { get; set; }

        public int AudioFormat { get; set; }

        public int Channels { get; set; }

        public int SampleRate { get; set; }

        public int ByteRate { get; set; }

        public int BlockAlign { get; set; }

        public int BitsPerSample { get; set; }

        public int Data { get; set; }

        public int DataLength { get; set; }

    }

    public class Translations
    {
        public class DetectedLanguage
        {
            public double confidence { get; set; }

            public string? language { get; set; }
        }

        public class ReceivedTranslation
        {
            public DetectedLanguage? detectedLanguage { get; set; }

            public string? translatedText { get; set; }
        }
    }

    public class Offers
    {
        [JsonProperty("@type")]
        public string? Type { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("priceCurrency")]
        public string? PriceCurrency { get; set; }

    }

    public class StorePageInfos
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Sku { get; set; }
        public string? Image { get; set; }
    }

    public class PS5DiscParamClass
    {

        public class Disc
        {
            [JsonProperty("masterDataId")]
            public string? MasterDataId { get; set; }

            [JsonProperty("role")]
            public string? Role { get; set; }
        }

        public class PS5DiscParam
        {
            [JsonProperty("disc")]
            public Disc[]? Disc { get; set; }

            [JsonProperty("discNumber")]
            public int DiscNumber { get; set; }

            [JsonProperty("discTotal")]
            public int DiscTotal { get; set; }

            [JsonProperty("masterVersion")]
            public string? MasterVersion { get; set; }

            [JsonProperty("pubtoolsVersion")]
            public string? PubtoolsVersion { get; set; }

            [JsonProperty("requiredSystemSoftwareVersion")]
            public string? RequiredSystemSoftwareVersion { get; set; }
        }

    }

    public class PSXDataCenterPS1GameInfo
    {
        public string? Title { get; set; }
        public string? Code { get; set; }
        public string? Region { get; set; }
        public string? CoverSrc { get; set; }
        public string? Genre { get; set; }
        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public string? ReleaseDate { get; set; }
        public string? Description { get; set; }
    }

    public class PSXDataCenterPS2GameInfo
    {
        public string? Title { get; set; }
        public string? Id { get; set; }
        public string? Region { get; set; }
        public string? Genre { get; set; }
        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public string? ReleaseDate { get; set; }
        public string? Description { get; set; }
    }

}