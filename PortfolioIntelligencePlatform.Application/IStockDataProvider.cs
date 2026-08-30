using PortfolioIntelligencePlatform.Domain;

namespace PortfolioIntelligencePlatform.Application;

public interface IStockDataProvider
{
    Task<Stock?> GetStockAsync(
        string symbol,
        CancellationToken cancellationToken = default);
}