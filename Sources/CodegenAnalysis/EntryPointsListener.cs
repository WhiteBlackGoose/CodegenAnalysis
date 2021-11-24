using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CodegenAnalysis;

internal class EntryPointsListener : EventListener
{
    internal static readonly EntryPointsListener listener = new();
    internal static readonly ConcurrentDictionary<MethodBase, List<Lazy<CodegenInfo>>> Codegens = new();
    internal static readonly ConcurrentDictionary<nuint, MethodBase> MethodByAddress = new();

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
            uint flags = (uint)GetPayload("MethodFlags");
            ulong start = (ulong)GetPayload("MethodStartAddress");
            uint size = (uint)GetPayload("MethodSize");

            InternalOptimizationTier opt = (InternalOptimizationTier)((flags >> 7) & 0b111);

            var res = new Lazy<CodegenInfo>(() =>
                {
                    byte[] codeBytes = new byte[size];
#if NET5_0_OR_GREATER
                    Unsafe.CopyBlock(ref codeBytes[0], ref *(byte*)start, size);
#else
                    fixed (byte* dst = codeBytes)
                    {
                        Buffer.MemoryCopy((byte*)start, dst, size, size);
                    }
#endif
                    return new CodegenInfo(codeBytes, (nuint)start, opt.ToPublicCT(), Disassembler.BytesToInstruction(codeBytes, (nuint)start));
                });

            var methodId = (ulong)GetPayload("MethodID");
            if (methodId is 0)
                return;
            var mb = MethodBaseHelper.GetMethodBaseFromHandle((IntPtr)methodId);
            if (mb is null)
                return;
            var key = mb;
            MethodByAddress[(nuint)start] = mb;
            MethodByAddress[(nuint)(nint)mb.MethodHandle.GetFunctionPointer()] = mb;
            if (Codegens.TryGetValue(key, out var list))
                list.Add(res);
            else
                Codegens[key] = new() { res };
        }
    }
}


// https://github.com/dotnet/runtime/discussions/46215
internal static class MethodBaseHelper
{
    private static readonly Type RuntimeMethodHandleInternal;
    private static readonly ConstructorInfo RuntimeMethodHandleInternal_Constructor;
    private static readonly Type RuntimeType;
    private static readonly MethodInfo RuntimeType_GetMethodBase;

    private static readonly BindingFlags DoNotWrapExceptions =
#if NET5_0_OR_GREATER
        BindingFlags.DoNotWrapExceptions;
#else
        (BindingFlags)33554432;
#endif

    static MethodBaseHelper()
    {
        RuntimeMethodHandleInternal ??= typeof(RuntimeMethodHandle).Assembly.GetType("System.RuntimeMethodHandleInternal", throwOnError: true)!;
        RuntimeMethodHandleInternal_Constructor ??= RuntimeMethodHandleInternal.GetConstructor
        (
            BindingFlags.NonPublic | BindingFlags.Instance | DoNotWrapExceptions,
            binder: null,
            new[] { typeof(IntPtr) },
            modifiers: null
        ) ?? throw new InvalidOperationException("RuntimeMethodHandleInternal constructor is missing!");

        RuntimeType ??= typeof(Type).Assembly.GetType("System.RuntimeType", throwOnError: true)!;
        RuntimeType_GetMethodBase ??= RuntimeType.GetMethod
        (
            "GetMethodBase",
            BindingFlags.NonPublic | BindingFlags.Static | DoNotWrapExceptions,
            binder: null,
            new[] { RuntimeType, RuntimeMethodHandleInternal },
            modifiers: null
        ) ?? throw new InvalidOperationException("RuntimeType.GetMethodBase is missing!");

    }

    public static MethodBase? GetMethodBaseFromHandle(IntPtr handle)
    {
        // Wrap the handle
        object runtimeHandle = RuntimeMethodHandleInternal_Constructor.Invoke(new[] { (object)handle });
        return (MethodBase?)RuntimeType_GetMethodBase.Invoke(null, new[] { null, runtimeHandle });
    }
}