using System;
using System.IO;
using System.Text;

namespace CodegenAnalysis.Benchmarks;

public interface IWriter : IDisposable
{
    public void Write(string text);
}

internal static class IWriterExtensions
{
    public static void WriteLine(this IWriter writer, string text)
        => writer.Write(text + "\n");
}

public sealed record class Output
{
    public IWriter? Logger { get; init; } = new ConsoleWriter();

    public IWriter? HtmlExporter { get; init; } = new ToFileWriter("CodegenAnalysis.Artifacts/report.html");

    public IWriter? MarkdownExporter { get; init; } = new ToFileWriter("CodegenAnalysis.Artifacts/report.md");
}

internal sealed class ConsoleWriter : IWriter
{
    public void Write(string text)
    {
        Console.Write(text);
    }

    public void Dispose()
    {
        
    }
}

internal sealed class ToFileWriter : IWriter
{
    private readonly string path;

    public ToFileWriter(string path)
    {
        this.path = path;
    }

    public void Write(string text)
    {
        // use inner buffer instead of appending every time
        File.AppendAllText(path, text);
    }

    public void Dispose()
    {
        
    }
}