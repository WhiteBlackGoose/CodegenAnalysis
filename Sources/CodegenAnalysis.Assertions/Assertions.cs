using Iced.Intel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodegenAnalysis.Assertions;

public static partial class AssertCodegen
{
    private static void AssertFact<T>(bool fact, T expected, T actual, CodegenInfo ci, string comment)
    {
        if (!fact)
        {
            throw new ExpectedActualException<T>(expected, actual, $"{comment}\n\nCodegen:\n\n{ci}");
        }
    }

    private static void AssertFact(bool fact, CodegenInfo ci, IEnumerable<int>? problematicLines, string comment)
    {
        if (!fact)
        {
            throw new CodegenAssertionFailedException($"{comment}\n\nCodegen:\n\n{ci.ToString(problematicLines)}");
        }
    }

    public static void LessThan(int expectedLengthBytes, CompilationTier tier, Expr func)
    {
        var (mi, instance, args) = ExpressionUtils.LambdaToMethodInfo(func);
        LessThan(expectedLengthBytes, tier, mi, instance, args);
    }
    public static void LessThan(int expectedLength, CompilationTier tier, MethodInfo? mi, object? instance, params object?[] arguments)
    {
        var ci = CodegenInfoResolver.GetCodegenInfo(tier, mi, instance, arguments);
        AssertFact(ci.Bytes.Count <= expectedLength, expectedLength, ci.Bytes.Count, ci, "Expected to be smaller");
    }


    public static void NoCalls(CompilationTier tier, Expr expr)
    {
        var (mi, instance, args) = ExpressionUtils.LambdaToMethodInfo(expr);
        NoCalls(tier, mi, instance, args);
    }
    public static void NoCalls(CompilationTier tier, MethodInfo? mi, object? instance, params object?[] arguments)
    {
        HasInRange(tier, null, 0, CodegenAnalyzers.GetCalls, "calls", mi, instance, arguments);
    }


    public static void NoBranches(CompilationTier tier, Expr expr)
    {
        var (mi, instance, args) = ExpressionUtils.LambdaToMethodInfo(expr);
        NoBranches(tier, mi, instance, args);
    }
    public static void NoBranches(CompilationTier tier, MethodInfo? mi, object? instance, params object?[] arguments)
    {
        HasInRange(tier, null, 0, CodegenAnalyzers.GetBranches, "branches", mi, instance, arguments);
    }

    public static void HasCalls(CompilationTier tier, Expr expr)
    {
        var (mi, instance, args) = ExpressionUtils.LambdaToMethodInfo(expr);
        HasCalls(tier, mi, instance, args);
    }
    public static void HasCalls(CompilationTier tier, MethodInfo? mi, object? instance, params object?[] arguments)
    {
        HasInRange(tier, 1, null, CodegenAnalyzers.GetCalls, "calls", mi, instance, arguments);
    }


    public static void HasBranches(CompilationTier tier, Expr expr)
    {
        var (mi, instance, args) = ExpressionUtils.LambdaToMethodInfo(expr);
        HasBranches(tier, mi, instance, args);
    }
    public static void HasBranches(CompilationTier tier, MethodInfo? mi, object? instance, params object?[] arguments)
    {
        HasInRange(tier, 1, null, CodegenAnalyzers.GetBranches, "branches", mi, instance, arguments);
    }


    public static void HasBranchesAtLeast(int atLeast, CompilationTier tier, Expr expr)
        => HasInRange(tier, atLeast, null, CodegenAnalyzers.GetBranches, "branches", expr);

    public static void HasBranchesNoMoreThan(int upperLimit, CompilationTier tier, Expr expr)
        => HasInRange(tier, null, upperLimit, CodegenAnalyzers.GetBranches, "branches", expr);

    public static void HasCallsAtLeast(int atLeast, CompilationTier tier, Expr expr)
        => HasInRange(tier, atLeast, null, CodegenAnalyzers.GetCalls, "calls", expr);

    public static void HasCallsNoMoreThan(int upperLimit, CompilationTier tier, Expr expr)
        => HasInRange(tier, null, upperLimit, CodegenAnalyzers.GetCalls, "calls", expr);


    public static void StackAllocatesInRange(int lowerLimit, int upperLimit, CompilationTier tier, Expr expr)
        => NumberInRange(tier, lowerLimit, upperLimit, CodegenAnalyzers.GetStaticStackAllocatedMemory, "static stack allocated memory", expr);

    public static void StackAllocatesNoMoreThan(int upperLimit, CompilationTier tier, Expr expr)
        => NumberInRange(tier, 0, upperLimit, CodegenAnalyzers.GetStaticStackAllocatedMemory, "static stack allocated memory", expr);

    internal static void HasInRange(CompilationTier tier, int? from, int? to, Func<IReadOnlyList<Instruction>, IEnumerable<int>> pred, string comment, Expr expr)
    {
        var (mi, instance, args) = ExpressionUtils.LambdaToMethodInfo(expr);
        HasInRange(tier, from, to, pred, comment, mi, instance, args);
    }

    internal static void NumberInRange(CompilationTier tier, int? from, int? to, Func<IReadOnlyList<Instruction>, int?> pred, string comment, Expr expr)
    {
        var (mi, instance, args) = ExpressionUtils.LambdaToMethodInfo(expr);
        var ci = CodegenInfoResolver.GetCodegenInfo(tier, mi, instance, args);
        var message = $"Expected to contain ";
        var count = pred(ci.Instructions);
        if (from is { } aFrom)
            message += $"at least {aFrom}";
        if (from is not null && to is not null)
            message += " no more than ";
        if (to is { } aTo)
            message += aTo;
        message += $" {comment}, got {count} instead";

        AssertFact(
            (from is not { } nnFrom || count >= nnFrom)
            && (to is not { } nnTo || count <= nnTo),
            ci, null, message);
    }

    internal static void HasInRange(CompilationTier tier, int? from, int? to, Func<IReadOnlyList<Instruction>, IEnumerable<int>> pred, string comment, MethodInfo? mi, object? instance, params object?[] arguments)
    {
        var ci = CodegenInfoResolver.GetCodegenInfo(tier, mi, instance, arguments);
        var problematicLines = pred(ci.Instructions);
        var count = problematicLines.Count();
        var message = $"Expected to contain ";

        if (from is { } aFrom)
            message += $"at least {aFrom}";
        if (from is not null && to is not null)
            message += " no more than ";
        if (to is { } aTo)
            message += aTo;
        message += $" {comment}, got {count} instead";

        AssertFact(
            (from is not { } nnFrom || count >= nnFrom)
            && (to is not { } nnTo || count <= nnTo),
            ci, problematicLines, message);
    }
}