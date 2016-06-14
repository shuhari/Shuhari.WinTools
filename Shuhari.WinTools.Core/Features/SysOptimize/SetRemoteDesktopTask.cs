using Microsoft.Win32;
using Shuhari.WinTools.Core.Utils;

namespace Shuhari.WinTools.Core.Features.SysOptimize
{
    public class SetRemoteDesktopTask : OptimizeTask
    {
        public SetRemoteDesktopTask(bool enable)
        {
            _enable = enable;
        }

        private bool _enable;

        public override string DisplayName
        {
            get { return string.Format("{0} 远程桌面", _enable ? "启用" : "禁用"); }
        }

        public override bool Execute()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server", true))
            {
                key.SetDWord("fDenyTSConnections", _enable ? 0 : 1);
            }

            return true;
        }
    }
}
