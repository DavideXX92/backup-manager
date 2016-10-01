using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class Register
    {
        public User user { get; set; }
        public bool isRegistred {get; set;}
        public string message {get; set;}
        public string error {get; set;}

        public Register()
        {
        }

        public Register(User user){
            this.user = user;
            isRegistred = false;
        }
    }

}
