using TerraVoice.Api.Dtos;
using TerraVoice.Api.Services;

namespace TerraVoice.Api.Endpoints;

public static class TtsEndpoints
{
    public static IEndpointRouteBuilder MapTtsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/tts",
            async (TtsRequest req, TtsService ttsService, HttpContext httpContext, UsageService usageService) =>
            {
                var audioData = await ttsService.SynthesizeAsync(req);

                // Prevent any caching (browser/CDN/proxies); ensure each request returns fresh audio
                httpContext.Response.Headers.CacheControl = "no-store";

                var charCount = req.Text?.Trim().Length ?? 0;
                await usageService.RecordAsync(charCount);

                return Results.File(
                    audioData,
                    "audio/mpeg"
                );
            });

        return app;
    }
}