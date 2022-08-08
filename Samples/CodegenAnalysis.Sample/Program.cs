using CodegenAnalysis;
using System;

var ci1 = CodegenInfo.Obtain(() => LibraryClass.Cbr(5.5f));
Console.WriteLine("Cbr codegen:");
Console.WriteLine(ci1);

var ci2 = CodegenInfo.Obtain(() => LibraryClass.Sum(12));
Console.WriteLine("\n\nSum codegen:");
Console.WriteLine(ci2);

Console.WriteLine("\n\nSum codegen with visualization:");
Console.WriteLine(ci2.ToString(true));


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