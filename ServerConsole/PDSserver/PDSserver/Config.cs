using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSserver
{
    public static class Config
    {
        public static int ServerPort {
        get { return int.Parse(ConfigurationManager.AppSettings["server_port"]); }
        }
        public static int KeepalivePort {
            get { return int.Parse(ConfigurationManager.AppSettings["keepalive_port"]); }
        }
        public static string serverDirRoot
        {
            get { return ConfigurationManager.AppSettings["serverDirRoot_path"]; }
        }
        public static string serverLogPath{
            get { return ConfigurationManager.AppSettings["serverLog_path"]; }
        }
        public static int nOfMaxVersionToSaveForUser{
            get { return int.Parse(ConfigurationManager.AppSettings["nOfMaxVersionToSaveForUser"]); }
        }
        public static string ServerCertificate {
            get { return ConfigurationManager.AppSettings["server_certificate"]; }
        }
        public static string ServerName {
            get { return ConfigurationManager.AppSettings["server_name"]; }
        }
        public static string DBAddress {
            get { return ConfigurationManager.AppSettings["db_address"]; }
        }
        public static string DBUser {
            get { return ConfigurationManager.AppSettings["db_user"]; }
        }
        public static string DBPassword {
            get { return ConfigurationManager.AppSettings["db_password"]; }
        }
        public static string DBName {
            get { return ConfigurationManager.AppSettings["db_name"]; }
        }
    }
}
