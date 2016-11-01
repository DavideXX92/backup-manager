using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSserver
{
    class CheckFile
    {
        public string operation { get; set; }
        public string oldPath { get; set; }
        public string newPath { get; set; }
        public string path { get; set; }
        public File file { get; set; }
        public Dir dir { get; set; }
        public string message { get; set; }

        public CheckFile()
        {
            this.file = null;
            this.dir = null;
        }

        public CheckFile(string operation)
        {
            this.operation = operation;
            this.file = null;
            this.dir = null;
        }
    }
}
