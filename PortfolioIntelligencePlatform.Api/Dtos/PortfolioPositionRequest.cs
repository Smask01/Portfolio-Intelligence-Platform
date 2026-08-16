namespace PortfolioIntelligencePlatform.Api.Dtos;

public record PortfolioPositionRequest
{
    public required string Ticker { get; init; }
    public required decimal AmountInvested { get; init; }
}