using Iced.Intel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodegenAnalysis;

/// <summary>
/// API for analyzing codegen.
/// </summary>
public static class CodegenAnalyzers
{
    // TODO
    private static readonly Func<Instruction, bool> isBranch = i => i.Code.ToString().StartsWith("J") && !i.Code.ToString().StartsWith("Jmp");

    /// <summary>
    /// Similar to <see cref="CodegenInfo.Branches"/>
    /// </summary>
    public static IEnumerable<int> GetBranches(IReadOnlyList<Instruction> instructions)
    {
        return instructions
            .Select((i, index) => (Instruction: i, Index: index))
            .Where(p => isBranch(p.Instruction))
            .Select(p => p.Index);
    }

    // TODO
    private static readonly Func<Instruction, bool> isCall = i => i.Code.ToString().StartsWith("Call");

    /// <summary>
    /// Similar to <see cref="CodegenInfo.Calls"/>
    /// </summary>
    public static IEnumerable<int> GetCalls(IReadOnlyList<Instruction> instructions)
    {
        return instructions
            .Select((i, index) => (Instruction: i, Index: index))
            .Where(p => isCall(p.Instruction))
            .Select(p => p.Index);
    }

    private static readonly Func<Instruction, bool> isPush = i => i.Code.ToString().StartsWith("Push");
    private static readonly Func<Instruction, bool> isLea = i => i.Code.ToString().StartsWith("Lea");
    private static readonly Func<Instruction, bool> isMov = i => i.Code.ToString().StartsWith("Mov");

    /// <summary>
    /// Similar to <see cref="CodegenInfo.StaticStackAllocatedMemory"/>
    /// </summary>
    public static int? GetStaticStackAllocatedMemory(IReadOnlyList<Instruction> instructions)
    {
        if (instructions.Count < 1)
            return 0;
        var i = 0;
        while (i < instructions.Count && isPush(instructions[i]))
            i++;

        // sub rsp, 10
        {
            if (i >= instructions.Count)
                return null;
            var candidate = instructions[i];
            if (candidate.Code is Code.Sub_rm64_imm32 or Code.Sub_rm64_imm8 && candidate.Op0Register == Register.RSP)
                return (int)candidate.Immediate32;
        }

        // lea r11, [rsp - 10]
        // call something
        // mov rsp, r11
        {
            if (i >= instructions.Count - 3)
                return null;
            var leaCandidate = instructions[i];
            var callCandidate = instructions[i + 1];
            var movToRspCandidate = instructions[i + 2];
            if (!isLea(leaCandidate))
                goto next;
            if (!isCall(callCandidate))
                goto next;
            if (!isMov(movToRspCandidate))
                goto next;

            // lea someregister, [rsp-displacement]
            if (leaCandidate.GetOpKind(0) != OpKind.Register)
                goto next;
            if (leaCandidate.GetOpKind(1) != OpKind.Memory)
                goto next;
            if (leaCandidate.MemoryBase != Register.RSP)
                goto next;

            // mov rsp, someregister 
            if (movToRspCandidate.Op0Kind != OpKind.Register)
                goto next;
            if (movToRspCandidate.Op1Kind != OpKind.Register)
                goto next;
            if (movToRspCandidate.Op0Register != Register.RSP)
                goto next;
            if (movToRspCandidate.Op1Register != leaCandidate.Op0Register)
                goto next;
            return (int)(-(long)leaCandidate.MemoryDisplacement64);
        }
        next:


        return null;
    }

    /// <summary>
    /// Gets backward jumps (normally loops).
    /// </summary>
    public static IReadOnlyList<(int From, int To)> GetBackwardJumps(IReadOnlyList<Instruction> instructions)
        => GetJumps(instructions).Where(p => p.To != int.MinValue && p.To < p.From).ToList();

    /// <summary>
    /// Similar to <see cref="CodegenInfo.Jumps"/>
    /// </summary>
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
