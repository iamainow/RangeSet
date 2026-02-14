
using BenchmarkDotNet.Attributes;

namespace RangeSet.Benchmarks.SpanHelperGenericBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class SpanHelperGeneric_UInt32_UnaryOperations
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000, 10_000)]
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

    private Range<uint>[][] rangesArray = [];

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
            .Select(_ => generator().Select(x => new Range<uint>(x.First, x.Last)).ToArray())
            .ToArray();
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
        Random random = new(42);
        this.rangesArray = Generate(Count, SetSize, Input, random);
    }

    [Benchmark]
    public int SpanHelperGeneric_UInt32_MakeNormalizedFromUnsorted()
    {
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            result += RangeOperations.MakeNormalizedFromUnsorted(this.rangesArray[index]);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelperGeneric_UInt32_MakeNormalizedFromUnsorted2()
    {
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            result += RangeOperations.MakeNormalizedFromUnsorted(this.rangesArray[index]);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelperGeneric_UInt32_Sort()
    {
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            RangeOperations.Sort(this.rangesArray[index]);
            result ^= this.rangesArray[index][0].GetHashCode(); // Prevent optimization
        }

        return result;
    }
}