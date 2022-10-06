using Avalonia.Controls.Selection;
using System;
using System.Collections;
using System.Collections.Generic;
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

        public ObservableCollection<SingleScriptViewModel> Items { get; private set; } 
            = new ObservableCollection<SingleScriptViewModel>();
        public ObservableCollection<SingleScriptViewModel> ChosenItems { get; private set; }
            = new ObservableCollection<SingleScriptViewModel>();
        public IList? SelectedLeft { get; set; }
        public IList? SelectedRight { get; set; }
        public bool CanAdd => (SelectedLeft?.Count ?? 0) > 0;
        public bool CanRemove => (SelectedRight?.Count ?? 0) > 0;
        public Dictionary<int, Adapters.Packet> Compiled => mModel.Compiled;

        public void Add()
        {
            if (CanAdd)
            {
                SelectedRight.Clear();
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
                var sel = SelectedRight.Cast<SingleScriptViewModel>().ToList();
                SelectedRight.Clear();
                RaisePropChangedCan();
                for (int i = 0; i < sel.Count; i++)
                {
                    SingleScriptViewModel s = sel[i];
                    try
                    {
                        ChosenItems.Remove(s);
                    }
                    catch (ArgumentOutOfRangeException)
                    { }
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
        public void Save()
        {
            mModel.Push();
            mChosenBcp = ChosenItems;
        }
        public void Restore()
        {
            mModel.Pop();
            SelectedLeft?.Clear();
            SelectedRight?.Clear();
            ChosenItems = mChosenBcp;
            RaisePropChangedCan();
        }
        public void Choose()
        {
            mModel.Choose(ChosenItems.Select(x => x.Name));
        }

        private readonly Models.Scripts mModel;
        private ObservableCollection<SingleScriptViewModel> mChosenBcp = new ObservableCollection<SingleScriptViewModel>();

        private void InitViewModels()
        {
            SelectedLeft?.Clear();
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChosenItems)));
        }
    }
}
