namespace Integration.Tests;

public class BufferTests : IntegrationTestBase
{
    [Fact]
    public void TestSimpleBuffer()
    {
        // SKip if < Python 3.12
        if (new Version(Env.Version.Split(' ')[0]) < new Version(3, 11, 0))
        {
            return;
        }
        var testModule = Env.TestBuffer();
        var bufferObject = testModule.TestSimpleBuffer();
        Assert.Equal(20, bufferObject.Length); // 5 * sizeof(int)
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<Int32> result = bufferObject.AsInt32Scalar();
        Assert.Equal((int)1, result[0]);
        Assert.Equal((int)5, result[4]);

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
        var bufferObject = testModule.TestFloatScalar();
        Assert.Equal(1532 * sizeof(float), bufferObject.Length); 
        Assert.True(bufferObject.Scalar);

        // Check the buffer contents
        Span<float> result = bufferObject.AsFloatScalar();
        Assert.Equal(1532, result.Length);
    }
}
