using ChatbotService.Models.Responses;
using Prometheus;

// Minimal-API-Endpunkt für einen Hello-World-Service
var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Logging.AddDebug();

// HuggingFace API-Konfiguration
var hfApiKey = builder.Configuration["HuggingFace:ApiKey"];
const string baseAdress = "https://api-inference.huggingface.co/models/";
const string modelName = "mistralai/Mixtral-8x7B-Instruct-v0.1";

builder.Services.AddHttpClient("HuggingFace", client =>
{
    client.BaseAddress = new Uri(baseAdress);
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hfApiKey);
});

var app = builder.Build();

// Health Check Endpoints
app.MapGet("/liveness", () => Results.Ok());
app.MapGet("/readiness", (IConfiguration config) =>
{
    var apiKey = config["HuggingFace:ApiKey"];
    return string.IsNullOrEmpty(apiKey)
        ? Results.StatusCode(503) // Service Unavailable wenn der API-Key fehlt
        : Results.Ok();
});

// Endpoint-Routes
app.MapGet("/hello", () => "World!");
app.MapGet("/env", () => app.Environment.EnvironmentName);
app.MapGet("/chat", async (string message, IHttpClientFactory clientFactory, ILogger<Program> logger) =>
{
    logger.LogInformation("Chat request received: {Message}", message);
    try
    {
        var client = clientFactory.CreateClient("HuggingFace");

        var payload = new
        {
            inputs = message,
            parameters = new
            {
                max_length = 50, // maximale Antwortlänge
                temperature = 1.0 // kreativer und weniger faktenbasiert
            }
        };

        var response = await client.PostAsJsonAsync(modelName, payload);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Failed API call to HuggingFace. Status: {StatusCode}", response.StatusCode);
            return Results.Problem("Failed to get response from AI model");
        }

        Metrics.CreateCounter("chatbot_chat_requests_total", "Total chat requests",
                      new[] { "status_code", "endpoint" });

        var content = await response.Content.ReadFromJsonAsync<HuggingFaceResponse[]>();
        var result = content?.FirstOrDefault()?.generated_text?.Trim();

        logger.LogInformation("Chat response: {Response}", result);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing chat request");
        return Results.Problem($"Error: {ex.Message}");
    }
});

app.Run();