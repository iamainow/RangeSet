using BenchmarkDotNet.Attributes;
using CommunityToolkit.HighPerformance.Buffers;

namespace RangeSet.Benchmarks.SpanHelperGenericBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class SpanHelperGeneric_UInt32_BinaryOperations
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    [Params(
        InputType.Normalized,
        InputType.Sorted_Overlapping_10,
        InputType.Sorted_Overlapping_20,
        InputType.Unsorted_Overlapping_0,
        InputType.Unsorted_Overlapping_10,
        InputType.Unsorted_Overlapping_20)]
    public required InputType Input { get; set; }

    public InputTypeGeneral InputGeneral => InputTypeParser.Parse(Input).Item1;

    private Range<uint>[][] rangesArray_1 = [];
    private Range<uint>[][] rangesArray_2 = [];

    private static Range<uint>[][] Generate(int count, int size, InputType input, Random random)
    {
        Func<ReadOnlySpan<byte>, uint> convert = BitConverter.ToUInt32;

        Func<Range<uint>[]> generator = InputTypeParser.Parse(input) switch
        {
            (InputTypeGeneral.Normalized, _) => () => CustomArrayExtensions.GenerateNormalized(size, convert, random),
            (InputTypeGeneral.Sorted, double overlappingPercent) => () => CustomArrayExtensions.GenerateSorted(size, convert, overlappingPercent, random),
            (InputTypeGeneral.Unsorted, double overlappingPercent) => () => CustomArrayExtensions.GenerateUnsorted(size, convert, overlappingPercent, random),
            _ => throw new NotImplementedException($"Input='{input}' is not implemented"),
        };

        return Enumerable.Range(0, count)
            .Select(_ => generator().ToArray())
            .ToArray();
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
        Random random = new();
        this.rangesArray_1 = Generate(Count, SetSize, Input, random);
        this.rangesArray_2 = Generate(Count, SetSize, Input, random);
    }

    [Benchmark]
    public int SpanHelperGeneric_UInt32_UnionNormalizedNormalized()
    {
        using var bufferSpanOwner = SpanOwner<Range<uint>>.Allocate(this.SetSize * 2);
        var buffer = bufferSpanOwner.Span;
        int result = 0;
        var fromType = InputGeneral;
        for (int index = 0; index < this.Count; ++index)
        {
            Span<Range<uint>> span1 = this.rangesArray_1[index];
            Span<Range<uint>> span2 = this.rangesArray_2[index];
            int length1 = InputTypeParser.Convert(span1, fromType, InputTypeGeneral.Normalized);
            int length2 = InputTypeParser.Convert(span2, fromType, InputTypeGeneral.Normalized);
            result += RangeOperations.UnionNormalizedNormalized(
                span1[..length1],
                span2[..length2],
                buffer);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelperGeneric_UInt32_ExceptNormalizedSorted()
    {
        using var bufferSpanOwner = SpanOwner<Range<uint>>.Allocate(this.SetSize * 2);
        var buffer = bufferSpanOwner.Span;
        int result = 0;
        var fromType = InputGeneral;
        for (int index = 0; index < this.Count; ++index)
        {
            Span<Range<uint>> span1 = this.rangesArray_1[index];
            Span<Range<uint>> span2 = this.rangesArray_2[index];
            int length1 = InputTypeParser.Convert(span1, fromType, InputTypeGeneral.Normalized);
            int length2 = InputTypeParser.Convert(span2, fromType, InputTypeGeneral.Sorted);
            result += RangeOperations.ExceptNormalizedSorted(
                span1[..length1],
                span2[..length2],
                buffer);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelperGeneric_UInt32_ExceptNormalizedNormalized()
    {
        using var bufferSpanOwner = SpanOwner<Range<uint>>.Allocate(this.SetSize * 2);
        var buffer = bufferSpanOwner.Span;
        int result = 0;
        var fromType = InputGeneral;
        for (int index = 0; index < this.Count; ++index)
        {
            Span<Range<uint>> span1 = this.rangesArray_1[index];
            Span<Range<uint>> span2 = this.rangesArray_2[index];
            int length1 = InputTypeParser.Convert(span1, fromType, InputTypeGeneral.Normalized);
            int length2 = InputTypeParser.Convert(span2, fromType, InputTypeGeneral.Normalized);
            result += RangeOperations.ExceptNormalizedSorted(
                span1[..length1],
                span2[..length2],
                buffer);
        }

        return result;
    }
}
