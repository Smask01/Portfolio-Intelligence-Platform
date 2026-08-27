using System.Text.Json.Serialization;

namespace PortfolioIntelligencePlatform.Infrastructure.AlphaVantage;

public record AlphaVantageSector
{
    [JsonPropertyName("sector")]
    public required string Sector { get; init; }

    [JsonPropertyName("weight")]
    public required string Weight { get; init; }
}