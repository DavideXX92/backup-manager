﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    interface HandleClient
    {
        void setMonitorDir(string pathDir);
        void registerRequest(string username, string password);
        void loginRequest(string username, string password);
        void logoutRequest();
        Version createNewVersion();
        void updateVersion(MyBuffer bufferOperation);
        void closeVersion(Version version);
        Version restoreVersion(int idVersion);
        void synchronize();

        void test();
    }
}
