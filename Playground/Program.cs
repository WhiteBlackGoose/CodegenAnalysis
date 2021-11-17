// See https://aka.ms/new-console-template for more information
using CodegenAssertions;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

Console.WriteLine(CodegenInfoResolver.GetCodegenInfo(CompilationTier.Tier1, () => A.AddG(3, 5)));
Console.WriteLine(CodegenInfoResolver.GetCodegenInfo(CompilationTier.Tier1, () => A.AddG(3f, 5f)));

class A
{
    public static int Add(int a, int b) => a + b * a;
    public static int Add1(int a, int b) => a + b * a;
    public static float Add(float a, float b) => a + b * a;
    public static float AddF(float a, float b, Func<int, int> _) => a + b * a;

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