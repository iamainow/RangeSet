using System.Runtime.InteropServices;
using System.Text;

namespace RangeSet;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Range<T> : IEquatable<Range<T>>
    where T : struct, IEquatable<T>, IComparable<T>
{
    private readonly T first;
    private readonly T last;

    public T First => this.first;
    public T Last => this.last;

    public Range(T first, T last)
    {
        if (first.CompareTo(last) > 0)
        {
            throw new ArgumentException($"{nameof(first)} must be less than or equal to {nameof(last)}");
        }
        this.first = first;
        this.last = last;
    }
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(this.first.ToString());
        sb.Append(" - ");
        sb.Append(this.last.ToString());
        return sb.ToString();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Range<T> other)
        {
            return false;
        }
        return Equals(other);
    }

    public bool Equals(Range<T> other)
    {
        return this.first.Equals(other.first) && this.last.Equals(other.last);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.first, this.last);
    }

    public static bool operator ==(Range<T> left, Range<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Range<T> left, Range<T> right)
    {
        return !left.Equals(right);
    }
}
