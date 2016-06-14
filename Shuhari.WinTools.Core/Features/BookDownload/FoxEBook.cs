using System.Collections.Generic;
using HtmlAgilityPack;

namespace Shuhari.WinTools.Core.Features.BookDownload
{
    public class FoxEBook : BookSiteBase
    {
        public FoxEBook()
            : base("foxebook", "http://www.foxebook.net/")
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

            foreach (var node in doc.SelectNodes("//div[@class='col-sm-6 col-md-3 col-lg-2']/div[@class='thumbnail']"))
            {
                var item = new BookDownloadItem();
                item.BookSite = "foxebook";

                var title = node.SelectSingleNode("span[@class='book-title']");
                if (title == null)
                    continue;
                item.Title = title.InnerText;

                var a = node.SelectSingleNode("a[@rel='bookmark']");
                if (a == null)
                    continue;
                item.PageUrl = HtmlHelper.CombineUrl(SiteUrl, a.Attributes["href"].Value);

                var img = a.SelectSingleNode("img[@class='cover']");
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

            var trs = doc.SelectNodes("//div[@id='download']/div[@class='panel-body']/table/tbody/tr");
            foreach (var tr in trs)
            {
                var item = parent.Clone();

                item.DownloadProvider = tr.SelectSingleNode("td[1]").InnerText;
                item.DownloadUrl = tr.SelectSingleNode("td[2]/a").Attributes["href"].Value;
                item.Format = tr.SelectSingleNode("td[3]").InnerText;

                result.Add(item);
            }

            return result.ToArray();
        }
    }
}
