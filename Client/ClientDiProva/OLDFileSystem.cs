using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class FileSystem
    {
        /*public FileSystem()
        {
        }

        public List<File> getAllFiles(String targetDir)
        {
            List<File> files = null;
            if (Directory.Exists(targetDir))
            {
                files = new List<File>();
                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(targetDir);
                foreach (string fileName in fileEntries)
                    files.Add(getFile(fileName, targetDir));

                // Recurse into subdirectories of this directory.
                string[] subdirectoryEntries = Directory.GetDirectories(targetDir);
                foreach (string subdirectory in subdirectoryEntries)
                {
                    fileEntries = Directory.GetFiles(subdirectory);
                    foreach (string fileName in fileEntries)
                        files.Add(getFile(fileName, targetDir));
                } 
            }
            else
                throw new Exception("Invalid path of dir");
            return files;
        }

        private File getFile(String filename, string rootPath)
        {
            File file = new File();
            FileInfo fileinfo = new FileInfo(filename);
            file.name = fileinfo.Name;
            file.path = getRelativePath(fileinfo.DirectoryName, rootPath);
            file.size = (int)fileinfo.Length;
            file.hash = getHash(filename);
            return file;
        }

        public string getHash(string filename)
        {
            HashAlgorithm sha1 = HashAlgorithm.Create();
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                return BitConverter.ToString(sha1.ComputeHash(stream));
        }

        public string getRelativePath(string fullPath, string monitorDir)
        {
            monitorDir = monitorDir.Substring(0, monitorDir.Length - 1);
            //string parent = Directory.GetParent(monitorDir).FullName;
            //fullPath = fullPath.Substring(parent.Length+1);
            if( fullPath.Equals(monitorDir) )
                return @"\";
            else
            {
                fullPath = fullPath.Substring(monitorDir.Length);
                fullPath += @"\";
                return fullPath;
            }
        }
         * */
    }
}
