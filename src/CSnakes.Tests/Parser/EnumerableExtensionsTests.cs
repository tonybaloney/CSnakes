using CSnakes.Parser;

namespace CSnakes.Tests.Parser;

public class EnumerableExtensionsTests
{
    [Theory]
    [InlineData(new int[] { 1 }, 1)]
    [InlineData(new int[] { 1, 1, 1, 1 }, 1)]
    [InlineData(new int[] { 0, 0, 0, 0 }, 0)]
    [InlineData(new int[] { 1, 0, 1 }, 1)]
    [InlineData(new int[] { 1, 2 }, 2)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 24)]
    [InlineData(new int[] { 4, 3, 2, 1 }, 24)]
    public void PermutationsBasicTest(int[] ranges, int expectedCount)
    {
        var permList = ranges.Permutations();
        Assert.Equal(expectedCount, permList.Count());
    }
}
