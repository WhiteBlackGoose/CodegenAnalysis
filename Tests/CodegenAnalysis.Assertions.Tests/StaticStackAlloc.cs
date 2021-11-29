using System.Runtime.InteropServices;
using Xunit;

namespace CodegenAnalysis.Assertions.Tests;

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
        CodegenInfo.Obtain(() => Add(3, 5))
            .ShouldStaticStackAllocate(s => s is null or 0);
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
        CodegenInfo.Obtain(() => AddComplicated(3, 5))
            .ShouldStaticStackAllocate(s => s is >= 32 and <= 80);
    }
}
