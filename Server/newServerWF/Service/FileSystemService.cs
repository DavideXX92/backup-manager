using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    interface FileSystemService
    {
        /*FILE OPERATIONS*/
        bool checkIfFileExists(string username, int version, string path);
        void addFile(string username, int version, File file);
        void renameFile(string username, int version, string oldPath, string newPath);
        void updateFile(string username, int version, File file);
        void deleteFile(string username, int version, File file);
        void setFileAsReceived(string username, string hash);
        List<string> getAllHashToBeingReceived(string username);
        
        /*DIR OPERATIONS*/
        bool checkIfDirExists(string username, int version, string path);
        void addDir(string username, int version, Dir dir);
        void renameDir(string username, int version, string oldPath, string newPath);
        void updateDir(string username, int version, Dir dir);
        void deleteDir(string username, int version, string path);

    }
}
