using CommunityToolkit.HighPerformance.Buffers;

using System.Numerics;
using System.Text;

namespace RangeSet;

public static class SortedRangeSet
{
    public static SortedRangeSet<T> Create<T>(scoped ReadOnlySpan<Range<T>> other)
        where T : unmanaged, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IIncrementOperators<T>, IDecrementOperators<T>
    {
        Span<Range<T>> resultBuffer = new Range<T>[other.Length];
        other.CopyTo(resultBuffer);
        int length = RangeOperations.NormalizeUnsorted(resultBuffer);
        return new SortedRangeSet<T>(resultBuffer[..length]);
    }
}

public readonly ref struct SortedRangeSet<T>
    where T : unmanaged, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IIncrementOperators<T>, IDecrementOperators<T>
{
    private readonly ReadOnlySpan<Range<T>> _items; // sorted by First, elements not overlapping, elements non-adjacent (disjoint)

    public readonly ReadOnlySpan<Range<T>> ToReadOnlySpan() => this._items;

    public readonly int RangesCount => this._items.Length;

    public SortedRangeSet()
    {
        this._items = ReadOnlySpan<Range<T>>.Empty;
    }

    public SortedRangeSet(scoped SortedRangeSet<T> other)
    {
        Span<Range<T>> resultBuffer = new Range<T>[other._items.Length];
        other._items.CopyTo(resultBuffer);
        this._items = resultBuffer;
    }

    internal SortedRangeSet(ReadOnlySpan<Range<T>> normalizedItems)
    {
        this._items = normalizedItems;
    }

    public readonly SortedRangeSet<T> Union(scoped SortedRangeSet<T> other)
    {
        Span<Range<T>> resultBuffer = new Range<T>[this._items.Length + other._items.Length];
        int length = RangeOperations.UnionNormalizedNormalized<T>(this._items, other._items, resultBuffer);
        return new SortedRangeSet<T>(resultBuffer[..length]);
    }

    public readonly SortedRangeSet<T> Union(scoped ReadOnlySpan<Range<T>> other)
    {
        using SpanOwner<Range<T>> otherSpanOwner = SpanOwner<Range<T>>.Allocate(other.Length);
        Span<Range<T>> otherSpan = otherSpanOwner.Span;

        other.CopyTo(otherSpan);
        int otherSpanLength = RangeOperations.NormalizeUnsorted(otherSpan);

        Span<Range<T>> resultBuffer = new Range<T>[this._items.Length + otherSpanLength];
        int length = RangeOperations.UnionNormalizedNormalized(this._items, otherSpan[..otherSpanLength], resultBuffer);
        return new SortedRangeSet<T>(resultBuffer[..length]);
    }

    public SortedRangeSet<T> Except(scoped SortedRangeSet<T> other)
    {
        Span<Range<T>> resultBuffer = new Range<T>[this._items.Length + other._items.Length];
        int length = RangeOperations.ExceptNormalizedSorted(this._items, other._items, resultBuffer);
        return new SortedRangeSet<T>(resultBuffer[..length]);
    }

    public SortedRangeSet<T> Except(scoped ReadOnlySpan<Range<T>> other)
    {
        using SpanOwner<Range<T>> otherSpanOwner = SpanOwner<Range<T>>.Allocate(other.Length);
        Span<Range<T>> otherSpan = otherSpanOwner.Span;

        other.CopyTo(otherSpan);
        RangeOperations.Sort(otherSpan);

        Span<Range<T>> resultBuffer = new Range<T>[this._items.Length + otherSpan.Length];
        int length = RangeOperations.ExceptNormalizedSorted(this._items, otherSpan, resultBuffer);
        return new SortedRangeSet<T>(resultBuffer[..length]);
    }

    public SortedRangeSet<T> Intersect(scoped SortedRangeSet<T> other)
    {
        if (this._items.Length == 0 || other._items.Length == 0)
        {
            return new SortedRangeSet<T>();
        }

        Span<Range<T>> resultBuffer = new Range<T>[this._items.Length + other._items.Length - 1];
        int length = RangeOperations.IntersectNormalizedNormalized(this._items, other._items, resultBuffer);
        return new SortedRangeSet<T>(resultBuffer[..length]);
    }

    public SortedRangeSet<T> Intersect(scoped ReadOnlySpan<Range<T>> other)
    {
        if (this._items.Length == 0 || other.Length == 0)
        {
            return new SortedRangeSet<T>();
        }

        using SpanOwner<Range<T>> otherSpanOwner = SpanOwner<Range<T>>.Allocate(other.Length);
        Span<Range<T>> otherSpan = otherSpanOwner.Span;

        other.CopyTo(otherSpan);
        int otherSpanLength = RangeOperations.NormalizeUnsorted(otherSpan);
        otherSpan = otherSpan[..otherSpanLength];

        Span<Range<T>> resultBuffer = new Range<T>[this._items.Length + otherSpan.Length - 1];
        int length = RangeOperations.IntersectNormalizedNormalized(this._items, otherSpan, resultBuffer);
        return new SortedRangeSet<T>(resultBuffer[..length]);
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