using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using UnityEngine;

namespace Autosplit.BE5;

public enum SplitKind
{
    Collectible,
    Scent,
    Level,
    Event,
    Dialogue,
}

public record Split(string Original, SplitKind Kind, string Value)
{
    public string Original { get; } = Original;
    public SplitKind Kind { get; } = Kind;
    public string Value { get; } = Value;
    
    public static Split Deserialize(string input)
    {
        Debug.Assert(input.StartsWith("("));
        Debug.Assert(input.EndsWith(")"));
        Debug.Assert(input.Length > 2);
        
        var sides = input.Substring(1, input.Length - 2).Split(' ');

        // at the moment, all split kinds accept only a single parameter
        Debug.Assert(sides.Length == 2);
        Debug.Assert(sides[0].Length > 0);
        Debug.Assert(sides[1].Length > 0);

        if (!Enum.TryParse<SplitKind>(sides[0], true, out var kind))
        {
            kind = sides[0] switch
            {
                "C" => SplitKind.Collectible,
                "S" => SplitKind.Scent,
                "L" => SplitKind.Level,
                "E" => SplitKind.Event,
                "D" => SplitKind.Dialogue,
                _ => throw new ArgumentException($"Unknown split kind: {sides[0]}")
            };
        }

        return new Split(input, kind, sides[1]);
    }

    public void Serialize(StringBuilder sb) => sb.Append(Original);
}

public record SplitConfig(Split[] Splits)
{
    public Split[] Splits { get; } = Splits;
    
    public static void AddConverters()
    {
        TomlTypeConverter.AddConverter(typeof(SplitConfig), new TypeConverter
        {
            ConvertToObject = FromTomlString,
            ConvertToString = ToTomlString,
        });
    }

    private static string ToTomlString(object input, Type type)
    {
        Debug.Assert(type == typeof(SplitConfig));
        Debug.Assert(input is SplitConfig);
        
        var splits = (SplitConfig)input;

        var builder = new StringBuilder();
        for (var idx = 0; idx < splits.Splits.Length; idx++)
        {
            if (idx > 0)
            {
                builder.Append(' ');
            }

            splits.Splits[idx].Serialize(builder);
        }
        
        return builder.ToString();
    }

    private static object FromTomlString(string input, Type type)
    {
        Debug.Assert(type == typeof(SplitConfig));
        
        input = input.Trim();
        var inEntry = false;
        var splits = new List<Split>();
        var currentEntry = new StringBuilder();
        foreach (var c in input)
        {
            if (char.IsWhiteSpace(c) && !inEntry)
                continue;

            currentEntry.Append(c);
            switch (c)
            {
                case '(':
                {
                    inEntry = true;
                    continue;
                }
                case ')':
                {
                    inEntry = false;
                    var split = Split.Deserialize(currentEntry.ToString());
                    splits.Add(split);
                    currentEntry.Clear();
                    break;
                }
            }
        }
        
        Debug.Assert(currentEntry.ToString().Length == 0);
        return new SplitConfig(splits.ToArray());
    }
}