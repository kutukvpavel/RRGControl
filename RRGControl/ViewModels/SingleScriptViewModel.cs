using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace RRGControl.ViewModels
{
    public class SingleScriptViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public SingleScriptViewModel(Adapters.Script s)
        {
            mScript = s;
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontColor)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontWeight)));
            }
        }
        public IBrush FontColor => IsSelected ? Brushes.Green : Brushes.Black;
        public FontWeight FontWeight => IsSelected ? FontWeight.Bold : FontWeight.Normal;

        private Adapters.Script mScript;
        private bool mSelected = false;
    }
}
