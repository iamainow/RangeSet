using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace RangeSet.Benchmarks;

public class BenchmarkManualConfig : ManualConfig
{
    public BenchmarkManualConfig()
    {
        AddJob(Job.Default
            .DontEnforcePowerPlan()
            .WithRuntime(CoreRuntime.Core10_0))
            .AddDiagnoser(new BenchmarkDotNet.Diagnosers.MemoryDiagnoser(new BenchmarkDotNet.Diagnosers.MemoryDiagnoserConfig()))
            .AddDiagnoser(new BenchmarkDotNet.Diagnosers.ExceptionDiagnoser(new BenchmarkDotNet.Attributes.ExceptionDiagnoserConfig(false)));

        //AddJob(Job.Default
        //    .DontEnforcePowerPlan()
        //    .WithRuntime(NativeAotRuntime.Net10_0));
    }
}