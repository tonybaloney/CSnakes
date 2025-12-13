using CSnakes.Parser.Types;

namespace CSnakes.Tests;

public class PythonTypeSpecTests
{
    public interface ITestData<out T> where T : PythonTypeSpec
    {
        static abstract T CreateInstance();
        static abstract T CreateEquivalentInstance();
        static abstract PythonTypeSpec CreateDifferentInstance();
        static abstract string ExpectedName { get; }
        static abstract string ExpectedToString { get; }
    }

    public abstract class TestBase<T, TTestData>
        where T : PythonTypeSpec
        where TTestData : ITestData<T>
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

    public class NoneTypeTests : TestBase<NoneType, NoneTypeTests>, ITestData<NoneType>
    {
        public static NoneType CreateInstance() => PythonTypeSpec.None;
        public static NoneType CreateEquivalentInstance() => PythonTypeSpec.None;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Int;
        public static string ExpectedName => "NoneType";
        public static string ExpectedToString => "NoneType";
    }

    public class AnyTypeTests : TestBase<AnyType, AnyTypeTests>, ITestData<AnyType>
    {
        public static AnyType CreateInstance() => PythonTypeSpec.Any;
        public static AnyType CreateEquivalentInstance() => PythonTypeSpec.Any;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Int;
        public static string ExpectedName => "Any";
        public static string ExpectedToString => "Any";
    }

    public class IntTypeTests : TestBase<IntType, IntTypeTests>, ITestData<IntType>
    {
        public static IntType CreateInstance() => PythonTypeSpec.Int;
        public static IntType CreateEquivalentInstance() => PythonTypeSpec.Int;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Str;
        public static string ExpectedName => "int";
        public static string ExpectedToString => "int";
    }

    public class StrTypeTests : TestBase<StrType, StrTypeTests>, ITestData<StrType>
    {
        public static StrType CreateInstance() => PythonTypeSpec.Str;
        public static StrType CreateEquivalentInstance() => PythonTypeSpec.Str;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Int;
        public static string ExpectedName => "str";
        public static string ExpectedToString => "str";
    }

    public class FloatTypeTests : TestBase<FloatType, FloatTypeTests>, ITestData<FloatType>
    {
        public static FloatType CreateInstance() => PythonTypeSpec.Float;
        public static FloatType CreateEquivalentInstance() => PythonTypeSpec.Float;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Int;
        public static string ExpectedName => "float";
        public static string ExpectedToString => "float";
    }

    public class BoolTypeTests : TestBase<BoolType, BoolTypeTests>, ITestData<BoolType>
    {
        public static BoolType CreateInstance() => PythonTypeSpec.Bool;
        public static BoolType CreateEquivalentInstance() => PythonTypeSpec.Bool;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Int;
        public static string ExpectedName => "bool";
        public static string ExpectedToString => "bool";
    }

    public class BytesTypeTests : TestBase<BytesType, BytesTypeTests>, ITestData<BytesType>
    {
        public static BytesType CreateInstance() => PythonTypeSpec.Bytes;
        public static BytesType CreateEquivalentInstance() => PythonTypeSpec.Bytes;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Int;
        public static string ExpectedName => "bytes";
        public static string ExpectedToString => "bytes";
    }

    public class BufferTypeTests : TestBase<BufferType, BufferTypeTests>, ITestData<BufferType>
    {
        public static BufferType CreateInstance() => PythonTypeSpec.Buffer;
        public static BufferType CreateEquivalentInstance() => PythonTypeSpec.Buffer;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Int;
        public static string ExpectedName => "Buffer";
        public static string ExpectedToString => "Buffer";
    }

    public class SequenceTypeTests : TestBase<SequenceType, SequenceTypeTests>, ITestData<SequenceType>
    {
        public static SequenceType CreateInstance() => new(PythonTypeSpec.Int);
        public static SequenceType CreateEquivalentInstance() => new(PythonTypeSpec.Int);
        public static PythonTypeSpec CreateDifferentInstance() => new SequenceType(PythonTypeSpec.Str);
        public static string ExpectedName => "Sequence";
        public static string ExpectedToString => "Sequence[int]";

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
        public void ImplementsISequenceType()
        {
            var ofType = PythonTypeSpec.Int;
            var type = new SequenceType(ofType);

            Assert.IsAssignableFrom<ISequenceType>(type);
        }
    }

    public class ListTypeTests : TestBase<ListType, ListTypeTests>, ITestData<ListType>
    {
        public static ListType CreateInstance() => new(PythonTypeSpec.Int);
        public static ListType CreateEquivalentInstance() => new(PythonTypeSpec.Int);
        public static PythonTypeSpec CreateDifferentInstance() => new ListType(PythonTypeSpec.Str);
        public static string ExpectedName => "list";
        public static string ExpectedToString => "list[int]";

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
        public void ImplementsISequenceType()
        {
            Assert.IsAssignableFrom<ISequenceType>(new ListType(PythonTypeSpec.Int));
        }
    }

    public class MappingTypeTests : TestBase<MappingType, MappingTypeTests>, ITestData<MappingType>
    {
        public static MappingType CreateInstance() => new(PythonTypeSpec.Str, PythonTypeSpec.Int);
        public static MappingType CreateEquivalentInstance() => new(PythonTypeSpec.Str, PythonTypeSpec.Int);
        public static PythonTypeSpec CreateDifferentInstance() => new MappingType(PythonTypeSpec.Int, PythonTypeSpec.Str);
        public static string ExpectedName => "Mapping";
        public static string ExpectedToString => "Mapping[str, int]";

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
        public void ImplementsIMappingType()
        {
            Assert.IsAssignableFrom<IMappingType>(new MappingType(PythonTypeSpec.Str, PythonTypeSpec.Int));
        }
    }

    public class DictTypeTests : TestBase<DictType, DictTypeTests>, ITestData<DictType>
    {
        public static DictType CreateInstance() => new(PythonTypeSpec.Str, PythonTypeSpec.Int);
        public static DictType CreateEquivalentInstance() => new(PythonTypeSpec.Str, PythonTypeSpec.Int);
        public static PythonTypeSpec CreateDifferentInstance() => new DictType(PythonTypeSpec.Int, PythonTypeSpec.Str);
        public static string ExpectedName => "dict";
        public static string ExpectedToString => "dict[str, int]";

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
        public void ImplementsIMappingType()
        {
            var type = new DictType(PythonTypeSpec.Str, PythonTypeSpec.Int);

            Assert.IsAssignableFrom<IMappingType>(type);
        }
    }

    public class CoroutineTypeTests : TestBase<CoroutineType, CoroutineTypeTests>, ITestData<CoroutineType>
    {
        public static CoroutineType CreateInstance() => new(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool);
        public static CoroutineType CreateEquivalentInstance() => new(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool);
        public static PythonTypeSpec CreateDifferentInstance() => new CoroutineType(PythonTypeSpec.Str, PythonTypeSpec.Int, PythonTypeSpec.Bool);
        public static string ExpectedName => "Coroutine";
        public static string ExpectedToString => "Coroutine[int, str, bool]";

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
    }

    public class GeneratorTypeTests : TestBase<GeneratorType, GeneratorTypeTests>, ITestData<GeneratorType>
    {
        public static GeneratorType CreateInstance() => new(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool);
        public static GeneratorType CreateEquivalentInstance() => new(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool);
        public static PythonTypeSpec CreateDifferentInstance() => new GeneratorType(PythonTypeSpec.Str, PythonTypeSpec.Int, PythonTypeSpec.Bool);
        public static string ExpectedName => "Generator";
        public static string ExpectedToString => "Generator[int, str, bool]";

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
    }

    public class LiteralTypeTests : TestBase<LiteralType, LiteralTypeTests>, ITestData<LiteralType>
    {
        private static class Constants
        {
            public static readonly PythonConstant Integer1 = PythonConstant.Integer.Decimal(42);
            public static readonly PythonConstant Integer2 = PythonConstant.Integer.Decimal(43);
            public static readonly PythonConstant String = new PythonConstant.String("hello");
            public static readonly PythonConstant Float = new PythonConstant.Float(3.14);
        }

        public static LiteralType CreateInstance() => new([Constants.Integer1]);
        public static LiteralType CreateEquivalentInstance() => new([Constants.Integer1]);
        public static PythonTypeSpec CreateDifferentInstance() => new LiteralType([Constants.Integer2]);
        public static string ExpectedName => "Literal";
        public static string ExpectedToString => "Literal[42]";

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
    }

    public class OptionalTypeTests : TestBase<OptionalType, OptionalTypeTests>, ITestData<OptionalType>
    {
        public static OptionalType CreateInstance() => new(PythonTypeSpec.Int);
        public static OptionalType CreateEquivalentInstance() => new(PythonTypeSpec.Int);
        public static PythonTypeSpec CreateDifferentInstance() => new OptionalType(PythonTypeSpec.Str);
        public static string ExpectedName => "Optional";
        public static string ExpectedToString => "Optional[int]";

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
    }

    public class CallableTypeTests : TestBase<CallableType, CallableTypeTests>, ITestData<CallableType>
    {
        public static CallableType CreateInstance() => new([PythonTypeSpec.Int, PythonTypeSpec.Str], PythonTypeSpec.Bool);
        public static CallableType CreateEquivalentInstance() => new([PythonTypeSpec.Int, PythonTypeSpec.Str], PythonTypeSpec.Bool);
        public static PythonTypeSpec CreateDifferentInstance() => new CallableType([PythonTypeSpec.Str], PythonTypeSpec.Bool);
        public static string ExpectedName => "Callback";
        public static string ExpectedToString => "Callback[[int, str], bool]";

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
    }

    public class TupleTypeTests : TestBase<TupleType, TupleTypeTests>, ITestData<TupleType>
    {
        public static TupleType CreateInstance() => new([PythonTypeSpec.Int, PythonTypeSpec.Str]);
        public static TupleType CreateEquivalentInstance() => new([PythonTypeSpec.Int, PythonTypeSpec.Str]);
        public static PythonTypeSpec CreateDifferentInstance() => new TupleType([PythonTypeSpec.Int]);
        public static string ExpectedName => "tuple";
        public static string ExpectedToString => "tuple[int, str]";

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
    }

    public class VariadicTupleTypeTests : TestBase<VariadicTupleType, VariadicTupleTypeTests>, ITestData<VariadicTupleType>
    {
        public static VariadicTupleType CreateInstance() => new(PythonTypeSpec.Int);
        public static VariadicTupleType CreateEquivalentInstance() => new(PythonTypeSpec.Int);
        public static PythonTypeSpec CreateDifferentInstance() => new VariadicTupleType(PythonTypeSpec.Str);
        public static string ExpectedName => "tuple";
        public static string ExpectedToString => "tuple[int, ...]";

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
    }

    public class UnionTypeTests : TestBase<UnionType, UnionTypeTests>, ITestData<UnionType>
    {
        public static UnionType CreateInstance() => new([PythonTypeSpec.Int, PythonTypeSpec.Str]);
        public static UnionType CreateEquivalentInstance() => new([PythonTypeSpec.Int, PythonTypeSpec.Str]);
        public static PythonTypeSpec CreateDifferentInstance() => new UnionType([PythonTypeSpec.Int, PythonTypeSpec.Bool]);
        public static string ExpectedName => "Union";
        public static string ExpectedToString => "Union[int, str]";

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
    }

    public class ParsedPythonTypeSpecTests : TestBase<ParsedPythonTypeSpec, ParsedPythonTypeSpecTests>, ITestData<ParsedPythonTypeSpec>
    {
        public static ParsedPythonTypeSpec CreateInstance() => new("MyCustomType", [PythonTypeSpec.Int, PythonTypeSpec.Str]);
        public static ParsedPythonTypeSpec CreateEquivalentInstance() => new("MyCustomType", [PythonTypeSpec.Int, PythonTypeSpec.Str]);
        public static PythonTypeSpec CreateDifferentInstance() => new ParsedPythonTypeSpec("MyCustomType", [PythonTypeSpec.Int]);
        public static string ExpectedName => "MyCustomType";
        public static string ExpectedToString => "MyCustomType[int, str]";

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
        public void ValueEquality_DifferentNames_NotEqual()
        {
            ValueArray<PythonTypeSpec> arguments = [PythonTypeSpec.Int];
            var a = new ParsedPythonTypeSpec("Type1", arguments);
            var b = new ParsedPythonTypeSpec("Type2", arguments);

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }
    }
}
