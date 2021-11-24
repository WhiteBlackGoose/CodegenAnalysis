using Iced.Intel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodegenAssertions;

public static class CodegenAnalyzers
{
    // TODO
    private static readonly Func<Instruction, bool> isBranch = i => i.Code.ToString().StartsWith("J") && !i.Code.ToString().StartsWith("Jmp");

    public static IEnumerable<int> GetBranches(IReadOnlyList<Instruction> instructions)
    {
        return instructions
            .Select((i, index) => (Instruction: i, Index: index))
            .Where(p => isBranch(p.Instruction))
            .Select(p => p.Index);
    }

    // TODO
    private static readonly Func<Instruction, bool> isCall = i => i.Code.ToString().StartsWith("Call");

    public static IEnumerable<int> GetCalls(IReadOnlyList<Instruction> instructions)
    {
        return instructions
            .Select((i, index) => (Instruction: i, Index: index))
            .Where(p => isCall(p.Instruction))
            .Select(p => p.Index);
    }

    public static int? GetStaticStackAllocatedMemory(IReadOnlyList<Instruction> instructions)
    {
        if (instructions.Count < 1)
            return 0;
        for (int i = 0; i <= 1; i++)
        {
            var candidate = instructions[i];
            if (candidate.Code is Code.Sub_rm64_imm32 or Code.Sub_rm64_imm8 && candidate.Op0Register == Register.RSP)
                return (int)candidate.Immediate32;
        }
        return 0;
    }
}
