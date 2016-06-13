using System;
using System.Collections.Generic;
using Shuhari.Library.Wpf.Controls;
using Shuhari.WinTools.Core.Services;
using Shuhari.WinTools.Gui.Models;
using Shuhari.WinTools.Gui.Views;

namespace Shuhari.WinTools.Gui.Services
{
    public class ApplicationService
    {
        public FeatureModel[] CreateModels(ViewStack parent)
        {
            var result = new List<FeatureModel>();
            var assembly = typeof(ApplicationService).Assembly;

            var features = FeatureService.LoadFeatures();
            foreach (var feature in features)
            {
                var viewType = assembly.GetType(feature.Metadata.ViewClass);
                if (viewType != null)
                {
                    var model = new FeatureModel();
                    model.Feature = feature;
                    model.Feature.Metadata.IconPath = string.Format("pack://application:,,,/{0}", model.Feature.Metadata.IconPath);
                    model.View = (FeatureView)Activator.CreateInstance(viewType);
                    parent.Children.Add(model.View);
                    result.Add(model);
                }
            }

            return result.ToArray();
        }
    }
}
