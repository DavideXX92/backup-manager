using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class CloseVersion : GenericRequest
    {
        public int idVersion { get; set; }

        public CloseVersion()
        {
        }

        public CloseVersion(int idVersion)
        {
            this.idVersion = idVersion;
        }
    }
}
