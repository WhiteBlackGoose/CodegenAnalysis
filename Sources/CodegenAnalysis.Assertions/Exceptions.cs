#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;

namespace CodegenAnalysis.Assertions;

/// <summary>
/// Occurs when the test failed
/// </summary>
public class CodegenAssertionFailedException : Exception
{
    internal CodegenAssertionFailedException() { }
    internal CodegenAssertionFailedException(string msg) : base(msg) { }
}

/// <summary>
/// Occurs when the test failed, to be specific, the actual value of something
/// is "bad" (e. g. when it's more than expected, or not equal, etc. depends on the test).
/// </summary>
public sealed class ExpectedActualException<T> : CodegenAssertionFailedException
{
    internal ExpectedActualException(T expected, T actual, string msg) : base($"Expected: {expected}\nActual: {actual}\nMessage: {msg}") { }
}