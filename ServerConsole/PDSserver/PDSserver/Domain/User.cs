﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSserver
{
    class User
    {
        public int idUser { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public List<string> monitorDir { get; set; }
        public bool isLogged { get; set; }

        public User()
        {
        }

        public User(int idUser, string username, string password, bool isLogged)
        {
            this.idUser = idUser;
            this.username = username;
            this.password = password;
            this.isLogged = isLogged;
        }

        public User(string username, string password)
        {
            this.idUser = -1;
            this.username = username;
            this.password = password;
            this.isLogged = false;
        }

        public void changeMonitorDir(string oldPath, string newPath)
        {
            if (monitorDir.Contains(oldPath))
            {
                int index = monitorDir.IndexOf(oldPath);
                monitorDir[index] = newPath;
            }
        }
    }

}