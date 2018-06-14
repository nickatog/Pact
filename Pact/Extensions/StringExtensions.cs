﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pact.StringExtensions
{
    public static class StringExtensions
    {
        private static readonly Regex s_tokenPattern =
            new Regex(
                @"((?<Key>\w+)=(?<Value>(?:\[(?>\[(?<DEPTH>)|\](?<-DEPTH>)|[^\[\]]*)*(?(DEPTH)(?!))\]|\S*(?:\s+[^=]+\s)*)))",
                RegexOptions.Compiled);

        public static bool Eq(
            this string left,
            string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        public static IDictionary<string, string> ParseKeyValuePairs(
            this string line)
        {
            if (line == null)
                return new Dictionary<string, string>();

            return
                EnumerateGroups(s_tokenPattern.Matches(line))
                .ToDictionary(
                    __kvp => __kvp.Key,
                    __kvp => __kvp.Value);

            IEnumerable<(string Key, string Value)> EnumerateGroups(MatchCollection matchCollection)
            {
                foreach (Match match in matchCollection)
                    yield return (Key: match.Groups["Key"].Value, Value: match.Groups["Value"].Value);
            }
        }
    }
}
