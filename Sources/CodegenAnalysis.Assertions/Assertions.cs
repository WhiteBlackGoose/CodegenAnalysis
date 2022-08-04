using Iced.Intel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CodegenAnalysis.Assertions;

#if !NET5_0_OR_GREATER
[AttributeUsage(AttributeTargets.Parameter)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute(string argName) { }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#endif

/// <summary>
/// Contains extension methods for your tests.
/// </summary>
/// <example>
/// <code>
/// public class Tests
/// {
///     public class A
///     {
///         public virtual int H => 3;
///     }
/// 
///     public sealed class B : A
///     {
///         public override int H => 6;
///     }
/// 
///     // this will get devirtualized at tier1, but not at tier0
///     static int Twice(B b) => b.H * 2;
/// 
///     [Fact]
///     public void NotDevirtTier0()
///     {
///         CodegenInfo.Obtain(() => Twice(new B()), CompilationTier.Default)
///             .ShouldHaveCalls(c => c >= 1)
///             .ShouldHaveBranches(0);
///     }
/// 
///     [Fact]
///     public void DevirtTier1()
///     {
///         CodegenInfo.Obtain(() => Twice(new B()), CompilationTier.Tier1)
///             .ShouldHaveCalls(0)
///             .ShouldStaticStackAllocateNoMoreThan(10 /* max bytes to be in stack */);
///     }
/// }
/// </code>
/// </example>
public static partial class AssertCodegen
{
    private static CodegenInfo Should(this CodegenInfo ci, string msg, Func<CodegenInfo, bool> fact, Func<CodegenInfo, string> prettifyOnFailure)
    {
        if (fact(ci))
            return ci;
        throw new CodegenAssertionFailedException($"Failed. {msg}\n\nCodegen:\n{prettifyOnFailure(ci)}");
    }


    /// <summary>
    /// Check if the readonly list of branches matches your expectation.
    /// </summary>
    /// <returns>The instance. Use it for fluent assertions.</returns>
    public static CodegenInfo ShouldHaveBranches(this CodegenInfo ci, Func<IReadOnlyList<int>, bool> predicate, [CallerArgumentExpression("predicate")] string expr = "")
        => ci.Should($"Expected specific branches ({expr}). Got {ci.Branches.Count} branches instead.", ci => predicate(ci.Branches), ci => ci.ToLines().Add(">>>", ci.Branches).ToString());

    /// <summary>
    /// Check if the number of branches matches your expectation.
    /// </summary>
    /// <returns>The instance. Use it for fluent assertions.</returns>
    public static CodegenInfo ShouldHaveBranches(this CodegenInfo ci, Func<int, bool> predicate, [CallerArgumentExpression("predicate")] string expr = "")
        => ci.ShouldHaveBranches(list => predicate(list.Count), expr);

    /// <summary>
    /// Check if there is as many branches as expected.
    /// </summary>
    /// <returns>The instance. Use it for fluent assertions.</returns>
    public static CodegenInfo ShouldHaveBranches(this CodegenInfo ci, int amount, [CallerArgumentExpression("amount")] string expr = "")
        => ci.ShouldHaveBranches(i => i.Count == amount, expr);



    /// <summary>
    /// Check if the readonly list of calls matches your expectation.
    /// </summary>
    /// <returns>The instance. Use it for fluent assertions.</returns>
    public static CodegenInfo ShouldHaveCalls(this CodegenInfo ci, Func<IReadOnlyList<int>, bool> predicate, [CallerArgumentExpression("predicate")] string expr = "")
        => ci.Should($"Expected specific calls ({expr}). Got {ci.Calls.Count} calls instead.", ci => predicate(ci.Calls), ci => ci.ToLines().Add(">>>", ci.Calls).ToString());

    /// <summary>
    /// Check if the number of calls matches your expectation.
    /// </summary>
    /// <returns>The instance. Use it for fluent assertions.</returns>
    public static CodegenInfo ShouldHaveCalls(this CodegenInfo ci, Func<int, bool> predicate, [CallerArgumentExpression("predicate")] string expr = "")
        => ci.ShouldHaveCalls(list => predicate(list.Count), expr);

    /// <summary>
    /// Check if there is as many branches as expected.
    /// </summary>
    /// <returns>The instance. Use it for fluent assertions.</returns>
    public static CodegenInfo ShouldHaveCalls(this CodegenInfo ci, int amount, [CallerArgumentExpression("amount")] string expr = "")
        => ci.ShouldHaveCalls(i => i.Count == amount, expr);


    /// <summary>
    /// Check if the amount of statically stack allocated memory matches your expectation.
    /// It is computed based on the bump stack pointer instructions. In case if the amount of memory
    /// allocated cannot be determined, the predicate expects a null. Note, that it excludes
    /// dynamically allocated - that is, all inner calls (and their stack allocations) as well
    /// as anything allocated with <see langword="stackalloc"/>.
    /// </summary>
    /// <returns>The instance. Use it for fluent assertions.</returns>
    public static CodegenInfo ShouldStaticStackAllocate(this CodegenInfo ci, Func<int?, bool> predicate, [CallerArgumentExpression("predicate")] string expr = "")
        => ci.Should($"Expected specific stack allocated size ({expr}). Got {ci.StaticStackAllocatedMemory} bytes instead.", ci => predicate(ci.StaticStackAllocatedMemory), ci => ci.ToString());

    /// <summary>
    /// Check if the amount of statically stack allocated memory is no more than the expected size.
    /// It is computed based on the bump stack pointer instructions. In case if the amount of memory
    /// allocated cannot be determined, the predicate expects a null.  Note, that it excludes
    /// dynamically allocated - that is, all inner calls (and their stack allocations) as well
    /// as anything allocated with <see langword="stackalloc"/>.
    /// </summary>
    /// <returns>The instance. Use it for fluent assertions.</returns>
    public static CodegenInfo ShouldStaticStackAllocateNoMoreThan(this CodegenInfo ci, int byteSizeUpperLimit, [CallerArgumentExpression("byteSizeUpperLimit")] string expr = "")
        => ci.ShouldStaticStackAllocate(i => i is null || i <= byteSizeUpperLimit, expr);


    /// <summary>
    /// Checks if the size of the codegen (the set of instructions) (in bytes)
    /// matches your expectation.
    /// </summary>
    /// <returns>The instance. Use it for fluent assertions.</returns>
    public static CodegenInfo ShouldBeOfSize(this CodegenInfo ci, Func<int, bool> predicate, [CallerArgumentExpression("predicate")] string expr = "")
        => ci.Should($"Expected codegen of specific size ({expr}). It is {ci.Size} bytes instead.", ci => predicate(ci.Size), ci => ci.ToString());

    /// <summary>
    /// Checks if the size of the codegen (the set of instructions) (in bytes)
    /// is not larger than the expected upper limit.
    /// </summary>
    /// <returns>The instance. Use it for fluent assertions.</returns>
    public static CodegenInfo ShouldBeNotLargerThan(this CodegenInfo ci, int byteSizeUpperLimit, [CallerArgumentExpression("byteSizeUpperLimit")] string expr = "")
        => ci.ShouldBeOfSize(b => b <= byteSizeUpperLimit, expr);
}