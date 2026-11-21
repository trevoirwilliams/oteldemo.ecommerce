# Trace a checkout across two APIs

Use this repo for lectures 15–21 of OpenTelemetry for .NET Developers. You’ll follow a checkout from Orders to Inventory, inspect the slow span in Jaeger, and compare the trace with latency in Grafana.

| Branch | When to use it |
| --- | --- |
| [`start`](https://github.com/trevoirwilliams/oteldemo.ecommerce/tree/start) | Run the sample before adding telemetry. |
| [`practice`](https://github.com/trevoirwilliams/oteldemo.ecommerce/tree/practice) | Work through the lesson-numbered TODOs. |
| [`final`](https://github.com/trevoirwilliams/oteldemo.ecommerce/tree/final) | Run the completed lab or compare it with your work. |

Install the current stable [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) and run `dotnet --version` from the repo. Use `10.0.400` or a later stable .NET 10 SDK. Any C# editor works. The `.http` files are ready for an editor with an HTTP client extension. Docker with Compose is needed when you send telemetry to the local backends.

```sh
git clone --branch practice https://github.com/trevoirwilliams/oteldemo.ecommerce.git
cd oteldemo.ecommerce
```

## Checkpoints

- [Lectures 15–16 · Distributed tracing before metrics](https://github.com/trevoirwilliams/oteldemo.ecommerce/tree/2f620c6bcb678249a4ef2ae56a18f116b005036d)
- [Lecture 17 · Stable metrics setup](https://github.com/trevoirwilliams/oteldemo.ecommerce/blob/final/README.md#metrics-setup-for-lecture-17)
- [Lectures 18–21 · PromQL and Grafana walkthrough](https://github.com/trevoirwilliams/oteldemo.ecommerce/blob/final/OBSERVABILITY.md)

If you’re still working through the single-API lessons, start with [oteldemo](https://github.com/trevoirwilliams/oteldemo).
