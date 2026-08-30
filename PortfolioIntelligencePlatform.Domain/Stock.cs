namespace PortfolioIntelligencePlatform.Domain;

public record Stock
{
    public required string Symbol { get; init; }
    public required string Name { get; init; }
    public required string Sector { get; init; }
}