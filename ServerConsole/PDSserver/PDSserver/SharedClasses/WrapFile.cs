using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSserver
{
    class WrapFile
    {
        public File file { get; set; }
        public string message { get; set; }
        public string error { get; set; }
        [JsonIgnore]
        public int size { get; set; }
        [JsonIgnore]
        public FileStream fs { get; set; }

        public WrapFile()
        {
        }

        public WrapFile(File file, int size, FileStream fs)
        {
            this.file = file;
            this.size = size;
            this.fs = fs;
        }
    }
}
