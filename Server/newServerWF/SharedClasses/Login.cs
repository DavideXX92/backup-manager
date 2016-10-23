﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class Login : GenericRequest
    {
        public User user { get; set; }

        public Login()
        {
        }

        public Login(User user)
        {
            this.user = user;
        }
    }
}
