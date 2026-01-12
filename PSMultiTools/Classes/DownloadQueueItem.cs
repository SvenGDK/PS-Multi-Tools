using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PSMultiTools.Classes
{

    public class DownloadQueueItem : INotifyPropertyChanged
    {

        private string? _GameID;
        private string? _FileName;
        private string? _PKGSize;
        private string? _DownloadURL;
        private string? _MergeState;
        private string? _DownloadState;

        public string? GameID
        {
            get => _GameID;
            set => SetProperty(ref _GameID, value);
        }

        public string? FileName
        {
            get => _FileName;
            set => SetProperty(ref _FileName, value);
        }

        public string? PKGSize
        {
            get => _PKGSize;
            set => SetProperty(ref _PKGSize, value);
        }

        public string? DownloadURL
        {
            get => _DownloadURL;
            set => SetProperty(ref _DownloadURL, value);
        }

        public string? DownloadState
        {
            get => _DownloadState;
            set => SetProperty(ref _DownloadState, value);
        }

        public string? MergeState
        {
            get => _MergeState;
            set => SetProperty(ref _MergeState, value);
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

    }
}