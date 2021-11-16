using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace CodegenAssertions;

public static partial class AssertCodegen
{
    public static void QuickJittedCodegenLessThan(int expectedLength, MethodInfo mi, params object[] arguments)
    {
        var ci = CodegenInfoResolver.GetCodegenInfo(mi, arguments);
        var quickJit = ci.FirstOrDefault(c => c.Tier == OptimizationTier.QuickJitted) 
                ?? throw new RequestedTierNotFoundException(OptimizationTier.QuickJitted);
        if (quickJit.Bytes.Length > expectedLength)
            throw new ExpectedActualException<int>(expectedLength, quickJit.Bytes.Length, $"The method is expected to be smaller. Codegen:\n{quickJit}");
    }
}