namespace PortfolioIntelligencePlatform.Domain;

public record Etf
{
    public required string Ticker { get; init; }
    public required string Name { get; init; }
    public IReadOnlyCollection<EtfHolding> Holdings { get; init; } = [];
    public IReadOnlyCollection<SectorAllocation> SectorAllocations { get; init; } = [];
}