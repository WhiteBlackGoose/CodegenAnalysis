using CodegenAssertions;
using System.Runtime.CompilerServices;
using Xunit;

namespace Tests;

public class CodegenSizeQuickJit
{
    public static int SomeMethod(int a, int b)
        => a + b;

    public static int SomeHeavyMethod(int a, int b)
    {
        a += b;
        b += a;
        b += a / b;
        b += b / a;
        a *= a;
        if (a < 0) a = 5;
        if (a < -5)
            throw new();
        return a - b;
    }

    [Fact]
    public void Test1()
    {
        AssertCodegen.CodegenLessThan(20, CompilationTier.Tier1, () => SomeMethod(4, 5));
    }

    [Fact]
    public void Test2()
    {
        Assert.Throws<ExpectedActualException<int>>(() =>
            AssertCodegen.CodegenLessThan(10, CompilationTier.Default, () => SomeHeavyMethod(4, 5))
        );
    }

    [Fact]
    public void Test3()
    {
        AssertCodegen.CodegenDoesNotHaveCalls(CompilationTier.Default , () => SomeMethod(4, 5));
    }

    [Fact]
    public void Test4()
    {
        Assert.Throws<CodegenAssertionFailedException>(() =>
            AssertCodegen.CodegenDoesNotHaveCalls(CompilationTier.Default, () => SomeHeavyMethod(4, 5))
        );
    }

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
        AssertCodegen.CodegenHasCalls(CompilationTier.Default, () => Twice(new B()));
    }

    [Fact]
    public void DevirtTier1()
    {
        AssertCodegen.CodegenDoesNotHaveCalls(CompilationTier.Tier1, () => Twice(new B()));
    }

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
        AssertCodegen.CodegenDoesNotHaveBranches(CompilationTier.Tier1, () => SmartThing());
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
        AssertCodegen.CodegenHasBranches(CompilationTier.Default, () => StupidThing());
    }
}