using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace PrimaryServer
{
    public partial class SqlExpress
    {
        Log log = new Log();
        string backupFileName;
        public bool RestoreDataBase(string IP, string PORT, string DB, string USER, string PASSWORD, string RESTORE_FILE = null)
        {
            string datetostring = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.CreateSpecificCulture("pl-PL"));
            string connectSQL = $"Data Source={IP},{PORT};Initial Catalog=master;User ID={USER};Password={PASSWORD}";
            using (SqlConnection connection = new SqlConnection(connectSQL))
            {
                try { 
                    connection.Open();
                    //restore SQL
                        //disconnect users
                    string sqlStmt1 = string.Format($"ALTER DATABASE [{DB}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
                    SqlCommand sqlRes1 = new SqlCommand(sqlStmt1, connection);
                    sqlRes1.ExecuteNonQuery();
                    //rename DB
                    //string sqlStmt2 = string.Format($"ALTER DATABASE [{DB}] MODIFY NAME = [{DB}_{datetostring}]");
                    //SqlCommand sqlRes2 = new SqlCommand(sqlStmt2, connection);
                    //sqlRes2.ExecuteNonQuery();
                    //restore from file
                    string sqlStmt3 = $@"USE MASTER RESTORE DATABASE [{DB}] FROM DISK = '{backupFileName}' WITH REPLACE;";
                    SqlCommand sqlRes3 = new SqlCommand(sqlStmt3, connection);
                    sqlRes3.ExecuteNonQuery();

                    string sqlStmt4 = string.Format($"ALTER DATABASE [{DB}] SET MULTI_USER");
                    SqlCommand sqlRes4 = new SqlCommand(sqlStmt4, connection);
                    sqlRes4.ExecuteNonQuery();
                    connection.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    log.WriteToFile(ex.Message);
                    return false;
                }
            }

        }

        public bool BackupDataBase(string IP, string PORT, string DB, string USER, string PASSWORD, string BACKUP_PATH)
        {
            string datetostring = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.CreateSpecificCulture("pl-PL"));
            string connectSQL = $"Data Source={IP},{PORT};Initial Catalog=master;User ID={USER};Password={PASSWORD}";
            backupFileName = $"{BACKUP_PATH}{DB}_{datetostring}.bak";
            using (SqlConnection connection = new SqlConnection(connectSQL))
            {
                try
                {
                    connection.Open();
                    SqlDataReader reader;
                    //backup SQL
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = $@"BACKUP DATABASE [{DB}] TO DISK = '{backupFileName}' WITH COPY_ONLY, INIT";
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;
                    reader = cmd.ExecuteReader();
                    connection.Close();
                    return true;
                } 
                catch (Exception ex)
                {
                    log.WriteToFile(ex.Message);
                    return false;
                }
            }
        }
    }
}
