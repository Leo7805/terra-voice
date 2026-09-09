using TerraVoice.Api.Configuration;
using TerraVoice.Api.Data;
using TerraVoice.Api.Endpoints;
using TerraVoice.Api.Extensions;
using TerraVoice.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add/register a CORS service
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins) // limit the allowed source
            .AllowAnyHeader() // Allow all kinds of header
            .AllowAnyMethod(); // Allow all kinds of methods
    });
});

// Register services used to generate API metadata and Swagger UI.
builder.Services.AddEndpointsApiExplorer(); // Discovers Minimal API endpoints for OpenAPI.
builder.Services.AddSwaggerGen(); // Generates the Swagger JSON and Swagger UI. (Swashbuckle)

// Register configuration
builder.Services.Configure<TtsSettings>(builder.Configuration.GetSection("Tts"));
builder.Services.Configure<AzureSpeechSettings>(builder.Configuration.GetSection("AzureSpeech"));
builder.Services.Configure<UsageSettings>(builder.Configuration.GetSection("Usage"));

// Register DB Services
builder.Services.AddScoped<UsageDb>();
builder.Services.AddScoped<UsageService>();

// Register tts
builder.Services.AddScoped<TtsService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var usageDb = scope.ServiceProvider.GetRequiredService<UsageDb>();
    await usageDb.InitializeAsync();
}


app.UseCors("Frontend"); // Start the CORS service: Frontend

// Add Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

app.UseGlobalExceptionHandler();

app.MapGet("/", () => Results.Ok(new { message = "TerraVoice is running!" }));

app.MapTtsEndpoints();

app.MapUsageEndpoints();
app.MapHealthEndpoints();

app.Run();