using System.Windows.Controls;
using Shuhari.WinTools.Core.Features;

namespace Shuhari.WinTools.Gui.Views
{
    /// <summary>
    /// 功能视图基类
    /// </summary>
    public abstract class FeatureView : UserControl
    {
        public BaseFeature Feature { get; internal set; }

        public virtual void OnCreated()
        {
        }

        public virtual void OnActivated()
        {
        }

        public virtual void OnDeactivated()
        {
        }
    }
}
