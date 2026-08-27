using System.Text.Json.Serialization;

namespace PortfolioIntelligencePlatform.Infrastructure.AlphaVantage;

public record AlphaVantageEtfProfileResponse()
{
    [JsonPropertyName("holdings")]
    public IReadOnlyCollection<AlphaVantageHolding> Holdings { get; init; } = [];
    
    [JsonPropertyName("sectors")]
    public IReadOnlyCollection<AlphaVantageSector> Sectors { get; init; } = [];
}