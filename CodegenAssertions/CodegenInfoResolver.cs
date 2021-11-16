using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace CodegenAssertions;

internal static class CodegenInfoResolver
{
    public static IEnumerable<CodegenInfo> GetCodegenInfo(MethodInfo mi, params object[] arguments)
    {
        var key = $"{mi.DeclaringType.Name}.{mi.Name}";
        if (EntryPointsListener.Codegens.TryGetValue(key, out var res))
            return res;
        mi.Invoke(null, arguments);
        mi.Invoke(null, arguments);
        mi.Invoke(null, arguments);
        mi.Invoke(null, arguments);
        Thread.Sleep(10000);
        return EntryPointsListener.Codegens.TryGetValue(key, out res)
            ? res 
            : throw new RequestedMethodNotCapturedForJittingException(mi.Name);
    }
}