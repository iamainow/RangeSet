namespace RangeSet;

public ref struct SpanList<T>
{
    private Span<T> _items;
    private int _size;

    /// <summary>
    /// Initializes a new instance of the SpanList<T> class using the specified buffer as the underlying storage.
    /// </summary>
    public SpanList(Span<T> rewritableInternalBuffer)
    {
        _items = rewritableInternalBuffer;
        _size = 0;
    }

    /// <summary>
    /// Initializes a new instance of the SpanList<T> class using the specified buffer as the underlying storage with specified first count of items
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public SpanList(Span<T> rewritableInternalBuffer, int count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, rewritableInternalBuffer.Length);

        _items = rewritableInternalBuffer;
        _size = count;
    }

    /// <summary>
    /// Initializes a new instance of the SpanList<T> class using the specified buffer as the underlying storage and copies the elements into it.
    /// </summary>
    public SpanList(Span<T> rewritableInternalBuffer, ReadOnlySpan<T> elements)
    {
        elements.CopyTo(rewritableInternalBuffer);
        _items = rewritableInternalBuffer;
        _size = elements.Length;
    }

    public readonly int Count => _size;

    public readonly int Capacity => _items.Length;

    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public readonly ref T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _size);

            return ref _items[index];
        }
    }

    public ref T Last()
    {
        return ref this[_size - 1];
    }

    public readonly ReadOnlySpan<T> this[Range range]
    {
        get
        {
            return AsReadOnlySpan()[range];
        }
    }

    /// <exception cref="InvalidOperationException"></exception>
    public void Add(T item)
    {
        if (Count + 1 > Capacity)
        {
            throw new InvalidOperationException("Capacity exceeded");
        }
        _items[_size++] = item;
    }

    /// <exception cref="InvalidOperationException"></exception>
    public void AddRange(scoped Span<T> items)
    {
        if (Count + items.Length > Capacity)
        {
            throw new InvalidOperationException("Capacity exceeded");
        }
        items.CopyTo(_items[Count..]);
        _size += items.Length;
    }

    /// <exception cref="InvalidOperationException"></exception>
    public void AddRange(scoped ReadOnlySpan<T> items)
    {
        if (Count + items.Length > Capacity)
        {
            throw new InvalidOperationException("Capacity exceeded");
        }
        items.CopyTo(_items[Count..]);
        _size += items.Length;
    }

    /// <exception cref="InvalidOperationException"></exception>
    public void AddRange(scoped SpanList<T> items)
    {
        AddRange(items.AsReadOnlySpan());
    }

    /// <exception cref="InvalidOperationException"></exception>
    public void RemoveLast()
    {
        if (Count < 1)
        {
            throw new InvalidOperationException("No items to remove");
        }
        --_size;
    }

    /// <exception cref="InvalidOperationException"></exception>
    public void RemoveLast(int count)
    {
        if (Count < count)
        {
            throw new InvalidOperationException("Not enough items to remove");
        }
        _size -= count;
    }

    /// <exception cref="ArgumentException"></exception>
    public void RemoveRegion(int start, int count)
    {
        AsReadOnlySpan()[(start + count)..].CopyTo(_items[start..]);
        _size -= count;
    }

    public void RemoveRegion(Range range)
    {
        (int start, int count) = range.GetOffsetAndLength(Count);
        RemoveRegion(start, count);
    }

    public void Clear()
    {
        _size = 0;
    }

    public readonly Span<T> AsSpan()
    {
        return _items[.._size];
    }

    public readonly ReadOnlySpan<T> AsReadOnlySpan()
    {
        return _items[.._size];
    }

    public readonly T[] ToArray()
    {
        return AsSpan().ToArray();
    }
}