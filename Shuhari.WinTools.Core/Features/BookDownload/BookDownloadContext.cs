using System.ComponentModel;

namespace Shuhari.WinTools.Core.Features.BookDownload
{
    public class BookDownloadContext
    {
        public IBookSite[] Sites { get; set; }

        public int PageCount { get; set; }

        public BackgroundWorker Worker { get; set; }
    }
}
