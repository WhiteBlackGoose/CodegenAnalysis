using CodegenAssertions;
using Xunit;

namespace Tests;

public class CodegenSizeQuickJit
{
    public static int SomeMethod(int a, int b)
        => a + b;

    [Fact]
    public void Test1()
    {
        AssertCodegen.QuickJittedCodegenLessThan(20,
            typeof(CodegenSizeQuickJit).GetMethod("SomeMethod"),
            4, 5);
    }
}