// See https://aka.ms/new-console-template for more information
using CodegenAnalysis;
using CodegenAnalysis.Benchmarks;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

CodegenBenchmarkRunner.Run<A>();


[CAJob(Tier = CompilationTier.Tier1)]

[CAColumn(CAColumn.Branches),
 CAColumn(CAColumn.Calls), 
 CAColumn(CAColumn.StaticStackAllocations),
 CAColumn(CAColumn.CodegenSize),
 CAColumn(CAColumn.ILSize)]

[CAExport(Export.Html),
 CAExport(Export.Md)]

[CAOptions(VisualizeBackwardJumps = true)]
public class A
{
    
    [CAAnalyze(3.5f)]
    [CAAnalyze(13.5f)]
    [CASubject("Do1")]
    public static float Heavy(float a)
    {
        var b = Do1(a);
        var c = Do1(b);
        if (a > 10)
            c += Aaa(a);
        return c + b;
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

    /*
    [CAAnalyze(6f)]
    public static float Square(float a)
    {
        return a * a;
    }

    [CAAnalyze(3)]
    public static float Sum(float a)
    {
        var r = 0f;
        for (int i = 0; i < 100; i++)
        {
            while (a > 0)
            {
                a -= 1f;
                r += a;
            }
            a = r > 0 ? 10f : 11f;
        }
        return r;
    }*/
}