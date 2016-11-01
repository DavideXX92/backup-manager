using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSserver
{
    class Operation
    {
        public string type { get; set; }
        public string oldPath { get; set; }
        public string newPath { get; set; }
        public string path { get; set; }
        public File file { get; set; }
        public Dir dir { get; set; }

        public Operation()
        {
        }

        public Operation(string operation)
        {
            this.type = operation;
            this.oldPath = null;
            this.newPath = null;
            this.path = null;
            this.file = null;
            this.dir = null;
        }

    }
}
