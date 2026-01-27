using IronSoftware.Drawing;

namespace PSMultiTools.Classes
{
    public class PS5Game
    {

        public enum Location
        {
            Remote,
            Local
        }

        public enum RootLocation
        {
            InternalStorage,
            InternalExtendedStorage,
            AttachedUSB
        }

        public enum BackupType
        {
            LocalFolder,
            LocalPKG,
            FTPFolder,
            FTPPKG
        }

        public string? GameTitle { get; set; }

        public string? GameID { get; set; }

        public string? GameSize { get; set; }

        public string? GameRegion { get; set; }

        public string? GameFileOrFolderPath { get; set; }

        public AnyBitmap? GameCoverSource { get; set; }

        public AnyBitmap? GameBGSource { get; set; }

        public Avalonia.Media.ImageBrush? GameBackgroundImageBrush { get; set; }

        public string? GameContentID { get; set; }

        public string? GameCategory { get; set; }

        public string? GameVersion { get; set; }

        public string? GameRequiredFirmware { get; set; }

        public string? GameSoundFile { get; set; }

        public string? DEGameTitle { get; set; }

        public string? FRGameTitle { get; set; }

        public string? ITGameTitle { get; set; }

        public string? ESGameTitle { get; set; }

        public string? JPGameTitle { get; set; }

        public string? IsCompatibleFW { get; set; }

        public string? DecFilesIncluded { get; set; }

        public string? GameContentIDs { get; set; }

        public BackupType? GameBackupType { get; set; }

        public Location GameLocation { get; set; }

        public RootLocation GameRootLocation { get; set; }

        public string? GameMasterVersion { get; set; }

        public string? GameSDKVersion { get; set; }

        public string? GamePubToolVersion { get; set; }

        public string? GameVersionFileURI { get; set; }

        public bool? IsInstalled { get; set; }

        public bool? DecryptedFilesExist { get; set; }

        public bool? FakelibFilesExist { get; set; }

        public static string GetGameRegion(string GameID)
        {
            if (GameID.StartsWith("PPSA"))
            {
                return "NA / Europe";
            }
            else if (GameID.StartsWith("ECAS"))
            {
                return "Asia";
            }
            else if (GameID.StartsWith("ELAS"))
            {
                return "Asia";
            }
            else if (GameID.StartsWith("ELJM"))
            {
                return "Japan";
            }
            else
            {
                return "";
            }
        }

    }
}
