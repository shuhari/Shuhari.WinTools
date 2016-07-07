using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Shuhari.WinTools.Core.Features.ImageFinder;

namespace Shuhari.WinTools.Gui.Views
{
    /// <summary>
    /// Interaction logic for ImageFinderView.xaml
    /// </summary>
    public partial class ImageFinderView : FeatureView
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
        private ObservableCollection<FileItem> _files;

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
                _files = new ObservableCollection<FileItem>();
                fileList.ItemsSource = _files;
                _task = new FinderTask(this, dirs.ToArray());
                _task.Start();
            }
            else
            {
                MessageBox.Show("请输入目录");
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (!CanEnterState(State.Stopped) || _task == null)
                return;
            _task.Stop();
        }

        private void chkShowPreview_Click(object sender, RoutedEventArgs e)
        {
            Grid grid = previewPanel;
            bool? isChecked = chkShowPreview.IsChecked;
            int num = (!isChecked.GetValueOrDefault() ? 0 : (isChecked.HasValue ? 1 : 0)) != 0 ? 0 : 2;
            grid.Visibility = (Visibility)num;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            FileItem[] fileItemArray = _files.Where(f => f.Selected)
                .ToArray();
            if (fileItemArray.Length == 0)
                return;

            int num1 = 0;
            int num2 = 0;
            int num3 = 0;
            long size = 0;
            for (int index = fileItemArray.Length - 1; index >= 0; --index)
            {
                FileItem file = fileItemArray[index];
                if (DeleteFile(file))
                {
                    _files.Remove(file);
                    ++num1;
                    size += file.Size;
                    if (CleanDir(file.DirName))
                        ++num3;
                }
                else
                    ++num2;
                if (index % 100 == 0)
                    GC.Collect();
            }
            foreach (FileItem fileItem in GetOrphans())
                _files.Remove(fileItem);
            sbiText.Content = string.Format("删除成功: {0}, 失败: {1}, 释放空间: {2}, 清理目录数={3}", 
                num1, num2, FormatSize(size), num3);
        }

        private void fileList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Space)
                return;
            FileItem selectedFile = GetSelectedFile();
            if (selectedFile != null)
                selectedFile.Selected = !selectedFile.Selected;
            UpdateSelectionMsg();
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
            FileItem[] sameGroup = GetSameGroup((FileItem)e.AddedItems[0]);
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

        private FileItem[] GetSameGroup(FileItem fi)
        {
            List<FileItem> list = new List<FileItem>();
            int num = _files.IndexOf(fi);
            if (num >= 0)
            {
                list.Add(fi);
                for (int index = num - 1; index >= 0; --index)
                {
                    FileItem fileItem = _files[index];
                    if (fileItem.GroupIndex == fi.GroupIndex)
                        list.Insert(0, fileItem);
                    else
                        break;
                }
                for (int index = num + 1; index < _files.Count; ++index)
                {
                    FileItem fileItem = _files[index];
                    if (fileItem.GroupIndex == fi.GroupIndex)
                        list.Add(fileItem);
                    else
                        break;
                }
            }
            return list.ToArray();
        }

        private void mnuSelectSameDir_Click(object sender, RoutedEventArgs e)
        {
            FileItem selectedFile = GetSelectedFile();
            if (selectedFile != null)
            {
                foreach (FileItem fileItem in _files)
                {
                    if (fileItem.DirName == selectedFile.DirName)
                        fileItem.Selected = true;
                }
            }
            UpdateSelectionMsg();
        }

        private FileItem GetSelectedFile()
        {
            return fileList.SelectedItem as FileItem;
        }

        private void UpdateSelectionMsg()
        {
            FileItem[] fileItemArray = _files.Where(f => f.Selected).ToArray();
            int[] numArray = fileItemArray.Select(f => f.GroupIndex).Distinct().ToArray();
            sbiText.Content = string.Format("共选中 {0} 个文件", fileItemArray.Length);
            var list = new List<int>();
            foreach (int num in numArray)
            {
                int groupIndex = num;
                if (fileItemArray.Where(f => f.GroupIndex == groupIndex).ToArray().Length == _files.Where(f => f.GroupIndex == groupIndex).ToArray().Length)
                    list.Add(groupIndex);
            }
            if (list.Count <= 0)
                return;
            MessageBox.Show("以下分组文件被全部选中，请谨慎删除！\r\n" + string.Join<int>("\r\n", list));
        }

        private void mnuUnselectSameDir_Click(object sender, RoutedEventArgs e)
        {
            FileItem selectedFile = GetSelectedFile();
            if (selectedFile != null)
            {
                foreach (FileItem fileItem in _files)
                {
                    if (fileItem.DirName == selectedFile.DirName)
                        fileItem.Selected = false;
                }
            }
            UpdateSelectionMsg();
        }

        private void mnuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            FileItem selectedFile = GetSelectedFile();
            if (selectedFile == null)
                return;
            ShellExecute((IntPtr)0, "open", selectedFile.GetFullPath(), null, selectedFile.DirName, 5);
        }

        private void mnuOpenDir_Click(object sender, RoutedEventArgs e)
        {
            FileItem selectedFile = GetSelectedFile();
            if (selectedFile == null)
                return;
            ShellExecute((IntPtr)0, "open", "explorer.exe", string.Format("/e,\"{0}\"", selectedFile.DirName), null, 5);
        }

        [DllImport("shell32.dll")]
        public static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Auto)]
        public struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public int wFunc;
            public string pFrom;
            public string pTo;
            public short fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        private void mnuCompareDir_Click(object sender, RoutedEventArgs e)
        {
            FileItem file = GetSelectedFile();
            if (file == null)
                return;
            string[] dirs = _files.Where(f => f.GroupIndex == file.GroupIndex)
                .Select(f => f.DirName)
                .Distinct().ToArray();
            var compareDirDialog = new CompareFileDialog();
            compareDirDialog.SetDirectories(dirs);
            compareDirDialog.ShowDialog();
        }

        private bool DeleteFile(FileItem file)
        {
            var sfo = new SHFILEOPSTRUCT()
            {
                wFunc = 3,
                fFlags = 80,
                pFrom = file.GetFullPath() + char.MinValue + char.MinValue
            };
            return SHFileOperation(ref sfo) == 0;
        }

        internal void NotifyFound(FileItem[] files)
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

        private FileItem[] GetOrphans()
        {
            return _files.GroupBy(f => f.GroupIndex)
                .Where(g => g.Count() == 1)
                .SelectMany(g => g)
                .ToArray();
        }

        private string FormatSize(long size)
        {
            if (size >= 1048576L)
                return (size / 1024.0 / 1024.0).ToString("F2") + " MB";
            if (size >= 1024L)
                return (size / 1024.0).ToString("F2") + " KB";
            return size.ToString() + "B";
        }

        internal void EnterState(State state)
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
            ShowProgressBar(false);
            _task = null;
        }

        private void ShowProgressBar(bool show)
        {
            progress.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void EnableButtons(bool canStart, bool canStop)
        {
            btnStart.IsEnabled = canStart;
            btnStop.IsEnabled = canStop;
        }

        internal void ReportMessage(string msg)
        {
            sbiText.Content = msg;
        }

        internal void ReportException(Exception exp)
        {
            MessageBox.Show(string.Format("Message={0}\r\nStack Trac={1}", exp.Message, exp.StackTrace));
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (; exp != null; exp = exp.InnerException)
                    stringBuilder.AppendFormat("Message={0}\r\nStack Trace={1}\r\n\r\n", exp.Message, exp.StackTrace);
                File.AppendAllText("error.log", stringBuilder.ToString());
            }
            catch (Exception ex)
            {
            }
        }

        internal void ReportPercentage(int percentage)
        {
            ShowProgressBar(true);
            progress.Value = percentage;
        }
    }

    public class RowBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (((FileItem)((FrameworkElement)value).DataContext).GroupIndex % 3)
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
        private readonly ImageFinderView _win;
        private readonly BackgroundWorker _worker;
        private readonly string[] _dirs;
        private int _totalFiles;
        private int _processFiles;
        private List<FileItem> _files;
        private int _groupIndex;

        public FinderTask(ImageFinderView win, string[] dirs)
        {
            _win = win;
            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _dirs = dirs;
            _worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            _worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
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
                ProcessDir(new Action<DirectoryInfo>(CountFiles));
                _processFiles = 0;
                ProcessDir(new Action<DirectoryInfo>(UpdateIndex));
                _processFiles = 0;
                _files = new List<FileItem>();
                Notify(new StatusEventArgs("登记所有文件...", new object[0]));
                ProcessDir(new Action<DirectoryInfo>(CollectFiles));
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
            foreach (IEnumerable<FileItem> source in _files.Where(f => f.Hash2 != null).GroupBy(f => f.Hash2).Where(g => g.Count() > 1))
            {
                FileItem[] files = Enumerable.ToArray<FileItem>(source);
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

    public abstract class NotifyArgs : EventArgs
    {
        public abstract void Apply(ImageFinderView win);
    }

    internal class ChangeStateArgs : NotifyArgs
    {
        private readonly State _state;

        public ChangeStateArgs(State state)
        {
            _state = state;
        }

        public override void Apply(ImageFinderView win)
        {
            win.EnterState(_state);
        }
    }

    internal class StatusEventArgs : NotifyArgs
    {
        private readonly string _msg;

        public StatusEventArgs(string format, params object[] args)
        {
            _msg = string.Format(format, args);
        }

        public override void Apply(ImageFinderView win)
        {
            win.ReportMessage(_msg);
        }
    }

    internal class ExceptionArgs : NotifyArgs
    {
        private readonly Exception _exp;

        public ExceptionArgs(Exception exp)
        {
            _exp = exp;
        }

        public override void Apply(ImageFinderView win)
        {
            win.ReportException(_exp);
        }
    }

    internal class ProgressEventArgs : NotifyArgs
    {
        private int _percentage;

        public ProgressEventArgs(int current, int total)
        {
            if (total <= 0)
                return;
            _percentage = (int)(current * 100L / total);
        }

        public override void Apply(ImageFinderView win)
        {
            win.ReportPercentage(_percentage);
        }
    }

    internal class FoundGroupArgs : NotifyArgs
    {
        private FileItem[] _files;

        public FoundGroupArgs(FileItem[] files)
        {
            _files = files;
        }

        public override void Apply(ImageFinderView win)
        {
            win.NotifyFound(_files);
        }
    }
}
