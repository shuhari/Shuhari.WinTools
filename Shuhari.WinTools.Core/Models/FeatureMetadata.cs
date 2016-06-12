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
    }
}
