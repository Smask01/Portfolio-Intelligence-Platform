using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPortfolioAnalyzer, PortfolioAnalyzer>();
builder.Services.AddScoped<EtfOverlapCalculator>();

builder.Services.Configure<AlphaVantageOptions>(builder.Configuration.GetSection("AlphaVantage"));
builder.Services.AddHttpClient<IEtfDataProvider, AlphaVantageEtfDataProvider>();

builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();