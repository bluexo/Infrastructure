using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origine
{
    public static class StringExtensions
    {
        public static string TitleCase(this string str) => (string.IsNullOrWhiteSpace(str) || char.IsUpper(str.FirstOrDefault())) ? str : str.Substring(0, 1).ToUpperInvariant() + str.Substring(1);
        public static string Format(this string str, params string[] args) => string.Format(str, args).Replace("\\n", "\n");
    }
}
