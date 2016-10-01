using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class CloseVersion : GenericRequest
    {
        public Version version { get; set; }

        public CloseVersion()
        {
        }

        public CloseVersion(Version version)
        {
            this.version = version;
        }
    }
}
