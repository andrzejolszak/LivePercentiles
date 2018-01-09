using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LivePercentiles.StreamingBuilders
{
    /// <summary>
    /// Base class for implementations based on the
    /// Jain & Chlamtac's 1985 paper.
    /// (cf. http://www.cse.wustl.edu/~jain/papers/ftp/psqr.pdf)
    /// </summary>
    public abstract class BasePsquareBuilder
    {
        protected List<double> _startupQueue;
        protected long _observationsCount;
        protected Marker[] _markers = new Marker[0];
        private bool _isInitialized;
        protected bool IsInitialized { get { return _isInitialized; } }

        public void AddValue(double value)
        {
            ++_observationsCount;

            if (!_isInitialized)
                StartupPhase(value);
            else
                NormalPhase(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double ComputePsquareValueForMarker(Marker previousMarker, Marker currentMarker, Marker nextMarker, int markerShift)
        {
            double ratioBetweenPreviousAndNextPosition = (double)markerShift / (nextMarker.Position - previousMarker.Position);
            int distanceBetweenPreviousAndNewPosition = currentMarker.Position - previousMarker.Position + markerShift;
            double differenceBetweenNextAndCurrentValue = nextMarker.Value - currentMarker.Value;
            int differenceBetweenNextAndCurrentPosition = nextMarker.Position - currentMarker.Position;
            int distanceBetweenNextAndNewPosition = nextMarker.Position - currentMarker.Position - markerShift;
            double differenceBetweenPreviousAndCurrentValue = currentMarker.Value - previousMarker.Value;
            int differenceBetweenPreviousAndCurrentPosition = currentMarker.Position - previousMarker.Position;

            return currentMarker.Value
                   + ratioBetweenPreviousAndNextPosition
                   * (distanceBetweenPreviousAndNewPosition * (differenceBetweenNextAndCurrentValue / differenceBetweenNextAndCurrentPosition)
                      + distanceBetweenNextAndNewPosition * (differenceBetweenPreviousAndCurrentValue / differenceBetweenPreviousAndCurrentPosition));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static double ComputeLinearValueForMarker(Marker previousMarker, Marker currentMarker, Marker nextMarker, int markerShift)
        {
            Marker otherMarker = markerShift < 0 ? previousMarker : nextMarker;
            double differenceBetweenOtherAndCurrentValue = otherMarker.Value - currentMarker.Value;
            int differenceBetweenOtherAndCurrentPosition = otherMarker.Position - currentMarker.Position;

            return currentMarker.Value + markerShift * (differenceBetweenOtherAndCurrentValue / differenceBetweenOtherAndCurrentPosition);
        }

        protected abstract bool IsReadyForNormalPhase();

        protected abstract void InitializeMarkers();

        protected abstract void RecomputeNonExtremeMarkersValuesIfNecessary();

        private void StartupPhase(double value)
        {
            _startupQueue.Add(value);
            if (!IsReadyForNormalPhase())
                return;

            InitializeMarkers();
            _isInitialized = true;
        }

        private void NormalPhase(double value)
        {
            var containingBucketIndex = FindContainingBucket(value);

            for (var i = containingBucketIndex + 1; i < _markers.Length; i++)
                _markers[i].IncrementPosition();

            RecomputeNonExtremeMarkersValuesIfNecessary();

            // TODO: Remove after thorough testing
            if (_observationsCount != _markers[_markers.Length - 1].Position)
                throw new InvalidOperationException("That can't be !");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindContainingBucket(double value)
        {
            if (value < _markers[0].Value)
            {
                _markers[0].Value = value;
                return 0;
            }

            for (var i = 0; i < _markers.Length - 2; ++i)
            {
                if (_markers[i].Value <= value && value < _markers[i + 1].Value)
                    return i;
            }

            // TODO: simplify
            if (_markers[_markers.Length - 2].Value <= value && value <= _markers[_markers.Length - 1].Value)
                return _markers.Length - 2;

            if (value > _markers[_markers.Length - 1].Value)
            {
                _markers[_markers.Length - 1].Value = value;
                return _markers.Length - 2;
            }

            throw new InvalidOperationException("Should not happen");
        }
    }
}