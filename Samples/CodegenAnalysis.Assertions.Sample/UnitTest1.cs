using Xunit;

namespace CodegenAnalysis.Assertions.Sample;

public class LibraryClass
{
    public static readonly float Pi = 3.14f;

    public static bool IsPiGreaterThan3()
    {
        return Pi > 3f;
    }
}

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        CodegenInfo.Obtain(() => LibraryClass.IsPiGreaterThan3(), CompilationTier.Default)
            .ShouldContainBranches(1);
    }

    [Fact]
    public void Test2()
    {
        CodegenInfo.Obtain(() => LibraryClass.IsPiGreaterThan3(), CompilationTier.Tier1)
            .ShouldContainBranches(0);
    }
}