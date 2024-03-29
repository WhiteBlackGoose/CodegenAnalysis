﻿
using System;
using Xunit;

namespace CodegenAnalysis.Benchmarks.Tests;

public class Static
{
    [CAColumn(CAColumn.CodegenSize)]
    public class BenchAdd
    {
        public BenchAdd()
        {
            throw new Exception("The only method is static, no need to call the ctor!");
        }

        [CAAnalyze(3, 5)]
        public static int Add(int a, int b)
        {
            return a + b;
        }
    }

    [Fact]
    public void VerifyNoCtorCalled()
    {
        CodegenBenchmarkRunner.Run<BenchAdd>();
    }
}
