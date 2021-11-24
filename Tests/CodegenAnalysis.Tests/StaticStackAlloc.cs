
using CodegenAssertions;
using System.Runtime.InteropServices;
using Xunit;

namespace Tests;

public class StaticStackAlloc
{
    static int Add(int a, int b) => a + b;

    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public struct Aaa
    {
        [FieldOffset(0)]
        public int A;
    }

    [Fact]
    public void NoAlloc()
    {
        AssertCodegen.StackAllocatesInRange(0, 0, CompilationTier.Tier1, () => Add(3, 5));
    }

    static int AddComplicated(int a, int b)
    {
        Aaa g;
        g.A = a;
        g.A += b;
        Aaa h;
        h.A = g.A;
        return h.A + g.A;
    }

    [Fact]
    public void Alloc()
    {
        AssertCodegen.StackAllocatesInRange(32, 80, CompilationTier.Tier1, () => AddComplicated(3, 5));
    }
}
