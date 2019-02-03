﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DoxygenEditor.Lexers
{
    static class RexUtils
    {
        public static IEnumerable<Match> GetMatches(string input, Regex rex)
        {
            List<Match> result = new List<Match>();
            MatchCollection matches = rex.Matches(input);
            foreach (Match match in matches)
                result.Add(match);
            return (result);
        }
    }
}
