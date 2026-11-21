var builder = WebApplication.CreateBuilder(args);

// Lecture 15: Add the Inventory.Api resource, server instrumentation and the same Jaeger OTLP exporter as Orders.
// Lecture 17: Add server and runtime metrics. Use the same Prometheus OTLP endpoint as Orders.

var app = builder.Build();

app.MapGet("/inventory/reserve", async (CancellationToken cancellationToken) =>
{
    await Task.Delay(150, cancellationToken);

    return Results.Ok(new
    {
        Reserved = true,
        Items = 3
    });
});

app.Run();
