using IronSoftware.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace PSMultiTools.Classes
{

    public class PS1Game : INotifyPropertyChanged
    {
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

        public static string GetRegionChar(string GameID)
        {
            if (GameID.StartsWith("SLES", StringComparison.OrdinalIgnoreCase))
            {
                return "P";
            }
            else if (GameID.StartsWith("SCES", StringComparison.OrdinalIgnoreCase))
            {
                return "P";
            }
            else if (GameID.StartsWith("SLUS", StringComparison.OrdinalIgnoreCase))
            {
                return "U";
            }
            else if (GameID.StartsWith("SCUS", StringComparison.OrdinalIgnoreCase))
            {
                return "U";
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

        public static string GetPS1GameTitleFromDatabaseList(string GameID)
        {
            string FoundGameTitle = "";

            foreach (string GameTitle in File.ReadLines(Path.Combine(Environment.CurrentDirectory, "Tools", "ps1ids.txt")))
            {
                if (GameTitle.Contains(GameID))
                {
                    FoundGameTitle = GameTitle.Split(';')[1];
                    break;
                }
            }

            if (string.IsNullOrEmpty(FoundGameTitle))
            {
                return "";
            }
            else
            {
                return FoundGameTitle;
            }
        }

        public static string IsGameProtected(string GameID)
        {
            string FoundValue = "";

            foreach (string GameIDInFile in File.ReadLines(Path.Combine(Environment.CurrentDirectory, "Tools", "libcrypt.txt")))
            {
                if (GameIDInFile.Contains(GameID))
                {
                    FoundValue = GameIDInFile.Split(' ')[1];
                    break;
                }
            }

            return FoundValue;
        }

    }
}