using ChatbotService.Models.Responses;
using Prometheus;

// SLI/SLO Definitions
// SLI: Totale Anzahl Requests
// SLO: 99.9% Verf�gbarkeit in einem bestimmten Betrachtungszeitraum
var httpRequestsTotal = Metrics.CreateCounter(
    "http_requests_total",
    "Total number of HTTP requests",
    new CounterConfiguration
    {
        LabelNames = new[] { "method", "route", "status_code" }
    });

// SLI: Erfolgreiche Requests
// SLO: 95% der Requests m�ssen erfolgreich sein (HTTP 200) z.B. innerhalb von 24h
var successfulMessagesCounter = Metrics.CreateCounter(
    "chat_successful_messages_total",
    "Total number of successfully generated messages");

// SLI: Request-Dauer
// SLO: 90% der Requests unter z.B. 2 Sekunden Response-Time
var chatRequestDuration = Metrics.CreateHistogram(
    "chat_request_duration_seconds",
    "Duration of /chat requests in seconds",
    new HistogramConfiguration
    {
        Buckets = new[] { 0.1, 0.5, 1.0, 2.0, 5.0 }
    });

// Minimal-API-Endpunkt f�r einen Hello-World-Service
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

// Middleware f�r Request-Metriken
app.UseRouting();
app.Use(async (context, next) =>
{
    var start = DateTime.UtcNow;
    await next(context);
    var duration = DateTime.UtcNow - start;

    var method = context.Request.Method;
    var route = context.Request.Path.Value ?? "unknown";
    var statusCode = context.Response.StatusCode;

    // SLI: Total Requests erfassen
    httpRequestsTotal.WithLabels(method, route, statusCode.ToString()).Inc();

    // SLI: Request-Dauer nur f�r den /chat-Endpunkt
    if (route.StartsWith("/chat"))
    {
        chatRequestDuration.Observe(duration.TotalSeconds);
    }
});

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
    using var timer = chatRequestDuration.NewTimer();

    try
    {
        var client = clientFactory.CreateClient("HuggingFace");

        var payload = new
        {
            inputs = message,
            parameters = new
            {
                max_length = 50, // maximale Antwortl�nge
                temperature = 1.0 // kreativer und weniger faktenbasiert
            }
        };

        var response = await client.PostAsJsonAsync(modelName, payload);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Failed API call to HuggingFace. Status: {StatusCode}", response.StatusCode);
            return Results.Problem("Failed to get response from AI model");
        }

        var content = await response.Content.ReadFromJsonAsync<HuggingFaceResponse[]>();
        var result = content?.FirstOrDefault()?.generated_text?.Trim();

        logger.LogInformation("Chat response: {Response}", result);
        successfulMessagesCounter.Inc();
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing chat request");
        return Results.Problem($"Error: {ex.Message}");
    }
});

app.MapMetrics();
app.Run();