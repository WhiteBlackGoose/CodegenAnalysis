#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;

namespace CodegenAnalysis.Assertions;

public class CodegenAssertionFailedException : Exception
{
    internal CodegenAssertionFailedException() { }
    internal CodegenAssertionFailedException(string msg) : base(msg) { }
}

public sealed class ExpectedActualException<T> : CodegenAssertionFailedException
{
    internal ExpectedActualException(T expected, T actual, string msg) : base($"Expected: {expected}\nActual: {actual}\nMessage: {msg}") { }
}