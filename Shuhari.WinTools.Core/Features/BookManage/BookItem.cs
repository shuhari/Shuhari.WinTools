using Shuhari.Library.Common.ComponentModel;

namespace Shuhari.WinTools.Core.Features.BookManage
{
    public class BookItem : Observable
    {
        private bool _selected;

        public bool Selected
        {
            get { return _selected; }
            set { SetProperty(nameof(Selected), ref _selected, value); }
        }

        public string OldName { get; set; }

        public string NewName { get; set; }
    }
}
