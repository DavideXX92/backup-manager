using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class UpdateVersion : GenericRequest
    {
        public List<Operation> list {get; set;}
        
        public UpdateVersion()
        {
            //list = new List<Operation>();
        }

        public UpdateVersion(List<Operation> list)
        {
            this.list = list;
        }
    }
}
