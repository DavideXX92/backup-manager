using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class MyBuffer
    {
        public List<CheckFile> list { get; set; }

        public MyBuffer()
        {
            list = new List<CheckFile>();
        }

        public File containsThisFile(string path)
        {
            foreach (CheckFile checkFile in list)
            {
                if(checkFile.file!=null)
                {
                    if (checkFile.file.path.Equals(path))
                        return checkFile.file;
                }
            }
            return null;
        }
        public bool removeThisFile(File file)
        {
            foreach (CheckFile checkFile in list)
            {
                if(checkFile.file!=null)
                {
                    if (checkFile.file.path.Equals(file.path))
                    {
                        list.Remove(checkFile);
                        return true;
                    } 
                }  
            }
            return false;
        }
        public CheckFile containsThisDir(string path)
        {
            foreach (CheckFile checkFile in list)
            {
                if(checkFile.dir!=null)
                {
                    if (checkFile.dir.path.Equals(path))
                        return checkFile;
                }   
            }
            return null;
        }
        public bool removeThisDir(string path)
        {
            foreach (CheckFile checkFile in list)
            {
                if(checkFile.dir!=null)
                {
                    if (checkFile.dir.path.Equals(path))
                    {
                        list.Remove(checkFile);
                        return true;
                    }
                }
                
            }
            return false;
        }
    }
}
