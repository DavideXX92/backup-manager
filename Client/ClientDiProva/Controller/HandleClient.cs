using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    interface HandleClient
    {
        void disconnect();
        void setMonitorDir(string pathDir);
        void registerRequest(string username, string password);
        void loginRequest(string username, string password);
        List<Version> askStoredVersions();
        Version createNewVersion();
        void closeVersion(Version version);
        Version restoreVersion(int idVersion);
        void synchronize();

        void test();
    }
}
