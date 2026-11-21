var builder = WebApplication.CreateBuilder(args);

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
