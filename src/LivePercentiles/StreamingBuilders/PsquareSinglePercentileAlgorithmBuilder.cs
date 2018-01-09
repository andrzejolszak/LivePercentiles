using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LivePercentiles.StreamingBuilders
{
    /// <summary>
    /// Implementation using the P² single value algorithm described in
    /// Jain & Chlamtac's 1985 paper.
    /// The desired percentile must be provided along with a precision,
    /// the more precise the estimate, the slower the calculation.
    /// The resulting percentile is an estimate but the input
    /// data is not stored, resulting in a very small memory
    /// footprint.
    /// (cf. http://www.cse.wustl.edu/~jain/papers/ftp/psqr.pdf)
    /// </summary>
    public class PsquareSinglePercentileAlgorithmBuilder : BasePsquareBuilder, IPercentileBuilder
    {
        private readonly double _desiredPercentile;
        private readonly int _intermediateMarkersCount;
        private readonly int _halfBucketCount;
        private readonly int _desiredPercentileIndex;

        public PsquareSinglePercentileAlgorithmBuilder(double desiredPercentile, Precision precision = Constants.DefaultPrecision)
        {
            if (desiredPercentile < 0)
                throw new ArgumentException("Only positive percentiles are allowed.", "desiredPercentile");
            _desiredPercentile = desiredPercentile;
            _intermediateMarkersCount = GetNumberOfIntermediateMarkersFromPrecision(precision);
            _halfBucketCount = (_intermediateMarkersCount + 2) / 2;
            _desiredPercentileIndex = (_intermediateMarkersCount + 3) / 2;
            _startupQueue = new List<double>(_intermediateMarkersCount + 3);
        }

        public Percentile[] GetPercentiles()
        {
            if (!IsInitialized)
                return new Percentile[0];

            return new[] { new Percentile(_desiredPercentile, _markers[_desiredPercentileIndex].Value) };
        }

        protected override bool IsReadyForNormalPhase()
        {
            return _observationsCount >= _intermediateMarkersCount + 3;
        }

        protected override void InitializeMarkers()
        {
            _startupQueue.Sort();
            _markers = new Marker[_startupQueue.Count];

            for (int i = 0; i < _startupQueue.Count; i++)
            {
                if (i == 0)
                    _markers[i] = new Marker(i + 1, _startupQueue[i], double.NaN);
                else if (i == _startupQueue.Count - 1)
                    _markers[i] = new Marker(i + 1, _startupQueue[i], double.NaN);
                else if (i < _desiredPercentileIndex)
                    _markers[i] = new Marker(i + 1, _startupQueue[i], _desiredPercentile / _halfBucketCount * i);
                else if (i > _desiredPercentileIndex)
                    _markers[i] = new Marker(i + 1, _startupQueue[i], _desiredPercentile + ((100d - _desiredPercentile) / _halfBucketCount * (i - _desiredPercentileIndex)));
                else
                    _markers[i] = new Marker(i + 1, _startupQueue[i], _desiredPercentile);
            }
        }

        // TODO: Factorize with other implem
        protected override void RecomputeNonExtremeMarkersValuesIfNecessary()
        {
            for (int i = 1; i < _markers.Length - 1; ++i)
            {
                Marker item = _markers[i];
                Marker itemNext = _markers[i + 1];
                Marker itemPrev = _markers[i - 1];

                double desiredPosition = 1d + (_observationsCount - 1L) * item.Percentile / 100d;
                double deltaToDesiredPosition = desiredPosition - item.Position;
                int deltaToNextMarker = itemNext.Position - item.Position;
                int deltaToPreviousMarker = itemPrev.Position - item.Position;

                if ((deltaToDesiredPosition >= 1d && deltaToNextMarker > 1) || (deltaToDesiredPosition <= -1d && deltaToPreviousMarker < -1))
                {
                    int unaryShift = deltaToDesiredPosition < 0d ? -1 : 1;
                    double newMarkerValue = ComputePsquareValueForMarker(itemPrev, item, itemNext, unaryShift);
                    if (itemPrev.Value < newMarkerValue && newMarkerValue < itemNext.Value)
                        item.Value = newMarkerValue;
                    else
                        item.Value = ComputeLinearValueForMarker(itemPrev, item, itemNext, unaryShift);

                    item.Position += unaryShift;
                }
            }
        }

        private int GetNumberOfIntermediateMarkersFromPrecision(Precision precision)
        {
            switch (precision)
            {
                case Precision.LessPreciseAndFaster:
                    return 2;

                case Precision.Normal:
                    return 4;

                case Precision.MorePreciseAndSlower:
                    return 6;

                default:
                    throw new ArgumentException("Unknown precision");
            }
        }
    }
}