namespace PortfolioIntelligencePlatform.Domain;

public record PortfolioPosition
{
    public required string Symbol { get; init; }
    public required decimal AmountInvested { get; init; }
}