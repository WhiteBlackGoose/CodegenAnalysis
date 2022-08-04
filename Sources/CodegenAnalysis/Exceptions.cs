using System;

namespace CodegenAnalysis;


/// <summary>
/// Occurs when the JIT did not report creating a method for the given tier (level) of compilation. It may occur on unsupported runtimes or if something about the method is not right.
/// </summary>
public sealed class RequestedTierNotFoundException : Exception

{
    internal RequestedTierNotFoundException(CompilationTier tier)
        : base($"Tier {tier} not found. Try toggling the build configuration to Release. Make sure that "
            + tier switch
            {
                CompilationTier.Tier1 => "the method is not annotated with NoOptimization",
                CompilationTier.Default => "the method is not annotated with AggressiveOptimization",
                _ => throw new($"Report this bug to the repo: unexpected tier {tier}")
            }
        ) { }
}


/// <summary>
/// A method compilation was missed. It may happen when by the time the CodegenAnalysis event listener is initialized, the method is already compiled. Make sure not to run it before using the methods from the library.
/// </summary>
public sealed class RequestedMethodNotCapturedForJittingException : Exception
{
    internal RequestedMethodNotCapturedForJittingException(string method)
        : base($"Method {method} wasn't JIT-ted or JIT-ted too early. Make sure you don't run it before the test.") { }
}
