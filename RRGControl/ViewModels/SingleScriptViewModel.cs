using Avalonia.Media;
using System;
using System.ComponentModel;

namespace RRGControl.ViewModels
{
    public class SingleScriptViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public SingleScriptViewModel(Adapters.Script s)
        {
            mScript = s;
            if (App.Current != null) App.Current.ActualThemeVariantChanged += ThemeVariant_Changed;
        }

        public string Name => mScript.Name;
        public string Comment => mScript.Comment;
        public string Duration => mScript.GetDuration().ToString();
        public bool IsSelected
        {
            get => mSelected;
            set
            {
                mSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontWeight)));
                UpdateColors();
            }
        }
        public IBrush FontColor => IsSelected ? (App.IsDarkThemed ? Brushes.LightGreen : Brushes.Green) : (App.IsDarkThemed ? Brushes.White : Brushes.Black);
        public FontWeight FontWeight => IsSelected ? FontWeight.Bold : FontWeight.Normal;

        private Adapters.Script mScript;
        private bool mSelected = false;

        private void UpdateColors()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontColor)));
        }
        private void ThemeVariant_Changed(object? sender, EventArgs e)
        {
            UpdateColors();
        }
    }
}
