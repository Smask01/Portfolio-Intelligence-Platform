namespace PortfolioIntelligencePlatform.Api.Dtos;

public record EtfOverlapResponse
{
    public required string FirstTicker { get; init; }
    public required string SecondTicker { get; init; }
    public decimal Overlap { get; init; }
}