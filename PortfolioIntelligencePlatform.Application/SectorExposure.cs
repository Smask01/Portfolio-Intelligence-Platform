namespace PortfolioIntelligencePlatform.Application;

public record SectorExposure
{
    public required string Sector { get; init; }
    public decimal AmountExposed { get; init; }
    public decimal PortfolioPercentage { get; init; }
}