using System;
using System.Reflection;

namespace CodegenAnalysis.Benchmarks;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class CAAnalyzeAttribute : Attribute
{
    internal readonly object[] Arguments;
    public CAAnalyzeAttribute(params object[] arguments)
        => Arguments = arguments;

    public override string ToString()
        => $"({string.Join(", ", Arguments)})";
}

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

public enum CAColumn
{
    CodegenSize,
    Calls,
    Branches,
    StaticStackAllocations,
    ILSize
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class CAExport : Attribute
{
    internal readonly Export Export;
    public CAExport(Export export)
    {
        Export = export;
    }
}

public enum Export
{
    Html,
    Md
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class CAOptionsAttribute : Attribute
{
    public bool VisualizeBackwardJumps { get; set; }
    public CAOptionsAttribute()
    {

    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class CASubjectAttribute : Attribute
{
    internal readonly string methodName;
    internal readonly Type[] typeArgs;
    internal readonly Type[] parameterTypes;

    public CASubjectAttribute(string methodName, Type[] typeArgs, params Type[] parameterTypes)
    {
        this.methodName = methodName;
        this.typeArgs = typeArgs;
        this.parameterTypes = parameterTypes;
    }

    public CASubjectAttribute(string methodName, params Type[] parameterTypes) : this(methodName, Array.Empty<Type>(), parameterTypes)
    {
    }
}