﻿using HonkSharp.Laziness;
using Iced.Intel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodegenAnalysis;

/// <summary>
/// Phase of compilation to capture.
/// </summary>
public enum CompilationTier
{
    /// <summary>
    /// This includes tier0, MinOpt - the one
    /// used by the debug configuration, and 
    /// unspecified (for old runtimes).
    /// </summary>
    Default,

    /// <summary>
    /// This includes tier1 and aggressive
    /// optimization, as well as the only phase
    /// functions containing loops get in.
    /// </summary>
    Tier1
}

/// <summary>
/// Represents info about codegen -
/// native instructions generated by JIT
/// </summary>
/// <param name="Bytes">
/// Raw contents of the codegen.
/// </param>
/// <param name="InstructionPointer">
/// The entry point address.
/// </param>
/// <param name="Tier">
/// The compilation tier with which this
/// codegen was obtained.
/// </param>
/// <param name="Instructions">
/// List of instructions decoded by Iced
/// from the raw bytes.
/// </param>
public partial record class CodegenInfo(IReadOnlyList<byte> Bytes, nuint InstructionPointer, CompilationTier Tier, IReadOnlyList<Instruction> Instructions)
{
    /// <inheritdoc/>
    public override string ToString() => ToLines().ToString();


    /// <summary>
    /// The list of branches. Each element of a list is
    /// the index of a branch in the <see cref="Instructions"/> list.
    /// </summary>
    public IReadOnlyList<int> Branches => branches.GetValue(@this => CodegenAnalyzers.GetBranches(@this.Instructions).ToList(), this);
    private LazyPropertyA<IReadOnlyList<int>> branches;


    /// <summary>
    /// The list of calls. Each element of a list is
    /// the index of a call in the <see cref="Instructions"/> list.
    /// </summary>
    public IReadOnlyList<int> Calls => calls.GetValue(@this => CodegenAnalyzers.GetCalls(@this.Instructions).ToList(), this);
    private LazyPropertyA<IReadOnlyList<int>> calls;

    /// <summary>
    /// The list of jumps. Each element of a list is
    /// a pair of two indicies: From is the index of the
    /// jump instruction in the <see cref="Instructions"/> list,
    /// whereas To is either an index of <see cref="Instructions"/> where
    /// the jump happens, or it is <see cref="int.MinValue"/> if the jump
    /// address is before the <see cref="InstructionPointer"/>, or it is
    /// <see cref="int.MaxValue"/> if it's after the last instruction's
    /// address of this codegen.
    /// </summary>
    public IReadOnlyList<(int From, int To)> Jumps => jumps.GetValue(@this => CodegenAnalyzers.GetJumps(@this.Instructions).ToList(), this);
    private LazyPropertyA<IReadOnlyList<(int From, int To)>> jumps;


    /// <summary>
    /// The amount of statically stack allocated memory in bytes.
    /// It excludes the memory allocated dynamically via <see langword="stackalloc"/>
    /// and memory allocated by calls.
    /// 
    /// It relies on stack bump instruction, so in case if the result cannot
    /// be determined, null is returned.
    /// </summary>
    public int? StaticStackAllocatedMemory => staticStackAllocatedMemory.GetValue(@this => CodegenAnalyzers.GetStaticStackAllocatedMemory(@this.Instructions), this);
    private LazyPropertyA<int?> staticStackAllocatedMemory;

    /// <summary>
    /// The size of the codegen in bytes.
    /// </summary>
    public int Size => Bytes.Count;

    internal unsafe Lines ToLines()
    {
        // adapted from https://github.com/icedland/iced/blob/master/src/csharp/Intel/README.md#disassemble-decode-and-format-instructions
        var formatter = new NasmFormatter();
        formatter.Options.DigitSeparator = "`";
        formatter.Options.FirstOperandCharIndex = 10;
        
        var output = new StringOutput();
        var list = new List<StringBuilder>();
        foreach (var instr in Instructions)
        {
            var sb = new StringBuilder();
            sb.Append(instr.IP.ToString("X16")).Append(" ");
            int instrLen = instr.Length;
            int byteBaseIndex = (int)(instr.IP - InstructionPointer);
            for (int i = 0; i < instrLen; i++)
                sb.Append(Bytes[byteBaseIndex + i].ToString("X2"));
            int missingBytes = HEXBYTES_COLUMN_BYTE_LENGTH - instrLen;
            for (int i = 0; i < missingBytes; i++)
                sb.Append("  ");
            sb.Append(" ");
            formatter.Format(instr, output);
            if ((instr.Op0Kind == OpKind.NearBranch64 && instr.OpCount is 1) && EntryPointsListener.MethodByAddress.TryGetValue((nuint)instr.NearBranch64, out var methodBase))
            {
                var o = output.ToStringAndReset().ToString();
                sb.Append(o.Substring(0, o.Length - 20));
                sb.Append(methodBase.ToString());
                sb.Append(" ").Append('(').Append(((ulong)instr.NearBranch64).ToString("X16")).Append(")");
            }
            else
            {
                sb.Append(output.ToStringAndReset());
            }

            list.Add(sb);
        }
        return new(list);
    }

    const int HEXBYTES_COLUMN_BYTE_LENGTH = 10;
}

internal static class Tier
{
    internal static InternalOptimizationTier ToInternalOT(this CompilationTier tier)
        => tier switch
        {
            CompilationTier.Default => InternalOptimizationTier.QuickJitted,
            CompilationTier.Tier1 => InternalOptimizationTier.OptimizedTier1,
            _ => throw new("We forgot to add something")
        };

    internal static CompilationTier ToPublicCT(this InternalOptimizationTier tier) 
        => tier switch
        {
            InternalOptimizationTier.QuickJitted
            or InternalOptimizationTier.MinOptJitted => CompilationTier.Default,

            InternalOptimizationTier.OptimizedTier1
            or InternalOptimizationTier.Optimized => CompilationTier.Tier1,

            _ => throw new($"Unknown {tier}")
        };
}

internal enum InternalOptimizationTier : byte
{
    Unknown = 0,
    MinOptJitted = 1,
    Optimized = 2,
    QuickJitted = 3,
    OptimizedTier1 = 4,
    ReadyToRun = 5,
}