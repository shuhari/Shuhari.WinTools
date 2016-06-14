using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Shuhari.Library.Common.Utils;
using Shuhari.Library.Wpf;
using Shuhari.WinTools.Core.Features.BookManage;

namespace Shuhari.WinTools.Gui.Views
{
    /// <summary>
    /// Interaction logic for BookManageView.xaml
    /// </summary>
    public partial class BookManageView : FeatureView
    {
        public BookManageView()
        {
            InitializeComponent();

            Loaded += BookManageView_Loaded;
        }

        private ObservableCollection<TrimItem> _items;
        private ObservableCollection<BookItem> _books;

        private void BookManageView_Loaded(object sender, RoutedEventArgs e)
        {
            _items = new ObservableCollection<TrimItem>();
            list.ItemsSource = _items;

            _books = new ObservableCollection<BookItem>();
            bookList.ItemsSource = _books;

            foreach (var fi in new DirectoryInfo(DIRNAME).GetFiles())
            {
                var newName = GetNewName(fi.Name);
                if (newName != null && newName != fi.Name)
                {
                    var book = new BookItem
                    {
                        OldName = fi.Name,
                        NewName = newName,
                        Selected = true
                    };
                    _books.Add(book);
                }
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = this.BrowseForFolder();
            if (path != null)
                txtDir.Text = path;
        }

        private void btnCompress_Click(object sender, RoutedEventArgs e)
        {
            string path = txtDir.Text.Trim();
            if (path.IsNotBlank())
            {
                txtCompressOutput.Text = string.Empty;
                ComparessFilesInDirectory(new DirectoryInfo(path));
            }
        }

        private void ComparessFilesInDirectory(DirectoryInfo di)
        {
            const string cmd = "C:\\Program Files\\WinRAR\\rar.exe";
            const string paramFormat = "a -df -m5 \"{0}\" \"{1}\"";

            Directory.SetCurrentDirectory(di.FullName);

            foreach (var fi in di.GetFiles())
            {
                var ext = fi.Extension ?? "";
                ext = ext.ToLower();
                if (ext == ".pdf" || ext == ".epub")
                {
                    var zipName = Path.ChangeExtension(fi.Name, ".rar");
                    if (!File.Exists(zipName))
                    {
                        var si = new ProcessStartInfo(cmd, string.Format(paramFormat, zipName, fi.Name));
                        si.CreateNoWindow = true;
                        si.UseShellExecute = false;
                        si.RedirectStandardOutput = true;
                        si.RedirectStandardError = true;
                        var p = Process.Start(si);
                        p.OutputDataReceived += p_OutputDataReceived;
                        p.ErrorDataReceived += p_ErrorDataReceived;
                        p.WaitForExit();
                    }
                }
            }

            foreach (var subDi in di.GetDirectories())
                ComparessFilesInDirectory(subDi);
        }

        void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            var process = (Process)sender;
            using (StreamReader output = process.StandardOutput)
            {
                string text = output.ReadToEnd();
                txtCompressOutput.Text += text;
            }
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var process = (Process)sender;
            using (StreamReader output = process.StandardOutput)
            {
                string text = output.ReadToEnd();
                txtCompressOutput.Text += text;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string path = txtDir.Text.Trim();
            if (path.IsNotBlank())
            {
                _items.Clear();
                SearchDir(new DirectoryInfo(path));
            }
        }

        private void SearchDir(DirectoryInfo dir)
        {
            var re = new Regex(@"(\.\d{10})\.(pdf|epub)$");
            foreach (var fi in dir.GetFiles())
            {
                var match = re.Match(fi.Name);
                if (match.Success)
                {
                    var item = new TrimItem(fi);
                    var part = match.Groups[1].Value;
                    item.NewName = fi.Name.Replace(part, "");
                    _items.Add(item);
                }
            }

            foreach (var subDir in dir.GetDirectories())
                SearchDir(subDir);
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items)
            {
                item.Selected = true;
            }
        }

        private void btnSelectNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items)
            {
                item.Selected = false;
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items)
            {
                item.Apply();
            }
        }

        const string DIRNAME = @"D:\Books\Upload\baiduyun";

        private string GetNewName(string fileName)
        {
            var re = new Regex(@"\.(.{10}\.)(pdf|epub)$");
            var match = re.Match(fileName);
            if (match.Success)
                return fileName.Replace(match.Groups[1].Value, "");

            re = new Regex(@"\.(.{10}_CODE)\.zip$");
            match = re.Match(fileName);
            if (match.Success)
                return fileName.Replace(match.Groups[1].Value, "CODE");

            return null;
        }

        private void btnCleanApply_Click(object sender, RoutedEventArgs e)
        {
            foreach (var book in _books.Where(b => b.Selected))
            {
                var oldPath = Path.Combine(DIRNAME, book.OldName);
                var newPath = Path.Combine(DIRNAME, book.NewName);
                try
                {
                    Directory.Move(oldPath, newPath);
                }
                catch (Exception exp)
                {
                    statusText.Content = exp.Message;
                }
            }
        }

        private void chkSelectAll_Click(object sender, RoutedEventArgs e)
        {
            var isChecked = (bool)chkSelectAll.IsChecked;
            foreach (var book in _books)
            {
                book.Selected = isChecked;
            }
        }
    }
}
