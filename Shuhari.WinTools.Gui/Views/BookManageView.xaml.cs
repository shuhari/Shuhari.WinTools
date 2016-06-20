using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Shuhari.Library.IO.Compression;
using Shuhari.Library.Utils;
using Shuhari.Library.Windows;
using BookManageFeature = Shuhari.WinTools.Core.Features.BookManage.Feature;

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

        private void BookManageView_Loaded(object sender, RoutedEventArgs e)
        {
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

        private void btnBrowseDownloadDir_Click(object sender, RoutedEventArgs e)
        {
            var path = this.BrowseForFolder();
            if (path.IsNotBlank())
                txtDownloadDir.Text = path;
        }

        private void btnProcessDownload_Click(object sender, RoutedEventArgs e)
        {
            var path = txtDownloadDir.Text.Trim();
            if (path.IsBlank() || !Directory.Exists(path))
            {
                MessageBox.Show("请选择目录");
                return;
            }

            var processWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            processWorker.DoWork += ProcessWorker_DoWork;
            processWorker.RunWorkerCompleted += ProcessWorker_RunWorkerCompleted;
            processWorker.ProgressChanged += ProcessWorker_ProgressChanged;
            btnProcessDownload.IsEnabled = false;
            processWorker.RunWorkerAsync(path);
        }

        private void ProcessWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void ProcessWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnProcessDownload.IsEnabled = true;
        }

        private void ProcessWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var feature = (BookManageFeature)Feature;

            try
            {
                var path = (string)e.Argument;
                var extensions = new string[] { ".pdf", ".epub", ".mobi", ".azw4", "azw3" };
                // 解压rar文件
                // 如果解压成功则删除原rar文件
                var cmd = new RarCommand().SetOverwriteMode(RarOverwriteMode.Overwrite);
                foreach (var rarFile in new DirectoryInfo(path).GetFiles("*.rar"))
                {
                    cmd.Decompress(rarFile.FullName, path).Exec();
                    bool extractOk = extensions.Any(ext => File.Exists(Path.ChangeExtension(rarFile.FullName, ext)));
                    if (extractOk)
                        rarFile.Delete();
                }

                // 删除FoxEbook.net.txt
                var txtPath = Path.Combine(path, "FoxEbook.net.txt");
                if (File.Exists(txtPath))
                    File.Delete(txtPath);

                // 去掉文件名中的后缀
                foreach (var file in new DirectoryInfo(path).GetFiles())
                {
                    var newName = feature.TrimFileName(file.Name);
                    if (newName != file.Name)
                    {
                        var newPath = Path.Combine(file.DirectoryName, newName);
                        file.MoveTo(newPath);
                    }
                }

                // 删除不需要的重复文件
                var allToDelete = new List<FileInfo>();
                foreach (var group in new DirectoryInfo(path).GetFiles()
                    .GroupBy(f => Path.GetFileNameWithoutExtension(f.Name)))
                {
                    allToDelete.AddRange(feature.GetFilesToDelete(group));
                }
                foreach (var toDelete in allToDelete)
                {
                    toDelete.Delete();
                }
            }
            catch (Exception exp)
            {
                ((App)Application.Current).LogException(exp);
            }
        }
    }
}
