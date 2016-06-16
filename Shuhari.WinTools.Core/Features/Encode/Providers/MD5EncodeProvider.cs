using System.Security.Cryptography;
using System.Text;

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

            StringBuilder sb = new StringBuilder();
            for (int i = 0, len = hash.Length; i < len; i++)
            {
                sb.Append(hash[i].ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString();

        }
    }
}
