using System;

namespace Shuhari.WinTools.Core.Features.Encode.Providers
{
    [Flags]
    public enum EncodeProviderFunctions
    {
        Encode = 0x1,

        Decode = 0x2,

        All = Encode | Decode
    };

}
