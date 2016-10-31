using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    interface HandlePackets
    {
        void startListen();
        void stopListen();
    }
}
