using System.ComponentModel;

namespace Shuhari.WinTools.Core.Features.SysOptimize
{
    public class OptimizeTaskItem : INotifyPropertyChanged
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
            set
            {
                if (_imagePath != value)
                {
                    _imagePath = value;
                    NotifyProperty("ImagePath");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyProperty(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
