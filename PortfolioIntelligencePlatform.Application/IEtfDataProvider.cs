using PortfolioIntelligencePlatform.Domain;

namespace PortfolioIntelligencePlatform.Application;

public interface IEtfDataProvider
{
    Task<Etf?> GetEtfAsync(string ticker, CancellationToken cancellationToken = default);
}