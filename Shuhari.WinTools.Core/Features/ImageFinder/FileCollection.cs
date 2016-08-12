using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Markup;

namespace Shuhari.WinTools.Core.Features.ImageFinder
{
    public sealed class FileCollection : ObservableCollection<FileItem>
    {
        public const string FILENAME = "imgfind.hash.xml";

        [Browsable(false)]
        [ReadOnly(true)]
        public bool IsChanged { get; private set; }

        public FileCollection()
        {
            IsChanged = false;
        }

        public static FileCollection Load(DirectoryInfo dir)
        {
            string filePath = GetFilePath(dir);
            if (!File.Exists(filePath))
                return new FileCollection();
            try
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                    return (FileCollection)XamlReader.Load(fileStream);
            }
            catch (Exception ex)
            {
                return new FileCollection();
            }
        }

        public void Save(DirectoryInfo dir)
        {
            string filePath = GetFilePath(dir);
            if (File.Exists(filePath))
                File.Delete(filePath);
            if (this.Count > 0)
                using (FileStream fileStream = File.OpenWrite(filePath))
                    XamlWriter.Save(this, fileStream);
        }

        private static string GetFilePath(DirectoryInfo dir)
        {
            return Path.Combine(dir.FullName, "imgfind.hash.xml");
        }

        public void AddItem(FileItem item)
        {
            Add(item);
            IsChanged = true;
        }

        public void UpdateItem(FileItem updateItem)
        {
            FileItem fileItem = GetItem(updateItem.Name);
            if (fileItem == null)
                return;
            fileItem.Update(updateItem);
            IsChanged = true;
        }

        public void RemoveItem(FileItem item)
        {
            FileItem fileItem = this.GetItem(item.Name);
            if (fileItem == null)
                return;
            Remove(fileItem);
            IsChanged = true;
        }

        public FileItem GetItem(string fileName)
        {
            return this.FirstOrDefault(f => f.MatchName(fileName));
        }
    }
}
