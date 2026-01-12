using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PSMultiTools.Classes
{
    public class ComboBoxProjectItem : INotifyPropertyChanged
    {

        private string? _ProjectFile;
        private string? _ProjectName;

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

        public string? ProjectName
        {
            get => _ProjectName;
            set => SetProperty(ref _ProjectName, value);
        }

        public string? ProjectFile
        {
            get => _ProjectFile;
            set => SetProperty(ref _ProjectFile, value);
        }

    }
}
