using Iced.Intel;
using System.Collections.Generic;

namespace CodegenAssertions;

internal static class Disassembler
{
    internal static unsafe Instruction[] BytesToInstruction(byte[] bytes, nuint start)
    {
        var codeReader = new ByteArrayCodeReader(bytes);
        var decoder = Decoder.Create(sizeof(nint) * 8, codeReader);
        decoder.IP = start;
        ulong endRip = decoder.IP + (uint)bytes.Length;

        var instructions = new List<Instruction>();
        while (decoder.IP < endRip)
            instructions.Add(decoder.Decode());

        return instructions.ToArray();
    }
}