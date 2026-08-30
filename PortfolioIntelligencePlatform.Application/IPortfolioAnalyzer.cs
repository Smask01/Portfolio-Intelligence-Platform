using PortfolioIntelligencePlatform.Domain;

namespace PortfolioIntelligencePlatform.Application;

public interface IPortfolioAnalyzer
{
    IReadOnlyCollection<HoldingExposure> CalculateExposure(IReadOnlyCollection<PortfolioPosition> positions, IReadOnlyCollection<Etf> etfs, IReadOnlyCollection<Stock> stocks);

    IReadOnlyCollection<SectorExposure> CalculateSectorExposure(IReadOnlyCollection<PortfolioPosition> positions, IReadOnlyCollection<Etf> etfs, IReadOnlyCollection<Stock> stocks);
}