using System;
using System.Text;

namespace Shuhari.WinTools.Core.Features.Encode.Providers
{
    public class Base64EncodeProvider : EncodeProvider
    {
        public override EncodeProviderMetadata Metadata
        {
            get { return new EncodeProviderMetadata("Base64", 1, EncodeProviderFunctions.All); }
        }

        public override string Encode(string input, Encoding encoding)
        {
            return Convert.ToBase64String(encoding.GetBytes(input));
        }

        public override string Decode(string input, Encoding encoding)
        {
            return encoding.GetString(Convert.FromBase64String(input));
        }
    }
}
