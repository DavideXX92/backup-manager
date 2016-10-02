using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class MyBuffer
    {
        public List<Operation> list { get; set; }

        public MyBuffer()
        {
            list = new List<Operation>();
        }

        public File containsThisFile(string path)
        {
            foreach (Operation operation in list)
            {
                if (operation.file != null)
                {
                    if (operation.file.path.Equals(path))
                        return operation.file;
                }
            }
            return null;
        }
        public bool removeThisFile(File file)
        {
            foreach (Operation operation in list)
            {
                if (operation.file != null)
                {
                    if (operation.file.path.Equals(file.path))
                    {
                        list.Remove(operation);
                        return true;
                    } 
                }  
            }
            return false;
        }
        public Operation containsThisDir(string path)
        {
            foreach (Operation operation in list)
            {
                if (operation.dir != null)
                {
                    if (operation.dir.path.Equals(path))
                        return operation;
                }   
            }
            return null;
        }
        public bool removeThisDir(string path)
        {
            foreach (Operation operation in list)
            {
                if (operation.dir != null)
                {
                    if (operation.dir.path.Equals(path))
                    {
                        list.Remove(operation);
                        return true;
                    }
                }
                
            }
            return false;
        }
        public Dictionary<string, File> getAllFileIntoAmap()
        {
            Dictionary<string, File> hashMap = new Dictionary<string, File>();
            foreach (Operation operation in list)
            {
                if (operation.file != null)
                    hashMap[operation.file.hash] = operation.file;
            }
            return hashMap;
        }
    }
}
