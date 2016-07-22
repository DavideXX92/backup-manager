using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    [Serializable]
    class User
    {
        public int idUser { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public bool isLogged { get; set; }

        public User()
        {

        }

        public User(int idUser, string username, string password)
        {
            this.idUser = idUser;
            this.username = username;
            this.password = password;
            this.isLogged = false;
        }

        public User(string username, string password)
        {
            this.idUser = -1;
            this.username = username;
            this.password = password;
            this.isLogged = false;
        }

    }

}
