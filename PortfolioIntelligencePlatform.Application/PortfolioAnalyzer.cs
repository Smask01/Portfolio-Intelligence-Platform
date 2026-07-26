using PortfolioIntelligencePlatform.Domain;

namespace PortfolioIntelligencePlatform.Application;

public class PortfolioAnalyzer : IPortfolioAnalyzer
{
    public IReadOnlyCollection<HoldingExposure> CalculateExposure(IReadOnlyCollection<PortfolioPosition> positions, IReadOnlyCollection<Etf> etfs)
    {
        throw new NotImplementedException();
    }
}