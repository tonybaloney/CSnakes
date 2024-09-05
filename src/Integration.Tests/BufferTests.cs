namespace Integration.Tests;

public class BufferTests : IntegrationTestBase
{
    [Fact]
    public void TestInt32Buffer()
    {
        // SKip if < Python 3.12
        if (new Version(Env.Version.Split(' ')[0]) < new Version(3, 11, 0))
            return;

        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestInt32Buffer();
        Assert.Equal(20, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<Int32> result = bufferObject.AsInt32Scalar();
        Assert.Equal((int)1, result[0]);
        Assert.Equal((int)5, result[4]);
    }

    [Fact]
    public void TestUInt32Buffer()
    {
        // SKip if < Python 3.12
        if (new Version(Env.Version.Split(' ')[0]) < new Version(3, 11, 0))
            return;

        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestUint32Buffer();
        Assert.Equal(20, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<UInt32> result = bufferObject.AsUInt32Scalar();
        Assert.Equal((uint)1, result[0]);
        Assert.Equal((uint)5, result[4]);
    }

    [Fact]
    public void TestInt64Buffer()
    {
        // SKip if < Python 3.12
        if (new Version(Env.Version.Split(' ')[0]) < new Version(3, 11, 0))
            return;

        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestInt64Buffer();
        Assert.Equal(sizeof(long) * 5, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<Int64> result = bufferObject.AsInt64Scalar();
        Assert.Equal(1L, result[0]);
        Assert.Equal(5L, result[4]);
    }

    [Fact]
    public void TestUInt64Buffer()
    {
        // SKip if < Python 3.12
        if (new Version(Env.Version.Split(' ')[0]) < new Version(3, 11, 0))
            return;

        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestUint64Buffer();
        Assert.Equal(sizeof(long) * 5, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<UInt64> result = bufferObject.AsUInt64Scalar();
        Assert.Equal(1UL, result[0]);
        Assert.Equal(5UL, result[4]);
    }

    [Fact]
    public void TestFloat32Buffer()
    {
        // SKip if < Python 3.12
        if (new Version(Env.Version.Split(' ')[0]) < new Version(3, 11, 0))
            return;

        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestFloat32Buffer();
        Assert.Equal(20, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<float> result = bufferObject.AsFloatScalar();
        Assert.Equal(1.1f, result[0]);
        Assert.Equal(5.5f, result[4]);
    }

    [Fact]
    public void TestFloat64Buffer()
    {
        // SKip if < Python 3.12
        if (new Version(Env.Version.Split(' ')[0]) < new Version(3, 11, 0))
            return;

        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestFloat64Buffer();
        Assert.Equal(sizeof(double) * 5, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<double> result = bufferObject.AsDoubleScalar();
        Assert.Equal(1.1, result[0]);
        Assert.Equal(5.5, result[4]);
    }

    [Fact]
    public void TestBufferLargeFloatScalar()
    {
        // SKip if < Python 3.12
        if (new Version(Env.Version.Split(' ')[0]) < new Version(3, 11, 0))
        {
            return;
        }
        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestVectorBuffer();
        Assert.Equal(1532 * sizeof(float), bufferObject.Length); 
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<float> result = bufferObject.AsFloatScalar();
        Assert.Equal(1532, result.Length);
    }

    [Fact]
    public void TestFloat32MatrixBuffer()
    {
        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestFloat32MatrixBuffer();
        Assert.Equal(sizeof(float) * 100 * 100, bufferObject.Length); 
        Assert.Equal(2, bufferObject.Dimensions);
    }
}
