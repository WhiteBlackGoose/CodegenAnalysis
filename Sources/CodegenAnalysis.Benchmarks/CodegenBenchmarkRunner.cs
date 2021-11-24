using System;
using System.Linq;
using System.Collections.Generic;

namespace CodegenAnalysis.Benchmarks;

public static class CodegenBenchmarkRunner
{
    public static void Run<T>(Output? output = null)
    {
        Run(typeof(T), output);
    }

    public static void Run(Type type, Output? output = null)
    {
        var jobs = GetJobs(type);
        output?.Logger?.WriteLine($"Detected jobs: {string.Join(", ", jobs)}");
    }


    private static IEnumerable<CAJobAttribute> GetJobs(Type type)
    {
        return type.GetCustomAttributes(typeof(CAJobAttribute), false).Select(c => (CAJobAttribute)c);
    }
}