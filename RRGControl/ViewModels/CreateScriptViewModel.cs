using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using RRGControl.Models;
using System;
using System.Reactive.Linq;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.ComponentModel;

namespace RRGControl.ViewModels
{
    public class CreateScriptViewModel : ViewModelBase
    {
        public event EventHandler? PlotUpdateRequested;

        public CreateScriptViewModel(Network network)
        {
            mNetwork = network;
            AddCommand = ReactiveCommand.Create(AddCommandExecute);
            RemoveCommand = ReactiveCommand.Create(RemoveCommandExecute);
            SaveScriptCommand = ReactiveCommand.CreateFromTask(SaveScriptExecute);
            Commands.CollectionChanged += (s, e) =>
            {
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        (item as ScriptCommandViewModel)!.PropertyChanged -= OnCommandPropertyChanged;
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        (item as ScriptCommandViewModel)!.PropertyChanged += OnCommandPropertyChanged;
                    }
                }
                this.RaisePropertyChanged(nameof(CanSave));
                ConstructScriptAndPreviews();
            };
        }

        private const string MessageBoxTitle = "Script GUI";
        private readonly Network mNetwork;
        private readonly Adapters.Script mScriptUnderConstruction = new();

        [Required]
        public string ScriptName
        {
            get => mScriptUnderConstruction.Name;
            set
            {
                mScriptUnderConstruction.Name = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(CanSave));
            }
        }
        public string ScriptComment
        {
            get => mScriptUnderConstruction.Comment;
            set => mScriptUnderConstruction.Comment = value;
        }
        public ObservableCollection<ScriptCommandViewModel> Commands { get; } = new();
        private ScriptCommandViewModel? _selectedCommand = null;
        public ScriptCommandViewModel? SelectedCommand
        { 
            get => _selectedCommand;
            set
            {
                if (_selectedCommand == value) return;
                this.RaiseAndSetIfChanged(ref _selectedCommand, value);
                this.RaisePropertyChanged(nameof(CanDelete));
                this.RaisePropertyChanged(nameof(CanEdit));
            }
        }
        public int SelectedCommandIndex { get; set; } = -1;
        public bool CanDelete => SelectedCommand != null;
        public bool CanSave => (Commands.Count > 0) && (ScriptName.Length > 0);
        public bool CanEdit => SelectedCommand != null;
        public ReactiveCommand<Unit, Unit> AddCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveScriptCommand { get; }
        public ScriptPreviewViewModel PreviewViewModel { get; } = new ScriptPreviewViewModel();
        private string _previewJson = string.Empty;
        public string PreviewJson
        {
            get => _previewJson;
            set
            {
                this.RaiseAndSetIfChanged(ref _previewJson, value);
            }
        }

        private void AddCommandExecute()
        {
            var svm = new ScriptCommandViewModel(new ScriptCommand(10, mNetwork.UnitsByName.Select(x => new UnitSetpoint(x.Key))));
            if (SelectedCommandIndex > -1)
            {
                Commands.Insert(SelectedCommandIndex + 1, svm);
            }
            else
            {
                Commands.Add(svm);
            }
            SelectedCommand = svm;
        }
        private void RemoveCommandExecute()
        {
            if (SelectedCommand == null) return;
            Commands.Remove(SelectedCommand); 
            SelectedCommand = null;
        }
        private async Task SaveScriptExecute()
        {
            if (ScriptName.Length <= 0)
            {
                await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(MessageBoxTitle, "Script Name can't be empty!").ShowAsync();
                return;
            }
            try
            {
                ConfigProvider.SaveNewScript(mScriptUnderConstruction, false, PreviewJson.Length > 0 ? PreviewJson : null);
            }
            catch (FileExistsException ex)
            {
                var mbResult = await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(MessageBoxTitle,
                    $"Script with this name already exists ({ex.Message}).\nOverwrite?", MsBox.Avalonia.Enums.ButtonEnum.YesNo).ShowAsync();
                if (mbResult == MsBox.Avalonia.Enums.ButtonResult.Yes)
                {
                    ConfigProvider.SaveNewScript(mScriptUnderConstruction, true, PreviewJson.Length > 0 ? PreviewJson : null);
                }
            }
            catch (Exception ex)
            {
                await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(MessageBoxTitle, $"Failed to save script:\n{ex}").ShowAsync();
            }
        }
        private void OnCommandPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ConstructScriptAndPreviews();
        }
        private void ConstructScriptAndPreviews()
        {
            mScriptUnderConstruction.Commands.Clear();
            mScriptUnderConstruction.Commands.AddRange(Commands.Select(x => x.Command.GetScriptAdapterElement()));
            PreviewViewModel.UpdatePreview(mScriptUnderConstruction.Compile(), mScriptUnderConstruction.GetDuration(), mNetwork, 0);
            PlotUpdateRequested?.Invoke(this, new EventArgs());
            PreviewJson = ConfigProvider.Serialize(mScriptUnderConstruction);
        }
    }
}