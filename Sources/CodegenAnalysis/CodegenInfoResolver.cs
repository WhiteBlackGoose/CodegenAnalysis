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

    public static CodegenInfo Obtain(Expr expr, CompilationTier tier = CompilationTier.Tier1)
    {
        var (mi, instance, args) = ExpressionUtils.LambdaToMethodInfo(expr);
        return Obtain(tier, mi, instance, args);
    }

    public static CodegenInfo Obtain(CompilationTier tier, MethodInfo? mi, object? instance, params object?[] arguments)
        => ObtainSilent(tier, mi, instance, arguments)
            .Switch(
                ci => ci,
                ex => throw ex
            );

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