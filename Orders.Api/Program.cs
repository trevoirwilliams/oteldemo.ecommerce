using System.Diagnostics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var inventoryUrl = builder.Configuration["Inventory:BaseUrl"] ?? "http://127.0.0.1:5090";
var inventoryClient = builder.Services.AddHttpClient("inventory", client =>
{
    client.BaseAddress = new Uri(inventoryUrl);
});

var maxConnections = builder.Configuration.GetValue<int?>("Inventory:MaxConnectionsPerServer");
if (maxConnections is > 0)
{
    inventoryClient.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        MaxConnectionsPerServer = maxConnections.Value
    });
}

var saveDelayMs = builder.Configuration.GetValue("Checkout:SaveDelayMs", 600);

const string activitySourceName = "Orders.Api";
using var activitySource = new ActivitySource(activitySourceName);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Orders.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options => options.RecordException = true)
        .AddHttpClientInstrumentation()
        .AddSource(activitySourceName)
        .AddOtlpExporter(options =>
        {
            options.Protocol = OtlpExportProtocol.Grpc;
            options.Endpoint = new Uri(builder.Configuration["Telemetry:TracesEndpoint"]
                ?? "http://127.0.0.1:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter((options, reader) =>
        {
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
            options.Endpoint = new Uri(builder.Configuration["Telemetry:MetricsEndpoint"]
                ?? "http://127.0.0.1:9090/api/v1/otlp/v1/metrics");
            reader.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 5000;
        }));

var app = builder.Build();

app.MapGet("/checkout", async (IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
{
    const string userTier = "gold";
    const decimal orderTotal = 129.99m;

    Activity.Current?.SetTag("user.tier", userTier);
    Activity.Current?.SetTag("order.total", (double)orderTotal);

    var client = httpClientFactory.CreateClient("inventory");
    using (activitySource.StartActivity("CallInventory"))
    {
        using var response = await client.GetAsync("/inventory/reserve", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    using (var saveActivity = activitySource.StartActivity("SaveOrderToDb"))
    {
        saveActivity?.SetTag("db.system.name", "postgresql");
        saveActivity?.SetTag("db.operation.name", "INSERT");
        // This delay represents a database save in the checkout exercise.
        await Task.Delay(saveDelayMs, cancellationToken);
    }

    return Results.Ok(new
    {
        Status = "Checkout complete",
        UserTier = userTier,
        Total = orderTotal
    });
});

app.Run();
