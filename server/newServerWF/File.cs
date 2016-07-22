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
        public int size { get; set; }
        public string hash { get; set; }

        public File()
        {
        }

        public File(string name, string path, int size, string hash)
        {
            this.name = name;
            this.path = path;
            this.size = size;
            this.hash = hash;
        }
    }
}
