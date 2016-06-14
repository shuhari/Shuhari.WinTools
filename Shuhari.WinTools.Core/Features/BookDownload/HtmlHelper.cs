using System;
using System.Linq;
using HtmlAgilityPack;

namespace Shuhari.WinTools.Core.Features.BookDownload
{
    static class HtmlHelper
    {
        public static T Extract<T>(this HtmlDocument doc, BookDownloadItem parent, Func<HtmlDocument, BookDownloadItem, T> handler)
        {
            return handler(doc, parent);
        }

        public static HtmlNode[] SelectNodes(this HtmlDocument doc, string xpath)
        {
            var nodes = doc.DocumentNode.SelectNodes(xpath);
            if (nodes != null)
                return nodes.OfType<HtmlNode>().ToArray();
            else
                return new HtmlNode[0];
        }

        public static string CombineUrl(string prefix, string suffix)
        {
            if (prefix.EndsWith("/") && suffix.StartsWith("/"))
                return prefix.TrimEnd('/') + suffix;
            else if (prefix.EndsWith("/") || suffix.EndsWith("/"))
                return prefix + suffix;
            else
                return prefix + "/" + suffix;
        }
    }
}
