using IronSoftware.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PSMultiTools.Classes
{

    public class NPSPKG : INotifyPropertyChanged
    {
        private string? _packageName;
        private string? _packageDescription;
        private string? _packageURL;
        private string? _packageTitleID;
        private string? _packageContentID;
        private string? _packageRAP;
        private string? _packagezRIF;
        private string? _packageReqFW;
        private string? _packageRegion;
        private string? _packageDate;
        private string? _packageDLCs;
        private string? _packageSize;
        private bool _isSelected;
        private string? _packageCoverSource;
        private AnyBitmap? _gameCoverSource;

        public string? PackageName
        {
            get => _packageName;
            set => SetProperty(ref _packageName, value);
        }

        public string? PackageDescription
        {
            get => _packageDescription;
            set => SetProperty(ref _packageDescription, value);
        }

        public string? PackageURL
        {
            get => _packageURL;
            set => SetProperty(ref _packageURL, value);
        }

        public string? PackageTitleID
        {
            get => _packageTitleID;
            set => SetProperty(ref _packageTitleID, value);
        }

        public string? PackageContentID
        {
            get => _packageContentID;
            set => SetProperty(ref _packageContentID, value);
        }

        public string? PackageRAP
        {
            get => _packageRAP;
            set => SetProperty(ref _packageRAP, value);
        }

        public string? PackagezRIF
        {
            get => _packagezRIF;
            set => SetProperty(ref _packagezRIF, value);
        }

        public string? PackageReqFW
        {
            get => _packageReqFW;
            set => SetProperty(ref _packageReqFW, value);
        }

        public string? PackageRegion
        {
            get => _packageRegion;
            set => SetProperty(ref _packageRegion, value);
        }

        public string? PackageDate
        {
            get => _packageDate;
            set => SetProperty(ref _packageDate, value);
        }

        public string? PackageDLCs
        {
            get => _packageDLCs;
            set => SetProperty(ref _packageDLCs, value);
        }

        public string? PackageSize
        {
            get => _packageSize;
            set => SetProperty(ref _packageSize, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string? PackageCoverSource
        {
            get => _packageCoverSource;
            set => SetProperty(ref _packageCoverSource, value);
        }

        public AnyBitmap? GameCoverSource
        {
            get => _gameCoverSource;
            set => SetProperty(ref _gameCoverSource, value);
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