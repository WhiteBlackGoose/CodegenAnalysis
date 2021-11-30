using Xunit;

namespace CodegenAnalysis.Assertions.Tests;

public class CodegenSize
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
    public void Test1()
    {
        CodegenInfo.Obtain(() => SomeMethod(4, 5), CompilationTier.Tier1)
            .ShouldBeNotLargerThan(20);
    }

    [Fact]
    public void Test2()
    {
        Assert.Throws<CodegenAssertionFailedException>(() =>
            CodegenInfo.Obtain(() => SomeHeavyMethod(4, 5), CompilationTier.Tier1)
                .ShouldBeNotLargerThan(10)
        );
    }

}