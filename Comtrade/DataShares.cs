using System.Collections;

namespace Comtrade
{


    public class DataShares
    {
        public static DataShares<T> Create<T>(IEnumerable<T> data, Func<T, double> metric, string totalName)
            => new(data, metric, totalName);
    }

    public class DataShares<T> : IEnumerable<DataShare<T>>
    {
        private readonly IEnumerable<DataShare<T>> data;

        public DataShares(IEnumerable<T> data, Func<T, double> metric, string totalName)
        {
            this.data = data.Select(d => new DataShare<T>(this, d, metric));
            Total = data.Sum(metric);
            TotalName = totalName;
        }

        public double Total { get; }
        public string TotalName { get; }

        public IEnumerator<DataShare<T>> GetEnumerator() => data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class DataShare<T>
    {
        public DataShare(DataShares<T> shareOf, T data, Func<T, double> metric)
        {
            ShareOf = shareOf;
            Data = data;
            Value = metric(data);
        }

        public DataShares<T> ShareOf { get; }
        public T Data { get; }
        public double Value { get; }
        public double Share => Value / ShareOf.Total;

        public override string ToString()
            => $"{Data} ({Share:P0} of {ShareOf.TotalName})";
    }
}
