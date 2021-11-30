using HonkSharp.Functional;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace CodegenAnalysis;

internal static class Exts
{
#if !NET5_0_OR_GREATER
    internal static TValue? GetValueOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
        => dict.TryGetValue(key, out var res) ? res : default(TValue);
#endif
}

partial record class CodegenInfo
{


    internal static CodegenInfo? GetByNameAndTier(MethodBase name, CompilationTier tier)
        => EntryPointsListener.Codegens.GetValueOrDefault(name)?.SingleOrDefault(c => c.Value.Tier == tier)?.Value;

    /// <summary>
    /// Returns codegen by the given method with the arguments passed.
    /// </summary>
    /// <param name="expr">
    /// This argument is a <see cref="System.Linq.Expressions.Expression"/>.
    /// It should be a lambda expression with valid arguments passed (those
    /// are the arguments it will be invoked to force JIT to promote it
    /// to tiers).
    /// </param>
    /// <param name="tier">
    /// Self-explanatory.
    /// </param>
    /// <example>
    /// <code>
    /// var ci = CodegenInfo.Obtain(() => MyType.MyMethod(1, 2));
    /// Console.WriteLine(ci);
    /// </code>
    /// </example>
    public static CodegenInfo Obtain(Expr expr, CompilationTier tier = CompilationTier.Tier1)
    {
        var (mi, instance, args) = ExpressionUtils.LambdaToMethodInfo(expr);
        return Obtain(tier, mi, instance, args);
    }

    /// <summary>
    /// Returns codegen by the given method with the arguments passed.
    /// </summary>
    /// <example>
    /// <code>
    /// var ci = CodegenInfo.Obtain(CompilationTier.Tier1, typeof(MyType).GetMethod("MyMethod"), null /* null for static methods */, 1, 2);
    /// Console.WriteLine(ci);
    /// </code>
    /// </example>
    public static CodegenInfo Obtain(CompilationTier tier, MethodInfo? mi, object? instance, params object?[] arguments)
        => ObtainSilent(tier, mi, instance, arguments)
            .Switch(
                ci => ci,
                ex => throw ex
            );

    /// <summary>
    /// Does not throw exceptions. See <see cref="Obtain(Expr, CompilationTier)"/>
    /// </summary>
    public static Either<CodegenInfo, Exception> ObtainSilent(CompilationTier tier, MethodInfo? mi, object? instance, object?[] arguments)
    {
        if (mi is null)
            return new ArgumentNullException(nameof(mi));
        var key = mi!;
        if (GetByNameAndTier(key, tier) is { } res)
            return res;
        if (tier is CompilationTier.Default)
        {
            try
            {
                mi.Invoke(instance, arguments);
            }
            catch (Exception e)
            {
                return e;
            }
            Thread.Sleep(100);
        }
        else if (tier is CompilationTier.Tier1)
        {
            var sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < 5000 && GetByNameAndTier(key, tier) is null)
            {
                try
                {
                    for (int i = 0; i < 1000; i++)
                        mi.Invoke(instance, arguments);
                }
                catch (Exception e)
                {
                    return e;
                }
            }
            if (GetByNameAndTier(key, tier) is { } valid)
                return valid;
            return new RequestedTierNotFoundException(tier);
        }
        var value = EntryPointsListener.Codegens
                .GetValueOrDefault(key)
                ?.SingleOrDefault(c => c.Value.Tier == tier)
                ?.Value;
        if (value is null)
            return new RequestedMethodNotCapturedForJittingException(mi.Name);
        return value;
    }
}