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

    public static int Convert<T>(Span<CustomRange<T>> span, T one, InputTypeGeneral fromType, InputTypeGeneral toType)
        where T : struct, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IIncrementOperators<T>
    {
        switch (fromType, toType)
        {
            case (InputTypeGeneral.Unsorted, InputTypeGeneral.Sorted):
                {
                    SpanHelperGeneric.Sort(span);
                    return span.Length;
                }
            case (InputTypeGeneral.Unsorted, InputTypeGeneral.Normalized):
                {
                    return SpanHelperGeneric.MakeNormalizedFromUnsorted(span);
                }
            case (InputTypeGeneral.Sorted, InputTypeGeneral.Normalized):
                {
                    return SpanHelperGeneric.MakeNormalizedFromSorted(span);
                }
            default: return span.Length;
        }
    }
}
