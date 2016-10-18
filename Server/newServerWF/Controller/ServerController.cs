using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    interface ServerController
    {
        //Funzioni che piò eseguire il server una volta stabilita la connessione con il client
        
        ServerController startLoop();
        void stop();

        GenericRequest helloMessage(GenericRequest request);
        GenericRequest closeConnectionWithTheClient(GenericRequest request);
        WrapFile handleRequestOfFile(File file);
        WrapFile initializeReceiptOfFile(File file);
        WrapFile fileReceived(WrapFile wrapFile);
        Register handleRegistration(Register request);
        Login handleLogin(Login loginRequest);
        MonitorDir addMonitorDir(MonitorDir request);
        MonitorDir changeMonitorDir(MonitorDir request);
        MonitorDir getMonitorDir(MonitorDir request);
        CreateVersion createNewVersion(CreateVersion request);
        UpdateVersion updateVersion(UpdateVersion request);
        CloseVersion closeVersion(CloseVersion request);
        StoredVersions getIDofAllVersions(StoredVersions request);
        GetVersion getVersion(GetVersion request);
        GetVersion getOpenVersion(GetVersion request);
        HashRequest sendHashToBeingReceived(HashRequest request);

        CheckFile checkFile(CheckFile request);
    }
}
