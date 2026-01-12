using IronSoftware.Drawing;

namespace PSMultiTools.Classes
{
    public class PSVGame
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
            PKG
        }

        public GameFileTypes GameFileType { get; set; }

        public string? GameCategory { get; set; }

        public string? GameRequiredFW { get; set; }

        public string? GameAppVer { get; set; }

        public string? GameVer { get; set; }

        public AnyBitmap? GameCoverSource { get; set; }

        public string? ContentID { get; set; }

        public double GridWidth { get; set; }

        public double GridHeight { get; set; }

        public double ImageWidth { get; set; }

        public double ImageHeight { get; set; }

        public static string GetCategory(string SFOCategory)
        {
            switch (SFOCategory ?? "")
            {
                case "ac":
                    {
                        return "Additional Content";
                    }
                case "gc":
                    {
                        return "Game Content";
                    }
                case "gd":
                    {
                        return "Game Digital Application";
                    }
                case "gda":
                    {
                        return "System Application";
                    }
                case "gdb":
                    {
                        return "System Application";
                    }
                case "gdc":
                    {
                        return "Non-Game Big Application";
                    }
                case "gdd":
                    {
                        return "BG Application";
                    }
                case "gp":
                    {
                        return "Game Patch";
                    }
                case "gpc":
                    {
                        return "Non-Game Big App Patch";
                    }
                case "gpd":
                    {
                        return "BG Application patch";
                    }
                case "sd":
                    {
                        return "Save Data";
                    }

                default:
                    {
                        return "Unknown";
                    }
            }
        }

        public static string GetGameRegion(string GameID)
        {
            if (GameID.StartsWith("PCSB"))
            {
                return "Europe";
            }
            else if (GameID.StartsWith("PCSF"))
            {
                return "Europe";
            }
            else if (GameID.StartsWith("PCSA"))
            {
                return "USA";
            }
            else if (GameID.StartsWith("PCSE"))
            {
                return "USA";
            }
            else if (GameID.StartsWith("PCSG"))
            {
                return "Japan";
            }
            else if (GameID.StartsWith("PCSC"))
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