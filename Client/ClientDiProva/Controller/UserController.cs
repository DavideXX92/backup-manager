using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    interface UserController
    {
        void setMonitorDir(string path);
        string getMonitorDir();
        void register(string username, string password);
        User login(string username, string password);
        void logout();
        Version createNewVersion(string monitorDir);
        void restoreVersion(int idVersion, string pathDst);
        List<Version> askStoredVersions();
        void synchronize(string monitorDir);
        void enableAutoSync();
        void disableAutoSync();
        void manualSync();
        bool checkIfCurrentVersionIsUpdated(string monitorDir);
    }
}
