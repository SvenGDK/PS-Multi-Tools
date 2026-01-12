using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;

namespace PSMultiTools.Classes
{

    public class PKGDownloadListViewItem : INotifyPropertyChanged
    {

        private string? _PackageTitleID;
        private string? _PackageName;
        private string? _PackageSize;
        private string? _PackageDownloadState;
        private WebClient? _AssociatedWebClient;
        private string? _PackageContentID;
        private string? _PackageDownloadDestination;

        public WebClient? AssociatedWebClient
        {
            get => _AssociatedWebClient;
            set => SetProperty(ref _AssociatedWebClient, value);
        }

        public string? PackageContentID
        {
            get => _PackageContentID;
            set => SetProperty(ref _PackageContentID, value);
        }

        public string? PackageTitleID
        {
            get => _PackageTitleID;
            set => SetProperty(ref _PackageTitleID, value);
        }

        public string? PackageName
        {
            get => _PackageName;
            set => SetProperty(ref _PackageName, value);
        }

        public string? PackageSize
        {
            get => _PackageSize;
            set => SetProperty(ref _PackageSize, value);
        }

        public string? PackageDownloadState
        {
            get => _PackageDownloadState;
            set => SetProperty(ref _PackageDownloadState, value);
        }

        public string? PackageDownloadDestination
        {
            get => _PackageDownloadDestination;
            set => SetProperty(ref _PackageDownloadDestination, value);
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