using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Shuhari.WinTools.Core.Features.SysOptimize;

namespace Shuhari.WinTools.Gui.Views
{
    /// <summary>
    /// Interaction logic for SysOptimizeView.xaml
    /// </summary>
    public partial class SysOptimizeView : FeatureView
    {
        public SysOptimizeView()
        {
            InitializeComponent();

            RegisterTasks();
            taskList.ItemsSource = _items;

            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
            };
            _worker.DoWork += worker_DoWork;
            _worker.ProgressChanged += worker_ProgressChanged;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        private BackgroundWorker _worker;
        private List<OptimizeTaskItem> _items;

        const string IMAGE_RUNNING = "/Resources/run-16.png";
        const string IMAGE_SUCCESS = "/Resources/success-16.png";
        const string IMAGE_ERROR = "/Resources/error-16.png";

        private void RegisterTasks()
        {
            _items = new List<OptimizeTaskItem>();

            RegisterTask(new SetTempDirTask(@"C:\Temp"));

            RegisterTask(new SetServiceTask("Browser", ServiceStartType.Manual));
            RegisterTask(new SetServiceTask("CryptSvc", ServiceStartType.Manual));
            RegisterTask(new SetServiceTask("TrkWks", ServiceStartType.Manual));
            RegisterTask(new SetServiceTask("cphs", ServiceStartType.Manual));
            RegisterTask(new SetServiceTask("igfxCUIService1.0.0.0", ServiceStartType.Manual));
            RegisterTask(new SetServiceTask("SharedAccess", ServiceStartType.Manual));
            RegisterTask(new SetServiceTask("IEEtwCollectorService", ServiceStartType.Manual));
            RegisterTask(new SetServiceTask("CscService", ServiceStartType.Manual));
            RegisterTask(new SetServiceTask("RemoteRegistry", ServiceStartType.Manual));
            RegisterTask(new SetServiceTask("wscsvc", ServiceStartType.Manual));
            RegisterTask(new SetServiceTask("ShellHWDetection", ServiceStartType.Manual));
            RegisterTask(new SetServiceTask("SQLWriter", ServiceStartType.Manual));
            RegisterTask(new SetServiceTask("MpsSvc", ServiceStartType.Manual));
            RegisterTask(new SetServiceTask("wuauserv", ServiceStartType.Manual));

            RegisterTask(new SetRemoteDesktopTask(false));
            RegisterTask(new SetSysStartupTask());
            RegisterTask(new SetPowerCfgTask());
            RegisterTask(new SetExplorerTask());
            RegisterTask(new SetAcdseeTask());
            /*
                // TODO: 系统还原无法禁止 new SetSysRestore(),
                // TODO: 性能：视觉效果
             */
        }

        private void RegisterTask(OptimizeTask task)
        {
            var item = new OptimizeTaskItem(task);
            _items.Add(item);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            _worker.RunWorkerAsync();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (var item in _items)
            {
                var task = item.Task;
                ReportTaskStatus(task, OptimizeTaskStatus.Running);
                try
                {
                    bool success = task.Execute();
                    ReportTaskStatus(task, success ? OptimizeTaskStatus.Success : OptimizeTaskStatus.Failure);
                }
                catch (Exception exp)
                {
                    ReportTaskStatus(task, OptimizeTaskStatus.Failure);
                    ((App)Application.Current).LogException(exp);
                }
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnStart.IsEnabled = true;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is OptimizeTaskStatusEventArgs)
            {
                var args = (OptimizeTaskStatusEventArgs)e.UserState;
                var item = _items.FirstOrDefault(it => it.Task == args.Task);
                if (item != null)
                {
                    switch (args.Status)
                    {
                        case OptimizeTaskStatus.Running: item.ImagePath = IMAGE_RUNNING; break;
                        case OptimizeTaskStatus.Success: item.ImagePath = IMAGE_SUCCESS; break;
                        case OptimizeTaskStatus.Failure: item.ImagePath = IMAGE_ERROR; break;
                    }
                }
            }
        }

        private void ReportTaskStatus(OptimizeTask task, OptimizeTaskStatus status)
        {
            _worker.ReportProgress(0, new OptimizeTaskStatusEventArgs(task, status));
        }
    }
}
