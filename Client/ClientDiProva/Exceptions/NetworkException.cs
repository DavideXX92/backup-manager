﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class NetworkException : Exception
    {
        public NetworkException() 
            : base()
        {
        }

        public NetworkException(string message) 
            : base(message)
        {
        }
    }
}