﻿using Xunit;
#pragma warning disable CS8605 // Unboxing a possibly null value.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

namespace CodegenAnalysis.Assertions.Tests;

public class GenericAPITests
{
    static T Add<T>(T a, T b)
    {
        if (typeof(T) == typeof(int))
            return (T) (object) ((int) (object) a + (int) (object) b);
        else if (typeof(T) == typeof(float))
            return (T) (object) ((float) (object) a + (float) (object) b);
        return default!;
    }

    [Fact]
    public void TestSize1()
    {
        CodegenInfo.Obtain(() => Add(3, 5))
            .ShouldHaveBranches(0);
    }

    [Fact]
    public void TestSize2()
    {
        CodegenInfo.Obtain(() => Add(3.3f, 5.2f))
            .ShouldHaveBranches(0);
    }

    private static class GenericDuck<T>
    {
        public static T Add(T a, T b) => Add<T>(a, b);
    }

    [Fact]
    public void TestGenericTypeSize1()
    {
        CodegenInfo.Obtain(() => GenericDuck<int>.Add(3, 5))
            .ShouldHaveBranches(0);
    }

    [Fact]
    public void TestGenericTypeSize2()
    {
        CodegenInfo.Obtain(() => GenericDuck<float>.Add(3.3f, 5.2f))
            .ShouldHaveBranches(0);
    }
}


#pragma warning restore CS8605 // Unboxing a possibly null value.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.