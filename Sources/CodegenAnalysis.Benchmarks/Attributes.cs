using System;

namespace CodegenAnalysis.Benchmarks;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class CAInputAttribute : Attribute
{
    internal readonly object[] Arguments;
    public CAInputAttribute(params object[] arguments)
        => Arguments = arguments;
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
}

public enum CAColumn
{
    CodegenSize,
    Calls,
    Branches,
    StaticStackAllocations
}