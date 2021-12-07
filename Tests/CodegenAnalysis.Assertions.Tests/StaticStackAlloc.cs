using System.Runtime.CompilerServices;
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


    [StructLayout(LayoutKind.Explicit, Size = 32973)]
    public struct BigStruct
    {
        
    }

    [SkipLocalsInit]
    public static unsafe int BigSizeAlloc()
    {
        BigStruct meh;
        var s = 0;
        for (int i = 0; i < sizeof(BigStruct); i++)
            s += ((byte*)&meh)[i];
        return s;
    }

    [Fact]
    public void BigSizeAllocTest()
    {
        CodegenInfo.Obtain(() => BigSizeAlloc(), CompilationTier.Default)
            .ShouldStaticStackAllocate(s => s is >= 30000 and <= 35000);
    }
}
