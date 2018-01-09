namespace LivePercentiles
{
    public interface IPercentileBuilder
    {
        void AddValue(double value);

        Percentile[] GetPercentiles();
    }
}