using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSserver
{
    class MonitorDir : GenericRequest
    {
        public List<string> monitorDir { get; set; }
        public string oldPath { get; set; }
        public string newPath { get; set; }
        public string path { get; set; }

        public MonitorDir()
        {
        }

        public MonitorDir(List<string> monitorDir)
        {
            this.monitorDir = monitorDir;
        }

        public MonitorDir(string oldPath, string newPath)
        {
            this.oldPath = oldPath;
            this.newPath = newPath;
        }

        public MonitorDir(string path)
        {
            this.path = path;
        }
    }
}
