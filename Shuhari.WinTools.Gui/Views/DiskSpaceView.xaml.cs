using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Shuhari.Library.Windows;
using Shuhari.WinTools.Core.Features.DiskSpace;

namespace Shuhari.WinTools.Gui.Views
{
    /// <summary>
    /// Interaction logic for DiskSpaceView.xaml
    /// </summary>
    public partial class DiskSpaceView : FeatureView
    {
        public DiskSpaceView()
        {
            InitializeComponent();
            Loaded += DiskSpaceView_Loaded;
        }

        private void DiskSpaceView_Loaded(object sender, RoutedEventArgs e)
        {
            var drives = DriveInfo.GetDrives()
                .Where(drv => drv.DriveType == DriveType.Fixed || drv.DriveType == DriveType.Removable)
                .Select(drv => drv.RootDirectory)
                .ToArray();
            driveList.ItemsSource = drives;
            driveRadio.IsChecked = true;
            optionRadio_Click(this, new RoutedEventArgs());

            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
            };
            _worker.DoWork += worker_DoWork;
            _worker.ProgressChanged += worker_ProgressChanged;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        private BackgroundWorker _worker;

        private void optionRadio_Click(object sender, RoutedEventArgs e)
        {
            bool isDrive = (driveRadio.IsChecked == true);
            driveList.IsEnabled = isDrive;
            txtDirName.IsEnabled = !isDrive;
            btnBrowse.IsEnabled = !isDrive;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var path = this.BrowseForFolder();
            if (path != null)
            {
                txtDirName.Text = path;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            var rootDir = GetRootDir();
            if (!string.IsNullOrEmpty(rootDir))
            {
                btnStart.IsEnabled = false;
                progress.Visibility = Visibility.Visible;
                _worker.RunWorkerAsync(rootDir);
            }
        }

        private string GetRootDir()
        {
            if (driveRadio.IsChecked == true)
            {
                var drive = driveList.SelectedItem as DirectoryInfo;
                if (drive != null)
                    return drive.FullName;
            }
            else
            {
                return txtDirName.Text;
            }
            return null;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progress.Visibility = Visibility.Collapsed;
            statusTextPane.Content = "查找完毕";
            btnStart.IsEnabled = true;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage > 0)
                progress.Value = e.ProgressPercentage;

            if (e.UserState is string)
                statusTextPane.Content = (string)e.UserState;
            else if (e.UserState is DirectorySizeItem)
                tree.Model = (DirectorySizeItem)e.UserState;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var rootDir = (string)e.Argument;

            try
            {
                var dir = new DirectoryInfo(rootDir);
                var re = new Regex(@"^[a-zA-Z]:\$", RegexOptions.IgnoreCase);
                FileSizeContext context;
                if (re.Match(rootDir).Success)
                {
                    context = new FileSizeContext(new DriveInfo(rootDir).TotalFreeSpace);
                }
                else
                    context = new FileSizeContext(0);
                // TODO: progress.IsIndeterminate = (context.DiskSize == 0);

                DirectorySizeItem dsi = LoadDirectoryRecursive(context, dir);
                ResolveSizeRecursive(dsi);
                _worker.ReportProgress(0, dsi);
            }
            catch (Exception exp)
            {
                ((App)Application.Current).LogException(exp);
            }
        }

        private DirectorySizeItem LoadDirectoryRecursive(FileSizeContext context, DirectoryInfo di)
        {
            var dsi = new DirectorySizeItem(context);
            dsi.Load(di);

            if (di.FullName.Length >= 230) // 再向下查找超过260个字符有可能引起异常，略过
                return dsi;

            try
            {
                foreach (var fi in di.GetFiles())
                {
                    var fsi = new FileSizeItem(context);
                    fsi.Load(fi);
                    dsi.Files.Add(fsi);
                    context.ReadedSize += fsi.Size;
                }
            }
            catch (UnauthorizedAccessException)
            {
            }

            _worker.ReportProgress(context.ReadedPercentage, string.Format("正在读取目录: {0}", di.FullName));

            try
            {
                foreach (var subDi in di.GetDirectories())
                {
                    var subDsi = LoadDirectoryRecursive(context, subDi);
                    dsi.SubDirectories.Add(subDsi);
                }
            }
            catch (UnauthorizedAccessException)
            {
            }

            return dsi;
        }

        private void ResolveSizeRecursive(DirectorySizeItem dsi)
        {
            foreach (var subDsi in dsi.SubDirectories)
                ResolveSizeRecursive(subDsi);

            dsi.SubDirectories.Sort();
            dsi.Files.Sort();
            dsi.ResolveSize();
        }
    }
}
