namespace Shuhari.WinTools.Core.Features.ImageFinder.Args
{
    public class StatusEventArgs : NotifyArgs
    {
        private readonly string _msg;

        public StatusEventArgs(string format, params object[] args)
        {
            _msg = string.Format(format, args);
        }

        public override void Apply(IImageFinderUI win)
        {
            win.ReportMessage(_msg);
        }
    }

}
