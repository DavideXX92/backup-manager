using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class RestoreVersion : GenericRequest
    {
        public Version version { get; set; }
        public List<File> elencoFile { get; set; }

        public RestoreVersion()
        {
        }

        public RestoreVersion(Version version)
        {
            this.version = version;
            this.elencoFile = new List<File>();
        }
    }
}
