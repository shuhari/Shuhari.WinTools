using System;
using System.Collections.Generic;
using System.Linq;
using Shuhari.Library.Utils;
using Shuhari.Library.Windows.Controls;
using Shuhari.WinTools.Core.Features;
using Shuhari.WinTools.Core.Services;
using Shuhari.WinTools.Gui.Views;

namespace Shuhari.WinTools.Gui.Services
{
    public class ApplicationService
    {
        public BaseFeature[] LoadFeatures(ViewStack parent)
        {
            var result = new List<BaseFeature>();
            var assembly = typeof(ApplicationService).Assembly;

            var features = FeatureService.LoadFeatures();
            foreach (var feature in features)
            {
                feature.Metadata.IconPath = string.Format("pack://application:,,,/{0}", feature.Metadata.IconPath);
            }

            return features;
        }

        public FeatureView EnsureView(BaseFeature feature, ViewStack parent)
        {
            var view = parent.Children.OfType<FeatureView>().FirstOrDefault(it => it.Feature == feature);
            if (view == null)
            {
                string viewClass = feature.Metadata.ViewClass;
                if (viewClass.IsNotBlank())
                {
                    var viewType = typeof(FeatureView).Assembly.GetType(viewClass);
                    if (viewType != null)
                    {
                        view = (FeatureView)Activator.CreateInstance(viewType);
                        view.Feature = feature;
                        parent.Children.Add(view);
                        view.OnCreated();
                    }
                }
            }

            return view;
        }
    }
}
