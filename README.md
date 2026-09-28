# OhtohsBGList

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-data%20store-07405E?logo=sqlite&logoColor=white)
![OpenAPI](https://img.shields.io/badge/OpenAPI-Swagger-85EA2D?logo=swagger&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)

A REST API over a board game catalog domain that includes games, mechanics, categories, and publishers, plus the accounts and roles that govern who may change that catalog. The project is coded along *Building Web APIs with ASP.NET Core* (Valerio De Sanctis, Manning Publications, 2023), whose sample project, MyBGList, provides the domain model and the bulk of the endpoint design. OhtohsBGList follows that book's REST API chapters rather than reproducing them; the divergences are described in the section below.

## Overview

Every list and single-item response is wrapped in an `ApiResponse<T>` envelope carrying the payload, a set of HATEOAS `Link`s (self, next/previous for lists; self/update/delete for single items), and an open `Details` dictionary. List endpoints additionally return a `PagedResponse<T>` (`Items`, `PageNumber`, `PageSize`, `TotalCount`, and the derived `TotalPages`/`HasNextPage`/`HasPreviousPage`), and accept a shared `PagedRequest<T>` query object for paging, dynamic sorting (validated against `T`'s own properties), and a simple name filter.

Authentication and authorization run on ASP.NET Core Identity's own API endpoints (`AddIdentityApiEndpoints`) rather than hand-issued JWTs, backed by SQLite through Entity Framework Core. Two roles, `Moderator` and `Administrator`, gate catalog writes; a third, `PowerUser`, is declared for future use. A `ModeratorWithMobilePhone` policy additionally requires a verified phone number, sourced from a custom `ApiUserClaimsPrincipalFactory` claim. Account confirmation and password-reset emails are sent through MailKit, rendered from Scriban templates under `Assets/Email`.

The API is versioned by URL segment (`api/v{version}/...`), documented through XML doc comments surfaced in Swagger UI, and cached at two levels: the `AddResponseCaching` middleware, driven by `NoCache`/`Any-60` cache profiles on individual actions, and an application-level `IDistributedCache` (SQLite-backed, via `NeoSmart.Caching.Sqlite`) that short-circuits the database for cached list queries.

## Origin and Divergence from the Book

MyBGList (the book's project) and OhtohsBGList share the same catalog domain, the same paging/sorting/filtering request shape, and the same HATEOAS-style response envelope. Several areas were deliberately reworked rather than copied:

| Area | MyBGList (the book) | OhtohsBGList |
|---|---|---|
| Data store | SQL Server | SQLite |
| Authentication | Manually issued JWTs, with `Microsoft.AspNetCore.Identity` used only for user storage | ASP.NET Core Identity's own endpoints and token handling (`AddIdentityApiEndpoints`) |
| Distributed cache | SQL Server- or Redis-backed `IDistributedCache` | SQLite-backed `IDistributedCache` |
| Structured logging | Serilog, writing to a file, a SQL Server table, and Application Insights | The default `ILogger` providers |
| API documentation | Swagger-specific attributes and filters (`SwaggerOperation`, custom `IOperationFilter`/`ISchemaFilter` implementations) | XML doc comments, surfaced through Swashbuckle's `IncludeXmlComments` |
| Catalog lookup entities | Domains, Mechanics | Domains, Mechanics, Categories, Publishers |
| Email delivery | Not covered | MailKit/MimeKit SMTP sender with Scriban-templated confirmation and password-reset emails |
| GraphQL and gRPC | A HotChocolate GraphQL server and a gRPC service, both exposed alongside the REST API | Not ported; the project is REST-only |

Some of the book's own inconsistencies were kept rather than smoothed over, since they come from the source material rather than from a rework decision: deleting a mechanic requires only an authenticated user, while deleting a domain, category, publisher, or board game requires the `Administrator` role; the domain list endpoint alone returns `501 Not Implemented` for an invalid page size instead of the usual `400 Bad Request`, through a `ManualValidationFilterAttribute` that opts that one action out of automatic model-state validation; and only board games and mechanics cache their list queries in `IDistributedCache`, matching the book's own caching choices for those two entities.

## API Reference

All endpoints are versioned (`v1`) and return the `ApiResponse<T>` envelope described above, except where noted.

| Route prefix | Purpose |
|---|---|
| `/api/auth` | ASP.NET Core Identity's built-in endpoints: registration, login, token refresh, email confirmation, and password reset |
| `/api/v{version}/accounts` | Role and test-account seeding (`POST seed`, Administrator-only), and self-service phone number / username changes |
| `/api/v{version}/board-games` | Paged/sorted/filtered listing, single-item lookup, create, update, delete, and bulk CSV import of board games |
| `/api/v{version}/domains` | CRUD over board game domains (e.g. "Wargames", "Strategy Games") |
| `/api/v{version}/mechanics` | CRUD over board game mechanics (e.g. "Dice Rolling", "Tile Placement") |
| `/api/v{version}/categories` | CRUD over board game categories |
| `/api/v{version}/publishers` | CRUD over board game publishers |
| `/api/v{version}/auth/identity-info` | Diagnostic endpoint reporting the caller's claims, roles, and, given a `policy` query parameter, the result of evaluating that named authorization policy |
| `/error` | The exception handler's fallback target, mapped via `UseExceptionHandler("/error")` |

## Prerequisites

- .NET 10 SDK, to run the API directly
- Docker, to run the API and/or the Papercut SMTP test server as containers

Neither is strictly required on its own: the API runs with `dotnet run` against a host-installed SDK, or entirely through `docker compose`, without SQLite requiring any separate installation in either case.

## Configuration

Configuration is read from `appsettings.json`, `appsettings.Development.json`, and user secrets.

| Section | Purpose |
|---|---|
| `ConnectionStrings:BgDbContext` | SQLite connection string for the application database |
| `SqlCache:Path` | File path for the SQLite-backed distributed cache |
| `Smtp` | `Host`, `Port`, `UseSsl`, `FromAddress`, `FromName` for outgoing Identity emails |
| `AllowedHosts` | Origins accepted by the default CORS policy |
| `UseDeveloperExceptionPage` | Toggles the developer exception page versus the `/error` handler |

## Running Locally

### Against the host SDK

Start the SMTP test server, then restore and run the API:

```bash
docker compose up -d papercut
dotnet restore
dotnet run --project src/OhtohsBGList
```

Alternatively, launch one of the profiles from `Properties/launchSettings.json`:

| Profile | URL(s) |
|---|---|
| `http` | `http://localhost:5272` |
| `https` | `https://localhost:7226` / `http://localhost:5272` |
| `IIS Express` | via IIS Express |

Each profile opens `/swagger` for interactive API documentation.

### Through Docker Compose

```bash
docker compose up -d --build
```

This builds the API image from the repository's `Dockerfile` and starts it alongside Papercut, reachable at `http://localhost:8081/swagger`. The API container bind-mounts `src/OhtohsBGList/BoardGames.db` (and its `-shm`/`-wal` companions) and `src/OhtohsBGList/App_Data`, so it reads and writes the same SQLite database and distributed cache the host SDK would use, and reaches Papercut over the compose network rather than `localhost`.

In either case, Papercut's web UI, at `http://localhost:8080`, shows any email the API sends (account confirmation, password reset) without delivering it anywhere real.

## Solution Structure

```
src/OhtohsBGList/
  Attributes/           Custom validation attributes and action-model conventions
  Constants/            Role names and configuration section keys
  Contracts/            Request/response DTOs and the paging/HATEOAS envelope types
    Account/
    BoardGames/
    Categories/
    Csv/
    Domains/
    Mechanics/
    Publishers/
  Controllers/          REST API controllers
  Data/
    Configurations/     EF Core Fluent API entity configurations
    Interceptors/       SaveChanges interceptor that stamps audit timestamps
    Migrations/         EF Core migrations
    Models/             Entity classes
  Endpoints/            Minimal API endpoints (identity diagnostics, error handler)
  Extensions/           IDistributedCache helper extensions
  Mappings/             Entity-to-DTO mapping extension methods
  Options/              Strongly-typed configuration options
  Services/             Identity claims factory and SMTP email sender
Assets/Email/            Scriban email templates
bgg_dataset.csv          BoardGameGeek dataset used for bulk import
Dockerfile               Multi-stage build for the API container
docker-compose.yml       API container plus the Papercut SMTP test server
```
