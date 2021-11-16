using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace CodegenAssertions;

internal class EntryPointsListener : EventListener
{
    internal static readonly EntryPointsListener listener = new();
    internal static readonly Dictionary<string, List<CodegenInfo>> Codegens = new();

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
            => eventData.Payload[eventData.PayloadNames.IndexOf(key)];

        if (eventData.EventName == "MethodLoadVerbose_V2")
        {
            string fullClassName = (string)GetPayload("MethodNamespace");
            string methodName = (string)GetPayload("MethodName");
            uint flags = (uint)GetPayload("MethodFlags");
            ulong start = (ulong)GetPayload("MethodStartAddress");
            uint size = (uint)GetPayload("MethodSize");
            OptimizationTier opt = (OptimizationTier)((flags >> 7) & 0b111);
            byte[] codeBytes = new byte[size];
            Unsafe.CopyBlock(ref codeBytes[0], ref *(byte*)start, size);
            var res = new CodegenInfo(codeBytes, (nuint)start, opt);
            var key = $"{fullClassName}.{methodName}";
            if (Codegens.TryGetValue(key, out var list))
                list.Add(res);
            else
                Codegens[key] = new() { res };
        }
    }
}