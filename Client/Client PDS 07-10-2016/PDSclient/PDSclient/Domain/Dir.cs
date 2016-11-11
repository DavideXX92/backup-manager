using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient
{
    public class Dir : Node
    {
        public int idDir { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string relativePath { get; set; }
        public DateTime creationTime { get; set; }
        public DateTime lastWriteTime { get; set; }
        public String creationTimeToString { get { return creationTime.ToShortDateString() + " " + creationTime.ToShortTimeString(); } }
        public String lastWriteTimeToString { get { return lastWriteTime.ToShortDateString() + " " + lastWriteTime.ToShortTimeString(); } }
        public Dir parentDir { get; set; }
        public ObservableCollection<Dir> elencoSubdirectory { set; get; }
        public List<File> elencoFile;
        public Boolean IsExpanded { set; get; }
        private bool _isSelected;

        public Dir()
        {
        }

        public Dir(string path, Dir parentDir)
        {
            if (path == "..")
                this.name = path;
            else
            {
                this.path = path;
                this.name = path.Substring(path.LastIndexOf(@"\") + 1);
                elencoSubdirectory = new System.Collections.ObjectModel.ObservableCollection<Dir>();
                elencoFile = new List<File>();
                this.creationTime = Directory.GetCreationTime(path);
                this.lastWriteTime = Directory.GetLastWriteTime(path);
            }
            this.parentDir = parentDir;
        }

        public Dir(string path, Dir parentDir, string monitorDir)
        {
            this.path = path;
            this.relativePath = getPathFromMonitorDir(path, monitorDir);
            this.name = path.Substring(path.LastIndexOf(@"\") + 1);
            this.parentDir = parentDir;
            elencoSubdirectory = new ObservableCollection<Dir>();
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
            elencoSubdirectory = new ObservableCollection<Dir>();
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
            //string monitorDirName = monitorDirPath.Substring(monitorDirPath.LastIndexOf(@"\"));
            string monitorDirName = monitorDirPath.Substring(monitorDirPath.LastIndexOf(@"\")).Substring(1);
            return monitorDirName + path.Substring(monitorDirPath.Length);
        }

        public String folderOpened
        {
            get { return @"images/Icons/Folder_Opened.ico"; }
            set { }
        }

        public String ImgSrc
        {
            get { return @"images/Icons/Folder_Closed.ico"; }
            set { }
        }

        public List<Node> exploreDir()
        {
            List<Node> directory = new List<Node>();

            if (this.parentDir != null)
                directory.Add(new Dir("..", this.parentDir));
            directory.AddRange(elencoSubdirectory);
            directory.AddRange(elencoFile);

            return directory;
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;

                }
            }
        }

        /*
         *Method used to open all the ancestrors of the directory I want to show 
         */
        public void expandTree()
        {
            if (this.parentDir != null)
                this.parentDir.expandTree();

            this.IsExpanded = true;
        }

        public String Lenght
        {
            get { return ""; }
            set { }
        }

    }
}
