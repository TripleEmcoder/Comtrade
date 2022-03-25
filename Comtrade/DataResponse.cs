using Humanizer;
using System.Text.Json.Serialization;

namespace Comtrade
{
    public record DataQuery
    {
        public int? Reporter { get; init; }
        public int? Partner { get; init; }
        public string Classification { get; init; }
        public string Commodity { get; init; }
        public int? Flow { get; init; }

        public string FlowArrow
            => Flow switch
            {
                1 => "<--",
                2 => "-->",
                3 => "<-<",
                4 => ">->",
                _ => throw new ArgumentOutOfRangeException(),
            };

        public string ToShortString()
            => $"{Commodity} {Reporter ?? 0} {FlowArrow} {Partner ?? 0}";
    }

    public class DataResponse
    {
        public ResponseValidation Validation { get; set; }

        public class ResponseValidation
        {
            public ValidationStatus Status { get; set; }
            public string Message { get; set; }

            public override string ToString()
                => $"{Status} {Message}";

            public class ValidationStatus
            {
                public string Name { get; set; }
                public int Value { get; set; }
                public string Description { get; set; }

                public override string ToString()
                    => $"{Value}: {Name} - {Description}";
            }
        }

        [JsonPropertyName("dataset")]
        public IList<DataResult> Results { get; set; }
    }

    public class DataResult
    {
        [JsonPropertyName("rtCode")]
        public int ReporterCode { get; set; }

        [JsonPropertyName("rt3ISO")]
        public string ReporterIsoCode { get; set; }

        [JsonPropertyName("rtTitle")]
        public string ReporterDescription { get; set; }

        [JsonPropertyName("ptCode")]
        public int PartnerCode { get; set; }

        [JsonPropertyName("pt3ISO")]
        public string PartnerIsoCode { get; set; }

        [JsonPropertyName("ptTitle")]
        public string PartnerDescription { get; set; }

        [JsonPropertyName("cmdCode")]
        public string CommodityCode { get; set; }

        [JsonPropertyName("cmdDescE")]
        public string CommodityDescription { get; set; }

        [JsonPropertyName("rgCode")]
        public int FlowCode { get; set; }

        [JsonPropertyName("rgDesc")]
        public string FlowDescription { get; set; }

        [JsonPropertyName("qtCode")]
        public int QuantityCode { get; set; }

        [JsonPropertyName("qtDesc")]
        public string QuantityDescription { get; set; }

        public long TradeQuantity { get; set; }
        public long TradeValue { get; set; }

        public string FlowArrow
            => FlowCode switch
            {
                1 => "<--",
                2 => "-->",
                3 => "<-<",
                4 => ">->",
                _ => throw new ArgumentOutOfRangeException(),
            };

        public override string ToString()
            => $"{CommodityCode} - {CommodityDescription.Truncate(40, "...")}"
            + $" {ReporterCode}({ReporterIsoCode}) {FlowArrow} {PartnerCode}({PartnerIsoCode}): {(1.0 * TradeValue).ToMetric(decimals: 1)}";
    }

    public static class DataResponseExtensions
    {
        public static DataShares<DataResult> AsTradeValueShares(this IEnumerable<DataResult> results, string totalName)
            => DataShares.Create(results, d => d.TradeValue, totalName);

        public static DataShare<DataResult> GetReporter(this DataShares<DataResult> all, int reporter)
            => all.SingleOrDefault(s => s.Data.ReporterCode == reporter);

        public static DataShare<DataResult> GetPartner(this DataShares<DataResult> all, int partner)
            => all.SingleOrDefault(s => s.Data.PartnerCode == partner);
    }
}
