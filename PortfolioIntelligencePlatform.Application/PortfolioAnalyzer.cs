using PortfolioIntelligencePlatform.Domain;

namespace PortfolioIntelligencePlatform.Application;

public class PortfolioAnalyzer : IPortfolioAnalyzer
{
    public IReadOnlyCollection<HoldingExposure> CalculateExposure(IReadOnlyCollection<PortfolioPosition> positions, IReadOnlyCollection<Etf> etfs)
    {
        var totalPortfolioValue = positions.Sum(position => position.AmountInvested);
        var exposures = new List<HoldingExposure>();

        foreach (var position in positions)
        {
            var etf = etfs.Single(etf =>
                etf.Ticker.Equals(position.EtfTicker, StringComparison.OrdinalIgnoreCase));

            foreach (var holding in etf.Holdings)
            {
                var amountExposed = position.AmountInvested * holding.Weight;

                exposures.Add(new HoldingExposure
                {
                    Symbol = holding.Symbol,
                    CompanyName = holding.CompanyName,
                    AmountExposed = amountExposed,
                    PortfolioPercentage = amountExposed / totalPortfolioValue * 100
                });
            }
        }

        return exposures;

    }
}