using System.Linq;

namespace Comtrade.Handlers
{
    internal class TopDominatedHandler
    {
        private readonly ComtradeClient comtrade;

        public TopDominatedHandler(ComtradeClient comtrade)
        {
            this.comtrade = comtrade;
        }

        public async Task Run(string classification, double minShare, CancellationToken cancellationToken)
        {
            var exports = new List<DataResult>();

            var totalExportsOfAllReportersQuery = new DataQuery
            {
                Reporter = null,
                Partner = 0,
                Classification = classification,
                Commodity = "total",
                Flow = 2
            };

            var totalExportsOfAllReporters = await comtrade.Data(totalExportsOfAllReportersQuery, cancellationToken);

            foreach (var totalExport in totalExportsOfAllReporters.Results)
            {
                Console.WriteLine(totalExport);

                var allExportsOfReporterQuery = new DataQuery
                {
                    Reporter = totalExport.ReporterCode,
                    Partner = 0,
                    Classification = classification,
                    Commodity = "AG4",
                    Flow = 2
                };

                var allExportsOfReporter = await comtrade.Data(allExportsOfReporterQuery, cancellationToken);
                exports.AddRange(allExportsOfReporter.Results);
            }

            var commoditiesWithTopExporters = exports
                .GroupBy(e => e.CommodityCode, (c, e) => e
                    .AsTradeValueShares(c)
                    .Where(e => e.Share >= minShare)
                    .OrderByDescending(e => e.Share)
                    .ToList()
                )
                .OrderByDescending(c => c.First().Share)
                .SelectMany(c => c)
                .ToList();
            

            foreach (var commodity in commoditiesWithTopExporters)
            {
                Console.WriteLine(commodity);
            }
        }
    }
}