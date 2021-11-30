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
        CodegenInfo.Obtain(() => LoopHHH(3))
            .ShouldHaveBranches(b => b >= 1)
            .ShouldHaveBranches(b => b <= 2);
    }
}
