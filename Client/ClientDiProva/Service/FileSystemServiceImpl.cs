using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class FileSystemServiceImpl : FileSystemService
    {
        public bool isAdir(string path)
        {
            string name = path.Substring(path.LastIndexOf(@"\"));
            string[] array = name.Split('.');
            if (array.Length == 1)
                return true;
            else
                return false;
            //return System.IO.File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }
        public bool isAfile(string path)
        {
            string name = path.Substring(path.LastIndexOf(@"\"));
            string[] array = name.Split('.');
            if (array.Length == 2)
                return true;
            else
                return false;
            //return System.IO.File.Exists(path);
        }
        public string getPathFromMonitorDir(string path, string monitorDirPath)
        {
            string monitorDirName = monitorDirPath.Substring(monitorDirPath.LastIndexOf(@"\"));
            return monitorDirName + path.Substring(monitorDirPath.Length);
        }
    }
}
