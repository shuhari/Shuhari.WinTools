using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Shuhari.Library.Utils;
using Shuhari.Library.Win32;
using Shuhari.Library.Windows;
using Shuhari.WinTools.Core.Features.ImageFinder;
using Shuhari.WinTools.Core.Features.ImageFinder.Args;

namespace Shuhari.WinTools.Gui.Views
{
    /// <summary>
    /// Interaction logic for ImageFinderView.xaml
    /// </summary>
    public partial class ImageFinderView : FeatureView, IImageFinderUI
    {
        public ImageFinderView()
        {
            InitializeComponent();

            Loaded += ImageFinderView_Loaded;
        }

        private void ImageFinderView_Loaded(object sender, RoutedEventArgs e)
        {
            EnterState(State.Stopped);
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            txtDir1.Text = configuration.AppSettings.Settings["imageFinder.dir1"].Value;
            txtDir2.Text = configuration.AppSettings.Settings["imageFinder.dir2"].Value;
            txtDir3.Text = configuration.AppSettings.Settings["imageFinder.dir3"].Value;
        }

        private State _state;
        private FinderTask _task;
        private FileItemCollection _files;

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanEnterState(State.Working))
                return;

            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings["imageFinder.dir1"].Value = txtDir1.Text.Trim();
            configuration.AppSettings.Settings["imageFinder.dir2"].Value = txtDir2.Text.Trim();
            configuration.AppSettings.Settings["imageFinder.dir3"].Value = txtDir3.Text.Trim();
            configuration.Save();

            var dirs = new List<string>();
            AddValidDir(dirs, txtDir1);
            AddValidDir(dirs, txtDir2);
            AddValidDir(dirs, txtDir3);
            if (dirs.Count > 0)
            {
                if (_files != null)
                {
                    _files.SelectionChanged -= files_SelectionChanged;
                }

                _files = new FileItemCollection();
                _files.SelectionChanged += files_SelectionChanged;
                fileList.ItemsSource = _files;
                _task = new FinderTask(this, dirs.ToArray());
                _task.Start();
            }
            else
            {
                MessageBox.Show("请输入目录");
            }
        }

        private void files_SelectionChanged(object sender, EventArgs e)
        {
            UpdateSelectionMsg();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (!CanEnterState(State.Stopped) || _task == null)
                return;
            _task.Stop();
        }

        private void chkShowPreview_Click(object sender, RoutedEventArgs e)
        {
            previewPanel.Show(chkShowPreview.IsChecked.GetValueOrDefault());
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var fileItemArray = _files.Where(f => f.Selected).ToArray();
            if (fileItemArray.Length == 0)
                return;

            int successCount = 0;
            int failedCount = 0;
            int dirCount = 0;
            long size = 0;
            for (int index = fileItemArray.Length - 1; index >= 0; --index)
            {
                FileItem file = fileItemArray[index];
                if (DeleteFile(file))
                {
                    _files.Remove(file);
                    ++successCount;
                    size += file.Size;
                    if (CleanDir(file.DirName))
                        ++dirCount;
                }
                else
                    ++failedCount;
                if (index % 100 == 0)
                    GC.Collect();
            }
            _files.RemoveOrphans();
            sbiText.Content = string.Format("删除成功: {0}, 失败: {1}, 释放空间: {2}, 清理目录数={3}", 
                successCount, failedCount, FormatSize(size), dirCount);
        }

        private void fileList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                FileItem selectedFile = GetSelectedFile();
                if (selectedFile != null)
                {
                    _files.Select(selectedFile, !selectedFile.Selected);
                }
            }
        }

        internal bool CanEnterState(State destState)
        {
            switch (destState)
            {
                case State.Stopped:
                    return _state == State.Working;
                case State.Working:
                    return _state == State.Stopped;
                default:
                    return false;
            }
        }

        private void AddValidDir(List<string> dirs, TextBox tb)
        {
            string path = tb.Text.Trim();
            if (path == null || !Directory.Exists(path))
                return;
            dirs.Add(path);
        }

        private void fileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
                return;
            FileItem[] sameGroup = _files.GetSameGroup((FileItem)e.AddedItems[0]);
            Preview(preview0, sameGroup, 0);
            Preview(preview1, sameGroup, 1);
            Preview(preview2, sameGroup, 2);
            Preview(preview3, sameGroup, 3);
            Preview(preview4, sameGroup, 4);
        }

        private void Preview(Image img, FileItem[] files, int index)
        {
            BitmapImage bitmapImage1 = img.Source as BitmapImage;
            if (bitmapImage1 != null)
            {
                Stream streamSource = bitmapImage1.StreamSource;
                img.Source = null;
                if (streamSource != null)
                {
                    img.Source = null;
                    streamSource.Dispose();
                }
            }
            if (index >= files.Length)
                return;
            FileItem fileItem = files[index];
            try
            {
                byte[] buffer = File.ReadAllBytes(fileItem.GetFullPath());
                BitmapImage bitmapImage2 = new BitmapImage();
                bitmapImage2.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage2.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmapImage2.BeginInit();
                bitmapImage2.StreamSource = new MemoryStream(buffer);
                bitmapImage2.EndInit();
                img.Source = bitmapImage2;
            }
            catch (Exception ex)
            {
            }
        }

        private void mnuSelectSameDir_Click(object sender, RoutedEventArgs e)
        {
            FileItem selectedFile = GetSelectedFile();
            if (selectedFile != null)
            {
                _files.SelectSameDir(selectedFile, true);
            }
        }

        private FileItem GetSelectedFile()
        {
            return fileList.SelectedItem as FileItem;
        }

        private void UpdateSelectionMsg()
        {
            int selectCount;
            int[] allSelectedGroups;
            _files.GetSelectInfo(out selectCount, out allSelectedGroups);
            sbiText.Content = string.Format("共选中 {0} 个文件", selectCount);
            if (allSelectedGroups.Length > 0)
                MessageBox.Show("以下分组文件被全部选中，请谨慎删除！\r\n" + string.Join("\r\n", allSelectedGroups));
        }

        private void mnuUnselectSameDir_Click(object sender, RoutedEventArgs e)
        {
            FileItem selectedFile = GetSelectedFile();
            if (selectedFile != null)
            {
                _files.SelectSameDir(selectedFile, false);
            }
        }

        private void mnuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            FileItem selectedFile = GetSelectedFile();
            if (selectedFile != null)
                ShellApi.ShellExecute((IntPtr)0, "open", selectedFile.GetFullPath(), null, selectedFile.DirName, 5);
        }

        private void mnuOpenDir_Click(object sender, RoutedEventArgs e)
        {
            FileItem selectedFile = GetSelectedFile();
            if (selectedFile != null)
                ShellApi.ShellExecute((IntPtr)0, "open", "explorer.exe", string.Format("/e,\"{0}\"", selectedFile.DirName), null, 5);
        }

        private void mnuCompareDir_Click(object sender, RoutedEventArgs e)
        {
            FileItem file = GetSelectedFile();
            if (file != null)
            {
                string[] dirs = _files.Where(f => f.GroupIndex == file.GroupIndex)
                    .Select(f => f.DirName)
                    .Distinct().ToArray();
                var compareDirDialog = new CompareFileDialog();
                compareDirDialog.SetDirectories(dirs);
                compareDirDialog.ShowDialog();
            }
        }

        private bool DeleteFile(FileItem file)
        {
            var sfo = new ShellApi.SHFILEOPSTRUCT()
            {
                wFunc = ShellApi.FileFuncFlags.FO_DELETE,
                fFlags = ShellApi.FILEOP_FLAGS.FOF_ALLOWUNDO | ShellApi.FILEOP_FLAGS.FOF_NOCONFIRMATION,
                pFrom = file.GetFullPath() + char.MinValue + char.MinValue
            };
            return ShellApi.SHFileOperation(ref sfo) == 0;
        }

        public void NotifyFound(FileItem[] files)
        {
            foreach (FileItem fileItem in files)
                _files.Add(fileItem);
        }

        private bool CleanDir(string dirName)
        {
            bool flag = false;
            FileInfo[] files = new DirectoryInfo(dirName).GetFiles();
            if (files.Length == 0)
                flag = true;
            else if (files.Length == 1 && files[0].Name == "imgfind.hash.xml")
            {
                files[0].Delete();
                flag = true;
            }
            if (flag)
            {
                try
                {
                    Directory.Delete(dirName, false);
                    return true;
                }
                catch (Exception ex)
                {
                }
            }
            return false;
        }

        private string FormatSize(long size)
        {
            if (size >= 1048576L)
                return (size / 1024.0 / 1024.0).ToString("F2") + " MB";
            if (size >= 1024L)
                return (size / 1024.0).ToString("F2") + " KB";
            return size.ToString() + "B";
        }

        public void EnterState(State state)
        {
            _state = state;
            switch (state)
            {
                case State.Stopped:
                    EnableButtons(true, false);
                    break;
                case State.Working:
                    EnableButtons(false, true);
                    break;
            }
            if (_state != State.Stopped)
                return;
            sbiText.Content = "";
            progress.Show(false);
            _task = null;
        }

        private void EnableButtons(bool canStart, bool canStop)
        {
            btnStart.IsEnabled = canStart;
            btnStop.IsEnabled = canStop;
        }

        public void ReportMessage(string msg)
        {
            sbiText.Content = msg;
        }

        public void ReportException(Exception exp)
        {
            MessageBox.Show(string.Format("Message={0}\r\nStack Trac={1}", exp.Message, exp.StackTrace));
            exp.LogToFile("{base}/error.log");
        }

        public void ReportPercentage(int percentage)
        {
            progress.Show(true);
            progress.Value = percentage;
        }
    }

    public class RowBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var groupIndex = System.Convert.ToInt32(value);
            switch (groupIndex % 3)
            // switch (((FileItem)((FrameworkElement)value).DataContext).GroupIndex % 3)
            {
                case 0:
                    return Brushes.LightGreen;
                case 1:
                    return Brushes.LightYellow;
                default:
                    return Brushes.LightPink;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FinderTask
    {
        private static readonly string[] _extensions = new string[]
        {
      ".jpg",
      ".jpeg",
      ".png",
      ".bmp",
      ".gif",
      ".avi",
      ".mp4",
      ".mpg",
      ".wmv",
      ".flv",
      ".mkv",
      ".ssa",
      ".srt",
        };
        private readonly IImageFinderUI _win;
        private readonly BackgroundWorker _worker;
        private readonly string[] _dirs;
        private int _totalFiles;
        private int _processFiles;
        private List<FileItem> _files;
        private int _groupIndex;

        public FinderTask(IImageFinderUI win, string[] dirs)
        {
            _win = win;
            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _dirs = dirs;
            _worker.DoWork += worker_DoWork;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            _worker.ProgressChanged += worker_ProgressChanged;
        }

        internal static FileInfo[] SafeFiles(DirectoryInfo dir)
        {
            try
            {
                return dir.GetFiles();
            }
            catch(Exception exp)
            {
                // TODO Log exception
                Console.WriteLine(exp.Message);
                return new FileInfo[0];
            }
        }

        internal static DirectoryInfo[] SafeDirs(DirectoryInfo dir)
        {
            try
            {
                return dir.GetDirectories();
            }
            catch(Exception exp)
            {
                // TODO Log exception
                Console.WriteLine(exp.Message);
                return new DirectoryInfo[0];
            }
        }
        private FileInfo[] GetFiles(DirectoryInfo dir)
        {
            List<FileInfo> list = new List<FileInfo>();
            foreach (FileInfo fileInfo in SafeFiles(dir))
            {
                string str = (fileInfo.Extension ?? "").ToLowerInvariant();
                if (_extensions.Contains(str))
                    list.Add(fileInfo);
            }
            return list.ToArray();
        }

        public void Start()
        {
            _worker.RunWorkerAsync();
            Notify(new ChangeStateArgs(State.Working));
        }

        private void Notify(NotifyArgs args)
        {
            _worker.ReportProgress(0, args);
        }

        public void Stop()
        {
            _worker.CancelAsync();
            Notify(new ChangeStateArgs(State.Stopped));
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _totalFiles = 0;
                ProcessDir(CountFiles);
                _processFiles = 0;
                ProcessDir(UpdateIndex);
                _processFiles = 0;
                _files = new List<FileItem>();
                Notify(new StatusEventArgs("登记所有文件...", new object[0]));
                ProcessDir(CollectFiles);
                _groupIndex = 0;
                Notify(new StatusEventArgs("按Hash分组...", new object[0]));
                FindDuplidateByHash();
                Notify(new StatusEventArgs("按Hash2分组...", new object[0]));
                FindDuplidateByHash2();
                _files.Clear();
            }
            catch (Exception ex)
            {
                Notify(new ExceptionArgs(ex));
            }
            Notify(new ChangeStateArgs(State.Stopped));
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var notifyArgs = e.UserState as NotifyArgs;
            if (notifyArgs == null)
                return;
            notifyArgs.Apply(_win);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void ProcessDir(Action<DirectoryInfo> handler)
        {
            foreach (string path in _dirs)
                ProcessDir(new DirectoryInfo(path), handler);
        }

        private void ProcessDir(DirectoryInfo di, Action<DirectoryInfo> handler)
        {
            handler(di);
            if (_worker.CancellationPending)
                return;
            foreach (DirectoryInfo di1 in SafeDirs(di))
            {
                if (_worker.CancellationPending)
                    break;
                ProcessDir(di1, handler);
            }
        }

        private void CountFiles(DirectoryInfo di)
        {
            try
            {
                Notify(new StatusEventArgs("搜索目录 {0} ...", new object[] { di.FullName }));
                _totalFiles += GetFiles(di).Length;
            }
            catch(Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        private void UpdateIndex(DirectoryInfo dir)
        {
            try
            {
                Notify(new StatusEventArgs("重建目录索引 {0} ...", new object[] { dir.FullName }));
                FileInfo[] files = GetFiles(dir);
                FileCollection fileCollection = FileCollection.Load(dir);
                foreach (FileInfo fi in files)
                {
                    FileItem fileItem1 = fileCollection.GetItem(fi.Name);
                    if (fileItem1 == null)
                    {
                        FileItem fileItem2 = FileItem.Load(fi);
                        if (fileItem2 != null)
                            fileCollection.AddItem(fileItem2);
                    }
                    else if (fileItem1.IsChanged(fi))
                    {
                        FileItem updateItem = FileItem.Load(fi);
                        if (updateItem != null)
                            fileCollection.UpdateItem(updateItem);
                    }
                }
                foreach (FileItem fileItem in fileCollection.Where(it => FileNotExist(files, it)).ToArray())
                    fileCollection.RemoveItem(fileItem);
                if (fileCollection.IsChanged)
                    fileCollection.Save(dir);
                _processFiles += files.Length;
                Notify(new ProgressEventArgs(_processFiles, _totalFiles));
            }
            catch(Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        private bool FileNotExist(FileInfo[] files, FileItem fileItem)
        {
            return files.FirstOrDefault(f => f.Name.Equals(fileItem.Name, StringComparison.InvariantCultureIgnoreCase)) == null;
        }

        private void CollectFiles(DirectoryInfo dir)
        {
            FileCollection fileCollection = FileCollection.Load(dir);
            foreach (FileItem fileItem in fileCollection)
            {
                fileItem.Dir = dir;
                _files.Add(fileItem);
            }
            _processFiles += fileCollection.Count;
            Notify(new ProgressEventArgs(_processFiles, _totalFiles));
        }

        private void FindDuplidateByHash()
        {
            foreach (IEnumerable<FileItem> source in _files.GroupBy(f => f.Hash).Where(g => g.Count() > 1))
            {
                FileItem[] files = source.ToArray();
                foreach (FileItem fileItem in files)
                {
                    fileItem.GroupIndex = _groupIndex;
                    _files.Remove(fileItem);
                }
                Notify(new FoundGroupArgs(files));
                ++_groupIndex;
            }
        }

        private void FindDuplidateByHash2()
        {
            foreach (var source in _files.Where(f => f.Hash2 != null)
                .GroupBy(f => f.Hash2)
                .Where(g => g.Count() > 1))
            {
                var files = source.ToArray();
                foreach (FileItem fileItem in files)
                {
                    fileItem.GroupIndex = _groupIndex;
                    _files.Remove(fileItem);
                }
                Notify(new FoundGroupArgs(files));
                ++_groupIndex;
            }
        }
    }

}
