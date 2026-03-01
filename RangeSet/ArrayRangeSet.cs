using CommunityToolkit.HighPerformance.Buffers;

using System.Numerics;
using System.Text;

namespace RangeSet;

public class ArrayRangeSet<T>
    where T : unmanaged,
    IEquatable<T>,
    IComparable<T>,
    IMinMaxValue<T>,
    IIncrementOperators<T>,
    IDecrementOperators<T>
{
    private readonly Range<T>[] _items; // sorted by First, elements not overlapping, elements non-adjacent (disjoint)
    private readonly int _length;

    public ReadOnlySpan<Range<T>> ToReadOnlySpan() => this._items.AsSpan()[.._length];

    public int RangesCount => _length;

    public ArrayRangeSet()
    {
        this._items = [];
        this._length = 0;
    }

    public ArrayRangeSet(ArrayRangeSet<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        
        this._items = new Range<T>[other.RangesCount];
        other.ToReadOnlySpan().CopyTo(this._items);
        this._length = other.RangesCount;
    }

    private ArrayRangeSet(Range<T>[] normalizedItems, int length)
    {
        this._items = normalizedItems;
        this._length = length;
    }
    
    public ArrayRangeSet(Range<T>[] items)
    {
        ArgumentNullException.ThrowIfNull(items);
        
        this._items = new Range<T>[items.Length];
        items.AsSpan().CopyTo(this._items);
        this._length = RangeOperations.NormalizeUnsorted(this._items);
    }

    public ArrayRangeSet<T> Union(ArrayRangeSet<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        Range<T>[] result = new Range<T>[this._length + other._length];
        int length = RangeOperations.UnionNormalizedNormalized(
            this.ToReadOnlySpan(),
            other.ToReadOnlySpan(),
            result);
        
        return new ArrayRangeSet<T>(result, length);
    }

    public ArrayRangeSet<T> Union(scoped ReadOnlySpan<Range<T>> other)
    {
        using SpanOwner<Range<T>> tempSpanOwner = SpanOwner<Range<T>>.Allocate(other.Length);
        Span<Range<T>> tempSpan = tempSpanOwner.Span;

        other.CopyTo(tempSpan);
        int tempSpanLength = RangeOperations.NormalizeUnsorted(tempSpan);

        Range<T>[] result = new Range<T>[this._length + tempSpanLength];
        int length = RangeOperations.UnionNormalizedNormalized(
            this.ToReadOnlySpan(),
            tempSpan[..tempSpanLength],
            result);
        return new ArrayRangeSet<T>(result, length);
    }

    public ArrayRangeSet<T> Except(ArrayRangeSet<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        Range<T>[] result = new Range<T>[this._length + other._length];
        int length = RangeOperations.ExceptNormalizedSorted(
            this.ToReadOnlySpan(),
            other.ToReadOnlySpan(),
            result);
        
        return new ArrayRangeSet<T>(result, length);
    }

    public ArrayRangeSet<T> Except(scoped ReadOnlySpan<Range<T>> other)
    {
        using SpanOwner<Range<T>> tempSpanOwner = SpanOwner<Range<T>>.Allocate(other.Length);
        Span<Range<T>> tempSpan = tempSpanOwner.Span;

        other.CopyTo(tempSpan);
        int tempSpanLength = RangeOperations.NormalizeUnsorted(tempSpan);

        Range<T>[] result = new Range<T>[this._length + tempSpanLength];
        int length = RangeOperations.ExceptNormalizedSorted(
            this.ToReadOnlySpan(),
            tempSpan[..tempSpanLength],
            result);
        return new ArrayRangeSet<T>(result, length);
    }

    public ArrayRangeSet<T> Intersect(ArrayRangeSet<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (this._length == 0 || other._length == 0)
        {
            return new ArrayRangeSet<T>();
        }

        Range<T>[] result = new Range<T>[this._length + other._length - 1];
        int length = RangeOperations.IntersectNormalizedNormalized(
            this.ToReadOnlySpan(),
            other._items,
            result);
        return new ArrayRangeSet<T>(result, length);
    }

    public ArrayRangeSet<T> Intersect(scoped ReadOnlySpan<Range<T>> other)
    {
        if (this._length == 0 || other.Length == 0)
        {
            return new ArrayRangeSet<T>();
        }

        using SpanOwner<Range<T>> tempSpanOwner = SpanOwner<Range<T>>.Allocate(other.Length);
        Span<Range<T>> tempSpan = tempSpanOwner.Span;

        other.CopyTo(tempSpan);
        int tempSpanLength = RangeOperations.NormalizeUnsorted(tempSpan);

        Range<T>[] result = new Range<T>[this._items.Length + tempSpanLength - 1];
        int length = RangeOperations.IntersectNormalizedNormalized(
            this.ToReadOnlySpan(),
            tempSpan[..tempSpanLength],
            result);
        return new ArrayRangeSet<T>(result, length);
    }

    public Range<T>[] ToArray()
    {
        Range<T>[] result = new Range<T>[this._length];
        this.ToReadOnlySpan().CopyTo(result);
        return result;
    }

    public override string ToString()
    {
        StringBuilder result = new();
        for (int index = 0; index < this._length; index++)
        {
            Range<T> item = this._items[index];
            result.AppendLine(item.ToString());
        }

        return result.ToString();
    }
}