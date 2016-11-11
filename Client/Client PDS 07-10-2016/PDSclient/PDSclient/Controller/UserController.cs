using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PDSclient
{
    enum functionAsynchronous { Synchronize, CheckVersion, UploadFile, CreateNewVersion, RestoreDir, ManualSynch, RestoreVersion, RestoreLastVersion, AutoSynch, Hello }

    public interface UserController
    {
        void sendHelloMessage();
        void addMonitorDir(string path);
        List<string> getMonitorDir();
        void deleteUserRepository();
        void register(string username, string password);
        User login(string username, string password);
        void logout();
        Version createNewVersion(string monitorDir);
        void restoreLastVersion(string monitorDir);
        void restoreVersion(int idVersion, string monitorDir);
        string restoreDir(string pathDst);
        List<Version> askStoredVersions();
        void synchronize(string monitorDir);
        void watcherInit(string monitorDir);
        void watcherDelete();
        void timerInit(RunWorkerCompletedEventHandler onWorkerComplete, ProgressChangedEventHandler autoSynch_ProgressChanged);
        void enableAutoSync();
        void disableAutoSync();
        int manualSync();
        bool checkIfCurrentVersionIsUpdated(string monitorDir);
        bool checkIfthereAreFileToSend();
        void uploadFile(string monitorDir);

        void runThread(object sender, DoWorkEventArgs e);
    }
}
