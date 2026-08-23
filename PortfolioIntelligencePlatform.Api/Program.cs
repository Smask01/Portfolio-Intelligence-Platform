using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPortfolioAnalyzer, PortfolioAnalyzer>();
builder.Services.AddScoped<EtfOverlapCalculator>();
builder.Services.AddScoped<IEtfDataProvider, JsonEtfDataProvider>();

builder.Services.Configure<AlphaVantageOptions>(builder.Configuration.GetSection("AlphaVantage"));
builder.Services.AddHttpClient<AlphaVantageEtfDataProvider>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapGet("/test-alpha/{ticker}",
    async (
        string ticker,
        AlphaVantageEtfDataProvider provider,
        CancellationToken cancellationToken) =>
    {
        await provider.GetEtfAsync(ticker, cancellationToken);

        return Results.Ok();
    });

app.Run();