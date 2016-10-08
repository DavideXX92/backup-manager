using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class GetVersion : GenericRequest
    {
        public Version version { get; set; }

        public GetVersion()
        {
        }

        public GetVersion(Version version)
        {
            this.version = version;
        }
    }
}
