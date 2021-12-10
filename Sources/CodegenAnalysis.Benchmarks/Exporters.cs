using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodegenAnalysis.Benchmarks;

internal static class Exporters
{
    public static void ExportHtml(IWriter html, MarkdownTable table, SortedDictionary<(MethodInfo, CompilationTier), CodegenInfo> codegens, CAOptionsAttribute options)
    {
        html.Write("<html><body>\n");

        html.Write("<h1>Codegen analysis report</h1>");
        html.Write($"<i>Date: {SuperiourDateTimeNow()}</i>");

        html.Write("<h2>Summary</h2>");
        html.WriteLine(table.ToHtml());

        html.Write("<h2>Codegens</h2>");
        foreach (var pair in codegens)
        {
            var ((mi, tier), code) = (pair.Key, pair.Value);
            html.Write($"<h3>{mi}: {tier}</h3>");
            html.Write($"<div style=\"\"><pre>{CiToString(code, options)}</pre></div>");
        }

        html.Write("</body></html>");
    }

    public static void ExportMd(IWriter md, MarkdownTable table, SortedDictionary<(MethodInfo, CompilationTier), CodegenInfo> codegens, CAOptionsAttribute options)
    {
        md.WriteLine("# Codegen analysis report");
        md.WriteLine($"*{SuperiourDateTimeNow()}*");
        md.WriteLine("## Summary");
        md.WriteLine("");
        md.WriteLine(table.ToString());
        md.WriteLine("## Codegens");
        md.WriteLine("");
        foreach (var pair in codegens)
        {
            var ((mi, tier), code) = (pair.Key, pair.Value);
            md.WriteLine($"### {mi}: {tier}");
            md.WriteLine($"```assembly\n{CiToString(code, options)}\n```");
        }
    }

    private static string SuperiourDateTimeNow()
        => $"{DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm")} UTC";


    internal static string CiToString(CodegenInfo ci, CAOptionsAttribute options)
    {
        var lines = ci.ToLines();
        if (options.VisualizeBackwardJumps)
            lines.DrawArrows(CodegenAnalyzers.GetBackwardJumps(ci.Instructions));
        if (options.VisualizeForwardJumps)
            lines.DrawArrows(CodegenAnalyzers.GetJumps(ci.Instructions).Where(c => c.To is { } to && to > c.From));
        return lines.ToString();
    }
}
