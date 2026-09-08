using TerraVoice.Api.Services;

namespace TerraVoice.Api.Endpoints;

public static class UsageEndpoints
{
    public static IEndpointRouteBuilder MapUsageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/usage", async (UsageService usageService) =>
        {
            var response = await usageService.GetCurrentMonthSummaryAsync();
            return Results.Ok(response);
        });

        return app;
    }
}