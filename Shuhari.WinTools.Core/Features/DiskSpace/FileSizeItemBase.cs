using System;
using System.Collections;
using System.IO;
using Aga.Controls.Tree;

namespace Shuhari.WinTools.Core.Features.DiskSpace
{
    public abstract class FileSizeItemBase : ITreeModel, IComparable
    {
        public FileSizeItemBase(FileSizeContext context)
        {
            _context = context;
        }

        protected FileSizeContext _context;

        public string Name { get; protected set; }

        public long Size { get; protected set; }

        public double PercentageInSibling { get; private set; }

        public double PercentageInAll { get; private set; }

        public string PercentageInSiblingStr
        {
            get { return PercentageInSibling.ToString("F2") + "%"; }
        }

        public string PercentageInAllStr
        {
            get { return PercentageInAll.ToString("F2") + "%"; }
        }

        internal void SetPercentage(long siblingSize, long totalSize)
        {
            PercentageInSibling = GetPercentage(Size, siblingSize);
            PercentageInAll = GetPercentage(Size, totalSize);
        }

        public IEnumerable GetChildren(object parent)
        {
            var fsb = parent as FileSizeItemBase;
            if (fsb != null)
                return fsb.GetChildrenCore();
            else
                return this.GetChildrenCore();
        }

        public bool HasChildren(object parent)
        {
            var fsb = parent as FileSizeItemBase;
            if (fsb != null)
                return fsb.HasChildrenCore();
            else
                return this.HasChildrenCore();
        }

        protected abstract IEnumerable GetChildrenCore();

        protected abstract bool HasChildrenCore();

        public abstract void Load(FileSystemInfo fsi);

        public abstract void ResolveSize();

        public int CompareTo(object obj)
        {
            var other = obj as FileSizeItemBase;
            if (other == null)
                return 0;

            return -this.Size.CompareTo(other.Size);
        }

        public override string ToString()
        {
            return Name;
        }

        protected double GetPercentage(long n1, long n2)
        {
            return (n2 != 0) ? (double)n1 * 100.0 / n2 : 0;
        }
    }
}
