namespace RangeSet.Benchmarks;

public static class CustomArrayExtensions
{
    public static unsafe CustomRange<T>[] GenerateNormalized<T>(int size, Func<ReadOnlySpan<byte>, T> convert, Random random)
        where T : unmanaged, IEquatable<T>, IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(convert);
        ArgumentNullException.ThrowIfNull(random);

        SortedSet<T> addresses = new();
        Span<byte> buffer = new byte[sizeof(T)];

        while (addresses.Count < size * 2)
        {
            random.NextBytes(buffer);
            addresses.Add(convert(buffer));
        }

        return addresses.Chunk(2).Select(x => new CustomRange<T>(x[0], x[1])).ToArray();
    }

    public static unsafe CustomRange<T>[] GenerateSorted<T>(int size, Func<ReadOnlySpan<byte>, T> convert, double overlappingPercent, Random random)
        where T : unmanaged, IEquatable<T>, IComparable<T>
    {
        var result = GenerateNormalized(size, convert, random);

        MakeOverlapping(result, overlappingPercent, random);

        return result;
    }

    public static unsafe CustomRange<T>[] GenerateUnsorted<T>(int size, Func<ReadOnlySpan<byte>, T> convert, double overlappingPercent, Random random)
        where T : unmanaged, IEquatable<T>, IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(random);

        var result = GenerateSorted(size, convert, overlappingPercent, random);

        random.Shuffle(result);

        return result;
    }

    private static void MakeOverlapping<T>(Span<CustomRange<T>> sortedArray, double overlappingPercent, Random random)
        where T : struct, IEquatable<T>, IComparable<T>
    {
        for (int i = 0; i < sortedArray.Length - 1; ++i)
        {
            if (random.NextDouble() < overlappingPercent)
            {
                var t1 = sortedArray[i].Last;
                var t2 = sortedArray[i + 1].First;

                sortedArray[i] = new CustomRange<T>(sortedArray[i].First, t2);
                sortedArray[i + 1] = new CustomRange<T>(t1, sortedArray[i + 1].Last);
            }
        }
    }
}