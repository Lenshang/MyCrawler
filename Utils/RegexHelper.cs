using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MyCrawler.Utils
{
    public class RegexHelper
    {
        public static string RegexOne(string input,string pattern)
        {
            var groups = Regex.Match(input, pattern).Groups;
            if (groups.Count > 1)
            {
                return groups[1].Value;
            }
            else if (groups.Count > 0)
            {
                return groups[0].Value;
            }
            else
            {
                return null;
            }
        }
    }
}
