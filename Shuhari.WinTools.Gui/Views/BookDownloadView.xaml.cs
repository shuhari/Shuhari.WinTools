using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Shuhari.WinTools.Core.Features.BookDownload;

namespace Shuhari.WinTools.Gui.Views
{
    /// <summary>
    /// Interaction logic for BookDownloadView.xaml
    /// </summary>
    public partial class BookDownloadView : FeatureView
    {
        public BookDownloadView()
        {
            InitializeComponent();

            Loaded += BookDownloadView_Loaded;
        }

        private IBookSite[] _sites;
        private BackgroundWorker _worker;
        private ObservableCollection<BookDownloadItem> _items;

        private void BookDownloadView_Loaded(object sender, RoutedEventArgs e)
        {
            _sites = new IBookSite[] {
                new FoxEBook(),
                new BookDL(),
            };
            siteList.ItemsSource = _sites;

            cboPageCount.ItemsSource = new int[] { 1, 2, 3 };
            cboPageCount.SelectedItem = 3;

            _items = new ObservableCollection<BookDownloadItem>();
            itemList.ItemsSource = _items;

            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += worker_DoWork;
            _worker.ProgressChanged += worker_ProgressChanged;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            var selectedSites = _sites.Where(it => it.Selected).ToArray();
            if (selectedSites.Length == 0)
            {
                MessageBox.Show("请选择站点");
                return;
            }
            int pageCount = Convert.ToInt32(cboPageCount.SelectedItem);

            var ctx = new BookDownloadContext
            {
                Sites = selectedSites,
                PageCount = pageCount
            };
            btnStart.IsEnabled = false;
            _items.Clear();
            _worker.RunWorkerAsync(ctx);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var ctx = (BookDownloadContext)e.Argument;
            ctx.Worker = _worker;

            try
            {
                foreach (var site in ctx.Sites)
                {
                    try
                    {
                        site.GetItems(ctx);
                    }
                    catch (Exception exp)
                    {
                        _worker.ReportProgress(0, string.Format("站点 {0} 获取失败: {1}", site.DisplayName, exp.Message));
                    }
                }
                _worker.ReportProgress(0, "获取完毕");
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string)
            {
                var msg = (string)e.UserState;
                statusText.Content = msg;
            }
            else if (e.UserState is BookDownloadItem[])
            {
                var items = (BookDownloadItem[])e.UserState;
                foreach (var item in items)
                    _items.Add(item);
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnStart.IsEnabled = true;
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items)
                item.Selected = true;
        }

        private void btnSelectNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items)
                item.Selected = false;
        }

        private void btnCopyLink_Click(object sender, RoutedEventArgs e)
        {
            var urls = _items.Where(it => it.Selected)
                .Select(it => it.DownloadUrl)
                .ToArray();
            string content = string.Join("\r\n", urls);
            Clipboard.SetText(content);
        }
    }
}
