namespace PortfolioIntelligencePlatform.Domain;

public record EtfOverlapResult
{
    public required string FirstTicker { get; init; }
    public required string SecondTicker { get; init; }
    public decimal Overlap { get; init; }
}