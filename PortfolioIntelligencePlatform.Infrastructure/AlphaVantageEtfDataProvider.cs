using System.Globalization;
using System.Text.Json;
using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Domain;
using Microsoft.Extensions.Options;
using PortfolioIntelligencePlatform.Infrastructure.AlphaVantage;

namespace PortfolioIntelligencePlatform.Infrastructure;

public class AlphaVantageEtfDataProvider : IEtfDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly AlphaVantageOptions _options;

    public AlphaVantageEtfDataProvider(HttpClient httpClient, IOptions<AlphaVantageOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        
        Console.WriteLine($"API key length: {_options.ApiKey.Length}");
    }

    public async Task<Etf?> GetEtfAsync(string ticker, CancellationToken cancellationToken = default)
    {
        var url =
            $"{_options.BaseUrl}/query" +
            $"?function=ETF_PROFILE" +
            $"&symbol={ticker}" +
            $"&apikey={_options.ApiKey}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var profile = await JsonSerializer.DeserializeAsync<AlphaVantageEtfProfileResponse>(stream, cancellationToken: cancellationToken);

        if (profile is null) return null;
        
        var etf = new Etf
        {
            Ticker = ticker.ToUpperInvariant(),
            Name = ticker.ToUpperInvariant(), // temporary
            Holdings = profile.Holdings
                .Select(x => new EtfHolding
                {
                    Symbol = x.Symbol,
                    CompanyName = x.Description,
                    Sector = "Unknown", // Alpha Vantage ETF_PROFILE doesn't give sector per holding
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

        return etf;
    }
}