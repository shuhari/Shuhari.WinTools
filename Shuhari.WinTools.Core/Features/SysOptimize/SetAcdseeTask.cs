using Microsoft.Win32;
using Shuhari.Library.Common.Win32;

namespace Shuhari.WinTools.Core.Features.SysOptimize
{
    public class SetAcdseeTask : OptimizeTask
    {
        public override string DisplayName
        {
            get { return "优化 ACDSee 3.1"; }
        }

        public override bool Execute()
        {
            var key = Registry.CurrentUser.CreateSubKey(@"Software\ACD Systems\ACDSeeCS");
            if (key != null)
            {
                key.SetSingleBinary("AutoCacheThumbs", 0);
                key.SetSingleBinary("AutoSaveWinPos", 1);
                key.SetSingleBinary("AutoShrink", 1);
                key.SetSingleBinary("AutoZoom", 0);
                key.SetSingleBinary("BrowserShowFullPath", 1);
                key.SetSingleBinary("HiddenDesc", 0);
                key.SetSingleBinary("ShellPrintAssoc", 0);
                key.SetSingleBinary("ShowHidden", 1);
                key.SetSingleBinary("SSPlayAudio", 0);
                key.SetSingleBinary("StretchThumbs", 0);
                key.SetSingleBinary("VerifyAssocs", 0);

                key.SetBinary("ThumbSize", new byte[] { 0x18, 0x01, 0x00, 0x00, 0xc8, 0x00, 0x00, 0x00 });
            }

            return true;
        }
    }
}

