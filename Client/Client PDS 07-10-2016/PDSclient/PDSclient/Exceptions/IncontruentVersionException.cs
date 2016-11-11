using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient{    
    class IncontruentVersionException:Exception {
        public IncontruentVersionException() 
            : base()
        {
        }

        public IncontruentVersionException(string message) 
            : base(message)
        {
        }
    }
}
