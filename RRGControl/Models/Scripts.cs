using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel;

namespace RRGControl.Models
{
    public class Scripts : INotifyPropertyChanged
    {
        public static event EventHandler<string>? LogEvent;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? ExecutionFinished;

        public Scripts(Adapters.ScriptAdapter a, string folder)
        {
            mAdapter = a;
            mFolder = new DirectoryInfo(folder);
            mAdapter.PropertyChanged += MAdapter_PropertyChanged;
            mAdapter.ExecutionFinished += MAdapter_ExecutionFinished;
        }

        public int Duration => mAdapter.Script?.GetDuration() ?? 0;
        public Dictionary<int, Adapters.Packet> Compiled => mAdapter.Compiled ?? new Dictionary<int, Adapters.Packet>();
        public List<Adapters.Script> Items { get; private set; } = new List<Adapters.Script>();
        public List<string> LastChosenNames => ConfigProvider.LastScripts;

        public void Update()
        {
            try
            {
                var res = new List<Adapters.Script>(Items.Capacity);
                foreach (var item in mFolder.EnumerateFiles(ConfigProvider.FilenameFilter, SearchOption.AllDirectories))
                {
                    try
                    {
                        var s = Adapters.Script.FromJson(File.ReadAllText(item.FullName));
                        if (s != null) res.Add(s);
                    }
                    catch (Exception ex)
                    {
                        LogEvent?.Invoke(this, ex.Message);
                    }
                }
                Items = res;
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.Message);
                Items = new List<Adapters.Script>();
            }
        }
        public void Choose(IEnumerable<string> names)
        {
            List<Adapters.Script> l = new List<Adapters.Script>();
            LastChosenNames.Clear();
            foreach (var item in names)
            {
                var cur = Items.FirstOrDefault(x => x.Name == item);
                if (cur != null) l.Add(cur);
                LastChosenNames.Add(item);
            }
            mAdapter.Script = new Adapters.Script(l);
        }
        public Adapters.ScriptAdapterState State => mAdapter.State;
        public int Progress => (int)Math.Round((mAdapter.Progress ?? 0) * 100);
        public void Start()
        {
            mAdapter.Start();
        }
        public void Pause()
        {
            mAdapter.Pause();
        }
        public void Stop()
        {
            mAdapter.Stop();
        }
        public void Save()
        {
            (App.Current as App)?.SaveLastUsedScripts();
        }
        public void Recall()
        {
            Choose(ConfigProvider.LastScripts);
        }

        private readonly Adapters.ScriptAdapter mAdapter;
        private readonly DirectoryInfo mFolder;

        private void MAdapter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
        private void MAdapter_ExecutionFinished(object? sender, EventArgs e)
        {
            ExecutionFinished?.Invoke(this, new EventArgs());
        }
    }
}
