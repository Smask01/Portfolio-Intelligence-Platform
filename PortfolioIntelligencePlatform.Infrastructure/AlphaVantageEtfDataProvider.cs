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
    private static readonly SemaphoreSlim RateLimitLock = new(1, 1);
    private static DateTime _lastRequestTime = DateTime.MinValue;

    public AlphaVantageEtfDataProvider(HttpClient httpClient, IOptions<AlphaVantageOptions> options, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
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
        
        await RateLimitLock.WaitAsync(cxlToken);

        try
        {
            var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;

            if (timeSinceLastRequest < TimeSpan.FromSeconds(1))
            {
                await Task.Delay(TimeSpan.FromSeconds(1) - timeSinceLastRequest, cxlToken);
            }

            using var response = await _httpClient.GetAsync(url, cxlToken);

            _lastRequestTime = DateTime.UtcNow;

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cxlToken);

            var profile = await JsonSerializer.DeserializeAsync<AlphaVantageEtfProfileResponse>(stream, cancellationToken: cxlToken);

            if (profile is null || profile.Holdings.Count == 0)
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
        finally
        {
            RateLimitLock.Release();
        }
    }
}