using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel;

namespace RRGControl.Models
{
    public class Scripts : INotifyPropertyChanged
    {
        public static event EventHandler<string> LogEvent;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? ExecutionFinished;

        public Scripts(Adapters.ScriptAdapter a, string folder)
        {
            mAdapter = a;
            mFolder = new DirectoryInfo(folder);
            mAdapter.PropertyChanged += MAdapter_PropertyChanged;
            mAdapter.ExecutionFinished += ExecutionFinished;
        }

        public Dictionary<int, Adapters.Packet> Compiled => mAdapter.Compiled ?? new Dictionary<int, Adapters.Packet>();
        public List<Adapters.Script> Items { get; private set; } = new List<Adapters.Script>();

        public void Update()
        {
            try
            {
                Items = mFolder.EnumerateFiles(ConfigProvider.FilenameFilter, SearchOption.AllDirectories)
                    .Select(x => Adapters.Script.FromJson(File.ReadAllText(x.FullName))).Where(x => x != null).Reverse().ToList();
            }
            catch (DirectoryNotFoundException ex)
            {
                LogEvent?.Invoke(this, ex.Message);
            }
        }
        public void Choose(IEnumerable<string> names)
        {
            List<Adapters.Script> l = new List<Adapters.Script>();
            foreach (var item in names)
            {
                var cur = Items.FirstOrDefault(x => x.Name == item);
                if (cur != null) l.Add(cur);
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

        private readonly Adapters.ScriptAdapter mAdapter;
        private DirectoryInfo mFolder;

        private void MAdapter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
