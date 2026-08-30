using PortfolioIntelligencePlatform.Domain;

namespace PortfolioIntelligencePlatform.Application;

public class PortfolioAnalyzer : IPortfolioAnalyzer
{
    public IReadOnlyCollection<HoldingExposure> CalculateExposure(IReadOnlyCollection<PortfolioPosition> positions, IReadOnlyCollection<Etf> etfs, IReadOnlyCollection<Stock> stocks)
    {
        if (positions.Count == 0)
        {
            throw new ArgumentException("Portfolio cannot be empty.", nameof(positions));
        }

        if (positions.Any(x => x.AmountInvested <= 0))
        {
            throw new ArgumentException("Amount invested must be greater than zero.");
        }
        
        foreach (var position in positions)
        {
            var isEtf = etfs.Any(x =>
                x.Ticker.Equals(position.Symbol, StringComparison.OrdinalIgnoreCase));

            var isStock = stocks.Any(x =>
                x.Symbol.Equals(position.Symbol, StringComparison.OrdinalIgnoreCase));

            if (!isEtf && !isStock)
            {
                throw new InvalidOperationException($"No ETF or stock found for symbol {position.Symbol}.");
            }
        }
        
        var exposureBySymbol = new Dictionary<string, HoldingExposure>(StringComparer.OrdinalIgnoreCase);
        var totalPortfolioValue = positions.Sum(x => x.AmountInvested);

        foreach (var etf in etfs)
        {
            var position = positions.First(x => x.Symbol.Equals(etf.Ticker, StringComparison.OrdinalIgnoreCase));

            foreach (var holding in etf.Holdings)
            {
                var amountExposed = position.AmountInvested * holding.Weight;

                if (exposureBySymbol.TryGetValue(holding.Symbol, out var existing))
                {
                    exposureBySymbol[holding.Symbol] = existing with
                    {
                        AmountExposed = existing.AmountExposed + amountExposed
                    };
                }
                else
                {
                    exposureBySymbol[holding.Symbol] = new HoldingExposure
                    {
                        Symbol = holding.Symbol,
                        CompanyName = holding.CompanyName,
                        AmountExposed = amountExposed,
                        PortfolioPercentage = 0
                    };
                }
            }
        }

        foreach (var stock in stocks)
        {
            var position = positions.First(x =>
                x.Symbol.Equals(stock.Symbol, StringComparison.OrdinalIgnoreCase));

            if (exposureBySymbol.TryGetValue(stock.Symbol, out var existing))
            {
                exposureBySymbol[stock.Symbol] = existing with
                {
                    AmountExposed =
                        existing.AmountExposed + position.AmountInvested
                };
            }
            else
            {
                exposureBySymbol[stock.Symbol] = new HoldingExposure
                {
                    Symbol = stock.Symbol,
                    CompanyName = stock.Name,
                    AmountExposed = position.AmountInvested,
                    PortfolioPercentage = 0
                };
            }
        }

        return exposureBySymbol.Values.Select(x => x with
            {
                PortfolioPercentage = totalPortfolioValue == 0 ? 0 : x.AmountExposed / totalPortfolioValue * 100
            })
            .OrderByDescending(x => x.AmountExposed)
            .ToList();
    }
    
    public IReadOnlyCollection<SectorExposure> CalculateSectorExposure(IReadOnlyCollection<PortfolioPosition> positions, IReadOnlyCollection<Etf> etfs, IReadOnlyCollection<Stock> stocks)
    {
        var exposureBySector = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var totalPortfolioValue = positions.Sum(x => x.AmountInvested);

        foreach (var etf in etfs)
        {
            var position = positions.First(x => x.Symbol.Equals(etf.Ticker, StringComparison.OrdinalIgnoreCase));

            foreach (var sector in etf.SectorAllocations)
            {
                var amountExposed = position.AmountInvested * sector.Weight;

                exposureBySector[sector.Sector] = exposureBySector.GetValueOrDefault(sector.Sector) + amountExposed;
            }
        }

        foreach (var stock in stocks)
        {
            var position = positions.First(x => x.Symbol.Equals(stock.Symbol, StringComparison.OrdinalIgnoreCase));

            exposureBySector[stock.Sector] = exposureBySector.GetValueOrDefault(stock.Sector) + position.AmountInvested;
        }

        return exposureBySector.Select(x => new SectorExposure
            {
                Sector = x.Key,
                AmountExposed = x.Value,
                PortfolioPercentage = totalPortfolioValue == 0 ? 0 : x.Value / totalPortfolioValue * 100
            })
            .OrderByDescending(x => x.AmountExposed)
            .ToList();
    }
}