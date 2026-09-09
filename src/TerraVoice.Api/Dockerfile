# -------------------------
# Build stage (SDK image)
# -------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/TerraVoice.Api/TerraVoice.Api.csproj", "src/TerraVoice.Api/"]
RUN dotnet restore "src/TerraVoice.Api/TerraVoice.Api.csproj"
COPY . .
RUN dotnet publish "src/TerraVoice.Api/TerraVoice.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -------------------------
# Runtime stage (ASP.NET runtime)
# -------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# HEALTHCHECK needs curl; the aspnet image doesn't ship with it.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -fsS http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "TerraVoice.Api.dll"]
