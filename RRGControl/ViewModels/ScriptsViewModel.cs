using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using RRGControl.Models;
using RRGControl.Views;

namespace RRGControl.ViewModels
{
    public class ScriptsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ScriptsViewModel(Network network, Models.Scripts s)
        {
            mModel = s;
            mNetwork = network;
            mModel.Update();
            InitViewModels();
            foreach (var item in mModel.LastChosenNames)
            {
                var i = Items.FirstOrDefault(x => x.Name == item);
                if (i != null) mChosenBcp.Add(i);
            }
            Restore();
        }

        public ObservableCollection<SingleScriptViewModel> Items { get; private set; } 
            = new ObservableCollection<SingleScriptViewModel>();
        public ObservableCollection<SingleScriptViewModel> ChosenItems { get; private set; }
            = new ObservableCollection<SingleScriptViewModel>();
        public IList? SelectedLeft { get; set; }
        public IList? SelectedRight { get; set; }
        public bool CanAdd => (SelectedLeft?.Count ?? 0) > 0;
        public bool CanRemove => (SelectedRight?.Count ?? 0) > 0;
        public bool CanEdit => (SelectedLeft?.Count ?? 0) == 1;
        public Dictionary<int, Adapters.Packet[]> Compiled => mModel.Compiled;
        public int Duration => mModel.Duration;

        public void Add()
        {
            if (CanAdd)
            {
                SelectedRight?.Clear();
                if (SelectedLeft != null)
                {
                    foreach (var item in SelectedLeft)
                    {
                        var i = (SingleScriptViewModel)item;
                        ChosenItems.Add(i);
                        i.IsSelected = true;
                    }
                }
            }
            RaisePropChangedCan();
        }
        public void Remove()
        {
            if (CanRemove && (SelectedRight != null))
            {
                var sel = SelectedRight.Cast<SingleScriptViewModel>().ToList();
                SelectedRight.Clear();
                RaisePropChangedCan();
                for (int i = 0; i < sel.Count; i++)
                {
                    SingleScriptViewModel s = sel[i];
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
        public void Save()
        {
            mModel.Save();
            mChosenBcp.Clear();
            mChosenBcp.AddRange(ChosenItems);
        }
        public void Restore()
        {
            SelectedLeft?.Clear();
            SelectedRight?.Clear();
            mModel.Recall();
            foreach (var item in ChosenItems)
            {
                item.IsSelected = false;
            }
            ChosenItems.Clear();
            foreach (var item in mChosenBcp)
            {
                item.IsSelected = true;
                ChosenItems.Add(item);
            }
            RaisePropChangedCan();
        }
        public void Choose()
        {
            mModel.Choose(ChosenItems.Select(x => x.Name));
        }
        public CreateScript? GetEditorWindow()
        {
            var selectedScipt = SelectedLeft?.Cast<SingleScriptViewModel>().First().Model;
            if (selectedScipt == null) return null;
            var w = new CreateScript()
            {
                DataContext = new CreateScriptViewModel(mNetwork, selectedScipt)
            };
            return w;
        }

        private readonly Models.Scripts mModel;
        private readonly Network mNetwork;
        private readonly List<SingleScriptViewModel> mChosenBcp = new List<SingleScriptViewModel>();

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
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChosenItems)));
        }
    }
}
