using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSserver
{
    class Register : GenericRequest
    {
        public User user { get; set; }
        public bool isRegistred { get; set; }

        public Register()
        {
        }

        public Register(User user)
        {
            this.user = user;
            isRegistred = false;
        }
    }
}
