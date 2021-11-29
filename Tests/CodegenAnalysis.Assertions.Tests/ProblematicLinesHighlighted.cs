using System;
using System.Text.RegularExpressions;
using Xunit;

namespace CodegenAnalysis.Assertions.Tests;

public class ProblematicLinesHighlighted
{
    static void Ducks()
    {
        Console.WriteLine(); // call
    }

    [Fact]
    public static void CallsHighlighted()
    {
        try
        {
            CodegenInfo.Obtain(() => Ducks(), CompilationTier.Default)
                .ShouldHaveCalls(0);
            Assert.True(false, "Expected to throw");
        }
        catch (CodegenAssertionFailedException e)
        {
            var regex = new Regex(@">>>[\dA-z]{16} [\dA-z]* *call");
            Assert.True(regex.Match(e.Message).Success, e.Message);
        }
    }
}
