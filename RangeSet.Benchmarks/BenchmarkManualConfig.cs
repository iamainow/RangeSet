using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace RangeSet.Benchmarks;

public class BenchmarkManualConfig : ManualConfig
{
    public BenchmarkManualConfig()
    {
        AddJob(Job.Default
            .DontEnforcePowerPlan()
            .WithRuntime(CoreRuntime.Core10_0)) // NativeAotRuntime.Net10_0
            .AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig()))
            .AddDiagnoser(new ExceptionDiagnoser(new ExceptionDiagnoserConfig(false)));
    }
}