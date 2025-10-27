using System;
using System.Collections.Generic;
#if NET9_0_OR_GREATER
using System.Numerics.Tensors;
#endif
using CSnakes.Runtime.Python;

namespace Integration.Tests;

public class BufferTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    public abstract class BufferUseAfterDisposalTestsBase(PythonEnvironmentFixture fixture) :
        IntegrationTestBase(fixture)
    {
        [Fact]
        public void TestLength() => Test(buffer => _ = buffer.Length);

        [Fact]
        public void TestDimensions() => Test(buffer => _ = buffer.Dimensions);

        [Fact]
        public abstract void TestAsSpan();

        [Fact]
        public abstract void TestAsReadOnlySpan();

        protected abstract IPyBuffer GetBuffer(ITestBuffer module);

        protected void Test(Action<IPyBuffer> action)
        {
            var testModule = Env.TestBuffer();
            using var bufferObject = GetBuffer(testModule);
            bufferObject.Dispose();
            var ex = Assert.Throws<ObjectDisposedException>(() => action(bufferObject));
            Assert.Equal(bufferObject.GetType().FullName, ex.ObjectName);
        }
    }

    public class ArrayBufferUseAfterDisposalTests(PythonEnvironmentFixture fixture) :
        BufferUseAfterDisposalTestsBase(fixture)
    {
        [Fact]
        public override void TestAsSpan() => Test(buffer => _ = buffer.AsSpan<int>());

        [Fact]
        public override void TestAsReadOnlySpan() => Test(buffer => _ = buffer.AsReadOnlySpan<int>());

        protected override IPyBuffer GetBuffer(ITestBuffer module) => module.TestInt32Buffer();
    }

    public class Array2DBufferUseAfterDisposalTests(PythonEnvironmentFixture fixture) :
        BufferUseAfterDisposalTestsBase(fixture)
    {
        [Fact]
        public override void TestAsSpan() => Test(buffer => _ = buffer.AsSpan2D<int>());

        [Fact]
        public override void TestAsReadOnlySpan() => Test(buffer => _ = buffer.AsReadOnlySpan2D<int>());

        protected override IPyBuffer GetBuffer(ITestBuffer module) => module.TestInt322dBuffer();
    }

#if NET9_0_OR_GREATER

    public class TensorBufferUseAfterDisposalTests(PythonEnvironmentFixture fixture) :
        BufferUseAfterDisposalTestsBase(fixture)
    {
        [Fact]
        public override void TestAsSpan() => Test(buffer => _ = buffer.AsTensorSpan<int>());

        [Fact]
        public override void TestAsReadOnlySpan() => Test(buffer => _ = buffer.AsReadOnlyTensorSpan<int>());

        protected override IPyBuffer GetBuffer(ITestBuffer module) => module.TestNdim3dBuffer();
    }

#endif

    [Fact]
    [Trait("requires", "numpy")]
    public void TestBoolBuffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestBoolBuffer();
        Assert.Equal(5, bufferObject.Length);
        Assert.True(bufferObject.IsScalar);

        // Check the buffer contents
        Span<bool> result = bufferObject.AsBoolSpan();
        Assert.Equal(typeof(bool), bufferObject.GetItemType());
        Assert.True(result[0]);
        Assert.False(result[4]);
    }

    [Fact]
    public void TestInt8Buffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestInt8Buffer();
        Assert.Equal(5 * sizeof(sbyte), bufferObject.Length);
        Assert.True(bufferObject.IsScalar);

        // Check the buffer contents
        Span<sbyte> result = bufferObject.AsSByteSpan();
        Assert.Equal(typeof(sbyte), bufferObject.GetItemType());
        Assert.Equal((sbyte)1, result[0]);
        Assert.Equal((sbyte)5, result[4]);
    }

    [Fact]
    public void TestUInt8Buffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestUint8Buffer();
        Assert.Equal(5 * sizeof(byte), bufferObject.Length);
        Assert.True(bufferObject.IsScalar);

        // Check the buffer contents
        Span<byte> result = bufferObject.AsByteSpan();
        Assert.Equal(typeof(byte), bufferObject.GetItemType());
        Assert.Equal((byte)1, result[0]);
        Assert.Equal((byte)5, result[4]);
    }

    [Fact]
    public void TestInt16Buffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestInt16Buffer();
        Assert.Equal(5 * sizeof(short), bufferObject.Length);
        Assert.True(bufferObject.IsScalar);

        // Check the buffer contents
        Span<short> result = bufferObject.AsInt16Span();
        Assert.Equal(typeof(short), bufferObject.GetItemType());
        Assert.Equal((short)1, result[0]);
        Assert.Equal((short)5, result[4]);
    }

    [Fact]
    public void TestUInt16Buffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestUint16Buffer();
        Assert.Equal(5 * sizeof(ushort), bufferObject.Length);
        Assert.True(bufferObject.IsScalar);

        // Check the buffer contents
        Span<ushort> result = bufferObject.AsUInt16Span();
        Assert.Equal(typeof(ushort), bufferObject.GetItemType());
        Assert.Equal((ushort)1, result[0]);
        Assert.Equal((ushort)5, result[4]);
    }

    [Fact]
    public void TestInt32Buffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestInt32Buffer();
        Assert.Equal(20, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.IsScalar);

        // Check the buffer contents
        Span<int> result = bufferObject.AsInt32Span();
        Assert.Equal(typeof(int), bufferObject.GetItemType());
        Assert.Equal((int)1, result[0]);
        Assert.Equal((int)5, result[4]);
    }

    [Fact]
    public void TestUInt32Buffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestUint32Buffer();
        Assert.Equal(20, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.IsScalar);

        // Check the buffer contents
        Span<uint> result = bufferObject.AsUInt32Span();
        Assert.Equal(typeof(uint), bufferObject.GetItemType());
        Assert.Equal((uint)1, result[0]);
        Assert.Equal((uint)5, result[4]);
    }

    [Fact]
    public void TestInt64Buffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestInt64Buffer();
        Assert.Equal(sizeof(long) * 5, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.IsScalar);

        // Check the buffer contents
        Span<long> result = bufferObject.AsInt64Span();
        Assert.Equal(typeof(long), bufferObject.GetItemType());
        Assert.Equal(1L, result[0]);
        Assert.Equal(5L, result[4]);
    }

    [Fact]
    public void TestUInt64Buffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestUint64Buffer();
        Assert.Equal(sizeof(long) * 5, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.IsScalar);

        // Check the buffer contents
        Span<ulong> result = bufferObject.AsUInt64Span();
        Assert.Equal(typeof(ulong), bufferObject.GetItemType());
        Assert.Equal(1UL, result[0]);
        Assert.Equal(5UL, result[4]);
    }

    [Fact]
    public void TestFloat16Buffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestFloat16Buffer();
        Assert.Equal(10, bufferObject.Length); // 5 * sizeof(o)
        Assert.True(bufferObject.IsScalar);

        // Check the buffer contents
        Span<Half> result = bufferObject.AsHalfSpan();
        Assert.Equal(typeof(Half), bufferObject.GetItemType());
        Assert.Equal(1.1f, (float)result[0], 0.01);
        Assert.Equal(5.5f, (float)result[4], 0.01);
    }

    [Fact]
    public void TestFloat32Buffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestFloat32Buffer();
        Assert.Equal(20, bufferObject.Length); // 5 * sizeof(o)
        Assert.True(bufferObject.IsScalar);

        // Check the buffer contents
        Span<float> result = bufferObject.AsFloatSpan();
        Assert.Equal(typeof(float), bufferObject.GetItemType());
        Assert.Equal(1.1f, result[0]);
        Assert.Equal(5.5f, result[4]);
    }

    [Fact]
    public void TestFloat64Buffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestFloat64Buffer();
        Assert.Equal(sizeof(double) * 5, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.IsScalar);

        // Check the buffer contents
        Span<double> result = bufferObject.AsDoubleSpan();
        Assert.Equal(typeof(double), bufferObject.GetItemType());
        Assert.Equal(1.1, result[0]);
        Assert.Equal(5.5, result[4]);
    }

    [Fact]
    public void TestBufferLargeFloatScalar()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestVectorBuffer();
        Assert.Equal(1532 * sizeof(float), bufferObject.Length);
        Assert.True(bufferObject.IsScalar);

        // Check the buffer contents
        Span<float> result = bufferObject.AsFloatSpan();
        Assert.Equal(typeof(float), bufferObject.GetItemType());
        Assert.Equal(1532, result.Length);
    }

    [Fact]
    public void TestInt8MatrixBuffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestInt82dBuffer();
        Assert.Equal(sizeof(sbyte) * 3, 3, bufferObject.Length);
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsSByteSpan2D();
        Assert.Equal(typeof(sbyte), bufferObject.GetItemType());
        Assert.Equal(1, matrix[0, 0]);
        Assert.Equal(6, matrix[1, 2]);
    }

    [Fact]
    public void TestUInt8MatrixBuffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestUint82dBuffer();
        Assert.Equal(sizeof(byte) * 3, 3, bufferObject.Length);
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsByteSpan2D();
        Assert.Equal(typeof(byte), bufferObject.GetItemType());
        Assert.Equal(1, matrix[0, 0]);
        Assert.Equal(6, matrix[1, 2]);
    }

    [Fact]
    public void TestInt16MatrixBuffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestInt162dBuffer();
        Assert.Equal(sizeof(short) * 3, 3, bufferObject.Length);
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsInt16Span2D();
        Assert.Equal(typeof(short), bufferObject.GetItemType());
        Assert.Equal(1, matrix[0, 0]);
        Assert.Equal(6, matrix[1, 2]);
    }

    [Fact]
    public void TestUInt16MatrixBuffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestUint162dBuffer();
        Assert.Equal(sizeof(ushort) * 3, 3, bufferObject.Length);
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsUInt16Span2D();
        Assert.Equal(typeof(ushort), bufferObject.GetItemType());
        Assert.Equal(1, matrix[0, 0]);
        Assert.Equal(6, matrix[1, 2]);
    }

    [Fact]
    public void TestInt32MatrixBuffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestInt322dBuffer();
        Assert.Equal(sizeof(int) * 3, 3, bufferObject.Length);
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsInt32Span2D();
        Assert.Equal(typeof(int), bufferObject.GetItemType());
        Assert.Equal(1, matrix[0, 0]);
        Assert.Equal(6, matrix[1, 2]);
    }

    [Fact]
    public void TestUInt32MatrixBuffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestUint322dBuffer();
        Assert.Equal(sizeof(uint) * 3, 3, bufferObject.Length);
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsUInt32Span2D();
        Assert.Equal(typeof(uint), bufferObject.GetItemType());
        Assert.Equal(1U, matrix[0, 0]);
        Assert.Equal(6U, matrix[1, 2]);
    }

    [Fact]
    public void TestInt64MatrixBuffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestInt642dBuffer();
        Assert.Equal(sizeof(long) * 3, 3, bufferObject.Length);
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsInt64Span2D();
        Assert.Equal(typeof(long), bufferObject.GetItemType());
        Assert.Equal(1, matrix[0, 0]);
        Assert.Equal(6, matrix[1, 2]);
    }

    [Fact]
    public void TestUInt64MatrixBuffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestUint642dBuffer();
        Assert.Equal(sizeof(ulong) * 3, 3, bufferObject.Length);
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsUInt64Span2D();
        Assert.Equal(typeof(ulong), bufferObject.GetItemType());
        Assert.Equal(1UL, matrix[0, 0]);
        Assert.Equal(6UL, matrix[1, 2]);
    }

    [Fact]
    public void TestFloat16MatrixBuffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestFloat162dBuffer();
        Assert.Equal(2 * 2 * 3, bufferObject.Length);
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsHalfSpan2D();
        Assert.Equal(typeof(Half), bufferObject.GetItemType());
        Assert.Equal(1.1f, (float)matrix[0, 0], 0.01);
        Assert.Equal(6.6f, (float)matrix[1, 2], 0.01);
    }

    [Fact]
    public void TestFloat32MatrixBuffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestFloat322dBuffer();
        Assert.Equal(sizeof(float) * 2 * 3, bufferObject.Length);
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsFloatSpan2D();
        Assert.Equal(typeof(float), bufferObject.GetItemType());
        Assert.Equal(1.1, matrix[0, 0], 0.00001);
        Assert.Equal(6.6, matrix[1, 2], 0.00001);
    }

    [Fact]
    public void TestFloat64MatrixBuffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestFloat642dBuffer();
        Assert.Equal(sizeof(double) * 2 * 3, bufferObject.Length);
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsDoubleSpan2D();
        Assert.Equal(typeof(double), bufferObject.GetItemType());
        Assert.Equal(1.1, matrix[0, 0]);
        Assert.Equal(6.6, matrix[1, 2]);
    }

    [Fact]
    public void TestModificationViaSpan()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestGlobalBuffer();
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsInt32Span2D();
        Assert.Equal(0, matrix[0, 0]);

        matrix[0, 0] = 42;

        // Fetch the object again
        using var bufferObject2 = testModule.TestGlobalBuffer();
        var matrix2 = bufferObject2.AsInt32Span2D();
        Assert.Equal(42, matrix2[0, 0]);
    }

    [Fact]
    public void TestBytesAsBuffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestBytesAsBuffer();
        Assert.Equal(5, bufferObject.Length);
        Assert.True(bufferObject.IsScalar);
        var result = bufferObject.AsByteReadOnlySpan();
        Assert.Equal(typeof(byte), bufferObject.GetItemType());
        Assert.Equal((byte)'h', result[0]);
        Assert.Equal((byte)'o', result[4]);
    }

    [Fact]
    public void TestByteArrayAsBuffer()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestBytearrayAsBuffer();
        Assert.Equal(5, bufferObject.Length);
        Assert.True(bufferObject.IsScalar);
        var result = bufferObject.AsByteSpan();
        Assert.Equal(typeof(byte), bufferObject.GetItemType());
        Assert.Equal((byte)'h', result[0]);
        Assert.Equal((byte)'o', result[4]);
    }

    [Fact]
    public void TestNonBuffer()
    {
        var testModule = Env.TestBuffer();
        Assert.Throws<InvalidCastException>(testModule.TestNonBuffer);
    }

    [Fact]
    public void TestNonContiguousBuffer()
    {
        var testModule = Env.TestBuffer();
        using var array = testModule.TestNonContiguousBuffer();
        Assert.Equal(sizeof(int) * 3, array.Length);
        var result = array.AsInt32Span2D();
        Assert.Equal(typeof(int), array.GetItemType());
        Assert.Equal(1, result[0, 0]);
        Assert.Equal(3, result[0, 2]);
    }

    [Fact]
    public void TestTransposedBuffer()
    {
        // ndarray transposed buffer is not contiguous, this should raise a clean error
        var testModule = Env.TestBuffer();
        Assert.Throws<PythonInvocationException>(testModule.TestTransposedBuffer);
    }

    [Fact]
    public void TestBidirectionalBuffer()
    {
        var testModule = Env.TestBuffer();
        List<Int32> list = new() { 1, 2, 3, 4, 5 };
        using var bufferGenerator = testModule.SumOf2dArray(5);
        bufferGenerator.MoveNext();
        var bufferObject = bufferGenerator.Current;

        Assert.Equal(100, bufferObject.Length);
        Assert.Equal(2, bufferObject.Dimensions);
        var bufferAsSpan = bufferObject.AsInt32Span2D();

        // Copy the list to the buffer
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = 0; j < list.Count; j++)
            {
                bufferAsSpan[i, j] = list[i];
            }
        }
        // Get the sum
        bufferGenerator.MoveNext();
        // Get result
        var result = bufferGenerator.Return;
        Assert.Equal(75, result);
    }

#if NET9_0_OR_GREATER
    [Fact]
    public void TestNDim3Tensor()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestNdim3dBuffer();
        Assert.Equal(3, bufferObject.Dimensions);
        var tensor = bufferObject.AsTensorSpan<int>();
        Assert.Equal(typeof(int), bufferObject.GetItemType());
        Assert.Equal(1, tensor[0, 0, 0]);
        Assert.Equal(3, tensor[1, 2, 3]);
    }

    [Fact]
    public void TestNDim3Float32TensorProductPrimitive()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestNdim3dFloat32Buffer();
        var tensor = bufferObject.AsTensorSpan<float>();
        Assert.Equal(sizeof(int) * 3 * 4 * 5, bufferObject.Length);
#pragma warning disable SYSLIB5001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#if NET10_0_OR_GREATER
        var tmpTensor = Tensor.CreateFromShape<float>(tensor.Lengths);
#else
        var tmpTensor = Tensor.Create<float>(tensor.Lengths);
#endif
        var shapeProduct = (long)TensorPrimitives.Product(tensor.Lengths);
        Assert.Equal(shapeProduct, tensor.FlattenedLength);
        var result = Tensor.Multiply(tensor, 255.0f, tmpTensor);
#pragma warning restore SYSLIB5001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    }

    [Fact]
    public void TestNDim4Tensor(){
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestNdim4dBuffer();
        Assert.Equal(4, bufferObject.Dimensions);
        var tensor = bufferObject.AsTensorSpan<int>();
        Assert.Equal(typeof(int), bufferObject.GetItemType());
        Assert.Equal(1, tensor[0, 0, 0, 0]);
        Assert.Equal(3, tensor[1, 2, 3, 4]);
    }

    [Fact]
    public void TestNDim3ReadOnlyTensor()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestNdim3dBuffer();
        Assert.Equal(3, bufferObject.Dimensions);
        var tensor = bufferObject.AsReadOnlyTensorSpan<int>();
        Assert.Equal(typeof(int), bufferObject.GetItemType());
        Assert.Equal(1, tensor[0, 0, 0]);
        Assert.Equal(3, tensor[1, 2, 3]);
    }

    [Fact]
    public void TestNDim4ReadOnlyTensor()
    {
        var testModule = Env.TestBuffer();
        using var bufferObject = testModule.TestNdim4dBuffer();
        Assert.Equal(4, bufferObject.Dimensions);
        var tensor = bufferObject.AsInt32ReadOnlyTensorSpan();
        Assert.Equal(typeof(int), bufferObject.GetItemType());
        Assert.Equal(1, tensor[0, 0, 0, 0]);
        Assert.Equal(3, tensor[1, 2, 3, 4]);
    }

#endif
}
