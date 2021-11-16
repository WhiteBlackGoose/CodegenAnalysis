using Iced.Intel;
using System.Collections.Generic;

namespace CodegenAssertions;

internal record CodegenInfo(byte[] Bytes, nuint InstructionPointer, OptimizationTier Tier)
{
    public override unsafe string ToString()
    {
        var codeReader = new ByteArrayCodeReader(Bytes);
        var decoder = Decoder.Create(sizeof(nint) * 8, codeReader);
        decoder.IP = InstructionPointer;
        ulong endRip = decoder.IP + (uint)Bytes.Length;

        var instructions = new List<Instruction>();
        while (decoder.IP < endRip)
            instructions.Add(decoder.Decode());

        // adapted from https://github.com/icedland/iced/blob/master/src/csharp/Intel/README.md#disassemble-decode-and-format-instructions
        var sb = new System.Text.StringBuilder();
        var formatter = new NasmFormatter();
        formatter.Options.DigitSeparator = "`";
        formatter.Options.FirstOperandCharIndex = 10;
        var output = new StringOutput();
        foreach (var instr in instructions)
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

internal enum OptimizationTier : byte
{
    Unknown = 0,
    MinOptJitted = 1,
    Optimized = 2,
    QuickJitted = 3,
    OptimizedTier1 = 4,
    ReadyToRun = 5,
}