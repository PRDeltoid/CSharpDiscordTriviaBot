using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot.Extensions
{
    public static class StringExtensions
    {
        public static string PadBoth(this string str, int length)
        {
            int spaces = length - str.Length;
            int padLeft = spaces / 2 + str.Length;
            return str.PadLeft(padLeft).PadRight(length);
        }

        public static string PadOffcenter(this string toPad, int centerLength, int totalLength)
        {
            int spaces = centerLength - toPad.Length;
            int padRight = totalLength - centerLength;
            return toPad.PadLeft(centerLength/2).PadRight(padRight);
        }
    }
}
