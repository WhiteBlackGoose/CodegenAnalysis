#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Reflection;

namespace CodegenAnalysis.Benchmarks;

/// <summary>
/// Put this on the methods to analyze. If you need
/// to cover multiple branches, put them such that
/// their inputs force multiple branches.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class CAAnalyzeAttribute : Attribute
{
    internal readonly object[] Arguments;

    public CAAnalyzeAttribute(params object[] arguments)
        => Arguments = arguments;

    public override string ToString()
        => $"({string.Join(", ", Arguments)})";
}

/// <summary>
/// Adds a job to work on.
/// Currently it only specifies the compilation tier.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class CAJobAttribute : Attribute
{
    public CompilationTier Tier { get; set; }
    public CAJobAttribute()
    {
        
    }

    public override string ToString()
        => $"(Tier = {Tier})";
}

/// <summary>
/// Adds a column to the output table.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class CAColumnAttribute : Attribute
{
    internal readonly CAColumn Column;
    public CAColumnAttribute(CAColumn column)
    {
        Column = column;
    }

    public override string ToString()
        => $"{Column}";
}

/// <summary>
/// Columns for the output table.
/// </summary>
public enum CAColumn
{
    /// <summary>
    /// Codegen size in bytes
    /// </summary>
    CodegenSize,

    /// <summary>
    /// The number of calls
    /// </summary>
    Calls,

    /// <summary>
    /// The number of branches
    /// </summary>
    Branches,

    /// <summary>
    /// The amount of statically
    /// stack allocated memory
    /// (excluding dynamic allocation).
    /// </summary>
    StaticStackAllocations,

    /// <summary>
    /// The size of the method in MSIL
    /// in bytes.
    /// </summary>
    ILSize
}

/// <summary>
/// Export to file.
/// No exports by default.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class CAExport : Attribute
{
    internal readonly Export Export;
    public CAExport(Export export)
    {
        Export = export;
    }
}

/// <summary>
/// Type of export.
/// </summary>
public enum Export
{
    /// <summary>
    /// To html - make sure to set your <see cref="Output.HtmlExporter"/>
    /// </summary>
    Html,

    /// <summary>
    /// To markdown - make sure to set your <see cref="Output.MarkdownExporter"/>
    /// </summary>
    Md
}

/// <summary>
/// Other settings of benchmark runner.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class CAOptionsAttribute : Attribute
{
    public bool VisualizeBackwardJumps { get; set; }
    public CAOptionsAttribute()
    {

    }
}

/// <summary>
/// Redirects the codegen analysis to another method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class CASubjectAttribute : Attribute
{
    internal readonly string methodName;
    internal readonly Type[] typeArgs;
    internal readonly Type[] parameterTypes;
    internal readonly Type holdingType;

    public CASubjectAttribute(Type holdingType, string methodName, Type[]? typeArgs, Type[]? parameterTypes)
    {
        this.methodName = methodName;
        this.typeArgs = typeArgs ?? Array.Empty<Type>();
        this.parameterTypes = parameterTypes ?? Array.Empty<Type>();
        this.holdingType = holdingType;
    }

    public CASubjectAttribute(Type holdingType, string methodName, Type[]? parameterTypes) : this(holdingType, methodName, Array.Empty<Type>(), parameterTypes)
    {
    }

    public CASubjectAttribute(Type holdingType, string methodName) : this(holdingType, methodName, null, null)
    {
    }
}