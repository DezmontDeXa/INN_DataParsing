using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Shared
{
    public static class Extensions
    {
        public static string Clear(this string str)
        {
            //str = Regex.Replace(str, @"[^\u0000-\u007F]+", string.Empty);
            str = str.Replace("\t", " ");
            str = str.Replace("\n", " ");
            str = str.Replace("\r", " ");
            str = Regex.Replace(str, " {2,}", " ");
            return str;
        }
    }
}
