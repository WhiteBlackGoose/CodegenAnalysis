using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace CodegenAnalysis;

public static class CodegenInfoResolver
{
#if !NET5_0_OR_GREATER
    internal static TValue? GetValueOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
        => dict.TryGetValue(key, out var res) ? res : default(TValue);
#endif

    internal static CodegenInfo? GetByNameAndTier(MethodBase name, CompilationTier tier)
        => EntryPointsListener.Codegens.GetValueOrDefault(name)?.SingleOrDefault(c => c.Value.Tier == tier)?.Value;

    public static CodegenInfo GetCodegenInfo(CompilationTier tier, Expr expr)
    {
        var (mi, instance, args) = ExpressionUtils.LambdaToMethodInfo(expr);
        return GetCodegenInfo(tier, mi, instance, args);
    }

    public static CodegenInfo GetCodegenInfo(CompilationTier tier, MethodInfo? mi, object? instance, params object?[] arguments)
    {
        if (mi is null)
            throw new System.ArgumentNullException(nameof(mi));
        var key = mi!;
        if (GetByNameAndTier(key, tier) is { } res)
            return res;
        if (tier is CompilationTier.Default)
        {
            mi.Invoke(instance, arguments);
            Thread.Sleep(100);
        }
        else if (tier is CompilationTier.Tier1)
        {
            var sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < 5000 && GetByNameAndTier(key, tier) is null)
            {
                for (int i = 0; i < 1000; i++)
                    mi.Invoke(instance, arguments);
            }
            return GetByNameAndTier(key, tier)
                ?? throw new RequestedTierNotFoundException(tier);
        }
        return EntryPointsListener.Codegens
                .GetValueOrDefault(key)
                ?.SingleOrDefault(c => c.Value.Tier == tier)
                ?.Value
                ?? throw new RequestedMethodNotCapturedForJittingException(mi.Name);
    }
}