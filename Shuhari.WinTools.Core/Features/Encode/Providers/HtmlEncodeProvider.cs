using System.Text;
using System.Web;

namespace Shuhari.WinTools.Core.Features.Encode.Providers
{
    public class HtmlEncodeProvider : EncodeProvider
    {
        public override EncodeProviderMetadata Metadata
        {
            get { return new EncodeProviderMetadata("HTML", 2, EncodeProviderFunctions.All); }
        }

        public override string Encode(string input, Encoding encoding)
        {
            return HttpUtility.HtmlEncode(input);
        }

        public override string Decode(string input, Encoding encoding)
        {
            return HttpUtility.HtmlDecode(input);
        }
    }
}
