using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Domain;
using Microsoft.Extensions.Options;
using PortfolioIntelligencePlatform.Infrastructure.AlphaVantage;

namespace PortfolioIntelligencePlatform.Infrastructure;

public class AlphaVantageEtfDataProvider : IEtfDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly AlphaVantageOptions _options;
    private readonly IMemoryCache _cache;
    private readonly AlphaVantageRateLimiter _rateLimiter;

    public AlphaVantageEtfDataProvider(HttpClient httpClient, IOptions<AlphaVantageOptions> options, IMemoryCache cache, AlphaVantageRateLimiter rateLimiter)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
        _rateLimiter = rateLimiter;
    }

    public async Task<Etf?> GetEtfAsync(string ticker, CancellationToken cxlToken = default)
    {
        var normalizedTicker = ticker.Trim().ToUpperInvariant();
        var cacheKey = $"etf:{normalizedTicker}";
        
        if (_cache.TryGetValue(cacheKey, out Etf? cachedEtf)) return cachedEtf;
        
        var url =
            $"{_options.BaseUrl}/query" +
            $"?function=ETF_PROFILE" +
            $"&symbol={normalizedTicker}" +
            $"&apikey={_options.ApiKey}";
        
        await _rateLimiter.WaitAsync(cxlToken);
        using var response = await _httpClient.GetAsync(url, cxlToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cxlToken);
        var profile = await JsonSerializer.DeserializeAsync<AlphaVantageEtfProfileResponse>(stream, cancellationToken: cxlToken);
        if (profile is null)
        {
            throw new InvalidOperationException($"Alpha Vantage returned an invalid response for {normalizedTicker}.");
        }

        if (!string.IsNullOrWhiteSpace(profile.ErrorMessage))
        {
            throw new InvalidOperationException($"Alpha Vantage error for {normalizedTicker}: {profile.ErrorMessage}"); 
        }

        if (!string.IsNullOrWhiteSpace(profile.Information))
        {
            throw new InvalidOperationException($"Alpha Vantage information: {profile.Information}"); 
        }
        if (!string.IsNullOrWhiteSpace(profile.Note))
        {
            throw new InvalidOperationException($"Alpha Vantage note: {profile.Note}");
        }

        if (profile.Holdings.Count == 0)
        {
            return null;
        }
        var etf = new Etf
        {
            Ticker = normalizedTicker,
            Name = normalizedTicker, // temporary
            Holdings = profile.Holdings
                .Where(x => !x.Symbol.Equals("n/a", StringComparison.OrdinalIgnoreCase))
                .Select(x => new EtfHolding
                {
                    Symbol = x.Symbol,
                    CompanyName = x.Description,
                    Sector = "Unknown",
                    Weight = decimal.Parse(x.Weight, CultureInfo.InvariantCulture)
                })
                .ToList(),

            SectorAllocations = profile.Sectors
                .Select(x => new SectorAllocation 
                {
                    Sector = x.Sector,
                    Weight = decimal.Parse(x.Weight, CultureInfo.InvariantCulture) 
                })
                .ToList()
        };

        _cache.Set(cacheKey, etf, TimeSpan.FromMinutes(30));
        return etf;
    }
}