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
            var etf = etfs.SingleOrDefault(etf => etf.Ticker.Equals(position.EtfTicker, StringComparison.OrdinalIgnoreCase));

            if (etf is null)
            {
                throw new InvalidOperationException($"ETF data was not found for ticker {position.EtfTicker}.");
            }
            
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
            
            if (positions.Count == 0)
            {
                throw new ArgumentException("Portfolio must contain at least one position.");
            }

            if (positions.Any(p => p.AmountInvested <= 0))
            {
                throw new ArgumentException("Investment amounts must be greater than zero.");
            }
        }

        return exposures
            .GroupBy(exposure => exposure.Symbol)
            .Select(group =>
            {
                var amountExposed = group.Sum(exposure => exposure.AmountExposed);

                return new HoldingExposure
                {
                    Symbol = group.Key,
                    CompanyName = group.First().CompanyName,
                    AmountExposed = amountExposed,
                    PortfolioPercentage = amountExposed / totalPortfolioValue * 100
                };
            })
            .ToList();
    }
}