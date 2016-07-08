using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Shuhari.WinTools.Core.Features.ImageFinder
{
    public class FileItemCollection : ObservableCollection<FileItem>
    {
        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="selected"></param>
        public void Select(FileItem item, bool selected)
        {
            if (item != null && item.Selected != selected)
            {
                item.Selected = selected;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler SelectionChanged;

        /// <summary>
        /// 获取同组文件
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        public FileItem[] GetSameGroup(FileItem fi)
        {
            var list = new List<FileItem>();
            int num = IndexOf(fi);
            if (num >= 0)
            {
                list.Add(fi);
                for (int index = num - 1; index >= 0; --index)
                {
                    FileItem fileItem = this[index];
                    if (fileItem.GroupIndex == fi.GroupIndex)
                        list.Insert(0, fileItem);
                    else
                        break;
                }
                for (int index = num + 1; index < Count; ++index)
                {
                    FileItem fileItem = this[index];
                    if (fileItem.GroupIndex == fi.GroupIndex)
                        list.Add(fileItem);
                    else
                        break;
                }
            }
            return list.ToArray();
        }

        public void RemoveOrphans()
        {
            var orphans = this.GroupBy(f => f.GroupIndex)
                .Where(g => g.Count() == 1)
                .SelectMany(g => g)
                .ToArray();
            foreach (var orphan in orphans)
                Remove(orphan);
        }

        public void SelectSameDir(FileItem item, bool selected)
        {
            bool changed = false;
            foreach (FileItem fileItem in this)
            {
                if (fileItem.DirName == item.DirName && fileItem.Selected)
                {
                    fileItem.Selected = selected;
                    changed = true;
                }
            }
            if (changed)
                SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void GetSelectInfo(out int selectCount, out int[] allSelectedGroups)
        {
            var selected = this.Where(f => f.Selected).ToArray();
            selectCount = selected.Length;

            int[] selectedGroups = selected.Select(f => f.GroupIndex).Distinct().ToArray();
            var resultGroups = new List<int>();
            foreach (int groupIndex in selectedGroups)
            {
                if (selected.Where(f => f.GroupIndex == groupIndex).Count() == this.Where(f => f.GroupIndex == groupIndex).Count())
                    resultGroups.Add(groupIndex);
            }
            allSelectedGroups = resultGroups.ToArray();
        }
    }
}
