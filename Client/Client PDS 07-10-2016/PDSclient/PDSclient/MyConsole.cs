using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient
{
    static class MyConsole
    {
        private static MainWindow.writeString dele = null;

        public static void setDel(MainWindow.writeString del)
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
