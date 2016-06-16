using System;
using System.Collections.Generic;
using System.Linq;
using Shuhari.WinTools.Core.Features.Encode.Providers;

namespace Shuhari.WinTools.Core.Features.Encode
{
    public class Feature : BaseFeature
    {
        public EncodeProvider[] GetProviders()
        {
            var result = new List<EncodeProvider>();

            foreach (var type in typeof(EncodeProvider).Assembly.GetExportedTypes()
                .Where(EncodeProvider.IsProviderType))
            {
                var provider = (EncodeProvider)Activator.CreateInstance(type);
                result.Add(provider);
            }

            return result.ToArray();
        }
    }
}
