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
        AssertCodegen.CodegenLessThan(20, CompilationTier.Tier0, () => SomeMethod(4, 5));
    }

    [Fact]
    public void Test2()
    {
        Assert.Throws<ExpectedActualException<int>>(() =>
            AssertCodegen.CodegenLessThan(10, CompilationTier.Tier0, () => SomeHeavyMethod(4, 5))
        );
    }

    [Fact]
    public void Test3()
    {
        AssertCodegen.CodegenDoesNotHaveCalls(CompilationTier.Tier0 , () => SomeMethod(4, 5));
    }

    [Fact]
    public void Test4()
    {
        Assert.Throws<CodegenAssertionFailedException>(() =>
            AssertCodegen.CodegenDoesNotHaveCalls(CompilationTier.Tier0, () => SomeHeavyMethod(4, 5))
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
        AssertCodegen.CodegenHasCalls(CompilationTier.Tier0, () => Twice(new B()));
    }

    [Fact]
    public void DevirtTier1()
    {
        AssertCodegen.CodegenDoesNotHaveCalls(CompilationTier.Tier1, () => Twice(new B()));
    }
}