using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class HashDaoImpl : HashDao
    {
        public int getMaxIdHash(int idUser) 
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT MAX(idHash) FROM hash WHERE idUser=@idUser";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            dataReader.Read();
            int idHash;
            try
            {
                idHash = dataReader.GetInt32(0);
            }
            catch (SqlNullValueException ex)
            {
                idHash = -1;
            }
            dbConnect.CloseConnection();
            return idHash;
        }
        public int saveHash(string hash, int idUser)
        {
            int idHash = getMaxIdHash(idUser) + 1;
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "INSERT INTO hash(idUser, idHash, hash, counter, received) VALUES(@idUser, @idHash, @hash, @counter, @received)";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idHash", idHash);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@counter", 1);
            cmd.Parameters.AddWithValue("@received", 0);
            try
            {
                cmd.ExecuteNonQuery();
                dbConnect.CloseConnection();
                return idHash;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                dbConnect.CloseConnection();
                throw;
            }
        }
        public int checkIfHashExists(string hash, int idUser)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT idHash FROM hash WHERE idUser=@idUser AND hash=@hash";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@hash", hash);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            if (dataReader.HasRows)
            {
                dataReader.Read();
                int idHash = dataReader.GetInt32(0);
                dbConnect.CloseConnection();
                return idHash;
            }
            else
            {
                dbConnect.CloseConnection();
                return -1;
            }
        }
        public void changeCounterOfHash(int idHash, int idUser, int op)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "UPDATE hash SET counter=counter+@counter WHERE idUser=@idUser AND idHash=@idHash";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@counter", op);
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idHash", idHash);
            try
            {
                cmd.ExecuteNonQuery();
                dbConnect.CloseConnection();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                dbConnect.CloseConnection();
                throw;
            }
        }
        public void changeHashAsReceived(string hash, int idUser)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "UPDATE hash SET received=1 WHERE idUser=@idUser AND hash=@hash";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@hash", hash);
            try
            {
                cmd.ExecuteNonQuery();
                dbConnect.CloseConnection();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                dbConnect.CloseConnection();
                throw;
            }
        }
        public List<string> getAllHashToBeingReceived(int idUser)
        {
            List<string> elencoHash = new List<string>();
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = @"SELECT hash FROM hash WHERE idUser=@idUser AND received=0";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
                elencoHash.Add(dataReader.GetString(dataReader.GetOrdinal("hash")));

            dataReader.Close();
            dbConnect.CloseConnection();
            return elencoHash;
        }
    }
}
