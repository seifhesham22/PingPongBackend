# PingPong.API

A chat backend built with ASP.NET Core (.NET 10), EF Core and PostgreSQL, using ASP.NET Identity for authentication.

## Prerequisites

| Requirement | Version | Notes |
| --- | --- | --- |
| .NET SDK | 10.0 | `dotnet --version` |
| PostgreSQL | 14+ | Local install or Docker |
| `dotnet-ef` | 10.x | `dotnet tool install --global dotnet-ef` |

## Setup

### 1. Clone and restore

```bash
git clone https://github.com/seifhesham22/PingPongBackend.git
cd PingPong.API
dotnet restore
```

### 2. Start PostgreSQL

If you don't have a local instance, Docker is the quickest option:

```bash
docker run --name pingpong-db -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16
```

### 3. Configure secrets

`appsettings.json` ships with an **empty** connection string, and the mail password should never be committed. Both live in user-secrets instead:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=pingpong;Username=postgres;Password=postgres"
dotnet user-secrets set "MailSettings:Password" "<your-smtp-password>"
```

The remaining `MailSettings` keys (`Host`, `Port`, `UserName`, `FromName`) are already in `appsettings.json`. Email sending is only used for confirmation and password-reset messages — the API will start without a valid password, but those endpoints will fail.

### 4. Apply migrations

```bash
dotnet ef database update
```

This creates the schema from `Migrations/20260718110601_initial.cs`.

### 5. Run

```bash
dotnet run --launch-profile https
```

| URL | |
| --- | --- |
| https://localhost:7008 | HTTPS |
| http://localhost:5148 | HTTP |
| https://localhost:7008/swagger | Swagger UI (opens automatically) |

Swagger is only mapped in the Development environment, which the `https` launch profile sets for you.

## Endpoints

All identity endpoints are grouped under `/auth`:

| Endpoint | Description |
| --- | --- |
| `POST /auth/register` | Register a new user |
| `POST /auth/login` | Log in, returns a bearer token |
| `POST /auth/forgotPassword` | Send a password reset code by email |
| `POST /auth/resetPassword` | Reset password using the code |
| `GET /auth/check2fa?email=` | Returns `enabled` / `not enabled` for a user |

`/auth/register`, `/auth/login` and the rest come from ASP.NET Identity's `MapIdentityApi<User>()`; browse Swagger for the full list.

## Running with Docker

The included `Dockerfile` builds the API only — it expects a reachable Postgres instance and the connection string passed in:

```bash
docker build -t pingpong-api .
docker run -p 8080:8080 -p 8081:8081 \
  -e "ConnectionStrings__DefaultConnection=Host=host.docker.internal;Port=5432;Database=pingpong;Username=postgres;Password=postgres" \
  pingpong-api
```

## Project structure

```
Data/         PingPongDbContext and EF configuration
Domain/       Entities: User, Chat, ChatMember, Message, Role, FriendShip
Features/
  Authentication/   IdentityEmailSender (MailKit)
  Shared/           Result<T> and Error types
Migrations/   EF Core migrations
```

## Branches

| Branch | Purpose |
| --- | --- |
| `master` | Stable |
| `dev` | Integration branch — features merge here first |
| `feature/*` | Individual features, branched from `dev` |

## Troubleshooting

**`dotnet ef` not found** — install it globally: `dotnet tool install --global dotnet-ef`

**`Npgsql.NpgsqlException: Connection refused`** — Postgres isn't running, or the connection string in user-secrets points somewhere else. Verify with `dotnet user-secrets list`.

**`ArgumentNullException` on the connection string** — `DefaultConnection` is empty in `appsettings.json` by design; it must be set in user-secrets (step 3).

**HTTPS certificate warning on first run** — trust the dev certificate: `dotnet dev-certs https --trust`
