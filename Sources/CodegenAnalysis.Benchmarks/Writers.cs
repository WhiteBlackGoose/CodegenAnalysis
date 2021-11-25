using System;
using System.IO;
using System.Text;

namespace CodegenAnalysis.Benchmarks;

public interface IWriter : IDisposable
{
    public void Write(string text, ConsoleColor color = ConsoleColor.Gray);
}

internal static class IWriterExtensions
{
    public static void WriteLine(this IWriter writer, string text, ConsoleColor color = ConsoleColor.Gray)
        => writer.Write(text + "\n", color);
}

public sealed record class Output
{
    // do not make constant: binary compatibility
    public static readonly string ArtifactsPath = "CodegenAnalysis.Artifacts";

    public IWriter? Logger { get; init; } = new CombinedWriter(new ConsoleWriter(), new ToFileWriter($"{ArtifactsPath}/log.txt"));

    public IWriter? HtmlExporter { get; init; } = new ToFileWriter($"{ArtifactsPath}/report.html");

    public IWriter? MarkdownExporter { get; init; } = new ToFileWriter($"{ArtifactsPath}/report.md");
}

internal sealed class CombinedWriter : IWriter
{
    private readonly IWriter[] writers;
    public CombinedWriter(params IWriter[] writers)
    {
        this.writers = writers;
    }

    public void Write(string text, ConsoleColor color)
    {
        foreach (var w in writers)
            w.Write(text, color);
    }

    public void Dispose()
    {
        foreach (var w in writers)
            w.Dispose();
    }
}

internal sealed class ConsoleWriter : IWriter
{
    public void Write(string text, ConsoleColor color)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = prev;
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

    public void Write(string text, ConsoleColor _)
    {
        if (!File.Exists(path))
        {
            var filePath = Path.GetDirectoryName(path);
            Directory.CreateDirectory(filePath);
            File.WriteAllText(path, "");
        }
        // TODO: use inner buffer instead of appending every time
        File.AppendAllText(path, text);
    }

    public void Dispose()
    {
        
    }
}