using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace RRGControl.ViewModels
{
    public class ScriptsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ScriptsViewModel(Models.Scripts s)
        {
            mModel = s;
            mModel.Update();
            InitViewModels();
        }

        public ObservableCollection<SingleScriptViewModel> Items { get; } = new ObservableCollection<SingleScriptViewModel>();
        public ObservableCollection<SingleScriptViewModel> ChosenItems { get; } = new ObservableCollection<SingleScriptViewModel>();
        public IList? SelectedLeft { get; set; }
        public IList? SelectedRight { get; set; }
        public bool CanAdd => (SelectedLeft?.Count ?? 0) > 0;
        public bool CanRemove => (SelectedRight?.Count ?? 0) > 0;

        public void Add()
        {
            if (CanAdd)
            {
                foreach (var item in SelectedLeft)
                {
                    var i = ((SingleScriptViewModel)item);
                    ChosenItems.Add(i);
                    i.IsSelected = true;
                }
            }
            RaisePropChangedCan();
        }
        public void Remove()
        {
            if (CanRemove)
            {
                for (int i = 0; i < SelectedRight.Count; i++)
                {
                    SingleScriptViewModel s = (SingleScriptViewModel)(SelectedRight[i]);
                    ChosenItems.Remove(s);
                    if (!ChosenItems.Any(x => x.Name == s.Name)) s.IsSelected = false;
                }
            }
            RaisePropChangedCan();
        }
        public void Update()
        {
            mModel.Update();
            InitViewModels();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));
        }
        public void Invalidate()
        {
            RaisePropChangedCan();
        }
        public void Choose()
        {
            mModel.Choose(ChosenItems.Select(x => x.Name));
        }

        private Models.Scripts mModel;

        private void InitViewModels()
        {
            Items.Clear();
            foreach (var item in mModel.Items.Select(x => new SingleScriptViewModel(x)))
            {
                Items.Add(item);
            }
        }
        private void RaisePropChangedCan()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanRemove)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAdd)));
        }
    }
}
