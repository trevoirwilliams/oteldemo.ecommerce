using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Inventory.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options => options.RecordException = true)
        .AddOtlpExporter(options =>
        {
            options.Protocol = OtlpExportProtocol.Grpc;
            options.Endpoint = new Uri(builder.Configuration["Telemetry:TracesEndpoint"]
                ?? "http://127.0.0.1:4317");
        }));

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
