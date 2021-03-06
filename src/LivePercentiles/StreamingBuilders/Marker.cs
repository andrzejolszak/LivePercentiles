﻿using System.Runtime.CompilerServices;

namespace LivePercentiles.StreamingBuilders
{
    public class Marker
    {
        public int Position { get; set; }
        public double Value { get; set; }
        public double Percentile { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementPosition()
        {
            Position = Position + 1;
        }

        public Marker(int position, double value, double percentile)
        {
            Position = position;
            Value = value;
            Percentile = percentile;
        }
    }
}