using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSserver
{
    class GenericRequest
    {
        public string message { get; set; }
        public string error { get; set; }

        public GenericRequest()
        {
            this.message = null;
            this.error = null;
        }
    }
}
