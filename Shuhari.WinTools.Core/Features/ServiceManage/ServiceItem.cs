using System.ServiceProcess;

namespace Shuhari.WinTools.Core.Features.ServiceManage
{
    public class ServiceItem
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public ServiceControllerStatus Status { get; set; }

        public ServiceStartMode StartMode { get; set; }
    }
}
