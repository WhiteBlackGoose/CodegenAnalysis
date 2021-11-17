using CodegenAssertions;
using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xunit;

namespace Tests;
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
        var ci = CodegenInfoResolver.GetCodegenInfo(CompilationTier.Tier1, () => Heavy(3f));
        Assert.Contains("call      Single Do1(Single)", ci.ToString());
    }

    [Fact]
    public void JmpNameResolved()
    {
        var ci = CodegenInfoResolver.GetCodegenInfo(CompilationTier.Tier1, () => Heavy(3f));
        Assert.Contains("jmp       Single AddN(Single, Single)", ci.ToString());
    }
}
