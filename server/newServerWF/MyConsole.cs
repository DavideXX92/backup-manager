using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace newServerWF
{
    static class MyConsole
    {
        private static Form1.WriteOnConsoleDel writeOnConsole = null;
        private static string serverLogPath = null;
        private static string clientLogPath = null;

        public static void setConsole(Form1.WriteOnConsoleDel del)
        {
            writeOnConsole = del;
        }
        
        public static void setClientLog(string filePath)
        {
            clientLogPath = filePath;
        }

        public static void setServerLog(string filePath)
        {
            serverLogPath = filePath;
        }

        public static void Write(string str){
            
            if (writeOnConsole == null)
                Console.WriteLine(str);
            else
                writeOnConsole(str);

            if (serverLogPath == null)
                Console.WriteLine("il file di log del server non e' stato settato");
            else
            {
                using (StreamWriter w = System.IO.File.AppendText(serverLogPath))
                {
                    w.WriteLine("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString() + ": " + str);
                }
            }
                
        }

        public static void Log(string logMessage)
        {
            if (clientLogPath == null)
                Console.WriteLine("il file di log del client non e' stato settato");
            else
            {
                using (StreamWriter w = System.IO.File.AppendText(clientLogPath))
                {
                    w.WriteLine("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString() + ": " + logMessage);
                }
            }
        }
    }
}
