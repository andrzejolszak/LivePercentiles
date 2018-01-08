using System;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace LivePercentiles.Benchmarks
{
    public class Runner
    {
        public static void Main(string[] args)
        {
            Console.SetWindowSize(140, 40);

            Type[] benchmarks = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public).Any(m => m.GetCustomAttributes(typeof(BenchmarkAttribute), false).Any()))
                .OrderBy(t => t.Namespace)
                .ThenBy(t => t.Name)
                .ToArray();

            BenchmarkSwitcher benchmarkSwitcher = new BenchmarkSwitcher(benchmarks);
            benchmarkSwitcher.Run();
            Console.ReadLine();
        }
    }

    public class DefaultConfig : ManualConfig
    {
        public DefaultConfig()
        {
            Add(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default);

            Add(Job.Dry
                .With(Platform.X64)
                .With(Jit.RyuJit)
                .With(Runtime.Clr)
                .WithGcServer(true)
                .WithWarmupCount(1)
                .WithLaunchCount(1)
                .WithTargetCount(40)
                .WithRemoveOutliers(true)
                .WithAnalyzeLaunchVariance(true)
                .WithEvaluateOverhead(true));
        }
    }
}