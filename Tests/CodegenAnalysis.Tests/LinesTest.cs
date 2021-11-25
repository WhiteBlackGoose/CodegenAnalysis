
using System.Linq;
using System.Text;
using Xunit;

namespace CodegenAnalysis.Tests;
public class LinesTest
{
    [Fact]
    public void DrawsArrows1()
    {
        var codegen =
$@"00007FFBC27D1AD0 33C0                 xor       eax,eax
00007FFBC27D1AD2 488B5118             mov       rdx,[rcx+18h]
00007FFBC27D1AD6 4C63C0               movsxd    r8,eax
00007FFBC27D1AD9 4A8D14C2             lea       rdx,[rdx+r8*8]
00007FFBC27D1ADD 4C8B4908             mov       r9,[rcx+8]
00007FFBC27D1AE1 478B04C1             mov       r8d,[r9+r8*8]
00007FFBC27D1AE5 49B878AD832250010000 mov       r8,150`2283`AD78h
00007FFBC27D1AEF 4D8B00               mov       r8,[r8]
00007FFBC27D1AF2 4D8B4008             mov       r8,[r8+8]
00007FFBC27D1AF6 4C8902               mov       [rdx],r8
00007FFBC27D1AF9 FFC0                 inc       eax
00007FFBC27D1AFB 83F878               cmp       eax,78h
00007FFBC27D1AFE 7CD2                 jl        short 0000`7FFB`C27D`1AD2h
00007FFBC27D1B00 C3                   ret";

        var expected =
$@"  00007FFBC27D1AD0 33C0                 xor       eax,eax
  00007FFBC27D1AD2 488B5118             mov       rdx,[rcx+18h]
  00007FFBC27D1AD6 4C63C0               movsxd    r8,eax
  00007FFBC27D1AD9 4A8D14C2             lea       rdx,[rdx+r8*8]
┌>00007FFBC27D1ADD 4C8B4908             mov       r9,[rcx+8]
│ 00007FFBC27D1AE1 478B04C1             mov       r8d,[r9+r8*8]
│ 00007FFBC27D1AE5 49B878AD832250010000 mov       r8,150`2283`AD78h
│ 00007FFBC27D1AEF 4D8B00               mov       r8,[r8]
│ 00007FFBC27D1AF2 4D8B4008             mov       r8,[r8+8]
│ 00007FFBC27D1AF6 4C8902               mov       [rdx],r8
│ 00007FFBC27D1AF9 FFC0                 inc       eax
└─00007FFBC27D1AFB 83F878               cmp       eax,78h
  00007FFBC27D1AFE 7CD2                 jl        short 0000`7FFB`C27D`1AD2h
  00007FFBC27D1B00 C3                   ret";

        var sbs = codegen.Replace("\r", "").Split('\n').Select(c => new StringBuilder(c)).ToList();
        var lines = new Lines(sbs);
        var actual = lines.DrawArrows(new [] { ( From: 11, To: 4 ) }).ToString();
        Assert.Equal<object>(expected.Replace("\r", ""), actual);
    }

    [Fact]
    public void DrawsArrows2()
    {
        var codegen =
$@"00007FFBC27D1AD0 33C0                 xor       eax,eax
00007FFBC27D1AD2 488B5118             mov       rdx,[rcx+18h]
00007FFBC27D1AD6 4C63C0               movsxd    r8,eax
00007FFBC27D1AD9 4A8D14C2             lea       rdx,[rdx+r8*8]
00007FFBC27D1ADD 4C8B4908             mov       r9,[rcx+8]
00007FFBC27D1AE1 478B04C1             mov       r8d,[r9+r8*8]
00007FFBC27D1AE5 49B878AD832250010000 mov       r8,150`2283`AD78h
00007FFBC27D1AEF 4D8B00               mov       r8,[r8]
00007FFBC27D1AF2 4D8B4008             mov       r8,[r8+8]
00007FFBC27D1AF6 4C8902               mov       [rdx],r8
00007FFBC27D1AF9 FFC0                 inc       eax
00007FFBC27D1AFB 83F878               cmp       eax,78h
00007FFBC27D1AFE 7CD2                 jl        short 0000`7FFB`C27D`1AD2h
00007FFBC27D1B00 C3                   ret";

        var expected =
$@"   00007FFBC27D1AD0 33C0                 xor       eax,eax
   00007FFBC27D1AD2 488B5118             mov       rdx,[rcx+18h]
   00007FFBC27D1AD6 4C63C0               movsxd    r8,eax
   00007FFBC27D1AD9 4A8D14C2             lea       rdx,[rdx+r8*8]
 ┌>00007FFBC27D1ADD 4C8B4908             mov       r9,[rcx+8]
 │ 00007FFBC27D1AE1 478B04C1             mov       r8d,[r9+r8*8]
 │ 00007FFBC27D1AE5 49B878AD832250010000 mov       r8,150`2283`AD78h
 │ 00007FFBC27D1AEF 4D8B00               mov       r8,[r8]
┌┼─00007FFBC27D1AF2 4D8B4008             mov       r8,[r8+8]
││ 00007FFBC27D1AF6 4C8902               mov       [rdx],r8
│└─00007FFBC27D1AF9 FFC0                 inc       eax
│  00007FFBC27D1AFB 83F878               cmp       eax,78h
└─>00007FFBC27D1AFE 7CD2                 jl        short 0000`7FFB`C27D`1AD2h
   00007FFBC27D1B00 C3                   ret";

        var sbs = codegen.Replace("\r", "").Split('\n').Select(c => new StringBuilder(c)).ToList();
        var lines = new Lines(sbs);
        var actual = lines.DrawArrows(new[] { (From: 10, To: 4), (From: 8, To: 12) }).ToString();
        Assert.Equal<object>(expected.Replace("\r", ""), actual);
    }
}
