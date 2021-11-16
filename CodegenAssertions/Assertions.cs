using Iced.Intel;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;
using Expr = System.Linq.Expressions.Expression<System.Action>;

namespace CodegenAssertions;

public enum CompilationTier
{
    Tier0,
    Tier1
}

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

    public static void CodegenLessThan(int expectedLength, CompilationTier tier, Expr func)
    {
        var (mi, args) = ExpressionUtils.LambdaToMethodInfo(func);
        CodegenLessThan(expectedLength, tier, mi, args);
    }

    public static void CodegenLessThan(int expectedLength, CompilationTier tier, MethodInfo? mi, params object?[] arguments)
    {
        var ci = CodegenInfoResolver.GetCodegenInfo(tier.ToInternalOT(), mi, arguments);
        AssertFact(ci.Bytes.Length <= expectedLength, expectedLength, ci.Bytes.Length, ci, "The method was expected to be smaller");
    }

    public static void CodegenDoesNotHaveCalls(CompilationTier tier, Expr expr)
    {
        var (mi, args) = ExpressionUtils.LambdaToMethodInfo(expr);
        CodegenDoesNotHaveCalls(tier, mi, args);
    }

    public static void CodegenDoesNotHaveCalls(CompilationTier tier, MethodInfo? mi, params object?[] arguments)
    {
        CodegenDoesNotHave(tier.ToInternalOT(), i => i.Code.ToString().StartsWith("Call"), "calls", mi, arguments);
    }

    internal static void CodegenDoesNotHave(InternalOptimizationTier tier, Func<Instruction, bool> pred, string comment, MethodInfo? mi, params object?[] arguments)
    {
        var ci = CodegenInfoResolver.GetCodegenInfo(tier, mi, arguments);
        AssertFact(
            !ci
            .Instructions
            .Any(pred), ci, $"It was supposed not to contain {comment}");
    }

    public static void CodegenHasCalls(CompilationTier tier, Expr expr)
    {
        var (mi, args) = ExpressionUtils.LambdaToMethodInfo(expr);
        CodegenHasCalls(tier, mi, args);
    }

    public static void CodegenHasCalls(CompilationTier tier, MethodInfo? mi, params object?[] arguments)
    {
        CodegenHas(tier.ToInternalOT(), i => i.Code.ToString().StartsWith("Call"), "calls", mi, arguments);
    }

    internal static void CodegenHas(InternalOptimizationTier tier, Func<Instruction, bool> pred, string comment, MethodInfo? mi, params object?[] arguments)
    {
        var ci = CodegenInfoResolver.GetCodegenInfo(tier, mi, arguments);
        AssertFact(
            ci
            .Instructions
            .Any(pred), ci, $"It was supposed to contain {comment}");
    }
}