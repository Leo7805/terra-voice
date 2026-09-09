namespace TerraVoice.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        // Liveness: process is up. Never depends on DB / external services.
        app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));

        // Readiness: process can serve real traffic. Minimal check: SQLite is reachable.
        // A fuller version (DB query + AzureSpeech ping) lives in future §6.x work.
        app.MapGet("/health/ready", async (Data.UsageDb db) =>
        {
            var ok = await db.IsReadyAsync();
            return ok
                ? Results.Ok(new { status = "ready" })
                : Results.Json(new { status = "not_ready" }, statusCode: 503);
        });
    }
}