using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using HtmlAgilityPack;

namespace Shuhari.WinTools.Core.Features.BookDownload
{
    public abstract class BookSiteBase : IBookSite
    {
        public BookSiteBase(string displayName, string siteUrl)
        {
            this.DisplayName = displayName;
            this.SiteUrl = siteUrl;
            this.Selected = true;
        }

        public string DisplayName { get; private set; }

        public string SiteUrl { get; private set; }

        public bool Selected { get; set; }

        protected BookDownloadContext Context { get; private set; }

        public void GetItems(BookDownloadContext ctx)
        {
            this.Context = ctx;
            ReportMessage("正在处理网站: {0}", this.DisplayName);
            GetItemsOverride();
        }

        protected abstract void GetItemsOverride();

        protected void ReportMessage(string format, params object[] args)
        {
            string msg = string.Format(format, args);
            Context.Worker.ReportProgress(0, msg);
        }

        protected void ReportItems(BookDownloadItem[] items)
        {
            Context.Worker.ReportProgress(0, items);
        }

        protected HtmlDocument GetDoc(string url)
        {
            string content = null;
            var req = WebRequest.CreateHttp(url);
            using (var res = req.GetResponse())
            using (var stream = res.GetResponseStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            return doc;
        }

        protected void Wait()
        {
            Thread.Sleep(TimeSpan.FromSeconds(3));
        }
    }
}
