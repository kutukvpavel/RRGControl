using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Collections.Generic;
using RRGControl.Models;
using System;
using System.Reactive.Linq;
using System.Linq;
using RRGControl.MyModbus;

namespace RRGControl.ViewModels
{
    public class CreateScriptViewModel : ViewModelBase
    {
        public event EventHandler? PlotUpdateRequested;

        public CreateScriptViewModel(Network network)
        {
            mUnitsByName = network.UnitsByName;
            mNewCommand = new(mUnitsByName.Keys.Select(x => new UnitSetpoint(x)));
            mCommandUnderConstruction = mNewCommand;
            AddCommand = ReactiveCommand.Create(AddCommandExecute);
            RemoveCommand = ReactiveCommand.Create(RemoveCommandExecute);
            SaveScriptCommand = ReactiveCommand.Create(SaveScriptExecute);
            Commands.CollectionChanged += (s, e) => ConstructScript();
        }

        private readonly Dictionary<string, RRGUnit> mUnitsByName;
        private readonly Adapters.Script mScriptUnderConstruction = new();
        private readonly ScriptCommand mNewCommand;
        private ScriptCommand mCommandUnderConstruction;

        public string ScriptName
        {
            get => mScriptUnderConstruction.Name;
            set => mScriptUnderConstruction.Name = value;
        }
        public string ScriptComment
        {
            get => mScriptUnderConstruction.Comment;
            set => mScriptUnderConstruction.Comment = value;
        }
        public ObservableCollection<ScriptCommandViewModel> Commands { get; } = new();
        public int NewDuration
        {
            get => mCommandUnderConstruction.Duration;
            set => mCommandUnderConstruction.Duration = value;
        }
        public ObservableCollection<UnitSetpoint> SetpointsUnderConstruction => mCommandUnderConstruction.UnitSetpoints;
        private ScriptCommandViewModel? _selectedCommand = null;
        public ScriptCommandViewModel? SelectedCommand
        { 
            get => _selectedCommand;
            set
            {
                if (_selectedCommand == value) return;
                this.RaiseAndSetIfChanged(ref _selectedCommand, value);
                mCommandUnderConstruction = _selectedCommand == null ? mNewCommand : _selectedCommand.Command;
                this.RaisePropertyChanged(nameof(NewDuration));
                this.RaisePropertyChanged(nameof(SetpointsUnderConstruction));
            }
        }
        public ReactiveCommand<Unit, Unit> AddCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveScriptCommand { get; }
        public ScriptPreviewViewModel PreviewViewModel { get; } = new ScriptPreviewViewModel();

        private void AddCommandExecute()
        {
            Commands.Add(new ScriptCommandViewModel(new ScriptCommand(mNewCommand.Duration, mNewCommand.UnitSetpoints)));
        }
        private void RemoveCommandExecute()
        {
            if (SelectedCommand == null) return;
            Commands.Remove(SelectedCommand); 
            SelectedCommand = null;
        }
        private void SaveScriptExecute()
        {
            
        }

        private void ConstructScript()
        {
            mScriptUnderConstruction.Commands.Clear();
            mScriptUnderConstruction.Commands.AddRange(Commands.Select(x => x.Command.GetScriptAdapterElement()));
            PlotUpdateRequested?.Invoke(this, new EventArgs());
        }
    }
}