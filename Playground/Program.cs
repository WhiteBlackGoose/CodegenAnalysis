// See https://aka.ms/new-console-template for more information
using CodegenAssertions;
using System.Linq.Expressions;
using System.Reflection;


var codegenInfo = CodegenInfoResolver.GetCodegenInfo(CompilationTier.Tier1, () => A.Add(3, 5));
Console.WriteLine(codegenInfo);

class A
{
    public static int Add(int a, int b) => a + b * a;
}