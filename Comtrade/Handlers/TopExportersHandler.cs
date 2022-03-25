using System.Linq;

namespace Comtrade.Handlers
{
    internal class TopExportersHandler
    {
        private readonly ComtradeClient comtrade;

        public TopExportersHandler(ComtradeClient comtrade)
        {
            this.comtrade = comtrade;
        }

        public async Task Run(string classification, string commodity, double minShare, CancellationToken cancellationToken)
        {
            var allExportersOfCommodityQuery = new DataQuery
            {
                Reporter = null,
                Partner = 0,
                Classification = classification,
                Commodity = commodity,
                Flow = 2
            };

            var topExportersOfCommodityQuery = (await comtrade.Data(allExportersOfCommodityQuery, cancellationToken))
                .Results.AsTradeValueShares("WLD")
                .Where(s => s.Share >= minShare)
                .OrderByDescending(s => s.Value)
                .ToList();

            foreach (var topExporter in topExportersOfCommodityQuery)
            {
                Console.WriteLine(topExporter);

                var topExporterPartnersQuery = allExportersOfCommodityQuery with
                {
                    Reporter = topExporter.Data.ReporterCode,
                    Partner = null
                };

                var topExporterPartners = (await comtrade.Data(topExporterPartnersQuery, cancellationToken))
                    .Results.AsTradeValueShares(topExporterPartnersQuery.ToShortString())
                    .Where(s => s.Share >= 0.05)
                    .OrderByDescending(s => s.Value);

                foreach (var topExporterPartner in topExporterPartners)
                {
                    Console.WriteLine(topExporterPartner);

                    var allImportsOfCommodityReportedByTopPartnerQuery = topExporterPartnersQuery with
                    {
                        Reporter = topExporterPartner.Data.PartnerCode,
                        Flow = 1,
                    };

                    var topExporterAsPartOfAllTopPartnerImports = (await comtrade.Data(allImportsOfCommodityReportedByTopPartnerQuery, cancellationToken))
                        .Results.AsTradeValueShares(allImportsOfCommodityReportedByTopPartnerQuery.ToShortString())
                        .GetPartner(topExporter.Data.ReporterCode);

                    Console.WriteLine(topExporterAsPartOfAllTopPartnerImports);
                }
            }
        }
    }
}