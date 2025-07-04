using CSnakes.Parser;

namespace CSnakes.Tests.Parser;

public class EnumerableExtensionsTests
{
    [Theory]
    [InlineData(new int[] { 1 }, 1)]
    [InlineData(new int[] { 1, 1, 1, 1 }, 1)]
    [InlineData(new int[] { 1, 2 }, 2)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 24)]
    [InlineData(new int[] { 4, 3, 2, 1 }, 24)]
    public void PermutationsBasicTest(int[] ranges, int expectedCount)
    {
        var permList = ranges.Permutations();
        Assert.Equal(expectedCount, permList.Count());
    }

    [Fact]
    public void Permutations231Test()
    {
        var permList = new[] { 2, 3, 1 }.Permutations().ToList();
        Assert.Equal(6, permList.Count);
        Assert.Equal([0, 0, 0], permList[0]);
        Assert.Equal([0, 1, 0], permList[1]);
        Assert.Equal([0, 2, 0], permList[2]);
        Assert.Equal([1, 0, 0], permList[3]);
        Assert.Equal([1, 1, 0], permList[4]);
        Assert.Equal([1, 2, 0], permList[5]);
    }

    [Fact]
    public void PermutationsEmptyTest()
    {
        var permList = Enumerable.Empty<int>().Permutations();
        Assert.Empty(permList);
    }

    [Fact]
    public void PairwiseEmptyTest()
    {
        var pairList = Enumerable.Empty<int>().Pairwise();
        Assert.Empty(pairList);
    }

    [Fact]
    public void ZeroElementRaisesArgumentException()
    {
        Assert.Throws<ArgumentException>(() => { new int[] { 1, 0, 0 }.Permutations().Count(); });
    }

    [Fact]
    public void PairwiseSingleElementTest()
    {
        var pairList = new[] { 1 }.Pairwise();
        Assert.Empty(pairList);
    }
}
