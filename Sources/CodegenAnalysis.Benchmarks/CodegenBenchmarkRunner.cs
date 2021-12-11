using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using HonkSharp.Functional;
using HonkSharp.Fluency;
using System.Diagnostics;

namespace CodegenAnalysis.Benchmarks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public static class CodegenBenchmarkRunner
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <summary>
    /// Runs the benchmarks on the given type. Takes
    /// public methods only. Will try to run the
    /// parameterless constructor if there is at least
    /// one instance public method.
    /// 
    /// The methods to run need to be annotated with <see cref="CAAnalyzeAttribute"/>.
    /// </summary>
    /// <returns>Null in case of failure</returns>
    public static BenchmarkResult? Run<T>(Output? output = null)
    {
        return Run(typeof(T), output);
    }

    /// <summary>
    /// Runs the benchmarks on the given type. Takes
    /// public methods only. Will try to run the
    /// parameterless constructor if there is at least
    /// one instance public method.
    /// 
    /// The methods to run need to be annotated with <see cref="CAAnalyzeAttribute"/>.
    /// </summary>
    /// <returns>Null in case of failure</returns>
    public static BenchmarkResult? Run(Type type, Output? outputWrapper = null)
    {
        using var output = outputWrapper ?? new();

        if (type.Assembly.IsInDebug())
        {
            output.Logger?.WriteLine("The assembly seems to be in Debug mode. Switch to release!", ConsoleColor.Red);
            return null;
        }

        var jobs = GetJobs(type);
        output.Logger?.WriteLine($"Detected jobs: \n  {string.Join("\n  ", (IEnumerable<CAJobAttribute>)jobs)}", ConsoleColor.DarkMagenta);

        var columns = GetColumns(type).ToArray();
        output.Logger?.WriteLine($"\nDetected columns: \n  {string.Join("\n  ", (IEnumerable<CAColumnAttribute>)columns)}", ConsoleColor.DarkMagenta);

        var methods = GetMethods(type);
        if (methods.Any())
        {
            output.Logger?.WriteLine($"\nDetected methods: \n  {string.Join("\n  ", methods.Select(c => c.Info + "(" + string.Join(", ", c.Args) + ")"))}", ConsoleColor.DarkMagenta);
        }
        else
        {
            output.Logger?.WriteLine($"\nNo methods with {nameof(CAAnalyzeAttribute)} were detected! Exitting...", ConsoleColor.Red);
            return null;
        }

        object? instance = methods.Any(mi => !mi.Info.IsStatic) ? Activator.CreateInstance(type) : null;

        var codegens = new SortedDictionary<(MethodInfo, CompilationTier), CodegenInfo>(new MiTierComparer());
        var table = new MarkdownTable(new [] { "Job", "Method" }.Concat(columns.Select(c => c.ToString())));

        output.Logger?.WriteLine("");

        var rowId = 0;
        var errorNumber = 1;
        var notFoundErrors = 0;
        foreach (var job in jobs)
        {
            foreach (var (mi, inputs) in methods)
            {
                Exception? error = null;
                CodegenInfo? ci = null;
                var actualMiAu = GetFinalSubject(mi);
                if (!actualMiAu.Is<MethodInfo>(out var actualMi))
                {
                    error = (Exception)actualMiAu;
                    goto fillingTable;
                }

                if (actualMi == mi)
                    output.Logger?.WriteLine($"Investigating {mi} {string.Join(", ", inputs)} {job}...");
                else
                    output.Logger?.WriteLine($"Investigating {mi} (subject: {actualMi}) {string.Join(", ", inputs)} {job}...");

                foreach (var input in inputs)
                {
                    var ciAu = CodegenInfo.ObtainSilent(job.Tier, mi, instance, input.Arguments);
                    if (ciAu.Is<CodegenInfo>(out var newCi))
                    {
                        ci = newCi;
                    }
                    else
                    {
                        error = NotJittedOrFound(actualMi);
                        goto fillingTable;
                    }
                }
                if (actualMi != mi)
                {
                    ci = CodegenInfo.GetByNameAndTier(actualMi, job.Tier);
                    if (ci is null)
                    {
                        error = NotJittedOrFound(actualMi);
                        goto fillingTable;
                    }
                }
                if (ci is null)
                {
                    error = NotJittedOrFound(actualMi);
                    goto fillingTable;
                }

                codegens[(actualMi, job.Tier)] = ci; // overwriting the last to get a richer result
                
                fillingTable:
                table[rowId, 0] = job.ToString();
                table[rowId, 1] = actualMi.ToString()!;

                if (ci is null && error is null) throw new Exception("Internal bug #1.");

                for (int i = 0; i < columns.Length; i++)
                {
                    if (error is null)
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        table[rowId, i + 2] = columns[i].Column switch
                        {
                            CAColumn.Branches => IntToString(CodegenAnalyzers.GetBranches(ci.Instructions).Count()),
                            CAColumn.Calls => IntToString(CodegenAnalyzers.GetCalls(ci.Instructions).Count()),
                            CAColumn.CodegenSize => BytesToString(ci.Bytes.Count),
                            CAColumn.StaticStackAllocations => BytesToString(CodegenAnalyzers.GetStaticStackAllocatedMemory(ci.Instructions)),
                            CAColumn.ILSize => BytesToString(mi.GetMethodBody()!.GetILAsByteArray()!.Length),
                            var unexpected => throw new($"Internal error. Unexpected {unexpected}")
                        };
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    else
                        table[rowId, i + 2] = $"NA ({errorNumber})";
                }
                rowId++;

                if (error is not null)
                {
                    output.Logger?.WriteLine($"NA ({errorNumber}): {error.Message}", ConsoleColor.Red);
                    errorNumber++;
                }
            }
        }

        output.Logger?.WriteLine("");

        if (notFoundErrors > 0)
        {
            output.Logger?.WriteLine($"{notFoundErrors} 'Not found' errors were detected. Here are possible reasons:", ConsoleColor.DarkYellow);
            output.Logger?.WriteLine($@"
  - The configuration is Debug. Make sure to switch to release if you need {CompilationTier.Tier1}+.
  - The method is annotated with AggressiveOptimization but you requested {CompilationTier.Default} tier.
  - The method is annotated with NoOptimization but you requested {CompilationTier.Tier1}.
  - The method contains a loop but you requested {CompilationTier.Default} tier.
  - The method has {nameof(CASubjectAttribute)} and the subject method is inlined into the outer method.
            ", ConsoleColor.DarkYellow);
        }

        output.Logger?.WriteLine("");

        var options = GetOptions(type);

        foreach (var pair in codegens)
        {
            var (mi, ci) = (pair.Key, pair.Value);
            output.Logger?.WriteLine("");
            output.Logger?.WriteLine(mi.ToString(), ConsoleColor.Blue);
            output.Logger?.WriteLine("    " + Exporters.CiToString(ci, options).Replace("\n", "\n    "), ConsoleColor.DarkGray);
            output.Logger?.WriteLine("");
        }

        output.Logger?.WriteLine(table.ToString(), ConsoleColor.Blue);
        
        if (type.AttributesOfType<CAExport>().Any(c => c.Export == Export.Html))
        {
            if (output.HtmlExporter is null)
                output.Logger?.WriteLine("Exporting to html was requested, but no html exporter was provided!", ConsoleColor.Red);
            else
                Exporters.ExportHtml(output.HtmlExporter, table, codegens, options);
        }

        if (type.AttributesOfType<CAExport>().Any(c => c.Export == Export.Md))
        {
            if (output.MarkdownExporter is null)
                output.Logger?.WriteLine("Exporting to markdown was requested, but no markdown exporter was provided!", ConsoleColor.Red);
            else
                Exporters.ExportMd(output.MarkdownExporter, table, codegens, options);
        }

        return new(Codegens: codegens, Table: table);


        static string IntToString(int a)
            => a is 0 ? " - " : a.ToString();

        static string BytesToString(int? a)
            => a switch
            {
                0 => " - ",
                null => " ? ",
                { } other => $"{other} B"
            };

        Exception NotJittedOrFound(MethodInfo mi)
        {
            notFoundErrors++;
            if (EntryPointsListener.Codegens.TryGetValue(mi, out var allFound))
            {
                return new Exception($"For the requested tier only tiers {allFound.Select(c => c.Value.Tier).Pipe(", ".Join)} were found");
            }
            return new Exception("No method for the requested tier was found.");
        }
    }

    internal static bool IsInDebug(this Assembly asm)
    {
        var debugs = asm.AttributesOfType<DebuggableAttribute>();
        if (debugs.Count() != 1)
            throw new($"Internal bug #2. {debugs.Count()} of {nameof(DebuggableAttribute)} detected.");
        var debug = debugs.Single();
        return debug.IsJITOptimizerDisabled;
    }

    private class MiTierComparer : IComparer<(MethodInfo, CompilationTier)>
    {
        public int Compare((MethodInfo, CompilationTier) x, (MethodInfo, CompilationTier) y)
        {
            if (x.Item1 == y.Item1)
                return ((int)x.Item2).CompareTo((int)y.Item2);
            return x.Item1.GetHashCode().CompareTo(y.Item1.GetHashCode());
        }
    }

    private static IEnumerable<CAJobAttribute> GetJobs(Type type)
    {
        var res = type.AttributesOfType<CAJobAttribute>();
        if (!res.Any())
            return new [] { new CAJobAttribute() { Tier = CompilationTier.Tier1 } };
        return res;
    }

    private static IEnumerable<CAColumnAttribute> GetColumns(Type type)
    {
        var res = type.AttributesOfType<CAColumnAttribute>();
        if (!res.Any())
            return new [] { new CAColumnAttribute(CAColumn.Branches), new CAColumnAttribute(CAColumn.Calls), new CAColumnAttribute(CAColumn.CodegenSize) };
        return res;
    }

    private static CAOptionsAttribute GetOptions(Type type)
    {
        var res = type.AttributesOfType<CAOptionsAttribute>();
        if (res.Any())
            return res.Single();
        return new CAOptionsAttribute();
    }

    private static IEnumerable<(MethodInfo Info, IEnumerable<CAAnalyzeAttribute> Args)> GetMethods(Type type)
    {
        return type
                .GetMethods()
                .Select(mi =>
                    (Mi: mi, Attrs: mi.AttributesOfType<CAAnalyzeAttribute>()))
                .Where(pair => pair.Attrs.Any());
    }

    private static IEnumerable<T> AttributesOfType<T>(this MethodInfo mi) where T : Attribute
        => mi.GetCustomAttributes(typeof(T)).Select(c => (T)c);

    private static IEnumerable<T> AttributesOfType<T>(this Type type) where T : Attribute
        => type.GetCustomAttributes(typeof(T)).Select(c => (T)c);

    private static IEnumerable<T> AttributesOfType<T>(this Assembly type) where T : Attribute
        => type.GetCustomAttributes(typeof(T)).Select(c => (T)c);

    private static T? RealSingleOrDefault<T>(this IEnumerable<T> seq)
    {
        var alreadyFilled = false;
        T? res = default(T);
        foreach (var e in seq)
        {
            if (alreadyFilled)
                return default(T);
            res = e;
            alreadyFilled = true;
        }
        return res;
    }

    private static Either<MethodInfo, Exception> GetFinalSubject(MethodInfo mi)
    {
        var subject = mi.AttributesOfType<CASubjectAttribute>().SingleOrDefault();
        if (subject is null)
            return mi;

        var type = subject.holdingType;
        var name = subject.methodName;
        var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(mi => mi.Name == name);

        if (!methods.Any())
            return new Exception($"{name} not found. Type has: {string.Join(", ", methods.Select(m => m.Name))}");

        var methodsGeneric = methods.Where(mi => mi.GetGenericArguments().Length == subject.typeArgs.Length);
        if (!methodsGeneric.Any())
            return new Exception($"{name} with type args not found. Type has: {string.Join(", ", methods)}");
        var theOnlyGeneric = methodsGeneric.RealSingleOrDefault();
        if (theOnlyGeneric is not null)
            return subject.typeArgs.Length > 0 ? theOnlyGeneric.MakeGenericMethod(subject.typeArgs) : theOnlyGeneric;

        var methodsGenericParameters = methods.Where(mi => SeqsCoincide(mi.GetParameters().Select(p => p.ParameterType), subject.parameterTypes));
        if (!methodsGenericParameters.Any())
            return new Exception($"{name} with type args not found. Type has: {string.Join(", ", methods)}");
        var theOnlyGenericParameters = methodsGenericParameters.RealSingleOrDefault();
        if (theOnlyGenericParameters is not null)
            return theOnlyGenericParameters;
        return new Exception($"Too many: {string.Join(", ", theOnlyGenericParameters)}");

        static bool SeqsCoincide<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            if (a.Count() != b.Count())
                return false;
            return a.Zip(b, (a, b) => (a, b)).All(c => c.Item1 is null && c.Item2 is null || c.Item1 is not null && c.Item1.Equals(c.Item2));
        }
    }
}