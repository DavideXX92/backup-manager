using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient
{
    interface FileSystemService
    {
        bool isAdir(string path);
        bool isAfile(string path);
        string getPathFromMonitorDir(string path, string monitorDirPath);
    }
}
