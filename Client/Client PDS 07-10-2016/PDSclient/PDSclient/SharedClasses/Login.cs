using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient
{
    class Login : GenericRequest
    {
        public User user {get; set;}
        public bool isLogged {get; set;}

        public Login()
        {
        }

        public Login(User user){
            this.user = user;
            isLogged = false;
        }
    }
}
