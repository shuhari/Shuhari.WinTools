using System;
using Microsoft.Win32;
using Shuhari.Library.Common.Utils;

namespace Shuhari.WinTools.Core.Utils
{
    /// <summary>
    /// Extend Registry/RegistryKey to provide read/write for common types
    /// </summary>
    public static class RegistryUtil
    {
        /// <summary>
        /// Set Binary value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetBinary(this RegistryKey key, string name, byte[] value)
        {
            Verify.IsNotNull(key, "key");
            Verify.IsNotNull(value, "value");

            key.SetValue(name, value, RegistryValueKind.Binary);
        }

        /// <summary>
        /// Set Binary value with only 1 byte
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetSingleBinary(this RegistryKey key, string name, byte value)
        {
            Verify.IsNotNull(key, "key");
            Verify.IsNotNull(value, "value");

            key.SetValue(name, new byte[] { value }, RegistryValueKind.Binary);
        }

        /// <summary>
        /// Set integer value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetDWord(this RegistryKey key, string name, int value)
        {
            Verify.IsNotNull(key, "key");

            key.SetValue(name, value, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Get integer value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetDWord(this RegistryKey key, string name)
        {
            Verify.IsNotNull(key, "key");

            return Convert.ToInt32(key.GetValue(name));
        }

        /// <summary>
        /// Set string value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetString(this RegistryKey key, string name, string value)
        {
            Verify.IsNotNull(key, "key");

            key.SetValue(name, value, RegistryValueKind.String);
        }
    }
}
