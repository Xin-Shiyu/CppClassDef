using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CppClassDef
{
    static class CodeStyle
    {
        public static string Indent(this string str) => str.Split('\n').Select(code => $"    {code}").ToLines();
        public static string JoinWith(this IEnumerable<string> strs, string delim) => string.Join(delim, strs);
        public static string ToLines(this IEnumerable<string> strs) => strs.JoinWith("\n");
    }
}
