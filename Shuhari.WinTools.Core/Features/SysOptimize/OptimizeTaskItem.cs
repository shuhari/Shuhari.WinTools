using Shuhari.Library.Common.ComponentModel;

namespace Shuhari.WinTools.Core.Features.SysOptimize
{
    public class OptimizeTaskItem : Observable
    {
        public OptimizeTaskItem(OptimizeTask task)
        {
            this.Task = task;
        }

        public OptimizeTask Task { get; private set; }

        private string _imagePath;

        public string DisplayName
        {
            get { return Task.DisplayName; }
        }

        public string ImagePath
        {
            get { return _imagePath; }
            set { SetProperty(nameof(ImagePath), ref _imagePath, value); }
        }
    }
}
