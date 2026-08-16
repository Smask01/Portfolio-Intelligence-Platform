namespace PortfolioIntelligencePlatform.Api.Dtos;

public record HoldingExposureResponse
{
    public required string Symbol { get; init; }
    public required string CompanyName { get; init; }
    public decimal AmountExposed { get; init; }
    public decimal PortfolioPercentage { get; init; }
}