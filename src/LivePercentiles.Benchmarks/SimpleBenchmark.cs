using System.Linq;
using BenchmarkDotNet.Attributes;
using System;
using LivePercentiles.StreamingBuilders;
using HdrHistogram;
using StatsLib;

namespace LivePercentiles.Benchmarks
{
    [Config(typeof(DefaultConfig))]
    public class SimpleBenchmark
    {
        private const int _few = 100;
        private const int _many = 100000;

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

        private TDigest _tDigest;
        private ConstantErrorBasicCKMSBuilder _ckms_95lowPrec;
        private ConstantErrorBasicCKMSBuilder _ckms_95highPrec;

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

            this._ckms_95lowPrec = new ConstantErrorBasicCKMSBuilder(0.001, new double[] { 95 });
            this._ckms_95highPrec = new ConstantErrorBasicCKMSBuilder(0.000001, new double[] { 95 });

            this._hdr_low = new IntHistogram(_few + 1, 3);
            this._hdr_high = new IntHistogram(_many + 1, 3);

            this._tDigest = new TDigest();
        }

        [Benchmark(Baseline = true, OperationsPerInvoke = _few)]
        public long MathAbs_baseline()
        {
            int res = 0;
            foreach (int e in this._dataFewLow)
            {
                res += Math.Abs(e);
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

        [Benchmark(OperationsPerInvoke = _few)]
        public int TDigest_fewLow()
        {
            foreach (double e in this._dataFewLow)
            {
                this._tDigest.Add(e);
            }

            return this._tDigest.GetHashCode();
        }

        [Benchmark(OperationsPerInvoke = _few)]
        public int Ckms_95lowPrec_fewLow()
        {
            foreach (double e in this._dataFewLow)
            {
                this._ckms_95lowPrec.AddValue(e);
            }

            return this._ckms_95lowPrec.GetHashCode();
        }

        [Benchmark(OperationsPerInvoke = _few)]
        public int Ckms_95highPrec_fewLow()
        {
            foreach (double e in this._dataFewLow)
            {
                this._ckms_95highPrec.AddValue(e);
            }

            return this._ckms_95highPrec.GetHashCode();
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

                 Method |         Mean |         Error |       StdDev |       Median |   Scaled | ScaledSD | Allocated |
----------------------- |-------------:|--------------:|-------------:|-------------:|---------:|---------:|----------:|
       MathAbs_baseline |     3.743 ns |     0.7768 ns |     1.340 ns |     2.844 ns |     1.00 |     0.00 |       0 B |
     P2_95Normal_fewLow |   145.843 ns |    18.3997 ns |    29.184 ns |   128.000 ns |    43.18 |    14.83 |       0 B |
     P2_99Normal_fewLow |   148.170 ns |    20.9349 ns |    33.205 ns |   128.000 ns |    43.87 |    15.73 |       0 B |
       P2_95Fast_fewLow |   155.124 ns |     3.3486 ns |     4.802 ns |   156.445 ns |    45.93 |    12.76 |       0 B |
       P2_99Fast_fewLow |   118.282 ns |    15.5149 ns |    25.922 ns |   105.245 ns |    35.02 |    12.45 |       0 B |
     P2_95Fast_manyHigh |    91.783 ns |     6.5257 ns |    11.081 ns |    90.220 ns |    27.17 |     8.22 |       0 B |
     P2_99Fast_manyHigh |    92.066 ns |     8.6573 ns |    14.701 ns |    85.632 ns |    27.26 |     8.74 |       0 B |
             Hdr_fewLow |    14.821 ns |     0.9514 ns |     1.641 ns |    14.222 ns |     4.39 |     1.31 |       0 B |
            Hdr_manyLow |     9.541 ns |     1.0625 ns |     1.861 ns |     9.418 ns |     2.82 |     0.96 |       0 B |
            Hdr_fewHigh |    19.279 ns |     1.0060 ns |     1.681 ns |    19.911 ns |     5.71 |     1.66 |       0 B |
           Hdr_manyHigh |    12.395 ns |     1.5568 ns |     2.727 ns |    11.745 ns |     3.67 |     1.31 |       0 B |
         TDigest_fewLow | 4,460.615 ns |   564.8650 ns |   974.364 ns | 4,632.180 ns | 1,320.59 |   469.09 |     901 B |
  Ckms_95lowPrec_fewLow | 4,608.148 ns | 1,770.2047 ns | 3,100.371 ns | 4,150.047 ns | 1,364.27 | 1,012.53 |      95 B |
 Ckms_95highPrec_fewLow | 5,160.574 ns | 2,133.1773 ns | 3,679.626 ns | 4,145.780 ns | 1,527.82 | 1,192.20 |      98 B |
     */
}
