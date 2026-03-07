namespace RangeSet.Tests;

using RangeSet;

public class ArrayRangeSetTests : RangeSetTestsBase
{
    #region Adapter Methods

    protected override Range<int>[] CreateSet(params Range<int>[] ranges) =>
        new ArrayRangeSet<int>(ranges).ToArray();

    protected override Range<int>[] Union(Range<int>[] a, Range<int>[] b) =>
        new ArrayRangeSet<int>(a).Union(new ArrayRangeSet<int>(b)).ToArray();

    protected override Range<int>[] Except(Range<int>[] a, Range<int>[] b) =>
        new ArrayRangeSet<int>(a).Except(new ArrayRangeSet<int>(b)).ToArray();

    protected override Range<int>[] Intersect(Range<int>[] a, Range<int>[] b) =>
        new ArrayRangeSet<int>(a).Intersect(new ArrayRangeSet<int>(b)).ToArray();

    protected override string SetToString(params Range<int>[] ranges) =>
        new ArrayRangeSet<int>(ranges).ToString();

    #endregion

    #region Constructor-Specific Tests

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
        var copy = new ArrayRangeSet<int>(new ArrayRangeSet<int>());
        Assert.Equal(0, copy.RangesCount);
    }

    [Fact]
    public void CopyConstructor_FromNonEmptySet_CreatesDeepCopy()
    {
        var original = new ArrayRangeSet<int>(new[] { R(1, 10), R(20, 30) });
        var copy = new ArrayRangeSet<int>(original);

        Assert.Equal(original.RangesCount, copy.RangesCount);
        Assert.Equal(original.ToArray(), copy.ToArray());
        Assert.NotSame(original.ToArray(), copy.ToArray());
    }

    [Fact]
    public void Constructor_FromNullArray_ThrowsArgumentNullException()
    {
        Range<int>[]? nullArray = null;
        Assert.Throws<ArgumentNullException>(() => new ArrayRangeSet<int>(nullArray!));
    }

    #endregion

    #region ToReadOnlySpan and ToArray

    [Fact]
    public void ToReadOnlySpan_ReturnsCorrectView()
    {
        var set = new ArrayRangeSet<int>(new[] { R(1, 10), R(20, 30) });
        var span = set.ToReadOnlySpan();

        Assert.Equal(2, span.Length);
        Assert.Equal(R(1, 10), span[0]);
        Assert.Equal(R(20, 30), span[1]);
    }

    [Fact]
    public void ToArray_ReturnsIndependentCopy()
    {
        var set = new ArrayRangeSet<int>(new[] { R(1, 10) });

        var array1 = set.ToArray();
        var array2 = set.ToArray();

        Assert.NotSame(array1, array2);
        Assert.Equal(array1, array2);
    }

    #endregion

    #region Null Argument Tests

    [Fact]
    public void Union_NullArgument_ThrowsArgumentNullException()
    {
        var set = new ArrayRangeSet<int>();
        ArrayRangeSet<int>? nullSet = null;
        Assert.Throws<ArgumentNullException>(() => set.Union(nullSet!));
    }

    [Fact]
    public void Except_NullArgument_ThrowsArgumentNullException()
    {
        var set = new ArrayRangeSet<int>();
        ArrayRangeSet<int>? nullSet = null;
        Assert.Throws<ArgumentNullException>(() => set.Except(nullSet!));
    }

    [Fact]
    public void Intersect_NullArgument_ThrowsArgumentNullException()
    {
        var set = new ArrayRangeSet<int>();
        ArrayRangeSet<int>? nullSet = null;
        Assert.Throws<ArgumentNullException>(() => set.Intersect(nullSet!));
    }

    #endregion

    #region Span Overload Tests

    [Fact]
    public void Union_WithSpan_ReturnsCorrectResult()
    {
        var set = new ArrayRangeSet<int>(new[] { R(1, 5) });
        var result = set.Union(new[] { R(3, 10) }.AsSpan());
        Assert.Equal([R(1, 10)], result.ToArray());
    }

    [Fact]
    public void Union_WithUnsortedSpan_NormalizesCorrectly()
    {
        var set = new ArrayRangeSet<int>(new[] { R(1, 5) });
        var result = set.Union(new[] { R(20, 30), R(3, 10) }.AsSpan());
        Assert.Equal(new[] { R(1, 10), R(20, 30) }, result.ToArray());
    }

    [Fact]
    public void Union_WithEmptySpan_ReturnsOriginal()
    {
        var set = new ArrayRangeSet<int>(new[] { R(1, 10) });
        var result = set.Union(ReadOnlySpan<Range<int>>.Empty);
        Assert.Equal(set.ToArray(), result.ToArray());
    }

    [Fact]
    public void Except_WithSpan_ReturnsCorrectResult()
    {
        var set = new ArrayRangeSet<int>(new[] { R(1, 10) });
        var result = set.Except(new[] { R(3, 7) }.AsSpan());
        Assert.Equal(new[] { R(1, 2), R(8, 10) }, result.ToArray());
    }

    [Fact]
    public void Except_WithEmptySpan_ReturnsOriginal()
    {
        var set = new ArrayRangeSet<int>(new[] { R(1, 10) });
        var result = set.Except(ReadOnlySpan<Range<int>>.Empty);
        Assert.Equal(set.ToArray(), result.ToArray());
    }

    [Fact]
    public void Except_WithUnsortedSpan_NormalizesCorrectly()
    {
        var set = new ArrayRangeSet<int>(new[] { R(1, 20) });
        var result = set.Except(new[] { R(10, 15), R(3, 7) }.AsSpan());
        Assert.Equal(new[] { R(1, 2), R(8, 9), R(16, 20) }, result.ToArray());
    }

    [Fact]
    public void Intersect_WithSpan_ReturnsCorrectResult()
    {
        var set = new ArrayRangeSet<int>(new[] { R(1, 10), R(20, 30) });
        var result = set.Intersect(new[] { R(5, 25) }.AsSpan());
        Assert.Equal(new[] { R(5, 10), R(20, 25) }, result.ToArray());
    }

    [Fact]
    public void Intersect_WithEmptySpan_ReturnsEmpty()
    {
        var set = new ArrayRangeSet<int>(new[] { R(1, 10) });
        var result = set.Intersect(ReadOnlySpan<Range<int>>.Empty);
        Assert.Equal(0, result.RangesCount);
    }

    #endregion

    #region Non-Mutation Tests

    [Fact]
    public void Union_DoesNotModifyOriginalSets()
    {
        var set1 = new ArrayRangeSet<int>(new[] { R(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { R(20, 30) });
        var original1 = set1.ToArray();
        var original2 = set2.ToArray();

        set1.Union(set2);

        Assert.Equal(original1, set1.ToArray());
        Assert.Equal(original2, set2.ToArray());
    }

    [Fact]
    public void Except_DoesNotModifyOriginalSets()
    {
        var set1 = new ArrayRangeSet<int>(new[] { R(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { R(3, 7) });
        var original1 = set1.ToArray();
        var original2 = set2.ToArray();

        set1.Except(set2);

        Assert.Equal(original1, set1.ToArray());
        Assert.Equal(original2, set2.ToArray());
    }

    [Fact]
    public void Intersect_DoesNotModifyOriginalSets()
    {
        var set1 = new ArrayRangeSet<int>(new[] { R(1, 10) });
        var set2 = new ArrayRangeSet<int>(new[] { R(5, 15) });
        var original1 = set1.ToArray();
        var original2 = set2.ToArray();

        set1.Intersect(set2);

        Assert.Equal(original1, set1.ToArray());
        Assert.Equal(original2, set2.ToArray());
    }

    #endregion

    #region Various Numeric Types

    [Fact]
    public void UInt_FullOperations()
    {
        var set1 = new ArrayRangeSet<uint>(new[] { new Range<uint>(1, 100), new Range<uint>(200, 300) });
        var set2 = new ArrayRangeSet<uint>(new[] { new Range<uint>(50, 250) });

        Assert.Equal(1, set1.Union(set2).RangesCount);
        Assert.Equal(2, set1.Intersect(set2).RangesCount);
        Assert.Equal(2, set1.Except(set2).RangesCount);
    }

    [Fact]
    public void Long_FullOperations()
    {
        var set1 = new ArrayRangeSet<long>(new[] { new Range<long>(1L, 100L), new Range<long>(200L, 300L) });
        var set2 = new ArrayRangeSet<long>(new[] { new Range<long>(50L, 250L) });

        var union = set1.Union(set2);
        Assert.Equal(1, union.RangesCount);
        Assert.Equal(new Range<long>(1L, 300L), union.ToArray()[0]);

        var intersect = set1.Intersect(set2);
        Assert.Equal(new[] { new Range<long>(50L, 100L), new Range<long>(200L, 250L) }, intersect.ToArray());

        var except = set1.Except(set2);
        Assert.Equal(new[] { new Range<long>(1L, 49L), new Range<long>(251L, 300L) }, except.ToArray());
    }

    #endregion
}
