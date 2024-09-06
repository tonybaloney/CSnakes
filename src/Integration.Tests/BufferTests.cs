namespace Integration.Tests;

public class BufferTests : IntegrationTestBase
{
    [Fact]
    public void TestInt32Buffer()
    {
        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestInt32Buffer();
        Assert.Equal(20, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<Int32> result = bufferObject.AsInt32Span();
        Assert.Equal((int)1, result[0]);
        Assert.Equal((int)5, result[4]);
    }

    [Fact]
    public void TestUInt32Buffer()
    {
        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestUint32Buffer();
        Assert.Equal(20, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<UInt32> result = bufferObject.AsUInt32Span();
        Assert.Equal((uint)1, result[0]);
        Assert.Equal((uint)5, result[4]);
    }

    [Fact]
    public void TestInt64Buffer()
    {
        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestInt64Buffer();
        Assert.Equal(sizeof(long) * 5, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<Int64> result = bufferObject.AsInt64Span();
        Assert.Equal(1L, result[0]);
        Assert.Equal(5L, result[4]);
    }

    [Fact]
    public void TestUInt64Buffer()
    {
        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestUint64Buffer();
        Assert.Equal(sizeof(long) * 5, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<UInt64> result = bufferObject.AsUInt64Span();
        Assert.Equal(1UL, result[0]);
        Assert.Equal(5UL, result[4]);
    }

    [Fact]
    public void TestFloat32Buffer()
    {
        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestFloat32Buffer();
        Assert.Equal(20, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<float> result = bufferObject.AsFloatSpan();
        Assert.Equal(1.1f, result[0]);
        Assert.Equal(5.5f, result[4]);
    }

    [Fact]
    public void TestFloat64Buffer()
    {
        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestFloat64Buffer();
        Assert.Equal(sizeof(double) * 5, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<double> result = bufferObject.AsDoubleSpan();
        Assert.Equal(1.1, result[0]);
        Assert.Equal(5.5, result[4]);
    }

    [Fact]
    public void TestBufferLargeFloatScalar()
    {
        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestVectorBuffer();
        Assert.Equal(1532 * sizeof(float), bufferObject.Length); 
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<float> result = bufferObject.AsFloatSpan();
        Assert.Equal(1532, result.Length);
    }

    [Fact]
    public void TestInt32MatrixBuffer()
    {
        var testModule = Env.TestBuffer();
        var bufferObject = testModule.Test2dInt32MatrixBuffer();
        Assert.Equal(sizeof(Int32) * 3, 3, bufferObject.Length);
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsInt32Span2D();
        Assert.Equal(1, matrix[0, 0]);
        Assert.Equal(6, matrix[1, 2]);
    }

    [Fact]
    public void TestFloat32MatrixBuffer()
    {
        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestFloat32MatrixBuffer();
        Assert.Equal(sizeof(float) * 100 * 100, bufferObject.Length); 
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsFloatSpan2D();
        Assert.True(matrix[0, 0] < 1);
    }

    [Fact]
    public void TestModificationViaSpan()
    {
        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestGlobalBuffer();
        Assert.Equal(2, bufferObject.Dimensions);
        var matrix = bufferObject.AsInt32Span2D();
        Assert.Equal(0, matrix[0, 0]);

        matrix[0, 0] = 42;

        // Fetch the object again
        var bufferObject2 = testModule.TestGlobalBuffer();
        var matrix2 = bufferObject2.AsInt32Span2D();
        Assert.Equal(42, matrix2[0, 0]);
    }
}
