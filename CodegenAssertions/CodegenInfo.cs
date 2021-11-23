using Iced.Intel;
using System.Collections.Generic;
using System.Linq;

namespace CodegenAssertions;

public record class CodegenInfo(IReadOnlyList<byte> Bytes, nuint InstructionPointer, CompilationTier Tier, IReadOnlyList<Instruction> Instructions)
{
    public override string ToString() => ToString(null);

    internal unsafe string ToString(IEnumerable<int>? linesToHighlight)
    {
        // adapted from https://github.com/icedland/iced/blob/master/src/csharp/Intel/README.md#disassemble-decode-and-format-instructions
        var sb = new System.Text.StringBuilder();
        var formatter = new NasmFormatter();
        formatter.Options.DigitSeparator = "`";
        formatter.Options.FirstOperandCharIndex = 10;
        var output = new StringOutput();
        var linesToHighlightSet = linesToHighlight is not null ? new HashSet<int>(linesToHighlight) : null;
        var lineId = 0;
        foreach (var instr in Instructions)
        {
            if (linesToHighlightSet is not null)
            {
                if (linesToHighlightSet.Contains(lineId))
                    sb.Append(">>> ");
                else
                    sb.Append("    ");
            }
            lineId++;
            
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
                sb.Append(" ").Append('(').Append(((ulong)instr.NearBranch64).ToString("X16")).AppendLine(")");
            }
            else
            {
                sb.AppendLine(output.ToStringAndReset());
            }
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