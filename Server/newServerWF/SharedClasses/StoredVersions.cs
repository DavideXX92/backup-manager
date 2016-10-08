using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class StoredVersions : GenericRequest
    {
        public List<int> elencoID { get; set; }

        public StoredVersions()
        {
        }

    }
}
