using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient
{
    interface NetworkService
    {
        void sendHelloMessage();
        List<string> getMonitorDir();
        void addMonitorDir(string path);
        void registerRequest(string username, string password);
        User loginRequest(string username, string password);
        void logoutRequest();
        Version createNewVersion(Dir dirTree);
        void updateVersion(MyBuffer bufferOperation);
        void closeVersion(int idVersion);
        Version getVersion(int idVersion);
        Version getOpenVersion();
        Version getLastVersion();
        void deleteUserRepository();
        List<int> askIDofAllVersions();
        List<string> askHashToSend();

        void requestAfile(File file, string pathDst);
        void sendAfile(File file);
    }
}
