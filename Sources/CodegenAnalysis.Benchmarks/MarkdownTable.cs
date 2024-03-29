﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodegenAnalysis.Benchmarks;

public class MarkdownTable
{
    private readonly string[] header;
    private readonly List<string[]> lines = new();

    public MarkdownTable(IEnumerable<string> header)
    {
        this.header = header.ToArray();
    }

    public string this[int rowId, int columnId]
    {
        internal set
        {
            if (rowId > lines.Count)
                throw new Exception("Unexpected thing");
            if (rowId == lines.Count)
                lines.Add(new string[Width]);
            lines[rowId][columnId] = value;
        }
        get => lines[rowId][columnId];
    }

    public int Width => header.Length;

    public override string ToString()
    {
        var maxWidths = new int[Width];
        var appended = lines.Append(header);
        for (int i = 0; i < Width; i++)
        {
            maxWidths[i] = appended.Select(c => c[i].Length).Max();
        }

        var sb = new StringBuilder();
        FormatWithWidths(sb, header, maxWidths);
        sb.AppendLine();
        for (int i = 0; i < Width; i++)
            sb.Append("|:").Append('-', maxWidths[i] + 1).Append(":");
        sb.Append('|');
        sb.AppendLine();
        foreach (var line in lines)
        {
            FormatWithWidths(sb, line, maxWidths);
            sb.AppendLine();
        }

        return sb.ToString();

        static void FormatWithWidths(StringBuilder sb, string[] strings, int[] widths)
        {
            for (int i = 0; i < strings.Length; i++)
                sb.Append("| ").Append(strings[i]).Append(' ', widths[i] - strings[i].Length + 2);
            sb.Append('|');
        }
    }


    public string ToHtml()
    {
        var sb = new StringBuilder();
        sb.Append("<table><tr>");
        foreach (var h in header)
            sb.Append("<th>").Append(h).Append("</th>");
        sb.Append("</tr>");
        foreach (var line in lines)
        {
            sb.Append("<tr>");
            foreach (var el in line)
                sb.Append("<td>").Append(el).Append("</td>");
            sb.Append("</tr>");
        }
        sb.Append("</table>");
        return sb.ToString();
    }
}
