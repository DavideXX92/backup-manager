using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class HashRequest : GenericRequest
    {
        public List<string> elencoHash { get; set; }

        public HashRequest()
        {
        }
    }
}
