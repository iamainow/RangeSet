using System.Buffers;

using CommunityToolkit.HighPerformance.Buffers;

using System.Numerics;
using System.Text;

namespace RangeSet;

public static class SpanRangeSet
{
    public static int CalculateUnionSize(int length1, int length2) => length1 + length2;

    public static int CalculateExceptSize(int length1, int length2) => length1 + length2;

    public static int CalculateIntersectSize(int length1, int length2) =>
        length1 == 0 || length2 == 0 ? 0 : length1 + length2 - 1;
}

public readonly ref struct SpanRangeSet<T>
    where T : unmanaged, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IIncrementOperators<T>, IDecrementOperators<T>
{
    // sorted by First, elements not overlapping, elements non-adjacent (disjoint)
    private readonly ReadOnlySpan<Range<T>> _items;

    public ReadOnlySpan<Range<T>> ToReadOnlySpan() => this._items;

    public int RangesCount => this._items.Length;

    public SpanRangeSet()
    {
        this._items = ReadOnlySpan<Range<T>>.Empty;
    }
    
    public SpanRangeSet(scoped SpanRangeSet<T> other, Span<Range<T>> internalBuffer)
    {
        other._items.CopyTo(internalBuffer);
        this._items = internalBuffer;
    }
    
    public SpanRangeSet(SpanRangeSet<T> other)
    {
        this._items = other._items;
    }

    private SpanRangeSet(ReadOnlySpan<Range<T>> normalizedItems)
    {
        this._items = normalizedItems;
    }
    
    public SpanRangeSet(Span<Range<T>> items)
    {
        int length = RangeOperations.NormalizeUnsorted(items);
        this._items = items[..length];
    }

    public SpanRangeSet<T> Union(scoped SpanRangeSet<T> other, Span<Range<T>> resultBuffer)
    {
        int length = RangeOperations.UnionNormalizedNormalized(this._items, other._items, resultBuffer);
        return new SpanRangeSet<T>((ReadOnlySpan<Range<T>>)resultBuffer[..length]);
    }

    public SpanRangeSet<T> Union(scoped ReadOnlySpan<Range<T>> other, Span<Range<T>> resultBuffer)
    {
        using SpanOwner<Range<T>> tempSpanOwner = SpanOwner<Range<T>>.Allocate(other.Length);
        Span<Range<T>> tempSpan = tempSpanOwner.Span;

        other.CopyTo(tempSpan);
        int otherSpanLength = RangeOperations.NormalizeUnsorted(tempSpan);

        int length = RangeOperations.UnionNormalizedNormalized(this._items, tempSpan[..otherSpanLength], resultBuffer);
        return new SpanRangeSet<T>((ReadOnlySpan<Range<T>>)resultBuffer[..length]);
    }

    public SpanRangeSet<T> Except(scoped SpanRangeSet<T> other)
    {
        Span<Range<T>> resultBuffer = new Range<T>[this._items.Length + other._items.Length];
        int length = RangeOperations.ExceptNormalizedSorted(this._items, other._items, resultBuffer);
        return new SpanRangeSet<T>((ReadOnlySpan<Range<T>>)resultBuffer[..length]);
    }

    public SpanRangeSet<T> Except(scoped ReadOnlySpan<Range<T>> other)
    {
        using SpanOwner<Range<T>> otherSpanOwner = SpanOwner<Range<T>>.Allocate(other.Length);
        Span<Range<T>> otherSpan = otherSpanOwner.Span;

        other.CopyTo(otherSpan);
        RangeOperations.Sort(otherSpan);

        Span<Range<T>> resultBuffer = new Range<T>[this._items.Length + otherSpan.Length];
        int length = RangeOperations.ExceptNormalizedSorted(this._items, otherSpan, resultBuffer);
        return new SpanRangeSet<T>((ReadOnlySpan<Range<T>>)resultBuffer[..length]);
    }

    public SpanRangeSet<T> Intersect(scoped SpanRangeSet<T> other)
    {
        if (this._items.Length == 0 || other._items.Length == 0)
        {
            return new SpanRangeSet<T>();
        }

        Span<Range<T>> resultBuffer = new Range<T>[this._items.Length + other._items.Length - 1];
        int length = RangeOperations.IntersectNormalizedNormalized(this._items, other._items, resultBuffer);
        return new SpanRangeSet<T>((ReadOnlySpan<Range<T>>)resultBuffer[..length]);
    }

    public SpanRangeSet<T> Intersect(scoped ReadOnlySpan<Range<T>> other)
    {
        if (this._items.Length == 0 || other.Length == 0)
        {
            return new SpanRangeSet<T>();
        }

        using SpanOwner<Range<T>> otherSpanOwner = SpanOwner<Range<T>>.Allocate(other.Length);
        Span<Range<T>> otherSpan = otherSpanOwner.Span;

        other.CopyTo(otherSpan);
        int otherSpanLength = RangeOperations.NormalizeUnsorted(otherSpan);
        otherSpan = otherSpan[..otherSpanLength];

        Span<Range<T>> resultBuffer = new Range<T>[this._items.Length + otherSpan.Length - 1];
        int length = RangeOperations.IntersectNormalizedNormalized(this._items, otherSpan, resultBuffer);
        return new SpanRangeSet<T>((ReadOnlySpan<Range<T>>)resultBuffer[..length]);
    }

    public Range<T>[] ToArray()
    {
        Range<T>[] result = new Range<T>[this._items.Length];
        this._items.CopyTo(result);
        return result;
    }

    public override string ToString()
    {
        StringBuilder result = new();
        foreach (Range<T> item in this._items)
        {
            result.AppendLine(item.ToString());
        }

        return result.ToString();
    }
}