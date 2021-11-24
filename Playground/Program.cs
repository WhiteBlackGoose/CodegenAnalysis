// See https://aka.ms/new-console-template for more information
using CodegenAnalysis;
using CodegenAnalysis.Benchmarks;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

CodegenBenchmarkRunner.Run<A>();

[CAJob(Tier = CompilationTier.Default),
 CAJob(Tier = CompilationTier.Tier1)]

[CAColumn(CAColumn.Branches), 
 CAColumn(CAColumn.Calls), 
 CAColumn(CAColumn.CodegenSize), 
 CAColumn(CAColumn.StaticStackAllocations)]
public class A
{
    [CAInput(3.5f)]
    [CAInput(13.5f)]
    public static float Heavy(float a)
    {
        var b = Do1(a);
        var c = Do1(b);
        if (a > 10)
            c += Aaa(a);
        return c + b;
    }

    [CAInput(6f)]
    public static float Square(float a)
    {
        return a * a;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float Do1(float a)
    {
        return a * 2;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float Aaa(float h)
    {
        return h * h * h;
    }
}