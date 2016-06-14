using System;
using System.IO;
using Shuhari.Library.Common.ComponentModel;

namespace Shuhari.WinTools.Core.Features.BookManage
{
    public class TrimItem : Observable
    {
        public TrimItem(FileInfo file)
        {
            this.File = file;
            this.Selected = true;
        }

        public FileInfo File { get; private set; }

        public string OldName
        {
            get { return File.Name; }
        }

        public string DirName
        {
            get { return File.DirectoryName; }
        }

        public string NewName { get; set; }

        private bool _selected;
        private string _result;

        public bool Selected
        {
            get { return _selected; }
            set { SetProperty(nameof(Selected), ref _selected, value); }
        }

        public string Result
        {
            get { return _result; }
            set { SetProperty(nameof(Result), ref _result, value); }
        }

        public void Apply()
        {
            try
            {
                var newFullName = Path.Combine(DirName, NewName);
                File.MoveTo(newFullName);
                Result = "Success";
            }
            catch (Exception exp)
            {
                Result = exp.Message;
            }
        }
    }
}
