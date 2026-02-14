using RangeSet;
using System.Numerics;

namespace RangeSet.Benchmarks;

public static class InputTypeParser
{
    public static (InputTypeGeneral, double) Parse(InputType inputType)
    {
        return inputType switch
        {
            InputType.Normalized => (InputTypeGeneral.Normalized, default),
            InputType.Sorted_Overlapping_10 => (InputTypeGeneral.Sorted, 0.1),
            InputType.Sorted_Overlapping_20 => (InputTypeGeneral.Sorted, 0.2),

            InputType.Unsorted_Overlapping_0 => (InputTypeGeneral.Unsorted, 0),
            InputType.Unsorted_Overlapping_10 => (InputTypeGeneral.Unsorted, 0.1),
            InputType.Unsorted_Overlapping_20 => (InputTypeGeneral.Unsorted, 0.2),
            _ => throw new NotImplementedException(),
        };
    }

    public static int Convert<T>(Span<Range<T>> span, InputTypeGeneral fromType, InputTypeGeneral toType)
        where T : struct, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IIncrementOperators<T>
    {
        switch (fromType, toType)
        {
            case (InputTypeGeneral.Unsorted, InputTypeGeneral.Sorted):
                {
                    RangeOperations.Sort(span);
                    return span.Length;
                }
            case (InputTypeGeneral.Unsorted, InputTypeGeneral.Normalized):
                {
                    return RangeOperations.NormalizeUnsorted(span);
                }
            case (InputTypeGeneral.Sorted, InputTypeGeneral.Normalized):
                {
                    return RangeOperations.NormalizeSorted(span);
                }
            default: return span.Length;
        }
    }
}
