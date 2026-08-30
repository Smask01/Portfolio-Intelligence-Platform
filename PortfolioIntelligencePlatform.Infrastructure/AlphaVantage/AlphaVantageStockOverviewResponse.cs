namespace PortfolioIntelligencePlatform.Infrastructure.AlphaVantage;

public record AlphaVantageStockOverviewResponse
{
    public string Symbol { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Sector { get; init; } = string.Empty;
}