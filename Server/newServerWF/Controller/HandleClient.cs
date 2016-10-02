using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    interface HandleClient
    {
        //Funzioni che piò eseguire il server una volta stabilita la connessione con il client
        
        HandleClient startLoop();
        void stop();

        GenericRequest closeConnectionWithTheClient(GenericRequest request);
        WrapFile handleRequestOfFile(File file);
        WrapFile initializeReceiptOfFile(File file);
        WrapFile fileReceived(WrapFile wrapFile);
        Register handleRegistration(Register request);
        Login handleLogin(Login loginRequest);
        CreateVersion createNewVersion(CreateVersion request);
        UpdateVersion updateVersion(UpdateVersion request);
        CloseVersion closeVersion(CloseVersion request);
        RestoreVersion restoreVersion(RestoreVersion request);
        StoredVersions sendStoredVersions(StoredVersions request);
        
        CheckFile checkFile(CheckFile request);
    }
}
