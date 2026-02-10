namespace RangeSet;

internal sealed class CustomRangeComparer<T> : IComparer<CustomRange<T>>
    where T : struct, IEquatable<T>, IComparable<T>
{
    public static readonly CustomRangeComparer<T> Instance = new();

    private CustomRangeComparer() { }

    public int Compare(CustomRange<T> x, CustomRange<T> y)
    {
        return x.First.CompareTo(y.First);
    }
}