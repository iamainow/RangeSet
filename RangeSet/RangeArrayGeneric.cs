using CommunityToolkit.HighPerformance.Buffers;

using System.Numerics;
using System.Text;

namespace RangeSet;

public readonly ref struct RangeArrayGeneric<T>
    where T : unmanaged, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IIncrementOperators<T>
{
#pragma warning disable CA1000 // Do not declare static members on generic types
    public static RangeArrayGeneric<T> Create(scoped ReadOnlySpan<CustomRange<T>> other)
#pragma warning restore CA1000 // Do not declare static members on generic types
    {
        Span<CustomRange<T>> resultBuffer = new CustomRange<T>[other.Length];
        other.CopyTo(resultBuffer);
        int length = SpanHelperGeneric.MakeNormalizedFromUnsorted(resultBuffer);
        return new RangeArrayGeneric<T>(resultBuffer[..length], 0);
    }

    private readonly ReadOnlySpan<CustomRange<T>> _items; // sorted by First, elements not overlapping, elements non-adjacent (disjoint)

    public readonly ReadOnlySpan<CustomRange<T>> ToReadOnlySpan() => this._items;

    public readonly int RangesCount => this._items.Length;

    public RangeArrayGeneric()
    {
        this._items = ReadOnlySpan<CustomRange<T>>.Empty;
    }

    public RangeArrayGeneric(scoped RangeArrayGeneric<T> other)
    {
        Span<CustomRange<T>> resultBuffer = new CustomRange<T>[other._items.Length];
        other._items.CopyTo(resultBuffer);
        this._items = resultBuffer;
    }

    private RangeArrayGeneric(ReadOnlySpan<CustomRange<T>> normalizedItems, int i)
    {
        this._items = normalizedItems;
    }

    public readonly RangeArrayGeneric<T> Union(scoped RangeArrayGeneric<T> other, T one)
    {
        Span<CustomRange<T>> resultBuffer = new CustomRange<T>[this._items.Length + other._items.Length];
        int length = SpanHelperGeneric.UnionNormalizedNormalized<T>(this._items, other._items, resultBuffer, one);
        return new RangeArrayGeneric<T>(resultBuffer[..length], 0);
    }

    public readonly RangeArrayGeneric<T> Union(scoped ReadOnlySpan<CustomRange<T>> other, T one)
    {
        using SpanOwner<CustomRange<T>> otherSpanOwner = SpanOwner<CustomRange<T>>.Allocate(other.Length);
        Span<CustomRange<T>> otherSpan = otherSpanOwner.Span;

        other.CopyTo(otherSpan);
        int otherSpanLength = SpanHelperGeneric.MakeNormalizedFromUnsorted(otherSpan);

        Span<CustomRange<T>> resultBuffer = new CustomRange<T>[this._items.Length + otherSpanLength];
        int length = SpanHelperGeneric.UnionNormalizedNormalized(this._items, otherSpan[..otherSpanLength], resultBuffer, one);
        return new RangeArrayGeneric<T>(resultBuffer[..length], 0);
    }

    public RangeArrayGeneric<T> Except(scoped RangeArrayGeneric<T> other, T one)
    {
        Span<CustomRange<T>> resultBuffer = new CustomRange<T>[this._items.Length + other._items.Length];
        int length = SpanHelperGeneric.ExceptNormalizedSorted(this._items, other._items, resultBuffer, one);
        return new RangeArrayGeneric<T>(resultBuffer[..length], 0);
    }

    public RangeArrayGeneric<T> Except(scoped ReadOnlySpan<CustomRange<T>> other, T one)
    {
        using SpanOwner<CustomRange<T>> otherSpanOwner = SpanOwner<CustomRange<T>>.Allocate(other.Length);
        Span<CustomRange<T>> otherSpan = otherSpanOwner.Span;

        other.CopyTo(otherSpan);
        SpanHelperGeneric.Sort(otherSpan);

        Span<CustomRange<T>> resultBuffer = new CustomRange<T>[this._items.Length + otherSpan.Length];
        int length = SpanHelperGeneric.ExceptNormalizedSorted(this._items, otherSpan, resultBuffer, one);
        return new RangeArrayGeneric<T>(resultBuffer[..length], 0);
    }

    public RangeArrayGeneric<T> Intersect(scoped RangeArrayGeneric<T> other)
    {
        if (this._items.Length == 0 || other._items.Length == 0)
        {
            return new RangeArrayGeneric<T>();
        }

        Span<CustomRange<T>> resultBuffer = new CustomRange<T>[this._items.Length + other._items.Length - 1];
        int length = SpanHelperGeneric.IntersectNormalizedNormalized(this._items, other._items, resultBuffer);
        return new RangeArrayGeneric<T>(resultBuffer[..length], 0);
    }

    public RangeArrayGeneric<T> Intersect(scoped ReadOnlySpan<CustomRange<T>> other)
    {
        if (this._items.Length == 0 || other.Length == 0)
        {
            return new RangeArrayGeneric<T>();
        }

        using SpanOwner<CustomRange<T>> otherSpanOwner = SpanOwner<CustomRange<T>>.Allocate(other.Length);
        Span<CustomRange<T>> otherSpan = otherSpanOwner.Span;

        other.CopyTo(otherSpan);
        int otherSpanLength = SpanHelperGeneric.MakeNormalizedFromUnsorted(otherSpan);
        otherSpan = otherSpan[..otherSpanLength];

        Span<CustomRange<T>> resultBuffer = new CustomRange<T>[this._items.Length + otherSpan.Length - 1];
        int length = SpanHelperGeneric.IntersectNormalizedNormalized(this._items, otherSpan, resultBuffer);
        return new RangeArrayGeneric<T>(resultBuffer[..length], 0);
    }

    public CustomRange<T>[] ToArray()
    {
        CustomRange<T>[] result = new CustomRange<T>[this._items.Length];
        this._items.CopyTo(result);
        return result;
    }

    public override string ToString()
    {
        StringBuilder result = new();
        foreach (CustomRange<T> item in this._items)
        {
            result.AppendLine(item.ToString());
        }

        return result.ToString();
    }
}