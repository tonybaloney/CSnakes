using CSnakes.Parser.Types;

namespace CSnakes.Tests;

public class PythonTypeSpecTests
{
    [Fact]
    public void Singletons_AreExpectedTypes()
    {
        _ = Assert.IsType<AnyType>(PythonTypeSpec.Any);
        _ = Assert.IsType<NoneType>(PythonTypeSpec.None);
        _ = Assert.IsType<IntType>(PythonTypeSpec.Int);
        _ = Assert.IsType<StrType>(PythonTypeSpec.Str);
        _ = Assert.IsType<FloatType>(PythonTypeSpec.Float);
        _ = Assert.IsType<BoolType>(PythonTypeSpec.Bool);
        _ = Assert.IsType<BytesType>(PythonTypeSpec.Bytes);
        _ = Assert.IsType<BufferType>(PythonTypeSpec.Buffer);
        _ = Assert.IsType<AnyType>(Assert.IsType<VariadicTupleType>(PythonTypeSpec.Tuple).Of);
    }

    public class NoneTypeTests
    {
        [Fact]
        public void Name_IsCorrect()
        {
            Assert.Equal("NoneType", PythonTypeSpec.None.Name);
        }

        [Fact]
        public void ToString_ReturnsName()
        {
            Assert.Equal("NoneType", PythonTypeSpec.None.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var a = PythonTypeSpec.None;
            var b = PythonTypeSpec.None;

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var a = PythonTypeSpec.None;
            var b = PythonTypeSpec.None;

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = PythonTypeSpec.None with { Metadata = [1] };
            var b = PythonTypeSpec.None with { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = PythonTypeSpec.None with { Metadata = [1] };
            var b = PythonTypeSpec.None with { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = PythonTypeSpec.None with { Metadata = [1] };
            var b = PythonTypeSpec.None with { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class AnyTypeTests
    {
        [Fact]
        public void Name_IsCorrect()
        {
            Assert.Equal("Any", PythonTypeSpec.Any.Name);
        }

        [Fact]
        public void ToString_ReturnsName()
        {
            Assert.Equal("Any", PythonTypeSpec.Any.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var a = PythonTypeSpec.Any;
            var b = PythonTypeSpec.Any;

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var a = PythonTypeSpec.Any;
            var b = PythonTypeSpec.Any;

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = PythonTypeSpec.Any with { Metadata = [1] };
            var b = PythonTypeSpec.Any with { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = PythonTypeSpec.Any with { Metadata = [1] };
            var b = PythonTypeSpec.Any with { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = PythonTypeSpec.Any with { Metadata = [1] };
            var b = PythonTypeSpec.Any with { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class IntTypeTests
    {
        [Fact]
        public void Name_IsCorrect()
        {
            Assert.Equal("int", PythonTypeSpec.Int.Name);
        }

        [Fact]
        public void ToString_ReturnsName()
        {
            Assert.Equal("int", PythonTypeSpec.Int.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var a = PythonTypeSpec.Int;
            var b = PythonTypeSpec.Int;

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var a = PythonTypeSpec.Int;
            var b = PythonTypeSpec.Int;

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = PythonTypeSpec.Int with { Metadata = [1] };
            var b = PythonTypeSpec.Int with { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = PythonTypeSpec.Int with { Metadata = [1] };
            var b = PythonTypeSpec.Int with { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = PythonTypeSpec.Int with { Metadata = [1] };
            var b = PythonTypeSpec.Int with { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class StrTypeTests
    {
        [Fact]
        public void Name_IsCorrect()
        {
            Assert.Equal("str", PythonTypeSpec.Str.Name);
        }

        [Fact]
        public void ToString_ReturnsName()
        {
            Assert.Equal("str", PythonTypeSpec.Str.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var a = PythonTypeSpec.Str;
            var b = PythonTypeSpec.Str;

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var a = PythonTypeSpec.Str;
            var b = PythonTypeSpec.Str;

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = PythonTypeSpec.Str with { Metadata = [1] };
            var b = PythonTypeSpec.Str with { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = PythonTypeSpec.Str with { Metadata = [1] };
            var b = PythonTypeSpec.Str with { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = PythonTypeSpec.Str with { Metadata = [1] };
            var b = PythonTypeSpec.Str with { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class FloatTypeTests
    {
        [Fact]
        public void Name_IsCorrect()
        {
            Assert.Equal("float", PythonTypeSpec.Float.Name);
        }

        [Fact]
        public void ToString_ReturnsName()
        {
            Assert.Equal("float", PythonTypeSpec.Float.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var a = PythonTypeSpec.Float;
            var b = PythonTypeSpec.Float;

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var a = PythonTypeSpec.Float;
            var b = PythonTypeSpec.Float;

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = PythonTypeSpec.Float with { Metadata = [1] };
            var b = PythonTypeSpec.Float with { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = PythonTypeSpec.Float with { Metadata = [1] };
            var b = PythonTypeSpec.Float with { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = PythonTypeSpec.Float with { Metadata = [1] };
            var b = PythonTypeSpec.Float with { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class BoolTypeTests
    {
        [Fact]
        public void Name_IsCorrect()
        {
            Assert.Equal("bool", PythonTypeSpec.Bool.Name);
        }

        [Fact]
        public void ToString_ReturnsName()
        {
            Assert.Equal("bool", PythonTypeSpec.Bool.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var a = PythonTypeSpec.Bool;
            var b = PythonTypeSpec.Bool;

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var a = PythonTypeSpec.Bool;
            var b = PythonTypeSpec.Bool;

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = PythonTypeSpec.Bool with { Metadata = [1] };
            var b = PythonTypeSpec.Bool with { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = PythonTypeSpec.Bool with { Metadata = [1] };
            var b = PythonTypeSpec.Bool with { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = PythonTypeSpec.Bool with { Metadata = [1] };
            var b = PythonTypeSpec.Bool with { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class BytesTypeTests
    {
        [Fact]
        public void Name_IsCorrect()
        {
            Assert.Equal("bytes", PythonTypeSpec.Bytes.Name);
        }

        [Fact]
        public void ToString_ReturnsName()
        {
            Assert.Equal("bytes", PythonTypeSpec.Bytes.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var a = PythonTypeSpec.Bytes;
            var b = PythonTypeSpec.Bytes;

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var a = PythonTypeSpec.Bytes;
            var b = PythonTypeSpec.Bytes;

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = PythonTypeSpec.Bytes with { Metadata = [1] };
            var b = PythonTypeSpec.Bytes with { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = PythonTypeSpec.Bytes with { Metadata = [1] };
            var b = PythonTypeSpec.Bytes with { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = PythonTypeSpec.Bytes with { Metadata = [1] };
            var b = PythonTypeSpec.Bytes with { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class BufferTypeTests
    {
        [Fact]
        public void Name_IsCorrect()
        {
            Assert.Equal("Buffer", PythonTypeSpec.Buffer.Name);
        }

        [Fact]
        public void ToString_ReturnsName()
        {
            Assert.Equal("Buffer", PythonTypeSpec.Buffer.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var a = PythonTypeSpec.Buffer;
            var b = PythonTypeSpec.Buffer;

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var a = PythonTypeSpec.Buffer;
            var b = PythonTypeSpec.Buffer;

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = PythonTypeSpec.Buffer with { Metadata = [1] };
            var b = PythonTypeSpec.Buffer with { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = PythonTypeSpec.Buffer with { Metadata = [1] };
            var b = PythonTypeSpec.Buffer with { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = PythonTypeSpec.Buffer with { Metadata = [1] };
            var b = PythonTypeSpec.Buffer with { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class SequenceTypeTests
    {
        [Fact]
        public void Constructor_SetsTypeArgument()
        {
            var ofType = PythonTypeSpec.Int;
            var type = new SequenceType(ofType);

            Assert.Equal("Sequence", type.Name);
            Assert.Equal(ofType, type.Of);
        }

        [Fact]
        public void ToString_ReturnsFullyFormattedName()
        {
            var type = new SequenceType(PythonTypeSpec.Str);

            Assert.Equal("Sequence[str]", type.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var a = new SequenceType(PythonTypeSpec.Int);
            var b = new SequenceType(PythonTypeSpec.Int);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void ValueEquality_DifferentElementTypes_NotEqual()
        {
            var a = new SequenceType(PythonTypeSpec.Int);
            var b = new SequenceType(PythonTypeSpec.Str);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var a = new SequenceType(PythonTypeSpec.Int);
            var b = new SequenceType(PythonTypeSpec.Int);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ImplementsISequenceType()
        {
            var ofType = PythonTypeSpec.Int;
            var type = new SequenceType(ofType);

            Assert.IsAssignableFrom<ISequenceType>(type);
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = new SequenceType(PythonTypeSpec.Int) { Metadata = [1] };
            var b = new SequenceType(PythonTypeSpec.Int) { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = new SequenceType(PythonTypeSpec.Int) { Metadata = [1] };
            var b = new SequenceType(PythonTypeSpec.Int) { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = new SequenceType(PythonTypeSpec.Int) { Metadata = [1] };
            var b = new SequenceType(PythonTypeSpec.Int) { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class ListTypeTests
    {
        [Fact]
        public void Constructor_SetsTypeArgument()
        {
            var ofType = PythonTypeSpec.Int;
            var type = new ListType(ofType);

            Assert.Equal("list", type.Name);
            Assert.Equal(ofType, type.Of);
        }

        [Fact]
        public void ToString_ReturnsFullyFormattedName()
        {
            var type = new ListType(PythonTypeSpec.Str);

            Assert.Equal("list[str]", type.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var a = new ListType(PythonTypeSpec.Int);
            var b = new ListType(PythonTypeSpec.Int);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void ValueEquality_DifferentElementTypes_NotEqual()
        {
            var a = new ListType(PythonTypeSpec.Int);
            var b = new ListType(PythonTypeSpec.Str);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var a = new ListType(PythonTypeSpec.Int);
            var b = new ListType(PythonTypeSpec.Int);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ImplementsISequenceType()
        {
            Assert.IsAssignableFrom<ISequenceType>(new ListType(PythonTypeSpec.Int));
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = new ListType(PythonTypeSpec.Int) { Metadata = [1] };
            var b = new ListType(PythonTypeSpec.Int) { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = new ListType(PythonTypeSpec.Int) { Metadata = [1] };
            var b = new ListType(PythonTypeSpec.Int) { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = new ListType(PythonTypeSpec.Int) { Metadata = [1] };
            var b = new ListType(PythonTypeSpec.Int) { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class MappingTypeTests
    {
        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            var keyType = PythonTypeSpec.Str;
            var valueType = PythonTypeSpec.Int;
            var type = new MappingType(keyType, valueType);

            Assert.Equal("Mapping", type.Name);
            Assert.Equal(keyType, type.Key);
            Assert.Equal(valueType, type.Value);
        }

        [Fact]
        public void ToString_ReturnsFullyFormattedName()
        {
            var type = new MappingType(PythonTypeSpec.Str, PythonTypeSpec.Int);

            Assert.Equal("Mapping[str, int]", type.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var keyType = PythonTypeSpec.Str;
            var valueType = PythonTypeSpec.Int;
            var a = new MappingType(keyType, valueType);
            var b = new MappingType(keyType, valueType);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void ValueEquality_DifferentTypes_NotEqual()
        {
            var a = new MappingType(PythonTypeSpec.Str, PythonTypeSpec.Int);
            var b = new MappingType(PythonTypeSpec.Int, PythonTypeSpec.Str);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var keyType = PythonTypeSpec.Str;
            var valueType = PythonTypeSpec.Int;
            var a = new MappingType(keyType, valueType);
            var b = new MappingType(keyType, valueType);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ImplementsIMappingType()
        {
            Assert.IsAssignableFrom<IMappingType>(new MappingType(PythonTypeSpec.Str, PythonTypeSpec.Int));
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = new MappingType(PythonTypeSpec.Str, PythonTypeSpec.Int) { Metadata = [1] };
            var b = new MappingType(PythonTypeSpec.Str, PythonTypeSpec.Int) { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = new MappingType(PythonTypeSpec.Str, PythonTypeSpec.Int) { Metadata = [1] };
            var b = new MappingType(PythonTypeSpec.Str, PythonTypeSpec.Int) { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = new MappingType(PythonTypeSpec.Str, PythonTypeSpec.Int) { Metadata = [1] };
            var b = new MappingType(PythonTypeSpec.Str, PythonTypeSpec.Int) { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class DictTypeTests
    {
        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            var keyType = PythonTypeSpec.Str;
            var valueType = PythonTypeSpec.Int;
            var type = new DictType(keyType, valueType);

            Assert.Equal("dict", type.Name);
            Assert.Equal(keyType, type.Key);
            Assert.Equal(valueType, type.Value);
        }

        [Fact]
        public void ToString_ReturnsFullyFormattedName()
        {
            var type = new DictType(PythonTypeSpec.Str, PythonTypeSpec.Int);

            Assert.Equal("dict[str, int]", type.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var keyType = PythonTypeSpec.Str;
            var valueType = PythonTypeSpec.Int;
            var a = new DictType(keyType, valueType);
            var b = new DictType(keyType, valueType);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void ValueEquality_DifferentTypes_NotEqual()
        {
            var a = new DictType(PythonTypeSpec.Str, PythonTypeSpec.Int);
            var b = new DictType(PythonTypeSpec.Int, PythonTypeSpec.Str);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var keyType = PythonTypeSpec.Str;
            var valueType = PythonTypeSpec.Int;
            var a = new DictType(keyType, valueType);
            var b = new DictType(keyType, valueType);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ImplementsIMappingType()
        {
            var keyType = PythonTypeSpec.Str;
            var valueType = PythonTypeSpec.Int;
            var type = new DictType(keyType, valueType);

            Assert.IsAssignableFrom<IMappingType>(type);
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = new DictType(PythonTypeSpec.Str, PythonTypeSpec.Int) { Metadata = [1] };
            var b = new DictType(PythonTypeSpec.Str, PythonTypeSpec.Int) { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = new DictType(PythonTypeSpec.Str, PythonTypeSpec.Int) { Metadata = [1] };
            var b = new DictType(PythonTypeSpec.Str, PythonTypeSpec.Int) { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = new DictType(PythonTypeSpec.Str, PythonTypeSpec.Int) { Metadata = [1] };
            var b = new DictType(PythonTypeSpec.Str, PythonTypeSpec.Int) { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class CoroutineTypeTests
    {
        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            var yieldType = PythonTypeSpec.Int;
            var sendType = PythonTypeSpec.Str;
            var returnType = PythonTypeSpec.Bool;
            var type = new CoroutineType(yieldType, sendType, returnType);

            Assert.Equal("Coroutine", type.Name);
            Assert.Equal(yieldType, type.Yield);
            Assert.Equal(sendType, type.Send);
            Assert.Equal(returnType, type.Return);
        }

        [Fact]
        public void ToString_ReturnsFullyFormattedName()
        {
            var type = new CoroutineType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool);

            Assert.Equal("Coroutine[int, str, bool]", type.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var yieldType = PythonTypeSpec.Int;
            var sendType = PythonTypeSpec.Str;
            var returnType = PythonTypeSpec.Bool;
            var a = new CoroutineType(yieldType, sendType, returnType);
            var b = new CoroutineType(yieldType, sendType, returnType);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void ValueEquality_DifferentTypes_NotEqual()
        {
            var a = new CoroutineType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool);
            var b = new CoroutineType(PythonTypeSpec.Str, PythonTypeSpec.Int, PythonTypeSpec.Bool);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var yieldType = PythonTypeSpec.Int;
            var sendType = PythonTypeSpec.Str;
            var returnType = PythonTypeSpec.Bool;
            var a = new CoroutineType(yieldType, sendType, returnType);
            var b = new CoroutineType(yieldType, sendType, returnType);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = new CoroutineType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool) { Metadata = [1] };
            var b = new CoroutineType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool) { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = new CoroutineType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool) { Metadata = [1] };
            var b = new CoroutineType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool) { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = new CoroutineType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool) { Metadata = [1] };
            var b = new CoroutineType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool) { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class GeneratorTypeTests
    {
        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            var yieldType = PythonTypeSpec.Int;
            var sendType = PythonTypeSpec.Str;
            var returnType = PythonTypeSpec.Bool;
            var type = new GeneratorType(yieldType, sendType, returnType);

            Assert.Equal("Generator", type.Name);
            Assert.Equal(yieldType, type.Yield);
            Assert.Equal(sendType, type.Send);
            Assert.Equal(returnType, type.Return);
        }

        [Fact]
        public void ToString_ReturnsFullyFormattedName()
        {
            var type = new GeneratorType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool);

            Assert.Equal("Generator[int, str, bool]", type.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var yieldType = PythonTypeSpec.Int;
            var sendType = PythonTypeSpec.Str;
            var returnType = PythonTypeSpec.Bool;
            var a = new GeneratorType(yieldType, sendType, returnType);
            var b = new GeneratorType(yieldType, sendType, returnType);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void ValueEquality_DifferentTypes_NotEqual()
        {
            var a = new GeneratorType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool);
            var b = new GeneratorType(PythonTypeSpec.Str, PythonTypeSpec.Int, PythonTypeSpec.Bool);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var yieldType = PythonTypeSpec.Int;
            var sendType = PythonTypeSpec.Str;
            var returnType = PythonTypeSpec.Bool;
            var a = new GeneratorType(yieldType, sendType, returnType);
            var b = new GeneratorType(yieldType, sendType, returnType);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = new GeneratorType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool) { Metadata = [1] };
            var b = new GeneratorType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool) { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = new GeneratorType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool) { Metadata = [1] };
            var b = new GeneratorType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool) { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = new GeneratorType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool) { Metadata = [1] };
            var b = new GeneratorType(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool) { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class LiteralTypeTests
    {
        private static class Constants
        {
            public static readonly PythonConstant Integer1 = PythonConstant.Integer.Decimal(42);
            public static readonly PythonConstant Integer2 = PythonConstant.Integer.Decimal(43);
            public static readonly PythonConstant String = new PythonConstant.String("hello");
            public static readonly PythonConstant Float = new PythonConstant.Float(3.14);
        }

        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            ValueArray<PythonConstant> constants = [Constants.Integer1, Constants.String];
            var type = new LiteralType(constants);

            Assert.Equal("Literal", type.Name);
            Assert.Equal(constants, type.Constants);
        }

        [Fact]
        public void ToString_FormatsConstants()
        {
            var type = new LiteralType([Constants.Integer1, Constants.String, Constants.Float]);

            Assert.Equal("Literal[42, 'hello', 3.14]", type.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            ValueArray<PythonConstant> constants = [Constants.Integer1];
            var a = new LiteralType(constants);
            var b = new LiteralType(constants);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void ValueEquality_DifferentConstants_NotEqual()
        {
            var a = new LiteralType([Constants.Integer1]);
            var b = new LiteralType([Constants.Integer2]);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            ValueArray<PythonConstant> constants = [Constants.Integer1];
            var a = new LiteralType(constants);
            var b = new LiteralType(constants);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = new LiteralType([Constants.Integer1]) { Metadata = [1] };
            var b = new LiteralType([Constants.Integer1]) { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = new LiteralType([Constants.Integer1]) { Metadata = [1] };
            var b = new LiteralType([Constants.Integer1]) { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = new LiteralType([Constants.Integer1]) { Metadata = [1] };
            var b = new LiteralType([Constants.Integer1]) { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class OptionalTypeTests
    {
        [Fact]
        public void Constructor_SetsTypeArgument()
        {
            var ofType = PythonTypeSpec.Int;
            var type = new OptionalType(ofType);

            Assert.Equal("Optional", type.Name);
            Assert.Equal(ofType, type.Of);
        }

        [Fact]
        public void ToString_ReturnsFullyFormattedName()
        {
            var type = new OptionalType(PythonTypeSpec.Str);

            Assert.Equal("Optional[str]", type.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var ofType = PythonTypeSpec.Int;
            var a = new OptionalType(ofType);
            var b = new OptionalType(ofType);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void ValueEquality_DifferentElementTypes_NotEqual()
        {
            var a = new OptionalType(PythonTypeSpec.Int);
            var b = new OptionalType(PythonTypeSpec.Str);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var ofType = PythonTypeSpec.Int;
            var a = new OptionalType(ofType);
            var b = new OptionalType(ofType);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = new OptionalType(PythonTypeSpec.Int) { Metadata = [1] };
            var b = new OptionalType(PythonTypeSpec.Int) { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = new OptionalType(PythonTypeSpec.Int) { Metadata = [1] };
            var b = new OptionalType(PythonTypeSpec.Int) { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = new OptionalType(PythonTypeSpec.Int) { Metadata = [1] };
            var b = new OptionalType(PythonTypeSpec.Int) { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class CallableTypeTests
    {
        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            ValueArray<PythonTypeSpec> parameters = [PythonTypeSpec.Int, PythonTypeSpec.Str];
            var returnType = PythonTypeSpec.Bool;
            var type = new CallableType(parameters, returnType);

            Assert.Equal("Callback", type.Name);
            Assert.Equal(parameters, type.Parameters);
            Assert.Equal(returnType, type.Return);
        }

        [Fact]
        public void ToString_ReturnsFullyFormattedName()
        {
            var type = new CallableType([PythonTypeSpec.Int, PythonTypeSpec.Str], PythonTypeSpec.Bool);

            Assert.Equal("Callback[[int, str], bool]", type.ToString());
        }

        [Fact]
        public void ToString_WhenNullParameters_FormatsEllipsis()
        {
            var type = new CallableType(null, PythonTypeSpec.Bool);

            Assert.Equal("Callback[..., bool]", type.ToString());
        }

        [Fact]
        public void ToString_NoParameters_ReturnsFullyFormattedName()
        {
            var type = new CallableType([], PythonTypeSpec.Bool);

            Assert.Equal("Callback[[], bool]", type.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            ValueArray<PythonTypeSpec> parameters = [PythonTypeSpec.Int, PythonTypeSpec.Str];
            var returnType = PythonTypeSpec.Bool;
            var a = new CallableType(parameters, returnType);
            var b = new CallableType(parameters, returnType);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void ValueEquality_DifferentTypes_NotEqual()
        {
            var a = new CallableType([PythonTypeSpec.Int], PythonTypeSpec.Bool);
            var b = new CallableType([PythonTypeSpec.Str], PythonTypeSpec.Bool);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            ValueArray<PythonTypeSpec> parameters = [PythonTypeSpec.Int, PythonTypeSpec.Str];
            var returnType = PythonTypeSpec.Bool;
            var a = new CallableType(parameters, returnType);
            var b = new CallableType(parameters, returnType);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = new CallableType([PythonTypeSpec.Int], PythonTypeSpec.Bool) { Metadata = [1] };
            var b = new CallableType([PythonTypeSpec.Int], PythonTypeSpec.Bool) { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = new CallableType([PythonTypeSpec.Int], PythonTypeSpec.Bool) { Metadata = [1] };
            var b = new CallableType([PythonTypeSpec.Int], PythonTypeSpec.Bool) { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = new CallableType([PythonTypeSpec.Int], PythonTypeSpec.Bool) { Metadata = [1] };
            var b = new CallableType([PythonTypeSpec.Int], PythonTypeSpec.Bool) { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class TupleTypeTests
    {
        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            ValueArray<PythonTypeSpec> parameters = [PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool];
            var type = new TupleType(parameters);

            Assert.Equal("tuple", type.Name);
            Assert.Equal(parameters, type.Parameters);
        }

        [Fact]
        public void ToString_ReturnsFullyFormattedName()
        {
            ValueArray<PythonTypeSpec> parameters = [PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool];
            var type = new TupleType(parameters);

            Assert.Equal("tuple[int, str, bool]", type.ToString());
        }

        [Fact]
        public void ToString_EmptyTuple()
        {
            ValueArray<PythonTypeSpec> parameters = [];
            var type = new TupleType(parameters);

            Assert.Equal("tuple[()]", type.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            ValueArray<PythonTypeSpec> parameters = [PythonTypeSpec.Int, PythonTypeSpec.Str];
            var a = new TupleType(parameters);
            var b = new TupleType(parameters);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void ValueEquality_DifferentParameters_NotEqual()
        {
            var a = new TupleType([PythonTypeSpec.Int]);
            var b = new TupleType([PythonTypeSpec.Str]);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            ValueArray<PythonTypeSpec> parameters = [PythonTypeSpec.Int, PythonTypeSpec.Str];
            var a = new TupleType(parameters);
            var b = new TupleType(parameters);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = new TupleType([PythonTypeSpec.Int, PythonTypeSpec.Str]) { Metadata = [1] };
            var b = new TupleType([PythonTypeSpec.Int, PythonTypeSpec.Str]) { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = new TupleType([PythonTypeSpec.Int, PythonTypeSpec.Str]) { Metadata = [1] };
            var b = new TupleType([PythonTypeSpec.Int, PythonTypeSpec.Str]) { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = new TupleType([PythonTypeSpec.Int, PythonTypeSpec.Str]) { Metadata = [1] };
            var b = new TupleType([PythonTypeSpec.Int, PythonTypeSpec.Str]) { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class VariadicTupleTypeTests
    {
        [Fact]
        public void Constructor_SetsTypeArgument()
        {
            var ofType = PythonTypeSpec.Int;
            var type = new VariadicTupleType(ofType);

            Assert.Equal("tuple", type.Name);
            Assert.Equal(ofType, type.Of);
        }

        [Fact]
        public void ToString_ReturnsFullyFormattedName()
        {
            var type = new VariadicTupleType(PythonTypeSpec.Str);

            Assert.Equal("tuple[str, ...]", type.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var a = new VariadicTupleType(PythonTypeSpec.Int);
            var b = new VariadicTupleType(PythonTypeSpec.Int);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void ValueEquality_DifferentElementTypes_NotEqual()
        {
            var a = new VariadicTupleType(PythonTypeSpec.Int);
            var b = new VariadicTupleType(PythonTypeSpec.Str);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var a = new VariadicTupleType(PythonTypeSpec.Int);
            var b = new VariadicTupleType(PythonTypeSpec.Int);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = new VariadicTupleType(PythonTypeSpec.Int) { Metadata = [1] };
            var b = new VariadicTupleType(PythonTypeSpec.Int) { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = new VariadicTupleType(PythonTypeSpec.Int) { Metadata = [1] };
            var b = new VariadicTupleType(PythonTypeSpec.Int) { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = new VariadicTupleType(PythonTypeSpec.Int) { Metadata = [1] };
            var b = new VariadicTupleType(PythonTypeSpec.Int) { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class UnionTypeTests
    {
        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            ValueArray<PythonTypeSpec> choices = [PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool];
            var type = new UnionType(choices);

            Assert.Equal("Union", type.Name);
            Assert.Equal(choices, type.Choices);
        }

        [Fact]
        public void ToString_ReturnsFullyFormattedName()
        {
            var type = new UnionType([PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool]);

            Assert.Equal("Union[int, str, bool]", type.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            ValueArray<PythonTypeSpec> choices = [PythonTypeSpec.Int, PythonTypeSpec.Str];
            var a = new UnionType(choices);
            var b = new UnionType(choices);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void ValueEquality_DifferentChoices_NotEqual()
        {
            var a = new UnionType([PythonTypeSpec.Int, PythonTypeSpec.Str]);
            var b = new UnionType([PythonTypeSpec.Int, PythonTypeSpec.Bool]);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            ValueArray<PythonTypeSpec> choices = [PythonTypeSpec.Int, PythonTypeSpec.Str];
            var a = new UnionType(choices);
            var b = new UnionType(choices);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Normalize_SingleType_ReturnsThatType()
        {
            var result = UnionType.Normalize([PythonTypeSpec.Int]);

            _ = Assert.IsType<IntType>(result);
        }

        [Fact]
        public void Normalize_TypeAndNone_ReturnsOptional()
        {
            var result = UnionType.Normalize([PythonTypeSpec.Int, PythonTypeSpec.None]);

            _ = Assert.IsType<IntType>(Assert.IsType<OptionalType>(result).Of);
        }

        [Fact]
        public void Normalize_NoneAndType_ReturnsOptional()
        {
            var result = UnionType.Normalize([PythonTypeSpec.None, PythonTypeSpec.Str]);

            _ = Assert.IsType<StrType>(Assert.IsType<OptionalType>(result).Of);
        }

        [Fact]
        public void Normalize_TwoDifferentTypes_ReturnsUnion()
        {
            var result = UnionType.Normalize([PythonTypeSpec.Int, PythonTypeSpec.Str]);

            var type = Assert.IsType<UnionType>(result);
            Assert.Equal(2, type.Choices.Length);
            Assert.Contains(PythonTypeSpec.Int, type.Choices);
            Assert.Contains(PythonTypeSpec.Str, type.Choices);
        }

        [Fact]
        public void Normalize_ThreeDifferentTypes_ReturnsUnion()
        {
            var result = UnionType.Normalize([PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool]);

            var type = Assert.IsType<UnionType>(result);
            Assert.Equal(3, type.Choices.Length);
            Assert.Contains(PythonTypeSpec.Int, type.Choices);
            Assert.Contains(PythonTypeSpec.Str, type.Choices);
            Assert.Contains(PythonTypeSpec.Bool, type.Choices);
        }

        [Fact]
        public void Normalize_NestedUnion_Flattens()
        {
            var result = UnionType.Normalize([new UnionType([PythonTypeSpec.Int, PythonTypeSpec.Str]), PythonTypeSpec.Bool]);

            var type = Assert.IsType<UnionType>(result);
            Assert.Equal(3, type.Choices.Length);
            Assert.Contains(PythonTypeSpec.Int, type.Choices);
            Assert.Contains(PythonTypeSpec.Str, type.Choices);
            Assert.Contains(PythonTypeSpec.Bool, type.Choices);
        }

        [Fact]
        public void Normalize_OptionalType_Flattens()
        {
            var result = UnionType.Normalize([new OptionalType(PythonTypeSpec.Int), PythonTypeSpec.Str]);

            var union = Assert.IsType<UnionType>(result);
            Assert.Equal(3, union.Choices.Length);
            Assert.Contains(PythonTypeSpec.Int, union.Choices);
            Assert.Contains(PythonTypeSpec.None, union.Choices);
            Assert.Contains(PythonTypeSpec.Str, union.Choices);
        }

        [Fact]
        public void Normalize_DuplicateTypes_RemovesDuplicates()
        {
            var result = UnionType.Normalize([PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Int]);

            var union = Assert.IsType<UnionType>(result);
            Assert.Equal(2, union.Choices.Length);
            Assert.Contains(PythonTypeSpec.Int, union.Choices);
            Assert.Contains(PythonTypeSpec.Str, union.Choices);
        }

        [Fact]
        public void Normalize_EmptyChoices_ThrowsException()
        {
            static void Act() => UnionType.Normalize([]);
            var ex = Assert.Throws<ArgumentException>(Act);
            Assert.Equal("choices", ex.ParamName);
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = new UnionType([PythonTypeSpec.Int, PythonTypeSpec.Str]) { Metadata = [1] };
            var b = new UnionType([PythonTypeSpec.Int, PythonTypeSpec.Str]) { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = new UnionType([PythonTypeSpec.Int, PythonTypeSpec.Str]) { Metadata = [1] };
            var b = new UnionType([PythonTypeSpec.Int, PythonTypeSpec.Str]) { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = new UnionType([PythonTypeSpec.Int, PythonTypeSpec.Str]) { Metadata = [1] };
            var b = new UnionType([PythonTypeSpec.Int, PythonTypeSpec.Str]) { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }

    public class ParsedPythonTypeSpecTests
    {
        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            const string name = "MyCustomType";
            ValueArray<PythonTypeSpec> arguments = [PythonTypeSpec.Int, PythonTypeSpec.Str];
            var type = new ParsedPythonTypeSpec(name, arguments);

            Assert.Equal(name, type.Name);
            Assert.Equal(arguments, type.Arguments);
        }

        [Fact]
        public void ToString_NoTypeArguments_ReturnsName()
        {
            const string name = "MyCustomType";
            var type = new ParsedPythonTypeSpec(name, []);

            Assert.Equal(name, type.ToString());
        }

        [Fact]
        public void ToString_SingleArgument_ReturnsFullyFormattedName()
        {
            var type = new ParsedPythonTypeSpec("MyCustomType", [PythonTypeSpec.Int]);

            Assert.Equal("MyCustomType[int]", type.ToString());
        }

        [Fact]
        public void ToString_MultipleTypeArguments_ReturnsFullyFormattedName()
        {
            var type = new ParsedPythonTypeSpec("MyCustomType", [PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool]);

            Assert.Equal("MyCustomType[int, str, bool]", type.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            const string name = "MyCustomType";
            ValueArray<PythonTypeSpec> arguments = [PythonTypeSpec.Int, PythonTypeSpec.Str];
            var a = new ParsedPythonTypeSpec(name, arguments);
            var b = new ParsedPythonTypeSpec(name, arguments);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void ValueEquality_DifferentNames_NotEqual()
        {
            ValueArray<PythonTypeSpec> arguments = [PythonTypeSpec.Int];
            var a = new ParsedPythonTypeSpec("Type1", arguments);
            var b = new ParsedPythonTypeSpec("Type2", arguments);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void ValueEquality_DifferentArguments_NotEqual()
        {
            var name = "MyCustomType";
            var a = new ParsedPythonTypeSpec(name, [PythonTypeSpec.Int]);
            var b = new ParsedPythonTypeSpec(name, [PythonTypeSpec.Str]);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var name = "MyCustomType";
            ValueArray<PythonTypeSpec> arguments = [PythonTypeSpec.Int, PythonTypeSpec.Str];
            var a = new ParsedPythonTypeSpec(name, arguments);
            var b = new ParsedPythonTypeSpec(name, arguments);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = new ParsedPythonTypeSpec("MyType", [PythonTypeSpec.Int]) { Metadata = [1] };
            var b = new ParsedPythonTypeSpec("MyType", [PythonTypeSpec.Int]) { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = new ParsedPythonTypeSpec("MyType", [PythonTypeSpec.Int]) { Metadata = [1] };
            var b = new ParsedPythonTypeSpec("MyType", [PythonTypeSpec.Int]) { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = new ParsedPythonTypeSpec("MyType", [PythonTypeSpec.Int]) { Metadata = [1] };
            var b = new ParsedPythonTypeSpec("MyType", [PythonTypeSpec.Int]) { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }
    }
}
