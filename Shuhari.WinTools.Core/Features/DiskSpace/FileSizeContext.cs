using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuhari.WinTools.Core.Features.DiskSpace
{
    public class FileSizeContext
    {
        public FileSizeContext(long diskSize)
        {
            this.TotalSize = 0;
            this.ReadedSize = 0;
            this.DiskSize = diskSize;
        }

        public long TotalSize { get; internal set; }

        public long ReadedSize { get; set; }

        public long DiskSize { get; internal set; }

        public int ReadedPercentage
        {
            get
            {
                if (DiskSize > 0)
                    return (int)((double)ReadedSize / DiskSize * 100);
                else
                    return 0;
            }
        }
    }
}
