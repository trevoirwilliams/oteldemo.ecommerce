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

See [the course index](https://github.com/trevoirwilliams/oteldemo.ecommerce#checkpoints) for intermediate lesson checkpoints. To compare without changing your working branch, open the checkpoint on GitHub.
