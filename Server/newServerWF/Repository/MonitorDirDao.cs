﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    interface MonitorDirDao
    {
        List<string> getMonitorDirsByIdUser(int idUser);
        void addMonitorDir(string path, int idUser);
        void deleteMonitorDir(string path, int idUser);
        void changeMonitorDir(string oldPath, string newPath, int idUser);
    }
}