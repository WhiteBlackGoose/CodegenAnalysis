using Iced.Intel;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;
using Expr = System.Linq.Expressions.Expression<System.Action>;

namespace CodegenAssertions;



public static partial class AssertCodegen
{
    private static void AssertFact<T>(bool fact, T expected, T actual, CodegenInfo ci, string comment)
    {
        if (!fact)
        {
            throw new ExpectedActualException<T>(expected, actual, $"{comment}\n\nCodegen:\n\n{ci}");
        }
    }

    private static void AssertFact(bool fact, CodegenInfo ci, string comment)
    {
        if (!fact)
        {
            throw new CodegenAssertionFailedException($"{comment}\n\nCodegen:\n\n{ci}");
        }
    }

    public static void QuickJittedCodegenLessThan(int expectedLength, Expr func)
    {
        var (mi, args) = ExpressionUtils.LambdaToMethodInfo(func);
        QuickJittedCodegenLessThan(expectedLength, mi, args);
    }

    public static void QuickJittedCodegenLessThan(int expectedLength, MethodInfo? mi, params object?[] arguments)
    {
        var ci = CodegenInfoResolver.GetCodegenInfo(OptimizationTier.QuickJitted, mi, arguments);
        AssertFact(ci.Bytes.Length <= expectedLength, expectedLength, ci.Bytes.Length, ci, "The method was expected to be smaller");
    }

    public static void QuickJittedCodegenDoesNotHaveCalls(Expr expr)
    {
        var (mi, args) = ExpressionUtils.LambdaToMethodInfo(expr);
        QuickJittedCodegenDoesNotHaveCalls(mi, args);
    }

    public static void QuickJittedCodegenDoesNotHaveCalls(MethodInfo? mi, params object?[] arguments)
    {
        CodegenDoesNotHave(OptimizationTier.QuickJitted, i => i.Code.ToString().StartsWith("Call"), "calls", mi, arguments);
    }


    internal static void CodegenDoesNotHave(OptimizationTier tier, Func<Instruction, bool> pred, string comment, MethodInfo? mi, params object?[] arguments)
    {
        var ci = CodegenInfoResolver.GetCodegenInfo(tier, mi, arguments);
        AssertFact(
            !ci
            .Instructions
            .Any(pred), ci, $"It was supposed not to contain {comment}");
    }
}