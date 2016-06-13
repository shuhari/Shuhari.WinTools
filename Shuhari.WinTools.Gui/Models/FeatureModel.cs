using Shuhari.WinTools.Core.Features;
using Shuhari.WinTools.Gui.Views;

namespace Shuhari.WinTools.Gui.Models
{
    public class FeatureModel
    {
        public BaseFeature Feature { get; internal set; }

        public FeatureView View { get; internal set; }
    }
}
