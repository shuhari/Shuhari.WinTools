using System.Collections.Generic;
using HtmlAgilityPack;

namespace Shuhari.WinTools.Core.Features.BookDownload
{
    public class BookDL : BookSiteBase
    {
        public BookDL()
            : base("bookdl", "http://bookdl.com/")
        {
        }

        protected override void GetItemsOverride()
        {
            for (int page = 1; page <= Context.PageCount; page++)
            {
                string pageUrl = HtmlHelper.CombineUrl(SiteUrl, "page/" + page);
                ReportMessage("正在读取第 {0} 页", page);
                var doc = GetDoc(pageUrl);
                var books = doc.Extract(null, ExtractBooks);
                Wait();

                var result = new List<BookDownloadItem>();
                foreach (var book in books)
                {
                    ReportMessage("正在读取页面: {0}", book.PageUrl);
                    doc = GetDoc(book.PageUrl);
                    var items = doc.Extract(book, ExtractItems);
                    ReportItems(items);
                    Wait();
                }
            }
        }

        private BookDownloadItem[] ExtractBooks(HtmlDocument doc, BookDownloadItem parent)
        {
            var result = new List<BookDownloadItem>();

            foreach (var node in doc.SelectNodes("//div[@id='books']/div[@class='bookitem']"))
            {
                var item = new BookDownloadItem();
                item.BookSite = "bookdl";

                var a = node.SelectSingleNode("a");
                if (a == null)
                    continue;
                item.PageUrl = a.Attributes["href"].Value;
                item.Title = a.Attributes["title"].Value;

                var img = a.SelectSingleNode("img[@class='bookcover']");
                if (img == null)
                    continue;
                item.ImageUrl = img.Attributes["src"].Value;
                if (item.ImageUrl.StartsWith("//"))
                    item.ImageUrl = "http:" + item.ImageUrl;

                result.Add(item);
            }

            return result.ToArray();
        }

        private BookDownloadItem[] ExtractItems(HtmlDocument doc, BookDownloadItem parent)
        {
            var result = new List<BookDownloadItem>();

            var links = doc.SelectNodes("//div[@id='download']/a");
            foreach (var link in links)
            {
                var item = parent.Clone();
                item.DownloadProvider = "bookdl";
                item.DownloadUrl = link.Attributes["href"].Value;
                var className = link.Attributes["class"].Value;
                if (className.Contains("pdf"))
                    item.Format = "pdf";
                else if (className.Contains("epub"))
                    item.Format = "epub";
                else if (className.Contains("extras"))
                    item.Format = "extras";
                if (item.Format != null)
                {
                    result.Add(item);
                }
            }

            return result.ToArray();
        }
    }
}
