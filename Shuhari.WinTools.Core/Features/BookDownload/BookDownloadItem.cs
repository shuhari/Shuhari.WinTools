using System.ComponentModel;

namespace Shuhari.WinTools.Core.Features.BookDownload
{
    public class BookDownloadItem : INotifyPropertyChanged
    {
        private bool _selected = false;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    NotifyProperty("Selected");
                }
            }
        }

        public string Title { get; set; }

        public string ImageUrl { get; set; }

        public string Format { get; set; }

        public string BookSite { get; set; }

        public string DownloadProvider { get; set; }

        public string DownloadUrl { get; set; }

        public string PageUrl { get; set; }

        public BookDownloadItem Clone()
        {
            return new BookDownloadItem
            {
                Title = this.Title,
                ImageUrl = this.ImageUrl,
                Format = this.Format,
                BookSite = this.BookSite,
                DownloadProvider = this.DownloadProvider,
                DownloadUrl = this.DownloadUrl,
                PageUrl = this.PageUrl
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyProperty(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
