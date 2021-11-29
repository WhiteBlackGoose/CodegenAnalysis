using System.Runtime.CompilerServices;
using Xunit;

namespace CodegenAnalysis.Assertions.Tests;
public class CallToManagedMethodResolved
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float Do1(float a)
    {
        return a * 2;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float AddN(float a, float b) => a + b;

    public static float Heavy(float a)
    {
        var b = Do1(a);
        var c = Do1(b);
        return AddN(b, c);
    }


    [Fact]
    public void CallNameResolved()
    {
        var ci = CodegenInfo.Obtain(() => Heavy(3f), CompilationTier.Tier1);
        Assert.Contains("call      Single Do1(Single)", ci.ToString());
    }

    [Fact]
    public void JmpNameResolved()
    {
        var ci = CodegenInfo.Obtain(() => Heavy(3f), CompilationTier.Tier1);
        Assert.Contains("jmp       Single AddN(Single, Single)", ci.ToString());
    }

    public static float Ducks(float a)
    {
        Quack();
        Quack(3);
        if (a > 3)
            return Quack(3);
        else
            return Quack();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float Quack(float c)
        => 3.4f + c;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float Quack()
        => 3.5f;

    [Fact]
    public void JbeNameResolved()
    {
        var ci = CodegenInfo.Obtain(() => Ducks(3.1f), CompilationTier.Tier1);
        CodegenInfo.Obtain(() => Ducks(2.9f), CompilationTier.Tier1);
        Assert.Contains("jmp       Single Quack(Single)", ci.ToString());
        Assert.Contains("jmp       Single Quack()", ci.ToString());
    }
}
