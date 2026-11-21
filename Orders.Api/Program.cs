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

// Lecture 15: Configure the Orders.Api resource, incoming and outgoing HTTP tracing,
// and OTLP/gRPC export. Declare an ActivitySource and subscribe to its name here.
// Lecture 17: Add HTTP server, HTTP client and runtime metrics. Send them to Prometheus using OTLP/HTTP.

var app = builder.Build();

app.MapGet("/checkout", async (IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
{
    const string userTier = "gold";
    const decimal orderTotal = 129.99m;

    // Lecture 15: Put the user tier and order total on the current server span.

    var client = httpClientFactory.CreateClient("inventory");
    // Lecture 15: Wrap the outgoing request in a CallInventory activity. Keep the HTTP client span beneath it.
    using (var response = await client.GetAsync("/inventory/reserve", cancellationToken))
    {
        response.EnsureSuccessStatusCode();
    }

    // This delay represents a database save in the checkout exercise.
    // Lecture 15: Wrap this delay in a SaveOrderToDb activity. Add database attributes to the span.
    await Task.Delay(saveDelayMs, cancellationToken);

    return Results.Ok(new
    {
        Status = "Checkout complete",
        UserTier = userTier,
        Total = orderTotal
    });
});

app.Run();
