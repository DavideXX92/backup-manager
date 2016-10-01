using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    static class MyConsole
    {
        private static Form1.delegato dele = null;

        public static void setDel(Form1.delegato del)
        {
            dele = del;
        }

        public static void write(string str)
        {
            if (dele == null)
                Console.WriteLine(str);
            else
                dele(str);
        }
    }
}
