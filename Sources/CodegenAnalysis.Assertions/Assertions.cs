using Iced.Intel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CodegenAnalysis.Assertions;

#if !NET5_0_OR_GREATER
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute(string argName) { }
}
#endif

public static partial class AssertCodegen
{
    private static CodegenInfo Should(this CodegenInfo ci, string msg, Func<CodegenInfo, bool> fact, Func<CodegenInfo, string> prettifyOnFailure)
    {
        if (fact(ci))
            return ci;
        throw new CodegenAssertionFailedException($"Failed. {msg}\n\nCodegen:\n{prettifyOnFailure(ci)}");
    }


    public static CodegenInfo ShouldHaveBranches(this CodegenInfo ci, Func<IReadOnlyList<int>, bool> predicate, [CallerArgumentExpression("predicate")] string expr = "")
        => ci.Should($"Expected specific branches ({expr}). Got {ci.Branches.Count} branches instead.", ci => predicate(ci.Branches), ci => ci.ToLines().Add(">>>", ci.Branches).ToString());
    public static CodegenInfo ShouldHaveBranches(this CodegenInfo ci, Func<int, bool> predicate, [CallerArgumentExpression("predicate")] string expr = "")
        => ci.ShouldHaveBranches(list => predicate(list.Count), expr);
    public static CodegenInfo ShouldHaveBranches(this CodegenInfo ci, int amount, [CallerArgumentExpression("amount")] string expr = "")
        => ci.ShouldHaveBranches(i => i.Count == amount, expr);

    public static CodegenInfo ShouldHaveCalls(this CodegenInfo ci, Func<IReadOnlyList<int>, bool> predicate, [CallerArgumentExpression("predicate")] string expr = "")
        => ci.Should($"Expected specific calls ({expr}). Got {ci.Calls.Count} calls instead.", ci => predicate(ci.Calls), ci => ci.ToLines().Add(">>>", ci.Calls).ToString());
    public static CodegenInfo ShouldHaveCalls(this CodegenInfo ci, Func<int, bool> predicate, [CallerArgumentExpression("predicate")] string expr = "")
        => ci.ShouldHaveCalls(list => predicate(list.Count), expr);
    public static CodegenInfo ShouldHaveCalls(this CodegenInfo ci, int amount, [CallerArgumentExpression("amount")] string expr = "")
        => ci.ShouldHaveCalls(i => i.Count == amount, expr);


    public static CodegenInfo ShouldStaticStackAllocate(this CodegenInfo ci, Func<int?, bool> predicate, [CallerArgumentExpression("predicate")] string expr = "")
        => ci.Should($"Expected specific stack allocated size ({expr}). Got {ci.StaticStackAllocatedMemory} bytes instead.", ci => predicate(ci.StaticStackAllocatedMemory), ci => ci.ToString());

    public static CodegenInfo ShouldStaticStackAllocateNoMoreThan(this CodegenInfo ci, int byteSizeUpperLimit, [CallerArgumentExpression("byteSizeUpperLimit")] string expr = "")
        => ci.ShouldStaticStackAllocate(i => i is null || i <= byteSizeUpperLimit, expr);


    public static CodegenInfo ShouldBeOfSize(this CodegenInfo ci, Func<int, bool> predicate, [CallerArgumentExpression("predicate")] string expr = "")
        => ci.Should($"Expected codegen of specific size ({expr}). It is {ci.Size} bytes instead.", ci => predicate(ci.Size), ci => ci.ToString());

    public static CodegenInfo ShouldBeNotLargerThan(this CodegenInfo ci, int byteSizeUpperLimit, [CallerArgumentExpression("byteSizeUpperLimit")] string expr = "")
        => ci.ShouldBeOfSize(b => b <= byteSizeUpperLimit, expr);
}