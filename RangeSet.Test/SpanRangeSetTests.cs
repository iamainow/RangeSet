namespace RangeSet.Tests;

using RangeSet;

public class SpanRangeSetTests : RangeSetTestsBase
{
    #region Adapter Methods

#pragma warning disable CA1062 // adapter methods are only called from base class with non-null args
    protected override Range<int>[] CreateSet(params Range<int>[] ranges)
    {
        var copy = (Range<int>[])ranges.Clone();
        var set = new SpanRangeSet<int>(copy.AsSpan());
        return set.ToArray();
    }

    protected override Range<int>[] Union(Range<int>[] a, Range<int>[] b)
    {
        var copyA = (Range<int>[])a.Clone();
        var copyB = (Range<int>[])b.Clone();
        var setA = new SpanRangeSet<int>(copyA.AsSpan());
        var setB = new SpanRangeSet<int>(copyB.AsSpan());
        var buffer = new Range<int>[SpanRangeSet.CalculateUnionSize(setA.RangesCount, setB.RangesCount)];
        return setA.Union(setB, buffer).ToArray();
    }

    protected override Range<int>[] Except(Range<int>[] a, Range<int>[] b)
    {
        var copyA = (Range<int>[])a.Clone();
        var copyB = (Range<int>[])b.Clone();
        var setA = new SpanRangeSet<int>(copyA.AsSpan());
        var setB = new SpanRangeSet<int>(copyB.AsSpan());
        var buffer = new Range<int>[SpanRangeSet.CalculateExceptSize(setA.RangesCount, setB.RangesCount)];
        return setA.Except(setB, buffer).ToArray();
    }

    protected override Range<int>[] Intersect(Range<int>[] a, Range<int>[] b)
    {
        var copyA = (Range<int>[])a.Clone();
        var copyB = (Range<int>[])b.Clone();
        var setA = new SpanRangeSet<int>(copyA.AsSpan());
        var setB = new SpanRangeSet<int>(copyB.AsSpan());
        var size = SpanRangeSet.CalculateIntersectSize(setA.RangesCount, setB.RangesCount);
        var buffer = new Range<int>[size];
        return setA.Intersect(setB, buffer).ToArray();
    }

    protected override string SetToString(params Range<int>[] ranges)
    {
        var copy = (Range<int>[])ranges.Clone();
        var set = new SpanRangeSet<int>(copy.AsSpan());
        return set.ToString();
    }

#pragma warning restore CA1062

    #endregion

    #region Constructor-Specific Tests

    [Fact]
    public void DefaultConstructor_CreatesEmptySet()
    {
        var set = new SpanRangeSet<int>();

        Assert.Equal(0, set.RangesCount);
        Assert.True(set.ToReadOnlySpan().IsEmpty);
        Assert.Empty(set.ToArray());
    }

    [Fact]
    public void Constructor_FromSpan_SingleRange_CreatesSet()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = R(1, 10);
        var set = new SpanRangeSet<int>(span);

        Assert.Equal(1, set.RangesCount);
        Assert.Equal(R(1, 10), set.ToReadOnlySpan()[0]);
    }

    [Fact]
    public void Constructor_Copy_CreatesShallowCopy()
    {
        Span<Range<int>> span = stackalloc Range<int>[2];
        span[0] = R(1, 10);
        span[1] = R(20, 30);
        var original = new SpanRangeSet<int>(span);

        var copy = new SpanRangeSet<int>(original);

        Assert.Equal(original.RangesCount, copy.RangesCount);
        Assert.Equal(original.ToReadOnlySpan().ToArray(), copy.ToReadOnlySpan().ToArray());
    }

    [Fact]
    public void Constructor_FromSpanRangeSet_WithBuffer_CopiesData()
    {
        Span<Range<int>> originalSpan = stackalloc Range<int>[2];
        originalSpan[0] = R(1, 10);
        originalSpan[1] = R(20, 30);
        var original = new SpanRangeSet<int>(originalSpan);

        Span<Range<int>> buffer = stackalloc Range<int>[2];
        var copy = new SpanRangeSet<int>(original, buffer);

        Assert.Equal(2, copy.RangesCount);
        Assert.Equal(R(1, 10), copy.ToReadOnlySpan()[0]);
        Assert.Equal(R(20, 30), copy.ToReadOnlySpan()[1]);
    }

    #endregion

    #region ToReadOnlySpan and ToArray

    [Fact]
    public void ToReadOnlySpan_ReturnsCorrectView()
    {
        Span<Range<int>> span = stackalloc Range<int>[2];
        span[0] = R(1, 10);
        span[1] = R(20, 30);
        var set = new SpanRangeSet<int>(span);
        var result = set.ToReadOnlySpan();

        Assert.Equal(2, result.Length);
        Assert.Equal(R(1, 10), result[0]);
        Assert.Equal(R(20, 30), result[1]);
    }

    [Fact]
    public void ToArray_ReturnsIndependentCopy()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = R(1, 10);
        var set = new SpanRangeSet<int>(span);

        var array1 = set.ToArray();
        var array2 = set.ToArray();

        Assert.NotSame(array1, array2);
        Assert.Equal(array1, array2);
    }

    #endregion

    #region Span Overload Tests

    [Fact]
    public void Union_WithSpan_ReturnsCorrectResult()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = R(1, 5);
        var set = new SpanRangeSet<int>(span);
        Span<Range<int>> buffer = stackalloc Range<int>[10];

        var result = set.Union(new[] { R(3, 10) }.AsSpan(), buffer);

        Assert.Equal([R(1, 10)], result.ToArray());
    }

    [Fact]
    public void Union_WithUnsortedSpan_NormalizesCorrectly()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = R(1, 5);
        var set = new SpanRangeSet<int>(span);
        Span<Range<int>> buffer = stackalloc Range<int>[10];

        var result = set.Union(new[] { R(20, 30), R(3, 10) }.AsSpan(), buffer);

        Assert.Equal(new[] { R(1, 10), R(20, 30) }, result.ToArray());
    }

    [Fact]
    public void Union_WithEmptySpan_ReturnsOriginal()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = R(1, 10);
        var set = new SpanRangeSet<int>(span);
        Span<Range<int>> buffer = stackalloc Range<int>[10];

        var result = set.Union(ReadOnlySpan<Range<int>>.Empty, buffer);

        Assert.Equal(set.ToArray(), result.ToArray());
    }

    [Fact]
    public void Except_WithSpan_ReturnsCorrectResult()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = R(1, 10);
        var set = new SpanRangeSet<int>(span);
        Span<Range<int>> buffer = stackalloc Range<int>[2];

        var result = set.Except(new[] { R(3, 7) }.AsSpan(), buffer);

        Assert.Equal(new[] { R(1, 2), R(8, 10) }, result.ToArray());
    }

    [Fact]
    public void Except_WithEmptySpan_ReturnsOriginal()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = R(1, 10);
        var set = new SpanRangeSet<int>(span);
        Span<Range<int>> buffer = stackalloc Range<int>[1];

        var result = set.Except(ReadOnlySpan<Range<int>>.Empty, buffer);

        Assert.Equal(set.ToArray(), result.ToArray());
    }

    [Fact]
    public void Except_WithUnsortedSpan_ReturnsCorrectResult()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = R(1, 20);
        var set = new SpanRangeSet<int>(span);
        Span<Range<int>> buffer = stackalloc Range<int>[3];

        var result = set.Except(new[] { R(10, 15), R(3, 7) }.AsSpan(), buffer);

        Assert.Equal(new[] { R(1, 2), R(8, 9), R(16, 20) }, result.ToArray());
    }

    [Fact]
    public void Intersect_WithSpan_ReturnsCorrectResult()
    {
        Span<Range<int>> span = stackalloc Range<int>[2];
        span[0] = R(1, 10);
        span[1] = R(20, 30);
        var set = new SpanRangeSet<int>(span);
        Span<Range<int>> buffer = stackalloc Range<int>[2];

        var result = set.Intersect(new[] { R(5, 25) }.AsSpan(), buffer);

        Assert.Equal(new[] { R(5, 10), R(20, 25) }, result.ToArray());
    }

    [Fact]
    public void Intersect_WithUnsortedSpan_NormalizesCorrectly()
    {
        Span<Range<int>> span = stackalloc Range<int>[2];
        span[0] = R(1, 10);
        span[1] = R(20, 30);
        var set = new SpanRangeSet<int>(span);
        Span<Range<int>> buffer = stackalloc Range<int>[3];

        var result = set.Intersect(new[] { R(25, 35), R(5, 15) }.AsSpan(), buffer);

        Assert.Equal(new[] { R(5, 10), R(25, 30) }, result.ToArray());
    }

    [Fact]
    public void Intersect_WithEmptySpan_ReturnsEmpty()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = R(1, 10);
        var set = new SpanRangeSet<int>(span);
        Span<Range<int>> buffer = stackalloc Range<int>[1];

        var result = set.Intersect(Span<Range<int>>.Empty, buffer);

        Assert.Equal(0, result.RangesCount);
    }

    #endregion

    #region Non-Mutation Tests

    [Fact]
    public void Union_DoesNotModifyOriginalSets()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = R(1, 10);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = R(20, 30);
        var set2 = new SpanRangeSet<int>(span2);
        var original1 = set1.ToArray();
        var original2 = set2.ToArray();
        Span<Range<int>> buffer = stackalloc Range<int>[2];

        set1.Union(set2, buffer);

        Assert.Equal(original1, set1.ToArray());
        Assert.Equal(original2, set2.ToArray());
    }

    [Fact]
    public void Except_DoesNotModifyOriginalSets()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = R(1, 10);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = R(3, 7);
        var set2 = new SpanRangeSet<int>(span2);
        var original1 = set1.ToArray();
        var original2 = set2.ToArray();
        Span<Range<int>> buffer = stackalloc Range<int>[2];

        set1.Except(set2, buffer);

        Assert.Equal(original1, set1.ToArray());
        Assert.Equal(original2, set2.ToArray());
    }

    [Fact]
    public void Intersect_DoesNotModifyOriginalSets()
    {
        Span<Range<int>> span1 = stackalloc Range<int>[1];
        span1[0] = R(1, 10);
        var set1 = new SpanRangeSet<int>(span1);
        Span<Range<int>> span2 = stackalloc Range<int>[1];
        span2[0] = R(5, 15);
        var set2 = new SpanRangeSet<int>(span2);
        var original1 = set1.ToArray();
        var original2 = set2.ToArray();
        Span<Range<int>> buffer = stackalloc Range<int>[1];

        set1.Intersect(set2, buffer);

        Assert.Equal(original1, set1.ToArray());
        Assert.Equal(original2, set2.ToArray());
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

    #region Various Numeric Types

    [Fact]
    public void UInt_FullOperations()
    {
        Span<Range<uint>> span1 = stackalloc Range<uint>[2];
        span1[0] = new(1, 100);
        span1[1] = new(200, 300);
        var set1 = new SpanRangeSet<uint>(span1);
        Span<Range<uint>> span2 = stackalloc Range<uint>[1];
        span2[0] = new(50, 250);
        var set2 = new SpanRangeSet<uint>(span2);

        Span<Range<uint>> unionBuf = stackalloc Range<uint>[3];
        Span<Range<uint>> intersectBuf = stackalloc Range<uint>[2];
        Span<Range<uint>> exceptBuf = stackalloc Range<uint>[3];

        Assert.Equal(1, set1.Union(set2, unionBuf).RangesCount);
        Assert.Equal(2, set1.Intersect(set2, intersectBuf).RangesCount);
        Assert.Equal(2, set1.Except(set2, exceptBuf).RangesCount);
    }

    #endregion
}
