﻿using System;

namespace SolastaUnfinishedBusiness.Api.LanguageExtensions;

internal static class StringExtensions
{
    internal static bool Matches(this string source, string other)
    {
        if (source == null || other == null)
        {
            return false;
        }

        return source.IndexOf(other, 0, StringComparison.InvariantCultureIgnoreCase) != -1;
    }

    internal static string MarkedSubstring(this string source, string other)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(other))
        {
            return source;
        }

        var index = source.IndexOf(other, StringComparison.InvariantCultureIgnoreCase);

        if (index == -1)
        {
            return source;
        }

        var substr = source.Substring(index, other.Length);

        source = source.Replace(substr, substr.Cyan()).Bold();

        return source;
    }

#if false
    private static string Repeat(this string s, int n)
    {
        if (n < 0 || s == null || s.Length == 0)
        {
            return s;
        }

        return new StringBuilder(s.Length * n).Insert(0, s, n).ToString();
    }

    internal static string Indent(this string s, int n)
    {
        return "    ".Repeat(n) + s;
    }
#endif
}
