using System.Text.Json.Serialization;
using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPortfolioAnalyzer, PortfolioAnalyzer>();
builder.Services.AddScoped<EtfOverlapCalculator>();

builder.Services.Configure<AlphaVantageOptions>(builder.Configuration.GetSection("AlphaVantage"));
builder.Services.AddHttpClient<IEtfDataProvider, AlphaVantageEtfDataProvider>();
builder.Services.AddHttpClient<IStockDataProvider, AlphaVantageStockDataProvider>();

builder.Services.AddSingleton<AlphaVantageRateLimiter>();

builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "https://portfolio-intelligence-platform-i5h5.onrender.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature =
            context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        if (exceptionFeature != null)
        {
            app.Logger.LogError(
                exceptionFeature.Error,
                "Unhandled exception while processing request");
        }

        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An internal server error occurred.");
    });
});

app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();