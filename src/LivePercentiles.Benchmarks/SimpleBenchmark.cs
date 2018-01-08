using System.Linq;
using BenchmarkDotNet.Attributes;
using System;
using LivePercentiles.StreamingBuilders;
using HdrHistogram;

namespace LivePercentiles.Benchmarks
{
    [Config(typeof(DefaultConfig))]
    public class SimpleBenchmark
    {
        private const int _few = 100;
        private const int _many = 100000;
        private const int _allocCount = 50;

        private double[] _dataFewLow;
        private double[] _dataManyLow;
        private double[] _dataFewHigh;
        private double[] _dataManyHigh;

        private long[] _dataFewLowL;
        private long[] _dataManyLowL;
        private long[] _dataFewHighL;
        private long[] _dataManyHighL;

        private PsquareSinglePercentileAlgorithmBuilder _p2_95Fast;
        private PsquareSinglePercentileAlgorithmBuilder _p2_99Fast;
        private PsquareSinglePercentileAlgorithmBuilder _p2_95Normal;
        private PsquareSinglePercentileAlgorithmBuilder _p2_99Normal;

        private IntHistogram _hdr_low;
        private IntHistogram _hdr_high;

        [GlobalSetup]
        public void Setup()
        {
            if (this._dataFewLow != null)
            {
                return;
            }

            Random rand = new Random(1234567);
            this._dataFewLow = Enumerable.Range(0, _few).Select(x => (double)rand.Next(_few)).ToArray();
            this._dataManyLow = Enumerable.Range(0, _many).Select(x => (double)rand.Next(_few)).ToArray();
            this._dataFewHigh = Enumerable.Range(0, _few).Select(x => (double)rand.Next(_many)).ToArray();
            this._dataManyHigh = Enumerable.Range(0, _many).Select(x => (double)rand.Next(_many)).ToArray();

            this._dataFewLowL = this._dataFewLow.Select(x => (long)x).ToArray();
            this._dataManyLowL = this._dataManyLow.Select(x => (long)x).ToArray();
            this._dataFewHighL = this._dataFewHigh.Select(x => (long)x).ToArray();
            this._dataManyHighL = this._dataManyHigh.Select(x => (long)x).ToArray();

            this._p2_95Fast = new PsquareSinglePercentileAlgorithmBuilder(95, Precision.LessPreciseAndFaster);
            this._p2_99Fast = new PsquareSinglePercentileAlgorithmBuilder(99, Precision.LessPreciseAndFaster);

            this._p2_95Normal = new PsquareSinglePercentileAlgorithmBuilder(95, Precision.Normal);
            this._p2_99Normal = new PsquareSinglePercentileAlgorithmBuilder(99, Precision.Normal);

            this._hdr_low = new IntHistogram(_few + 1, 3);
            this._hdr_high = new IntHistogram(_many + 1, 3);
        }

        [Benchmark(Baseline = true, OperationsPerInvoke = _few)]
        public long MathAbs()
        {
            int res = 0;
            foreach (int e in this._dataFewLow)
            {
                res += Math.Abs(e);
            }

            return res;
        }

        [Benchmark(OperationsPerInvoke = _allocCount)]
        public int P2_manyHigh_alloc()
        {
            int res = 0;
            for (int i = 0; i < _allocCount; i++)
            {
                res += new PsquareSinglePercentileAlgorithmBuilder(99, Precision.LessPreciseAndFaster).GetHashCode();
            }

            return res;
        }


        [Benchmark(OperationsPerInvoke = _allocCount)]
        public int Hdr_manyHigh_alloc()
        {
            int res = 0;
            for (int i = 0; i < _allocCount; i++)
            {
                res += new IntHistogram(_many + 1, 3).GetHashCode();
            }

            return res;
        }

        [Benchmark(OperationsPerInvoke = _few)]
        public int P2_95Normal_fewLow()
        {
            foreach (double e in this._dataFewLow)
            {
                this._p2_95Normal.AddValue(e);
            }

            return this._p2_95Normal.GetHashCode();
        }

        [Benchmark(OperationsPerInvoke = _few)]
        public int P2_99Normal_fewLow()
        {
            foreach (double e in this._dataFewLow)
            {
                this._p2_99Normal.AddValue(e);
            }

            return this._p2_99Normal.GetHashCode();
        }

        [Benchmark(OperationsPerInvoke = _few)]
        public int P2_95Fast_fewLow()
        {
            foreach (double e in this._dataFewLow)
            {
                this._p2_95Fast.AddValue(e);
            }

            return this._p2_95Fast.GetHashCode();
        }

        [Benchmark(OperationsPerInvoke = _few)]
        public int P2_99Fast_fewLow()
        {
            foreach (double e in this._dataFewLow)
            {
                this._p2_99Fast.AddValue(e);
            }

            return this._p2_99Fast.GetHashCode();
        }

        [Benchmark(OperationsPerInvoke = _many)]
        public int P2_95Fast_manyHigh()
        {
            foreach (double e in this._dataManyHigh)
            {
                this._p2_95Fast.AddValue(e);
            }

            return this._p2_95Fast.GetHashCode();
        }

        [Benchmark(OperationsPerInvoke = _many)]
        public int P2_99Fast_manyHigh()
        {
           foreach (double e in this._dataManyHigh)
           {
               this._p2_99Fast.AddValue(e);
           }

            return this._p2_99Fast.GetHashCode();
        }

        [Benchmark(OperationsPerInvoke = _few)]
        public long Hdr_fewLow()
        {
            foreach (long e in this._dataFewLowL)
            {
                _hdr_low.RecordValue(e);
            }

            return _hdr_low.TotalCount;
        }

        [Benchmark(OperationsPerInvoke = _many)]
        public long Hdr_manyLow()
        {
            foreach (long e in this._dataManyLowL)
            {
                _hdr_low.RecordValue(e);
            }

            return _hdr_low.TotalCount;
        }

        [Benchmark(OperationsPerInvoke = _few)]
        public long Hdr_fewHigh()
        {
            foreach (long e in this._dataFewHighL)
            {
                _hdr_high.RecordValue(e);
            }

            return _hdr_high.TotalCount;
        }

        [Benchmark(OperationsPerInvoke = _many)]
        public long Hdr_manyHigh()
        {
            foreach (long e in this._dataManyHighL)
            {
                _hdr_high.RecordValue(e);
            }

            return _hdr_high.TotalCount;
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

             Method |         Mean |       Error |      StdDev |       Median | Scaled | ScaledSD | Allocated |
------------------- |-------------:|------------:|------------:|-------------:|-------:|---------:|----------:|
            MathAbs |     4.042 ns |   0.8251 ns |   1.4233 ns |     2.844 ns |   1.00 |     0.00 |       0 B |
  P2_manyHigh_alloc |    78.669 ns |   6.4222 ns |  10.5518 ns |    73.956 ns |  21.83 |     7.47 |     174 B |
 Hdr_manyHigh_alloc | 1,754.075 ns | 241.0969 ns | 382.4052 ns | 1,763.557 ns | 486.85 |   187.53 |   32988 B |
 P2_95Normal_fewLow |   184.336 ns |  20.0484 ns |  33.4964 ns |   197.689 ns |  51.16 |    18.66 |       0 B |
 P2_99Normal_fewLow |   161.288 ns |  22.2047 ns |  37.7052 ns |   150.756 ns |  44.77 |    17.69 |       0 B |
   P2_95Fast_fewLow |   126.541 ns |  15.4023 ns |  26.9759 ns |   125.156 ns |  35.12 |    13.44 |       0 B |
   P2_99Fast_fewLow |   131.069 ns |  14.2234 ns |  24.5347 ns |   142.222 ns |  36.38 |    13.38 |       0 B |
 P2_95Fast_manyHigh |    96.814 ns |   3.5479 ns |   6.1199 ns |    95.030 ns |  26.87 |     8.58 |       0 B |
 P2_99Fast_manyHigh |    93.240 ns |   7.6073 ns |  12.4991 ns |    90.320 ns |  25.88 |     8.85 |       0 B |
         Hdr_fewLow |    15.345 ns |   0.9804 ns |   1.6912 ns |    14.222 ns |   4.26 |     1.42 |       0 B |
        Hdr_manyLow |     8.796 ns |   0.4852 ns |   0.8370 ns |     8.677 ns |   2.44 |     0.80 |       0 B |
        Hdr_fewHigh |    19.834 ns |   1.3381 ns |   2.2722 ns |    19.911 ns |   5.51 |     1.84 |       0 B |
       Hdr_manyHigh |    12.604 ns |   1.1513 ns |   2.0164 ns |    11.975 ns |   3.50 |     1.24 |       0 B |
     */
}
