# Salubrity API Reference

**Base URL (Production):** `https://api-salubrity.dataposit.co.ke`  
**API Version:** v1  
**URL pattern:** `/api/v1/{resource}`

---

## Authentication

All endpoints require a JWT Bearer token unless marked **[Public]**.  
Include in every request:
```
Authorization: Bearer <access_token>
```

The BI endpoints use a separate API key mechanism:
```
X-BI-Key: <bi_api_key>
```

### Standard Response Envelope

All JSON responses follow this shape:

```json
{
  "success": true,
  "message": "Optional human-readable message",
  "data": { ... }
}
```

Paginated responses return:
```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 120,
    "page": 1,
    "pageSize": 20
  }
}
```

---

## Roles

| Role | Description |
|------|-------------|
| `Admin` | Full system access |
| `Concierge` | Camp operations, queue management, billing |
| `Doctor` | Clinical review, recommendations, referrals |
| `Subcontractor` | Service provider at a camp station |
| `Patient` | Registered camp participant |

---

## 1. Authentication & Security
`/api/v1/auth`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/auth/register` | Public | Register a new user account |
| POST | `/auth/login` | Public | Login — returns `accessToken` + `refreshToken` |
| GET | `/auth/me` | Bearer | Return current user profile |
| POST | `/auth/refresh-token` | Public | Exchange a refresh token for a new access token |
| POST | `/auth/logout` | Public | Invalidate session (`?userId=`) |
| POST | `/auth/forgot-password` | Public | Send OTP to user's email |
| POST | `/auth/reset-password-with-token` | Public | Reset password using OTP token |
| POST | `/auth/reset-password` | Public | Reset password (alternative flow) |
| PUT | `/auth/change-password` | Public | Change password (`?userId=`) |
| POST | `/auth/setup-mfa` | Public | Generate TOTP secret for MFA setup (`?email=`) |
| POST | `/auth/verify-mfa-code` | Public | Verify a TOTP code |

**Login request:**
```json
{ "email": "user@example.com", "password": "Secret123!" }
```

**Login response (data):**
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "abc...",
  "expiresIn": 3600
}
```

---

## 2. User Management
`/api/v1/users`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/users` | Bearer | List all users |
| GET | `/users/{id}` | Admin, UserManager | Get user by ID |
| POST | `/users` | Public | Create a new user |
| PUT | `/users/{id}` | Bearer | Update user profile |
| DELETE | `/users/{id}` | Admin | Delete user |
| GET | `/users/onboarding/me` | Bearer | Get onboarding status for current user |
| POST | `/users/onboarding/me/check` | Bearer | Evaluate and update onboarding completion |

---

## 3. Health Camps Management
`/api/v1/health-camps`

### Camp CRUD

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/health-camps` | Bearer | List all camps |
| GET | `/health-camps/{id}` | Bearer | Get camp detail |
| POST | `/health-camps` | Bearer | Create a camp |
| PUT | `/health-camps/{id}` | Bearer | Update a camp |
| DELETE | `/health-camps/{id}` | Bearer | Soft-delete a camp |

**Create camp body:**
```json
{
  "name": "PWC Annual Wellness 2026",
  "organizationId": "...",
  "startDate": "2026-06-25",
  "endDate": "2026-06-27",
  "location": "Nairobi",
  "expectedParticipants": 200,
  "requiresSelfAssessment": true
}
```

### My Camps (role-filtered views)

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/health-camps/my/upcoming` | Sub, Concierge, Doctor, Admin | Camps not yet launched where StartDate ≥ today |
| GET | `/health-camps/my/billing-camps` | Admin | All active camps (including launched) for billing page |
| GET | `/health-camps/my/ongoing` | Sub, Concierge, Doctor, Admin | Launched camps currently running |
| GET | `/health-camps/my/complete` | Sub, Concierge, Doctor, Admin | Past camps |
| GET | `/health-camps/my/canceled` | Sub, Concierge, Doctor, Admin | Canceled camps |
| GET | `/health-camps/my-camp-services/{status}` | Concierge, Sub, Admin | Camps with role details for `upcoming\|complete\|canceled\|ongoing` |

### Camp Lifecycle

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/health-camps/{id}/launch` | Bearer | Launch a camp (generates QR codes, activates queue) |
| POST | `/health-camps/{id}/cancel` | Bearer | Cancel a camp |

### Participants

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| POST | `/health-camps/{campId}/participants` | Bearer | Add a participant to a camp |
| GET | `/health-camps/{campId}/participants` | Bearer | Paged participant list. Query: `serviceId`, `participantId`, `status` (All\|Served\|NotSeen), `q`, `sort`, `page`, `pageSize` |
| GET | `/health-camps/{campId}/patients/all` | Sub, Admin | All patients with filter/sort/search. Query: `filter=all\|served\|not-seen`, `q`, `sort`, `page`, `pageSize` |
| GET | `/health-camps/{campId}/patients/{participantId}/detail-with-forms` | Sub, Doctor, Admin, Concierge | Full patient record with submitted form responses |
| POST | `/health-camps/participants/remove` | Bearer | Remove patient from camp |
| GET | `/health-camps/{campId}/participant/{participantId}/billing-status` | Bearer | Get participant billing status |
| PUT | `/health-camps/{campId}/participant/{participantId}/billing-status` | Bearer | Update participant billing status |

### Package Assignment

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/health-camps/{campId}/packages` | Admin, Concierge, Doctor, FrontDesk | List packages available for a camp |
| POST | `/health-camps/participants/assign-package` | Admin, Concierge | Assign a package to one participant |
| POST | `/health-camps/{campId}/participants/assign-package/bulk` | Admin, Concierge | Bulk-assign package to many participants |

**Assign package body:**
```json
{
  "participantId": "...",
  "healthCampId": "...",
  "healthCampPackageId": "..."
}
```

### Billing

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/health-camps/{campId}/billing-statuses` | Admin, Concierge | All participants with billing status for a camp |

### Subcontractor Assignments

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| POST | `/health-camps/{campId}/subcontractors/add` | Admin, Concierge | Assign a subcontractor to a camp |
| DELETE | `/health-camps/{campId}/subcontractors/{subcontractorId}/remove` | Admin, Concierge | Remove subcontractor from camp |
| GET | `/health-camps/{campId}/my-station-assignments` | Sub, Doctor, Admin | List stations assigned to the current subcontractor |

### Reports & Stats

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/health-camps/{campId}/stats` | Concierge, Doctor, Sub, Admin | Overall camp statistics (served counts, donut data) |
| GET | `/health-camps/{campId}/stats/sa-status` | Admin | Self-assessment completion status |
| GET | `/health-camps/{campId}/final-report-status` | Bearer | Whether final reports have been published |
| POST | `/health-camps/{campId}/publish-final-reports` | Admin | Publish final individual reports to patients |
| GET | `/health-camps/organization/{organizationId}` | Bearer | Camps for an organization |
| GET | `/health-camps/organization/{organizationId}/stats` | Bearer | Aggregate stats for an organization |
| GET | `/health-camps/camps/upcoming-dates` | Bearer | List of upcoming camp start dates |

### QR Posters

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/health-camps/{campId}/posters/{kind}` | Public | Download QR PNG. `kind`: `participant` or `subcontractor` |
| POST | `/health-camps/posters/details` | Public | Decode a QR poster token |

### Camp Management Views

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/health-camps/{healthCampId}/organization-summary` | Bearer | Org + client summary for a camp |
| GET | `/health-camps/{healthCampId}/service-stations` | Bearer | Service station list. `?group=true` groups by category |
| GET | `/health-camps/{healthCampId}/patients` | Bearer | Summary patient list |
| GET | `/health-camps/{healthCampId}/activity-summary` | Bearer | Daily activity breakdown |
| GET | `/health-camps/{healthCampId}/billing` | Bearer | Billing summary |

---

## 4. My Camps (Patient / Participant View)
`/api/v1/camps/my`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/camps/my/upcoming` | Bearer | Patient's upcoming registered camps. Query: `page`, `pageSize`, `search` |
| GET | `/camps/my/ongoing` | Bearer | Patient's currently running camps |
| GET | `/camps/my/complete` | Bearer | Patient's completed camps |
| GET | `/camps/my/{campId}/service-stations` | Bearer | Service stations available in a camp for the current user |
| POST | `/camps/my/{campId}/service-stations/{assignmentId}/check-in` | Patient | Check in to a station queue |
| POST | `/camps/my/{campId}/cancel-check-in` | Bearer | Cancel an active check-in |
| GET | `/camps/my/{campId}/check-in-state` | Bearer | Current check-in state for the user |
| GET | `/camps/my/{campId}/service-stations/{assignmentId}/position` | Bearer | Queue position at a specific station |
| GET | `/camps/my/{campId}/queue` | Sub, Concierge, Admin | View the queue for the current user's assigned station |

---

## 5. Camp Station Operations
`/api/v1/camps/stations`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/camps/stations/{checkInId}/start` | CampCoordinator, Staff | Mark a check-in as InService |
| POST | `/camps/stations/{checkInId}/complete` | CampCoordinator, Staff | Mark a check-in as Completed |

---

## 6. Concierge
`/api/v1/concierge`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/concierge/{campId}/service-stations-info` | Bearer | Station info cards for concierge dashboard |
| GET | `/concierge/{campId}/queue-priorities` | Bearer | Queue priority summary per station |
| GET | `/concierge/camps/{campId}/stations-queue` | Bearer | Full stations + queue list |
| GET | `/concierge/{patientId}/detail` | Bearer | Full patient detail card |
| GET | `/concierge/participants/{participantId}/stations` | Bearer | Station completion status per participant |
| GET | `/concierge/{campId}/live-stats` | Bearer | Live camp stats (queue depths, served counts) |
| GET | `/concierge/{campId}/participants/search` | Bearer | Search participants by name/number. Query: `q` |
| GET | `/concierge/{campId}/station-bottlenecks` | Bearer | Stations with queue > threshold |
| GET | `/concierge/{campId}/registration-timeline` | Bearer | Hourly registration count timeline (EAT) |

---

## 7. Organizations
`/api/v1/organizations`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/organizations` | Bearer | Create organization |
| GET | `/organizations` | Bearer | List all organizations |
| GET | `/organizations/{id}` | Bearer | Get organization |
| PUT | `/organizations/{id}` | Bearer | Update organization |
| DELETE | `/organizations/{id}` | Bearer | Delete organization |
| GET | `/organization-overview` | Bearer | High-level org stats |

### Branches

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/organizations/{orgId}/branches` | Bearer | Create branch |
| GET | `/organizations/{orgId}/branches` | Bearer | List branches |
| GET | `/organizations/branches/{branchId}` | Bearer | Get branch |
| PUT | `/organizations/branches/{branchId}` | Bearer | Update branch |
| DELETE | `/organizations/branches/{branchId}` | Bearer | Delete branch |
| POST | `/organizations/{orgId}/branches/bulk-upload` | Bearer | Bulk upload branches via XLSX |

### Departments

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/organizations/{orgId}/departments` | Bearer | List departments |
| POST | `/organizations/{orgId}/departments` | Admin | Add a department |
| POST | `/organizations/{orgId}/departments/upload` | Admin | Bulk upload via XLSX (col 1: Name, col 2: Description) |
| GET | `/organizations/{orgId}/departments/template` | Bearer | Download XLSX upload template |
| DELETE | `/organizations/{orgId}/departments/{id}` | Admin | Soft-delete department |

---

## 8. Subcontractors
`/api/v1/subcontractors`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/subcontractors` | Bearer | Create subcontractor |
| GET | `/subcontractors` | Bearer | List all subcontractors |
| GET | `/subcontractors/{id}` | Bearer | Get subcontractor |
| PUT | `/subcontractors/{id}` | Bearer | Update subcontractor |
| DELETE | `/subcontractors/{id}` | Bearer | Soft-delete subcontractor |
| POST | `/subcontractors/{id}/assign-role` | Bearer | Assign role(s) to subcontractor |
| POST | `/subcontractors/{id}/assign-specialty` | Bearer | Assign specialty to subcontractor |
| GET | `/subcontractor-overview` | Bearer | Summary stats for subcontractor dashboard |

---

## 9. Patients
`/api/v1/patients`

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| POST | `/patients/generate-number` | Admin | Generate a new patient number |
| POST | `/patients/assign-legacy-numbers` | Admin | Backfill patient numbers for legacy records |
| GET | `/patients/{userId}/camp-overview` | Bearer | Camps attended and upcoming camps for a patient |

---

## 10. Employees
`/api/v1/employees`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/employees` | Bearer | List all employees |
| GET | `/employees/{id}` | Bearer | Get employee |
| POST | `/employees` | Bearer | Create employee |
| PUT | `/employees/{id}` | Bearer | Update employee |
| DELETE | `/employees/{id}` | Bearer | Delete employee |
| GET | `/employees/by-organization/{organizationId}` | Bearer | Employees for an org |
| GET | `/employees/template` | Bearer | Download XLSX onboarding template |
| POST | `/employees/upload-template` | Bearer | Upload filled XLSX template |
| POST | `/employees/bulk-upload` | Bearer | Bulk create from CSV |

---

## 11. Healthcare Services
`/api/v1/services` | `/api/v1/service-categories` | `/api/v1/service-subcategories` | `/api/v1/service-packages`

All four resources follow the same CRUD pattern:

| Method | Path | Description |
|--------|------|-------------|
| GET | `/{resource}` | List all |
| GET | `/{resource}/{id}` | Get by ID |
| POST | `/{resource}` | Create |
| PUT | `/{resource}/{id}` | Update |
| DELETE | `/{resource}/{id}` | Delete |

Additional service endpoints:

| Method | Path | Description |
|--------|------|-------------|
| POST | `/services/assign-form` | Link an intake form to a service |

---

## 12. Intake Forms
`/api/v1/forms` | `/api/v1/intake-form-responses`

### Form Definitions

| Method | Path | Description |
|--------|------|-------------|
| GET | `/forms` | List all forms |
| GET | `/forms/{id}` | Get form |
| POST | `/forms` | Create form |
| PUT | `/forms/{id}` | Update form |
| DELETE | `/forms/{id}` | Delete form |
| POST | `/forms/blueprint` | Create form from blueprint structure |

### Responses

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/intake-form-responses` | Bearer | Submit a form response |
| PATCH | `/intake-form-responses/{id}` | Bearer | Patch (update) an existing response |
| GET | `/intake-form-responses/{id}` | Bearer | Get response by ID |
| GET | `/intake-form-responses?patientId=&healthCampId=` | Bearer | Get all responses for a patient in a camp |

### Data Export

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/intake-form-responses/bulk-upload/lab-results/template?campId=` | Bearer | Download lab results XLSX template for a camp |
| POST | `/intake-form-responses/bulk-upload/lab-results` | Bearer | Upload completed lab results XLSX |
| GET | `/intake-form-responses/camp/{campId}/data/export?branchId=` | Bearer | Export all camp findings as XLSX |
| GET | `/intake-form-responses/all-camps/data/export` | Admin | Export all camps' data as XLSX |

---

## 13. Health Assessment Forms (Self-Assessment)
`/api/v1/health-assessments/forms`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/health-assessments/forms/submit-section` | Bearer | Submit one section of the self-assessment |
| GET | `/health-assessments/forms/submitted-responses?patientId=&campId=` | Bearer | Get submitted responses for a patient |
| GET | `/health-assessments/forms/me/status` | Bearer | Self-assessment completion status for current user |

The three required self-assessment form types (UUIDs are seeded at DB setup):
- **Occupation Details** — `bcad133b-...`
- **General Health History** — `4ab8a5f2-...`
- **Wellness and Lifestyle** — `bd5e98e0-...`

---

## 14. Clinical
`/api/v1/doctor-recommendations` | `/api/v1/service-referrals`

### Doctor Recommendations

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| POST | `/doctor-recommendations` | Doctor, Admin | Create recommendation |
| GET | `/doctor-recommendations/{id}` | Doctor, Admin | Get by ID |
| GET | `/doctor-recommendations/patient/{patientId}` | Doctor, Admin | All recommendations for a patient |
| PUT | `/doctor-recommendations/{id}` | Doctor, Admin | Update recommendation |
| DELETE | `/doctor-recommendations/{id}` | Doctor, Admin | Delete recommendation |
| POST | `/doctor-recommendations/draft` | Doctor, Admin | AI-draft the instructions paragraph (Gemini) |
| POST | `/doctor-recommendations/draft-field` | Doctor, Admin | AI-draft one specific textarea field (Gemini) |

**Note:** AI draft endpoints return `{ "data": { "draft": "..." } }` but require Gemini API key to be configured.

### Service Referrals

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| POST | `/service-referrals` | Sub, Doctor, Admin | Create referral |
| GET | `/service-referrals/{id}` | Sub, Doctor, Admin | Get by ID |
| GET | `/service-referrals?healthCampId=&participantId=` | Sub, Doctor, Admin | List referrals for a camp / participant |
| PUT | `/service-referrals/{id}` | Sub, Doctor, Admin | Update referral |
| DELETE | `/service-referrals/{id}` | Sub, Doctor, Admin | Delete referral |

---

## 15. Reports
`/api/v1/reports`

### Individual Preliminary Report

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/reports/individual-preliminary/{participantId}` | Bearer | JSON data for a specific participant |
| GET | `/reports/individual-preliminary/by-camp/{campId}` | Bearer | JSON data — resolves participant from JWT |
| GET | `/reports/individual-preliminary/{participantId}/pdf` | Bearer | PDF download for a specific participant |
| GET | `/reports/individual-preliminary/by-camp/{campId}/pdf` | Bearer | PDF download — resolves participant from JWT |

### Individual Final Report

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/reports/individual-final/{participantId}` | Bearer | JSON data for a specific participant |
| GET | `/reports/individual-final/by-camp/{campId}?participantId=` | Bearer | JSON data — resolves from JWT unless `participantId` passed |
| GET | `/reports/individual-final/{participantId}/pdf` | Bearer | PDF download |
| GET | `/reports/individual-final/by-camp/{campId}/pdf?participantId=` | Bearer | PDF download — resolves from JWT unless `participantId` passed |

> Patient self-view of Final Report is gated: returns 404 until admin publishes via `POST /health-camps/{campId}/publish-final-reports`.

### Corporate Reports (Admin only)

All accept optional query filters: `?gender=Male&age=25-34&day=1&department=Finance`

| Method | Path | Description |
|--------|------|-------------|
| GET | `/reports/corporate/{campId}` | Preliminary corporate report JSON |
| GET | `/reports/corporate/{campId}/pdf` | Preliminary corporate report PDF |
| POST | `/reports/corporate/{campId}/send-email` | Email preliminary report to recipients |
| GET | `/reports/corporate/{campId}/final` | Final corporate report JSON |
| GET | `/reports/corporate/{campId}/final/pdf` | Final corporate report PDF |
| POST | `/reports/corporate/{campId}/final/send-email` | Email final report to recipients |
| GET | `/reports/corporate/{campId}/exco` | EXCO executive summary JSON |
| GET | `/reports/corporate/{campId}/exco/pdf?exclude=key1,key2` | EXCO PDF with optional section exclusions |

**Send email body:**
```json
{
  "recipients": ["ceo@company.com", "hr@company.com"],
  "subject": "Wellness Camp Report"
}
```

---

## 16. Notifications
`/api/v1/notifications`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/notifications/user/{userId}` | Bearer | Get all notifications for a user |
| PUT | `/notifications/{notificationId}/mark-as-read` | Bearer | Mark one notification as read |
| PUT | `/notifications/user/{userId}/mark-all-read` | Bearer | Mark all notifications as read |
| DELETE | `/notifications/user/{userId}/clear` | Bearer | Clear all notifications for a user |

---

## 17. RBAC
`/api/v1/roles` | `/api/v1/user-roles` | `/api/v1/permissions`

All three follow standard CRUD:

| Method | Path | Description |
|--------|------|-------------|
| GET | `/{resource}` | List all |
| GET | `/{resource}/{id}` | Get by ID |
| POST | `/{resource}` | Create |
| PUT | `/{resource}/{id}` | Update |
| DELETE | `/{resource}/{id}` | Delete |

Additional:

| Method | Path | Description |
|--------|------|-------------|
| GET | `/permissions/by-role/{roleId}` | Permissions for a role |
| POST | `/rbac/seed` | Seed default roles and permission groups |

---

## 18. Lookups
`/api/v1/lookups`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/lookups/available` | Bearer | List all lookup type names |
| GET | `/lookups/{lookupName}` | Bearer | Get all items for a lookup type |

Available lookup names include: `genders`, `languages`, `departments`, `billing-statuses`, `follow-up-recommendations`, `follow-up-schedules`, `urgencies`, `recommendation-types`, `subcontractor-statuses`, `organization-statuses`, `health-camp-statuses`.

---

## 19. Admin Dashboard
`/api/v1/admin`

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/admin/dashboard/overview?year=` | Admin | Top-level dashboard band: ongoing/upcoming/last camp, YTD totals, pending publishes |

---

## 20. BI (Power BI Integration)
`/api/v1/bi`

Authenticated via `X-BI-Key` header (not JWT). Used by Power BI scheduled refresh.

| Method | Path | Description |
|--------|------|-------------|
| GET | `/bi/organizations` | Dimension: organizations |
| GET | `/bi/camps?orgId=&year=` | Dimension: camps |
| GET | `/bi/patients?orgId=&campId=&year=` | Fact: one wide row per (patient, camp) |
| GET | `/bi/findings?orgId=&campId=&year=` | Fact: one row per finding |
| GET | `/bi/lab-results?orgId=&campId=&year=` | Fact: one row per lab result |

---

## 21. Reporting Metrics
`/api/v1/reporting/metrics`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/reporting/metrics` | Bearer | List metric configurations |
| POST | `/reporting/metrics` | Bearer | Create a metric mapping |
| GET | `/reporting/metrics/{metricCode}` | Bearer | Get metric by code |

---

## 22. Overview Dashboards

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/api/v1/overview` | Bearer | Homepage overview stats |
| GET | `/api/v1/health-camp-overview` | Bearer | Health camp overview stats |
| GET | `/api/v1/organization-overview` | Bearer | Organization overview stats |
| GET | `/api/v1/subcontractor-overview` | Bearer | Subcontractor overview stats |

---

## Error Codes

| HTTP Status | Meaning |
|-------------|---------|
| 200 | Success |
| 201 | Created |
| 204 | No content |
| 400 | Bad request / validation failure |
| 401 | Unauthorized (missing or invalid token) |
| 403 | Forbidden (valid token, insufficient role) |
| 404 | Resource not found |
| 422 | Unprocessable entity |
| 500 | Internal server error |

Error response shape:
```json
{
  "success": false,
  "message": "Resource not found.",
  "errors": ["Field X is required"]
}
```

---

## Swagger UI

Interactive docs available at:  
`https://api-salubrity.dataposit.co.ke/swagger`
