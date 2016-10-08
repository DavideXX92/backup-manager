using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    interface DirDao
    {
        int checkIfPathExists(int idUser, int idVersion, string path);
        int createDirFromPath(int idUser, int idVersion, string path);
        int saveDirectory(string name, int idParent, int idUser, int idVersion, DateTime creationTime, DateTime lastWriteTime);
        void updateDirectory(int idUser, int idVersion, int idDir, DateTime lastWriteTime);
        void renameDirectory(int idUser, int idVersion, int idDir, string newName);
        void deleteDirectory(int idUser, int idVersion, int idDir);
        Dir getRootDir(int idUser, int idVersion);
        List<Dir> getAllSubDirOfDir(Dir dir, int idUser, int idVersion);
    }
}
