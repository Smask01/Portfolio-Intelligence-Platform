namespace PortfolioIntelligencePlatform.Domain;

public record SectorAllocation
{
    public required string Sector { get; init; }
    public decimal Weight { get; init; }
}