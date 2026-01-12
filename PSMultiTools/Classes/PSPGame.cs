using IronSoftware.Drawing;

namespace PSMultiTools.Classes
{
    public class PSPGame
    {
        public string? GameTitle { get; set; }

        public string? GameID { get; set; }

        public string? GameSize { get; set; }

        public string? GameRegion { get; set; }

        public string? GameFilePath { get; set; }

        public string? GameFolderPath { get; set; }

        public enum GameFileTypes
        {
            Backup,
            ISO
        }

        public GameFileTypes GameFileType { get; set; }

        public string? GameCategory { get; set; }

        public string? GameRequiredFW { get; set; }

        public string? GameAppVer { get; set; }

        public AnyBitmap? GameCoverSource { get; set; }

        public AnyBitmap? GameBackgroundSource { get; set; }

        public string? GameBackgroundSoundFile { get; set; }

        public static string GetCategory(string SFOCategory)
        {
            switch (SFOCategory ?? "")
            {
                case "UG":
                    {
                        return "UMD Disc Game";
                    }
                case "PG":
                    {
                        return "Game Update";
                    }
                case "EG":
                    {
                        return "PSP Remaster";
                    }
                case "MA":
                    {
                        return "App";
                    }
                case "ME":
                    {
                        return "PS1 Classic";
                    }
                case "MS":
                    {
                        return "MemoryStick Save for Game&Apps";
                    }

                default:
                    {
                        return "Unknown";
                    }
            }
        }

        public static string GetGameRegion(string GameID)
        {
            if (GameID.StartsWith("UCES"))
            {
                return "Europe";
            }
            else if (GameID.StartsWith("ULES"))
            {
                return "Europe";
            }
            else if (GameID.StartsWith("SLES"))
            {
                return "Europe";
            }
            else if (GameID.StartsWith("UCUS"))
            {
                return "USA";
            }
            else if (GameID.StartsWith("ULUS"))
            {
                return "USA";
            }
            else if (GameID.StartsWith("NPUG"))
            {
                return "USA";
            }
            else if (GameID.StartsWith("SLUS"))
            {
                return "USA";
            }
            else if (GameID.StartsWith("ULJS"))
            {
                return "Japan";
            }
            else if (GameID.StartsWith("UCJS"))
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