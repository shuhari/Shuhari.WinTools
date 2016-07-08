using System;

namespace Shuhari.WinTools.Core.Features.ImageFinder.Args
{
    public class ExceptionArgs : NotifyArgs
    {
        private readonly Exception _exp;

        public ExceptionArgs(Exception exp)
        {
            _exp = exp;
        }

        public override void Apply(IImageFinderUI win)
        {
            win.ReportException(_exp);
        }
    }

}
