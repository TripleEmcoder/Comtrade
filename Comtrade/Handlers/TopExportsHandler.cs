using System.Linq;

namespace Comtrade.Handlers
{
    internal class TopExportsHandler
    {
        private readonly ComtradeClient comtrade;

        public TopExportsHandler(ComtradeClient comtrade)
        {
            this.comtrade = comtrade;
        }

        public async Task Run(string classification, int reporter, CancellationToken cancellationToken)
        {
            var allExportedCommoditiesOfReporterQuery = new DataQuery
            {
                Reporter = reporter,
                Partner = 0,
                Classification = classification,
                Commodity = "AG4",
                Flow = 2
            };

            var topExportedCommoditiesOfReporter = (await comtrade.Data(allExportedCommoditiesOfReporterQuery, cancellationToken))
                .Results.AsTradeValueShares(allExportedCommoditiesOfReporterQuery.ToShortString())
                .Where(s => s.Share >= 0.05)
                .OrderByDescending(s => s.Value)
                .ToList();

            foreach (var topCommodity in topExportedCommoditiesOfReporter)
            {
                Console.WriteLine(topCommodity);

                var topCommodityAsPartOfWorldExportsQuery = allExportedCommoditiesOfReporterQuery with
                {
                    Reporter = null,
                    Commodity = topCommodity.Data.CommodityCode,
                };

                var topCommodityAsPartOfWorldExports = (await comtrade.Data(topCommodityAsPartOfWorldExportsQuery, cancellationToken))
                    .Results.AsTradeValueShares(topCommodityAsPartOfWorldExportsQuery.ToShortString())
                    .GetReporter(allExportedCommoditiesOfReporterQuery.Reporter.Value);

                Console.WriteLine(topCommodityAsPartOfWorldExports);

                var topCommodityPartnersQuery = allExportedCommoditiesOfReporterQuery with
                {
                    Partner = null,
                    Commodity = topCommodity.Data.CommodityCode,
                };

                var topCommodityPartners = (await comtrade.Data(topCommodityPartnersQuery, cancellationToken))
                    .Results.AsTradeValueShares(topCommodityPartnersQuery.ToShortString())
                    .Where(s => s.Share >= 0.1)
                    .OrderByDescending(s => s.Value);

                foreach (var topCommodityPartner in topCommodityPartners)
                {
                    Console.WriteLine(topCommodityPartner);

                    var allImportsOfTopCommodityReportedByPartnerQuery = topCommodityPartnersQuery with
                    {
                        Reporter = topCommodityPartner.Data.PartnerCode,
                        Flow = 1,
                    };

                    var topCommodityAsPartOfPartnerImports = (await comtrade.Data(allImportsOfTopCommodityReportedByPartnerQuery, cancellationToken))
                        .Results.AsTradeValueShares(allImportsOfTopCommodityReportedByPartnerQuery.ToShortString())
                        .GetPartner(allExportedCommoditiesOfReporterQuery.Reporter.Value);

                    Console.WriteLine(topCommodityAsPartOfPartnerImports);
                }

            }
        }
    }
}