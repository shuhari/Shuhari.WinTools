namespace Shuhari.WinTools.Core.Features.Encode.Providers
{
    public sealed class EncodeProviderMetadata
    {
        public EncodeProviderMetadata(string displayName, int order, EncodeProviderFunctions functions)
        {
            this.DisplayName = displayName;
            this.Order = order;
            this.Functions = functions;
        }

        public string DisplayName { get; private set; }

        public int Order { get; private set; }

        public EncodeProviderFunctions Functions { get; private set; }
    }
}
