# CodegenAssertions

Test library for verifying the expected characteristics of codegen.

## Examples

### Naive tests

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
        AssertCodegen.CodegenLessThan(20, CompilationTier.Tier0, () => SomeMethod(4, 5));
    }

    [Fact]
    public void Test2()
    {
        AssertCodegen.CodegenDoesNotHaveCalls(CompilationTier.Tier0, () => SomeMethod(4, 5))
    }
}
```


### Testing .NET 6's devirtualization

```cs
public class Tests
{
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
```
