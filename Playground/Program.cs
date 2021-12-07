// See https://aka.ms/new-console-template for more information
using CodegenAnalysis;
using CodegenAnalysis.Benchmarks;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

CodegenBenchmarkRunner.Run<A>();
// Console.WriteLine(CodegenInfoResolver.GetCodegenInfo(CompilationTier.Tier1, () => A.SomeHeavyMethod(3, 5)));


[CAJob(Tier = CompilationTier.Tier1),
 CAJob(Tier = CompilationTier.Default)]

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
    [CASubject(typeof(A), "Do1", new [] { typeof(float) })]
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

    [CAAnalyze(13.5f)]
    public static float AaaLoop(float h)
    {
        while (h < 0)
        {
            h -= 5f;
        }
        return h;
    }

    public static int SomeHeavyMethod(int a, int b)
    {
        a += b;
        b += a;
        b += a / b;
        b += b / a;
        a *= a;
        if (a < 0) a = 5;
        if (a < -5)
            throw new();
        return a - b;
    }
}

