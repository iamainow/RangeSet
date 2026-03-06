namespace RangeSet.Tests;

using RangeSet;

public class ArrayRangeSetTests
{
    #region Constructors

    [Fact]
    public void DefaultConstructor_CreatesEmptySet()
    {
        var set = new ArrayRangeSet<int>();
        
        Assert.Equal(0, set.RangesCount);
        Assert.True(set.ToReadOnlySpan().IsEmpty);
        Assert.Empty(set.ToArray());
    }

    [Fact]
    public void CopyConstructor_FromEmptySet_CreatesEmptySet()
    {
        var original = new ArrayRangeSet<int>();
        var copy = new ArrayRangeSet<int>(original);
        
        Assert.Equal(0, copy.RangesCount);
    }

    [Fact]
    public void CopyConstructor_FromNonEmptySet_CreatesDeepCopy()
    {
        var original = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10), new Range<int>(20, 30) });
        var copy = new ArrayRangeSet<int>(original);
        
        Assert.Equal(original.RangesCount, copy.RangesCount);
        Assert.Equal(original.ToArray(), copy.ToArray());
        Assert.NotSame(original.ToArray(), copy.ToArray());
    }

    [Fact]
    public void Constructor_FromEmptyArray_CreatesEmptySet()
    {
        var set = new ArrayRangeSet<int>(Array.Empty<Range<int>>());
        
        Assert.Equal(0, set.RangesCount);
    }

    [Fact]
    public void Constructor_FromNullArray_ThrowsArgumentNullException()
    {
        Range<int>[]? nullArray = null;
        Assert.Throws<ArgumentNullException>(() => new ArrayRangeSet<int>(nullArray!));
    }

    [Theory]
    [InlineData(new int[] { 20, 30, 1, 10, 5, 15 }, new int[] { 1, 15, 20, 30 })]
    public void Constructor_FromUnsortedArray_NormalizesAndMerges(int[] input, int[] expected)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(expected);
        var set = new ArrayRangeSet<int>(CreateRangesFromPairs(input));

        Assert.Equal(CreateRangesFromPairs(expected), set.ToArray());
    }

    [Fact]
    public void Constructor_FromAdjacentRanges_MergesIntoSingleRange()
    {
        var ranges = new[] { new Range<int>(1, 10), new Range<int>(11, 20) };
        
        var set = new ArrayRangeSet<int>(ranges);
        
        Assert.Equal(1, set.RangesCount);
        Assert.Equal(new(1, 20), set.ToArray()[0]);
    }

    [Fact]
    public void Constructor_FromDuplicateRanges_MergesIntoSingle()
    {
        var ranges = new[] { new Range<int>(1, 10), new Range<int>(1, 10), new Range<int>(1, 10) };
        
        var set = new ArrayRangeSet<int>(ranges);
        
        Assert.Equal(1, set.RangesCount);
        Assert.Equal(new(1, 10), set.ToArray()[0]);
    }

    [Fact]
    public void Constructor_FromContainedRanges_MergesIntoOuterRange()
    {
        var ranges = new[] { new Range<int>(1, 100), new Range<int>(20, 30), new Range<int>(50, 60) };
        
        var set = new ArrayRangeSet<int>(ranges);
        
        Assert.Equal(1, set.RangesCount);
        Assert.Equal(new(1, 100), set.ToArray()[0]);
    }

    [Fact]
    public void Constructor_RangeAtMinValue_Success()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(int.MinValue, -100) });
        
        Assert.Equal(1, set.RangesCount);
        Assert.Equal(int.MinValue, set.ToArray()[0].First);
    }

    [Fact]
    public void Constructor_RangeAtMaxValue_Success()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(100, int.MaxValue) });
        
        Assert.Equal(1, set.RangesCount);
        Assert.Equal(int.MaxValue, set.ToArray()[0].Last);
    }

    [Fact]
    public void Constructor_FullRange_Success()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(int.MinValue, int.MaxValue) });
        
        Assert.Equal(1, set.RangesCount);
    }

    #endregion

    #region ToReadOnlySpan and ToArray

    [Fact]
    public void ToReadOnlySpan_ReturnsCorrectView()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10), new Range<int>(20, 30) });
        
        var span = set.ToReadOnlySpan();
        
        Assert.Equal(2, span.Length);
        Assert.Equal(new(1, 10), span[0]);
        Assert.Equal(new(20, 30), span[1]);
    }

    [Fact]
    public void ToArray_ReturnsIndependentCopy()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        
        var array1 = set.ToArray();
        var array2 = set.ToArray();
        
        Assert.NotSame(array1, array2);
        Assert.Equal(array1, array2);
    }

    #endregion

    #region Union Tests

    [Fact]
    public void Union_BothEmpty_ReturnsEmpty()
    {
        var result = new ArrayRangeSet<int>().Union(new ArrayRangeSet<int>());
        
        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Union_WithEmpty_ReturnsOriginal()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        
        var result = set.Union(new ArrayRangeSet<int>());
        
        Assert.Equal(set.ToArray(), result.ToArray());
    }

    [Fact]
    public void Union_EmptyWithNonEmpty_ReturnsOther()
    {
        var other = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        
        var result = new ArrayRangeSet<int>().Union(other);
        
        Assert.Equal(other.ToArray(), result.ToArray());
    }

    [Fact]
    public void Union_NonOverlapping_ReturnsBothRangesSorted()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(20, 30) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        
        var result = set1.Union(set2);
        
        Assert.Equal(2, result.RangesCount);
        Assert.Equal(new(1, 10), result.ToArray()[0]);
        Assert.Equal(new(20, 30), result.ToArray()[1]);
    }

    [Fact]
    public void Union_Overlapping_MergesIntoSingle()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(5, 15) });
        
        var result = set1.Union(set2);
        
        Assert.Equal(1, result.RangesCount);
        Assert.Equal(new(1, 15), result.ToArray()[0]);
    }

    [Fact]
    public void Union_Adjacent_MergesIntoSingle()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(11, 20) });
        
        var result = set1.Union(set2);
        
        Assert.Equal(1, result.RangesCount);
        Assert.Equal(new(1, 20), result.ToArray()[0]);
    }

    [Fact]
    public void Union_Commutative()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10), new Range<int>(30, 40) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(5, 35) });
        
        var result1 = set1.Union(set2);
        var result2 = set2.Union(set1);
        
        Assert.Equal(result1.ToArray(), result2.ToArray());
    }

    [Fact]
    public void Union_Self_ReturnsSameRanges()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10), new Range<int>(20, 30) });
        
        var result = set.Union(set);
        
        Assert.Equal(set.ToArray(), result.ToArray());
    }

    [Fact]
    public void Union_DoesNotModifyOriginalSets()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(20, 30) });
        var original1 = set1.ToArray();
        var original2 = set2.ToArray();
        
        set1.Union(set2);
        
        Assert.Equal(original1, set1.ToArray());
        Assert.Equal(original2, set2.ToArray());
    }

    [Fact]
    public void Union_NullArgument_ThrowsArgumentNullException()
    {
        var set = new ArrayRangeSet<int>();
        ArrayRangeSet<int>? nullSet = null;
        
        Assert.Throws<ArgumentNullException>(() => set.Union(nullSet!));
    }

    [Fact]
    public void Union_WithSpan_ReturnsCorrectResult()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 5) });
        var span = new[] { new Range<int>(3, 10) };
        
        var result = set.Union(span.AsSpan());
        
        Assert.Equal(new(1, 10), result.ToArray()[0]);
    }

    [Fact]
    public void Union_WithUnsortedSpan_NormalizesCorrectly()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 5) });
        var unsorted = new[] { new Range<int>(20, 30), new Range<int>(3, 10) };
        
        var result = set.Union(unsorted.AsSpan());
        
        Assert.Equal(2, result.RangesCount);
        Assert.Equal(new(1, 10), result.ToArray()[0]);
        Assert.Equal(new(20, 30), result.ToArray()[1]);
    }

    #endregion

    #region Except Tests

    [Fact]
    public void Except_BothEmpty_ReturnsEmpty()
    {
        var result = new ArrayRangeSet<int>().Except(new ArrayRangeSet<int>());
        
        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Except_FromEmpty_ReturnsEmpty()
    {
        var other = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        
        var result = new ArrayRangeSet<int>().Except(other);
        
        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Except_EmptyFromOther_ReturnsOriginal()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        
        var result = set.Except(new ArrayRangeSet<int>());
        
        Assert.Equal(set.ToArray(), result.ToArray());
    }

    [Fact]
    public void Except_NoOverlap_ReturnsOriginal()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(20, 30) });
        
        var result = set1.Except(set2);
        
        Assert.Equal(set1.ToArray(), result.ToArray());
    }

    [Fact]
    public void Except_CompleteOverlap_ReturnsEmpty()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        
        var result = set1.Except(set2);
        
        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Except_Superset_ReturnsEmpty()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(5, 8) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        
        var result = set1.Except(set2);
        
        Assert.Equal(0, result.RangesCount);
    }

    [Theory]
    [InlineData(1, 10, 1, 5, 6, 10)]
    [InlineData(1, 10, 5, 10, 1, 4)]
    [InlineData(1, 10, 4, 6, 1, 3, 7, 10)]
    public void Except_PartialOverlap_ReturnsCorrectResult(int r1Start, int r1End, int r2Start, int r2End, params int[] expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(r1Start, r1End) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(r2Start, r2End) });
        var expectedRanges = CreateRangesFromPairs(expected);
        
        var result = set1.Except(set2);
        
        Assert.Equal(expectedRanges, result.ToArray());
    }

    [Fact]
    public void Except_MultipleExclusionsFromSingleRange_SplitsCorrectly()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 20) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(5, 8), new Range<int>(12, 15) });
        
        var result = set1.Except(set2);
        
        Assert.Equal(3, result.RangesCount);
        var array = result.ToArray();
        Assert.Equal(new(1, 4), array[0]);
        Assert.Equal(new(9, 11), array[1]);
        Assert.Equal(new(16, 20), array[2]);
    }

    [Fact]
    public void Except_AtMinValue_HandlesCorrectly()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(int.MinValue, 100) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(int.MinValue, 0) });
        
        var result = set1.Except(set2);
        
        Assert.Equal(1, result.RangesCount);
        Assert.Equal(new(1, 100), result.ToArray()[0]);
    }

    [Fact]
    public void Except_AtMaxValue_HandlesCorrectly()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(0, int.MaxValue) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(int.MaxValue, int.MaxValue) });
        
        var result = set1.Except(set2);
        
        Assert.Equal(1, result.RangesCount);
        Assert.Equal(new(0, int.MaxValue - 1), result.ToArray()[0]);
    }

    [Fact]
    public void Except_NullArgument_ThrowsArgumentNullException()
    {
        var set = new ArrayRangeSet<int>();
        ArrayRangeSet<int>? nullSet = null;
        
        Assert.Throws<ArgumentNullException>(() => set.Except(nullSet!));
    }

    [Fact]
    public void Except_DoesNotModifyOriginalSets()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(3, 7) });
        var original1 = set1.ToArray();
        var original2 = set2.ToArray();
        
        set1.Except(set2);
        
        Assert.Equal(original1, set1.ToArray());
        Assert.Equal(original2, set2.ToArray());
    }

    [Fact]
    public void Except_WithSpan_ReturnsCorrectResult()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var span = new[] { new Range<int>(3, 7) };
        
        var result = set.Except(span.AsSpan());
        
        Assert.Equal(2, result.RangesCount);
        Assert.Equal(new(1, 2), result.ToArray()[0]);
        Assert.Equal(new(8, 10), result.ToArray()[1]);
    }

    #endregion

    #region Intersect Tests

    [Fact]
    public void Intersect_BothEmpty_ReturnsEmpty()
    {
        var result = new ArrayRangeSet<int>().Intersect(new ArrayRangeSet<int>());
        
        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Intersect_WithEmpty_ReturnsEmpty()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        
        var result = set.Intersect(new ArrayRangeSet<int>());
        
        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Intersect_EmptyWithOther_ReturnsEmpty()
    {
        var other = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        
        var result = new ArrayRangeSet<int>().Intersect(other);
        
        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Intersect_NoOverlap_ReturnsEmpty()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(20, 30) });
        
        var result = set1.Intersect(set2);
        
        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Intersect_CompleteOverlap_ReturnsSame()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        
        var result = set1.Intersect(set2);
        
        Assert.Equal(new(1, 10), result.ToArray()[0]);
    }

    [Fact]
    public void Intersect_PartialOverlap_ReturnsIntersection()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(5, 15) });
        
        var result = set1.Intersect(set2);
        
        Assert.Equal(new(5, 10), result.ToArray()[0]);
    }

    [Fact]
    public void Intersect_Contained_ReturnsSmaller()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(3, 7) });
        
        var result = set1.Intersect(set2);
        
        Assert.Equal(new(3, 7), result.ToArray()[0]);
    }

    [Fact]
    public void Intersect_Commutative()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10), new Range<int>(20, 30) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(5, 25) });
        
        var result1 = set1.Intersect(set2);
        var result2 = set2.Intersect(set1);
        
        Assert.Equal(result1.ToArray(), result2.ToArray());
    }

    [Fact]
    public void Intersect_MultipleRanges_ReturnsAllIntersections()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 5), new Range<int>(10, 15), new Range<int>(20, 25) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(3, 12), new Range<int>(18, 22) });
        
        var result = set1.Intersect(set2);
        
        Assert.Equal(3, result.RangesCount);
        var array = result.ToArray();
        Assert.Equal(new(3, 5), array[0]);
        Assert.Equal(new(10, 12), array[1]);
        Assert.Equal(new(20, 22), array[2]);
    }

    [Fact]
    public void Intersect_NullArgument_ThrowsArgumentNullException()
    {
        var set = new ArrayRangeSet<int>();
        ArrayRangeSet<int>? nullSet = null;
        
        Assert.Throws<ArgumentNullException>(() => set.Intersect(nullSet!));
    }

    [Fact]
    public void Intersect_DoesNotModifyOriginalSets()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(5, 15) });
        var original1 = set1.ToArray();
        var original2 = set2.ToArray();
        
        set1.Intersect(set2);
        
        Assert.Equal(original1, set1.ToArray());
        Assert.Equal(original2, set2.ToArray());
    }

    [Fact]
    public void Intersect_WithSpan_ReturnsCorrectResult()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10), new Range<int>(20, 30) });
        var span = new[] { new Range<int>(5, 25) };
        
        var result = set.Intersect(span.AsSpan());
        
        Assert.Equal(2, result.RangesCount);
        Assert.Equal(new(5, 10), result.ToArray()[0]);
        Assert.Equal(new(20, 25), result.ToArray()[1]);
    }

    #endregion

    #region Set Theory Properties

    [Fact]
    public void Union_Identity_WithEmpty()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var empty = new ArrayRangeSet<int>();
        
        var unionWithEmpty = set.Union(empty);
        var emptyUnionSet = empty.Union(set);
        
        Assert.Equal(set.ToArray(), unionWithEmpty.ToArray());
        Assert.Equal(set.ToArray(), emptyUnionSet.ToArray());
    }

    [Fact]
    public void Intersect_Identity_WithEmpty()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var empty = new ArrayRangeSet<int>();
        
        var intersectWithEmpty = set.Intersect(empty);
        
        Assert.Equal(0, intersectWithEmpty.RangesCount);
    }

    [Fact]
    public void Except_Identity_WithEmpty()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var empty = new ArrayRangeSet<int>();
        
        var exceptEmpty = set.Except(empty);
        
        Assert.Equal(set.ToArray(), exceptEmpty.ToArray());
    }

    [Fact]
    public void Except_Self_ReturnsEmpty()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10), new Range<int>(20, 30) });
        
        var result = set.Except(set);
        
        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Distributivity_UnionOverIntersect()
    {
        var a = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var b = new ArrayRangeSet<int>(new[] { new Range<int>(5, 15) });
        var c = new ArrayRangeSet<int>(new[] { new Range<int>(8, 20) });
        
        var left = a.Union(b).Intersect(c);
        var right = a.Intersect(c).Union(b.Intersect(c));
        
        Assert.Equal(left.ToArray(), right.ToArray());
    }

    #endregion

    #region Chained Operations

    [Fact]
    public void Chained_UnionThenExcept_ReturnsCorrectResult()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(5, 15) });
        var set3 = new ArrayRangeSet<int>(new[] { new Range<int>(8, 12) });
        
        var result = set1.Union(set2).Except(set3);
        
        Assert.Equal(2, result.RangesCount);
        Assert.Equal(new(1, 7), result.ToArray()[0]);
        Assert.Equal(new(13, 15), result.ToArray()[1]);
    }

    [Fact]
    public void Chained_IntersectThenUnion_ReturnsCorrectResult()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 20) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(5, 15) });
        var set3 = new ArrayRangeSet<int>(new[] { new Range<int>(25, 35) });
        
        var result = set1.Intersect(set2).Union(set3);
        
        Assert.Equal(2, result.RangesCount);
        Assert.Equal(new(5, 15), result.ToArray()[0]);
        Assert.Equal(new(25, 35), result.ToArray()[1]);
    }

    [Fact]
    public void Chained_ExceptThenIntersect_ReturnsCorrectResult()
    {
        var set1 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 20) });
        var set2 = new ArrayRangeSet<int>(new[] { new Range<int>(5, 10) });
        var set3 = new ArrayRangeSet<int>(new[] { new Range<int>(1, 15) });
        
        var result = set1.Except(set2).Intersect(set3);
        
        Assert.Equal(2, result.RangesCount);
        Assert.Equal(new(1, 4), result.ToArray()[0]);
        Assert.Equal(new(11, 15), result.ToArray()[1]);
    }

    #endregion

    #region Various Numeric Types

    [Fact]
    public void ArrayRangeSet_UInt_FullOperations()
    {
        var set1 = new ArrayRangeSet<uint>(new[] { new Range<uint>(1, 100), new Range<uint>(200, 300) });
        var set2 = new ArrayRangeSet<uint>(new[] { new Range<uint>(50, 250) });

        var union = set1.Union(set2);
        var intersect = set1.Intersect(set2);
        var except = set1.Except(set2);

        Assert.Equal(1, union.RangesCount);
        Assert.Equal(2, intersect.RangesCount);
        Assert.Equal(2, except.RangesCount);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_EmptySet_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, new ArrayRangeSet<int>().ToString());
    }

    [Fact]
    public void ToString_NonEmptySet_ContainsRangeInfo()
    {
        var set = new ArrayRangeSet<int>(new[] { new Range<int>(1, 10), new Range<int>(20, 30) });
        
        var result = set.ToString();
        
        Assert.Contains("1", result);
        Assert.Contains("10", result);
        Assert.Contains("20", result);
        Assert.Contains("30", result);
    }

    #endregion

    #region Helper Methods

    private static Range<int>[] CreateRangesFromPairs(int[] pairs)
    {
        var ranges = new Range<int>[pairs.Length / 2];
        for (int i = 0; i < pairs.Length / 2; i++)
        {
            ranges[i] = new(pairs[i * 2], pairs[i * 2 + 1]);
        }
        return ranges;
    }

    #endregion
}
