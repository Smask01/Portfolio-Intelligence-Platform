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
        
        var holdings = exposures.GroupBy(exposure => exposure.Symbol).Select(group =>
        { 
            var amountExposed = group.Sum(exposure => exposure.AmountExposed);
            return new HoldingExposure
            { 
                Symbol = group.Key, 
                CompanyName = group.First().CompanyName, 
                AmountExposed = amountExposed, 
                PortfolioPercentage = amountExposed / totalPortfolioValue * 100
            };
        }).ToList();
        
        return holdings;
    }
    
    public IReadOnlyCollection<SectorExposure> CalculateSectorExposure(IReadOnlyCollection<PortfolioPosition> positions, IReadOnlyCollection<Etf> etfs)
    {
        if (positions.Count == 0)
        {
            throw new ArgumentException("Portfolio must contain at least one position.");
        }

        var totalPortfolioValue = positions.Sum(position => position.AmountInvested);

        var exposures = new List<SectorExposure>();

        foreach (var position in positions)
        {
            var etf = etfs.SingleOrDefault(etf => etf.Ticker.Equals(position.EtfTicker, StringComparison.OrdinalIgnoreCase));

            if (etf is null)
            {
                throw new InvalidOperationException($"ETF data was not found for ticker {position.EtfTicker}.");
            }

            foreach (var sector in etf.SectorAllocations)
            {
                var amountExposed =
                    position.AmountInvested * sector.Weight;

                exposures.Add(new SectorExposure
                {
                    Sector = sector.Sector,
                    AmountExposed = amountExposed,
                    PortfolioPercentage =
                        amountExposed / totalPortfolioValue * 100
                });
            }
        }

        var sectorExposure = exposures
            .GroupBy(exposure => exposure.Sector)
            .Select(group =>
            {
                var amountExposed = group.Sum(x => x.AmountExposed);

                return new SectorExposure
                {
                    Sector = group.Key,
                    AmountExposed = amountExposed,
                    PortfolioPercentage = amountExposed / totalPortfolioValue * 100
                };
            })
            .ToList();
        
        return sectorExposure;
    }
}