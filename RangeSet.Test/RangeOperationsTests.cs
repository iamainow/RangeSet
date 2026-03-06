namespace RangeSet.Tests;

using RangeSet;

public class RangeOperationsTests
{
    #region Sort Tests

    [Fact]
    public void Sort_EmptySpan_DoesNotThrow()
    {
        Span<Range<int>> span = [];
        
        RangeOperations.Sort(span);
        
        Assert.Equal(0, span.Length);
    }

    [Fact]
    public void Sort_SingleElement_RemainsUnchanged()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(5, 10);
        
        RangeOperations.Sort(span);
        
        Assert.Equal(new(5, 10), span[0]);
    }

    [Fact]
    public void Sort_AlreadySorted_RemainsUnchanged()
    {
        Span<Range<int>> span = stackalloc Range<int>[3];
        span[0] = new(1, 5);
        span[1] = new(10, 15);
        span[2] = new(20, 25);
        
        RangeOperations.Sort(span);
        
        Assert.Equal(new(1, 5), span[0]);
        Assert.Equal(new(10, 15), span[1]);
        Assert.Equal(new(20, 25), span[2]);
    }

    [Fact]
    public void Sort_ReverseOrder_SortsCorrectly()
    {
        Span<Range<int>> span = stackalloc Range<int>[3];
        span[0] = new(20, 25);
        span[1] = new(10, 15);
        span[2] = new(1, 5);
        
        RangeOperations.Sort(span);
        
        Assert.Equal(new(1, 5), span[0]);
        Assert.Equal(new(10, 15), span[1]);
        Assert.Equal(new(20, 25), span[2]);
    }

    [Fact]
    public void Sort_Unsorted_SortsByFirstValue()
    {
        Span<Range<int>> span = stackalloc Range<int>[4];
        span[0] = new(100, 200);
        span[1] = new(1, 10);
        span[2] = new(50, 60);
        span[3] = new(5, 15);
        
        RangeOperations.Sort(span);
        
        Assert.Equal(new(1, 10), span[0]);
        Assert.Equal(new(5, 15), span[1]);
        Assert.Equal(new(50, 60), span[2]);
        Assert.Equal(new(100, 200), span[3]);
    }

    [Fact]
    public void Sort_WithDuplicates_HandlesCorrectly()
    {
        Span<Range<int>> span = stackalloc Range<int>[3];
        span[0] = new(10, 20);
        span[1] = new(10, 20);
        span[2] = new(10, 20);
        
        RangeOperations.Sort(span);
        
        Assert.Equal(new(10, 20), span[0]);
        Assert.Equal(new(10, 20), span[1]);
        Assert.Equal(new(10, 20), span[2]);
    }

    #endregion

    #region NormalizeSorted Tests

    [Fact]
    public void NormalizeSorted_EmptySpan_ReturnsZero()
    {
        Span<Range<int>> span = [];
        
        int result = RangeOperations.NormalizeSorted(span);
        
        Assert.Equal(0, result);
    }

    [Fact]
    public void NormalizeSorted_SingleElement_ReturnsOne()
    {
        Span<Range<int>> span = stackalloc Range<int>[1];
        span[0] = new(1, 10);
        
        int result = RangeOperations.NormalizeSorted(span);
        
        Assert.Equal(1, result);
    }

    [Fact]
    public void NormalizeSorted_NonOverlapping_ReturnsSameCount()
    {
        Span<Range<int>> span = stackalloc Range<int>[3];
        span[0] = new(1, 5);
        span[1] = new(10, 15);
        span[2] = new(20, 25);
        
        int result = RangeOperations.NormalizeSorted(span);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void NormalizeSorted_Overlapping_MergesRanges()
    {
        Span<Range<int>> span = stackalloc Range<int>[3];
        span[0] = new(1, 10);
        span[1] = new(5, 15);
        span[2] = new(20, 30);
        
        int result = RangeOperations.NormalizeSorted(span);
        
        Assert.Equal(2, result);
        Assert.Equal(new(1, 15), span[0]);
        Assert.Equal(new(20, 30), span[1]);
    }

    [Fact]
    public void NormalizeSorted_Adjacent_MergesRanges()
    {
        Span<Range<int>> span = stackalloc Range<int>[3];
        span[0] = new(1, 10);
        span[1] = new(11, 20);
        span[2] = new(21, 30);
        
        int result = RangeOperations.NormalizeSorted(span);
        
        Assert.Equal(1, result);
        Assert.Equal(new(1, 30), span[0]);
    }

    [Fact]
    public void NormalizeSorted_Contained_MergesIntoOuter()
    {
        Span<Range<int>> span = stackalloc Range<int>[3];
        span[0] = new(1, 100);
        span[1] = new(20, 30);
        span[2] = new(50, 60);
        
        int result = RangeOperations.NormalizeSorted(span);
        
        Assert.Equal(1, result);
        Assert.Equal(new(1, 100), span[0]);
    }

    [Fact]
    public void NormalizeSorted_AllOverlapping_MergesIntoSingle()
    {
        Span<Range<int>> span = stackalloc Range<int>[5];
        span[0] = new(1, 10);
        span[1] = new(5, 20);
        span[2] = new(15, 30);
        span[3] = new(25, 40);
        span[4] = new(35, 50);
        
        int result = RangeOperations.NormalizeSorted(span);
        
        Assert.Equal(1, result);
        Assert.Equal(new(1, 50), span[0]);
    }

    [Fact]
    public void NormalizeSorted_AtMaxValue_HandlesCorrectly()
    {
        Span<Range<int>> span = stackalloc Range<int>[2];
        span[0] = new(1, int.MaxValue - 1);
        span[1] = new(int.MaxValue, int.MaxValue);
        
        int result = RangeOperations.NormalizeSorted(span);
        
        Assert.Equal(1, result);
        Assert.Equal(new(1, int.MaxValue), span[0]);
    }

    #endregion

    #region NormalizeUnsorted Tests

    [Fact]
    public void NormalizeUnsorted_EmptySpan_ReturnsZero()
    {
        Span<Range<int>> span = [];
        
        int result = RangeOperations.NormalizeUnsorted(span);
        
        Assert.Equal(0, result);
    }

    [Fact]
    public void NormalizeUnsorted_UnsortedInput_SortsAndNormalizes()
    {
        Span<Range<int>> span = stackalloc Range<int>[3];
        span[0] = new(20, 30);
        span[1] = new(1, 10);
        span[2] = new(5, 15);
        
        int result = RangeOperations.NormalizeUnsorted(span);
        
        Assert.Equal(2, result);
        Assert.Equal(new(1, 15), span[0]);
        Assert.Equal(new(20, 30), span[1]);
    }

    [Fact]
    public void NormalizeUnsorted_AlreadySortedAndNormalized_ReturnsSame()
    {
        Span<Range<int>> span = stackalloc Range<int>[3];
        span[0] = new(1, 5);
        span[1] = new(10, 15);
        span[2] = new(20, 25);
        
        int result = RangeOperations.NormalizeUnsorted(span);
        
        Assert.Equal(3, result);
        Assert.Equal(new(1, 5), span[0]);
        Assert.Equal(new(10, 15), span[1]);
        Assert.Equal(new(20, 25), span[2]);
    }

    #endregion

    #region UnionNormalizedNormalized Tests

    [Fact]
    public void UnionNormalizedNormalized_BothEmpty_ReturnsZero()
    {
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.UnionNormalizedNormalized([], [], result);
        
        Assert.Equal(0, count);
    }

    [Fact]
    public void UnionNormalizedNormalized_FirstEmpty_ReturnsSecond()
    {
        Span<Range<int>> normalized2 = stackalloc Range<int>[2];
        normalized2[0] = new(1, 5);
        normalized2[1] = new(10, 15);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.UnionNormalizedNormalized([], normalized2, result);
        
        Assert.Equal(2, count);
        Assert.Equal(new(1, 5), result[0]);
        Assert.Equal(new(10, 15), result[1]);
    }

    [Fact]
    public void UnionNormalizedNormalized_SecondEmpty_ReturnsFirst()
    {
        Span<Range<int>> normalized1 = stackalloc Range<int>[2];
        normalized1[0] = new(1, 5);
        normalized1[1] = new(10, 15);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.UnionNormalizedNormalized(normalized1, [], result);
        
        Assert.Equal(2, count);
        Assert.Equal(new(1, 5), result[0]);
        Assert.Equal(new(10, 15), result[1]);
    }

    [Fact]
    public void UnionNormalizedNormalized_NonOverlapping_ReturnsBoth()
    {
        Span<Range<int>> normalized1 = stackalloc Range<int>[2];
        normalized1[0] = new(1, 5);
        normalized1[1] = new(20, 25);
        Span<Range<int>> normalized2 = stackalloc Range<int>[2];
        normalized2[0] = new(10, 15);
        normalized2[1] = new(30, 35);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.UnionNormalizedNormalized(normalized1, normalized2, result);
        
        Assert.Equal(4, count);
        Assert.Equal(new(1, 5), result[0]);
        Assert.Equal(new(10, 15), result[1]);
        Assert.Equal(new(20, 25), result[2]);
        Assert.Equal(new(30, 35), result[3]);
    }

    [Fact]
    public void UnionNormalizedNormalized_Overlapping_Merges()
    {
        Span<Range<int>> normalized1 = stackalloc Range<int>[1];
        normalized1[0] = new(1, 10);
        Span<Range<int>> normalized2 = stackalloc Range<int>[1];
        normalized2[0] = new(5, 15);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.UnionNormalizedNormalized(normalized1, normalized2, result);
        
        Assert.Equal(1, count);
        Assert.Equal(new(1, 15), result[0]);
    }

    [Fact]
    public void UnionNormalizedNormalized_Adjacent_Merges()
    {
        Span<Range<int>> normalized1 = stackalloc Range<int>[1];
        normalized1[0] = new(1, 10);
        Span<Range<int>> normalized2 = stackalloc Range<int>[1];
        normalized2[0] = new(11, 20);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.UnionNormalizedNormalized(normalized1, normalized2, result);
        
        Assert.Equal(1, count);
        Assert.Equal(new(1, 20), result[0]);
    }

    [Fact]
    public void UnionNormalizedNormalized_ResultOverlapsFirst_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<Range<int>> normalized1 = stackalloc Range<int>[1];
            normalized1[0] = new(1, 10);
            Span<Range<int>> normalized2 = stackalloc Range<int>[1];
            normalized2[0] = new(20, 30);
            RangeOperations.UnionNormalizedNormalized(normalized1, normalized2, normalized1);
        });
    }

    [Fact]
    public void UnionNormalizedNormalized_ResultOverlapsSecond_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<Range<int>> normalized1 = stackalloc Range<int>[1];
            normalized1[0] = new(1, 10);
            Span<Range<int>> normalized2 = stackalloc Range<int>[1];
            normalized2[0] = new(20, 30);
            RangeOperations.UnionNormalizedNormalized(normalized1, normalized2, normalized2);
        });
    }

    [Fact]
    public void UnionNormalizedNormalized_AtMaxValue_HandlesCorrectly()
    {
        Span<Range<int>> normalized1 = stackalloc Range<int>[1];
        normalized1[0] = new(1, int.MaxValue);
        Span<Range<int>> normalized2 = stackalloc Range<int>[1];
        normalized2[0] = new(100, 200);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.UnionNormalizedNormalized(normalized1, normalized2, result);
        
        Assert.Equal(1, count);
        Assert.Equal(new(1, int.MaxValue), result[0]);
    }

    #endregion

    #region ExceptNormalizedSorted Tests

    [Fact]
    public void ExceptNormalizedSorted_BothEmpty_ReturnsZero()
    {
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.ExceptNormalizedSorted([], [], result);
        
        Assert.Equal(0, count);
    }

    [Fact]
    public void ExceptNormalizedSorted_NormalizedEmpty_ReturnsZero()
    {
        Span<Range<int>> sorted = stackalloc Range<int>[1];
        sorted[0] = new(1, 10);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.ExceptNormalizedSorted([], sorted, result);
        
        Assert.Equal(0, count);
    }

    [Fact]
    public void ExceptNormalizedSorted_SortedEmpty_ReturnsNormalized()
    {
        Span<Range<int>> normalized = stackalloc Range<int>[2];
        normalized[0] = new(1, 5);
        normalized[1] = new(10, 15);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.ExceptNormalizedSorted(normalized, [], result);
        
        Assert.Equal(2, count);
        Assert.Equal(new(1, 5), result[0]);
        Assert.Equal(new(10, 15), result[1]);
    }

    [Fact]
    public void ExceptNormalizedSorted_NoOverlap_ReturnsNormalized()
    {
        Span<Range<int>> normalized = stackalloc Range<int>[1];
        normalized[0] = new(1, 10);
        Span<Range<int>> sorted = stackalloc Range<int>[1];
        sorted[0] = new(20, 30);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.ExceptNormalizedSorted(normalized, sorted, result);
        
        Assert.Equal(1, count);
        Assert.Equal(new(1, 10), result[0]);
    }

    [Fact]
    public void ExceptNormalizedSorted_CompleteOverlap_ReturnsEmpty()
    {
        Span<Range<int>> normalized = stackalloc Range<int>[1];
        normalized[0] = new(1, 10);
        Span<Range<int>> sorted = stackalloc Range<int>[1];
        sorted[0] = new(1, 10);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.ExceptNormalizedSorted(normalized, sorted, result);
        
        Assert.Equal(0, count);
    }

    [Fact]
    public void ExceptNormalizedSorted_PartialOverlap_Splits()
    {
        Span<Range<int>> normalized = stackalloc Range<int>[1];
        normalized[0] = new(1, 20);
        Span<Range<int>> sorted = stackalloc Range<int>[1];
        sorted[0] = new(5, 10);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.ExceptNormalizedSorted(normalized, sorted, result);
        
        Assert.Equal(2, count);
        Assert.Equal(new(1, 4), result[0]);
        Assert.Equal(new(11, 20), result[1]);
    }

    [Fact]
    public void ExceptNormalizedSorted_MultipleExclusions_SplitsMultipleTimes()
    {
        Span<Range<int>> normalized = stackalloc Range<int>[1];
        normalized[0] = new(1, 100);
        Span<Range<int>> sorted = stackalloc Range<int>[3];
        sorted[0] = new(10, 20);
        sorted[1] = new(30, 40);
        sorted[2] = new(50, 60);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.ExceptNormalizedSorted(normalized, sorted, result);
        
        Assert.Equal(4, count);
        Assert.Equal(new(1, 9), result[0]);
        Assert.Equal(new(21, 29), result[1]);
        Assert.Equal(new(41, 49), result[2]);
        Assert.Equal(new(61, 100), result[3]);
    }

    [Fact]
    public void ExceptNormalizedSorted_ResultOverlapsNormalized_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<Range<int>> normalized = stackalloc Range<int>[2];
            normalized[0] = new(1, 10);
            Span<Range<int>> sorted = stackalloc Range<int>[1];
            sorted[0] = new(20, 30);
            RangeOperations.ExceptNormalizedSorted(normalized, sorted, normalized);
        });
    }

    [Fact]
    public void ExceptNormalizedSorted_ResultOverlapsSorted_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<Range<int>> normalized = stackalloc Range<int>[1];
            normalized[0] = new(1, 10);
            Span<Range<int>> sorted = stackalloc Range<int>[2];
            sorted[0] = new(20, 30);
            RangeOperations.ExceptNormalizedSorted(normalized, sorted, sorted);
        });
    }

    [Fact]
    public void ExceptNormalizedSorted_AtMinValue_HandlesCorrectly()
    {
        Span<Range<int>> normalized = stackalloc Range<int>[1];
        normalized[0] = new(int.MinValue, 100);
        Span<Range<int>> sorted = stackalloc Range<int>[1];
        sorted[0] = new(int.MinValue, 0);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.ExceptNormalizedSorted(normalized, sorted, result);
        
        Assert.Equal(1, count);
        Assert.Equal(new(1, 100), result[0]);
    }

    [Fact]
    public void ExceptNormalizedSorted_AtMaxValue_HandlesCorrectly()
    {
        Span<Range<int>> normalized = stackalloc Range<int>[1];
        normalized[0] = new(0, int.MaxValue);
        Span<Range<int>> sorted = stackalloc Range<int>[1];
        sorted[0] = new(int.MaxValue, int.MaxValue);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.ExceptNormalizedSorted(normalized, sorted, result);
        
        Assert.Equal(1, count);
        Assert.Equal(new(0, int.MaxValue - 1), result[0]);
    }

    #endregion

    #region IntersectNormalizedNormalized Tests

    [Fact]
    public void IntersectNormalizedNormalized_BothEmpty_ReturnsZero()
    {
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.IntersectNormalizedNormalized([], [], result);
        
        Assert.Equal(0, count);
    }

    [Fact]
    public void IntersectNormalizedNormalized_FirstEmpty_ReturnsZero()
    {
        Span<Range<int>> normalized2 = stackalloc Range<int>[1];
        normalized2[0] = new(1, 10);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.IntersectNormalizedNormalized([], normalized2, result);
        
        Assert.Equal(0, count);
    }

    [Fact]
    public void IntersectNormalizedNormalized_SecondEmpty_ReturnsZero()
    {
        Span<Range<int>> normalized1 = stackalloc Range<int>[1];
        normalized1[0] = new(1, 10);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.IntersectNormalizedNormalized(normalized1, [], result);
        
        Assert.Equal(0, count);
    }

    [Fact]
    public void IntersectNormalizedNormalized_NoOverlap_ReturnsEmpty()
    {
        Span<Range<int>> normalized1 = stackalloc Range<int>[1];
        normalized1[0] = new(1, 10);
        Span<Range<int>> normalized2 = stackalloc Range<int>[1];
        normalized2[0] = new(20, 30);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.IntersectNormalizedNormalized(normalized1, normalized2, result);
        
        Assert.Equal(0, count);
    }

    [Fact]
    public void IntersectNormalizedNormalized_CompleteOverlap_ReturnsSame()
    {
        Span<Range<int>> normalized1 = stackalloc Range<int>[1];
        normalized1[0] = new(1, 10);
        Span<Range<int>> normalized2 = stackalloc Range<int>[1];
        normalized2[0] = new(1, 10);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.IntersectNormalizedNormalized(normalized1, normalized2, result);
        
        Assert.Equal(1, count);
        Assert.Equal(new(1, 10), result[0]);
    }

    [Fact]
    public void IntersectNormalizedNormalized_PartialOverlap_ReturnsIntersection()
    {
        Span<Range<int>> normalized1 = stackalloc Range<int>[1];
        normalized1[0] = new(1, 10);
        Span<Range<int>> normalized2 = stackalloc Range<int>[1];
        normalized2[0] = new(5, 15);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.IntersectNormalizedNormalized(normalized1, normalized2, result);
        
        Assert.Equal(1, count);
        Assert.Equal(new(5, 10), result[0]);
    }

    [Fact]
    public void IntersectNormalizedNormalized_Contained_ReturnsSmaller()
    {
        Span<Range<int>> normalized1 = stackalloc Range<int>[1];
        normalized1[0] = new(1, 100);
        Span<Range<int>> normalized2 = stackalloc Range<int>[1];
        normalized2[0] = new(20, 30);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.IntersectNormalizedNormalized(normalized1, normalized2, result);
        
        Assert.Equal(1, count);
        Assert.Equal(new(20, 30), result[0]);
    }

    [Fact]
    public void IntersectNormalizedNormalized_MultipleRanges_ReturnsAllIntersections()
    {
        Span<Range<int>> normalized1 = stackalloc Range<int>[3];
        normalized1[0] = new(1, 5);
        normalized1[1] = new(10, 15);
        normalized1[2] = new(20, 25);
        Span<Range<int>> normalized2 = stackalloc Range<int>[2];
        normalized2[0] = new(3, 12);
        normalized2[1] = new(18, 22);
        Span<Range<int>> result = stackalloc Range<int>[10];
        
        int count = RangeOperations.IntersectNormalizedNormalized(normalized1, normalized2, result);
        
        Assert.Equal(3, count);
        Assert.Equal(new(3, 5), result[0]);
        Assert.Equal(new(10, 12), result[1]);
        Assert.Equal(new(20, 22), result[2]);
    }

    [Fact]
    public void IntersectNormalizedNormalized_ResultOverlapsFirst_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<Range<int>> normalized1 = stackalloc Range<int>[1];
            normalized1[0] = new(1, 10);
            Span<Range<int>> normalized2 = stackalloc Range<int>[1];
            normalized2[0] = new(20, 30);
            RangeOperations.IntersectNormalizedNormalized(normalized1, normalized2, normalized1);
        });
    }

    [Fact]
    public void IntersectNormalizedNormalized_ResultOverlapsSecond_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<Range<int>> normalized1 = stackalloc Range<int>[1];
            normalized1[0] = new(1, 10);
            Span<Range<int>> normalized2 = stackalloc Range<int>[1];
            normalized2[0] = new(20, 30);
            RangeOperations.IntersectNormalizedNormalized(normalized1, normalized2, normalized2);
        });
    }

    #endregion

    #region CalcIntersectBufferLength Tests

    [Fact]
    public void CalcIntersectBufferLength_BothZero_ReturnsZero()
    {
        Assert.Equal(0, RangeOperations.CalcIntersectBufferLength(0, 0));
    }

    [Fact]
    public void CalcIntersectBufferLength_FirstZero_ReturnsZero()
    {
        Assert.Equal(0, RangeOperations.CalcIntersectBufferLength(0, 5));
    }

    [Fact]
    public void CalcIntersectBufferLength_SecondZero_ReturnsZero()
    {
        Assert.Equal(0, RangeOperations.CalcIntersectBufferLength(5, 0));
    }

    [Fact]
    public void CalcIntersectBufferLength_BothNonZero_ReturnsCorrectValue()
    {
        Assert.Equal(4, RangeOperations.CalcIntersectBufferLength(2, 3));
        Assert.Equal(9, RangeOperations.CalcIntersectBufferLength(5, 5));
        Assert.Equal(1, RangeOperations.CalcIntersectBufferLength(1, 1));
    }

    #endregion
}