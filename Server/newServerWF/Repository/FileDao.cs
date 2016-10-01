using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    interface FileDao
    {
        int getMaxIdFile(int idUser, int idVersion);
        int checkIfFileExists(int idUser, int idVersion, string path);
        int saveFile(string name, int size, string hash, string extension, int idDir, int idUser, int idVersion, DateTime creationTime, DateTime lastWriteTime);
        void updateFile(int idUser, int idVersion, int idFile, string hash, int size, DateTime lastWriteTime);
        void deleteFile(int idUser, int idVersion, int idFile);
        void renameFile(int idUser, int idVersion, int idFile, string newName);
        List<File> getAllFilesOfDir(Dir dir, int idUser, int idVersion);
        
       

    }
}
