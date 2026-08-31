using System.Text.Json;
using Microsoft.Extensions.Options;
using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Domain;
using PortfolioIntelligencePlatform.Infrastructure.AlphaVantage;

namespace PortfolioIntelligencePlatform.Infrastructure;

public class AlphaVantageStockDataProvider : IStockDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly AlphaVantageOptions _options;

    private readonly AlphaVantageRateLimiter _rateLimiter;

    public AlphaVantageStockDataProvider(HttpClient httpClient, IOptions<AlphaVantageOptions> options, AlphaVantageRateLimiter rateLimiter)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _rateLimiter = rateLimiter;
    }

    public async Task<Stock?> GetStockAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();

        var url =
            $"{_options.BaseUrl}/query" +
            $"?function=OVERVIEW" +
            $"&symbol={normalizedSymbol}" +
            $"&apikey={_options.ApiKey}";

        await _rateLimiter.WaitAsync(cancellationToken);

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var overview = await JsonSerializer.DeserializeAsync<AlphaVantageStockOverviewResponse>(stream, cancellationToken: cancellationToken);

        if (overview is null || string.IsNullOrWhiteSpace(overview.Symbol)) return null;

        return new Stock
        {
            Symbol = normalizedSymbol,
            Name = overview.Name,
            Sector = overview.Sector
        };
    }
}