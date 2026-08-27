# PingPong.API

A Discord-style chat backend: friends, real-time direct messages, and servers with
a role-based permission system.

Built with ASP.NET Core (.NET 10), EF Core, PostgreSQL, SignalR and MediatR.

Copyright (c) 2026 Seif Elmoazen. All rights reserved.
This source is published for viewing only. No permission is granted to use, copy,
modify, or distribute it.

---

## What it does

- **Auth** — registration with email confirmation, login by email, password reset
- **Friends** — requests, accept/reject, unfriend, blocking; accepting a request
  opens the direct chat
- **Direct messages** — sent over HTTP, pushed to every device of both participants
  over SignalR.
- **Servers** — create, invite by link, join; channels and channel groups
- **Roles** — a rank-ordered permission system with an implicit `@everyone` role

## Architecture and Patterns used

**Vertical slices.** Each feature is one folder under `Features/` holding a single
static class that holds everything it needs: the MediatR request, its handler, the
DTOs, and the route registration. with no repository or service layer.
and business rules live in the domain entities.
For more information about vertical slice architecture, check it's founders (Jimmy Bogard) repository.[VSA Architecture Explanation](https://github.com/jbogard/presentations/tree/master/VerticalSliceArchitectures), [VSA code samples](https://dev.to/cristiansifuentes/vertical-slice-architecture-in-net-from-n-tier-layers-to-feature-slices-4iha)


**Results, not exceptions.** Handlers use result pattern and a RFC 7807 problem response returned in case error ocuured. Domain rule violations throw `DomainException`, which
handlers catch and translate.


**Rank-based permissions.** Roles carry a 16 flag permission bitfield and a position
that is unique per server. A member's permissions are the union of their roles, and
their rank is the highest position they hold. The rule everything derives from: you
may only act on a role or member ranked strictly below you, and you may never grant a
permission you do not hold yourself.

**Note.** The permission system used in the app is simulating the same permission system in discord, for more info about it [click here](https://docs.discord.com/developers/topics/permissions)


---
### project structure
```
Data/         PingPongDbContext and EF configuration
Domain/       Entities and the rules that guard them
Features/     One folder per feature, plus Shared/ folder for Result, Error and permissions
Migrations/   EF Core migrations
tests/        Domain and handler tests
```

---

## Running it

### With Docker

Brings up Postgres and the API together.

```bash
git clone https://github.com/seifhesham22/PingPongBackend.git
cd PingPong.API
cp .env.example .env
docker compose up --build
```

Fill in the mail settings in `.env` if you want confirmation and password-reset
emails to send; everything else has working defaults. Then create the schema:

```bash
dotnet ef database update
```

The API is on `http://localhost:8080`, Swagger at `/swagger`.

### Locally

Needs the .NET 10 SDK, a PostgreSQL 14+ instance, and
`dotnet tool install --global dotnet-ef`.

```bash
dotnet restore

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=pingpong;Username=postgres;Password=postgres"
dotnet user-secrets set "MailSettings:Password" "<your-smtp-password>"

dotnet ef database update
dotnet run --launch-profile https
```

Swagger opens automatically at `https://localhost:7008/swagger`.

### Tests

```bash
dotnet test
```
---

## Not implemented

Modeled in the domain but without endpoints: reactions, delete,
replies, attachments in messages, unread counts and
push notifications.

Permission checks are enforced on the role endpoints; other server endpoints do not
yet consult them.
