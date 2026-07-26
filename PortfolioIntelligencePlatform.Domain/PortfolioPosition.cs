namespace PortfolioIntelligencePlatform.Domain;

public record PortfolioPosition
{
    public required string EtfTicker { get; init; }
    public required decimal AmountInvested { get; init; }
}