using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class Dir
    {
        public int idDir { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public DateTime creationTime { get; set; }
        public DateTime lastWriteTime { get; set; }
        public Dir parentDir { get; set; }
        public List<Dir> elencoSubdirectory;
        public List<File> elencoFile;

        public Dir()
        {
        }

        public Dir(string path, Dir parentDir)
        {
            this.path = path;
            this.name = path.Substring(path.LastIndexOf(@"\"));
            this.parentDir = parentDir;
            elencoSubdirectory = new List<Dir>();
            elencoFile = new List<File>();
            this.creationTime = Directory.GetCreationTime(path);
            this.lastWriteTime = Directory.GetLastWriteTime(path);
        }

        public Dir(int idDir, string name, Dir parentDir, DateTime creationTime, DateTime lastWriteTime)
        {
            this.idDir = idDir;
            this.name = name;
            if (parentDir == null)
                this.path = name;
            else
                this.path = parentDir.path + name;
            this.creationTime = creationTime;
            this.lastWriteTime = lastWriteTime;
            this.parentDir = parentDir;
            elencoSubdirectory = new List<Dir>();
            elencoFile = new List<File>();
        }
    }
}
