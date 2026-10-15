# ICMarkets.WEBAPI.NET.Project
Test task for ICMarkets .Net developer test task

Web API that periodically stores blockchain snapshots from [BlockCypher](https://www.blockcypher.com/dev/)
in PostgreSQL and returns their history. Redis coordinates instances: only one of them polls BlockCypher.

## How it works

**Startup.** Migrations are applied (if `Database:MigrateOnStartup` is on), then coins, chains and their
supported pairs are loaded from the database into an in-memory cache. This reference data rarely changes,
so requests never read it from the database.

**Data collection.** A background job polls BlockCypher for every supported pair at a fixed interval and saves
each response as a snapshot. UTXO chains (BTC-like) and EVM chains (Ethereum) report different fee data, so each
snapshot type stores its own fields. Requests to BlockCypher go through a rate limiter (at most N per interval)
and are retried on HTTP 429, honoring `Retry-After`.

**Several instances.** Only the instance holding a Redis lock polls; it keeps extending the lock while it runs.
The others wait. When the polling instance stops (including `docker stop` or a container restart), it releases
the lock and publishes a message via Redis Pub/Sub, and a waiting instance takes over right away. If the instance
dies without publishing (process killed), another one takes over once the lock expires.

**Requests.** Controllers send queries through [Mediator](https://github.com/martinothamar/Mediator).
A pipeline validator checks the query first (coin/chain pair supported, page size positive); an invalid request
gets `400` with `ProblemDetails`. History is paged by snapshot creation time, newest first.

**Logging.** Each request gets a correlation id: taken from the `X-Correlation-ID` header or generated, and
returned in the same header. It is attached to every log entry written during the request. Every request is
logged with its status code and duration, and every exception with the correlation id. Logs are written by
Serilog to the console as JSON with fixed fields (`@t` time, `@l` level, `@mt` message template, `@x` exception,
`CorrelationId`, `SourceContext`, `Application`, plus the message's own values).

**Layers.** `Domain` holds entities; `Application` holds queries, handlers, validators and interfaces;
`Infrastructure` implements them (EF Core + PostgreSQL, BlockCypher client, Redis, background jobs); `API` holds
controllers and middleware.

## Requirements

- .NET 8 SDK — for running locally and for tests
- Docker with Docker Compose — for PostgreSQL and Redis, and for running in containers

## Run locally

1. Start PostgreSQL and Redis:
   ```sh
   docker compose up -d postgres redis
   ```
2. Run the API (Development environment: migrations are applied on startup):
   ```sh
   dotnet run --project src/API --launch-profile http
   ```
3. Open Swagger UI: http://localhost:5273/swagger

Connection strings for this setup are in `src/API/appsettings.Development.json`.

## Run in Docker

```sh
docker compose up -d --build
```

Swagger UI: http://localhost:8080/swagger. Stop with `docker compose down` (add `-v` to also delete the database).

To build only the image (from the repository root):
```sh
docker build -f src/API/Dockerfile -t icmarkets-api .
```

## Configuration

| Setting | Description |
|---|---|
| `ConnectionStrings:Blockchain` | PostgreSQL connection string (required) |
| `ConnectionStrings:Redis` | Redis connection string (required) |
| `Database:MigrateOnStartup` | Apply EF Core migrations on startup; `true` in Development |
| `dataProvider:BaseUrl` | BlockCypher API address (required) |
| `dataProvider:rateLimitRequests`, `dataProvider:rateLimitIntervalMilliseconds` | At most N requests to BlockCypher per interval |
| `dataProvider:periodicDataRequestEnabled` | Turns the periodic polling on or off |
| `dataProvider:periodicDataRequestIntervalSeconds` | Polling interval |
| `dataProvider:periodicDataRequestLockSeconds` | Lifetime of the Redis lock; another instance takes over after it if the polling one dies |
| `Cors:*` | Allowed origins, methods and headers |
| `LogFormat` | `Json` (one JSON object per line, for log collectors) or `Text`; `Text` in Development, `Json` in Docker Compose |
| `Serilog:MinimumLevel` | Log levels, overall and per category |

In environment variables use `__` instead of `:`, e.g. `ConnectionStrings__Redis=redis:6379`.

## API

| Request | Description |
|---|---|
| `GET /api/blockchain/coins` | Supported coins |
| `GET /api/blockchain/chain` | Supported chains |
| `GET /api/blockchain/pairs` | Supported coin/chain pairs; their `coin` and `chain` go into the history route |
| `GET /api/blockchain/{coin}/{chain}?pageSize=&latest=` | Snapshot history of a pair, newest first; for the next page pass `createdAt` of the last item as `latest` |

Every response has an `X-Correlation-ID` header; send your own in the request to have it used in the logs.
Examples are in `src/API/ICMarkets.API.http`.

## Tests

```sh
dotnet test
```

Integration tests start PostgreSQL and Redis in Docker (Testcontainers), so Docker must be running.
