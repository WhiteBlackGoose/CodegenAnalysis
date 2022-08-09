using Xunit;

namespace CodegenAnalysis.Assertions.Sample;

public class LibraryClass
{
    public static readonly float Pi = 3.14f;

    public static bool IsPiGreaterThan3()
    {
        return Pi > 3f;
    }

    public static decimal RandomComputations()
    {
        decimal a1 = 1; decimal a2 = 2; decimal a3 = 3;
        a1 = a2 + a3; a2 = a1 + a3; a3 = a1 + a2 + a3; a2 = a1 + a3;
        return a1 / a2;
    }

    public static float Cbr(float g)
    {
        return g * g * g;
    }
}

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        CodegenInfo.Obtain(() => LibraryClass.IsPiGreaterThan3(), CompilationTier.Default)
            .ShouldHaveCalls(1);
    }

    [Fact]
    public void Test2()
    {
        CodegenInfo.Obtain(() => LibraryClass.IsPiGreaterThan3(), CompilationTier.Tier1)
            .ShouldHaveCalls(0)
            .ShouldHaveBranches(0);
    }

    [Fact]
    public void Test3()
    {
        CodegenInfo.Obtain(() => LibraryClass.RandomComputations(), CompilationTier.Tier1)
            .ShouldStaticStackAllocateNoMoreThan(32); // should fail
    }

    [Fact]
    public void Test4()
    {
        CodegenInfo.Obtain(() => LibraryClass.Cbr(3f), CompilationTier.Tier1)
            .ShouldBeOfSize(size => size < 16)
            .ShouldStaticStackAllocateNoMoreThan(0);
    }
}