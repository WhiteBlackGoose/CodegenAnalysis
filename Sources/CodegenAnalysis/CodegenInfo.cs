using HonkSharp.Laziness;
using Iced.Intel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodegenAnalysis;

public enum CompilationTier
{
    Default,
    Tier1
}

public partial record class CodegenInfo(IReadOnlyList<byte> Bytes, nuint InstructionPointer, CompilationTier Tier, IReadOnlyList<Instruction> Instructions)
{
    public override string ToString() => ToLines().ToString();


    private LazyPropertyA<IReadOnlyList<int>> branches;
    public IReadOnlyList<int> Branches => branches.GetValue(@this => CodegenAnalyzers.GetBranches(@this.Instructions).ToList(), this);


    private LazyPropertyA<IReadOnlyList<int>> calls;
    public IReadOnlyList<int> Calls => calls.GetValue(@this => CodegenAnalyzers.GetCalls(@this.Instructions).ToList(), this);


    private LazyPropertyA<IReadOnlyList<(int From, int To)>> jumps;
    public IReadOnlyList<(int From, int To)> Jumps => jumps.GetValue(@this => CodegenAnalyzers.GetJumps(@this.Instructions).ToList(), this);


    private LazyPropertyA<int?> staticStackAllocatedMemory;
    public int? StaticStackAllocatedMemory => staticStackAllocatedMemory.GetValue(@this => CodegenAnalyzers.GetStaticStackAllocatedMemory(@this.Instructions), this);

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