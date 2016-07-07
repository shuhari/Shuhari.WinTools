using System;
using System.Collections;
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
            this.EnterState(State.Stopped);
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            this.txtDir1.Text = configuration.AppSettings.Settings["imageFinder.dir1"].Value;
            this.txtDir2.Text = configuration.AppSettings.Settings["imageFinder.dir2"].Value;
            this.txtDir3.Text = configuration.AppSettings.Settings["imageFinder.dir3"].Value;
        }

        private State _state;
        private FinderTask _task;
        private ObservableCollection<FileItem> _files;

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!this.CanEnterState(State.Working))
                return;
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings["imageFinder.dir1"].Value = this.txtDir1.Text.Trim();
            configuration.AppSettings.Settings["imageFinder.dir2"].Value = this.txtDir2.Text.Trim();
            configuration.AppSettings.Settings["imageFinder.dir3"].Value = this.txtDir3.Text.Trim();
            configuration.Save();
            List<string> dirs = new List<string>();
            this.AddValidDir(dirs, this.txtDir1);
            this.AddValidDir(dirs, this.txtDir2);
            this.AddValidDir(dirs, this.txtDir3);
            if (dirs.Count > 0)
            {
                this._files = new ObservableCollection<FileItem>();
                this.fileList.ItemsSource = (IEnumerable)this._files;
                this._task = new FinderTask(this, dirs.ToArray());
                this._task.Start();
            }
            else
            {
                int num = (int)MessageBox.Show("请输入目录");
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (!this.CanEnterState(State.Stopped) || this._task == null)
                return;
            this._task.Stop();
        }

        private void chkShowPreview_Click(object sender, RoutedEventArgs e)
        {
            Grid grid = this.previewPanel;
            bool? isChecked = this.chkShowPreview.IsChecked;
            int num = (!isChecked.GetValueOrDefault() ? 0 : (isChecked.HasValue ? 1 : 0)) != 0 ? 0 : 2;
            grid.Visibility = (Visibility)num;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            FileItem[] fileItemArray = Enumerable.ToArray<FileItem>(Enumerable.Where<FileItem>((IEnumerable<FileItem>)this._files, (Func<FileItem, bool>)(f => f.Selected)));
            if (fileItemArray.Length == 0)
                return;
            int num1 = 0;
            int num2 = 0;
            int num3 = 0;
            long size = 0;
            for (int index = fileItemArray.Length - 1; index >= 0; --index)
            {
                FileItem file = fileItemArray[index];
                if (this.DeleteFile(file))
                {
                    this._files.Remove(file);
                    ++num1;
                    size += file.Size;
                    if (this.CleanDir(file.DirName))
                        ++num3;
                }
                else
                    ++num2;
                if (index % 100 == 0)
                    GC.Collect();
            }
            foreach (FileItem fileItem in this.GetOrphans())
                this._files.Remove(fileItem);
            this.sbiText.Content = (object)string.Format("删除成功: {0}, 失败: {1}, 释放空间: {2}, 清理目录数={3}", (object)num1, (object)num2, (object)this.FormatSize(size), (object)num3);
        }

        private void fileList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Space)
                return;
            FileItem selectedFile = this.GetSelectedFile();
            if (selectedFile != null)
                selectedFile.Selected = !selectedFile.Selected;
            this.UpdateSelectionMsg();
        }

        internal bool CanEnterState(State destState)
        {
            switch (destState)
            {
                case State.Stopped:
                    return this._state == State.Working;
                case State.Working:
                    return this._state == State.Stopped;
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
            FileItem[] sameGroup = this.GetSameGroup((FileItem)e.AddedItems[0]);
            this.Preview(this.preview0, sameGroup, 0);
            this.Preview(this.preview1, sameGroup, 1);
            this.Preview(this.preview2, sameGroup, 2);
            this.Preview(this.preview3, sameGroup, 3);
            this.Preview(this.preview4, sameGroup, 4);
        }

        private void Preview(Image img, FileItem[] files, int index)
        {
            BitmapImage bitmapImage1 = img.Source as BitmapImage;
            if (bitmapImage1 != null)
            {
                Stream streamSource = bitmapImage1.StreamSource;
                img.Source = (ImageSource)null;
                if (streamSource != null)
                {
                    img.Source = (ImageSource)null;
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
                bitmapImage2.StreamSource = (Stream)new MemoryStream(buffer);
                bitmapImage2.EndInit();
                img.Source = (ImageSource)bitmapImage2;
            }
            catch (Exception ex)
            {
            }
        }

        private FileItem[] GetSameGroup(FileItem fi)
        {
            List<FileItem> list = new List<FileItem>();
            int num = this._files.IndexOf(fi);
            if (num >= 0)
            {
                list.Add(fi);
                for (int index = num - 1; index >= 0; --index)
                {
                    FileItem fileItem = this._files[index];
                    if (fileItem.GroupIndex == fi.GroupIndex)
                        list.Insert(0, fileItem);
                    else
                        break;
                }
                for (int index = num + 1; index < this._files.Count; ++index)
                {
                    FileItem fileItem = this._files[index];
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
            FileItem selectedFile = this.GetSelectedFile();
            if (selectedFile != null)
            {
                foreach (FileItem fileItem in (Collection<FileItem>)this._files)
                {
                    if (fileItem.DirName == selectedFile.DirName)
                        fileItem.Selected = true;
                }
            }
            this.UpdateSelectionMsg();
        }

        private FileItem GetSelectedFile()
        {
            return this.fileList.SelectedItem as FileItem;
        }

        private void UpdateSelectionMsg()
        {
            FileItem[] fileItemArray = Enumerable.ToArray<FileItem>(Enumerable.Where<FileItem>((IEnumerable<FileItem>)this._files, (Func<FileItem, bool>)(f => f.Selected)));
            int[] numArray = Enumerable.ToArray<int>(Enumerable.Distinct<int>(Enumerable.Select<FileItem, int>((IEnumerable<FileItem>)fileItemArray, (Func<FileItem, int>)(f => f.GroupIndex))));
            this.sbiText.Content = (object)string.Format("共选中 {0} 个文件", (object)fileItemArray.Length);
            List<int> list = new List<int>();
            foreach (int num in numArray)
            {
                int groupIndex = num;
                if (Enumerable.ToArray<FileItem>(Enumerable.Where<FileItem>((IEnumerable<FileItem>)fileItemArray, (Func<FileItem, bool>)(f => f.GroupIndex == groupIndex))).Length == Enumerable.ToArray<FileItem>(Enumerable.Where<FileItem>((IEnumerable<FileItem>)this._files, (Func<FileItem, bool>)(f => f.GroupIndex == groupIndex))).Length)
                    list.Add(groupIndex);
            }
            if (list.Count <= 0)
                return;
            int num1 = (int)MessageBox.Show("以下分组文件被全部选中，请谨慎删除！\r\n" + string.Join<int>("\r\n", (IEnumerable<int>)list));
        }

        private void mnuUnselectSameDir_Click(object sender, RoutedEventArgs e)
        {
            FileItem selectedFile = this.GetSelectedFile();
            if (selectedFile != null)
            {
                foreach (FileItem fileItem in (Collection<FileItem>)this._files)
                {
                    if (fileItem.DirName == selectedFile.DirName)
                        fileItem.Selected = false;
                }
            }
            this.UpdateSelectionMsg();
        }

        private void mnuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            FileItem selectedFile = this.GetSelectedFile();
            if (selectedFile == null)
                return;
            ShellExecute((IntPtr)0, "open", selectedFile.GetFullPath(), (string)null, selectedFile.DirName, 5);
        }

        private void mnuOpenDir_Click(object sender, RoutedEventArgs e)
        {
            FileItem selectedFile = this.GetSelectedFile();
            if (selectedFile == null)
                return;
            ShellExecute((IntPtr)0, "open", "explorer.exe", string.Format("/e,\"{0}\"", (object)selectedFile.DirName), (string)null, 5);
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
            FileItem file = this.GetSelectedFile();
            if (file == null)
                return;
            string[] dirs = Enumerable.ToArray<string>(Enumerable.Distinct<string>(Enumerable.Select<FileItem, string>(Enumerable.Where<FileItem>((IEnumerable<FileItem>)this._files, (Func<FileItem, bool>)(f => f.GroupIndex == file.GroupIndex)), (Func<FileItem, string>)(f => f.DirName))));
            CompareFileDialog compareDirDialog = new CompareFileDialog();
            compareDirDialog.SetDirectories(dirs);
            compareDirDialog.ShowDialog();
        }

        private bool DeleteFile(FileItem file)
        {
            var sfo = new SHFILEOPSTRUCT()
            {
                wFunc = 3,
                fFlags = (short)80,
                pFrom = file.GetFullPath() + (object)char.MinValue + (string)(object)char.MinValue
            };
            return SHFileOperation(ref sfo) == 0;
        }

        internal void NotifyFound(FileItem[] files)
        {
            foreach (FileItem fileItem in files)
                this._files.Add(fileItem);
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
            return Enumerable.ToArray<FileItem>(Enumerable.SelectMany<IGrouping<int, FileItem>, FileItem>(Enumerable.Where<IGrouping<int, FileItem>>(Enumerable.GroupBy<FileItem, int>((IEnumerable<FileItem>)this._files, (Func<FileItem, int>)(f => f.GroupIndex)), (Func<IGrouping<int, FileItem>, bool>)(g => Enumerable.Count<FileItem>((IEnumerable<FileItem>)g) == 1)), (Func<IGrouping<int, FileItem>, IEnumerable<FileItem>>)(g => (IEnumerable<FileItem>)g)));
        }

        private string FormatSize(long size)
        {
            if (size >= 1048576L)
                return ((double)size / 1024.0 / 1024.0).ToString("F2") + " MB";
            if (size >= 1024L)
                return ((double)size / 1024.0).ToString("F2") + " KB";
            return size.ToString() + "B";
        }

        internal void EnterState(State state)
        {
            this._state = state;
            switch (state)
            {
                case State.Stopped:
                    this.EnableButtons(true, false);
                    break;
                case State.Working:
                    this.EnableButtons(false, true);
                    break;
            }
            if (this._state != State.Stopped)
                return;
            this.sbiText.Content = (object)"";
            this.ShowProgressBar(false);
            this._task = (FinderTask)null;
        }

        private void ShowProgressBar(bool show)
        {
            this.progress.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void EnableButtons(bool canStart, bool canStop)
        {
            this.btnStart.IsEnabled = canStart;
            this.btnStop.IsEnabled = canStop;
        }

        internal void ReportMessage(string msg)
        {
            this.sbiText.Content = (object)msg;
        }

        internal void ReportException(Exception exp)
        {
            int num = (int)MessageBox.Show(string.Format("Message={0}\r\nStack Trac={1}", (object)exp.Message, (object)exp.StackTrace));
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (; exp != null; exp = exp.InnerException)
                    stringBuilder.AppendFormat("Message={0}\r\nStack Trace={1}\r\n\r\n", (object)exp.Message, (object)exp.StackTrace);
                File.AppendAllText("error.log", stringBuilder.ToString());
            }
            catch (Exception ex)
            {
            }
        }

        internal void ReportPercentage(int percentage)
        {
            this.ShowProgressBar(true);
            this.progress.Value = (double)percentage;
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
            this._win = win;
            this._worker = new BackgroundWorker();
            this._worker.WorkerReportsProgress = true;
            this._worker.WorkerSupportsCancellation = true;
            this._dirs = dirs;
            this._worker.DoWork += new DoWorkEventHandler(this.worker_DoWork);
            this._worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.worker_RunWorkerCompleted);
            this._worker.ProgressChanged += new ProgressChangedEventHandler(this.worker_ProgressChanged);
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
                if (Enumerable.Contains<string>((IEnumerable<string>)FinderTask._extensions, str))
                    list.Add(fileInfo);
            }
            return list.ToArray();
        }

        public void Start()
        {
            this._worker.RunWorkerAsync();
            this.Notify((NotifyArgs)new ChangeStateArgs(State.Working));
        }

        private void Notify(NotifyArgs args)
        {
            this._worker.ReportProgress(0, (object)args);
        }

        public void Stop()
        {
            this._worker.CancelAsync();
            this.Notify((NotifyArgs)new ChangeStateArgs(State.Stopped));
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this._totalFiles = 0;
                this.ProcessDir(new Action<DirectoryInfo>(this.CountFiles));
                this._processFiles = 0;
                this.ProcessDir(new Action<DirectoryInfo>(this.UpdateIndex));
                this._processFiles = 0;
                this._files = new List<FileItem>();
                this.Notify((NotifyArgs)new StatusEventArgs("登记所有文件...", new object[0]));
                this.ProcessDir(new Action<DirectoryInfo>(this.CollectFiles));
                this._groupIndex = 0;
                this.Notify((NotifyArgs)new StatusEventArgs("按Hash分组...", new object[0]));
                this.FindDuplidateByHash();
                this.Notify((NotifyArgs)new StatusEventArgs("按Hash2分组...", new object[0]));
                this.FindDuplidateByHash2();
                this._files.Clear();
            }
            catch (Exception ex)
            {
                this.Notify((NotifyArgs)new ExceptionArgs(ex));
            }
            this.Notify((NotifyArgs)new ChangeStateArgs(State.Stopped));
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            NotifyArgs notifyArgs = e.UserState as NotifyArgs;
            if (notifyArgs == null)
                return;
            notifyArgs.Apply(this._win);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void ProcessDir(Action<DirectoryInfo> handler)
        {
            foreach (string path in this._dirs)
                this.ProcessDir(new DirectoryInfo(path), handler);
        }

        private void ProcessDir(DirectoryInfo di, Action<DirectoryInfo> handler)
        {
            handler(di);
            if (this._worker.CancellationPending)
                return;
            foreach (DirectoryInfo di1 in FinderTask.SafeDirs(di))
            {
                if (this._worker.CancellationPending)
                    break;
                this.ProcessDir(di1, handler);
            }
        }

        private void CountFiles(DirectoryInfo di)
        {
            try
            {
                this.Notify((NotifyArgs)new StatusEventArgs("搜索目录 {0} ...", new object[1]
                {
        (object) di.FullName
                }));
                this._totalFiles += this.GetFiles(di).Length;
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
                this.Notify((NotifyArgs)new StatusEventArgs("重建目录索引 {0} ...", new object[1]
                {
        (object) dir.FullName
                }));
                FileInfo[] files = this.GetFiles(dir);
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
                foreach (FileItem fileItem in Enumerable.ToArray<FileItem>(Enumerable.Where<FileItem>((IEnumerable<FileItem>)fileCollection, (Func<FileItem, bool>)(it => this.FileNotExist(files, it)))))
                    fileCollection.RemoveItem(fileItem);
                if (fileCollection.IsChanged)
                    fileCollection.Save(dir);
                this._processFiles += files.Length;
                this.Notify((NotifyArgs)new ProgressEventArgs(this._processFiles, this._totalFiles));
            }
            catch(Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        private bool FileNotExist(FileInfo[] files, FileItem fileItem)
        {
            return Enumerable.FirstOrDefault<FileInfo>((IEnumerable<FileInfo>)files, (Func<FileInfo, bool>)(f => f.Name.Equals(fileItem.Name, StringComparison.InvariantCultureIgnoreCase))) == null;
        }

        private void CollectFiles(DirectoryInfo dir)
        {
            FileCollection fileCollection = FileCollection.Load(dir);
            foreach (FileItem fileItem in (Collection<FileItem>)fileCollection)
            {
                fileItem.Dir = dir;
                this._files.Add(fileItem);
            }
            this._processFiles += fileCollection.Count;
            this.Notify((NotifyArgs)new ProgressEventArgs(this._processFiles, this._totalFiles));
        }

        private void FindDuplidateByHash()
        {
            foreach (IEnumerable<FileItem> source in Enumerable.Where<IGrouping<string, FileItem>>(Enumerable.GroupBy<FileItem, string>((IEnumerable<FileItem>)this._files, (Func<FileItem, string>)(f => f.Hash)), (Func<IGrouping<string, FileItem>, bool>)(g => Enumerable.Count<FileItem>((IEnumerable<FileItem>)g) > 1)))
            {
                FileItem[] files = Enumerable.ToArray<FileItem>(source);
                foreach (FileItem fileItem in files)
                {
                    fileItem.GroupIndex = this._groupIndex;
                    this._files.Remove(fileItem);
                }
                this.Notify((NotifyArgs)new FoundGroupArgs(files));
                ++this._groupIndex;
            }
        }

        private void FindDuplidateByHash2()
        {
            foreach (IEnumerable<FileItem> source in Enumerable.Where<IGrouping<string, FileItem>>(Enumerable.GroupBy<FileItem, string>(Enumerable.Where<FileItem>((IEnumerable<FileItem>)this._files, (Func<FileItem, bool>)(f => f.Hash2 != null)), (Func<FileItem, string>)(f => f.Hash2)), (Func<IGrouping<string, FileItem>, bool>)(g => Enumerable.Count<FileItem>((IEnumerable<FileItem>)g) > 1)))
            {
                FileItem[] files = Enumerable.ToArray<FileItem>(source);
                foreach (FileItem fileItem in files)
                {
                    fileItem.GroupIndex = this._groupIndex;
                    this._files.Remove(fileItem);
                }
                this.Notify((NotifyArgs)new FoundGroupArgs(files));
                ++this._groupIndex;
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
            this._state = state;
        }

        public override void Apply(ImageFinderView win)
        {
            win.EnterState(this._state);
        }
    }

    internal class StatusEventArgs : NotifyArgs
    {
        private readonly string _msg;

        public StatusEventArgs(string format, params object[] args)
        {
            this._msg = string.Format(format, args);
        }

        public override void Apply(ImageFinderView win)
        {
            win.ReportMessage(this._msg);
        }
    }

    internal class ExceptionArgs : NotifyArgs
    {
        private readonly Exception _exp;

        public ExceptionArgs(Exception exp)
        {
            this._exp = exp;
        }

        public override void Apply(ImageFinderView win)
        {
            win.ReportException(this._exp);
        }
    }

    internal class ProgressEventArgs : NotifyArgs
    {
        private int _percentage;

        public ProgressEventArgs(int current, int total)
        {
            if (total <= 0)
                return;
            this._percentage = (int)((long)current * 100L / (long)total);
        }

        public override void Apply(ImageFinderView win)
        {
            win.ReportPercentage(this._percentage);
        }
    }

    internal class FoundGroupArgs : NotifyArgs
    {
        private FileItem[] _files;

        public FoundGroupArgs(FileItem[] files)
        {
            this._files = files;
        }

        public override void Apply(ImageFinderView win)
        {
            win.NotifyFound(this._files);
        }
    }
}
