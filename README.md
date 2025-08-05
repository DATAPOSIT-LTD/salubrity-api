# Salubrity Backend – API Documentation

Salubrity is a scalable, clean-architecture-based .NET 9 backend for modern healthcare systems. This documentation explains how the backend works and how to extend it.

---

## Tech Stack

| Layer        | Tech                                      |
| ------------ | ----------------------------------------- |
| Language     | C# (.NET 9 / .NET Core)                   |
| Database     | PostgreSQL                                |
| ORM          | Entity Framework Core                     |
| Architecture | Clean Architecture / Domain-Driven Design |
| Auth         | JWT + RBAC                                |
| API Style    | RESTful + Swagger UI                      |
| Migrations   | EF Core CLI                               |
| Auditing     | `BaseAuditableEntity` on all entities     |

---

## Project Structure

```
Salubrity/
├── Salubrity.Domain/                 Core business entities
│   ├── Entities/
│   │   ├── Identity/                 User, Employee, Patient
│   │   ├── HealthCamps/             Health camp models
│   │   ├── Join/                    Many-to-many joins
│   │   ├── Lookups/                 Genders, BloodTypes, etc.
│   │   └── Common/                  BaseEntity, enums
│
├── Salubrity.Application/           DTOs, Interfaces, Services
│   ├── DTOs/                        Request/response models
│   ├── Interfaces/                  IService & IRepository contracts
│   └── Services/                    Core business logic
│
├── Salubrity.Infrastructure/        Data access & integration
│   ├── Persistence/                 DbContext and config
│   └── Repositories/                EF Core implementations
│
├── Salubrity.Api/                   Web API endpoints
│   ├── Controllers/                 RESTful controllers
│   └── Program.cs                   DI & Middleware
│
└── Migrations/                      EF Core migrations
```

---

## Getting Started

### 1. Clone the repo

```bash
git clone https://github.com/DATAPOSIT-LTD/salubrity-api.git
cd salubrity-api
```

### 2. Create the database

```sql
CREATE DATABASE salubrity;
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
```

### 3. Run migrations

```bash
dotnet ef database update
```

### 4. Start the API

```bash
dotnet run --project Salubrity.Api
```

Visit `https://localhost:{PORT}/docs` to test endpoints via Swagger.

---

## Polymorphic User Handling

A single `User` can map to:

- `Employee` — Internal staff
- `Patient` — Registered client
- `HealthCampParticipant` — Event attendee

Resolved via `RelatedEntityType` field + joins.

---

## Lookup Management

All lookups (e.g., Gender, Province, Insurance):

- Live under `Domain.Entities.Lookups`
- Served from `/api/v1/lookups/{type}`
- Use `BaseLookupResponse` DTOs

---

## Clean Architecture Summary

| Layer          | Purpose                       |
| -------------- | ----------------------------- |
| Domain         | Pure POCOs, no EF annotations |
| Application    | DTOs, interfaces, services    |
| Infrastructure | Persistence, integrations     |
| API            | Thin controller endpoints     |

---

## Developer Workflow

1. Define model in `Domain.Entities`
2. Create request/response DTOs
3. Add interface in `Application.Interfaces`
4. Implement logic in `Application.Services`
5. Wire up controller in `Api.Controllers`
6. Test via Swagger/Postman

---

## Common Commands

### Create migration

```bash
dotnet ef migrations add AddSomethingImportant
```

### Apply latest migrations

```bash
dotnet ef database update
```

### List PostgreSQL tables

```sql
\dt
```

### Example seed

```sql
INSERT INTO "Genders" ("Id", "Name", "Description", "CreatedAt", "IsDeleted")
VALUES
  (gen_random_uuid(), 'Male', 'Male gender', NOW(), FALSE),
  (gen_random_uuid(), 'Female', 'Female gender', NOW(), FALSE);
```

---

## Contribution Standards

- Always use DTOs — never expose EF entities
- Use `BaseAuditableEntity` on all tracked tables
- Keep controllers thin — no logic inside
- Responses wrapped with `ApiResponse<T>`
- Migrations must be tested
- Follow directory structure strictly

---

## Debugging & Support

- Swagger: `/docs`
- Seeds: `Migrations/Seeds/`
- Use `BaseController` as your controller scaffold
- Review existing services before adding new ones

---

## License

Maintained by DATAPOSIT LTD — internal use only unless explicitly authorized.
