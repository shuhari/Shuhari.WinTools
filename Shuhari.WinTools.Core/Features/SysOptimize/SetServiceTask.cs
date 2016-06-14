using System.ServiceProcess;
using Microsoft.Win32;
using Shuhari.Library.Common.Win32;

namespace Shuhari.WinTools.Core.Features.SysOptimize
{
    public enum ServiceStartType
    {
        Automatic_DelayedStart = 1,
        Automatic = 2,
        Manual = 3,
        Disabled = 4,
    }

    public class SetServiceTask : OptimizeTask
    {
        public SetServiceTask(string serviceName, ServiceStartType startType, bool stopNow = false)
        {
            _serviceName = serviceName;
            _startType = startType;
            _stopNow = stopNow;
        }

        private string _serviceName;
        private ServiceStartType _startType;
        private bool _stopNow;

        private string StartTypeName
        {
            get
            {
                switch (_startType)
                {
                    case ServiceStartType.Automatic: return "自动";
                    case ServiceStartType.Automatic_DelayedStart: return "自动（延迟启动）";
                    case ServiceStartType.Disabled: return "禁用";
                    case ServiceStartType.Manual: return "手动";
                    default: return "";
                }
            }
        }

        public override string DisplayName
        {
            get
            {
                return string.Format("设置服务 {0} 启动状态为 {1}",
              _serviceName,
              StartTypeName);
            }
        }

        public override bool Execute()
        {
            var regPath = string.Format(@"System\CurrentControlSet\Services\{0}", _serviceName);
            var serviceKey = Registry.LocalMachine.OpenSubKey(regPath, true);
            if (serviceKey != null)
            {
                int newType = (int)_startType;
                var currentType = serviceKey.GetValue("Start");
                if (currentType != null && (int)currentType != newType)
                {
                    serviceKey.SetDWord("Start", newType);
                }
                serviceKey.Close();
            }

            if (_stopNow)
            {
                var service = new ServiceController(_serviceName, null);
                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                }
            }

            return true;
        }
    }
}
