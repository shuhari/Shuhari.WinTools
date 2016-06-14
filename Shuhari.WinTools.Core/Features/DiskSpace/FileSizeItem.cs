using System.Collections;
using System.IO;

namespace Shuhari.WinTools.Core.Features.DiskSpace
{
    public class FileSizeItem : FileSizeItemBase
    {
        public FileSizeItem(FileSizeContext context)
            : base(context)
        {

        }

        protected override IEnumerable GetChildrenCore()
        {
            return null;
        }

        protected override bool HasChildrenCore()
        {
            return false;
        }

        public override void Load(FileSystemInfo fsi)
        {
            var info = (FileInfo)fsi;

            this.Name = info.Name;
            this.Size = info.Length;
            _context.TotalSize += info.Length;
        }

        public override void ResolveSize()
        {
        }
    }
}
