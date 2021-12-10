
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodegenAnalysis;

internal sealed class Lines
{
    private readonly IReadOnlyList<StringBuilder> lines;
    
    private int padding = 0;
    
    internal Lines(IReadOnlyList<StringBuilder> lines)
        => this.lines = lines;

    private void InsertColumn(int width)
    {
        padding += width;
        var toInsert = new string(' ', width);
        foreach (var line in lines)
            line.Insert(0, toInsert);
    }

    internal Lines Add(string prefix, IEnumerable<int> dst)
    {
        var toFind = new string(' ', prefix.Length);
        foreach (var id in dst)
        {
            var fs = FreeSpace(prefix.Length, lines[id]);
            if (fs < prefix.Length)
                InsertColumn(prefix.Length - fs);
            OnTop(prefix, lines[id]);
        }

        return this;

        static int FreeSpace(int count, StringBuilder sb)
        {
            for (int i = 0; i < count; i++)
                if (sb[i] != ' ')
                    return i;
            return count;
        }

        static void OnTop(string prefix, StringBuilder sb)
        {
            for (int i = 0; i < prefix.Length; i++)
                sb[i] = prefix[i];
        }
    }

    public Lines DrawArrows(IEnumerable<(int From, int To)> dst, bool ascii = false)
    {
        var symbolVertical =      ascii ? '|' : '│';
        var symbolHorizontal =    ascii ? '-' : '─';
        var symbolIntersection =  ascii ? '+' : '┼';
        var symbolUpRight =       ascii ? '+' : '└';
        var symbolDownRight =     ascii ? '+' : '┌';
        var symbolUpDownRight =   ascii ? '+' : '├';
        var symbolLeftDownRight = ascii ? '+' : '┬';
        var symbolArrow =         ascii ? '>' : '>';
        var symbolLeftUpRight =   ascii ? '+' : '┴';

        foreach (var (from, to) in dst)
        {
            var padToTry = padding - 2;
            while (padToTry >= 0 && !HasNoVerticals(from, to, padToTry))
                padToTry--;
            if (padToTry < 0)
            {
                InsertColumn(padding == 0 ? 2 : 1);
                padToTry = 0;
            }

            for (int i = Math.Min(from, to); i <= Math.Max(from, to); i++)
            {
                lines[i][padToTry] = Merge(lines[i][padToTry], symbolVertical);
            }

            lines[from][padToTry] = from < to ? symbolDownRight : symbolUpRight;
            lines[to][padToTry] = from > to ? symbolDownRight : symbolUpRight;
            for (int i = padToTry + 1; i < padding; i++)
            {
                lines[from][i] = Merge(symbolHorizontal, lines[from][i]);
                lines[to][i] = Merge(symbolHorizontal, lines[to][i]);
            }
            lines[to][padding - 1] = symbolArrow;
        }

        return this;

        bool HasNoVerticals(int from, int to, int colId)
        {
            for (int i = Math.Min(from, to); i <= Math.Max(from, to); i++)
            {
                if (lines[i][colId] == symbolVertical)
                    return false;
            }
            return true;
        }

        char Merge(char a, char b)
        {
            if (Are(a, b, symbolVertical, symbolHorizontal))
                return symbolIntersection;
            if (Are(a, b, symbolVertical, ' '))
                return symbolVertical;
            if (a == ' ') return b;
            if (b == ' ') return a;
            if (a == b)
                return a;
            if (Are(a, b, symbolUpRight, symbolVertical) || Are(a, b, symbolDownRight, symbolVertical))
                return symbolUpDownRight;
            if (Are(a, b, symbolUpDownRight, symbolVertical))
                return symbolUpDownRight;
            if (Are(a, b, symbolHorizontal, symbolDownRight))
                return symbolLeftDownRight;
            if (Are(a, b, symbolHorizontal, symbolUpRight))
                return symbolLeftUpRight;
            if (Are(a, b, symbolArrow, symbolHorizontal))
                return symbolArrow;
            // TODO: add other symbols
            throw new($"Unsupported merge of {a} and {b}");

            static bool Are(char a, char b, char one, char other)
                => a == one && b == other || a == other && b == one;
        }
    }

    public override string ToString()
        => string.Join("\n", lines.Select(c => c.ToString()));
}

internal static class LinesPrettifier
{
    internal static void MarkLines(IReadOnlyList<StringBuilder> text, IEnumerable<int> lines)
    {
    }


}
