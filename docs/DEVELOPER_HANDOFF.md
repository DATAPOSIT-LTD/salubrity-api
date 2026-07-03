# Salubrity Developer Handoff

This document is the entry point for developers joining the project. Read it before touching any code.

---

## System Overview

Salubrity is a health-camp management platform. It manages the full lifecycle of corporate wellness camps:
- **Organizations** book camps with Salubrity
- **Subcontractors** (doctors, nurses, lab technicians) are assigned to service stations
- **Patients** (employees) register via QR code, join station queues, submit intake forms, and receive health reports

### Production URLs

| Service | URL |
|---------|-----|
| Admin/Staff Frontend | `https://app.salubritycentre.com` |
| Patient Frontend | `https://patient.salubritycentre.com` |
| Clinic Frontend | `https://clinic.salubritycentre.com` |
| API | `https://api-salubrity.dataposit.co.ke` |
| Swagger | `https://api-salubrity.dataposit.co.ke/swagger` |

### Server

**Host:** `172.31.10.155` (salubrity)  
**OS:** Ubuntu, managed via systemd + nginx + PM2

---

## Repository Structure

### Backend — `salubrity-api-working/`

```
salubrity-backend.sln
├── Salubrity.Domain/          Entity definitions, no business logic
│   └── Entities/              All EF Core entities (see DATABASE_SCHEMA.md)
├── Salubrity.Application/     Business logic, interfaces, DTOs, AutoMapper profiles
│   ├── Interfaces/            IRepository*, IService*
│   ├── Services/              Service implementations
│   └── DTOs/                  Data transfer objects
├── Salubrity.Infrastructure/  EF Core, repositories, migrations, external services
│   ├── Persistence/           AppDbContext, EF configs
│   ├── Repositories/          Concrete repository implementations
│   └── Migrations/            EF migration files
├── Salubrity.Shared/          Shared utilities, exceptions, response models
│   ├── Responses/             ApiResponse<T>, PagedResult<T>
│   └── Exceptions/            NotFoundException, ValidationException
└── Salubrity.Api/             ASP.NET Core API project
    ├── Controllers/           Grouped by domain
    ├── Middleware/            ExceptionHandlingMiddleware
    ├── HostedServices/        CampStatusReconcilerService
    └── Program.cs
```

**Architecture:** Clean Architecture (Domain → Application → Infrastructure → API)  
**Framework:** ASP.NET Core 9 (.NET 9)  
**Database:** EF Core 9, PostgreSQL 16  
**PDF generation:** QuestPDF (Community license)  
**Excel:** EPPlus (NonCommercial license)  
**Logging:** Serilog → Console  
**API versioning:** URL segment (`/api/v1/...`)

### Frontend — `salubrity-frontend/`

```
├── app/                  Next.js 16 App Router pages
├── components/           React components (admin, doctor, patient, concierge, UI)
├── hooks/                React Query hooks grouped by domain
├── lib/api/api.ts        Axios + fetch API client
├── stores/               Zustand stores (auth, etc.)
├── types/                TypeScript type definitions
└── utils/                Helpers
```

**Framework:** Next.js 16 (App Router)  
**State:** React Query v5 (server state), Zustand (client state)  
**HTTP:** Axios (via `lib/api/api.ts`) + native `fetch` (for blob downloads)  
**UI:** Tailwind CSS, Radix UI, Lucide icons  
**Forms:** React Hook Form + Zod  

---

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | 9.0.x | `dotnet --version` |
| Node.js | 18+ | `node --version` |
| npm | 10+ | `npm --version` |
| PostgreSQL | 16 | Local install or Docker |
| EF Core tools | latest | `dotnet tool install -g dotnet-ef` |

---

## Backend Setup (Local)

### 1. Clone and restore

```bash
git clone <repo-url>
cd salubrity-api-working
dotnet restore
```

### 2. Configure secrets

Copy the secrets template and fill in values. **Never commit real secrets.**

```bash
cp Salubrity.Api/appsettings.Development.json.example Salubrity.Api/appsettings.Development.json
```

Required values in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=salubrity_dev;Username=postgres;Password=yourpassword"
  },
  "JwtSettings": {
    "Issuer": "Salubrity",
    "Audience": "SalubrityClient",
    "Secret": "at-least-32-char-random-string-here"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your@email.com",
    "Password": "your-app-password",
    "FromEmail": "your@email.com",
    "FromName": "Salubrity Dev"
  },
  "Encryption": {
    "Key": "32-char-hex-key",
    "IV":  "16-char-hex-iv"
  },
  "BI": {
    "ApiKey": "any-string-for-local"
  }
}
```

> Gemini API key is **currently disabled** in production. Leave it empty for local dev — AI draft endpoints will fail gracefully.

### 3. Create the database

```bash
sudo -u postgres createdb salubrity_dev
sudo -u postgres psql -c "CREATE USER salubrity_user WITH PASSWORD 'yourpassword';"
sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE salubrity_dev TO salubrity_user;"
```

### 4. Run migrations

```bash
dotnet ef database update --project Salubrity.Infrastructure --startup-project Salubrity.Api
```

### 5. Seed RBAC defaults

After the API is running, call:

```
POST /api/v1/rbac/seed
```

This seeds default roles and permission groups.

### 6. Run the API

```bash
cd Salubrity.Api
dotnet run
```

API runs at `https://localhost:7000` (or check `launchSettings.json`).  
Swagger UI: `https://localhost:7000/swagger`

---

## Frontend Setup (Local)

### 1. Install dependencies

```bash
cd salubrity-frontend
npm install
```

### 2. Configure environment

```bash
cp .env.example .env.local
```

Edit `.env.local`:

```env
NEXT_PUBLIC_API_BASE_URL=https://localhost:7000
```

### 3. Run dev server

```bash
npm run dev
```

Frontend runs at `http://localhost:3000`.

---

## Production Deployment

### API

```bash
# On server: /srv/apps/salubrity-api/salubrity-api-working

# 1. Build
export PATH=$PATH:/usr/share/dotnet
dotnet build salubrity-backend.sln -c Release --no-restore

# 2. Publish
dotnet publish Salubrity.Api/Salubrity.Api.csproj -c Release -o /srv/apps/salubrity-api/publish --no-restore

# 3. Apply migrations (if any)
dotnet ef database update --project Salubrity.Infrastructure --startup-project Salubrity.Api

# 4. Restart service
sudo systemctl restart salubrity-api.service
sudo systemctl status salubrity-api.service
```

Or use the provided script:

```bash
bash deploy_and_migrate.sh
```

### Frontend

```bash
cd /srv/apps/salubrity-frontend
npm run build
pm2 restart salubrity-frontend
pm2 status
```

### Services

| Service | Manager | Command |
|---------|---------|---------|
| .NET API | systemd | `sudo systemctl restart salubrity-api.service` |
| Next.js | PM2 | `pm2 restart salubrity-frontend` |
| nginx | systemd | `sudo systemctl reload nginx` |
| PostgreSQL | systemd | `sudo systemctl status postgresql@16-main.service` |

### Environment file

API secrets live in `/etc/salubrity-api.env`. Format is `KEY=value`, one per line.  
Edit with: `sudo nano /etc/salubrity-api.env`  
After changes: `sudo systemctl restart salubrity-api.service`

---

## Logs

```bash
# API logs (last 100 lines, follow)
sudo journalctl -u salubrity-api.service -n 100 -f

# Frontend logs
pm2 logs salubrity-frontend

# nginx access/error
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log
```

---

## Architecture Decisions

### Why two separate "my camps" queries?
`GET /health-camps/my/upcoming` (filter: `!IsLaunched && StartDate >= today`) is shared by Admin, Concierge, and Doctor dashboards. The billing page needs a different filter (include launched camps, use EndDate), so it uses the dedicated `GET /health-camps/my/billing-camps` endpoint. **Do not change the upcoming endpoint filter** — it affects three dashboards simultaneously.

### Package assignment double-write
When a package is assigned to a participant, two writes happen:
1. `HealthCampParticipantPackages` — the junction table (supports future history)
2. `HealthCampParticipants.HealthCampPackageId` — the FK on the main row (allows participant list queries to join package directly)

Both must stay in sync. The junction table is the source of truth; the FK is a denormalized convenience.

### Cross-camp form response scoping
`IntakeFormResponses.HealthCampId` is the canonical scope column. It prevents a patient's lab result from appearing in reports for every camp that has the same service. Always populate `HealthCampId` when creating a new response. Legacy rows with `null` HealthCampId fall back to the old join-based filter.

### Self-assessment gate
If `HealthCamp.RequiresSelfAssessment = true`, a participant must submit all three self-assessment forms (Occupation Details, General Health History, Wellness and Lifestyle) before appearing in the triage queue (`GetCampParticipantsPagedByServiceAsync`). This is checked at query time — missing forms cause the participant to be excluded from the paged result.

### Timezone
All camp date logic is evaluated in EAT (Africa/Nairobi, UTC+3). The server clock is UTC. When comparing dates, convert `DateTime.UtcNow` using `TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi")` before comparing to date-only columns.

### BI endpoints
Power BI connects using `X-BI-Key` header, not JWT. The key is stored in `/etc/salubrity-api.env` as `BI__ApiKey`. These endpoints bypass normal auth entirely.

### Gemini AI
Gemini is used to generate draft text for doctor recommendation fields. The API key is stored in `/etc/salubrity-api.env` as `Gemini__ApiKey`. It is **currently disabled** (key commented out). To re-enable, uncomment the line and restart the API. Endpoints that call Gemini will return graceful errors when the key is absent.

---

## Common Tasks

### Add a new API endpoint

1. Define DTO(s) in `Salubrity.Application/DTOs/{Domain}/`
2. Add method signature to the relevant `IService` interface in `Salubrity.Application/Interfaces/Services/`
3. Add method signature to the relevant `IRepository` interface in `Salubrity.Application/Interfaces/Repositories/`
4. Implement in the repository (`Salubrity.Infrastructure/Repositories/`)
5. Implement in the service (`Salubrity.Application/Services/`)
6. Add the controller action (`Salubrity.Api/Controllers/`)
7. Build: `dotnet build salubrity-backend.sln -c Release --no-restore`

### Add a database column

1. Add the property to the entity in `Salubrity.Domain/Entities/`
2. Add any EF config in `Salubrity.Infrastructure/Persistence/Configurations/` if needed
3. Create migration: `dotnet ef migrations add MigrationName --project Salubrity.Infrastructure --startup-project Salubrity.Api`
4. Review the generated migration file before applying
5. Apply: `dotnet ef database update --project Salubrity.Infrastructure --startup-project Salubrity.Api`

### Add a frontend hook

```typescript
// hooks/camps/useMyNewEndpoint.ts
import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api/api";
import { useSignInStore } from "@/stores/auth/useSignInStore";

export const useMyNewEndpoint = (campId: string | undefined) => {
  const { accessToken } = useSignInStore();

  return useQuery({
    queryKey: ["my-new-endpoint", campId],
    queryFn: async () => {
      const res = await api.get(`/api/v1/health-camps/${campId}/my-data`, {
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      return res.data.data;
    },
    enabled: !!campId && !!accessToken,
  });
};
```

### Download a PDF (blob request)

Use `api.getBlob()` rather than `api.get()` — it reads the token from localStorage directly and returns a raw `Blob`:

```typescript
const blob = await api.getBlob(`/api/v1/reports/individual-final/${participantId}/pdf`);
const url = window.URL.createObjectURL(blob);
const a = document.createElement("a");
a.href = url;
a.download = "report.pdf";
a.click();
window.URL.revokeObjectURL(url);
```

**Important:** Never pass `?day=Any` or other "Any"-sentinel values to `int?` query parameters — the API will return 400. Always guard with `if (day && day !== "Any")`.

---

## Key Files Reference

| File | Purpose |
|------|---------|
| `Salubrity.Api/Program.cs` | DI registration, middleware pipeline |
| `Salubrity.Infrastructure/Persistence/AppDbContext.cs` | EF DbContext |
| `Salubrity.Infrastructure/Persistence/Configurations/` | EF Fluent API configs |
| `Salubrity.Application/Services/HealthCamps/HealthCampService.cs` | Core camp business logic |
| `Salubrity.Infrastructure/Repositories/HealthCamps/HealthCampRepository.cs` | Camp queries |
| `Salubrity.Infrastructure/Repositories/Reporting/` | Report data aggregation |
| `Salubrity.Application/Services/Reporting/` | Report PDF generation |
| `/etc/salubrity-api.env` | Production secrets (server-side only) |
| `/srv/apps/salubrity-api/publish/` | Published API binaries |
| `/srv/apps/salubrity-frontend/` | Frontend source + build |

---

## Contacts & Access

| Resource | Detail |
|----------|--------|
| Server SSH | `172.31.10.155` (ubuntu user) |
| Production DB | PostgreSQL on `localhost:5432`, db: `salubrity`, user: `salubrity_user` |
| Admin email | `veronicah.anzimbu@dataposit.co.ke` |
| Swagger (prod) | `https://api-salubrity.dataposit.co.ke/swagger` |

---

## Known Issues & Notes

- **Raphael Dianga / triage visibility:** Patient must complete all 3 self-assessment sections (Occupation Details, General Health History, Wellness and Lifestyle) to appear in the triage queue when `RequiresSelfAssessment = true`.
- **Gemini AI drafts are disabled:** Key is commented out in `/etc/salubrity-api.env`. Doctor draft endpoints will return an error until re-enabled.
- **Doctor/Concierge dashboards share the `my/upcoming` filter:** The `!IsLaunched` guard in that endpoint must not be changed. Use the dedicated `my/billing-camps` endpoint for any admin billing page.
- **QuestPDF community license:** PDF generation is licensed under QuestPDF Community. Do not use for commercial closed-source products without upgrading the license.
- **EPPlus NonCommercial:** Excel generation uses EPPlus under a NonCommercial license.
