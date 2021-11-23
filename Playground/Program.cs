// See https://aka.ms/new-console-template for more information
using CodegenAssertions;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

// Console.WriteLine(CodegenInfoResolver.GetCodegenInfo(CompilationTier.Tier1, () => A.Heavy(3f)));
// Console.WriteLine(CodegenInfoResolver.GetCodegenInfo(CompilationTier.Tier1, () => A.Do1(3f)));
// Console.WriteLine(CodegenInfoResolver.GetCodegenInfo(CompilationTier.Tier1, () => A.LoopHHH(3)));
Console.WriteLine(CodegenInfoResolver.GetCodegenInfo(CompilationTier.Tier1, () => A.Heavy(0)));
Console.WriteLine(CodegenInfoResolver.GetCodegenInfo(CompilationTier.Tier1, () => A.Aaa(0)));
Console.WriteLine(CodegenInfoResolver.GetCodegenInfo(CompilationTier.Tier1, () => A.Heavy(0)));

static class A
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float Do1(float a)
    {
        return a * 2;
    }

    public static float Heavy(float a)
    {
        var b = Do1(a);
        var c = Do1(b);
        if (a > 10)
            c += Aaa(a);
        return c + b;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float Aaa(float h)
    {
        return h * h * h;
    }
}