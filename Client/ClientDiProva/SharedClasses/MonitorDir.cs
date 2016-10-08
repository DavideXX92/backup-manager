using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class MonitorDir : GenericRequest
    {
        public string monitorDir { get; set; }

        public MonitorDir()
        {
        }

        public MonitorDir(string monitorDir)
        {
            this.monitorDir = monitorDir;
        }
    }
}
