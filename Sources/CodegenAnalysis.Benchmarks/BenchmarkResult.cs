
using System.Collections.Generic;
using System.Reflection;

namespace CodegenAnalysis.Benchmarks;

/// <summary>
/// The result of benchmark
/// </summary>
public sealed record class BenchmarkResult
    (
    SortedDictionary<(MethodInfo, CompilationTier), CodegenInfo> Codegens,
    MarkdownTable Table
    );
