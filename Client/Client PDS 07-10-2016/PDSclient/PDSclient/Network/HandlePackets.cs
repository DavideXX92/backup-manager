using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient
{
    interface HandlePackets
    {
        Object doRequest(string code, Object objRequest);
    }
}
