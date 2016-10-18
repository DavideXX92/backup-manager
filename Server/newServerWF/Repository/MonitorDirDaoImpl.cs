using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class MonitorDirDaoImpl : MonitorDirDao
    {
        public List<string> getMonitorDirsByIdUser(int idUser)
        {
            List<string> monitorDirList = new List<string>();
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = @"SELECT path FROM monitorDir WHERE idUser=@idUser";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
                monitorDirList.Add(dataReader.GetString(dataReader.GetOrdinal("path")));

            dataReader.Close();
            dbConnect.CloseConnection();
            return monitorDirList;

        }
        public void addMonitorDir(string path, int idUser)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "INSERT INTO monitorDir(id, path, idUser) VALUES(NULL, @path, @idUser)";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@path", path);
            cmd.Parameters.AddWithValue("@idUser", idUser);
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
        public void deleteMonitorDir(string path, int idUser)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "DELETE FROM monitorDir WHERE path=@path AND idUser=@idUser";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@path", path);
            cmd.Parameters.AddWithValue("@idUser", idUser);

            cmd.ExecuteNonQuery();
            dbConnect.CloseConnection();
        }
        public void changeMonitorDir(string oldPath, string newPath, int idUser)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "UPDATE monitorDir SET path=@newPath WHERE path=@oldPath AND idUser=@idUser";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@oldPath", oldPath);
            cmd.Parameters.AddWithValue("@newPath", newPath);
            cmd.Parameters.AddWithValue("@idUser", idUser);
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
    }
}
