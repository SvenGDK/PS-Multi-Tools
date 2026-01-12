using IronSoftware.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace PSMultiTools.Classes
{

    public class PS2Game : INotifyPropertyChanged
    {
        public enum GameFileType
        {
            ISO,
            CSO
        }

        private string? _gameTitle;
        private string? _gameID;
        private string? _gameSize;
        private string? _gameRegion;
        private string? _gameFilePath;
        private string? _gameFolderPath;
        private AnyBitmap? _gameCoverSource;
        private string? _gameGenre;
        private string? _gameDeveloper;
        private string? _gamePublisher;
        private string? _gameReleaseDate;
        private string? _gameDescription;
        private GameFileType _gameBackupType;
        private string? _gameWebsite;
        private string? _gameCoverURL;
        private string? _assignedPartitionDriveLetter;
        private string? _partitionName;

        public string? GameTitle
        {
            get => _gameTitle;
            set => SetProperty(ref _gameTitle, value);
        }

        public string? GameID
        {
            get => _gameID;
            set => SetProperty(ref _gameID, value);
        }

        public string? GameSize
        {
            get => _gameSize;
            set => SetProperty(ref _gameSize, value);
        }

        public string? GameRegion
        {
            get => _gameRegion;
            set => SetProperty(ref _gameRegion, value);
        }

        public string? GameFilePath
        {
            get => _gameFilePath;
            set => SetProperty(ref _gameFilePath, value);
        }

        public string? GameFolderPath
        {
            get => _gameFolderPath;
            set => SetProperty(ref _gameFolderPath, value);
        }

        public AnyBitmap? GameCoverSource
        {
            get => _gameCoverSource;
            set => SetProperty(ref _gameCoverSource, value);
        }

        public string? GameGenre
        {
            get => _gameGenre;
            set => SetProperty(ref _gameGenre, value);
        }

        public string? GameDeveloper
        {
            get => _gameDeveloper;
            set => SetProperty(ref _gameDeveloper, value);
        }

        public string? GamePublisher
        {
            get => _gamePublisher;
            set => SetProperty(ref _gamePublisher, value);
        }

        public string? GameReleaseDate
        {
            get => _gameReleaseDate;
            set => SetProperty(ref _gameReleaseDate, value);
        }

        public string? GameDescription
        {
            get => _gameDescription;
            set => SetProperty(ref _gameDescription, value);
        }

        public GameFileType GameBackupType
        {
            get => _gameBackupType;
            set => SetProperty(ref _gameBackupType, value);
        }

        public string? GameWebsite
        {
            get => _gameWebsite;
            set => SetProperty(ref _gameWebsite, value);
        }

        public string? GameCoverURL
        {
            get => _gameCoverURL;
            set => SetProperty(ref _gameCoverURL, value);
        }

        public string? AssignedPartitionDriveLetter
        {
            get => _assignedPartitionDriveLetter;
            set => SetProperty(ref _assignedPartitionDriveLetter, value);
        }

        public string? PartitionName
        {
            get => _partitionName;
            set => SetProperty(ref _partitionName, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static string GetGameRegionByGameID(string GameID)
        {
            if (GameID.StartsWith("SLES", StringComparison.OrdinalIgnoreCase))
            {
                return "E";
            }
            else if (GameID.StartsWith("SCES", StringComparison.OrdinalIgnoreCase))
            {
                return "E";
            }
            else if (GameID.StartsWith("SLUS", StringComparison.OrdinalIgnoreCase))
            {
                return "U";
            }
            else if (GameID.StartsWith("SCUS", StringComparison.OrdinalIgnoreCase))
            {
                return "U";
            }
            else if (GameID.StartsWith("SCPS", StringComparison.OrdinalIgnoreCase))
            {
                return "J";
            }
            else if (GameID.StartsWith("SLPS", StringComparison.OrdinalIgnoreCase))
            {
                return "J";
            }
            else if (GameID.StartsWith("SLPM", StringComparison.OrdinalIgnoreCase))
            {
                return "J";
            }
            else if (GameID.StartsWith("SCCS", StringComparison.OrdinalIgnoreCase))
            {
                return "J";
            }
            else if (GameID.StartsWith("SLKA", StringComparison.OrdinalIgnoreCase))
            {
                return "J";
            }
            else
            {
                return "";
            }
        }

        public static string GetPS2GameID(string GameISO)
        {
            string GameID = "";

            try
            {
                using (var SevenZip = new Process())
                {
                    SevenZip.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "7z.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "7zz");
                    SevenZip.StartInfo.Arguments = "l -ba \"" + GameISO + "\"";
                    SevenZip.StartInfo.RedirectStandardOutput = true;
                    SevenZip.StartInfo.UseShellExecute = false;
                    SevenZip.StartInfo.CreateNoWindow = true;
                    SevenZip.Start();

                    // Read the output
                    var OutputReader = SevenZip.StandardOutput;
                    string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.None);

                    if (ProcessOutput.Length > 0)
                    {
                        foreach (string Line in ProcessOutput)
                        {
                            if (Line.Contains("SLES_") | Line.Contains("SLUS_") | Line.Contains("SCES_") | Line.Contains("SCUS_"))
                            {
                                if (Line.Contains("Volume:")) // ID found in the ISO Header
                                {
                                    if (Line.Split(["Volume: "], StringSplitOptions.RemoveEmptyEntries).Length > 0)
                                    {
                                        GameID = Line.Split(["Volume: "], StringSplitOptions.RemoveEmptyEntries)[1];
                                        break;
                                    }
                                }
                                else if (string.Join(" ", Line.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries)).Split(' ').Length > 4) // ID found in the ISO files
                                {
                                    GameID = string.Join(" ", Line.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries)).Split(' ')[5].Trim();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            return GameID;
        }

        public static string GetPS2GameTitleFromDatabaseList(string GameID)
        {
            string FoundGameTitle = "";
            GameID = GameID.Replace("-", "");

            foreach (string GameTitle in File.ReadLines(Path.Combine(Environment.CurrentDirectory, "Tools", "ps2ids.txt")))
            {
                if (GameTitle.Contains(GameID))
                {
                    FoundGameTitle = GameTitle.Split(';')[1];
                    break;
                }
            }

            if (string.IsNullOrEmpty(FoundGameTitle))
            {
                return "Unknown PS2 game";
            }
            else
            {
                return FoundGameTitle;
            }
        }

    }
}