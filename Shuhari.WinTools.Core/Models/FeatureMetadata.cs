using Newtonsoft.Json;

namespace Shuhari.WinTools.Core.Models
{
    public class FeatureMetadata
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("iconPath")]
        public string IconPath { get; set; }

        [JsonProperty("featureClass")]
        public string FeatureClass { get; set; }

        [JsonProperty("viewClass")]
        public string ViewClass { get; set; }
    }
}
