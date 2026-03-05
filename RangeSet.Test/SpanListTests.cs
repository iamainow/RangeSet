namespace RangeSet.Tests;

using RangeSet;

public class SpanListTests
{
    #region Constructors

    [Fact]
    public void DefaultConstructor_CreatesEmptyList()
    {
        Span<int> buffer = stackalloc int[10];
        var list = new SpanList<int>(buffer);
        
        Assert.Equal(0, list.Count);
        Assert.Equal(10, list.Capacity);
    }

    [Fact]
    public void Constructor_WithCount_InitializesWithElements()
    {
        Span<int> buffer = stackalloc int[10];
        buffer[0] = 1;
        buffer[1] = 2;
        buffer[2] = 3;
        
        var list = new SpanList<int>(buffer, 3);
        
        Assert.Equal(3, list.Count);
        Assert.Equal(10, list.Capacity);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void Constructor_WithCount_ThrowsWhenCountExceedsCapacity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            Span<int> buffer = stackalloc int[5];
            var list = new SpanList<int>(buffer, 6);
        });
    }

    [Fact]
    public void Constructor_WithCount_AcceptsZeroCount()
    {
        Span<int> buffer = stackalloc int[5];
        
        var list = new SpanList<int>(buffer, 0);
        
        Assert.Equal(0, list.Count);
    }

    [Fact]
    public void Constructor_WithCount_AcceptsFullCapacityCount()
    {
        Span<int> buffer = stackalloc int[5];
        for (int i = 0; i < 5; i++) buffer[i] = i + 1;
        
        var list = new SpanList<int>(buffer, 5);
        
        Assert.Equal(5, list.Count);
        Assert.Equal(5, list[4]);
    }

    [Fact]
    public void Constructor_FromReadOnlySpan_CopiesElements()
    {
        Span<int> buffer = stackalloc int[10];
        ReadOnlySpan<int> source = stackalloc int[] { 1, 2, 3, 4, 5 };
        
        var list = new SpanList<int>(buffer, source);
        
        Assert.Equal(5, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
        Assert.Equal(4, list[3]);
        Assert.Equal(5, list[4]);
    }

    [Fact]
    public void Constructor_FromReadOnlySpan_CopiesElementsFromSource()
    {
        Span<int> buffer = stackalloc int[10];
        var source = new int[] { 1, 2, 3 };

        var list = new SpanList<int>(buffer, source);

        // Mutate source after construction; list should be unaffected.
        source[0] = 99;

        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    #endregion

    #region Indexer

    [Fact]
    public void Indexer_Get_ReturnsCorrectElement()
    {
        Span<int> buffer = stackalloc int[10];
        buffer[0] = 42;
        buffer[1] = 100;
        var list = new SpanList<int>(buffer, 2);
        
        Assert.Equal(42, list[0]);
        Assert.Equal(100, list[1]);
    }

    [Fact]
    public void Indexer_Get_ThrowsWhenIndexNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            Span<int> buffer = stackalloc int[10];
            var list = new SpanList<int>(buffer);
            var _ = list[-1];
        });
    }

    [Fact]
    public void Indexer_Get_ThrowsWhenIndexEqualsCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            Span<int> buffer = stackalloc int[10];
            var list = new SpanList<int>(buffer);
            var _ = list[0];
        });
    }

    [Fact]
    public void Indexer_Get_ThrowsWhenIndexExceedsCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            Span<int> buffer = stackalloc int[10];
            buffer[0] = 1;
            var list = new SpanList<int>(buffer, 1);
            var _ = list[5];
        });
    }

    [Fact]
    public void Indexer_Set_UpdatesElement()
    {
        Span<int> buffer = stackalloc int[10];
        buffer[0] = 1;
        var list = new SpanList<int>(buffer, 1);
        
        list[0] = 42;
        
        Assert.Equal(42, list[0]);
    }

    [Fact]
    public void Indexer_Range_ReturnsCorrectSlice()
    {
        Span<int> buffer = stackalloc int[10];
        for (int i = 0; i < 5; i++) buffer[i] = i + 1;
        var list = new SpanList<int>(buffer, 5);
        
        var slice = list[1..3];
        
        Assert.Equal(2, slice.Length);
        Assert.Equal(2, slice[0]);
        Assert.Equal(3, slice[1]);
    }

    #endregion

    #region Last

    [Fact]
    public void Last_ReturnsRefToLastElement()
    {
        Span<int> buffer = stackalloc int[10];
        buffer[0] = 1;
        buffer[1] = 2;
        buffer[2] = 3;
        var list = new SpanList<int>(buffer, 3);
        
        ref int last = ref list.Last();
        
        Assert.Equal(3, last);
        
        last = 42;
        
        Assert.Equal(42, list[2]);
    }

    [Fact]
    public void Last_ThrowsWhenListEmpty()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            Span<int> buffer = stackalloc int[10];
            var list = new SpanList<int>(buffer);
            list.Last();
        });
    }

    #endregion

    #region Add

    [Fact]
    public void Add_IncrementsCount()
    {
        Span<int> buffer = stackalloc int[10];
        var list = new SpanList<int>(buffer);
        
        list.Add(1);
        
        Assert.Equal(1, list.Count);
        
        list.Add(2);
        
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void Add_StoresElement()
    {
        Span<int> buffer = stackalloc int[10];
        var list = new SpanList<int>(buffer);
        
        list.Add(42);
        
        Assert.Equal(42, list[0]);
    }

    [Fact]
    public void Add_ThrowsWhenCapacityExceeded()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            Span<int> buffer = stackalloc int[2];
            var list = new SpanList<int>(buffer);
            list.Add(1);
            list.Add(2);
            list.Add(3);
        });
    }

    [Fact]
    public void Add_MultipleElements_MaintainsOrder()
    {
        Span<int> buffer = stackalloc int[10];
        var list = new SpanList<int>(buffer);
        
        for (int i = 0; i < 5; i++)
        {
            list.Add(i);
        }
        
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(i, list[i]);
        }
    }

    #endregion

    #region AddRange

    [Fact]
    public void AddRange_Span_AppendsAllElements()
    {
        Span<int> buffer = stackalloc int[10];
        var list = new SpanList<int>(buffer);
        Span<int> items = stackalloc int[] { 1, 2, 3 };
        
        list.AddRange(items);
        
        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void AddRange_ReadOnlySpan_AppendsAllElements()
    {
        Span<int> buffer = stackalloc int[10];
        var list = new SpanList<int>(buffer);
        ReadOnlySpan<int> items = stackalloc int[] { 1, 2, 3 };
        
        list.AddRange(items);
        
        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void AddRange_SpanList_AppendsAllElements()
    {
        Span<int> buffer1 = stackalloc int[10];
        Span<int> buffer2 = stackalloc int[10];
        var list1 = new SpanList<int>(buffer1);
        var list2 = new SpanList<int>(buffer2);
        
        list2.Add(1);
        list2.Add(2);
        list2.Add(3);
        
        list1.AddRange(list2);
        
        Assert.Equal(3, list1.Count);
        Assert.Equal(1, list1[0]);
        Assert.Equal(2, list1[1]);
        Assert.Equal(3, list1[2]);
    }

    [Fact]
    public void AddRange_ThrowsWhenCapacityExceeded()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            Span<int> buffer = stackalloc int[2];
            var list = new SpanList<int>(buffer);
            Span<int> items = stackalloc int[] { 1, 2, 3 };
            list.AddRange(items);
        });
    }

    [Fact]
    public void AddRange_ToNonEmptyList_AppendsAfterExistingElements()
    {
        Span<int> buffer = stackalloc int[10];
        var list = new SpanList<int>(buffer);
        list.Add(0);
        
        Span<int> items = stackalloc int[] { 1, 2, 3 };
        list.AddRange(items);
        
        Assert.Equal(4, list.Count);
        Assert.Equal(0, list[0]);
        Assert.Equal(1, list[1]);
        Assert.Equal(2, list[2]);
        Assert.Equal(3, list[3]);
    }

    #endregion

    #region RemoveLast

    [Fact]
    public void RemoveLast_DecrementsCount()
    {
        Span<int> buffer = stackalloc int[10];
        buffer[0] = 1;
        buffer[1] = 2;
        var list = new SpanList<int>(buffer, 2);
        
        list.RemoveLast();
        
        Assert.Equal(1, list.Count);
    }

    [Fact]
    public void RemoveLast_ThrowsWhenListEmpty()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            Span<int> buffer = stackalloc int[10];
            var list = new SpanList<int>(buffer);
            list.RemoveLast();
        });
    }

    [Fact]
    public void RemoveLast_AllowsReadding()
    {
        Span<int> buffer = stackalloc int[10];
        buffer[0] = 1;
        var list = new SpanList<int>(buffer, 1);
        
        list.RemoveLast();
        list.Add(42);
        
        Assert.Equal(1, list.Count);
        Assert.Equal(42, list[0]);
    }

    [Fact]
    public void RemoveLast_WithCount_RemovesMultiple()
    {
        Span<int> buffer = stackalloc int[10];
        for (int i = 0; i < 5; i++) buffer[i] = i + 1;
        var list = new SpanList<int>(buffer, 5);
        
        list.RemoveLast(3);
        
        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
    }

    [Fact]
    public void RemoveLast_WithCount_ThrowsWhenNotEnoughElements()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            Span<int> buffer = stackalloc int[10];
            buffer[0] = 1;
            var list = new SpanList<int>(buffer, 1);
            list.RemoveLast(5);
        });
    }

    [Fact]
    public void RemoveLast_WithCount_AcceptsZero()
    {
        Span<int> buffer = stackalloc int[10];
        buffer[0] = 1;
        var list = new SpanList<int>(buffer, 1);
        
        list.RemoveLast(0);
        
        Assert.Equal(1, list.Count);
    }

    #endregion

    #region RemoveRegion

    [Fact]
    public void RemoveRegion_RemovesMiddleElements()
    {
        Span<int> buffer = stackalloc int[10];
        for (int i = 0; i < 5; i++) buffer[i] = i + 1;
        var list = new SpanList<int>(buffer, 5);
        
        list.RemoveRegion(1, 2);
        
        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(4, list[1]);
        Assert.Equal(5, list[2]);
    }

    [Fact]
    public void RemoveRegion_RemovesFromStart()
    {
        Span<int> buffer = stackalloc int[10];
        for (int i = 0; i < 5; i++) buffer[i] = i + 1;
        var list = new SpanList<int>(buffer, 5);
        
        list.RemoveRegion(0, 2);
        
        Assert.Equal(3, list.Count);
        Assert.Equal(3, list[0]);
        Assert.Equal(4, list[1]);
        Assert.Equal(5, list[2]);
    }

    [Fact]
    public void RemoveRegion_RemovesFromEnd()
    {
        Span<int> buffer = stackalloc int[10];
        for (int i = 0; i < 5; i++) buffer[i] = i + 1;
        var list = new SpanList<int>(buffer, 5);
        
        list.RemoveRegion(3, 2);
        
        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void RemoveRegion_WithRange_RemovesCorrectElements()
    {
        Span<int> buffer = stackalloc int[10];
        for (int i = 0; i < 5; i++) buffer[i] = i + 1;
        var list = new SpanList<int>(buffer, 5);
        
        list.RemoveRegion(1..3);
        
        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(4, list[1]);
        Assert.Equal(5, list[2]);
    }

    #endregion

    #region Clear

    [Fact]
    public void Clear_ResetsCountToZero()
    {
        Span<int> buffer = stackalloc int[10];
        for (int i = 0; i < 5; i++) buffer[i] = i + 1;
        var list = new SpanList<int>(buffer, 5);
        
        list.Clear();
        
        Assert.Equal(0, list.Count);
    }

    [Fact]
    public void Clear_AllowsReuse()
    {
        Span<int> buffer = stackalloc int[10];
        for (int i = 0; i < 5; i++) buffer[i] = i + 1;
        var list = new SpanList<int>(buffer, 5);
        
        list.Clear();
        list.Add(42);
        
        Assert.Equal(1, list.Count);
        Assert.Equal(42, list[0]);
    }

    #endregion

    #region AsSpan and AsReadOnlySpan

    [Fact]
    public void AsSpan_ReturnsCorrectElements()
    {
        Span<int> buffer = stackalloc int[10];
        for (int i = 0; i < 5; i++) buffer[i] = i + 1;
        var list = new SpanList<int>(buffer, 5);
        
        var span = list.AsSpan();
        
        Assert.Equal(5, span.Length);
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(i + 1, span[i]);
        }
    }

    [Fact]
    public void AsReadOnlySpan_ReturnsCorrectElements()
    {
        Span<int> buffer = stackalloc int[10];
        for (int i = 0; i < 5; i++) buffer[i] = i + 1;
        var list = new SpanList<int>(buffer, 5);
        
        var span = list.AsReadOnlySpan();
        
        Assert.Equal(5, span.Length);
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(i + 1, span[i]);
        }
    }

    [Fact]
    public void AsSpan_EmptyList_ReturnsEmptySpan()
    {
        Span<int> buffer = stackalloc int[10];
        var list = new SpanList<int>(buffer);
        
        var span = list.AsSpan();
        
        Assert.Equal(0, span.Length);
    }

    #endregion

    #region ToArray

    [Fact]
    public void ToArray_ReturnsCorrectArray()
    {
        Span<int> buffer = stackalloc int[10];
        for (int i = 0; i < 5; i++) buffer[i] = i + 1;
        var list = new SpanList<int>(buffer, 5);
        
        var array = list.ToArray();
        
        Assert.Equal(5, array.Length);
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(i + 1, array[i]);
        }
    }

    [Fact]
    public void ToArray_EmptyList_ReturnsEmptyArray()
    {
        Span<int> buffer = stackalloc int[10];
        var list = new SpanList<int>(buffer);
        
        var array = list.ToArray();
        
        Assert.Empty(array);
    }

    [Fact]
    public void ToArray_ReturnsIndependentCopy()
    {
        Span<int> buffer = stackalloc int[10];
        buffer[0] = 1;
        var list = new SpanList<int>(buffer, 1);
        
        var array1 = list.ToArray();
        var array2 = list.ToArray();
        
        Assert.NotSame(array1, array2);
    }

    #endregion

    #region Generic Type Tests

    [Fact]
    public void SpanList_WithLong_WorksCorrectly()
    {
        Span<long> buffer = stackalloc long[10];
        var list = new SpanList<long>(buffer);
        
        list.Add(long.MaxValue);
        list.Add(long.MinValue);
        
        Assert.Equal(2, list.Count);
        Assert.Equal(long.MaxValue, list[0]);
        Assert.Equal(long.MinValue, list[1]);
    }

    [Fact]
    public void SpanList_WithRangeOfInt_WorksCorrectly()
    {
        Span<Range<int>> buffer = stackalloc Range<int>[10];
        var list = new SpanList<Range<int>>(buffer);
        
        list.Add(new Range<int>(1, 10));
        list.Add(new Range<int>(20, 30));
        
        Assert.Equal(2, list.Count);
        Assert.Equal(new Range<int>(1, 10), list[0]);
        Assert.Equal(new Range<int>(20, 30), list[1]);
    }

    #endregion
}