

using System;
using System.Text;

namespace Origine
{
    public static partial class Utility
    {
        /// <summary>
        /// 字符相关的实用函数。
        /// </summary>
        public static class Text
        {
            [ThreadStatic]
            private static StringBuilder _cachedStringBuilder = null;

            /// <summary>
            /// 获取格式化字符串。
            /// </summary>
            /// <param name="format">字符串格式。</param>
            /// <param name="arg0">字符串参数 0。</param>
            /// <returns>格式化后的字符串。</returns>
            public static string Format(string format, object arg0)
            {
                if (format == null)
                {
                    throw new GameException("Format is invalid.");
                }

                CheckCachedStringBuilder();
                _cachedStringBuilder.Length = 0;
                _cachedStringBuilder.AppendFormat(format, arg0);
                return _cachedStringBuilder.ToString();
            }

            /// <summary>
            /// 获取格式化字符串。
            /// </summary>
            /// <param name="format">字符串格式。</param>
            /// <param name="arg0">字符串参数 0。</param>
            /// <param name="arg1">字符串参数 1。</param>
            /// <returns>格式化后的字符串。</returns>
            public static string Format(string format, object arg0, object arg1)
            {
                if (format == null)
                {
                    throw new GameException("Format is invalid.");
                }

                CheckCachedStringBuilder();
                _cachedStringBuilder.Length = 0;
                _cachedStringBuilder.AppendFormat(format, arg0, arg1);
                return _cachedStringBuilder.ToString();
            }

            /// <summary>
            /// 获取格式化字符串。
            /// </summary>
            /// <param name="format">字符串格式。</param>
            /// <param name="arg0">字符串参数 0。</param>
            /// <param name="arg1">字符串参数 1。</param>
            /// <param name="arg2">字符串参数 2。</param>
            /// <returns>格式化后的字符串。</returns>
            public static string Format(string format, object arg0, object arg1, object arg2)
            {
                if (format == null)
                {
                    throw new GameException("Format is invalid.");
                }

                CheckCachedStringBuilder();
                _cachedStringBuilder.Length = 0;
                _cachedStringBuilder.AppendFormat(format, arg0, arg1, arg2);
                return _cachedStringBuilder.ToString();
            }

            /// <summary>
            /// 获取格式化字符串。
            /// </summary>
            /// <param name="format">字符串格式。</param>
            /// <param name="args">字符串参数。</param>
            /// <returns>格式化后的字符串。</returns>
            public static string Format(string format, params object[] args)
            {
                if (format == null)
                {
                    throw new GameException("Format is invalid.");
                }

                if (args == null)
                {
                    throw new GameException("Args is invalid.");
                }

                CheckCachedStringBuilder();
                _cachedStringBuilder.Length = 0;
                _cachedStringBuilder.AppendFormat(format, args);
                return _cachedStringBuilder.ToString();
            }

            private static void CheckCachedStringBuilder()
            {
                if (_cachedStringBuilder == null)
                {
                    _cachedStringBuilder = new StringBuilder(1024);
                }
            }
        }
    }
}
