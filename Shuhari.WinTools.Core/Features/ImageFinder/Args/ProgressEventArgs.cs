namespace Shuhari.WinTools.Core.Features.ImageFinder.Args
{
    public class ProgressEventArgs : NotifyArgs
    {
        private int _percentage;

        public ProgressEventArgs(int current, int total)
        {
            if (total <= 0)
                return;
            _percentage = (int)(current * 100L / total);
        }

        public override void Apply(IImageFinderUI win)
        {
            win.ReportPercentage(_percentage);
        }
    }

}
