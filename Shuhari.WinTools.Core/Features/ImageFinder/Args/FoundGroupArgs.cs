namespace Shuhari.WinTools.Core.Features.ImageFinder.Args
{
    public class FoundGroupArgs : NotifyArgs
    {
        private FileItem[] _files;

        public FoundGroupArgs(FileItem[] files)
        {
            _files = files;
        }

        public override void Apply(IImageFinderUI win)
        {
            win.NotifyFound(_files);
        }
    }
}

