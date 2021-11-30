using System.Runtime.CompilerServices;
using Xunit;

namespace CodegenAnalysis.Assertions.Tests;
public class Branches
{
    private static readonly bool True = true;

#pragma warning disable CS0162 // Unreachable code detected
    static int SmartThing()
    {
        if (True)
            return 5;
        return 10;
    }

    [Fact]
    public void BranchElimination()
    {
        CodegenInfo.Obtain(() => SmartThing())
            .ShouldHaveBranches(0);
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    static int StupidThing()
    {
        if (True)
            return 5;
        return 10;
    }
#pragma warning restore CS0162 // Unreachable code detected

    [Fact]
    public void NoBranchElimination()
    {
        CodegenInfo.Obtain(() => StupidThing(), CompilationTier.Default)
            .ShouldHaveBranches(b => b > 0);
    }
}
