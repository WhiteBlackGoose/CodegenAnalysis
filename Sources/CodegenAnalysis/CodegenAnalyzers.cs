using Iced.Intel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodegenAnalysis;

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

    public static IReadOnlyList<(int From, int To)> GetBackwardJumps(IReadOnlyList<Instruction> instructions)
        => GetJumps(instructions).Where(p => p.To != int.MinValue && p.To < p.From).ToList();

    public static IReadOnlyList<(int From, int To)> GetJumps(IReadOnlyList<Instruction> instructions)
    {
        var list = new List<(int, int)>();        

        for (int i = 0; i < instructions.Count; i++)
            // TODO
            if (instructions[i].Code.ToString().StartsWith("J"))
                list.Add((i, FindInstructionByIP(instructions, instructions[i].NearBranch64)));

        // throw new(list.Count + $"{string.Join(", ", list)}\n{string.Join("\n", instructions.Select(c => c.Code))}");

        return list;


        static int FindInstructionByIP(IReadOnlyList<Instruction> instructions, ulong ip)
        {
            var a = 0;
            var b = instructions.Count - 1;
            if (instructions[a].IP > ip)
                return int.MinValue;
            if (instructions[b].IP < ip)
                return int.MaxValue;
            while (b - a > 1)
            {
                var m = (a + b) / 2;
                if (instructions[m].IP > ip)
                    b = m;
                else if (instructions[m].IP < ip)
                    a = m;
                else
                    return m;
            }
            return a;
        }
    }
}
