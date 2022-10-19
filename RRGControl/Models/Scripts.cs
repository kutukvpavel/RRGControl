using MoreLinq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace RRGControl.Models
{
    public class Scripts : INotifyPropertyChanged
    {
        public static event EventHandler<string>? LogEvent;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? ExecutionFinished;

        public Scripts(Adapters.ScriptAdapter a, string scriptsFolder, string csvsFolder, List<MyModbus.RRGUnitMapping> m,
            CancellationToken t)
        {
            mToken = t;
            mAdapter = a;
            mFolder = new DirectoryInfo(scriptsFolder);
            mAdapter.PropertyChanged += MAdapter_PropertyChanged;
            mAdapter.ExecutionFinished += MAdapter_ExecutionFinished;
            mAdapter.PacketSent += MAdapter_PacketSent;
            mCsvsFolder = csvsFolder;
            mUnitNames = m.Select(x => x.Units.Select(x => x.Value.Name)).Flatten().Cast<string>().ToArray();
            mCsvThread = new Thread(CsvThread);
            mCsvThread.Start();
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
            if (State == Adapters.ScriptAdapterState.Stopped)
            {
                lock (mCsvQueue)
                {
                    try
                    {
                        string name = ReplaceAll(mAdapter.Script?.Name ?? "null", Path.GetInvalidFileNameChars(), '_');
                        mCurrentCsv = new FileInfo(Path.Combine(mCsvsFolder, $"{DateTime.Now:yyyy-MM-dd HH-mm-ss} - {name}.csv"));
                        mCurrentCsv.Directory?.Create();
                        File.WriteAllText(mCurrentCsv.FullName, "Timestamp,ScriptProgress,UnitName,RegisterName,RegisterValue," +
                            string.Join(',', mUnitNames));
                    }
                    catch (Exception ex)
                    {
                        mCurrentCsv = null;
                        LogEvent?.Invoke(this, ex.ToString());
                    }
                }
            }
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
        private readonly string mCsvsFolder;
        private readonly string[] mUnitNames;
        private readonly BlockingCollection<string> mCsvQueue = new BlockingCollection<string>();
        private readonly CancellationToken mToken;
        private readonly Thread mCsvThread;
        private FileInfo? mCurrentCsv = null;

        private void MAdapter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
        private void MAdapter_ExecutionFinished(object? sender, EventArgs e)
        {
            ExecutionFinished?.Invoke(this, new EventArgs());
        }
        private void MAdapter_PacketSent(object? sender, Adapters.Packet e)
        {
            //time, unit, reg, value, [setpoints per unit]
            var line = new StringBuilder($"{DateTime.Now},{mAdapter.Progress},{e.UnitName},{e.RegisterName},{e.Value}");
            foreach (var item in mUnitNames)
            {
                if (e.RegisterName == ConfigProvider.MeasuredRegName && item == e.UnitName)
                {
                    line.Append($",{e.Value}");
                }
                else
                {
                    line.Append(',');
                }
            }
            line.Append('\n');
            mCsvQueue.Add(line.ToString());
        }
        private void CsvThread()
        {
            while (mAdapter.State != Adapters.ScriptAdapterState.Cancelled && !mToken.IsCancellationRequested)
            {
                try
                {
                    string line = mCsvQueue.Take(mToken);
                    try
                    {
                        lock (mCsvQueue)
                        {
                            if (mCurrentCsv == null) continue;
                            File.AppendAllText(mCurrentCsv.FullName, line);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogEvent?.Invoke(this, $"CsvThread: {ex}");
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        private static string ReplaceAll(string seed, char[] chars, char replacementCharacter)
        {
            return chars.Aggregate(seed, (str, cItem) => str.Replace(cItem, replacementCharacter));
        }
    }
}
