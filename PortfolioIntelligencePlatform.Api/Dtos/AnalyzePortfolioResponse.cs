namespace PortfolioIntelligencePlatform.Api.Dtos;

public record AnalyzePortfolioResponse
{
    public required IReadOnlyCollection<HoldingExposureResponse> HoldingExposures { get; init; }
    public required IReadOnlyCollection<SectorExposureResponse> SectorExposures { get; init; }
    public required IReadOnlyCollection<EtfOverlapResponse> Overlaps { get; init; }
}