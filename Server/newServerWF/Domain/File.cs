using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class File
    {
        public string name { get; set; }
        public string path { get; set; }
        public Dir parentDir { get; set; }
        public int size { get; set; }
        public string hash { get; set; }
        public string extension { get; set; }
        public DateTime creationTime { get; set; }
        public DateTime lastWriteTime { get; set; }

        public File()
        {
        }

        public File(string name, Dir parentDir, int size, string hash, string extension, DateTime creationTime, DateTime lastWriteTime)
        {
            this.name = name;
            this.path = parentDir.path + @"\" + name;
            this.parentDir = parentDir;
            this.size = size;
            this.hash = hash;
            this.extension = extension;
            this.creationTime = creationTime;
            this.lastWriteTime = lastWriteTime;
        }
        
    }
}
