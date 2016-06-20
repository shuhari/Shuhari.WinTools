using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Shuhari.Library.Utils;
using Shuhari.WinTools.Core.Features;
using Shuhari.WinTools.Core.Models;

namespace Shuhari.WinTools.Core.Services
{
    public class FeatureService
    {
        public static BaseFeature[] LoadFeatures()
        {
            var result = new List<BaseFeature>();
            var assembly = typeof(BaseFeature).Assembly;

            var featureJson = assembly.GetResourceText("Resources/features.json", Encoding.UTF8);
            var metadatas = JsonConvert.DeserializeObject<FeatureMetadata[]>(featureJson);

            foreach (var metadata in metadatas)
            {
                var type = assembly.GetType(metadata.FeatureClass);
                if (type == null)
                    continue; // TODO: remove at final

                // Verify.IsNotNull(type, nameof(type));
                var feature = (BaseFeature)Activator.CreateInstance(type);
                feature.Metadata = metadata;
                result.Add(feature);
            }

            return result.ToArray();
        }
    }
}
