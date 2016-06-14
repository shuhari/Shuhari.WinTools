using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuhari.WinTools.Core.Features.DiskSpace
{
    public class DirectorySizeItem : FileSizeItemBase
    {
        public DirectorySizeItem(FileSizeContext context)
            : base(context)
        {
            SubDirectories = new List<DirectorySizeItem>();
            Files = new List<FileSizeItem>();
        }

        public List<DirectorySizeItem> SubDirectories { get; private set; }
        public List<FileSizeItem> Files { get; private set; }

        protected override IEnumerable GetChildrenCore()
        {
            var children = new List<FileSizeItemBase>();
            children.AddRange(SubDirectories);
            children.AddRange(Files);
            return children;
        }

        protected override bool HasChildrenCore()
        {
            return SubDirectories.Count > 0 || Files.Count > 0;
        }

        public override void Load(FileSystemInfo fsi)
        {
            var info = (DirectoryInfo)fsi;
            this.Name = info.Name;
            this.Size = 0;
        }

        public override void ResolveSize()
        {
            this.Size = SubDirectories.Sum(it => it.Size) + Files.Sum(it => it.Size);
            foreach (var dir in SubDirectories)
            {
                dir.SetPercentage(this.Size, _context.TotalSize);
            }

            foreach (var fsi in Files)
            {
                fsi.SetPercentage(this.Size, _context.TotalSize);
            }
        }
    }
}
