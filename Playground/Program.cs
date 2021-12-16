// See https://aka.ms/new-console-template for more information
using CodegenAnalysis;
using CodegenAnalysis.Benchmarks;
using HonkSharp.Functional;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

CodegenBenchmarkRunner.Run<DSdsd>();

[CAJob(Tier = CompilationTier.Default)]
[CAJob(Tier = CompilationTier.Tier1)]
class DSdsd
{
    static int x;

    static volatile int xv;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int IncrementField()
    {
        x++;
        return x;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int IncrementFieldVolatile()
    {
        xv++;
        return xv;
    }

    [CAAnalyze]
    [CASubject(typeof(DSdsd), nameof(IncrementField))]
    public static void IncrementFieldEntryPoint()
    {
        IncrementField();
    }

    [CAAnalyze]
    [CASubject(typeof(DSdsd), nameof(IncrementFieldVolatile))]
    public static void IncrementFieldVolatileEntryPoint()
    {
        IncrementFieldVolatile();
    }
}