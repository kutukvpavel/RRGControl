using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRGControl.Models
{
    public class Scripts
    {
        public Scripts(Adapters.ScriptAdapter a)
        {
            mAdapter = a;
        }

        public Dictionary<int, Adapters.Packet> Compiled => mAdapter.Compiled ?? new Dictionary<int, Adapters.Packet>();
        public List<Adapters.Script> Items { get; private set; } = new List<Adapters.Script>();

        public void Set(List<Adapters.Script> src)
        {
            mAdapter.Script = new Adapters.Script(src);
            Items = src;
        }

        private readonly Adapters.ScriptAdapter mAdapter;
    }
}
