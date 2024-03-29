﻿using System.Runtime.CompilerServices;
using Xunit;

namespace CodegenAnalysis.Benchmarks.Tests;

public class Aaa
{
    private int field;
    public Aaa(int f) => field = f;
    public int Do(int a) => a + field;
        
}

public class SubjectTests
{
    public class Local
    {
        [CAAnalyze(3.5f)]
        [CAAnalyze(13.5f)]
        [CASubject(typeof(Local), "Do1", null)]
        public static float Heavy(float a)
        {
            var b = Do1(a);
            var c = Do1(b);
            if (a > 10)
                c += Aaa(a);
            return c + b;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static float Do1(float a)
        {
            return a * 2;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static float Aaa(float h)
        {
            return h * h * h;
        }
    }


    [Fact]
    public void LocalTest()
    {
        var writer = new FakeWriter();
        CodegenBenchmarkRunner.Run<Local>(new Output() { Logger = writer });
        Assert.Contains("| (Tier = Tier1)  | Single Do1(Single)  |  -        |  -     | 8 B          |", writer.Output);
    }

    public class OuterType
    {
        [CAAnalyze]
        [CASubject(typeof(Aaa), "Do")]
        public static float Heavy()
        {
            var a = new Aaa(4);
            var b = new Aaa(5);
            return a.Do(b.Do(4));
        }
    }

    [Fact]
    public void OuterTypeTest()
    {
        var writer = new FakeWriter();
        CodegenBenchmarkRunner.Run<OuterType>(new Output() { Logger = writer });
        Assert.Contains("| (Tier = Tier1)  | Int32 Do(Int32)  |  -        |  -     | 6 B          |", writer.Output);
    }

#if DEBUG
    [CAJob(Tier = CompilationTier.Default)]
#else
    [CAJob(Tier = CompilationTier.Tier1)]
#endif
    public class GenericMethod
    {
        [CAAnalyze]
        [CASubject(typeof(GenericMethod), "MyGetType", typeArgs: new [] { typeof(int) }, parameterTypes: null)]
        public string Ducks()
        {
            return MyGetType<int>();
        }

        public static string MyGetType<T>() => typeof(T).ToString();
    }

    [Fact]
    public void GenericMethodTest()
    {
        var writer = new FakeWriter();
        CodegenBenchmarkRunner.Run<GenericMethod>(new Output() { Logger = writer });
        Assert.Contains("| (Tier = Tier1)  | System.String MyGetType[Int32]()  |  -        | 1      | 3", writer.Output);
    }

#if DEBUG
    [CAJob(Tier = CompilationTier.Default)]
#else
    [CAJob(Tier = CompilationTier.Tier1)]
#endif
    public class TheRightOverload
    {
        [CAAnalyze]
        [CASubject(typeof(TheRightOverload), "Triple", new [] { typeof(float) })]
        public float DoThing()
        {
            return Triple(2f * Triple(3));
        }

        public float Triple(float a)
        {
            return a * 3;
        }

        public int Triple(int a)
        {
            return a * 3;
        }
    }

    [Fact]
    public void TheRightOverloadTest()
    {
        var writer = new FakeWriter();
        CodegenBenchmarkRunner.Run<TheRightOverload>(new Output() { Logger = writer });
        Assert.Contains("| (Tier = Tier1)  | Single Triple(Single)  |  -        |  -     |", writer.Output);
    }

#if DEBUG
    [CAJob(Tier = CompilationTier.Default)]
#else
    [CAJob(Tier = CompilationTier.Tier1)]
#endif
    public class TheRightOverloadGeneric
    {
#pragma warning disable CS8605 // Unboxing a possibly null value.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        [CAAnalyze]
        [CASubject(typeof(TheRightOverloadGeneric), "Triple", new[] { typeof(float) }, new[] { typeof(float), typeof(int) })]
        public float DoThing()
        {
            return
            Triple<int>(3, 5)
            + Triple(3f, 5)
            + Triple<float>(3f, 5);
        }

        public T Triple<T>(T a, int b)
        {
            if (typeof(T) == typeof(float))
                return (T) (object) ((float) (object) a * 3f);
            if (typeof(T) == typeof(int))
                return (T)(object)((int)(object)a * 3);
            return default!;
        }

        public float Triple(float a, int _)
        {
            return a * 3;
        }
#pragma warning restore CS8605 // Unboxing a possibly null value.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
    }

    [Fact]
    public void TheRightOverloadGenericTest()
    {
        var writer = new FakeWriter();
        CodegenBenchmarkRunner.Run<TheRightOverloadGeneric>(new Output() { Logger = writer });
        Assert.Contains("| (Tier = Tier1)  | Single Triple[Single](Single, Int32)", writer.Output);
    }
}