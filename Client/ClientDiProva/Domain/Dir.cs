using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class Dir
    {
        public int idDir { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string relativePath { get; set; }
        public DateTime creationTime { get; set; }
        public DateTime lastWriteTime { get; set; }
        public Dir parentDir { get; set; }
        public List<Dir> elencoSubdirectory;
        public List<File> elencoFile;

        public Dir()
        {
        }

        public Dir(string path, Dir parentDir)
        {
            this.path = path;
            this.name = path.Substring(path.LastIndexOf(@"\"));
            this.parentDir = parentDir;
            elencoSubdirectory = new List<Dir>();
            elencoFile = new List<File>();
            this.creationTime = Directory.GetCreationTime(path);
            this.lastWriteTime = Directory.GetLastWriteTime(path);
        }

        public Dir(string path, Dir parentDir, string monitorDir)
        {
            this.path = path;
            this.relativePath = getPathFromMonitorDir(path, monitorDir);
            this.name = path.Substring(path.LastIndexOf(@"\"));
            this.parentDir = parentDir;
            elencoSubdirectory = new List<Dir>();
            elencoFile = new List<File>();
            this.creationTime = Directory.GetCreationTime(path);
            this.lastWriteTime = Directory.GetLastWriteTime(path);
        }

        public Dir(int idDir, string name, Dir parentDir, DateTime creationTime, DateTime lastWriteTime)
        {
            this.idDir = idDir;
            this.name = name;
            if (parentDir == null)
                this.path = name;
            else
                this.path = parentDir.path + name;
            this.creationTime = creationTime;
            this.lastWriteTime = lastWriteTime;
            this.parentDir = parentDir;
            elencoSubdirectory = new List<Dir>();
            elencoFile = new List<File>();
        }

        public Dir(string path)
        {
            this.path = path;
        }

        public void setCreationTime(string fullPath)
        {
            this.creationTime = Directory.GetCreationTime(fullPath);
        }
        public void setLastWriteTime(string fullPath)
        {
            this.lastWriteTime = Directory.GetLastWriteTime(fullPath);
        }

        private bool containsThisFile(File fileToSearch){
            foreach(File file in elencoFile)
                if (fileToSearch == file)
                    return true;
            return false;
        }

        private bool containsThisDir(Dir dirToSearch)
        {
            foreach (Dir dir in elencoSubdirectory)
                if (dirToSearch == dir)
                    return true;
            return false;
        }

        private string getPathFromMonitorDir(string path, string monitorDirPath)
        {
            string monitorDirName = monitorDirPath.Substring(monitorDirPath.LastIndexOf(@"\"));
            return monitorDirName + path.Substring(monitorDirPath.Length);
        }

    }
}
