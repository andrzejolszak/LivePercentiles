using System.Collections.Generic;
using System.Linq;
using HdrHistogram;

namespace LivePercentiles.Tests
{
    /// <summary>
    /// Wrapper around Hdr histogram for benching / comparison
    /// </summary>
    public class HdrHistogramBuilder : IPercentileBuilder
    {
        private readonly IntHistogram _histogram;
        private readonly double[] _desiredPercentiles;

        public HdrHistogramBuilder(int highestTrackableValue, int numberOfSignificantValueDigits, double[] desiredPercentiles = null)
        {
            _desiredPercentiles = desiredPercentiles ?? Constants.DefaultPercentiles;
            _histogram = new IntHistogram(highestTrackableValue, numberOfSignificantValueDigits);
        }

        public void AddValue(double value)
        {
            _histogram.RecordValue((long)value);
        }

        public Percentile[] GetPercentiles()
        {
            return DoGetPercentiles().ToArray();
        }

        public int GetEstimatedSize()
        {
            return _histogram.GetEstimatedFootprintInBytes();
        }

        private IEnumerable<Percentile> DoGetPercentiles()
        {
            foreach (var desiredPercentile in _desiredPercentiles)
            {
                yield return new Percentile(desiredPercentile, _histogram.GetValueAtPercentile(desiredPercentile));
            }
        }
    }
}