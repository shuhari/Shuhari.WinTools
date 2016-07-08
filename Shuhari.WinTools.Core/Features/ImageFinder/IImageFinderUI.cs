using System;

namespace Shuhari.WinTools.Core.Features.ImageFinder
{
    public interface IImageFinderUI
    {
        void EnterState(State state);

        void ReportException(Exception exp);

        void NotifyFound(FileItem[] files);

        void ReportPercentage(int percentage);

        void ReportMessage(string msg);
    }
}
