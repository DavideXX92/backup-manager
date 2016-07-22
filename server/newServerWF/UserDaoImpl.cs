using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class UserDaoImpl : UserDao
    {
        private List<User> users;

        public List<User> getAllUsers()
        {
            return null;
        }

        public bool findUser(User user)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT idUser FROM user WHERE username=@user AND password=@pass";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@user", user.username);
            cmd.Parameters.AddWithValue("@pass", user.password);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            if (dataReader.HasRows)
            {
                dataReader.Read();
                user.idUser = dataReader.GetInt32(0);
                //user.isLogged = true;
                dbConnect.CloseConnection();
                return true;
            }
            else{
                dbConnect.CloseConnection();
                return false;
            }
            /*while (rdr.Read())
            {
                Console.WriteLine(rdr.GetInt32(0) + ": " + rdr.GetString(1) + ": " + rdr.GetString(2));
            }*/
        }

        public void addUser(User user)
        {
        }
        
        public void updateUser(User user)
        {
        }

        public void deleteUser(User user)
        {
        }

    }
}
