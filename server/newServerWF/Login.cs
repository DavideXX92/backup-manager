using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class Login
    {
        public User user { get; set; }
        public bool isLogged { get; set; }
        public string message { get; set; }
        public string error { get; set; }

        public Login()
        {
        }

        public Login(User user)
        {
            this.user = user;
            isLogged = false;
        }
    }
}
