# CodegenAssertions

Test library for verifying the expected characteristics of codegen.

## Examples

### Codegen size

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
}
```


### Having calls in the codegen

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

    // this will get devirtualized at tier1, but not at tier0
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

### Testing if we have branches

```cs
    private static readonly bool True = true;

    static int SmartThing()
    {
        if (True)
            return 5;
        return 10;
    }

    [Fact]
    public void BranchElimination()
    {
        AssertCodegen.CodegenDoesNotHaveBranches(CompilationTier.Tier1, () => SmartThing());
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    static int StupidThing()
    {
        if (True)
            return 5;
        return 10;
    }

    [Fact]
    public void NoBranchElimination()
    {
        AssertCodegen.CodegenHasBranches(CompilationTier.Default, () => StupidThing());
    }
```