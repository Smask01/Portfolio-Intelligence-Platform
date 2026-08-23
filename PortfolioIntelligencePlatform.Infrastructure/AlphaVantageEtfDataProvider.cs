using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Domain;
using Microsoft.Extensions.Options;

namespace PortfolioIntelligencePlatform.Infrastructure;

public class AlphaVantageEtfDataProvider : IEtfDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly AlphaVantageOptions _options;

    public AlphaVantageEtfDataProvider(HttpClient httpClient, IOptions<AlphaVantageOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
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

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        // Temporary: inspect Alpha Vantage's response first
        Console.WriteLine(json);

        return null;
    }
}