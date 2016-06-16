using System.Text;
using System.Web;

namespace Shuhari.WinTools.Core.Features.Encode.Providers
{
    public class UrlEncodeProvider : EncodeProvider
    {
        public override EncodeProviderMetadata Metadata
        {
            get { return new EncodeProviderMetadata("Url", 3, EncodeProviderFunctions.Encode); }
        }

        public override string Encode(string input, Encoding encoding)
        {
            return HttpUtility.UrlEncode(input, encoding);
        }

        public override string Decode(string input, Encoding encoding)
        {
            return HttpUtility.UrlDecode(input, encoding);
        }
    }
}
