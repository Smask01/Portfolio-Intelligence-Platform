namespace PortfolioIntelligencePlatform.Api.Dtos;

public record SectorExposureResponse
{
    public required string Sector { get; init; }
    public decimal AmountExposed { get; init; }
    public decimal PortfolioPercentage { get; init; }
}