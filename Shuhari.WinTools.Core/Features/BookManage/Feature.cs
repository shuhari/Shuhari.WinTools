using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Shuhari.WinTools.Core.Features.BookManage
{
    public class Feature : BaseFeature
    {
        /// <summary>
        /// 去掉文件名中的后缀数字
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string TrimFileName(string fileName)
        {
            var re = new Regex(@"\.([0-9X]{10})\.(pdf|epub|mobi|azw4|azw3)$");
            return re.Replace(fileName, m =>
            {
                return "." + m.Groups[2].Value;
            });
        }

        /// <summary>
        /// 找到可以删除的重复文件。
        /// pdf/epub保留，其他可以删除，但如果只有1个文件则不论何种格式均不能删除
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public FileInfo[] GetFilesToDelete(IEnumerable<FileInfo> files)
        {
            var fileList = new List<FileInfo>(files);

            int keeped = 0;
            keeped += fileList.RemoveAll(it => Path.GetExtension(it.FullName).ToLowerInvariant() == ".pdf");
            keeped += fileList.RemoveAll(it => Path.GetExtension(it.FullName).ToLowerInvariant() == ".epub");

            fileList.Sort(CompareFilePriority);
            int toSkip = keeped > 0 ? 0 : 1;
            return fileList.Skip(toSkip).ToArray();
        }

        private int CompareFilePriority(FileInfo f1, FileInfo f2)
        {
            return GetPriority(f1).CompareTo(GetPriority(f2));
        }

        private int GetPriority(FileInfo fi)
        {
            var extensions = new List<string>(new []{ ".pdf", ".epub", ".mobi", ".azw4", ".azw3" });
            int index = extensions.IndexOf(fi.Extension.ToLower());
            if (index < 0)
                index = 9999;
            return index;
        }
    }
}
