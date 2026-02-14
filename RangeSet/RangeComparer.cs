namespace RangeSet;

internal sealed class RangeComparer<T> : IComparer<Range<T>>
    where T : struct, IEquatable<T>, IComparable<T>
{
    public static readonly RangeComparer<T> Instance = new();

    private RangeComparer() { }

    public int Compare(Range<T> x, Range<T> y)
    {
        return x.First.CompareTo(y.First);
    }
}