# DukaFlow — Phase 1 Backend

WhatsApp-first restaurant SaaS platform. This package contains the Phase 1 backend:
ASP.NET Core Web API (modular monolith) + SQL Server via EF Core, JWT authentication,
restaurant registration/login/profile.

## Projects

```
backend/
  DukaFlow.sln
  src/
    DukaFlow.Domain          Entities, enums, base types. No external dependencies.
    DukaFlow.Application     DTOs, service interfaces, exceptions.
    DukaFlow.Infrastructure  EF Core DbContext, entity configs, JWT/password services,
                              AuthService/RestaurantService implementations.
    DukaFlow.API             Controllers, Program.cs, middleware, Swagger.
tests/
  DukaFlow.UnitTests
  DukaFlow.IntegrationTests
```

## Setup (Visual Studio Community 2022)

1. Open `backend/DukaFlow.sln`.
2. Let VS restore NuGet packages (or right-click the solution → Restore NuGet Packages).
3. Set `DukaFlow.API` as the startup project.
4. Right-click `DukaFlow.API` → **Manage User Secrets** and set a real `Jwt:Key`
   (32+ random characters). Don't commit a real secret in `appsettings.json`.
5. Confirm the connection string in `appsettings.json` matches your local SQL Server
   instance (defaults to LocalDB — adjust if you use SQL Server Express/Developer
   edition with a named instance).
6. Open **Tools → NuGet Package Manager → Package Manager Console**.
   - Set **Default project** to `DukaFlow.Infrastructure`.
   - Run:
     ```powershell
     Add-Migration InitialCreate
     Update-Database
     ```
7. Press F5 (or Ctrl+F5) to run. Swagger opens automatically at `/swagger`.
8. In Swagger, try `POST /api/auth/register`, then use the returned `accessToken`
   with the "Authorize" button to call `GET /api/auth/me` and `GET /api/restaurants/me`.

## What's implemented

- User + Restaurant entities, one owner ↔ one restaurant
- Register / Login / Me
- JWT auth (Bearer) wired into Swagger
- Restaurant profile GET/PUT, scoped to the authenticated owner (no trusting client-supplied IDs)
- Centralized exception handling (Conflict / Unauthorized / NotFound → consistent JSON)
- Password hashing via `PasswordHasher<T>` (no custom crypto)
- CORS opened for `http://localhost:5173` (your Vite dev server)

## Not yet implemented

- Automated test coverage (stub projects included — see `tests/DukaFlow.IntegrationTests/README.md`)
- Anything from Phase 2 onward (menu, orders, WhatsApp, payments) — intentionally out of scope
