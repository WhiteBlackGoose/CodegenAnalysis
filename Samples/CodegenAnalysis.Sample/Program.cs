using CodegenAnalysis;
using System;

var ci1 = CodegenInfo.Obtain(() => LibraryClass.Cbr(5.5f));
Console.WriteLine("Cbr codegen:");
Console.WriteLine(ci1);

var ci2 = CodegenInfo.Obtain(() => LibraryClass.Sum(12));
Console.WriteLine("\n\nSum codegen:");
Console.WriteLine(ci2);

var lines1 = ci2.ToLines();
Console.WriteLine("\n\nSum codegen with visualization of loops:");
lines1.DrawArrows(CodegenAnalyzers.GetBackwardJumps(ci2.Instructions));
Console.WriteLine(lines1);

var lines2 = ci2.ToLines();
Console.WriteLine("\n\nSum codegen with visualization of ifs:");
lines2.DrawArrows(CodegenAnalyzers.GetJumps(ci2.Instructions));
Console.WriteLine(lines2);

var lines3 = ci2.ToLines();
Console.WriteLine("\n\nSum codegen with visualization of ifs and loops:");
lines3.DrawArrows(CodegenAnalyzers.GetBackwardJumps(ci2.Instructions));
lines3.DrawArrows(CodegenAnalyzers.GetJumps(ci2.Instructions));
Console.WriteLine(lines3);


public static class LibraryClass
{
    public static float Cbr(float a)
    {
        return a * a * a;
    }

    public static float Sum(int count)
    {
        var a = 0;
        for (int i = 0; i < count; i++)
            a += i % 2 == 0 ? i : 3;
        return a;
    }
}