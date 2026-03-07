namespace RangeSet.Tests;

using RangeSet;

internal static class TestHelpers
{
    internal static Range<int>[] CreateRangesFromPairs(int[] pairs)
    {
        var ranges = new Range<int>[pairs.Length / 2];
        for (int i = 0; i < pairs.Length / 2; i++)
        {
            ranges[i] = new(pairs[i * 2], pairs[i * 2 + 1]);
        }
        return ranges;
    }
}
