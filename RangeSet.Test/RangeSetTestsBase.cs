namespace RangeSet.Tests;

using RangeSet;

public abstract class RangeSetTestsBase
{
    protected abstract Range<int>[] CreateSet(params Range<int>[] ranges);
    protected abstract Range<int>[] Union(Range<int>[] a, Range<int>[] b);
    protected abstract Range<int>[] Except(Range<int>[] a, Range<int>[] b);
    protected abstract Range<int>[] Intersect(Range<int>[] a, Range<int>[] b);
    protected abstract string SetToString(params Range<int>[] ranges);

    protected static Range<int> R(int first, int last) => new(first, last);

    #region Constructor Tests

    [Fact]
    public void Constructor_FromEmptyInput_CreatesEmptySet()
    {
        Assert.Empty(CreateSet());
    }

    [Theory]
    [InlineData(new int[] { 20, 30, 1, 10, 5, 15 }, new int[] { 1, 15, 20, 30 })]
#pragma warning disable CA1062 // params from [InlineData] are never null
    public void Constructor_FromUnsortedInput_NormalizesAndMerges(int[] input, int[] expected)
    {
        var result = CreateSet(TestHelpers.CreateRangesFromPairs(input));
        Assert.Equal(TestHelpers.CreateRangesFromPairs(expected), result);
    }
#pragma warning restore CA1062

    [Fact]
    public void Constructor_FromAdjacentRanges_MergesIntoSingleRange()
    {
        var result = CreateSet(R(1, 10), R(11, 20));
        Assert.Equal([R(1, 20)], result);
    }

    [Fact]
    public void Constructor_FromDuplicateRanges_MergesIntoSingle()
    {
        var result = CreateSet(R(1, 10), R(1, 10), R(1, 10));
        Assert.Equal([R(1, 10)], result);
    }

    [Fact]
    public void Constructor_FromContainedRanges_MergesIntoOuterRange()
    {
        var result = CreateSet(R(1, 100), R(20, 30), R(50, 60));
        Assert.Equal([R(1, 100)], result);
    }

    [Fact]
    public void Constructor_RangeAtMinValue_Success()
    {
        var result = CreateSet(R(int.MinValue, -100));
        Assert.Single(result);
        Assert.Equal(int.MinValue, result[0].First);
    }

    [Fact]
    public void Constructor_RangeAtMaxValue_Success()
    {
        var result = CreateSet(R(100, int.MaxValue));
        Assert.Single(result);
        Assert.Equal(int.MaxValue, result[0].Last);
    }

    [Fact]
    public void Constructor_FullRange_Success()
    {
        var result = CreateSet(R(int.MinValue, int.MaxValue));
        Assert.Single(result);
    }

    #endregion

    #region Union Tests

    [Fact]
    public void Union_BothEmpty_ReturnsEmpty()
    {
        Assert.Empty(Union(CreateSet(), CreateSet()));
    }

    [Fact]
    public void Union_WithEmpty_ReturnsOriginal()
    {
        var set = CreateSet(R(1, 10));
        Assert.Equal(set, Union(set, CreateSet()));
    }

    [Fact]
    public void Union_EmptyWithNonEmpty_ReturnsOther()
    {
        var set = CreateSet(R(1, 10));
        Assert.Equal(set, Union(CreateSet(), set));
    }

    [Fact]
    public void Union_NonOverlapping_ReturnsBothRangesSorted()
    {
        var result = Union(CreateSet(R(20, 30)), CreateSet(R(1, 10)));
        Assert.Equal(new[] { R(1, 10), R(20, 30) }, result);
    }

    [Fact]
    public void Union_Overlapping_MergesIntoSingle()
    {
        var result = Union(CreateSet(R(1, 10)), CreateSet(R(5, 15)));
        Assert.Equal([R(1, 15)], result);
    }

    [Fact]
    public void Union_Adjacent_MergesIntoSingle()
    {
        var result = Union(CreateSet(R(1, 10)), CreateSet(R(11, 20)));
        Assert.Equal([R(1, 20)], result);
    }

    [Fact]
    public void Union_Commutative()
    {
        var a = CreateSet(R(1, 10), R(30, 40));
        var b = CreateSet(R(5, 35));
        Assert.Equal(Union(a, b), Union(b, a));
    }

    [Fact]
    public void Union_Self_ReturnsSameRanges()
    {
        var set = CreateSet(R(1, 10), R(20, 30));
        Assert.Equal(set, Union(set, set));
    }

    #endregion

    #region Except Tests

    [Fact]
    public void Except_BothEmpty_ReturnsEmpty()
    {
        Assert.Empty(Except(CreateSet(), CreateSet()));
    }

    [Fact]
    public void Except_FromEmpty_ReturnsEmpty()
    {
        Assert.Empty(Except(CreateSet(), CreateSet(R(1, 10))));
    }

    [Fact]
    public void Except_WithEmptySubtrahend_ReturnsOriginal()
    {
        var set = CreateSet(R(1, 10));
        Assert.Equal(set, Except(set, CreateSet()));
    }

    [Fact]
    public void Except_NoOverlap_ReturnsOriginal()
    {
        var set = CreateSet(R(1, 10));
        Assert.Equal(set, Except(set, CreateSet(R(20, 30))));
    }

    [Fact]
    public void Except_CompleteOverlap_ReturnsEmpty()
    {
        Assert.Empty(Except(CreateSet(R(1, 10)), CreateSet(R(1, 10))));
    }

    [Fact]
    public void Except_Superset_ReturnsEmpty()
    {
        Assert.Empty(Except(CreateSet(R(5, 8)), CreateSet(R(1, 10))));
    }

    [Theory]
    [InlineData(1, 10, 1, 5, 6, 10)]
    [InlineData(1, 10, 5, 10, 1, 4)]
    [InlineData(1, 10, 4, 6, 1, 3, 7, 10)]
#pragma warning disable CA1062 // params from [InlineData] are never null
    public void Except_PartialOverlap_ReturnsCorrectResult(int r1Start, int r1End, int r2Start, int r2End, params int[] expected)
    {
        var result = Except(CreateSet(R(r1Start, r1End)), CreateSet(R(r2Start, r2End)));
        Assert.Equal(TestHelpers.CreateRangesFromPairs(expected), result);
    }
#pragma warning restore CA1062

    [Fact]
    public void Except_MultipleExclusionsFromSingleRange_SplitsCorrectly()
    {
        var result = Except(CreateSet(R(1, 20)), CreateSet(R(5, 8), R(12, 15)));
        Assert.Equal(new[] { R(1, 4), R(9, 11), R(16, 20) }, result);
    }

    [Fact]
    public void Except_AtMinValue_HandlesCorrectly()
    {
        var result = Except(CreateSet(R(int.MinValue, 100)), CreateSet(R(int.MinValue, 0)));
        Assert.Equal([R(1, 100)], result);
    }

    [Fact]
    public void Except_AtMaxValue_HandlesCorrectly()
    {
        var result = Except(CreateSet(R(0, int.MaxValue)), CreateSet(R(int.MaxValue, int.MaxValue)));
        Assert.Equal([R(0, int.MaxValue - 1)], result);
    }

    #endregion

    #region Intersect Tests

    [Fact]
    public void Intersect_BothEmpty_ReturnsEmpty()
    {
        Assert.Empty(Intersect(CreateSet(), CreateSet()));
    }

    [Fact]
    public void Intersect_WithEmpty_ReturnsEmpty()
    {
        Assert.Empty(Intersect(CreateSet(R(1, 10)), CreateSet()));
    }

    [Fact]
    public void Intersect_EmptyWithOther_ReturnsEmpty()
    {
        Assert.Empty(Intersect(CreateSet(), CreateSet(R(1, 10))));
    }

    [Fact]
    public void Intersect_NoOverlap_ReturnsEmpty()
    {
        Assert.Empty(Intersect(CreateSet(R(1, 10)), CreateSet(R(20, 30))));
    }

    [Fact]
    public void Intersect_CompleteOverlap_ReturnsSame()
    {
        var result = Intersect(CreateSet(R(1, 10)), CreateSet(R(1, 10)));
        Assert.Equal([R(1, 10)], result);
    }

    [Fact]
    public void Intersect_PartialOverlap_ReturnsIntersection()
    {
        var result = Intersect(CreateSet(R(1, 10)), CreateSet(R(5, 15)));
        Assert.Equal([R(5, 10)], result);
    }

    [Fact]
    public void Intersect_Contained_ReturnsSmaller()
    {
        var result = Intersect(CreateSet(R(1, 10)), CreateSet(R(3, 7)));
        Assert.Equal([R(3, 7)], result);
    }

    [Fact]
    public void Intersect_Commutative()
    {
        var a = CreateSet(R(1, 10), R(20, 30));
        var b = CreateSet(R(5, 25));
        Assert.Equal(Intersect(a, b), Intersect(b, a));
    }

    [Fact]
    public void Intersect_MultipleRanges_ReturnsAllIntersections()
    {
        var a = CreateSet(R(1, 5), R(10, 15), R(20, 25));
        var b = CreateSet(R(3, 12), R(18, 22));
        var result = Intersect(a, b);
        Assert.Equal(new[] { R(3, 5), R(10, 12), R(20, 22) }, result);
    }

    [Fact]
    public void Intersect_Self_ReturnsSameRanges()
    {
        var set = CreateSet(R(1, 10), R(20, 30));
        Assert.Equal(set, Intersect(set, set));
    }

    #endregion

    #region Set Theory Properties

    [Fact]
    public void Union_Identity_WithEmpty()
    {
        var set = CreateSet(R(1, 10));
        var empty = CreateSet();
        Assert.Equal(set, Union(set, empty));
        Assert.Equal(set, Union(empty, set));
    }

    [Fact]
    public void Intersect_Identity_WithEmpty()
    {
        Assert.Empty(Intersect(CreateSet(R(1, 10)), CreateSet()));
    }

    [Fact]
    public void Except_Identity_WithEmpty()
    {
        var set = CreateSet(R(1, 10));
        Assert.Equal(set, Except(set, CreateSet()));
    }

    [Fact]
    public void Except_Self_ReturnsEmpty()
    {
        var set = CreateSet(R(1, 10), R(20, 30));
        Assert.Empty(Except(set, set));
    }

    [Fact]
    public void Distributivity_UnionOverIntersect()
    {
        var a = CreateSet(R(1, 10));
        var b = CreateSet(R(5, 15));
        var c = CreateSet(R(8, 20));

        Assert.Equal(
            Intersect(Union(a, b), c),
            Union(Intersect(a, c), Intersect(b, c)));
    }

    [Fact]
    public void Distributivity_IntersectOverUnion()
    {
        var a = CreateSet(R(1, 10));
        var b = CreateSet(R(5, 15));
        var c = CreateSet(R(8, 20));

        Assert.Equal(
            Intersect(a, Union(b, c)),
            Union(Intersect(a, b), Intersect(a, c)));
    }

    [Fact]
    public void Union_Associative()
    {
        var a = CreateSet(R(1, 10));
        var b = CreateSet(R(5, 20));
        var c = CreateSet(R(15, 30));

        Assert.Equal(
            Union(Union(a, b), c),
            Union(a, Union(b, c)));
    }

    [Fact]
    public void Intersect_Associative()
    {
        var a = CreateSet(R(1, 20));
        var b = CreateSet(R(5, 25));
        var c = CreateSet(R(10, 30));

        Assert.Equal(
            Intersect(Intersect(a, b), c),
            Intersect(a, Intersect(b, c)));
    }

    #endregion

    #region Chained Operations

    [Fact]
    public void Chained_UnionThenExcept_ReturnsCorrectResult()
    {
        var result = Except(
            Union(CreateSet(R(1, 10)), CreateSet(R(5, 15))),
            CreateSet(R(8, 12)));
        Assert.Equal(new[] { R(1, 7), R(13, 15) }, result);
    }

    [Fact]
    public void Chained_IntersectThenUnion_ReturnsCorrectResult()
    {
        var result = Union(
            Intersect(CreateSet(R(1, 20)), CreateSet(R(5, 15))),
            CreateSet(R(25, 35)));
        Assert.Equal(new[] { R(5, 15), R(25, 35) }, result);
    }

    [Fact]
    public void Chained_ExceptThenIntersect_ReturnsCorrectResult()
    {
        var result = Intersect(
            Except(CreateSet(R(1, 20)), CreateSet(R(5, 10))),
            CreateSet(R(1, 15)));
        Assert.Equal(new[] { R(1, 4), R(11, 15) }, result);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_EmptySet_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, SetToString());
    }

    [Fact]
    public void ToString_NonEmptySet_ContainsRangeInfo()
    {
        var result = SetToString(R(1, 10), R(20, 30));
        Assert.Contains("1 - 10", result);
        Assert.Contains("20 - 30", result);
    }

    #endregion
}
