using System;
using System.Net.NetworkInformation;
using System.Net;

namespace PrimaryServer
{
    public partial class Server : IDisposable
    {
        private bool disposed;
        Log log = new Log();
        public bool CheckServerConnection(string ip)
        {
            Ping pingRequest = new Ping();
            PingReply requestReply = pingRequest.Send(IPAddress.Parse(ip));
            try
            {
                if (requestReply.Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        requestReply = pingRequest.Send(IPAddress.Parse(ip));
                        if (requestReply.Status == IPStatus.Success)
                        {
                            return true;
                        }
                        else
                        {
                            log.WriteToFile(requestReply.Status.ToString());
                        }
                        System.Threading.Thread.Sleep(2000);
                    }
                    return false;
                }

            }
            catch (Exception ex)
            {
                log.WriteToFile(ex.Message);
                return false;
            }
        }

        ~Server()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if(disposing)
                {
                    
                }
            }
            disposed = true;
        }

    }
}
