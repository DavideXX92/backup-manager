using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient
{
    class BusyResourceException : Exception
    {
        public BusyResourceException() 
            : base()
        {
        }

        public BusyResourceException(string message) 
            : base(message)
        {
        }
    }
}
