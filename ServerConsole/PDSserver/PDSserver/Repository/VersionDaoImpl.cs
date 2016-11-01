using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSserver
{
    class VersionDaoImpl : VersionDao
    {
        private int getMaxIdVersion(int idUser)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT MAX(idVersion) FROM version WHERE idUser=@idUser";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            int idVersion = 0;
            dataReader.Read();
            if (!dataReader.IsDBNull(0))
                idVersion = dataReader.GetInt32(0);

            dbConnect.CloseConnection();
            return idVersion;
        }
        public int addVersion(int idUser, DateTime dateCreation)
        {
            int idVersion = getMaxIdVersion(idUser) + 1;
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "INSERT INTO version(idUser, idVersion, dateCreation, dateClosed, lastUpdate) VALUES(@idUser, @idVersion, @dateCreation, NULL, NULL)";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            cmd.Parameters.AddWithValue("@dateCreation", dateCreation);
            try
            {
                cmd.ExecuteNonQuery();
                dbConnect.CloseConnection();
                return idVersion;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                dbConnect.CloseConnection();
                throw;
            }
        }
        public void closeVersion(int idUser, int idVersion)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "UPDATE version SET dateClosed=@dateClosed WHERE idVersion=@idVersion AND idUser=@idUser";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            cmd.Parameters.AddWithValue("@dateClosed", DateTime.Now);
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
        public void deleteVersion(int idUser, int idVersion)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "DELETE FROM version WHERE idVersion=@idVersion AND idUser=@idUser";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();          
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            cmd.Parameters.AddWithValue("@idUser", idUser);

            cmd.ExecuteNonQuery();
            dbConnect.CloseConnection();
        }
        public void refreshLastUpdateDate(int idUser, int idVersion)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "UPDATE version SET lastUpdate=@lastUpdate WHERE idUser=@idUser AND idVersion=@idVersion";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@lastUpdate", DateTime.Now);
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);

            cmd.ExecuteNonQuery();
            dbConnect.CloseConnection();
        }
        public Version getVersionInfo(int idUser, int idVersion)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT dateCreation, dateClosed FROM version WHERE idUser=@idUser AND idVersion=@idVersion";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            Version version = null;
            if (dataReader.HasRows)
            {
                dataReader.Read();
                if (dataReader.IsDBNull(1)) //if dataClosed is null
                    version = new Version(idVersion, dataReader.GetDateTime("dateCreation"));
                else
                    version = new Version(idVersion, dataReader.GetDateTime("dateCreation"), dataReader.GetDateTime("dateClosed"));
            }
            dbConnect.CloseConnection();
            return version;
        }
        public List<int> getAllIdOfVersionsOfaUser(int idUser)
        {
            List<int> idVersionList = new List<int>();
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = @"SELECT idVersion FROM version WHERE idUser=@idUser ORDER BY idVersion";                   
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
                idVersionList.Add(dataReader.GetInt32(dataReader.GetOrdinal("idVersion")));

            dataReader.Close();
            dbConnect.CloseConnection();
            return idVersionList;
        }
        public int getCurrentVersionID(int idUser)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT idVersion FROM version WHERE idUser=@idUser AND dateClosed IS NULL";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            int idVersion = -1;
            if (dataReader.HasRows)
            {
                dataReader.Read();
                idVersion = dataReader.GetInt32(0);
            }
            dbConnect.CloseConnection();
            return idVersion;
        }
        public int getLastClosedVersionID(int idUser)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT MAX(idVersion) FROM version WHERE idUser=@idUser AND dateClosed IS NOT NULL";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            int idVersion = -1;
            if (dataReader.HasRows)
            {
                dataReader.Read();
                idVersion = dataReader.GetInt32(0);
            }
            dbConnect.CloseConnection();
            return idVersion;
        }
    }
}
