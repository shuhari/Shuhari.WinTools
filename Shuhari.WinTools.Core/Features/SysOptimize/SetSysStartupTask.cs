using Microsoft.Win32;
using Shuhari.Library.Win32;

namespace Shuhari.WinTools.Core.Features.SysOptimize
{
    public class SetSysStartupTask : OptimizeTask
    {
        public override string DisplayName
        {
            get { return "优化系统启动"; }
        }

        public override bool Execute()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\CrashControl", true))
            {
                key.SetDWord("CrashDumpEnabled", 0);
            }

            return true;
        }
    }
}
