using System.Configuration;
using System.ServiceProcess;

namespace PrimaryServer
{
    class Program
    {
        public const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        static void Main(string[] args)
        {
            
            string ip = ConfigurationManager.AppSettings.Get("remoteServerIP");
            string user = ConfigurationManager.AppSettings.Get("remoteServerUsername");
            string pwd = ConfigurationManager.AppSettings.Get("remoteServerPassword");
            string remotePostgresPath = ConfigurationManager.AppSettings.Get("remotePostgresPath");
            string saltoServiceName = "ProAccessSpaceService";
            string postgresServiceName = "postgresql-x64-10";
            ServiceController postgresService = new ServiceController(postgresServiceName, ip);
            ServiceController saltoService = new ServiceController(saltoServiceName, ip);

            Service service = new Service();
            Postgres postgres = new Postgres();
            Server server = new Server();
            Promote promote = new Promote();
            Log log = new Log();
            
            if (!server.CheckServerConnection(ip))
            {
                log.WriteToFile("Cannot Connect to Secondary Server", "error");
                log.Dispose();
                return;
            }

            using (var impersonation = new ImpersonateUser(user, ip, pwd, LOGON32_LOGON_NEW_CREDENTIALS))
            {
                if (postgres.IsPostgresPromoted(remotePostgresPath) && service.IsServiceRunning(postgresService))
                {
                    if (service.IsServiceRunning(saltoService))
                    {
                        promote.Primary();
                    } else
                    {
                        log.WriteToFile("Salto Service is Not Running on Secondary Server, there is nothing to update");
                    }
                    return;
                } else
                {
                    log.WriteToFile("Postgres is Not Promoted on Seconadry Server");
                    log.Dispose();
                    return;
                }
            }
        }
    }
}
