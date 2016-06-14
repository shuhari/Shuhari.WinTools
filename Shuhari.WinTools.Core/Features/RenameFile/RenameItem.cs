using Shuhari.Library.Common.ComponentModel;

namespace Shuhari.WinTools.Core.Features.RenameFile
{
    public class RenameItem : Observable
    {
        public RenameItem(string currentName, string newName, string dirName)
        {
            this.CurrentName = currentName;
            this.NewName = newName;
            this.DirectoryName = dirName;

            _selected = true;
        }

        private bool _selected;

        public string CurrentName { get; private set; }

        public string NewName { get; private set; }

        public string DirectoryName { get; private set; }

        public bool Selected
        {
            get { return _selected; }
            set { SetProperty(nameof(Selected), ref _selected, value); }
        }
    }
}
