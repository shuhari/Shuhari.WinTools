using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using Shuhari.Library.Windows;

namespace Shuhari.WinTools.Gui.Views
{
    /// <summary>
    /// Interaction logic for CleanDirView.xaml
    /// </summary>
    public partial class CleanDirView : FeatureView
    {
        public CleanDirView()
        {
            InitializeComponent();

            Loaded += CleanDirView_Loaded;
        }

        private BackgroundWorker _worker;
        private ObservableCollection<string> _logs;

        private void CleanDirView_Loaded(object sender, RoutedEventArgs e)
        {
            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };
            _worker.DoWork += worker_DoWork;
            _worker.ProgressChanged += worker_ProgressChanged;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            _logs = new ObservableCollection<string>();
            logList.ItemsSource = _logs;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var path = this.BrowseForFolder();
            if (path != null)
            {
                txtDirName.Text = path;
            }
        }

        private void btnClean_Click(object sender, RoutedEventArgs e)
        {
            string baseDir = txtDirName.Text;
            if (!string.IsNullOrWhiteSpace(baseDir))
            {
                btnClean.IsEnabled = false;
                _logs.Clear();
                _worker.RunWorkerAsync(baseDir);
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnClean.IsEnabled = true;
            _logs.Add("清理完毕");
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var log = e.UserState as string;
            if (log != null)
            {
                _logs.Add(log);
                logList.SelectedItem = log;
                logList.ScrollIntoView(log);
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string baseDir = (string)e.Argument;

            CleanDir(new DirectoryInfo(baseDir));
        }

        private DirectoryInfo[] SafeGetDirectories(DirectoryInfo di)
        {
            try
            {
                return di.GetDirectories();
            }
            catch (UnauthorizedAccessException)
            {
                return new DirectoryInfo[0];
            }
        }

        private FileInfo[] SafeGetFiles(DirectoryInfo di)
        {
            try
            {
                return di.GetFiles();
            }
            catch (UnauthorizedAccessException)
            {
                return new FileInfo[0];
            }
        }

        private void CleanDir(DirectoryInfo di)
        {
            foreach (var subDi in SafeGetDirectories(di))
            {
                CleanDir(subDi);
            }

            var allFiles = SafeGetFiles(di);
            var filesToDelete = allFiles.Where(IsDeletableFile).ToArray();
            if (filesToDelete.Length == allFiles.Length)
            {
                foreach (var fi in filesToDelete)
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch (Exception exp)
                    {
                        var log = string.Format("删除文件失败: {0}, 原因={1}", fi.FullName, exp.Message);
                        _worker.ReportProgress(0, log);
                    }
                }
            }

            if (SafeGetDirectories(di).Length == 0 && SafeGetFiles(di).Length == 0)
            {
                try
                {
                    di.Delete();
                    var log = string.Format("删除目录成功: {0}", di.FullName);
                    _worker.ReportProgress(0, log);
                }
                catch (Exception exp)
                {
                    var log = string.Format("删除目录失败: {0}, 原因={1}", di.FullName, exp.Message);
                    _worker.ReportProgress(0, log);
                }
            }
        }

        private bool IsDeletableFile(FileInfo fi)
        {
            string[] names = { "Thumbs.db", "imgfind.hash.xml" };
            foreach (var name in names)
            {
                if (fi.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
