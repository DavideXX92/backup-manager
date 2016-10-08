using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class UserDaoImpl : UserDao
    {
        public bool exists(string username)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT idUser FROM user WHERE username=@username";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@username", username);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            bool exists = false;
            if (dataReader.HasRows)
            {
                dataReader.Read();
                exists = true;
            }
            dbConnect.CloseConnection();
            return exists;
        }
        public int getIdByUsername(string username)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT idUser FROM user WHERE username=@username";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@username", username);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            int idUser = -1;
            if (dataReader.HasRows)
            {
                dataReader.Read();
                idUser = dataReader.GetInt32(0);
                
            }
            dbConnect.CloseConnection();
            return idUser;
        }
        public User findOne(string username)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT idUser, username, password, monitorDir FROM user WHERE username=@username";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@username", username);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            dataReader.Read();
            User user = null;
            string monitorDir = null;
            if (dataReader.HasRows)
            {
                if (!dataReader.IsDBNull(3)) //if monitorDir is not null
                    monitorDir = dataReader.GetString(dataReader.GetOrdinal("monitorDir"));

                user = new User(dataReader.GetInt32(dataReader.GetOrdinal("idUser")),
                                dataReader.GetString(dataReader.GetOrdinal("username")),
                                dataReader.GetString(dataReader.GetOrdinal("password")),
                                monitorDir
                                );
            }
            dbConnect.CloseConnection();
            return user;
           
        }
        public User save(User user)
        {
            int idUser = getMaxIdUser() + 1;
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "INSERT INTO user(idUser, username, password, monitorDir) VALUES(@idUser, @username, @password, @monitorDir)";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@username", user.username);
            cmd.Parameters.AddWithValue("@password", user.password);
            cmd.Parameters.AddWithValue("@monitorDir", user.monitorDir);
            try
            {
                cmd.ExecuteNonQuery();
                dbConnect.CloseConnection();
                user.idUser = idUser;
                return user;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                dbConnect.CloseConnection();
                throw;
            }
        }
        public void update(User user)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "UPDATE user SET username=@username, password=@password, monitorDir=@monitorDir WHERE idUser=@idUser";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@username", user.username);
            cmd.Parameters.AddWithValue("@password", user.password);
            cmd.Parameters.AddWithValue("@monitorDir", user.monitorDir);
            cmd.Parameters.AddWithValue("@idUser", user.idUser);

            cmd.ExecuteNonQuery();
            dbConnect.CloseConnection();
        }
        public bool checkIfCredentialsAreCorrected(string username, string password)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT idUser FROM user WHERE username=@user AND password=@pass";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@user", username);
            cmd.Parameters.AddWithValue("@pass", password);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            bool result = false;
            if (dataReader.HasRows)
            {
                dataReader.Read();
                result = true;
            }
            dbConnect.CloseConnection();
            return result;
        }

        private int getMaxIdUser()
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT MAX(idUser) FROM user";
            cmd.Prepare();
            MySqlDataReader dataReader = cmd.ExecuteReader();

            int idUser = 0;
            dataReader.Read();
            if (!dataReader.IsDBNull(0))
                idUser = dataReader.GetInt32(0);
            
            dbConnect.CloseConnection();
            return idUser;
        }
    }
}
