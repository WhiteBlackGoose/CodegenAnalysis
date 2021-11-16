## CodegenAssertions

Test library for verifying the expected characteristics of codegen.

### Examples

```cs
using CodegenAssertions;
using Xunit;

public class CodegenSizeQuickJit
{
    public static int SomeMethod(int a, int b)
        => a + b;

    [Fact]
    public void Test1()
    {
        AssertCodegen.QuickJittedCodegenLessThan(20, () => SomeMethod(4, 5));
    }

    [Fact]
    public void Test2()
    {
        AssertCodegen.QuickJittedCodegenDoesNotHaveCalls(() => SomeMethod(4, 5))
    }
}
```
