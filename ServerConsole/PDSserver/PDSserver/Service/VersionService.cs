using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSserver
{
    interface VersionService
    {
        Version saveVersion(Dir dirTree, string username, string clientDir);
        Version getVersion(string username, int idVersion);
        void updateVersion(string username, int idVersion, UpdateVersion updateVersion);
        void closeVersion(string username, int idVersion);
        void deleteVersion(string username, int idVersion, string clientDir);
        List<Version> getAllVersionsOfaUser(string username);
        int getCurrentVersionID(string username);
        int getLastClosedVersionID(string username);
        List<int> getAllIdOfVersions(string username);
        List<File> getAllFileIntoAlist(Version version);
    }
}
