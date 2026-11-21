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

var app = builder.Build();

app.MapGet("/checkout", async (IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
{
    const string userTier = "gold";
    const decimal orderTotal = 129.99m;

    var client = httpClientFactory.CreateClient("inventory");
    using (var response = await client.GetAsync("/inventory/reserve", cancellationToken))
    {
        response.EnsureSuccessStatusCode();
    }

    // This delay represents a database save in the checkout exercise.
    await Task.Delay(saveDelayMs, cancellationToken);

    return Results.Ok(new
    {
        Status = "Checkout complete",
        UserTier = userTier,
        Total = orderTotal
    });
});

app.Run();
