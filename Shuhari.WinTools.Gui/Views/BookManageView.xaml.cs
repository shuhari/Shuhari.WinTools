using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Shuhari.Library.Common.Utils;
using Shuhari.Library.Wpf;

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

        private void BookManageView_Loaded(object sender, RoutedEventArgs e)
        {
            _items = new ObservableCollection<TrimItem>();
            list.ItemsSource = _items;
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
    }

    class TrimItem : INotifyPropertyChanged
    {
        public TrimItem(FileInfo file)
        {
            this.File = file;
            this.Selected = true;
        }

        public FileInfo File { get; private set; }

        public string OldName
        {
            get { return File.Name; }
        }

        public string DirName
        {
            get { return File.DirectoryName; }
        }

        public string NewName { get; set; }

        private bool _selected;
        private string _result;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Selected"));
                }
            }
        }

        public string Result
        {
            get { return _result; }
            set
            {
                if (_result != value)
                {
                    _result = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Result"));
                }
            }
        }

        public void Apply()
        {
            try
            {
                var newFullName = Path.Combine(DirName, NewName);
                File.MoveTo(newFullName);
                Result = "Success";
            }
            catch (Exception exp)
            {
                Result = exp.Message;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
