using System.Numerics;

namespace RangeSet;

// unsorted - unsorted, overlapping/adjacent
// sorted - sorted but overlapping/adjacent
// normalized - sorted, not overlapped, non-adjacent
public static class SpanHelperGeneric
{
    private static T Min<T>(T item1, T item2)
        where T : IComparable<T>
    {
        return item1.CompareTo(item2) <= 0 ? item1 : item2;
    }

    private static T Max<T>(T item1, T item2)
        where T : IComparable<T>
    {
        return item1.CompareTo(item2) >= 0 ? item1 : item2;
    }

    public static int MakeNormalizedFromSorted<T>(Span<CustomRange<T>> result)
        where T : struct, IEquatable<T>, IComparable<T>, IIncrementOperators<T>
    {
        if (result.Length <= 1)
        {
            return result.Length;
        }

        var resultList = new SpanList<CustomRange<T>>(result, 1);

        for (int i = 1; i < result.Length; i++)
        {
            var current = result[i];
            ref var last = ref resultList.Last();
            // current.First >= last.First due sorting

            if (last.Last.CompareTo(current.First) >= 0)
            {
                // Overlapping - merge
                last = new CustomRange<T>(last.First, Max(last.Last, current.Last));
            }
            else
            {
                // Not overlapping, check adjacency
                // Safe to increment: last.Last < current.First <= MaxValue
                var nextAfterLast = last.Last;
                ++nextAfterLast;
                if (nextAfterLast.Equals(current.First))
                {
                    // Adjacent - merge (current.Last > last.Last since not overlapping)
                    last = new CustomRange<T>(last.First, current.Last);
                }
                else
                {
                    resultList.Add(current);
                }
            }
        }

        return resultList.Count;
    }

    public static int MakeNormalizedFromUnsorted<T>(Span<CustomRange<T>> result)
        where T : struct, IEquatable<T>, IComparable<T>, IIncrementOperators<T>
    {
        Sort(result);
        return MakeNormalizedFromSorted(result);
    }

    public static void Sort<T>(Span<CustomRange<T>> result)
        where T : struct, IEquatable<T>, IComparable<T>
    {
        result.Sort(CustomRangeComparer<T>.Instance);
    }

    public static int UnionNormalizedNormalized<T>(ReadOnlySpan<CustomRange<T>> normalized1, ReadOnlySpan<CustomRange<T>> normalized2, Span<CustomRange<T>> result)
        where T : struct, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IIncrementOperators<T>
    {
        if (result.Overlaps(normalized1))
        {
            throw new ArgumentException($"result can't overlap with sorted1", nameof(result));
        }

        if (result.Overlaps(normalized2))
        {
            throw new ArgumentException("result can't overlap with sorted2", nameof(result));
        }

        if (normalized1.Length == 0)
        {
            normalized2.CopyTo(result);
            return normalized2.Length;
        }

        if (normalized2.Length == 0)
        {
            normalized1.CopyTo(result);
            return normalized1.Length;
        }

        SpanList<CustomRange<T>> resultList = new SpanList<CustomRange<T>>(result);
        int index1 = 0;
        int index2 = 0;

        // first pass
        {
            var item1 = normalized1[0];
            var item2 = normalized2[0];
            if (item1.First.CompareTo(item2.First) <= 0)
            {
                resultList.Add(item1);
                index1++;
            }
            else
            {
                resultList.Add(item2);
                index2++;
            }
        }

        while (index1 < normalized1.Length && index2 < normalized2.Length)
        {
            CustomRange<T> current;
            var item1 = normalized1[index1];
            var item2 = normalized2[index2];
            if (item1.First.CompareTo(item2.First) <= 0)
            {
                current = item1;
                index1++;
            }
            else
            {
                current = item2;
                index2++;
            }

            ref var last = ref resultList.Last();
            if (T.MaxValue.Equals(last.Last))
            {
                last = new CustomRange<T>(last.First, T.MaxValue);
                return resultList.Count;
            }
            else
            {
                var afterLastLast = last.Last;
                ++afterLastLast;
                if (afterLastLast.CompareTo(current.First) >= 0)
                {
                    last = new CustomRange<T>(last.First, Max(last.Last, current.Last));
                }
                else
                {
                    resultList.Add(current);
                }
            }
        }

        while (index2 < normalized2.Length)
        {
            CustomRange<T> current = normalized2[index2];

            ref var last = ref resultList.Last();
            if (T.MaxValue.Equals(last.Last))
            {
                last = new CustomRange<T>(last.First, T.MaxValue);
                return resultList.Count;
            }
            else
            {
                var afterLastLast = last.Last;
                ++afterLastLast;
                if (afterLastLast.CompareTo(current.First) >= 0)
                {
                    last = new CustomRange<T>(last.First, Max(last.Last, current.Last));
                    ++index2;
                }
                else
                {
                    resultList.AddRange(normalized2[index2..]);
                    return resultList.Count;
                }
            }
        }

        while (index1 < normalized1.Length)
        {
            CustomRange<T> current = normalized1[index1];

            ref var last = ref resultList.Last();
            if (T.MaxValue.Equals(last.Last))
            {
                last = new CustomRange<T>(last.First, T.MaxValue);
                return resultList.Count;
            }
            else
            {
                var afterLastLast = last.Last;
                ++afterLastLast;
                if (afterLastLast.CompareTo(current.First) >= 0)
                {
                    last = new CustomRange<T>(last.First, Max(last.Last, current.Last));
                    ++index1;
                }
                else
                {
                    resultList.AddRange(normalized1[index1..]);
                    return resultList.Count;
                }
            }
        }

        return resultList.Count;
    }

    /// <returns>left and right parts</returns>
    private static (CustomRange<T>?, CustomRange<T>?) IntersectableExcept<T>(CustomRange<T> range, CustomRange<T> other)
        where T : struct, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IIncrementOperators<T>, IDecrementOperators<T>
    {
        bool hasLeftPart = other.First.CompareTo(range.First) > 0 && !T.MinValue.Equals(other.First);
        bool hasRightPart = other.Last.CompareTo(range.Last) < 0 && !T.MaxValue.Equals(other.Last);

        if (hasLeftPart)
        {
            var beforeOtherFirst = other.First;
            --beforeOtherFirst;
            if (hasRightPart)
            {
                var afterOtherLast = other.Last;
                ++afterOtherLast;
                return (new CustomRange<T>(range.First, beforeOtherFirst), new CustomRange<T>(afterOtherLast, range.Last));
            }
            else
            {
                return (new CustomRange<T>(range.First, beforeOtherFirst), null);
            }
        }
        else
        {
            if (hasRightPart)
            {
                var afterOtherLast = other.Last;
                ++afterOtherLast;
                return (null, new CustomRange<T>(afterOtherLast, range.Last));
            }
            else
            {
                return (null, null);
            }
        }
    }

    public static int ExceptNormalizedSorted<T>(ReadOnlySpan<CustomRange<T>> normalized, ReadOnlySpan<CustomRange<T>> sorted, Span<CustomRange<T>> result)
        where T : struct, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IIncrementOperators<T>, IDecrementOperators<T>
    {
        if (result.Overlaps(normalized))
        {
            throw new ArgumentException($"result can't overlap with normalized", nameof(result));
        }

        if (result.Overlaps(sorted))
        {
            throw new ArgumentException("result can't overlap with sorted", nameof(result));
        }

        if (normalized.Length == 0)
        {
            return 0;
        }

        if (sorted.Length == 0)
        {
            normalized.CopyTo(result);
            return normalized.Length;
        }

        SpanList<CustomRange<T>> resultList = new SpanList<CustomRange<T>>(result);

        int i = 0;
        int j = 0;
        CustomRange<T> currentRange = normalized[0];

        while (true)
        {
            if (j >= sorted.Length)
            {
                // No more exclusion ranges, add current and remaining ranges
                resultList.Add(currentRange);
                i++;
                resultList.AddRange(normalized[i..]);
                break;
            }

            var otherCurr = sorted[j];

            if (currentRange.Last.CompareTo(otherCurr.First) < 0)
            {
                // Current range is entirely before exclusion range - keep it and move to next
                resultList.Add(currentRange);
                i++;
                if (i >= normalized.Length)
                {
                    break;
                }
                currentRange = normalized[i];
            }
            else if (currentRange.First.CompareTo(otherCurr.Last) > 0)
            {
                // Current range is entirely after exclusion range - move to next exclusion
                j++;
            }
            else
            {
                // Ranges overlap - compute the difference
                (var leftPart, var rightPart) = IntersectableExcept(currentRange, otherCurr);

                if (leftPart.HasValue)
                {
                    resultList.Add(leftPart.Value);
                }

                if (rightPart.HasValue)
                {
                    currentRange = rightPart.Value;
                    j++;
                }
                else
                {
                    i++;
                    if (i >= normalized.Length)
                    {
                        break;
                    }
                    currentRange = normalized[i];
                }
            }
        }

        return resultList.Count;
    }

    public static int IntersectNormalizedNormalized<T>(ReadOnlySpan<CustomRange<T>> normalized1, ReadOnlySpan<CustomRange<T>> normalized2, Span<CustomRange<T>> result)
        where T : struct, IEquatable<T>, IComparable<T>
    {
        if (result.Overlaps(normalized1))
        {
            throw new ArgumentException($"result can't overlap with normalized1", nameof(result));
        }

        if (result.Overlaps(normalized2))
        {
            throw new ArgumentException("result can't overlap with normalized2", nameof(result));
        }

        if (normalized1.Length == 0)
        {
            return 0;
        }

        if (normalized2.Length == 0)
        {
            return 0;
        }

        int maxLength = normalized1.Length + normalized2.Length - 1;

        SpanList<CustomRange<T>> resultList = new SpanList<CustomRange<T>>(result);
        int index1 = 0;
        int index2 = 0;
        while (index1 < normalized1.Length && index2 < normalized2.Length)
        {
            var item1 = normalized1[index1];
            var item2 = normalized2[index2];
            if (item1.Last.CompareTo(item2.First) < 0)
            {
                // item1 is before item2
                index1++;
            }
            else if (item2.Last.CompareTo(item1.First) < 0)
            {
                // item2 is before item1
                index2++;
            }
            else
            {
                // Ranges overlap
                T start = Max(item1.First, item2.First);
                T end = Min(item1.Last, item2.Last);
                resultList.Add(new CustomRange<T>(start, end));

                int comparing = item1.Last.CompareTo(item2.Last);
                if (comparing <= 0)
                {
                    index1++;
                }
                if (comparing >= 0)
                {
                    index2++;
                }
            }
        }

        return resultList.Count;
    }
}