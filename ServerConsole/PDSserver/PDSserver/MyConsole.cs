using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PDSserver
{
    static class MyConsole
    {
        private static string serverLogPath = null;
        private static string clientLogPath = null;
        private static object _lock = new object();

        private static List<string> bufferMsg = new List<string>();
        
        public static void setClientLog(string filePath)
        {
            clientLogPath = filePath;
        }

        public static void setServerLog(string filePath)
        {
            serverLogPath = filePath;
        }

        public static void Write(string str){
            lock (_lock)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " [" + Thread.CurrentThread.ManagedThreadId.ToString("00") +"]  "+ str + "\n");

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
        public static void Append(string logMessage)
        {
            bufferMsg.Add(logMessage);
        }
        public static void LogCommit()
        {
            if (clientLogPath == null)
                Console.WriteLine("il file di log del client non e' stato settato");
            else
            {
                foreach(string logMessage in bufferMsg)
                {
                    using (StreamWriter w = System.IO.File.AppendText(clientLogPath))
                    {
                        w.WriteLine("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString() + ": " + logMessage);
                    }
                }
                bufferMsg.Clear();
            }
        }
        public static void LogRollback()
        {
            bufferMsg.Clear();
        }
    }
}
