using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Domain;

namespace PortfolioIntelligencePlatform.Infrastructure;

public class InMemoryEtfDataProvider : IEtfDataProvider
{
    private readonly List<Etf> _etfs =
    [
        new()
        {
            Ticker = "EFIV",
            Name = "EFIV",
            Holdings =
            [
                new EtfHolding
                {
                    Symbol = "AAPL",
                    CompanyName = "Apple Inc.",
                    Sector = "Technology",
                    Weight = 0.10m
                }
            ]
        }
    ];

    public Task<Etf?> GetEtfAsync(string ticker, CancellationToken cancellationToken = default)
    {
        var etf = _etfs.SingleOrDefault(x => x.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(etf);
    }
}