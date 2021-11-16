using CodegenAssertions;
using Xunit;

namespace Tests;

public class CodegenSizeQuickJit
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
        AssertCodegen.QuickJittedCodegenLessThan(20,
            typeof(CodegenSizeQuickJit).GetMethod("SomeMethod"),
            4, 5);
    }

    [Fact]
    public void Test2()
    {
        Assert.Throws<ExpectedActualException<int>>(() =>
            AssertCodegen.QuickJittedCodegenLessThan(10,
                typeof(CodegenSizeQuickJit).GetMethod("SomeHeavyMethod"),
                4, 5)
        );
    }
}