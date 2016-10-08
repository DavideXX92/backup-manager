using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    interface VersionDao
    {
        int addVersion(int idUser, DateTime dateCreation);
        void closeVersion(int idUser, int idVersion);
        void deleteVersion(int idUser, int idVersion);
        void refreshLastUpdateDate(int idUser, int idVersion);
        Version getVersionInfo(int idUser, int idVersion);
        List<int> getAllIdOfVersionsOfaUser(int idUser);
        int getCurrentVersionID(int idUser);
    }
}
