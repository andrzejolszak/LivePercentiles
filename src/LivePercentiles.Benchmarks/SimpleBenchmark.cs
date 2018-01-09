using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using HdrHistogram;
using LivePercentiles.StreamingBuilders;
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

            this._hdr_low = new IntHistogram(Int32.MaxValue / 2, 0);
            this._hdr_high = new IntHistogram(Int32.MaxValue, 0);

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
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2600.0
  Job-DPZLSI : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2600.0

AnalyzeLaunchVariance=True  EvaluateOverhead=True  RemoveOutliers=True
Jit=RyuJit  Platform=X64  Runtime=Clr
Server=True  LaunchCount=1  RunStrategy=ColdStart
TargetCount=40  UnrollFactor=1  WarmupCount=1

                 Method |         Mean |         Error |       StdDev |       Median |   Scaled | ScaledSD | Allocated |
----------------------- |-------------:|--------------:|-------------:|-------------:|---------:|---------:|----------:|
       MathAbs_baseline |     3.443 ns |     0.6813 ns |     1.175 ns |     2.844 ns |     1.00 |     0.00 |       0 B |
     P2_95Normal_fewLow |    59.296 ns |     5.6203 ns |     9.843 ns |    59.733 ns |    18.65 |     5.28 |       0 B |
     P2_99Normal_fewLow |    62.420 ns |     3.7508 ns |     6.267 ns |    62.578 ns |    19.63 |     4.90 |       0 B |
       P2_95Fast_fewLow |    44.344 ns |     2.6006 ns |     4.555 ns |    45.511 ns |    13.95 |     3.49 |       0 B |
       P2_99Fast_fewLow |    45.146 ns |     3.1435 ns |     5.506 ns |    45.511 ns |    14.20 |     3.68 |       0 B |
     P2_95Fast_manyHigh |    29.879 ns |     1.1804 ns |     1.906 ns |    29.852 ns |     9.40 |     2.23 |       0 B |
     P2_99Fast_manyHigh |    27.450 ns |     2.4700 ns |     4.261 ns |    25.482 ns |     8.63 |     2.39 |       0 B |
             Hdr_fewLow |    14.808 ns |     0.7230 ns |     1.167 ns |    14.222 ns |     4.66 |     1.12 |       0 B |
            Hdr_manyLow |     9.581 ns |     1.0222 ns |     1.790 ns |     8.400 ns |     3.01 |     0.89 |       0 B |
            Hdr_fewHigh |    17.730 ns |     0.8175 ns |     1.224 ns |    17.067 ns |     5.58 |     1.33 |       0 B |
           Hdr_manyHigh |    12.431 ns |     0.9206 ns |     1.612 ns |    12.917 ns |     3.91 |     1.03 |       0 B |
         TDigest_fewLow | 4,126.522 ns |   434.3215 ns |   737.511 ns | 4,132.980 ns | 1,298.04 |   377.54 |     900 B |
  Ckms_95lowPrec_fewLow | 4,282.423 ns | 1,629.9868 ns | 2,854.790 ns | 4,147.202 ns | 1,347.08 |   959.53 |      95 B |
 Ckms_95highPrec_fewLow | 3,696.175 ns | 1,292.1317 ns | 2,263.064 ns | 3,313.780 ns | 1,162.67 |   767.83 |      94 B |
     */
}