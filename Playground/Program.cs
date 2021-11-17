// See https://aka.ms/new-console-template for more information
using CodegenAssertions;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

Console.WriteLine(CodegenInfoResolver.GetCodegenInfo(CompilationTier.Tier1, () => A.Heavy(3f)));
Console.WriteLine(CodegenInfoResolver.GetCodegenInfo(CompilationTier.Tier1, () => A.Do1(3f)));

class A
{
    public static int Add(int a, int b) => a + b * a;
    public static int Add1(int a, int b) => a + b * a;
    public static float Add(float a, float b) => a + b * a;
    public static float AddF(float a, float b, Func<int, int> _) => a + b * a;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float Do1(float a)
    {
        return a * 2;
    }

    public static float Heavy(float a)
    {
        var b = Do1(a);
        var c = Do1(b);
        return AddN(b, c);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float AddN(float a, float b) => a + b;

    public static T AddG<T>(T a, T b)
    {
        if (typeof(T) == typeof(int))
            return (T)(object)((int)(object)a + (int)(object)b);
        else if (typeof(T) == typeof(float))
            return (T)(object)((float)(object)a + (float)(object)b);
        return default!;
    }

    public static class GenericDuck<T>
    {
        public static T AddC(T a, T b) => AddG<T>(a, b);
    }
}