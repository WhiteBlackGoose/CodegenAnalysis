using Xunit;

namespace CodegenAnalysis.Assertions.Tests;

public class Calls
{
    public static int SomeMethod(int a, int b)
        => a + b;

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

    [Fact]
    public void Test3()
    {
        CodegenInfo.Obtain(() => SomeMethod(4, 5), CompilationTier.Default)
            .ShouldHaveCalls(0);
    }

    [Fact]
    public void Test4()
    {
        Assert.Throws<CodegenAssertionFailedException>(() =>
            CodegenInfo.Obtain(() => SomeHeavyMethod(4, 5), CompilationTier.Default)
                .ShouldHaveCalls(0)
        );
    }

    public class A
    {
        public virtual int H => 3;
    }

    public sealed class B : A
    {
        public override int H => 6;
    }

    static int Twice(B b) => b.H * 2;

    [Fact]
    public void NotDevirtTier0()
    {
        CodegenInfo.Obtain(() => Twice(new B()), CompilationTier.Default)
            .ShouldHaveCalls(c => c >= 0)
            .ShouldHaveCalls(c => c >= 1);
        Assert.Throws<CodegenAssertionFailedException>(() =>
            CodegenInfo.Obtain(() => Twice(new B()), CompilationTier.Default)
                .ShouldHaveCalls(c => c >= 2)
        );
    }

    [Fact]
    public void DevirtTier1()
    {
        CodegenInfo.Obtain(() => Twice(new B()), CompilationTier.Tier1)
            .ShouldHaveCalls(0);
    }
}
