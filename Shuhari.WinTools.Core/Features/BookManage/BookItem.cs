using System.ComponentModel;

namespace Shuhari.WinTools.Core.Features.BookManage
{
    public class BookItem : INotifyPropertyChanged
    {
        private bool _selected;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Selected"));
                }
            }
        }

        public string OldName { get; set; }

        public string NewName { get; set; }
    }
}
