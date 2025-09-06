using System.Collections;
using System.Collections.Immutable;
using CSnakes.Parser.Types;

namespace CSnakes.Tests;

public class ValueArrayTests
{
    [Fact]
    public void DefaultValueArray_ShouldBeEmpty()
    {
        var array = new ValueArray<int>();

        Assert.Equal(0, array.Length);
        Assert.Empty(array.Entries);
    }

    [Fact]
    public void Constructor_WithImmutableArray_ShouldInitializeCorrectly()
    {
        var entries = ImmutableArray.Create(1, 2, 3);
        var array = new ValueArray<int>(entries);

        Assert.Equal(3, array.Length);
        Assert.Equal(entries, array.Entries);
        Assert.Equal(1, array[0]);
        Assert.Equal(2, array[1]);
        Assert.Equal(3, array[2]);
    }

    [Fact]
    public void Create_FromReadOnlySpan_ShouldCreateCorrectly()
    {
        ReadOnlySpan<string> span = ["apple", "banana", "cherry"];
        var array = ValueArray.Create(span);

        Assert.Equal(3, array.Length);
        Assert.Equal("apple", array[0]);
        Assert.Equal("banana", array[1]);
        Assert.Equal("cherry", array[2]);
    }

    [Fact]
    public void Create_FromEmptySpan_ShouldCreateEmptyArray()
    {
        ReadOnlySpan<int> span = [];
        var array = ValueArray.Create(span);

        Assert.Equal(0, array.Length);
        Assert.Empty(array.Entries);
    }

    [Fact]
    public void CollectionBuilder_ShouldWork()
    {
        ValueArray<int> array = [1, 2, 3, 4, 5];

        Assert.Equal(5, array.Length);
        Assert.Equal(1, array[0]);
        Assert.Equal(2, array[1]);
        Assert.Equal(3, array[2]);
        Assert.Equal(4, array[3]);
        Assert.Equal(5, array[4]);
    }

    [Fact]
    public void Indexer_ShouldReturnCorrectElements()
    {
        var array = ValueArray.Params('a', 'b', 'c');

        Assert.Equal('a', array[0]);
        Assert.Equal('b', array[1]);
        Assert.Equal('c', array[2]);
    }

    [Fact]
    public void Count_ShouldMatchLength()
    {
        var array = ValueArray.Params(10, 20, 30);
        IReadOnlyCollection<int> collection = array;

        Assert.Equal(array.Length, collection.Count);
        Assert.Equal(3, collection.Count);
    }

    [Fact]
    public void GetEnumerator_ShouldReturnImmutableArrayEnumerator()
    {
        var array = ValueArray.Params(1, 2, 3);
        var enumerator = array.GetEnumerator();

        Assert.True(enumerator.MoveNext());
        Assert.Equal(1, enumerator.Current);

        Assert.True(enumerator.MoveNext());
        Assert.Equal(2, enumerator.Current);

        Assert.True(enumerator.MoveNext());
        Assert.Equal(3, enumerator.Current);

        Assert.False(enumerator.MoveNext());
    }

    [Fact]
    public void GenericEnumerator_ShouldWork()
    {
        var array = ValueArray.Params("x", "y", "z");
        IEnumerable<string> enumerable = array;

        var result = enumerable.ToList();
        Assert.Equal(["x", "y", "z"], result);
    }

    [Fact]
    public void NonGenericEnumerator_ShouldWork()
    {
        var array = ValueArray.Params(1.1, 2.2, 3.3);
        IEnumerable enumerable = array;
        var enumerator = enumerable.GetEnumerator();

        Assert.True(enumerator.MoveNext());
        Assert.Equal(1.1, enumerator.Current);

        Assert.True(enumerator.MoveNext());
        Assert.Equal(2.2, enumerator.Current);

        Assert.True(enumerator.MoveNext());
        Assert.Equal(3.3, enumerator.Current);

        Assert.False(enumerator.MoveNext());
    }

    [Fact]
    public void Equals_WithSameElements_ShouldReturnTrue()
    {
        var array1 = ValueArray.Params(1, 2, 3);
        var array2 = ValueArray.Params(1, 2, 3);

        Assert.True(array1.Equals(array2));
        Assert.True(array1 == array2);
    }

    [Fact]
    public void Equals_WithDifferentElements_ShouldReturnFalse()
    {
        var array1 = ValueArray.Params(1, 2, 3);
        var array2 = ValueArray.Params(1, 2, 4);

        Assert.False(array1.Equals(array2));
        Assert.False(array1 == array2);
    }

    [Fact]
    public void Equals_WithDifferentLengths_ShouldReturnFalse()
    {
        var array1 = ValueArray.Params(1, 2, 3);
        var array2 = ValueArray.Params(1, 2);

        Assert.False(array1.Equals(array2));
        Assert.False(array1 == array2);
    }

    [Fact]
    public void Equals_WithEmptyArrays_ShouldReturnTrue()
    {
        var array1 = ValueArray.Create<int>([]);
        var array2 = ValueArray.Create<int>([]);

        Assert.True(array1.Equals(array2));
        Assert.True(array1 == array2);
    }

    [Fact]
    public void Equals_WithNullableElements_ShouldWork()
    {
        var array1 = ValueArray.Params("a", null, "c");
        var array2 = ValueArray.Params("a", null, "c");
        var array3 = ValueArray.Params("a", null, "b");

        Assert.True(array1.Equals(array2));
        Assert.False(array1.Equals(array3));
    }

    [Fact]
    public void GetHashCode_WithSameElements_ShouldBeEqual()
    {
        var array1 = ValueArray.Params(1, 2, 3);
        var array2 = ValueArray.Params(1, 2, 3);

        Assert.Equal(array1.GetHashCode(), array2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithNullElements_ShouldWork()
    {
        var array1 = ValueArray.Params("a", null, "c");
        var array2 = ValueArray.Params("a", null, "c");

        Assert.Equal(array1.GetHashCode(), array2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithEmptyArrays_ShouldBeEqual()
    {
        var array1 = ValueArray.Create<int>([]);
        var array2 = ValueArray.Create<int>([]);

        Assert.Equal(array1.GetHashCode(), array2.GetHashCode());
    }

    [Fact]
    public void ImplicitConversion_FromImmutableArray_ShouldWork()
    {
        ImmutableArray<int> immutableArray = [1, 2, 3];
        ValueArray<int> array = immutableArray;

        Assert.Equal(3, array.Length);
        Assert.Equal(immutableArray, array.Entries);
    }

    [Fact]
    public void ImplicitConversion_ToImmutableArray_ShouldWork()
    {
        ValueArray<int> array = [1, 2, 3];
        ImmutableArray<int> immutableArray = array;

        Assert.Equal(3, immutableArray.Length);
        Assert.Equal(array.Entries, immutableArray);
    }

    [Fact]
    public void ImplicitConversion_ToReadOnlySpan_ShouldWork()
    {
        var array = ValueArray.Params("hello", "world");
        ReadOnlySpan<string> span = array;

        Assert.Equal(2, span.Length);
        Assert.Equal("hello", span[0]);
        Assert.Equal("world", span[1]);
    }

    [Fact]
    public void Entries_WithDefaultArray_ShouldReturnEmpty()
    {
        var array = new ValueArray<string>(default);

        Assert.Empty(array.Entries);
        Assert.Equal(0, array.Length);
    }

    [Fact]
    public void ValueSemantics_ShouldWorkInHashSet()
    {
        var array1 = ValueArray.Params(1, 2, 3);
        var array2 = ValueArray.Params(1, 2, 3);
        var array3 = ValueArray.Params(3, 2, 1);

        var hashSet = new HashSet<ValueArray<int>> { array1, array2, array3 };

        // Should only contain 2 unique arrays (array1 and array2 are considered equal)
        Assert.Equal(2, hashSet.Count);
        Assert.Contains(array1, hashSet);
        Assert.Contains(array3, hashSet);
    }

    [Fact]
    public void ValueSemantics_ShouldWorkInDictionary()
    {
        var array1 = ValueArray.Params("a", "b");
        var array2 = ValueArray.Params("a", "b");

        var dictionary = new Dictionary<ValueArray<string>, int>
        {
            [array1] = 1
        };

        // Should be able to access with array2 since it's equal to array1
        Assert.Equal(1, dictionary[array2]);
        Assert.True(dictionary.ContainsKey(array2));
    }

    [Fact]
    public void Record_Equality_ShouldWorkCorrectly()
    {
        var array1 = ValueArray.Params(1, 2, 3);
        var array2 = ValueArray.Params(1, 2, 3);
        var array3 = ValueArray.Params(1, 2, 4);

        // Test record equality
        Assert.True(array1.Equals((object)array2));
        Assert.False(array1.Equals((object)array3));

        // Test with different types
        object notAnArray = "not an array";
        Assert.False(array1.Equals(notAnArray));
        Assert.False(array1.Equals(null));
    }
}
