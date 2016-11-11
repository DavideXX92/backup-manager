using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient
{
    class CreateVersion : GenericRequest
    {
        public Version version { get; set; }

        public CreateVersion()
        {
        }

        public CreateVersion(Version version)
        {
            this.version = version;
        }
    }

}
