
using System;
using System.Text;
using Xunit;

namespace CodegenAnalysis.Benchmarks.Tests;

public sealed class FakeWriter : IWriter
{
    internal string Output { get; private set; } = "";

    public void Write(string text, ConsoleColor color = ConsoleColor.Gray)
    {
        Output += text;
    }

    public void Dispose()
    {
        
    }
}

public class InstanceMethods
{
    [CAColumn(CAColumn.CodegenSize)]
    public class BenchAdd
    {
        private StringBuilder sb;

        public BenchAdd()
        {
            sb = new();
            sb.Append("5");
        }

        [CAAnalyze(3, 5)]
        public int Add(int a, int b)
        {
            return a + b;
        }

        [CAAnalyze]
        public int Parse()
        {
            return int.Parse(sb.ToString()); // will be NRE if sb is not inited
        }
    }

    [Fact]
    public void ExpectedTable()
    {
        var writer = new FakeWriter();
        CodegenBenchmarkRunner.Run<BenchAdd>(new Output() { Logger = writer });
        Assert.Contains("| Int32 Add(Int32, Int32)  | 3, 5   | 5 B", writer.Output);
    }

    [Fact]
    public void ExpectedCodegen()
    {
        var writer = new FakeWriter();
        CodegenBenchmarkRunner.Run<BenchAdd>(new Output() { Logger = writer });
        Assert.Contains("lea       eax,[rdx+r8]", writer.Output);
        Assert.Contains("ret", writer.Output);
    }

    [Fact]
    public void ParseWorks()
    {
        var writer = new FakeWriter();
        CodegenBenchmarkRunner.Run<BenchAdd>(new Output() { Logger = writer });
        Assert.Contains("call      ParsingStatus TryParseInt32IntegerStyle", writer.Output);
    }
}
