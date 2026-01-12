using IronSoftware.Drawing;

namespace PSMultiTools.Classes
{
    public class PS4Game
    {
        public enum GameFileTypes
        {
            Backup,
            PKG
        }

        public string? GameTitle { get; set; }

        public string? GameID { get; set; }

        public string? GameSize { get; set; }

        public string? GameRegion { get; set; }

        public string? GameFilePath { get; set; }

        public string? GameFolderPath { get; set; }

        public string? GameCategory { get; set; }

        public string? GameRequiredFW { get; set; }

        public string? GameAppVer { get; set; }

        public string? GameVer { get; set; }

        public AnyBitmap? GameCoverSource { get; set; }

        public AnyBitmap? GameBackgroundSource { get; set; }

        public string? GameContentID { get; set; }

        public byte[]? GameSoundtrackBytes { get; set; }

        public GameFileTypes GameFileType { get; set; }

        public static string GetCategory(string SFOCategory)
        {
            switch (SFOCategory ?? "")
            {
                case "ac":
                    {
                        return "Additional Content";
                    }
                case "bd":
                    {
                        return "Blu-ray Disc";
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
                        return "Unknown";
                    }
                case "gdc":
                    {
                        return "Non-Game Big Application";
                    }
                case "gdd":
                    {
                        return "BG Application";
                    }
                case "gde":
                    {
                        return "Non-Game Mini App / Video Service Native App";
                    }
                case "gdk":
                    {
                        return "Video Service Web App";
                    }
                case "gdl":
                    {
                        return "PS Cloud Beta App";
                    }
                case "gdO":
                    {
                        return "PS2 Classic";
                    }
                case "gp":
                    {
                        return "Game Application Patch";
                    }
                case "gpc":
                    {
                        return "Non-Game Big App Patch";
                    }
                case "gpd":
                    {
                        return "BG Application patch";
                    }
                case "gpe":
                    {
                        return "Non-Game Mini App Patch / Video Service Native App Patch";
                    }
                case "gpk":
                    {
                        return "Video Service Web App Patch";
                    }
                case "gpl":
                    {
                        return "PS Cloud Beta App Patch";
                    }
                case "sd":
                    {
                        return "Save Data";
                    }
                case "la":
                    {
                        return "Live Area";
                    }
                case "wda":
                    {
                        return "Unknown";
                    }

                default:
                    {
                        return "Unknown";
                    }
            }
        }

        public static string GetGameRegion(string ContentID)
        {
            if (ContentID.StartsWith("EP"))
            {
                return "Europe";
            }
            else if (ContentID.StartsWith("UP"))
            {
                return "USA";
            }
            else if (ContentID.StartsWith("JP"))
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