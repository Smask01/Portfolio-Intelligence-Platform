using System.Text.Json.Serialization;

namespace PortfolioIntelligencePlatform.Infrastructure.AlphaVantage;

public record AlphaVantageHolding()
{
    [JsonPropertyName("symbol")]
    public required string Symbol { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("weight")]
    public required string Weight { get; init; }
}