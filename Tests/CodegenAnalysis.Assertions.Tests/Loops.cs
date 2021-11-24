using Xunit;

namespace CodegenAnalysis.Assertions.Tests;
public class Loops
{
    public static int LoopHHH(int a)
    {
        var res = 0d;
        for (int i = 0; i < a; i++)
            res += a;
        return (int)res;
    }


    [Fact]
    public void LoopsGetTier1()
    {
        AssertCodegen.HasBranchesAtLeast(1, CompilationTier.Tier1, () => LoopHHH(3));
        AssertCodegen.HasBranchesNoMoreThan(2, CompilationTier.Tier1, () => LoopHHH(3));
    }
}
