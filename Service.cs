using System;
using System.ServiceProcess;

namespace PrimaryServer
{
    class Service
    {
        Log log = new Log();
        public bool IsServiceRunning(ServiceController service)
        {
            return service.Status.Equals(ServiceControllerStatus.Running);
        }

        public void StopService(ServiceController service)
        {
            if (IsServiceRunning(service))
            {
                try
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }
                catch (Exception ex)
                {
                    log.WriteToFile(ex.Message);
                }

            }
        }

        public void StartService(ServiceController service)
        {
            if (!IsServiceRunning(service))
            {
                try
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                }
                catch (Exception ex)
                {
                    log.WriteToFile(ex.Message);
                }
            }
        }
    }
}
