using CSnakes.Parser.Types;

namespace CSnakes.Tests;

public abstract class PythonTypeSpecTestBase<T, TTestData>
    where T : PythonTypeSpec
    where TTestData : IPythonTypeSpecTestData<T>
{
    [Fact]
    public void Name_IsCorrect()
    {
        Assert.Equal(TTestData.ExpectedName, TTestData.CreateInstance().Name);
    }

    [Fact]
    public void ToString_ReturnsExpectedValue()
    {
        Assert.Equal(TTestData.ExpectedToString, TTestData.CreateInstance().ToString());
    }

    [Fact]
    public void Equality_HasValueSemantics()
    {
        var a = TTestData.CreateInstance();
        var b = TTestData.CreateEquivalentInstance();

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void GetHashCode_IsConsistent()
    {
        var a = TTestData.CreateInstance();
        var b = TTestData.CreateEquivalentInstance();

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_SameMetadata_Equal()
    {
        var a = TTestData.CreateInstance() with { Metadata = [1] };
        var b = TTestData.CreateEquivalentInstance() with { Metadata = [1] };

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Equality_DifferentMetadata_NotEqual()
    {
        var a = TTestData.CreateInstance() with { Metadata = [1] };
        var b = TTestData.CreateEquivalentInstance() with { Metadata = [2] };

        Assert.NotEqual(a, b);
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Equality_DifferentMetadata_EqualWithoutMetadata()
    {
        var a = TTestData.CreateInstance() with { Metadata = [1] };
        var b = TTestData.CreateEquivalentInstance() with { Metadata = [2] };
        var at = a with { Metadata = default };
        var bt = b with { Metadata = default };

        Assert.Equal(at, bt);
        Assert.True(at == bt);
        Assert.False(at != bt);
    }

    [Fact]
    public void ValueEquality_DifferentInstances_NotEqual()
    {
        var a = TTestData.CreateInstance();
        var b = TTestData.CreateDifferentInstance();

        Assert.NotEqual(a, b);
        Assert.False(a == b);
        Assert.True(a != b);
    }
}
