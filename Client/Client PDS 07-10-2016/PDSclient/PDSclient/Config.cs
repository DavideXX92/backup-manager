using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient
{
    public static class Config
    {
        public static string ServerIP
        {
            get { return ConfigurationManager.AppSettings["server_IP"]; }
        }
        public static int ServerPort
        {
            get { return int.Parse(ConfigurationManager.AppSettings["server_port"]); }
        }
        public static int KeepalivePort
        {
            get { return int.Parse(ConfigurationManager.AppSettings["keepalive_port"]); }
        }
       
    }
}
