using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Domain;
using PortfolioIntelligencePlatform.Infrastructure.AlphaVantage;

namespace PortfolioIntelligencePlatform.Infrastructure;

public class AlphaVantageStockDataProvider : IStockDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly AlphaVantageOptions _options;
    private readonly IMemoryCache _cache;

    private readonly AlphaVantageRateLimiter _rateLimiter;

    public AlphaVantageStockDataProvider(HttpClient httpClient, IOptions<AlphaVantageOptions> options, IMemoryCache cache, AlphaVantageRateLimiter rateLimiter)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _rateLimiter = rateLimiter;
        _cache = cache;
    }

    public async Task<Stock?> GetStockAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var cacheKey = $"stock:{normalizedSymbol}";

        if (_cache.TryGetValue(cacheKey, out Stock? cachedStock))
        {
            return cachedStock;
        }

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

        var stock = new Stock
        {
            Symbol = normalizedSymbol,
            Name = overview.Name,
            Sector = overview.Sector
        };
        
        _cache.Set(cacheKey, stock, TimeSpan.FromMinutes(30));
        
        return stock;
    }
}