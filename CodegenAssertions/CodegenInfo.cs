using Iced.Intel;
using System.Collections.Generic;

namespace CodegenAssertions;

public record CodegenInfo(byte[] Bytes, nuint InstructionPointer, CompilationTier Tier, Instruction[] Instructions)
{
    public override unsafe string ToString()
    {
        // adapted from https://github.com/icedland/iced/blob/master/src/csharp/Intel/README.md#disassemble-decode-and-format-instructions
        var sb = new System.Text.StringBuilder();
        var formatter = new NasmFormatter();
        formatter.Options.DigitSeparator = "`";
        formatter.Options.FirstOperandCharIndex = 10;
        var output = new StringOutput();
        foreach (var instr in Instructions)
        {
            formatter.Format(instr, output);
            sb.Append(instr.IP.ToString("X16")).Append(" ");
            int instrLen = instr.Length;
            int byteBaseIndex = (int)(instr.IP - InstructionPointer);
            for (int i = 0; i < instrLen; i++)
                sb.Append(Bytes[byteBaseIndex + i].ToString("X2"));
            int missingBytes = HEXBYTES_COLUMN_BYTE_LENGTH - instrLen;
            for (int i = 0; i < missingBytes; i++)
                sb.Append("  ");
            sb.Append(" ");
            sb.AppendLine(output.ToStringAndReset());
        }
        return sb.ToString();
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
            InternalOptimizationTier.OptimizedTier1 => CompilationTier.Tier1,
            InternalOptimizationTier.Optimized => CompilationTier.AO,
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