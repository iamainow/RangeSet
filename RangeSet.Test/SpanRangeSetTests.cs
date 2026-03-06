namespace RangeSet.Tests;

using RangeSet;

public class SpanRangeSetTests
{
    #region Constructors

    [Fact]
    public void DefaultConstructor_CreatesEmptySet()
    {
        var set = new SpanRangeSet<int>();
        
        Assert.Equal(0, set.RangesCount);
        Assert.True(set.ToReadOnlySpan().IsEmpty);
        Assert.Empty(set.ToArray());
    }

    [Fact]
    public void Constructor_FromSpan_EmptySpan_CreatesEmptySet()
    {
        Span<Range<int>> span = [];
        
        var set = new SpanRangeSet<int>(span);
        
        Assert.Equal(0, set.RangesCount);
    }

    [Fact]
    public void Constructor_FromSpan_SingleRange_CreatesSet()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 10);
        
        var set = new SpanRangeSet<int>(span);
        
        Assert.Equal(1, set.ToReadOnlySpan().Length);
        Assert.Equal(new(1, 10), set.ToReadOnlySpan()[0]);
    }

    [Fact]
    public void Constructor_FromSpan_OverlappingRanges_Merges()
    {
        Span<Range<int>> span = stackalloc Range<int>[3];
        span[0] = new(1, 10);
        span[1] = new(5, 15);
        span[2] = new(20, 30);
        
        var set = new SpanRangeSet<int>(span);
        
        Assert.Equal(2, set.RangesCount);
        var result = set.ToReadOnlySpan();
        Assert.Equal(new(1, 15), result[0]);
        Assert.Equal(new(20, 30), result[1]);
    }

    [Fact]
    public void Constructor_FromSpan_AdjacentRanges_Merges()
    {
        Span<Range<int>> span = stackalloc Range<int>[2];
        span[0] = new(1, 10);
        span[1] = new(11, 20);
        
        var set = new SpanRangeSet<int>(span);
        
        Assert.Equal(1, set.RangesCount);
        Assert.Equal(new(1, 20), set.ToReadOnlySpan()[0]);
    }

    [Fact]
    public void Constructor_FromSpan_ContainedRanges_Merges()
    {
        Span<Range<int>> span = stackalloc Range<int>[3];
        span[0] = new(1, 100);
        span[1] = new(20, 30);
        span[2] = new(50, 60);
        
        var set = new SpanRangeSet<int>(span);
        
        Assert.Equal(1, set.RangesCount);
        Assert.Equal(new(1, 100), set.ToReadOnlySpan()[0]);
    }

    [Fact]
    public void Constructor_Copy_CreatesShallowCopy()
    {
        Span<Range<int>> span = stackalloc Range<int>[2];
        span[0] = new(1, 10);
        span[1] = new(20, 30);
        var original = new SpanRangeSet<int>(span);
        
        var copy = new SpanRangeSet<int>(original);
        
        Assert.Equal(original.RangesCount, copy.RangesCount);
        Assert.Equal(original.ToReadOnlySpan().ToArray(), copy.ToReadOnlySpan().ToArray());
    }

    [Fact]
    public void Constructor_FromSpanRangeSet_WithBuffer_CopiesData()
    {
        Span<Range<int>> originalSpan = stackalloc Range<int>[2];
        originalSpan[0] = new(1, 10);
        originalSpan[1] = new(20, 30);
        var original = new SpanRangeSet<int>(originalSpan);
        
        Span<Range<int>> buffer = stackalloc Range<int>[2];
        var copy = new SpanRangeSet<int>(original, buffer);
        
        Assert.Equal(2, copy.RangesCount);
        Assert.Equal(new(1, 10), copy.ToReadOnlySpan()[0]);
        Assert.Equal(new(20, 30), copy.ToReadOnlySpan()[1]);
    }

    [Fact]
    public void Constructor_AtMinValue_Success()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(int.MinValue, -100);
        
        var set = new SpanRangeSet<int>(span);
        
        Assert.Equal(1, set.RangesCount);
        Assert.Equal(int.MinValue, set.ToReadOnlySpan()[0].First);
    }

    [Fact]
    public void Constructor_AtMaxValue_Success()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(100, int.MaxValue);
        
        var set = new SpanRangeSet<int>(span);
        
        Assert.Equal(1, set.RangesCount);
        Assert.Equal(int.MaxValue, set.ToReadOnlySpan()[0].Last);
    }

    #endregion

    #region ToReadOnlySpan and ToArray

    [Fact]
    public void ToReadOnlySpan_ReturnsCorrectView()
    {
        Span<Range<int>> span = stackalloc Range<int>[2];
        span[0] = new(1, 10);
        span[1] = new(20, 30);
        var set = new SpanRangeSet<int>(span);
        
        var result = set.ToReadOnlySpan();
        
        Assert.Equal(2, result.Length);
        Assert.Equal(new(1, 10), result[0]);
        Assert.Equal(new(20, 30), result[1]);
    }

    [Fact]
    public void ToArray_ReturnsIndependentCopy()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 10);
        var set = new SpanRangeSet<int>(span);
        
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
        var set1 = new SpanRangeSet<int>();
        var set2 = new SpanRangeSet<int>();
        Span<Range<int>> buffer = stackalloc Range<int>[10];
        
        var result = set1.Union(set2, buffer);
        
        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Union_WithEmpty_ReturnsOriginal()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 10);
        var set = new SpanRangeSet<int>(span);
        var empty = new SpanRangeSet<int>();
        Span<Range<int>> buffer = stackalloc Range<int>[10];
        
        var result = set.Union(empty, buffer);
        
        Assert.Equal(set.ToReadOnlySpan().ToArray(), result.ToReadOnlySpan().ToArray());
    }

    [Fact]
    public void Union_EmptyWithNonEmpty_ReturnsOther()
    {
        var empty = new SpanRangeSet<int>();
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 10);
        var other = new SpanRangeSet<int>(span);
        Span<Range<int>> buffer = stackalloc Range<int>[10];
        
        var result = empty.Union(other, buffer);
        
        Assert.Equal(other.ToReadOnlySpan().ToArray(), result.ToReadOnlySpan().ToArray());
    }

    [Fact]
    public void Union_NonOverlapping_ReturnsBothRangesSorted()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(20, 30);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(1, 10);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer = stackalloc Range<int>[10];
        
        var result = set1.Union(set2, buffer);
        
        Assert.Equal(2, result.RangesCount);
        Assert.Equal(new(1, 10), result.ToReadOnlySpan()[0]);
        Assert.Equal(new(20, 30), result.ToReadOnlySpan()[1]);
    }

    [Fact]
    public void Union_Overlapping_MergesIntoSingle()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(1, 10);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(5, 15);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer = stackalloc Range<int>[10];
        
        var result = set1.Union(set2, buffer);
        
        Assert.Equal(1, result.RangesCount);
        Assert.Equal(new(1, 15), result.ToReadOnlySpan()[0]);
    }

    [Fact]
    public void Union_Adjacent_MergesIntoSingle()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(1, 10);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(11, 20);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer = stackalloc Range<int>[10];
        
        var result = set1.Union(set2, buffer);
        
        Assert.Equal(1, result.RangesCount);
        Assert.Equal(new(1, 20), result.ToReadOnlySpan()[0]);
    }

    [Fact]
    public void Union_Commutative()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[2];
        span1[0] = new(1, 10);
        span1[1] = new(30, 40);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(5, 35);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer1 = stackalloc Range<int>[10];
        Span<Range<int>> buffer2 = stackalloc Range<int>[10];
        
        var result1 = set1.Union(set2, buffer1);
        var result2 = set2.Union(set1, buffer2);
        
        Assert.Equal(result1.ToReadOnlySpan().ToArray(), result2.ToReadOnlySpan().ToArray());
    }

    [Fact]
    public void Union_Self_ReturnsSameRanges()
    {
        Span<Range<int>> span = stackalloc Range<int>[2];
        span[0] = new(1, 10);
        span[1] = new(20, 30);
        var set = new SpanRangeSet<int>(span);
        Span<Range<int>> buffer = stackalloc Range<int>[10];
        
        var result = set.Union(set, buffer);
        
        Assert.Equal(set.ToReadOnlySpan().ToArray(), result.ToReadOnlySpan().ToArray());
    }

    [Fact]
    public void Union_WithSpan_ReturnsCorrectResult()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 5);
        var set = new SpanRangeSet<int>(span);
        var other = new[] { new Range<int>(3, 10) };
        Span<Range<int>> buffer = stackalloc Range<int>[10];
        
        var result = set.Union(other.AsSpan(), buffer);
        
        Assert.Equal(new(1, 10), result.ToReadOnlySpan()[0]);
    }

    [Fact]
    public void Union_WithUnsortedSpan_NormalizesCorrectly()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 5);
        var set = new SpanRangeSet<int>(span);
        var unsorted = new[] { new Range<int>(20, 30), new Range<int>(3, 10) };
        Span<Range<int>> buffer = stackalloc Range<int>[10];
        
        var result = set.Union(unsorted.AsSpan(), buffer);
        
        Assert.Equal(2, result.RangesCount);
        Assert.Equal(new(1, 10), result.ToReadOnlySpan()[0]);
        Assert.Equal(new(20, 30), result.ToReadOnlySpan()[1]);
    }

    #endregion

    #region Except Tests

    [Fact]
    public void Except_BothEmpty_ReturnsEmpty()
    {
        var set1 = new SpanRangeSet<int>();
        var set2 = new SpanRangeSet<int>();
        Span<Range<int>> buffer = stackalloc Range<int>[1];
        var result = set1.Except(set2, buffer);

        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Except_FromEmpty_ReturnsEmpty()
    {
        var empty = new SpanRangeSet<int>();
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 10);
        var other = new SpanRangeSet<int>(span);
        Span<Range<int>> buffer = stackalloc Range<int>[1];
        var result = empty.Except(other, buffer);

        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Except_EmptyFromOther_ReturnsOriginal()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 10);
        var set = new SpanRangeSet<int>(span);
        var empty = new SpanRangeSet<int>();
        Span<Range<int>> buffer = stackalloc Range<int>[1];
        var result = set.Except(empty, buffer);

        Assert.Equal(set.ToReadOnlySpan().ToArray(), result.ToReadOnlySpan().ToArray());
    }

    [Fact]
    public void Except_NoOverlap_ReturnsOriginal()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(1, 10);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(20, 30);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer = stackalloc Range<int>[2];
        var result = set1.Except(set2, buffer);

        Assert.Equal(set1.ToReadOnlySpan().ToArray(), result.ToReadOnlySpan().ToArray());
    }

    [Fact]
    public void Except_CompleteOverlap_ReturnsEmpty()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(1, 10);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(1, 10);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer = stackalloc Range<int>[2];
        var result = set1.Except(set2, buffer);

        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Except_Superset_ReturnsEmpty()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(5, 8);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(1, 10);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer = stackalloc Range<int>[2];
        var result = set1.Except(set2, buffer);

        Assert.Equal(0, result.RangesCount);
    }

    [Theory]
    [InlineData(1, 10, 1, 5, 6, 10)]
    [InlineData(1, 10, 5, 10, 1, 4)]
    [InlineData(1, 10, 4, 6, 1, 3, 7, 10)]
    public void Except_PartialOverlap_ReturnsCorrectResult(int r1Start, int r1End, int r2Start, int r2End, params int[] expected)
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(r1Start, r1End);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(r2Start, r2End);
        var set2 = new SpanRangeSet<int>(span2);
        ArgumentNullException.ThrowIfNull(expected);
        var expectedRanges = CreateRangesFromPairs(expected);
        Span<Range<int>> buffer = stackalloc Range<int>[2];
        var result = set1.Except(set2, buffer);

        Assert.Equal(expectedRanges, result.ToReadOnlySpan().ToArray());
    }

    [Fact]
    public void Except_MultipleExclusionsFromSingleRange_SplitsCorrectly()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(1, 20);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[2];
        span2[0] = new(5, 8);
        span2[1] = new(12, 15);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer = stackalloc Range<int>[3];
        var result = set1.Except(set2, buffer);

        Assert.Equal(3, result.RangesCount);
        var array = result.ToReadOnlySpan();
        Assert.Equal(new(1, 4), array[0]);
        Assert.Equal(new(9, 11), array[1]);
        Assert.Equal(new(16, 20), array[2]);
    }

    [Fact]
    public void Except_AtMinValue_HandlesCorrectly()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(int.MinValue, 100);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(int.MinValue, 0);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer = stackalloc Range<int>[2];
        var result = set1.Except(set2, buffer);

        Assert.Equal(1, result.RangesCount);
        Assert.Equal(new(1, 100), result.ToReadOnlySpan()[0]);
    }

    [Fact]
    public void Except_AtMaxValue_HandlesCorrectly()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(0, int.MaxValue);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(int.MaxValue, int.MaxValue);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer = stackalloc Range<int>[2];
        var result = set1.Except(set2, buffer);

        Assert.Equal(1, result.RangesCount);
        Assert.Equal(new(0, int.MaxValue - 1), result.ToReadOnlySpan()[0]);
    }

    [Fact]
    public void Except_WithSpan_ReturnsCorrectResult()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 10);
        var set = new SpanRangeSet<int>(span);
        var other = new[] { new Range<int>(3, 7) };
        Span<Range<int>> buffer = stackalloc Range<int>[2];
        var result = set.Except(other.AsSpan(), buffer);
        
        Assert.Equal(2, result.RangesCount);
        Assert.Equal(new(1, 2), result.ToReadOnlySpan()[0]);
        Assert.Equal(new(8, 10), result.ToReadOnlySpan()[1]);
    }

    #endregion

    #region Intersect Tests

    [Fact]
    public void Intersect_BothEmpty_ReturnsEmpty()
    {
        var set1 = new SpanRangeSet<int>();
        var set2 = new SpanRangeSet<int>();
        Span<Range<int>> buffer = stackalloc Range<int>[1];
        var result = set1.Intersect(set2, buffer);

        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Intersect_WithEmpty_ReturnsEmpty()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 10);
        var set = new SpanRangeSet<int>(span);
        var empty = new SpanRangeSet<int>();
        Span<Range<int>> buffer = stackalloc Range<int>[1];
        var result = set.Intersect(empty, buffer);

        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Intersect_EmptyWithOther_ReturnsEmpty()
    {
        var empty = new SpanRangeSet<int>();
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 10);
        var other = new SpanRangeSet<int>(span);
        Span<Range<int>> buffer = stackalloc Range<int>[1];
        var result = empty.Intersect(other, buffer);

        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Intersect_NoOverlap_ReturnsEmpty()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(1, 10);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(20, 30);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer = stackalloc Range<int>[1];
        var result = set1.Intersect(set2, buffer);

        Assert.Equal(0, result.RangesCount);
    }

    [Fact]
    public void Intersect_CompleteOverlap_ReturnsSame()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 10);
        var set1 = new SpanRangeSet<int>(span);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(1, 10);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer = stackalloc Range<int>[1];
        var result = set1.Intersect(set2, buffer);

        Assert.Equal(new(1, 10), result.ToReadOnlySpan()[0]);
    }

    [Fact]
    public void Intersect_PartialOverlap_ReturnsIntersection()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(1, 10);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(5, 15);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer = stackalloc Range<int>[1];
        var result = set1.Intersect(set2, buffer);

        Assert.Equal(new(5, 10), result.ToReadOnlySpan()[0]);
    }

    [Fact]
    public void Intersect_Contained_ReturnsSmaller()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(1, 10);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(3, 7);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer = stackalloc Range<int>[1];
        var result = set1.Intersect(set2, buffer);

        Assert.Equal(new(3, 7), result.ToReadOnlySpan()[0]);
    }

    [Fact]
    public void Intersect_Commutative()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[2];
        span1[0] = new(1, 10);
        span1[1] = new(20, 30);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(5, 25);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer1 = stackalloc Range<int>[2];
        Span<Range<int>> buffer2 = stackalloc Range<int>[2];
        var result1 = set1.Intersect(set2, buffer1);
        var result2 = set2.Intersect(set1, buffer2);
        
        Assert.Equal(result1.ToReadOnlySpan().ToArray(), result2.ToReadOnlySpan().ToArray());
    }

    [Fact]
    public void Intersect_MultipleRanges_ReturnsAllIntersections()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[3];
        span1[0] = new(1, 5);
        span1[1] = new(10, 15);
        span1[2] = new(20, 25);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[2];
        span2[0] = new(3, 12);
        span2[1] = new(18, 22);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> buffer = stackalloc Range<int>[4];
        var result = set1.Intersect(set2, buffer);

        Assert.Equal(3, result.RangesCount);
        var array = result.ToReadOnlySpan();
        Assert.Equal(new(3, 5), array[0]);
        Assert.Equal(new(10, 12), array[1]);
        Assert.Equal(new(20, 22), array[2]);
    }

    [Fact]
    public void Intersect_WithSpan_ReturnsCorrectResult()
    {
        Span<Range<int>> span = stackalloc Range<int>[2];
        span[0] = new(1, 10);
        span[1] = new(20, 30);
        var set = new SpanRangeSet<int>(span);
        var other = new[] { new Range<int>(5, 25) };
        Span<Range<int>> buffer = stackalloc Range<int>[2];
        var result = set.Intersect(other.AsSpan(), buffer);
        
        Assert.Equal(2, result.RangesCount);
        Assert.Equal(new(5, 10), result.ToReadOnlySpan()[0]);
        Assert.Equal(new(20, 25), result.ToReadOnlySpan()[1]);
    }

    [Fact]
    public void Intersect_WithUnsortedSpan_NormalizesCorrectly()
    {
        Span<Range<int>> span = stackalloc Range<int>[2];
        span[0] = new(1, 10);
        span[1] = new(20, 30);
        var set = new SpanRangeSet<int>(span);
        var unsorted = new[] { new Range<int>(25, 35), new Range<int>(5, 15) };
        Span<Range<int>> buffer = stackalloc Range<int>[3];
        var result = set.Intersect(unsorted.AsSpan(), buffer);
        
        Assert.Equal(2, result.RangesCount);
        Assert.Equal(new(5, 10), result.ToReadOnlySpan()[0]);
        Assert.Equal(new(25, 30), result.ToReadOnlySpan()[1]);
    }

    [Fact]
    public void Intersect_WithEmptySpan_ReturnsEmpty()
    {
        Span<Range<int>> span = stackalloc Range<int>[2];
        span[0] = new(1, 10);
        span[1] = new(20, 30);
        var set = new SpanRangeSet<int>(span);
        
        Span<Range<int>> buffer = stackalloc Range<int>[1];
        var result = set.Intersect(Span<Range<int>>.Empty, buffer);
        
        Assert.Equal(0, result.RangesCount);
    }

    #endregion

    #region Set Theory Properties

    [Fact]
    public void Union_Identity_WithEmpty()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 10);
        var set = new SpanRangeSet<int>(span);
        var empty = new SpanRangeSet<int>();
        Span<Range<int>> buffer = stackalloc Range<int>[10];
        
        var unionWithEmpty = set.Union(empty, buffer);
        
        Assert.Equal(set.ToReadOnlySpan().ToArray(), unionWithEmpty.ToReadOnlySpan().ToArray());
    }

    [Fact]
    public void Intersect_Identity_WithEmpty()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 10);
        var set = new SpanRangeSet<int>(span);
        var empty = new SpanRangeSet<int>();
        
        Span<Range<int>> buffer = stackalloc Range<int>[1];
        var intersectWithEmpty = set.Intersect(empty, buffer);

        Assert.Equal(0, intersectWithEmpty.RangesCount);
    }

    [Fact]
    public void Except_Identity_WithEmpty()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 10);
        var set = new SpanRangeSet<int>(span);
        var empty = new SpanRangeSet<int>();
        
        Span<Range<int>> buffer = stackalloc Range<int>[1];
        var exceptEmpty = set.Except(empty, buffer);

        Assert.Equal(set.ToReadOnlySpan().ToArray(), exceptEmpty.ToReadOnlySpan().ToArray());
    }

    [Fact]
    public void Except_Self_ReturnsEmpty()
    {
        Span<Range<int>> span = stackalloc Range<int>[2];
        span[0] = new(1, 10);
        span[1] = new(20, 30);
        var set = new SpanRangeSet<int>(span);
        Span<Range<int>> buffer = stackalloc Range<int>[4];
        var result = set.Except(set, buffer);

        Assert.Equal(0, result.RangesCount);
    }

    #endregion

    #region Chained Operations

    [Fact]
    public void Chained_UnionThenExcept_ReturnsCorrectResult()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(1, 10);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(5, 15);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> span3 = stackalloc Range<int>[1];
        span3[0] = new(8, 12);
        var set3 = new SpanRangeSet<int>(span3);
        Span<Range<int>> buffer = stackalloc Range<int>[10];
        Span<Range<int>> exceptBuffer = stackalloc Range<int>[3];
        var result = set1.Union(set2, buffer).Except(set3, exceptBuffer);
        
        Assert.Equal(2, result.RangesCount);
        Assert.Equal(new(1, 7), result.ToReadOnlySpan()[0]);
        Assert.Equal(new(13, 15), result.ToReadOnlySpan()[1]);
    }

    [Fact]
    public void Chained_IntersectThenUnion_ReturnsCorrectResult()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = new(1, 20);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = new(5, 15);
        var set2 = new SpanRangeSet<int>(span2);
        Span<Range<int>> span3 = stackalloc Range<int>[1];
        span3[0] = new(25, 35);
        var set3 = new SpanRangeSet<int>(span3);
        Span<Range<int>> buffer = stackalloc Range<int>[10];
        Span<Range<int>> intersectBuffer = stackalloc Range<int>[1];
        var result = set1.Intersect(set2, intersectBuffer).Union(set3, buffer);
        
        Assert.Equal(2, result.RangesCount);
        Assert.Equal(new(5, 15), result.ToReadOnlySpan()[0]);
        Assert.Equal(new(25, 35), result.ToReadOnlySpan()[1]);
    }

    #endregion

    #region Various Numeric Types

    [Fact]
    public void SpanRangeSet_UInt_FullOperations()
    {
        Span<Range<uint>> span1 = stackalloc Range<uint>[2];
        span1[0] = new(1, 100);
        span1[1] = new(200, 300);
        var set1 = new SpanRangeSet<uint>(span1);
        Span<Range<uint>> span2 = stackalloc Range<uint>[1];
        span2[0] = new(50, 250);
        var set2 = new SpanRangeSet<uint>(span2);
        Span<Range<uint>> buffer = stackalloc Range<uint>[10];

        Span<Range<uint>> intersectBuffer = stackalloc Range<uint>[2];
        Span<Range<uint>> exceptBuffer = stackalloc Range<uint>[3];
        var union = set1.Union(set2, buffer);
        var intersect = set1.Intersect(set2, intersectBuffer);
        var except = set1.Except(set2, exceptBuffer);

        Assert.Equal(1, union.RangesCount);
        Assert.Equal(2, intersect.RangesCount);
        Assert.Equal(2, except.RangesCount);
    }

    #endregion

    #region Static Helper Methods

    [Fact]
    public void CalculateUnionSize_ReturnsCorrectSize()
    {
        Assert.Equal(5, SpanRangeSet.CalculateUnionSize(2, 3));
        Assert.Equal(0, SpanRangeSet.CalculateUnionSize(0, 0));
        Assert.Equal(10, SpanRangeSet.CalculateUnionSize(10, 0));
        Assert.Equal(10, SpanRangeSet.CalculateUnionSize(0, 10));
    }

    [Fact]
    public void CalculateExceptSize_ReturnsCorrectSize()
    {
        Assert.Equal(5, SpanRangeSet.CalculateExceptSize(2, 3));
        Assert.Equal(0, SpanRangeSet.CalculateExceptSize(0, 0));
        Assert.Equal(10, SpanRangeSet.CalculateExceptSize(10, 0));
        // Returns a conservative upper bound; Except(∅, n) is always empty at runtime,
        // but the buffer size formula doesn't special-case this.
        Assert.Equal(5, SpanRangeSet.CalculateExceptSize(0, 5));
    }

    [Fact]
    public void CalculateIntersectSize_ReturnsCorrectSize()
    {
        Assert.Equal(4, SpanRangeSet.CalculateIntersectSize(2, 3));
        Assert.Equal(0, SpanRangeSet.CalculateIntersectSize(0, 3));
        Assert.Equal(0, SpanRangeSet.CalculateIntersectSize(2, 0));
        Assert.Equal(0, SpanRangeSet.CalculateIntersectSize(0, 0));
        Assert.Equal(1, SpanRangeSet.CalculateIntersectSize(1, 1));
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_EmptySet_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, new SpanRangeSet<int>().ToString());
    }

    [Fact]
    public void ToString_NonEmptySet_ContainsRangeInfo()
    {
        Span<Range<int>> span = stackalloc Range<int>[2];
        span[0] = new(1, 10);
        span[1] = new(20, 30);
        var set = new SpanRangeSet<int>(span);
        
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
