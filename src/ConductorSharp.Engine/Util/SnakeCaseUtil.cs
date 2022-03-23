using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConductorSharp.Engine.Util;

public class SnakeCaseUtil
{
    public static string ToCapitalizedPrefixSnakeCase(string str)
    {
        str = str.Replace("-", "");
        var pattern = new Regex(
            @"[A-Z0-9_]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z][0-9]+|[A-Z]"
        );
        var prefixPattern = new Regex(@"[A-Z0-9]{2,}");

        var matches = pattern.Matches(str);

        var chunks = new List<string>();

        for (var i = 0; i < matches.Count; i++)
        {
            if (i == 0 && prefixPattern.Match(matches[i].Value).Success)
            {
                chunks.Add(matches[i].Value.Replace("_", ""));
                continue;
            }
            else
                chunks.Add(matches[i].Value.ToLower());
        }

        return string.Join("_", chunks);
    }

    public static string ToLowercasedPrefixSnakeCase(string str)
    {
        str = str.Replace("-", "");
        var pattern = new Regex(
            @"[A-Z0-9_]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z][0-9]+|[A-Z]"
        );
        var prefixPattern = new Regex(@"[A-Z0-9]{2,}");

        var matches = pattern.Matches(str);

        var chunks = new List<string>();

        for (var i = 0; i < matches.Count; i++)
        {
            if (i == 0 && prefixPattern.Match(matches[i].Value).Success)
            {
                chunks.Add(matches[i].Value.Replace("_", "").ToLower());
                continue;
            }
            else
                chunks.Add(matches[i].Value.ToLower());
        }

        return string.Join("_", chunks);
    }

    public static string ToSnakeCase(string str) =>
        string.Concat(
                str.Select(
                    (x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()
                )
            )
            .ToLower();

    public static string ToPascalCase(string str)
    {
        str = str.Replace("-", "");
        return str.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => char.ToUpperInvariant(s[0]) + s[1..].ToLowerInvariant())
            .Aggregate(string.Empty, (s1, s2) => s1 + s2);
    }

    public static string ToCamelCase(string str)
    {
        str = str.Replace("-", "");
        str = str.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => char.ToUpperInvariant(s[0]) + s[1..])
            .Aggregate(string.Empty, (s1, s2) => s1 + s2);

        return char.ToLowerInvariant(str[0]) + str[1..];
    }

    // Note: This is created special for BOP. Please don't change this method
    // Example:  comCPEIPAddress_v6  -> comCpeipAddressV6
    public static string ConvertStrToCustomCamelCaseVariant(string input)
    {
        var pattern = new Regex(
            @"[A-Z0-9_]{2,}(?=[A-Z_][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[a-z0-9]+|[A-Z][a-z0-9]+"
        );
        var chunks = new List<string>();
        var matches = pattern.Matches(input);
        var str = string.Join("_", matches.Select(e => e.Value.ToLower()));
        return str.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(
                (s, i) =>
                    (i == 0 ? char.ToLowerInvariant(s[0]) : char.ToUpperInvariant(s[0]))
                    + s[1..]
            )
            .Aggregate(string.Empty, (s1, s2) => s1 + s2);
    }
}
