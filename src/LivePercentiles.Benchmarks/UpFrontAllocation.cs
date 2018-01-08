using System.Linq;
using BenchmarkDotNet.Attributes;
using System;
using LivePercentiles.StreamingBuilders;
using HdrHistogram;
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
                res += new PsquareSinglePercentileAlgorithmBuilder(99, Precision.LessPreciseAndFaster).GetHashCode();
            }

            return res;
        }


        [Benchmark(OperationsPerInvoke = _allocCount)]
        public int HdrHistogramHigh()
        {
            int res = 0;
            for (int i = 0; i < _allocCount; i++)
            {
                res += new IntHistogram(Int32.MaxValue, 0).GetHashCode();
            }

            return res;
        }

        [Benchmark(OperationsPerInvoke = _allocCount)]
        public int HdrHistogramLow()
        {
            int res = 0;
            for (int i = 0; i < _allocCount; i++)
            {
                res += new IntHistogram(1000000, 0).GetHashCode();
            }

            return res;
        }

        [Benchmark(OperationsPerInvoke = _allocCount)]
        public int TDigest()
        {
            int res = 0;
            for (int i = 0; i < _allocCount; i++)
            {
                res += new TDigest().GetHashCode();
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
                res += new ConstantErrorBasicCKMSBuilder(0.001, p).GetHashCode();
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
                res += new ConstantErrorBasicCKMSBuilder(0.000001, p).GetHashCode();
            }

            return res;
        }
    }

    /*
    Reference results:

BenchmarkDotNet=v0.10.11, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.125)
Processor=Intel Core i7-7700 CPU 3.60GHz (Kaby Lake), ProcessorCount=8
Frequency=3515623 Hz, Resolution=284.4446 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2600.0
  Job-UIUNXN : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2600.0

AnalyzeLaunchVariance=True  EvaluateOverhead=True  RemoveOutliers=True
Jit=RyuJit  Platform=X64  Runtime=Clr
Server=True  LaunchCount=1  RunStrategy=ColdStart
TargetCount=40  UnrollFactor=1  WarmupCount=1

           Method |        Mean |      Error |     StdDev |      Median | Allocated |
----------------- |------------:|-----------:|-----------:|------------:|----------:|
               P2 |    86.72 ns |   5.606 ns |   9.520 ns |    85.33 ns |     170 B |
 HdrHistogramHigh |   174.57 ns |  20.515 ns |  33.706 ns |   187.73 ns |     387 B |
  HdrHistogramLow |   163.59 ns |  19.589 ns |  33.264 ns |   164.98 ns |     389 B |
          TDigest | 1,291.09 ns | 217.019 ns | 380.091 ns | 1,234.49 ns |     796 B |
          CkmsLow |    63.11 ns |   3.128 ns |   4.870 ns |    62.58 ns |     174 B |
         CkmsHigh |    65.83 ns |   3.964 ns |   6.514 ns |    62.58 ns |     170 B |
     */
}
