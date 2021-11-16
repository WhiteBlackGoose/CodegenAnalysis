using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace CodegenAssertions;

internal static class CodegenInfoResolver
{
    private static CodegenInfo? GetByNameAndTier(string name, OptimizationTier tier)
        => EntryPointsListener.Codegens.GetValueOrDefault(name)?.SingleOrDefault(c => c.Value.Tier == tier)?.Value;

    public static CodegenInfo GetCodegenInfo(OptimizationTier tier, MethodInfo? mi, params object[] arguments)
    {
        System.ArgumentNullException.ThrowIfNull(mi);
        var key = $"{mi.DeclaringType?.FullName}.{mi.Name}";
        if (GetByNameAndTier(key, tier) is { } res)
            return res;
        if (tier is OptimizationTier.QuickJitted)
        {
            mi.Invoke(null, arguments);
            Thread.Sleep(100);
        }
        else if (tier is OptimizationTier.OptimizedTier1)
        {
            var sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < 10_000 && GetByNameAndTier(key, tier) is null)
            {
                for (int i = 0; i < 1000; i++)
                    mi.Invoke(null, arguments);
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