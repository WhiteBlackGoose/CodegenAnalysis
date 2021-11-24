using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace CodegenAnalysis.Benchmarks;

public static class CodegenBenchmarkRunner
{
    public static void Run<T>(Output? output = null)
    {
        Run(typeof(T), output);
    }

    public static void Run(Type type, Output? output = null)
    {
        output ??= new();

        var jobs = GetJobs(type);
        output?.Logger?.WriteLine($"Detected jobs: \n  {string.Join("\n  ", (IEnumerable<CAJobAttribute>)jobs)}", ConsoleColor.DarkMagenta);

        var columns = GetColumns(type).ToArray();
        output?.Logger?.WriteLine($"\nDetected columns: \n  {string.Join("\n  ", (IEnumerable<CAColumnAttribute>)columns)}", ConsoleColor.DarkMagenta);

        var methods = GetMethods(type);
        output?.Logger?.WriteLine($"\nDetected methods: \n  {string.Join("\n  ", methods.Select(c => c.Info + "(" + string.Join(", ", c.Args) + ")"))}", ConsoleColor.DarkMagenta);

        var codegens = new SortedDictionary<(MethodInfo, CompilationTier), CodegenInfo>(new MiTierComparer());
        var table = new MarkdownTable(new [] { "Job", "Method", "Input" }.Concat(columns.Select(c => c.ToString())));

        output?.Logger?.WriteLine("");

        var rowId = 0;
        // for (int i = 0; i < jobs.Length; i++)
        foreach (var job in jobs)
        {
            foreach (var (mi, args) in methods)
            {
                output?.Logger?.WriteLine($"Investigating {mi} {string.Join(", ", args)} {job}...");
                var ci = CodegenInfoResolver.GetCodegenInfo(job.Tier, mi, args);
                codegens[(mi, job.Tier)] = ci; // overwriting the last to get a richer result
                table[rowId, 0] = job.ToString();
                table[rowId, 1] = mi.ToString();
                table[rowId, 2] = string.Join(", ", args);
                
                for (int i = 0; i < columns.Length; i++)
                {
                    table[rowId, i + 3] = columns[i].Column switch
                    {
                        CAColumn.Branches => IntToString(CodegenAnalyzers.GetBranches(ci.Instructions).Count()),
                        CAColumn.Calls => IntToString(CodegenAnalyzers.GetCalls(ci.Instructions).Count()),
                        CAColumn.CodegenSize => BytesToString(ci.Bytes.Count),
                        CAColumn.StaticStackAllocations => BytesToString(CodegenAnalyzers.GetStaticStackAllocatedMemory(ci.Instructions)),
                        var unexpected => throw new($"Internal error. Unexpected {unexpected}")
                    };
                }
                rowId++;
            }
        }

        output?.Logger?.WriteLine("");

        foreach (var pair in codegens)
        {
            var (mi, ci) = (pair.Key, pair.Value);
            output?.Logger?.WriteLine("");
            output?.Logger?.WriteLine(mi.ToString(), ConsoleColor.Blue);
            output?.Logger?.WriteLine("    " + ci.ToString().Replace("\n", "\n    "), ConsoleColor.DarkGray);
            output?.Logger?.WriteLine("");
        }

        output?.Logger?.WriteLine(table.ToString(), ConsoleColor.Blue);
        
        static string IntToString(int a)
            => a is 0 ? " - " : a.ToString();

        static string BytesToString(int? a)
            => a switch
            {
                0 => " - ",
                null => " ? ",
                { } other => $"{other} B"
            };
    }


    private class MiTierComparer : IComparer<(MethodInfo, CompilationTier)>
    {
        public int Compare((MethodInfo, CompilationTier) x, (MethodInfo, CompilationTier) y)
        {
            if (x.Item1 == y.Item1)
                return ((int)x.Item2).CompareTo((int)y.Item2);
            return x.Item1.GetHashCode().CompareTo(y.Item1.GetHashCode());
        }
    }

    private static IEnumerable<CAJobAttribute> GetJobs(Type type)
    {
        return type
                .GetCustomAttributes(typeof(CAJobAttribute), false)
                .Select(c => (CAJobAttribute)c);
    }

    private static IEnumerable<CAColumnAttribute> GetColumns(Type type)
    {
        return type.GetCustomAttributes(typeof(CAColumnAttribute), false).Select(c => (CAColumnAttribute)c);
    }

    private static IEnumerable<(MethodInfo Info, object[] Args)> GetMethods(Type type)
    {
        return type
                .GetMethods()
                .Select(mi =>
                    (Mi: mi, Attrs: mi.GetCustomAttributes(typeof(CAInputAttribute), false)))
                .Where(c => c.Attrs.Any())
                .SelectMany(c =>
                    c.Attrs.Zip(Enumerable.Repeat(c.Mi, c.Attrs.Count()), (l, r) => 
                        (Mi: r, Attrs: (CAInputAttribute)l)
                    )
                )
                .Select(c =>
                    (Mi: c.Mi, Attrs: c.Attrs.Arguments)
                );
    }
}