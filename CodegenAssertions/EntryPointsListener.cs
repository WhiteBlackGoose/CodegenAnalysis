using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace CodegenAssertions;

internal class EntryPointsListener : EventListener
{
    internal static readonly EntryPointsListener listener = new();
    internal static readonly Dictionary<string, List<Lazy<CodegenInfo>>> Codegens = new();

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "Microsoft-Windows-DotNETRuntime")
        {
            EventKeywords jitTracing = (EventKeywords)0x10;
            EnableEvents(eventSource, EventLevel.Verbose, jitTracing);
        }
    }

    protected override unsafe void OnEventWritten(EventWrittenEventArgs eventData)
    {
        object GetPayload(string key)
            => (eventData.Payload ?? throw new("Unexpected error"))
              [(eventData.PayloadNames ?? throw new("Unexpected error")).IndexOf(key)]
              ?? throw new("Unexpected error");

        if (eventData.EventName == "MethodLoadVerbose_V2")
        {
            string fullClassName = (string)GetPayload("MethodNamespace");
            string methodName = (string)GetPayload("MethodName");
            uint flags = (uint)GetPayload("MethodFlags");
            ulong start = (ulong)GetPayload("MethodStartAddress");
            uint size = (uint)GetPayload("MethodSize");
            InternalOptimizationTier opt = (InternalOptimizationTier)((flags >> 7) & 0b111);

            var res = new Lazy<CodegenInfo>(() =>
                {
                    byte[] codeBytes = new byte[size];
                    Unsafe.CopyBlock(ref codeBytes[0], ref *(byte*)start, size);
                    return new CodegenInfo(codeBytes, (nuint)start, opt, Disassembler.BytesToInstruction(codeBytes, (nuint)start));
                });

            var key = $"{fullClassName}.{methodName}";
            if (Codegens.TryGetValue(key, out var list))
                list.Add(res);
            else
                Codegens[key] = new() { res };
        }
    }
}