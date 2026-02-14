using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RRGControl.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public static event EventHandler<string>? LogEvent;
        public new event PropertyChangedEventHandler? PropertyChanged;

        private readonly Models.Scripts mScript;
        private readonly Models.Network mNetwork;
        private readonly string[] mScriptProps =
        {
            nameof(ScriptStartEnable),
            nameof(ScriptStopEnable),
            nameof(ScriptPauseEnable),
            nameof(ScriptConfigureEnable),
            nameof(ScriptProgress),
            nameof(ShowProgressBar),
            nameof(BarColor)
        };
        private string mStatus = "Ready.";
        public MainWindowViewModel(Models.Network n, Models.Scripts s)
        {
            mNetwork = n;
            mScript = s;
            Scripts = new ScriptsViewModel(mNetwork, mScript);
            Connections = new List<ConnectionViewModel>(n.Connections.Count);
            foreach (var item in n.Connections)
            {
                try
                {
                    var c = new ConnectionViewModel(item);
                    c.PropertyChanged += C_PropertyChanged;
                    Connections.Add(c);
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(this, ex.ToString());
                }
            }
            s.PropertyChanged += S_PropertyChanged;
            s.ExecutionFinished += S_ExecutionFinished;
            base.PropertyChanged += PropertyChanged;
        }

        private void S_ExecutionFinished(object? sender, EventArgs e)
        {
            Status = "Script execution finished.";
        }
        private void S_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            foreach (var item in mScriptProps)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(item));
            }
        }

        private void C_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public ScriptsViewModel Scripts { get; }
        public List<ConnectionViewModel> Connections { get; }
        public string Status
        {
            get => mStatus;
            set
            {
                mStatus = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }
        public bool ScriptStartEnable
        {
            get => mScript.State == Adapters.ScriptAdapterState.Paused
                || mScript.State == Adapters.ScriptAdapterState.Stopped;
        }
        public bool ScriptPauseEnable
        {
            get => mScript.State == Adapters.ScriptAdapterState.Running;
        }
        public bool ScriptStopEnable
        {
            get => mScript.State == Adapters.ScriptAdapterState.Running
                || mScript.State == Adapters.ScriptAdapterState.Paused;
        }
        public bool ScriptConfigureEnable
        {
            get => mScript.State == Adapters.ScriptAdapterState.Stopped;
        }
        public int ScriptProgress => mScript.Progress;
        public bool ShowProgressBar => ScriptStopEnable;
        public IBrush BarColor => mScript.State == Adapters.ScriptAdapterState.Running ? App.GreenOK : App.OrangeWarning;
        public bool ShowGenerateExamples => ConfigProvider.Settings.ExampleGenerationAvailable;

        public void ScriptStart()
        {
            mScript.Start();
            Status = "Executing script...";
        }
        public void ScriptPause()
        {
            mScript.Pause();
            Status = "Script paused.";
        }
        public void ScriptStop()
        {
            mScript.Stop();
            Status = "Script stopped.";
        }
        public async Task RescanNetwork()
        {
            try
            {
                Status = "Scanning network...";
                await mNetwork.Scan();
                Status = "Network scan OK.";
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.ToString());
                Status = "Network scan failed";
            }
        }
        public async Task ReadAll()
        {
            try
            {
                Status = "Reading all unit registers, please wait...";
                await mNetwork.ReadAll();
                Status = "Reading all units OK.";
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.ToString());
                Status = "Reading all units failed";
            }
        }
    }
}
