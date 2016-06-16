using System;
using System.Text;
using Shuhari.Library.Common.Utils;

namespace Shuhari.WinTools.Core.Features.Encode.Providers
{
    /// <summary>
    /// 解码器基类
    /// </summary>
    public abstract class EncodeProvider : IComparable
    {
        public abstract EncodeProviderMetadata Metadata { get; }

        public virtual string Encode(string input, Encoding encoding)
        {
            throw new NotSupportedException("Must implement encode in derived class");
        }

        public virtual string Decode(string input, Encoding encoding)
        {
            throw new NotSupportedException("Must implement encode in derived class");
        }

        public int CompareTo(object obj)
        {
            return this.CompareBy(obj, x => x.Metadata.Order);
        }

        internal static bool IsProviderType(Type t)
        {
            return t.IsClass && t.IsPublic && !t.IsAbstract &&
                t.IsSubclassOf(typeof(EncodeProvider));
        }
    }
}
