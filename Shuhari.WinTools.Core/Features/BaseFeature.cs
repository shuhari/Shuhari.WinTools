using Shuhari.WinTools.Core.Models;

namespace Shuhari.WinTools.Core.Features
{
    /// <summary>
    /// 功能项
    /// </summary>
    public abstract class BaseFeature
    {
        public BaseFeature()
        {
        }

        /// <summary>
        /// Metadata
        /// </summary>
        public FeatureMetadata Metadata { get; internal set; }
    }
}
