using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    interface VersionDao
    {
        int getMaxIdVersion(int idUser);
        int addVersion(int idUser);
        void closeVersion(int idUser, int idVersion);
        Version getVersionInfo(int idUser, int idVersion);
        List<int> getAllIdOfVersionsOfaUser(int idUser);  
    }
}
