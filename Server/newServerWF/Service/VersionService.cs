using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    interface VersionService
    {
        Version saveVersion(Dir dirTree, string username);
        Version getVersion(string username, int version);
        void closeVersion(string username, int idVersion);
        List<Version> getAllVersionsOfaUser(string username);
        List<File> getAllFileIntoAlist(Version version);
    }
}
