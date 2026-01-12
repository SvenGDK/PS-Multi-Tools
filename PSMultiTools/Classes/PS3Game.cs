using IronSoftware.Drawing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PSMultiTools.Classes
{

    public partial class PS3Game
    {
        public enum GameFileTypes
        {
            Backup,
            PKG,
            PS3ISO,
            PS2ISO,
            PSXISO,
            PSPISO
        }

        public enum GameLocation
        {
            WebMANMOD,
            Local
        }

        public string? GameTitle { get; set; }

        public string? GameID { get; set; }

        public string? GameSize { get; set; }

        public string? GameRegion { get; set; }

        public string? GameFilePath { get; set; }

        public string? GameFolderPath { get; set; }

        public GameFileTypes GameFileType { get; set; }

        public string? GameCategory { get; set; }

        public string? GameRequiredFW { get; set; }

        public string? GameAppVer { get; set; }

        public string? GameVer { get; set; }

        public AnyBitmap? GameCoverSource { get; set; }

        public AnyBitmap? GameBackgroundSource { get; set; }

        public string? GameBackgroundPath { get; set; }

        public string? GameBackgroundSoundFile { get; set; }

        public byte[]? GameBackgroundSoundBytes { get; set; }

        public string? PKGType { get; set; }

        public string? ContentID { get; set; }

        public string? GameResolution { get; set; }

        public string? GameSoundFormat { get; set; }

        public double GridWidth { get; set; }

        public double GridHeight { get; set; }

        public double ImageWidth { get; set; }

        public double ImageHeight { get; set; }

        public GameLocation GameRootLocation { get; set; }

        public string? ISOEncryption { get; set; }

        #region Functions

        public static string GetCategory(string SFOCategory)
        {
            switch (SFOCategory ?? "")
            {
                case "DG":
                    {
                        return "Disc Game";
                    }
                case "AR":
                    {
                        return "Autoinstall Root";
                    }
                case "DP":
                    {
                        return "Disc Packages";
                    }
                case "IP":
                    {
                        return "Install Package";
                    }
                case "TR":
                    {
                        return "Theme Root";
                    }
                case "VR":
                    {
                        return "Vide Root";
                    }
                case "VI":
                    {
                        return "Video Item";
                    }
                case "XR":
                    {
                        return "Extra Root";
                    }
                case "DM":
                    {
                        return "Disc Movie";
                    }
                case "HG":
                    {
                        return "HDD Game";
                    }
                case "GD":
                    {
                        return "Game Data";
                    }
                case "SD":
                    {
                        return "Save Data";
                    }
                case "PP":
                    {
                        return "PSP";
                    }
                case "PE":
                    {
                        return "PSP Emulator";
                    }
                case "MN":
                    {
                        return "PSP Minis";
                    }
                case "1P":
                    {
                        return "PS1 PSN";
                    }
                case "2P":
                    {
                        return "PS2 PSN";
                    }

                default:
                    {
                        return "Unknown";
                    }
            }
        }

        public static string GetGameRegion(string GameID)
        {
            if (GameID.StartsWith("BLES"))
            {
                return "Europe";
            }
            else if (GameID.StartsWith("BCES"))
            {
                return "Europe";
            }
            else if (GameID.StartsWith("NPEB"))
            {
                return "Europe";
            }
            else if (GameID.StartsWith("BLUS"))
            {
                return "USA";
            }
            else if (GameID.StartsWith("BCUS"))
            {
                return "USA";
            }
            else if (GameID.StartsWith("NPUB"))
            {
                return "USA";
            }
            else if (GameID.StartsWith("BCJS"))
            {
                return "Japan";
            }
            else if (GameID.StartsWith("BLJS"))
            {
                return "Japan";
            }
            else if (GameID.StartsWith("NPJB"))
            {
                return "Japan";
            }
            else if (GameID.StartsWith("BCAS"))
            {
                return "Asia";
            }
            else if (GameID.StartsWith("BLAS"))
            {
                return "Asia";
            }
            else
            {
                return "";
            }
        }

        public static string GetGameResolution(string SFOResolution)
        {
            SFOResolution ??= string.Empty;
            var match = FormatRegex().Match(SFOResolution.TrimStart());
            double numeric = 0.0;

            if (match.Success)
            {
                double.TryParse(match.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out numeric);
            }

            long ResolutionValue = (long)Math.Round(numeric, 0, MidpointRounding.AwayFromZero);
            var SupportedResolutions = new List<string>();

            if ((ResolutionValue & 1L) == 1L) SupportedResolutions.Add("480p");
            if ((ResolutionValue & 2L) == 2L) SupportedResolutions.Add("576p");
            if ((ResolutionValue & 4L) == 4L) SupportedResolutions.Add("720p");
            if ((ResolutionValue & 8L) == 8L) SupportedResolutions.Add("1080p");
            if ((ResolutionValue & 16L) == 16L) SupportedResolutions.Add("480p (16:9)");
            if ((ResolutionValue & 32L) == 32L) SupportedResolutions.Add("576p (16:9)");
            if ((ResolutionValue & 63L) == 63L) SupportedResolutions.Add("All video modes supported");

            var sb = new StringBuilder();
            foreach (var resolution in SupportedResolutions)
                sb.Append(resolution).Append("\r\n");

            return "Supported Resolutions: " + Environment.NewLine + sb.ToString();
        }

        public static string GetGameSoundFormat(string SFOSoundFormat)
        {
            SFOSoundFormat ??= string.Empty;
            var match = FormatRegex().Match(SFOSoundFormat.TrimStart());
            double numeric = 0.0;

            if (match.Success)
            {
                double.TryParse(match.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out numeric);
            }

            long SoundValue = (long)Math.Round(numeric, MidpointRounding.AwayFromZero);
            var SupportedSoundFormats = new List<string>();

            if ((SoundValue & 1L) == 1L) SupportedSoundFormats.Add("2.0 LPCM");
            if ((SoundValue & 2L) == 2L) SupportedSoundFormats.Add("5.1 LPCM");
            if ((SoundValue & 8L) == 8L) SupportedSoundFormats.Add("7.1 LPCM");
            if ((SoundValue & 21L) == 21L) SupportedSoundFormats.Add("All LPCM modes");
            if ((SoundValue & 256L) == 256L) SupportedSoundFormats.Add("Dolby Digital 5.1");
            if ((SoundValue & 258L) == 258L) SupportedSoundFormats.Add("Only Dolby Digital 5.1");
            if ((SoundValue & 279L) == 279L) SupportedSoundFormats.Add("All LPCM modes + Dolby Digital 5.1");
            if ((SoundValue & 512L) == 512L) SupportedSoundFormats.Add("DTS 5.1");
            if ((SoundValue & 514L) == 514L) SupportedSoundFormats.Add("Only DTS 5.1");
            if ((SoundValue & 791L) == 791L) SupportedSoundFormats.Add("All sound modes");

            var sb = new StringBuilder();
            foreach (var formats in SupportedSoundFormats)
                sb.Append(formats).Append("\r\n");

            return "Supported Sound Formats: " + Environment.NewLine + sb.ToString();
        }

        [GeneratedRegex(@"^[+-]?(?:\d+(\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?")]
        private static partial Regex FormatRegex();

        #endregion

    }
}