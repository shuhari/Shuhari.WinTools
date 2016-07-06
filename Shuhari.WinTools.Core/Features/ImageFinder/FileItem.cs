using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shuhari.WinTools.Core.Features.ImageFinder
{
    public sealed class FileItem : INotifyPropertyChanged
    {
        private bool _selected;

        public string Name { get; set; }

        public long Size { get; set; }

        public DateTime ModifyTime { get; set; }

        public string Hash { get; set; }

        [DefaultValue(null)]
        public string Hash2 { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DirectoryInfo Dir { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int GroupIndex { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DirName
        {
            get
            {
                if (this.Dir == null)
                    return string.Empty;
                return this.Dir.FullName;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Selected
        {
            get
            {
                return this._selected;
            }
            set
            {
                if (this._selected == value)
                    return;
                this._selected = value;
                this.NotifyPropertyChanged("Selected");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool MatchName(string name)
        {
            return name.ToLowerInvariant() == this.Name.ToLowerInvariant();
        }

        public static FileItem Load(FileInfo fi)
        {
            FileItem fileItem = new FileItem();
            fileItem.Name = fi.Name;
            fileItem.Size = fi.Length;
            fileItem.ModifyTime = fi.LastWriteTime;
            if (fi.Length >= 10485760L)
            {
                using (FileStream fileStream = File.OpenRead(fi.FullName))
                {
                    fileItem.Hash = FileItem.GetStreamHash((Stream)fileStream, 0);
                    fileItem.Hash2 = FileItem.GetStreamHash((Stream)fileStream, 256);
                }
            }
            else
            {
                byte[] content = File.ReadAllBytes(fi.FullName);
                fileItem.Hash = FileItem.GetHash(content, 0);
                fileItem.Hash2 = FileItem.GetHash(content, 256);
            }
            return fileItem;
        }

        public bool IsChanged(FileInfo fi)
        {
            if (this.Size == fi.Length)
                return this.ModifyTime != fi.LastWriteTime;
            return true;
        }

        public void Update(FileItem item)
        {
            this.Size = item.Size;
            this.ModifyTime = item.ModifyTime;
            this.Hash = item.Hash;
            this.Hash2 = item.Hash2;
        }

        private static string GetHash(byte[] content, int offset)
        {
            if (offset > content.Length)
                return (string)null;
            if (offset > 0)
            {
                byte[] numArray = new byte[content.Length - offset];
                Array.Copy((Array)content, offset, (Array)numArray, 0, numArray.Length);
                content = numArray;
            }
            using (MD5CryptoServiceProvider cryptoServiceProvider = new MD5CryptoServiceProvider())
            {
                byte[] hash = cryptoServiceProvider.ComputeHash(content);
                cryptoServiceProvider.Clear();
                return FileItem.ToHexString(hash);
            }
        }

        private static string GetStreamHash(Stream stream, int offset)
        {
            stream.Seek((long)offset, SeekOrigin.Begin);
            using (MD5CryptoServiceProvider cryptoServiceProvider = new MD5CryptoServiceProvider())
            {
                byte[] hash = cryptoServiceProvider.ComputeHash(stream);
                cryptoServiceProvider.Clear();
                return FileItem.ToHexString(hash);
            }
        }

        private static string ToHexString(byte[] hash)
        {
            StringBuilder stringBuilder = new StringBuilder(32);
            foreach (byte num in hash)
                stringBuilder.Append(num.ToString("x2"));
            return stringBuilder.ToString();
        }

        public string GetFullPath()
        {
            return Path.Combine(this.DirName, this.Name);
        }

        private void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(propName));
        }
    }
}
