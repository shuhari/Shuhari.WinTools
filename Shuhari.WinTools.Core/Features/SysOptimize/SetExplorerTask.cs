using Microsoft.Win32;
using Shuhari.Library.Win32;

namespace Shuhari.WinTools.Core.Features.SysOptimize
{
    public class SetExplorerTask : OptimizeTask
    {
        public override string DisplayName
        {
            get { return "优化资源管理器"; }
        }

        public override bool Execute()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer", true))
            {
                key.SetDWord("EnableAutoTray", 0);
                key.SetBinary("link", new byte[] { 0x00, 0x00, 0x00, 0x00 });
            }

            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true))
            {
                key.SetDWord("AlwaysShowMenus", 1);
                key.SetDWord("Hidden", 1);
                key.SetDWord("HideFileExt", 0);
                key.SetDWord("NavPaneExpandToCurrentFolder", 1);
                key.SetDWord("NavPaneShowAllFolders", 1);
                key.SetDWord("SharingWizardOn", 0);
                key.SetDWord("ShowSuperHidden", 1);
                key.SetDWord("TaskbarAnimations", 0);
                key.SetDWord("TaskbarSmallIcons", 1);
                key.SetDWord("NoNetCrawling", 1);
            }

            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true))
            {
                key.SetDWord("NoInternetOpenWith", 1);
                key.SetDWord("NoLowDiskSpaceChecks", 1);
            }

            using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
                key.SetString("FontSmoothing", "2");
                key.SetDWord("FontSmoothingType", 2);
            }

            return true;
        }
    }
}
