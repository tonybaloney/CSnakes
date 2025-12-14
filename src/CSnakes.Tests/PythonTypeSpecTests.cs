using CSnakes.Parser.Types;

namespace CSnakes.Tests;

public class PythonTypeSpecTests
{
    public interface ITest<TSelf, out T>
        where TSelf : ITest<TSelf, T>
        where T : PythonTypeSpec
    {
        static abstract T CreateInstance();
        static abstract PythonTypeSpec CreateDifferentInstance();
        static abstract string ExpectedName { get; }
        static abstract string ExpectedToString { get; }
    }

    public abstract class TestBase<T, TTest>
        where T : PythonTypeSpec
        where TTest : ITest<TTest, T>
    {
        [Fact]
        public void Name_IsCorrect()
        {
            Assert.Equal(TTest.ExpectedName, TTest.CreateInstance().Name);
        }

        [Fact]
        public void ToString_ReturnsExpectedValue()
        {
            Assert.Equal(TTest.ExpectedToString, TTest.CreateInstance().ToString());
        }

        [Fact]
        public void ToString_WithMetadata_IncludesAnnotated()
        {
            var type = TTest.CreateInstance() with { Metadata = [42, "foo", 3.14] };
            Assert.Equal($"Annotated[{TTest.ExpectedToString}, 42, 'foo', 3.14]", type.ToString());
        }

        [Fact]
        public void Equality_HasValueSemantics()
        {
            var a = TTest.CreateInstance();
            var b = TTest.CreateInstance();

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            var a = TTest.CreateInstance();
            var b = TTest.CreateInstance();

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_SameMetadata_Equal()
        {
            var a = TTest.CreateInstance() with { Metadata = [1] };
            var b = TTest.CreateInstance() with { Metadata = [1] };

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_NotEqual()
        {
            var a = TTest.CreateInstance() with { Metadata = [1] };
            var b = TTest.CreateInstance() with { Metadata = [2] };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_DifferentMetadata_EqualWithoutMetadata()
        {
            var a = TTest.CreateInstance() with { Metadata = [1] };
            var b = TTest.CreateInstance() with { Metadata = [2] };
            var at = a with { Metadata = default };
            var bt = b with { Metadata = default };

            Assert.Equal(at, bt);
            Assert.True(at == bt);
            Assert.False(at != bt);
        }

        [Fact]
        public void ValueEquality_DifferentInstances_NotEqual()
        {
            var a = TTest.CreateInstance();
            var b = TTest.CreateDifferentInstance();

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

    public class NoneTypeTests : TestBase<NoneType, NoneTypeTests>, ITest<NoneTypeTests, NoneType>
    {
        public static NoneType CreateInstance() => PythonTypeSpec.None;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Int;
        public static string ExpectedName => "NoneType";
        public static string ExpectedToString => "NoneType";
    }

    public class AnyTypeTests : TestBase<AnyType, AnyTypeTests>, ITest<AnyTypeTests, AnyType>
    {
        public static AnyType CreateInstance() => PythonTypeSpec.Any;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Int;
        public static string ExpectedName => "Any";
        public static string ExpectedToString => "Any";
    }

    public class IntTypeTests : TestBase<IntType, IntTypeTests>, ITest<IntTypeTests, IntType>
    {
        public static IntType CreateInstance() => PythonTypeSpec.Int;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Str;
        public static string ExpectedName => "int";
        public static string ExpectedToString => "int";
    }

    public class StrTypeTests : TestBase<StrType, StrTypeTests>, ITest<StrTypeTests, StrType>
    {
        public static StrType CreateInstance() => PythonTypeSpec.Str;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Int;
        public static string ExpectedName => "str";
        public static string ExpectedToString => "str";
    }

    public class FloatTypeTests : TestBase<FloatType, FloatTypeTests>, ITest<FloatTypeTests, FloatType>
    {
        public static FloatType CreateInstance() => PythonTypeSpec.Float;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Int;
        public static string ExpectedName => "float";
        public static string ExpectedToString => "float";
    }

    public class BoolTypeTests : TestBase<BoolType, BoolTypeTests>, ITest<BoolTypeTests, BoolType>
    {
        public static BoolType CreateInstance() => PythonTypeSpec.Bool;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Int;
        public static string ExpectedName => "bool";
        public static string ExpectedToString => "bool";
    }

    public class BytesTypeTests : TestBase<BytesType, BytesTypeTests>, ITest<BytesTypeTests, BytesType>
    {
        public static BytesType CreateInstance() => PythonTypeSpec.Bytes;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Int;
        public static string ExpectedName => "bytes";
        public static string ExpectedToString => "bytes";
    }

    public class BufferTypeTests : TestBase<BufferType, BufferTypeTests>, ITest<BufferTypeTests, BufferType>
    {
        public static BufferType CreateInstance() => PythonTypeSpec.Buffer;
        public static PythonTypeSpec CreateDifferentInstance() => PythonTypeSpec.Int;
        public static string ExpectedName => "Buffer";
        public static string ExpectedToString => "Buffer";
    }

    public class SequenceTypeTests : TestBase<SequenceType, SequenceTypeTests>, ITest<SequenceTypeTests, SequenceType>
    {
        public static SequenceType CreateInstance() => new(PythonTypeSpec.Int);
        public static PythonTypeSpec CreateDifferentInstance() => new SequenceType(PythonTypeSpec.Str);
        public static string ExpectedName => "Sequence";
        public static string ExpectedToString => "Sequence[int]";

        [Fact]
        public void Constructor_SetsTypeArgument()
        {
            var type = CreateInstance();

            Assert.Equal(ExpectedName, type.Name);
            Assert.Equal(PythonTypeSpec.Int, type.Of);
        }

        [Fact]
        public void ImplementsISequenceType()
        {
            var ofType = PythonTypeSpec.Int;
            var type = new SequenceType(ofType);

            Assert.IsAssignableFrom<ISequenceType>(type);
        }
    }

    public class ListTypeTests : TestBase<ListType, ListTypeTests>, ITest<ListTypeTests, ListType>
    {
        public static ListType CreateInstance() => new(PythonTypeSpec.Int);
        public static PythonTypeSpec CreateDifferentInstance() => new ListType(PythonTypeSpec.Str);
        public static string ExpectedName => "list";
        public static string ExpectedToString => "list[int]";

        [Fact]
        public void Constructor_SetsTypeArgument()
        {
            var type = CreateInstance();

            Assert.Equal(ExpectedName, type.Name);
            Assert.Equal(PythonTypeSpec.Int, type.Of);
        }

        [Fact]
        public void ImplementsISequenceType()
        {
            Assert.IsAssignableFrom<ISequenceType>(new ListType(PythonTypeSpec.Int));
        }
    }

    public class MappingTypeTests : TestBase<MappingType, MappingTypeTests>, ITest<MappingTypeTests, MappingType>
    {
        public static MappingType CreateInstance() => new(PythonTypeSpec.Str, PythonTypeSpec.Int);
        public static PythonTypeSpec CreateDifferentInstance() => new MappingType(PythonTypeSpec.Int, PythonTypeSpec.Str);
        public static string ExpectedName => "Mapping";
        public static string ExpectedToString => "Mapping[str, int]";

        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            var type = CreateInstance();

            Assert.Equal(ExpectedName, type.Name);
            Assert.Equal(PythonTypeSpec.Str, type.Key);
            Assert.Equal(PythonTypeSpec.Int, type.Value);
        }

        [Fact]
        public void ImplementsIMappingType()
        {
            Assert.IsAssignableFrom<IMappingType>(new MappingType(PythonTypeSpec.Str, PythonTypeSpec.Int));
        }
    }

    public class DictTypeTests : TestBase<DictType, DictTypeTests>, ITest<DictTypeTests, DictType>
    {
        public static DictType CreateInstance() => new(PythonTypeSpec.Str, PythonTypeSpec.Int);
        public static PythonTypeSpec CreateDifferentInstance() => new DictType(PythonTypeSpec.Int, PythonTypeSpec.Str);
        public static string ExpectedName => "dict";
        public static string ExpectedToString => "dict[str, int]";

        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            var type = CreateInstance();

            Assert.Equal(ExpectedName, type.Name);
            Assert.Equal(PythonTypeSpec.Str, type.Key);
            Assert.Equal(PythonTypeSpec.Int, type.Value);
        }

        [Fact]
        public void ImplementsIMappingType()
        {
            var type = new DictType(PythonTypeSpec.Str, PythonTypeSpec.Int);

            Assert.IsAssignableFrom<IMappingType>(type);
        }
    }

    public class CoroutineTypeTests : TestBase<CoroutineType, CoroutineTypeTests>, ITest<CoroutineTypeTests, CoroutineType>
    {
        public static CoroutineType CreateInstance() => new(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool);
        public static PythonTypeSpec CreateDifferentInstance() => new CoroutineType(PythonTypeSpec.Str, PythonTypeSpec.Int, PythonTypeSpec.Bool);
        public static string ExpectedName => "Coroutine";
        public static string ExpectedToString => "Coroutine[int, str, bool]";

        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            var type = CreateInstance();

            Assert.Equal(ExpectedName, type.Name);
            Assert.Equal(PythonTypeSpec.Int, type.Yield);
            Assert.Equal(PythonTypeSpec.Str, type.Send);
            Assert.Equal(PythonTypeSpec.Bool, type.Return);
        }
    }

    public class GeneratorTypeTests : TestBase<GeneratorType, GeneratorTypeTests>, ITest<GeneratorTypeTests, GeneratorType>
    {
        public static GeneratorType CreateInstance() => new(PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool);
        public static PythonTypeSpec CreateDifferentInstance() => new GeneratorType(PythonTypeSpec.Str, PythonTypeSpec.Int, PythonTypeSpec.Bool);
        public static string ExpectedName => "Generator";
        public static string ExpectedToString => "Generator[int, str, bool]";

        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            var type = CreateInstance();

            Assert.Equal(ExpectedName, type.Name);
            Assert.Equal(PythonTypeSpec.Int, type.Yield);
            Assert.Equal(PythonTypeSpec.Str, type.Send);
            Assert.Equal(PythonTypeSpec.Bool, type.Return);
        }
    }

    public class LiteralTypeTests : TestBase<LiteralType, LiteralTypeTests>, ITest<LiteralTypeTests, LiteralType>
    {
        private static class Constants
        {
            public static readonly PythonConstant Integer1 = PythonConstant.Integer.Decimal(42);
            public static readonly PythonConstant Integer2 = PythonConstant.Integer.Decimal(43);
            public static readonly PythonConstant String = new PythonConstant.String("hello");
            public static readonly PythonConstant Float = new PythonConstant.Float(3.14);
        }

        public static LiteralType CreateInstance() => new([Constants.Integer1, Constants.String, Constants.Float]);
        public static PythonTypeSpec CreateDifferentInstance() => new LiteralType([Constants.Integer2, Constants.String, Constants.Float]);
        public static string ExpectedName => "Literal";
        public static string ExpectedToString => "Literal[42, 'hello', 3.14]";

        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            var type = CreateInstance();

            Assert.Equal(ExpectedName, type.Name);
            Assert.Equal([Constants.Integer1, Constants.String, Constants.Float], type.Constants);
        }
    }

    public class OptionalTypeTests : TestBase<OptionalType, OptionalTypeTests>, ITest<OptionalTypeTests, OptionalType>
    {
        public static OptionalType CreateInstance() => new(PythonTypeSpec.Int);
        public static PythonTypeSpec CreateDifferentInstance() => new OptionalType(PythonTypeSpec.Str);
        public static string ExpectedName => "Optional";
        public static string ExpectedToString => "Optional[int]";

        [Fact]
        public void Constructor_SetsTypeArgument()
        {
            var type = CreateInstance();

            Assert.Equal(ExpectedName, type.Name);
            Assert.Equal(PythonTypeSpec.Int, type.Of);
        }
    }

    public class CallableTypeTests : TestBase<CallableType, CallableTypeTests>, ITest<CallableTypeTests, CallableType>
    {
        public static CallableType CreateInstance() => new([PythonTypeSpec.Int, PythonTypeSpec.Str], PythonTypeSpec.Bool);
        public static PythonTypeSpec CreateDifferentInstance() => new CallableType([PythonTypeSpec.Str], PythonTypeSpec.Bool);
        public static string ExpectedName => "Callback";
        public static string ExpectedToString => "Callback[[int, str], bool]";

        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            var type = CreateInstance();

            Assert.Equal(ExpectedName, type.Name);
            Assert.Equal([PythonTypeSpec.Int, PythonTypeSpec.Str], type.Parameters);
            Assert.Equal(PythonTypeSpec.Bool, type.Return);
        }

        [Fact]
        public void ToString_WhenNullParameters_FormatsEllipsis()
        {
            var type = new CallableType(null, PythonTypeSpec.Bool);
            var annotatedType = type with { Metadata = [42] };

            Assert.Equal("Callback[..., bool]", type.ToString());
            Assert.Equal("Annotated[Callback[..., bool], 42]", annotatedType.ToString());
        }

        [Fact]
        public void ToString_NoParameters_ReturnsFullyFormattedName()
        {
            var type = new CallableType([], PythonTypeSpec.Bool);
            var annotatedType = type with { Metadata = [42] };

            Assert.Equal("Callback[[], bool]", type.ToString());
            Assert.Equal("Annotated[Callback[[], bool], 42]", annotatedType.ToString());
        }
    }

    public class TupleTypeTests : TestBase<TupleType, TupleTypeTests>, ITest<TupleTypeTests, TupleType>
    {
        public static TupleType CreateInstance() => new([PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool]);
        public static PythonTypeSpec CreateDifferentInstance() => new TupleType([PythonTypeSpec.Int, PythonTypeSpec.Str]);
        public static string ExpectedName => "tuple";
        public static string ExpectedToString => "tuple[int, str, bool]";

        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            var type = CreateInstance();

            Assert.Equal(ExpectedName, type.Name);
            Assert.Equal([PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool], type.Parameters);
        }

        [Fact]
        public void ToString_EmptyTuple()
        {
            var type = new TupleType([]);
            var annotatedType = type with { Metadata = [42] };

            Assert.Equal("tuple[()]", type.ToString());
            Assert.Equal("Annotated[tuple[()], 42]", annotatedType.ToString());
        }
    }

    public class VariadicTupleTypeTests : TestBase<VariadicTupleType, VariadicTupleTypeTests>, ITest<VariadicTupleTypeTests, VariadicTupleType>
    {
        public static VariadicTupleType CreateInstance() => new(PythonTypeSpec.Int);
        public static PythonTypeSpec CreateDifferentInstance() => new VariadicTupleType(PythonTypeSpec.Str);
        public static string ExpectedName => "tuple";
        public static string ExpectedToString => "tuple[int, ...]";

        [Fact]
        public void Constructor_SetsTypeArgument()
        {
            var type = CreateInstance();

            Assert.Equal(ExpectedName, type.Name);
            Assert.Equal(PythonTypeSpec.Int, type.Of);
        }
    }

    public class UnionTypeTests : TestBase<UnionType, UnionTypeTests>, ITest<UnionTypeTests, UnionType>
    {
        public static UnionType CreateInstance() => new([PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool]);
        public static PythonTypeSpec CreateDifferentInstance() => new UnionType([PythonTypeSpec.Int, PythonTypeSpec.Bool]);
        public static string ExpectedName => "Union";
        public static string ExpectedToString => "Union[int, str, bool]";

        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            var type = CreateInstance();

            Assert.Equal([PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool], type.Choices);
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

    public class ParsedPythonTypeSpecTests : TestBase<ParsedPythonTypeSpec, ParsedPythonTypeSpecTests>, ITest<ParsedPythonTypeSpecTests, ParsedPythonTypeSpec>
    {
        public static ParsedPythonTypeSpec CreateInstance() => new("MyCustomType", [PythonTypeSpec.Int, PythonTypeSpec.Str]);
        public static PythonTypeSpec CreateDifferentInstance() => new ParsedPythonTypeSpec("MyCustomType", [PythonTypeSpec.Int]);
        public static string ExpectedName => "MyCustomType";
        public static string ExpectedToString => "MyCustomType[int, str]";

        [Fact]
        public void Constructor_SetsTypeArguments()
        {
            var type = CreateInstance();

            Assert.Equal(ExpectedName, type.Name);
            Assert.Equal([PythonTypeSpec.Int, PythonTypeSpec.Str], type.Arguments);
        }

        [Fact]
        public void ToString_NoTypeArguments_ReturnsName()
        {
            var type = CreateInstance() with { Arguments = [] };
            var annotatedType = type with { Metadata = [42] };

            Assert.Equal(ExpectedName, type.ToString());
            Assert.Equal($"Annotated[{ExpectedName}, 42]", annotatedType.ToString());
        }

        [Fact]
        public void ToString_SingleArgument_ReturnsFullyFormattedName()
        {
            var type = CreateInstance() with { Arguments = [PythonTypeSpec.Int] };
            var annotatedType = type with { Metadata = [42] };

            Assert.Equal($"{ExpectedName}[int]", type.ToString());
            Assert.Equal($"Annotated[{ExpectedName}[int], 42]", annotatedType.ToString());
        }

        [Fact]
        public void ToString_MultipleTypeArguments_ReturnsFullyFormattedName()
        {
            var type = CreateInstance() with { Arguments = [PythonTypeSpec.Int, PythonTypeSpec.Str, PythonTypeSpec.Bool] };
            var annotatedType = type with { Metadata = [42] };

            Assert.Equal($"{ExpectedName}[int, str, bool]", type.ToString());
            Assert.Equal($"Annotated[{ExpectedName}[int, str, bool], 42]", annotatedType.ToString());
        }

        [Fact]
        public void ValueEquality_DifferentNames_NotEqual()
        {
            var a = CreateInstance();
            var b = a with { Name = $"{a.Name}{a.Name}" };

            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.True(a != b);
        }
    }
}
