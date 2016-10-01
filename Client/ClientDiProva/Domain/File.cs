using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class File
    {
        public string name { get; set; }
        public string path { get; set; }
        public Dir parentDir { get; set; }
        public int size { get; set; }
        public string hash { get; set; }
        public string extension { get; set; }
        public DateTime creationTime { get; set; }
        public DateTime lastWriteTime { get; set; }

        public File()
        {
        }

        public File(string path, string monitorDirPath)
        {
            try
            {
                FileInfo fileinfo = new FileInfo(path);
                this.name = fileinfo.Name;
                this.path = getPathFromMonitorDir(path, monitorDirPath);
                this.parentDir = null;
                this.size = (int)fileinfo.Length;
                this.hash = getHash(path);
                this.extension = Path.GetExtension(path);
                this.creationTime = fileinfo.CreationTime;
                this.lastWriteTime = fileinfo.LastWriteTime;
            }catch(Exception e)
            {
                Console.WriteLine("File ignorato: " + e.Message);
            }
        }

        public File(string filename, Dir parentDir)
        {
            try
            {
                FileInfo fileinfo = new FileInfo(filename);
                this.name = fileinfo.Name;
                this.path = parentDir.path + @"\" + this.name;
                this.parentDir = parentDir;
                this.size = (int)fileinfo.Length;
                this.hash = getHash(filename);
                this.extension = Path.GetExtension(filename);
                this.creationTime = fileinfo.CreationTime;
                this.lastWriteTime = fileinfo.LastWriteTime;
            }
            catch (Exception e)
            {
                Console.WriteLine("File ignorato: " + e.Message);
            }
        }

        public File(string name, Dir parentDir, int size, string hash, string extension, DateTime creationTime, DateTime lastWriteTime)
        {
            this.name = name;
            this.path = parentDir.path + @"\" + name;
            this.parentDir = parentDir;
            this.size = size;
            this.hash = hash;
            this.extension = extension;
            this.creationTime = creationTime;
            this.lastWriteTime = lastWriteTime;
        }

        private string getHash(string filename)
        {
            HashAlgorithm sha1 = HashAlgorithm.Create();
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                return BitConverter.ToString(sha1.ComputeHash(stream));
        }
        private string getPathFromMonitorDir(string path, string monitorDirPath)
        {
            string monitorDirName = monitorDirPath.Substring(monitorDirPath.LastIndexOf(@"\"));
            return monitorDirName + path.Substring(monitorDirPath.Length);
        }
    }
}
