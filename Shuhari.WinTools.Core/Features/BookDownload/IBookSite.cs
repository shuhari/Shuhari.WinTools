using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuhari.WinTools.Core.Features.BookDownload
{
    public interface IBookSite
    {
        string DisplayName { get; }

        string SiteUrl { get; }

        bool Selected { get; set; }

        void GetItems(BookDownloadContext ctx);
    }
}
