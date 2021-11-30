#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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

public sealed record class Output : IDisposable
{
    // do not make constant: binary compatibility
    /// <summary>
    /// The default path for exporters and loggers.
    /// If you want to change it, change the exporter/logger.
    /// </summary>
    public static readonly string ArtifactsPath = "CodegenAnalysis.Artifacts";

    public IWriter? Logger { get; init; } = new CombinedWriter(new ConsoleWriter(), new ToFileWriter($"{ArtifactsPath}/log.txt"));

    public IWriter? HtmlExporter { get; init; } = new ToFileWriter($"{ArtifactsPath}/report.html");

    public IWriter? MarkdownExporter { get; init; } = new ToFileWriter($"{ArtifactsPath}/report.md");

    public void Dispose()
    {
        Logger?.Dispose();
        HtmlExporter?.Dispose();
        MarkdownExporter?.Dispose();
    }
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
    private bool created;

    public ToFileWriter(string path)
    {
        this.path = path;
        created = false;
    }

    public void Write(string text, ConsoleColor _)
    {
        if (!created)
        {
            var filePath = Path.GetDirectoryName(path);
            Directory.CreateDirectory(filePath ?? throw new("Internal bug #2."));
            File.WriteAllText(path, "");
            created = true;
        }
        // TODO: use inner buffer instead of appending every time
        File.AppendAllText(path, text);
    }

    public void Dispose()
    {
        
    }
}