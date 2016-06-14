using System.Diagnostics;

namespace Shuhari.WinTools.Core.Features.SysOptimize
{
    public class SetPowerCfgTask : OptimizeTask
    {
        public override string DisplayName
        {
            get { return "禁用休眠"; }
        }

        public override bool Execute()
        {
            var si = new ProcessStartInfo("powercfg", "-h off");
            si.UseShellExecute = false;
            Process.Start(si);

            return true;
        }
    }
}
