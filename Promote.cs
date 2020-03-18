using System;
using System.Configuration;
using System.IO;
using System.ServiceProcess;

namespace PrimaryServer
{
    class Promote
    {
        public void Primary()
        {
            string secondaryIp = ConfigurationManager.AppSettings.Get("remoteServerIP");
            string primaryIp = ConfigurationManager.AppSettings.Get("primaryServerIP");

            string postgresPort = ConfigurationManager.AppSettings.Get("postgresServerPort");
            string postgresUser = ConfigurationManager.AppSettings.Get("postgresUsername");
            string postgresPwd = ConfigurationManager.AppSettings.Get("postgresPassword");

            string sqlExpressPort = ConfigurationManager.AppSettings.Get("sqlExpressPort");
            string sqlExpressDB = ConfigurationManager.AppSettings.Get("sqlExpressDatabase");
            string sqlExpressUSR = ConfigurationManager.AppSettings.Get("sqlExpressUsername");
            string sqlExpressPWD = ConfigurationManager.AppSettings.Get("sqlExpressPassword");

            string postgresServiceName = "postgresql-x64-10";
            string saltoServiceName = "ProAccessSpaceService";

            string remotePostgresPath = ConfigurationManager.AppSettings.Get("remotePostgresPath");

            string saltoBackupDir = ConfigurationManager.AppSettings.Get("saltoBackupsDirectory");

            ServiceController saltoService = new ServiceController(saltoServiceName, secondaryIp);
            ServiceController primarySaltoService = new ServiceController(saltoServiceName, primaryIp);
            ServiceController primaryPostgresService = new ServiceController(postgresServiceName, primaryIp);

            SqlExpress sql = new SqlExpress();
            Service service = new Service();
            Log log = new Log();
            Postgres postgres = new Postgres();

            try
            {
                log.WriteToFile("Starting promoting primary server");
                service.StopService(saltoService);
                if (sql.BackupDataBase(secondaryIp, sqlExpressPort, sqlExpressDB, sqlExpressUSR, sqlExpressPWD, saltoBackupDir))
                {
                    sql.RestoreDataBase(primaryIp, sqlExpressPort, sqlExpressDB, sqlExpressUSR, sqlExpressPWD);
                    service.StartService(primarySaltoService);
                    service.StopService(primaryPostgresService);

                    if (postgres.PostgresPrimaryRestore(secondaryIp, postgresPort, postgresUser, postgresPwd))
                    {
                        service.StartService(primaryPostgresService);
                        File.Create($@"{remotePostgresPath}\data\PrimaryPromoted.trigger").Dispose();
                    }

                } else
                {
                    log.WriteToFile("Cannot Connect to MSQL Server", "error");
                }
                return;
            } catch (Exception ex)
            {
                log.WriteToFile(ex.Message);
                log.Dispose();
                return;
            }
        }
    }
}
