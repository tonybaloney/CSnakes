using CSnakes.Runtime.Python;
using System;
#if NET10_0_OR_GREATER
using System.Numerics.Tensors;
#else
using System.Numerics;
#endif

namespace Integration.Tests;

/// <summary>
/// Tests that exercise the Map/Do/CopyTo/CopyFrom API on buffer types with practical, real-world operations (numeric
/// reductions, in-place transforms, cross-buffer dot products, etc.).
/// </summary>
public class BufferOperationTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    // ────────────────────────────────────────────────────────────
    //  1D  PyArrayBuffer<float>
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void Sum_1D_Map()
    {
        // Map(func) – pure reduction over the buffer
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        var sum = buffer.Map(TensorPrimitives.Sum);

        Assert.Equal(15.0f, sum);
    }

    [Fact]
    public void DotProduct_1D_MapWithArray()
    {
        // Map(TArg, func) – TArg is float[]
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        float[] other = [5, 4, 3, 2, 1];
        // 1·5 + 2·4 + 3·3 + 4·2 + 5·1 = 35
        var dot = buffer.Map(other, static (span, arr) => TensorPrimitives.Dot(span, arr));

        Assert.Equal(35.0f, dot);
    }

    [Fact]
    public void InPlaceNegate_1D_Do()
    {
        // Do(func) – parameterless Do for in-place unary operation
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        buffer.Do(span => TensorPrimitives.Negate(span, span));

        Assert.Equal(-1.0f, buffer[0]);
        Assert.Equal(-2.0f, buffer[1]);
        Assert.Equal(-3.0f, buffer[2]);
        Assert.Equal(-4.0f, buffer[3]);
        Assert.Equal(-5.0f, buffer[4]);
    }

    [Fact]
    public void InPlaceScale_1D_DoWithScalar()
    {
        // Do(TArg, func) – TArg is a scalar value
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        buffer.Do(3.0f, static (span, factor) => TensorPrimitives.Multiply(span, factor, span));

        Assert.Equal(3.0f,  buffer[0]);
        Assert.Equal(6.0f,  buffer[1]);
        Assert.Equal(9.0f,  buffer[2]);
        Assert.Equal(12.0f, buffer[3]);
        Assert.Equal(15.0f, buffer[4]);
    }

    [Fact]
    public void InPlaceAdd_1D_DoWithArray()
    {
        // Do(TArg, func) – TArg is float[]
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        float[] addends = [10, 20, 30, 40, 50];
        buffer.Do(addends, static (span, arr) => TensorPrimitives.Add(span, arr, span));

        Assert.Equal(11.0f, buffer[0]);
        Assert.Equal(22.0f, buffer[1]);
        Assert.Equal(33.0f, buffer[2]);
        Assert.Equal(44.0f, buffer[3]);
        Assert.Equal(55.0f, buffer[4]);
    }

    [Fact]
    public void CopyTo_1D()
    {
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        var dest = new float[5];
        buffer.CopyTo(dest);

        Assert.Equal([1.0f, 2.0f, 3.0f, 4.0f, 5.0f], dest);
    }

    [Fact]
    public void CopyFrom_1D()
    {
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // writable numpy array

        float[] source = [10, 20, 30, 40, 50];
        buffer.CopyFrom(source);

        Assert.Equal(10.0f, buffer[0]);
        Assert.Equal(50.0f, buffer[4]);
    }

#if NET10_0_OR_GREATER
    [Fact]
    public void Sum_1D_TensorPrimitives()
    {
        // Map(func) with TensorPrimitives.Sum
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer();

        var sum = buffer.Map(TensorPrimitives.Sum);

        Assert.Equal(15.0f, sum);
    }

    [Fact]
    public void DotProduct_1D_MapWithSpan()
    {
        // Map(TArg, func) where TArg is ReadOnlySpan<float> — exercises "allows ref struct"
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        ReadOnlySpan<float> other = [5, 4, 3, 2, 1];
        var dot = buffer.Map(other, static (span, o) => TensorPrimitives.Dot(span, o));

        Assert.Equal(35.0f, dot);
    }

    [Fact]
    public void CosineSimilarity_1D_MapWithArray()
    {
        // Self-similarity should be 1.0
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        float[] same = [1, 2, 3, 4, 5];
        var similarity = buffer.Map(same, static (span, arr) => TensorPrimitives.CosineSimilarity(span, arr));

        Assert.Equal(1.0f, similarity, tolerance: 1e-6f);
    }

    [Fact]
    public void InPlaceAdd_1D_DoWithSpan()
    {
        // Do(TArg, func) where TArg is ReadOnlySpan<float> — exercises "allows ref struct"
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        ReadOnlySpan<float> addends = [10, 20, 30, 40, 50];
        buffer.Do(addends, static (span, a) => TensorPrimitives.Add(span, a, span));

        Assert.Equal(11.0f, buffer[0]);
        Assert.Equal(55.0f, buffer[4]);
    }

    [Fact]
    public void DotProduct_1D_NestedMap()
    {
        // Two Python-backed buffers; nested Map passes outer span as ref-struct TArg
        var module = Env.TestBuffer();
        using var buffer1 = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]
        using var buffer2 = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        // [1,2,3,4,5] · [1,2,3,4,5] = 1+4+9+16+25 = 55
        var dot = buffer1.Map(buffer2, static (span1, buffer2) => buffer2.Map(span1, TensorPrimitives.Dot));

        Assert.Equal(55.0f, dot);
    }

    [Fact]
    public void WeightedDot_1D_MapWith2Args()
    {
        // Map(TArg1, TArg2, func) — pure expression: dot(buf, a) + dot(buf, b)
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        ReadOnlySpan<float> a = [1, 0, 1, 0, 1]; // dot = 1+3+5 = 9
        ReadOnlySpan<float> b = [0, 1, 0, 1, 0]; // dot = 2+4 = 6
        var result = buffer.Map(a, b, static (span, aa, bb) =>
            TensorPrimitives.Dot(span, aa) + TensorPrimitives.Dot(span, bb));

        Assert.Equal(15.0f, result); // 9 + 6
    }

    [Fact]
    public void WeightedDistance_1D_MapWith3Args()
    {
        // Map(TArg1, TArg2, TArg3, func) — pure expression using 3 extra spans
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        ReadOnlySpan<float> a = [1, 0, 1, 0, 1];
        ReadOnlySpan<float> b = [0, 1, 0, 1, 0];
        ReadOnlySpan<float> c = [1, 1, 1, 1, 1];
        // dot(buf,a) + dot(buf,b) + dot(buf,c) = 9 + 6 + 15 = 30
        var result = buffer.Map(a, b, c, static (span, a, b, c) => TensorPrimitives.Dot(span, a)
                                                                 + TensorPrimitives.Dot(span, b)
                                                                 + TensorPrimitives.Dot(span, c));

        Assert.Equal(30.0f, result);
    }

    [Fact]
    public void FusedMultiplyAdd_1D_DoWith2Args()
    {
        // Do(TArg1, TArg2, action) — ternary in-place FMA: buf = buf * y + z
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        ReadOnlySpan<float> y = [2, 2, 2, 2, 2];
        ReadOnlySpan<float> z = [10, 20, 30, 40, 50];
        // FMA: [1*2+10, 2*2+20, 3*2+30, 4*2+40, 5*2+50] = [12, 24, 36, 48, 60]
        buffer.Do(y, z, static (span, y, z) => TensorPrimitives.FusedMultiplyAdd(span, y, z, span));

        Assert.Equal(12.0f, buffer[0]);
        Assert.Equal(24.0f, buffer[1]);
        Assert.Equal(36.0f, buffer[2]);
        Assert.Equal(48.0f, buffer[3]);
        Assert.Equal(60.0f, buffer[4]);
    }

    [Fact]
    public void AddMultiply_1D_DoWith2Args()
    {
        // Do(TArg1, TArg2, action) — ternary in-place AddMultiply: dest = (buf + y) * z
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        ReadOnlySpan<float> addend = [10, 20, 30, 40, 50];
        ReadOnlySpan<float> multiplier = [2, 2, 2, 2, 2];
        // AddMultiply: [(1+10)*2, (2+20)*2, (3+30)*2, (4+40)*2, (5+50)*2] = [22, 44, 66, 88, 110]
        buffer.Do(addend, multiplier, static (span, a, m) => TensorPrimitives.AddMultiply(span, a, m, span));

        Assert.Equal(22.0f, buffer[0]);
        Assert.Equal(44.0f, buffer[1]);
        Assert.Equal(66.0f, buffer[2]);
        Assert.Equal(88.0f, buffer[3]);
        Assert.Equal(110.0f, buffer[4]);
    }

    [Fact]
    public void Lerp_1D_DoWith2Args()
    {
        // Do(TArg1, TArg2, action) — ternary Lerp writing to an external destination
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        ReadOnlySpan<float> y = [11, 12, 13, 14, 15];
        Span<float> result = new float[5];
        // Lerp at 0.5: midpoint between x and y = [6, 7, 8, 9, 10]
        buffer.Do(y, result, static (span, y, dest) => TensorPrimitives.Lerp(span, y, 0.5f, dest));

        Assert.Equal([6.0f, 7.0f, 8.0f, 9.0f, 10.0f], result.ToArray());
    }

    [Fact]
    public void SinCos_1D_DoWith2Args()
    {
        // Do(TArg1, TArg2, action) — multi-output: compute sin and cos simultaneously
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        Span<float> sin = new float[5];
        Span<float> cos = new float[5];
        buffer.Do(sin, cos, static (span, sin, cos) => TensorPrimitives.SinCos(span, sin, cos));

        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(MathF.Sin(i + 1), sin[i], tolerance: 1e-6f);
            Assert.Equal(MathF.Cos(i + 1), cos[i], tolerance: 1e-6f);
        }
    }

    [Fact]
    public void DivRem_1D_DoWith3Args()
    {
        // Do(TArg1, TArg2, TArg3, action) — binary op with 2 output destinations
        var module = Env.TestBuffer();
        using var buffer = (PyArrayBuffer<float>)module.TestFloat32IntBuffer(); // [1,2,3,4,5]

        ReadOnlySpan<float> divisor = [3, 3, 3, 3, 3];
        Span<float> quotient = new float[5];
        Span<float> remainder = new float[5];
        buffer.Do(divisor, quotient, remainder, static (span, div, quot, rem) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                quot[i] = MathF.Truncate(span[i] / div[i]);
                rem[i] = span[i] % div[i];
            }
        });

        // 1/3 = 0 rem 1, 2/3 = 0 rem 2, 3/3 = 1 rem 0, 4/3 = 1 rem 1, 5/3 = 1 rem 2
        Assert.Equal([0.0f, 0.0f, 1.0f, 1.0f, 1.0f], quotient.ToArray());
        Assert.Equal([1.0f, 2.0f, 0.0f, 1.0f, 2.0f], remainder.ToArray());
    }
#endif // NET10_0_OR_GREATER

    // ────────────────────────────────────────────────────────────
    //  2D  PyArray2DBuffer<float>
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void RowSums_2D_Map()
    {
        // Map(func) – compute per-row sums over a 2D buffer
        var module = Env.TestBuffer();
        using var buffer = (PyArray2DBuffer<float>)module.TestFloat32Int2dBuffer(); // [[1,2,3],[4,5,6]]

        var sums = buffer.Map(static span =>
        {
            var result = new float[span.Height];
            for (var row = 0; row < span.Height; row++)
            {
                float s = 0;
                for (var col = 0; col < span.Width; col++)
                    s += span[row, col];
                result[row] = s;
            }
            return result;
        });

        Assert.Equal(6.0f, sums[0]);   // 1+2+3
        Assert.Equal(15.0f, sums[1]);  // 4+5+6
    }

    [Fact]
    public void Clear_2D_Do()
    {
        // Do(func) – parameterless Do for in-place clear
        var module = Env.TestBuffer();
        using var buffer = (PyArray2DBuffer<float>)module.TestFloat32Int2dBuffer(); // [[1,2,3],[4,5,6]]

        buffer.Do(static span => span.Clear());

        Assert.Equal(0f, buffer[0, 0]);
        Assert.Equal(0f, buffer[0, 2]);
        Assert.Equal(0f, buffer[1, 0]);
        Assert.Equal(0f, buffer[1, 2]);
    }

    [Fact]
    public void Fill_2D_DoWithScalar()
    {
        // Do(TArg, func) – fill entire matrix with a value
        var module = Env.TestBuffer();
        using var buffer = (PyArray2DBuffer<float>)module.TestFloat32Int2dBuffer(); // [[1,2,3],[4,5,6]]

        buffer.Do(42.0f, static (span, val) =>
        {
            for (var row = 0; row < span.Height; row++)
                for (var col = 0; col < span.Width; col++)
                    span[row, col] = val;
        });

        Assert.Equal(42f, buffer[0, 0]);
        Assert.Equal(42f, buffer[1, 2]);
    }

    [Fact]
    public void Flatten_2D_Map()
    {
        // Map(func) – flatten to 1D array (row-major)
        var module = Env.TestBuffer();
        using var buffer = (PyArray2DBuffer<float>)module.TestFloat32Int2dBuffer(); // [[1,2,3],[4,5,6]]

        var flat = buffer.Map(static span =>
        {
            var result = new float[span.Height * span.Width];
            for (var row = 0; row < span.Height; row++)
                for (var col = 0; col < span.Width; col++)
                    result[row * span.Width + col] = span[row, col];
            return result;
        });

        Assert.Equal([1f, 2f, 3f, 4f, 5f, 6f], flat);
    }

    [Fact]
    public void CopyTo_2D()
    {
        var module = Env.TestBuffer();
        using var buffer = (PyArray2DBuffer<float>)module.TestFloat32Int2dBuffer(); // [[1,2,3],[4,5,6]]

        var dest = new float[6];
        buffer.CopyTo(dest);

        Assert.Equal([1f, 2f, 3f, 4f, 5f, 6f], dest);
    }

    // ────────────────────────────────────────────────────────────
    //  Tensor  PyTensorBuffer<float>  — NET10+ only
    // ────────────────────────────────────────────────────────────

#if NET10_0_OR_GREATER
    [Fact]
    public void Sum_Tensor_Map()
    {
        // Map(func) – sum all elements of a 3D tensor
        // test_ndim_3d_float32_buffer() returns np.ones((3,4,5), float32)
        var module = Env.TestBuffer();
        using var buffer = (PyTensorBuffer<float>)module.TestNdim3dFloat32Buffer();

        var sum = buffer.Map(static span =>
        {
            var flat = new float[span.FlattenedLength];
            span.FlattenTo(flat);
            return TensorPrimitives.Sum(flat);
        });

        Assert.Equal(60f, sum); // 3×4×5 ones
    }

    [Fact]
    public void InPlaceNegate_Tensor_Do()
    {
        // Do(func) – parameterless Do for in-place negate on tensor
        var module = Env.TestBuffer();
        using var buffer = (PyTensorBuffer<float>)module.TestNdim3dFloat32Buffer(); // all 1.0f

        buffer.Do(static span => Tensor.Negate(span, span));

        Assert.Equal(-1f, buffer[0, 0, 0]);
        Assert.Equal(-1f, buffer[2, 3, 4]);
    }

    [Fact]
    public void InPlaceScale_Tensor_DoWithScalar()
    {
        // Do(TArg, func) – multiply all elements by a scalar
        var module = Env.TestBuffer();
        using var buffer = (PyTensorBuffer<float>)module.TestNdim3dFloat32Buffer(); // all 1.0f

        buffer.Do(255f, static (span, factor) => Tensor.Multiply(span, factor, span));

        Assert.Equal(255f, buffer[0, 0, 0]);
        Assert.Equal(255f, buffer[2, 3, 4]);
    }

    [Fact]
    public void Sum_Tensor_MapWith1Arg()
    {
        // Map(TArg, func) — pass a scalar multiplier, return sum of scaled elements
        var module = Env.TestBuffer();
        using var buffer = (PyTensorBuffer<float>)module.TestNdim3dFloat32Buffer(); // all 1.0f, shape (3,4,5)

        var sum = buffer.Map(3.0f, static (span, factor) => Tensor.Sum<float>(Tensor.Multiply(span, factor)));

        Assert.Equal(180f, sum); // 60 ones × 3
    }

    [Fact]
    public void WeightedSum_Tensor_MapWith2Args()
    {
        // Map(TArg1, TArg2, func) — weighted sum: sum(tensor * weights) + bias
        var module = Env.TestBuffer();
        using var buffer = (PyTensorBuffer<float>)module.TestNdim3dFloat32Buffer(); // all 1.0f, shape (3,4,5)

        var weights = Tensor.CreateFromShape<float>(buffer.Lengths);
        weights.AsTensorSpan().Fill(2.0f);
        var result = buffer.Map(weights.AsReadOnlyTensorSpan(), 10.0f,
                                static (span, w, bias) =>  Tensor.Sum<float>(Tensor.Multiply(span, w)) + bias);

        Assert.Equal(130f, result); // 60 × (1×2) + 10 = 130
    }

    [Fact]
    public void TripleWeightedSum_Tensor_MapWith3Args()
    {
        // Map(TArg1, TArg2, TArg3, func) — sum(tensor * w1 * w2) + bias
        var module = Env.TestBuffer();
        using var buffer = (PyTensorBuffer<float>)module.TestNdim3dFloat32Buffer(); // all 1.0f, shape (3,4,5)

        var w1 = Tensor.CreateFromShape<float>(buffer.Lengths);
        var w2 = Tensor.CreateFromShape<float>(buffer.Lengths);
        w1.AsTensorSpan().Fill(2.0f);
        w2.AsTensorSpan().Fill(3.0f);
        var result = buffer.Map(w1.AsReadOnlyTensorSpan(), w2.AsReadOnlyTensorSpan(), 5.0f, static (span, a, b, bias) =>
            Tensor.Sum<float>(Tensor.Multiply(Tensor.Multiply(span, a), b)) + bias);

        Assert.Equal(365f, result); // 60 × (1×2×3) + 5 = 365
    }

    [Fact]
    public void ScaleAddStore_Tensor_DoWith3Args()
    {
        // Do(TArg1, TArg2, TArg3, action) — compute (tensor + addend) * scale, write to dest
        var module = Env.TestBuffer();
        using var buffer = (PyTensorBuffer<float>)module.TestNdim3dFloat32Buffer(); // all 1.0f, shape (3,4,5)

        var addend = Tensor.CreateFromShape<float>(buffer.Lengths);
        var result = Tensor.CreateFromShape<float>(buffer.Lengths);
        addend.AsTensorSpan().Fill(4f);
        buffer.Do(addend.AsReadOnlyTensorSpan(), 2f, result.AsTensorSpan(),
                  static (span, add, factor, dest) =>
                  {
                      Tensor.Add(span, add, dest);         // dest = 1 + 4 = 5
                      Tensor.Multiply(dest, factor, dest);  // dest = 5 * 2 = 10
                  });

        Assert.Equal(10f, result[0, 0, 0]);
        Assert.Equal(10f, result[2, 3, 4]);
    }

    [Fact]
    public void CopyTo_Tensor()
    {
        var module = Env.TestBuffer();
        using var buffer = (PyTensorBuffer<float>)module.TestNdim3dFloat32Buffer(); // ones(3,4,5)

        var dest = Tensor.CreateFromShape<float>(buffer.Lengths);
        buffer.CopyTo(dest.AsTensorSpan());

        // Verify a few values from the copy
        Assert.Equal(1f, dest[0, 0, 0]);
        Assert.Equal(1f, dest[2, 3, 4]);
    }

    [Fact]
    public void AddAndScale_Tensor_DoWith2Args()
    {
        // Do(TArg1, TArg2, action) — add a tensor then scale by a factor
        // Demonstrates 2-arg Do with mixed ref-struct and scalar args
        var module = Env.TestBuffer();
        using var buffer = (PyTensorBuffer<float>)module.TestNdim3dFloat32Buffer(); // all 1.0f, shape (3,4,5)

        var addend = Tensor.CreateFromShape<float>(buffer.Lengths);
        addend.AsTensorSpan().Fill(2f);
        buffer.Do(addend.AsReadOnlyTensorSpan(), 3f, static (span, add, factor) =>
        {
            Tensor.Add(span, add, span);      // span = 1 + 2 = 3
            Tensor.Multiply(span, factor, span); // span = 3 * 3 = 9
        });

        Assert.Equal(9f, buffer[0, 0, 0]);
        Assert.Equal(9f, buffer[2, 3, 4]);
    }

    [Fact]
    public void BinaryOpToSeparateDestination_Tensor_DoWith2Args()
    {
        // Do(TArg1, TArg2, action) — add two tensors, writing to a separate destination
        var module = Env.TestBuffer();
        using var buffer = (PyTensorBuffer<float>)module.TestNdim3dFloat32Buffer(); // all 1.0f, shape (3,4,5)

        var other = Tensor.CreateFromShape<float>(buffer.Lengths);
        var result = Tensor.CreateFromShape<float>(buffer.Lengths);
        other.AsTensorSpan().Fill(4f);
        buffer.Do(other.AsReadOnlyTensorSpan(), result.AsTensorSpan(),
                  static (span, o, dest) => Tensor.Add(span, o, dest)); // dest = 1 + 4 = 5

        Assert.Equal(5f, result[0, 0, 0]);
        Assert.Equal(5f, result[2, 3, 4]);
    }
#endif // NET10_0_OR_GREATER
}

#if !NET10_0_OR_GREATER
/// <summary>
/// Polyfill implementations of tensor operations when running on .NET versions prior to 10.0.
/// </summary>
file static class TensorPrimitives
{
    public static T Sum<T>(ReadOnlySpan<T> x) where T : INumber<T>
    {
        var result = T.Zero;
        foreach (var v in x)
            result += v;
        return result;
    }

    public static T Dot<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : INumber<T>
    {
        var result = T.Zero;
        for (var i = 0; i < x.Length; i++)
            result += x[i] * y[i];
        return result;
    }

    public static void Negate<T>(ReadOnlySpan<T> x, Span<T> dest) where T : INumber<T>
    {
        for (var i = 0; i < x.Length; i++)
            dest[i] = -x[i];
    }

    public static void Multiply<T>(ReadOnlySpan<T> x, T y, Span<T> dest) where T : INumber<T>
    {
        for (var i = 0; i < x.Length; i++)
            dest[i] = x[i] * y;
    }

    public static void Add<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> dest) where T : INumber<T>
    {
        for (var i = 0; i < x.Length; i++)
            dest[i] = x[i] + y[i];
    }
}
#endif
