using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace PrimaryServer
{
    class Postgres
    {
        Log log = new Log();
        public bool IsPostgresPromoted(string postgresPath)
        {
            if (File.Exists(postgresPath + @"\data\recovery.done"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Pgbasebackup(string ip, string port, string usr, string pwd, string path)
        {
            string localPostgresPath = ConfigurationManager.AppSettings.Get("localPostgresPath");
            ProcessStartInfo startinfo = new ProcessStartInfo
            {
                FileName = localPostgresPath + @"\bin\pg_basebackup.exe",
                Arguments = $@" --dbname=postgresql://{usr}:{pwd}@{ip}:{port}/postgres -X stream -R -P -D ""{path}\data""",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startinfo))
            {
                using (StreamReader reader = process.StandardError)
                {
                    log.WriteToFile(reader.ReadToEnd());
                }
                process.WaitForExit();
                process.Close();
            }
        }

        public bool PostgresPrimaryRestore(string ip, string port, string user, string password)
        {
            string remotePostgresPath = ConfigurationManager.AppSettings.Get("remotePostgresPath");
            string datetostring = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.CreateSpecificCulture("pl-PL"));
            string localPostgresPath = ConfigurationManager.AppSettings.Get("localPostgresPath");
            try
            {
                if (Directory.Exists(localPostgresPath + "\\data"))
                {
                    Directory.Move(localPostgresPath + "\\data", localPostgresPath + "\\data_" + datetostring);
                    Pgbasebackup(ip, port, user, password, localPostgresPath);

                    if (File.Exists(localPostgresPath + "\\data\\recovery.done"))
                    {
                        File.Delete(localPostgresPath + "\\data\\recovery.done");
                    }

                    if (File.Exists(localPostgresPath + "\\data\\recovery.conf"))
                    {
                        File.Delete(localPostgresPath + "\\data\\recovery.conf");
                    }

                    if (File.Exists(localPostgresPath + "\\data\\failover\\failover.trigger"))
                    {
                        File.Delete(localPostgresPath + "\\data\\failover\\failover.trigger");
                    }
                    File.Create($@"{remotePostgresPath}\data\PrimaryPromoted.trigger").Dispose();
                }
                log.WriteToFile("Primary Server Promote Succeed");
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
