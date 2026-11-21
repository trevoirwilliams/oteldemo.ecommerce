# PromQL and Grafana for the checkout lab

Use this walkthrough for lectures 18–21. Start both instrumented APIs and the Compose stack. Each API sends OTLP metrics to Prometheus roughly every five seconds. In Prometheus, the OTLP resource name is available as the `job` label.

## Generate enough traffic for the charts

Run one checkout first and confirm it returns HTTP 200. For a repeatable graph, send requests for about a minute and look at the last five minutes. The following examples use Bash or PowerShell 7. Each starts four local request loops.

```sh
for worker in 1 2 3 4; do
  (for i in $(seq 1 60); do
    curl -fsS http://127.0.0.1:5089/checkout >/dev/null || exit 1
  done) &
done
wait
```

```powershell
1..4 | ForEach-Object -Parallel {
    1..60 | ForEach-Object {
        Invoke-RestMethod http://127.0.0.1:5089/checkout | Out-Null
    }
} -ThrottleLimit 4
```

To see HTTP connection queueing under this load, restart Orders with `Inventory__MaxConnectionsPerServer` set to `1` for the lab. In Bash use `Inventory__MaxConnectionsPerServer=1 dotnet run --project Orders.Api --launch-profile http`. In PowerShell set `$env:Inventory__MaxConnectionsPerServer = '1'` in the Orders terminal before running it. Remove the setting before measuring the default checkout again.

## Compare the series

Check that both APIs are sending data by running the following in [Prometheus](http://127.0.0.1:9090).

```promql
sum by (job, http_route) (
  http_server_request_duration_seconds_count{job=~"Orders[.]Api|Inventory[.]Api"}
)
```

To graph total time spent waiting for an available outbound HTTP connection per second, run the following query. For mean wait in seconds, divide by `sum(rate(http_client_request_time_in_queue_seconds_count{job="Orders.Api"}[2m]))` over the same window and use the ratio only where that denominator is positive.

```promql
sum(rate(http_client_request_time_in_queue_seconds_sum{job="Orders.Api"}[2m]))
```

Estimate server p95 for successful checkout requests. Keep `le` in the aggregation so Prometheus can read each cumulative histogram bucket. The result is in seconds. Remove the status filter to see how including other responses affects the query.

```promql
histogram_quantile(0.95,
  sum by (le) (
    rate(http_server_request_duration_seconds_bucket{
      job="Orders.Api", http_route="/checkout", http_response_status_code="200"
    }[2m])
  )
)
```

For the RED exercise, get checkout requests per second and the 5xx fraction. The fallback in the numerator represents zero reported 5xx only when the request series exists. Interpret the ratio only when total request rate is positive.

```promql
sum(rate(http_server_request_duration_seconds_count{
  job="Orders.Api", http_route="/checkout"
}[2m]))
```

```promql
(
  sum(rate(http_server_request_duration_seconds_count{
    job="Orders.Api", http_route="/checkout", http_response_status_code=~"5.."
  }[2m]))
  or
  0 * sum(rate(http_server_request_duration_seconds_count{
    job="Orders.Api", http_route="/checkout"
  }[2m]))
)
/
sum(rate(http_server_request_duration_seconds_count{
  job="Orders.Api", http_route="/checkout"
}[2m]))
```

## Compare the panel with a trace

Compose pins Jaeger to `2.20.0` so Grafana’s Jaeger data source can use the query API it expects.

1. Open [Grafana](http://127.0.0.1:3000). The default credentials for this local instance are `admin` and `admin`. Follow the password prompt.
2. Under **Connections**, add Prometheus with URL `http://prometheus:9090` and Jaeger with URL `http://jaeger:16686`. Run **Save & test** for both.
3. Create a Prometheus time-series panel and paste the checkout p95 query. Pick a time range that includes the load run and set the unit to seconds.
4. Open **Explore**, select Jaeger, and search for `Orders.Api` and `GET /checkout` in the same time window. Inspect a representative trace and record its trace ID and the duration of `SaveOrderToDb`.

Write down the route, time window, p95, trace ID and slow span. To test whether the simulated save drove the latency, restart Orders with `Checkout__SaveDelayMs` set to `100`. Repeat the load and compare a fresh time window with your original one.
