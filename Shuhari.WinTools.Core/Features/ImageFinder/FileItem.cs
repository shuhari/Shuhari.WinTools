using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;

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
                if (Dir == null)
                    return string.Empty;
                return this.Dir.FullName;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected == value)
                    return;
                _selected = value;
                NotifyPropertyChanged("Selected");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool MatchName(string name)
        {
            return name.Equals(Name, StringComparison.InvariantCultureIgnoreCase);
        }

        private const long SIZE_LIMIT = 1024L * 1024L * 50;

        public static FileItem Load(FileInfo fi)
        {
            var fileItem = new FileItem();
            fileItem.Name = fi.Name;
            fileItem.Size = fi.Length;
            fileItem.ModifyTime = fi.LastWriteTime;
            if (fi.Length >= SIZE_LIMIT)
            {
                using (var fileStream = File.OpenRead(fi.FullName))
                {
                    fileItem.Hash = GetStreamHash(fileStream, 0);
                    fileItem.Hash2 = GetStreamHash(fileStream, 256);
                }
            }
            else
            {
                byte[] content = File.ReadAllBytes(fi.FullName);
                fileItem.Hash = GetHash(content, 0);
                fileItem.Hash2 = GetHash(content, 256);
            }
            return fileItem;
        }

        public bool IsChanged(FileInfo fi)
        {
            if (Size == fi.Length)
                return ModifyTime != fi.LastWriteTime;
            return true;
        }

        public void Update(FileItem item)
        {
            Size = item.Size;
            ModifyTime = item.ModifyTime;
            Hash = item.Hash;
            Hash2 = item.Hash2;
        }

        private static string GetHash(byte[] content, int offset)
        {
            if (offset > content.Length)
                return null;
            if (offset > 0)
            {
                byte[] numArray = new byte[content.Length - offset];
                Array.Copy(content, offset, numArray, 0, numArray.Length);
                content = numArray;
            }
            using (var cryptoServiceProvider = new MD5CryptoServiceProvider())
            {
                byte[] hash = cryptoServiceProvider.ComputeHash(content);
                cryptoServiceProvider.Clear();
                return ToHexString(hash);
            }
        }

        private static string GetStreamHash(Stream stream, int offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            using (var cryptoServiceProvider = new MD5CryptoServiceProvider())
            {
                byte[] hash = cryptoServiceProvider.ComputeHash(stream);
                cryptoServiceProvider.Clear();
                return ToHexString(hash);
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
            return Path.Combine(DirName, Name);
        }

        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged == null)
                return;
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
