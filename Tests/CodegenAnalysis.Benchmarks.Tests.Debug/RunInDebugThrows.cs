using System;
using Xunit;

namespace CodegenAnalysis.Benchmarks.Tests.Debug;

public sealed class FakeWriter : IWriter
{
    internal string Output { get; private set; } = "";

    public bool ThereWasError { get; private set; }

    public void Write(string text, ConsoleColor color = ConsoleColor.Gray)
    {
        Output += text;
        if (color == ConsoleColor.Red)
            ThereWasError = true;
    }

    public void Dispose()
    {

    }
}

public class RunInDebugThrows
{
    public class Aaa
    {
        [CAAnalyze(1, 3)]
        public int Add(int a, int b) => a + b;
    }

    [Fact]
    public void RunInDebugThrowsTest()
    {
        var writer = new FakeWriter();
        CodegenBenchmarkRunner.Run<Aaa>(new() { Logger = writer });
        Assert.Contains("Debug", writer.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Release", writer.Output, StringComparison.OrdinalIgnoreCase);
        Assert.True(writer.ThereWasError);
    }
}
