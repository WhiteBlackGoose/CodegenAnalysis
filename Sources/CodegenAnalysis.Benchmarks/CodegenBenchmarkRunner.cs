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
        output?.Logger?.WriteLine($"Detected jobs: {string.Join(", ", jobs)}");

        var columns = GetColumns(type);
        output?.Logger?.WriteLine($"Detected columns: {string.Join(", ", columns)}");

        var methods = GetMethods(type);
        output?.Logger?.WriteLine($"Detected methods: {string.Join(", ", methods)}");
    }

    private static IEnumerable<CAJobAttribute> GetJobs(Type type)
    {
        return type.GetCustomAttributes(typeof(CAJobAttribute), false).Select(c => (CAJobAttribute)c);
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