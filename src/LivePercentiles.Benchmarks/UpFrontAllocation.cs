using System;
using BenchmarkDotNet.Attributes;
using HdrHistogram;
using LivePercentiles.StreamingBuilders;
using StatsLib;

namespace LivePercentiles.Benchmarks
{
    [Config(typeof(DefaultConfig))]
    public class UpFrontAllocation
    {
        private const int _allocCount = 50;

        [Benchmark(OperationsPerInvoke = _allocCount)]
        public int P2()
        {
            int res = 0;
            for (int i = 0; i < _allocCount; i++)
            {
                var obj = new PsquareSinglePercentileAlgorithmBuilder(99, Precision.LessPreciseAndFaster);
                obj.AddValue(1d);
                obj.AddValue(2d);
                res += obj.GetHashCode();
            }

            return res;
        }

        [Benchmark(OperationsPerInvoke = _allocCount)]
        public int HdrHistogramHigh()
        {
            int res = 0;
            for (int i = 0; i < _allocCount; i++)
            {
                var obj = new IntHistogram(Int32.MaxValue, 0);
                obj.RecordValue(1L);
                obj.RecordValue(2L);
                res += obj.GetHashCode();
            }

            return res;
        }

        [Benchmark(OperationsPerInvoke = _allocCount)]
        public int HdrHistogramLow()
        {
            int res = 0;
            for (int i = 0; i < _allocCount; i++)
            {
                var obj = new IntHistogram(Int32.MaxValue / 2, 0);
                obj.RecordValue(1L);
                obj.RecordValue(2L);
                res += obj.GetHashCode();
            }

            return res;
        }

        [Benchmark(OperationsPerInvoke = _allocCount)]
        public int TDigest()
        {
            int res = 0;
            for (int i = 0; i < _allocCount; i++)
            {
                var obj = new TDigest();
                obj.Add(1d);
                obj.Add(2d);
                res += obj.GetHashCode();
            }

            return res;
        }

        [Benchmark(OperationsPerInvoke = _allocCount)]
        public int CkmsLow()
        {
            double[] p = new double[] { 95 };
            int res = 0;
            for (int i = 0; i < _allocCount; i++)
            {
                var obj = new ConstantErrorBasicCKMSBuilder(0.01, p);
                obj.AddValue(1d);
                obj.AddValue(2d);
                res += obj.GetHashCode();
            }

            return res;
        }

        [Benchmark(OperationsPerInvoke = _allocCount)]
        public int CkmsHigh()
        {
            double[] p = new double[] { 95 };
            int res = 0;
            for (int i = 0; i < _allocCount; i++)
            {
                var obj = new ConstantErrorBasicCKMSBuilder(0.000001, p);
                obj.AddValue(1d);
                obj.AddValue(2d);
                res += obj.GetHashCode();
            }

            return res;
        }
    }

    /*
    Reference results:

BenchmarkDotNet=v0.10.11, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.125)
Processor=Intel Core i7-7700 CPU 3.60GHz (Kaby Lake), ProcessorCount=8
Frequency=3515623 Hz, Resolution=284.4446 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2600.0
  Job-DPZLSI : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2600.0

AnalyzeLaunchVariance=True  EvaluateOverhead=True  RemoveOutliers=True
Jit=RyuJit  Platform=X64  Runtime=Clr
Server=True  LaunchCount=1  RunStrategy=ColdStart
TargetCount=40  UnrollFactor=1  WarmupCount=1

           Method |        Mean |      Error |    StdDev |      Median | Allocated |
----------------- |------------:|-----------:|----------:|------------:|----------:|
               P2 |    95.68 ns |   9.164 ns |  14.54 ns |    91.02 ns |     321 B |
 HdrHistogramHigh |   183.05 ns |  26.738 ns |  43.18 ns |   159.29 ns |     385 B |
  HdrHistogramLow |   191.68 ns |  23.881 ns |  39.90 ns |   199.11 ns |     387 B |
          TDigest | 2,610.62 ns | 326.146 ns | 571.22 ns | 2,560.00 ns |    1658 B |
          CkmsLow |   114.70 ns |   9.493 ns |  16.12 ns |   113.78 ns |     323 B |
         CkmsHigh |   113.30 ns |  11.230 ns |  18.76 ns |   116.62 ns |     319 B |
     */
}