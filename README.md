# Trace a checkout across two APIs

This lab follows a checkout from Orders to Inventory in lectures 15–21. Both APIs run on your machine. The save and inventory delays are simulated.

| Branch | When to use it |
| --- | --- |
| [`start`](https://github.com/trevoirwilliams/oteldemo.ecommerce/tree/start) | Run the sample before adding telemetry. |
| [`practice`](https://github.com/trevoirwilliams/oteldemo.ecommerce/tree/practice) | Work through the lesson-numbered TODOs. |
| [`final`](https://github.com/trevoirwilliams/oteldemo.ecommerce/tree/final) | Run the completed lab or compare it with your work. |

Install the current stable [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) and run `dotnet --version` from the repo. Use `10.0.400` or a later stable .NET 10 SDK. Any C# editor works. The `.http` files are ready for an editor with an HTTP client extension. Docker with Compose is needed when you send telemetry to the local backends.

## Run the APIs

```sh
# Terminal 1
dotnet run --project Inventory.Api --launch-profile http

# Terminal 2
dotnet run --project Orders.Api --launch-profile http
```

Send `GET http://127.0.0.1:5089/checkout`. It calls `GET http://127.0.0.1:5090/inventory/reserve` before returning the checkout response. Use the request files in each project to try both endpoints.

## Follow the videos

- **15–16** Open both `.csproj` files and both `Program.cs` files. Add Hosting, OTLP and ASP.NET Core instrumentation to both APIs. Add HTTP instrumentation to Orders. Use stable `1.21.0` for each OpenTelemetry package. Follow the TODOs to add tracing and run [the Jaeger check](#check-the-checkout-trace).
- **17** Add Runtime instrumentation at `1.21.0` to both projects. Collect ASP.NET Core and runtime metrics in both APIs and HTTP client metrics in Orders. Complete the Compose TODO and use the [metrics setup](#metrics-setup-for-lecture-17).
- **18–21** Use [the query and Grafana walkthrough](OBSERVABILITY.md) to generate traffic, interpret the results and write a short incident timeline.

## Check the checkout trace

Start Jaeger with `docker compose up -d jaeger`. Restart both APIs with tracing configured, then send `GET /checkout`. In [Jaeger](http://127.0.0.1:16686), search for service `Orders.Api` and set the minimum duration to `500ms`. Select the `GET /checkout` operation if you want to narrow the search. The recorded code may show `ProcessCheckout` as the operation name.

Expand `CallInventory` to see the HTTP client span and the Inventory server span in the same trace. `SaveOrderToDb` should account for about 600 ms. Inventory contributes about 150 ms plus HTTP overhead.

## Metrics setup for lecture 17

The video shows a prerelease exporter and one scrape target per API. In this repo, enable the Prometheus OTLP receiver with `--web.enable-otlp-receiver` in Compose. In both APIs use the metrics version of `AddOtlpExporter` with `OtlpExportProtocol.HttpProtobuf` and the full endpoint `http://127.0.0.1:9090/api/v1/otlp/v1/metrics`. Set the periodic export interval to 5000 milliseconds for the lab. Skip the scrape targets and the `AddPrometheusExporter` and `MapPrometheusScrapingEndpoint` calls in the video.

Run `docker compose up -d` and restart both APIs. Send several checkout requests and open [Prometheus](http://127.0.0.1:9090). Run `http_server_request_duration_seconds_count{job=~"Orders[.]Api|Inventory[.]Api"}` to confirm that both jobs have request metrics. Verify the API series in the query view. The Targets page shows scrape health.

[Generate traffic, run PromQL and set up Grafana](OBSERVABILITY.md). When you finish, run `docker compose down`.


See [the course index](https://github.com/trevoirwilliams/oteldemo.ecommerce#checkpoints) for intermediate lesson checkpoints. To compare without changing your working branch, open the checkpoint on GitHub.
