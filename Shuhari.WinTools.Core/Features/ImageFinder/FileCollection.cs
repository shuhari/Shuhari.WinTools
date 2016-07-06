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
            this.IsChanged = false;
        }

        public static FileCollection Load(DirectoryInfo dir)
        {
            string filePath = FileCollection.GetFilePath(dir);
            if (!File.Exists(filePath))
                return new FileCollection();
            try
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                    return (FileCollection)XamlReader.Load((Stream)fileStream);
            }
            catch (Exception ex)
            {
                return new FileCollection();
            }
        }

        public void Save(DirectoryInfo dir)
        {
            string filePath = FileCollection.GetFilePath(dir);
            if (File.Exists(filePath))
                File.Delete(filePath);
            using (FileStream fileStream = File.OpenWrite(filePath))
                XamlWriter.Save((object)this, (Stream)fileStream);
        }

        private static string GetFilePath(DirectoryInfo dir)
        {
            return Path.Combine(dir.FullName, "imgfind.hash.xml");
        }

        public void AddItem(FileItem item)
        {
            this.Add(item);
            this.IsChanged = true;
        }

        public void UpdateItem(FileItem updateItem)
        {
            FileItem fileItem = this.GetItem(updateItem.Name);
            if (fileItem == null)
                return;
            fileItem.Update(updateItem);
            this.IsChanged = true;
        }

        public void RemoveItem(FileItem item)
        {
            FileItem fileItem = this.GetItem(item.Name);
            if (fileItem == null)
                return;
            this.Remove(fileItem);
            this.IsChanged = true;
        }

        public FileItem GetItem(string fileName)
        {
            return Enumerable.FirstOrDefault<FileItem>((IEnumerable<FileItem>)this, (Func<FileItem, bool>)(f => f.MatchName(fileName)));
        }
    }
}
