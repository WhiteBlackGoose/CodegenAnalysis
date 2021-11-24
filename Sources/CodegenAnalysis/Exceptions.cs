using System;

namespace CodegenAnalysis;

public sealed class RequestedTierNotFoundException : Exception
{
    internal RequestedTierNotFoundException(CompilationTier tier)
        : base($"Tier {tier} not found. Try toggling the build configuration to Release. Make sure that "
            + tier switch
            {
                CompilationTier.Tier1 => "the method is not annotated with NoOptimization",
                CompilationTier.Default => "the method is not annotated with AggressiveOptimization",
                _ => throw new("Um, oops")
            }
        ) { }
}

public sealed class RequestedMethodNotCapturedForJittingException : Exception
{
    internal RequestedMethodNotCapturedForJittingException(string method)
        : base($"Method {method} wasn't JIT-ted or JIT-ted too early. Make sure you don't run it before the test.") { }
}
