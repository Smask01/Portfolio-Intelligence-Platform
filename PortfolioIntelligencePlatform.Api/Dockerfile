FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PortfolioIntelligencePlatform.Api/PortfolioIntelligencePlatform.Api.csproj", "PortfolioIntelligencePlatform.Api/"]
COPY ["PortfolioIntelligencePlatform.Application/PortfolioIntelligencePlatform.Application.csproj", "PortfolioIntelligencePlatform.Application/"]
COPY ["PortfolioIntelligencePlatform.Domain/PortfolioIntelligencePlatform.Domain.csproj", "PortfolioIntelligencePlatform.Domain/"]
COPY ["PortfolioIntelligencePlatform.Infrastructure/PortfolioIntelligencePlatform.Infrastructure.csproj", "PortfolioIntelligencePlatform.Infrastructure/"]
RUN dotnet restore "PortfolioIntelligencePlatform.Api/PortfolioIntelligencePlatform.Api.csproj"
COPY . .
WORKDIR "/src/PortfolioIntelligencePlatform.Api"
RUN dotnet build "./PortfolioIntelligencePlatform.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PortfolioIntelligencePlatform.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PortfolioIntelligencePlatform.Api.dll"]
