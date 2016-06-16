using System.Security.Cryptography;
using System.Text;
using Shuhari.Library.Common.Utils;

namespace Shuhari.WinTools.Core.Features.Encode.Providers
{
    public class MD5EncodeProvider : EncodeProvider
    {
        public override EncodeProviderMetadata Metadata
        {
            get { return new EncodeProviderMetadata("MD5", 0, EncodeProviderFunctions.Encode); }
        }

        public override string Encode(string input, Encoding encoding)
        {
            var csp = new MD5CryptoServiceProvider();
            byte[] value = encoding.GetBytes(input);
            byte[] hash = csp.ComputeHash(value);
            csp.Clear();

            return hash.ToHexString();
        }
    }
}
