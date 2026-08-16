namespace PortfolioIntelligencePlatform.Api.Dtos;

public record AnalyzePortfolioRequest
{
    public required IReadOnlyCollection<PortfolioPositionRequest> Positions { get; init; }
}