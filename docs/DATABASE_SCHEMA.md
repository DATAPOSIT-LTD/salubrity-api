# Salubrity Database Schema

**Engine:** PostgreSQL 16  
**Database:** `salubrity`  
**Owner:** `salubrity_user`  
**ORM:** Entity Framework Core 9 (Code-First)

All tables have a shared base pattern from `BaseAuditableEntity`:

| Column | Type | Notes |
|--------|------|-------|
| `Id` | `uuid` | Primary key |
| `CreatedAt` | `timestamp` | UTC |
| `UpdatedAt` | `timestamp` | UTC |
| `IsDeleted` | `boolean` | Soft delete flag |

---

## Domain Groups

```
Identity & Access
  Users ──────────────────────── UserRoles ── Roles ── RolePermissionGroups ── PermissionGroups
  Patients                                             PermissionGroupPermissions ── Permissions

Organizations
  Organizations ── OrganizationBranches
                ── OrganizationDepartments

Health Services
  Services ── IntakeForms
  ServiceCategories
  ServiceSubcategories
  ServicePackages ── HealthCampPackageItems

Health Camps
  HealthCamps ── HealthCampPackages (links ServicePackages)
              ── HealthCampServiceAssignments (links Subcontractors to stations)
              ── HealthCampParticipants ── HealthCampParticipantPackages
                                       ── HealthCampParticipantServiceStatuses
                                       ── HealthCampStationCheckIns
              ── SubcontractorHealthCampAssignments
              ── HealthCampTempCredentials

Intake Forms
  IntakeForms ── IntakeFormVersions ── IntakeFormSections ── IntakeFormFields ── IntakeFormFieldOptions
  IntakeFormResponses ── IntakeFormFieldResponses

Clinical
  DoctorRecommendations
  ServiceReferrals

Self-Assessment
  HealthAssessments ── HealthAssessmentFormResponses ── HealthAssessmentDynamicFieldResponses
                    ── HealthAssessmentMetrics
  HealthMetricConfigs ── HealthMetricThresholds

Notifications
  Notifications ── NotificationRecipients

Employees
  Employees

Subcontractors
  Subcontractors ── SubcontractorSpecialties
                 ── SubcontractorRoleAssignments
                 ── SubcontractorHealthCampAssignments
```

---

## Tables

### Users

| Column | Type | Constraints |
|--------|------|-------------|
| `Email` | `varchar` | Required, unique |
| `PasswordHash` | `text` | Bcrypt hash |
| `FirstName` | `varchar` | Required |
| `MiddleName` | `varchar` | Nullable |
| `LastName` | `varchar` | Required |
| `Phone` | `varchar` | Nullable |
| `NationalId` | `varchar` | Nullable |
| `DateOfBirth` | `date` | Nullable |
| `PrimaryLanguage` | `varchar` | Nullable |
| `Department` | `varchar` | Patient-selected at onboarding; used as filter on reports |
| `ProfileImage` | `varchar` | Path or URL |
| `GenderId` | `uuid FK → Genders` | Nullable |
| `OrganizationId` | `uuid FK → Organizations` | Nullable; sets org membership |
| `RelatedEntityId` | `uuid` | Polymorphic FK (Patient, Subcontractor, etc.) |
| `RelatedEntityType` | `varchar` | `"Patient"` / `"Subcontractor"` / etc. |
| `IsActive` | `boolean` | Default true |
| `IsVerified` | `boolean` | Default false |
| `LastLoginAt` | `timestamp` | Nullable |
| `RefreshToken` | `text` | Nullable |
| `RefreshTokenExpiryTime` | `timestamp` | Nullable |
| `TotpSecret` | `text` | Encrypted TOTP secret for MFA |

---

### Patients

| Column | Type | Constraints |
|--------|------|-------------|
| `UserId` | `uuid FK → Users` | Required |
| `PrimaryOrganizationId` | `uuid FK → Organizations` | Nullable |
| `PatientNumber` | `varchar` | Unique patient identifier (e.g. `SAL-00001`) |
| `LegacyPatientNumber` | `varchar` | From legacy/migrated systems |
| `Notes` | `text` | Nullable |

---

### Organizations

| Column | Type | Constraints |
|--------|------|-------------|
| `BusinessName` | `varchar` | Required |
| `Email` | `varchar` | Email format |
| `Phone` | `varchar` | Phone format |
| `Location` | `varchar` | Nullable |
| `ClientLogoPath` | `varchar` | Nullable |
| `ContactPersonId` | `uuid` | Nullable |
| `StatusId` | `uuid FK → OrganizationStatuses` | Nullable |

### OrganizationBranches

| Column | Type | Constraints |
|--------|------|-------------|
| `OrganizationId` | `uuid FK → Organizations` | Required |
| `BranchName` | `varchar` | Required |
| `Code` | `varchar` | Optional branch code (e.g. `EQT-005`) |
| `Address` | `varchar` | Nullable |
| `City` | `varchar` | Nullable |
| `Region` | `varchar` | Nullable |
| `ContactEmail` | `varchar` | Nullable |
| `ContactPhone` | `varchar` | Nullable |
| `IsActive` | `boolean` | Default true |

### OrganizationDepartments

| Column | Type | Constraints |
|--------|------|-------------|
| `OrganizationId` | `uuid FK → Organizations` | Required |
| `Name` | `varchar` | Required |
| `Description` | `varchar` | Nullable |

---

### Services

| Column | Type | Constraints |
|--------|------|-------------|
| `Name` | `varchar(100)` | Required |
| `Description` | `varchar(255)` | Nullable |
| `PricePerPerson` | `numeric(10,2)` | Nullable |
| `IndustryId` | `uuid FK → Industries` | Nullable |
| `IntakeFormId` | `uuid FK → IntakeForms` | Nullable; the intake form filled at this station |
| `ImageUrl` | `varchar(2048)` | Nullable |
| `IsActive` | `boolean` | Default true |

### ServicePackages

| Column | Type | Constraints |
|--------|------|-------------|
| `Name` | `varchar(100)` | Required |
| `Description` | `varchar(255)` | Nullable |
| `Price` | `numeric(10,2)` | Nullable |
| `RangeOfPeople` | `varchar(100)` | e.g. "1–50" |
| `IsActive` | `boolean` | Default true |

### HealthCampPackageItems

Maps which services/categories belong to a `ServicePackage`.

| Column | Type | Constraints |
|--------|------|-------------|
| `ServicePackageId` | `uuid FK → ServicePackages` | Required |
| `ReferenceId` | `uuid` | ID of the Service / Category / Subcategory |
| `ItemType` | `int (enum)` | `0=Service`, `1=ServiceCategory`, `2=ServiceSubcategory` |

---

### HealthCamps

| Column | Type | Constraints |
|--------|------|-------------|
| `Name` | `varchar` | Required |
| `Description` | `text` | Nullable |
| `Location` | `varchar` | Nullable |
| `StartDate` | `date` | Required |
| `EndDate` | `date` | Nullable |
| `StartTime` | `interval` | Nullable |
| `OrganizationId` | `uuid FK → Organizations` | Required |
| `ExpectedParticipants` | `int` | Nullable |
| `IsActive` | `boolean` | Default true |
| `IsLaunched` | `boolean` | Set by POST /launch |
| `LaunchedAt` | `timestamp` | Nullable |
| `CloseDate` | `timestamp` | Nullable |
| `HealthCampStatusId` | `uuid FK → HealthCampStatuses` | Nullable |
| `Slug` | `varchar(160)` | Unique kebab-case identifier |
| `ParticipantPosterJti` | `varchar` | JWT ID for participant QR poster token |
| `SubcontractorPosterJti` | `varchar` | JWT ID for subcontractor QR poster token |
| `PosterTokensExpireAt` | `timestamptz` | Nullable |
| `RequiresSelfAssessment` | `boolean` | Default false. When true, all 3 SA forms must be submitted before patient enters triage |
| `FinalReportsPublishedAt` | `timestamp` | Set when admin publishes final reports |
| `FinalReportsPublishedById` | `uuid` | Nullable |

### HealthCampPackages

Junction between a camp and its available service packages.

| Column | Type | Notes |
|--------|------|-------|
| `HealthCampId` | `uuid FK → HealthCamps` | |
| `ServicePackageId` | `uuid FK → ServicePackages` | |
| `IsActive` | `boolean` | |
| `DisplayName` | `varchar` | Optional label shown at camp |
| `PriceOverride` | `numeric(10,2)` | Nullable price override |

### HealthCampParticipants

One row per (patient, camp) enrollment.

| Column | Type | Notes |
|--------|------|-------|
| `HealthCampId` | `uuid FK → HealthCamps` | |
| `UserId` | `uuid FK → Users` | |
| `PatientId` | `uuid FK → Patients` | Nullable |
| `IsEmployee` | `boolean` | |
| `Notes` | `text` | |
| `BillingStatusId` | `uuid FK → BillingStatuses` | Not Billed / Billed / Proceed without billing |
| `HealthCampPackageId` | `uuid FK → HealthCampPackages` | FK synced when package assigned |
| `ParticipatedAt` | `timestamp` | |
| `TempPasswordHash` | `text` | |
| `TempPasswordExpiresAt` | `timestamptz` | |

### HealthCampParticipantPackages

Active package assignment (junction, one row active per participant).

| Column | Type | Notes |
|--------|------|-------|
| `ParticipantId` | `uuid FK → HealthCampParticipants` | |
| `HealthCampPackageId` | `uuid FK → HealthCampPackages` | |
| `AssignedAt` | `timestamptz` | |
| `IsActive` | `boolean` | Only one row active per participant |

### HealthCampServiceAssignments

Maps a subcontractor to a service/station within a camp.

| Column | Type | Notes |
|--------|------|-------|
| `HealthCampId` | `uuid FK → HealthCamps` | |
| `AssignmentId` | `uuid` | ID of the Service / Category / Subcategory |
| `AssignmentType` | `int (enum)` | `0=Service`, `1=Category`, `2=Subcategory` |
| `SubcontractorId` | `uuid FK → Subcontractors` | |
| `ProfessionId` | `uuid FK → SubcontractorRoles` | Nullable |

### HealthCampParticipantServiceStatuses

Tracks which station has served each participant.

| Column | Type | Notes |
|--------|------|-------|
| `ParticipantId` | `uuid FK → HealthCampParticipants` | |
| `ServiceAssignmentId` | `uuid FK → HealthCampServiceAssignments` | |
| `SubcontractorId` | `uuid` | Who served |
| `ServedAt` | `timestamp` | |
| `Notes` | `text` | |

### HealthCampStationCheckIns

Queue state per participant per station.

| Column | Type | Notes |
|--------|------|-------|
| `HealthCampId` | `uuid` | |
| `HealthCampParticipantId` | `uuid FK → HealthCampParticipants` | |
| `HealthCampServiceAssignmentId` | `uuid FK → HealthCampServiceAssignments` | |
| `Status` | `varchar` | `Queued` → `InService` → `Completed` (or `Canceled`) |
| `Priority` | `int` | Higher = served sooner; FIFO within same priority |
| `StartedAt` | `timestamptz` | |
| `FinishedAt` | `timestamptz` | |

### SubcontractorHealthCampAssignments

Formal assignment of a subcontractor to a camp.

| Column | Type | Notes |
|--------|------|-------|
| `HealthCampId` | `uuid FK → HealthCamps` | |
| `SubcontractorId` | `uuid FK → Subcontractors` | |
| `AssignmentId` | `uuid` | Service/Category/Subcategory |
| `AssignmentType` | `int (enum)` | |
| `BoothLabel` | `varchar(100)` | Required |
| `RoomNumber` | `varchar(50)` | Nullable |
| `AssignmentStatusId` | `uuid FK → SubcontractorHealthCampAssignmentStatuses` | |
| `StartDate` | `timestamp` | Required |
| `EndDate` | `timestamp` | Nullable |
| `IsPrimaryAssignment` | `boolean` | |
| `TempPasswordHash` | `text` | |
| `TempPasswordExpiresAt` | `timestamptz` | |

### HealthCampTempCredentials

Short-lived credentials issued via QR scan.

| Column | Type | Notes |
|--------|------|-------|
| `HealthCampId` | `uuid` | |
| `UserId` | `uuid` | |
| `Role` | `varchar(32)` | `patient` or `subcontractor` |
| `TempPasswordHash` | `text` | |
| `TempPasswordExpiresAt` | `timestamptz` | |
| `SignInJti` | `varchar(64)` | |
| `TokenExpiresAt` | `timestamptz` | |

---

### IntakeForms

| Column | Type | Notes |
|--------|------|-------|
| `Name` | `varchar(150)` | Required |
| `Description` | `varchar(500)` | |
| `IsActive` | `boolean` | |
| `IsLabForm` | `boolean` | True for lab result forms |

### IntakeFormVersions / IntakeFormSections / IntakeFormFields / IntakeFormFieldOptions

Hierarchical structure: Form → Version → Section → Field → Option.

### IntakeFormResponses

One row per patient submission at a station.

| Column | Type | Notes |
|--------|------|-------|
| `IntakeFormVersionId` | `uuid FK → IntakeFormVersions` | |
| `SubmittedByUserId` | `uuid FK → Users` | |
| `PatientId` | `uuid FK → Patients` | |
| `HealthCampId` | `uuid FK → HealthCamps` | Nullable (null for legacy rows) |
| `SubmittedServiceId` | `uuid` | What the patient selected (service/category/subcategory) |
| `SubmittedServiceType` | `int (enum)` | |
| `ResolvedServiceId` | `uuid FK → Services` | Canonical service ID |
| `ResponseStatusId` | `uuid FK → IntakeFormResponseStatuses` | |

### IntakeFormFieldResponses

One row per field per response.

---

### DoctorRecommendations

| Column | Type | Notes |
|--------|------|-------|
| `PatientId` | `uuid` | |
| `DoctorId` | `uuid` | |
| `HealthCampId` | `uuid` | |
| `PertinentHistoryFindings` | `text` | |
| `PertinentClinicalFindings` | `text` | |
| `DiagnosticImpression` | `text` | |
| `Conclusion` | `text` | |
| `FollowUpRecommendationId` | `uuid FK → FollowUpRecommendations` | Fit to work / Not Fit / etc. |
| `RecommendationTypeId` | `uuid FK → RecommendationTypes` | Normal Exam / Advised / Follow up with Specialist |
| `Instructions` | `text` | Specific doctor instructions |

### ServiceReferrals

| Column | Type | Notes |
|--------|------|-------|
| `ParticipantId` | `uuid` | |
| `HealthCampId` | `uuid` | |
| `ServiceAssignmentId` | `uuid FK → HealthCampServiceAssignments` | Station where raised |
| `ServiceProviderId` | `uuid` | User who created it |
| `Reason` | `text` | |
| `UrgencyId` | `uuid FK → Urgencies` | |
| `FollowUpScheduleId` | `uuid FK → FollowUpSchedules` | |

---

### Subcontractors

| Column | Type | Notes |
|--------|------|-------|
| `UserId` | `uuid FK → Users` | |
| `IndustryId` | `uuid FK → Industries` | |
| `LicenseNumber` | `varchar(100)` | |
| `Bio` | `text` | |
| `StatusId` | `uuid FK → SubcontractorStatuses` | |

### SubcontractorSpecialties / SubcontractorRoleAssignments

Many-to-many linking subcontractors to specialties and roles.

---

### Notifications

| Column | Type | Notes |
|--------|------|-------|
| `Title` | `varchar` | |
| `Message` | `text` | |
| `Type` | `varchar` | e.g. `TriageAlert`, `FinalReportPublished` |

### NotificationRecipients

| Column | Type | Notes |
|--------|------|-------|
| `NotificationId` | `uuid FK → Notifications` | |
| `UserId` | `uuid FK → Users` | |
| `IsRead` | `boolean` | |
| `ReadAt` | `timestamp` | |

---

### Lookup Tables

Seeded at startup. All follow `{ Id uuid, Name varchar, Description varchar }`:

| Table | Key Values |
|-------|-----------|
| `BillingStatuses` | Not Billed, Billed, Proceed without billing |
| `HealthCampStatuses` | Upcoming, Ongoing, Completed, Canceled |
| `OrganizationStatuses` | Active, Inactive, Suspended |
| `SubcontractorStatuses` | Active, Inactive, Suspended |
| `SubcontractorHealthCampAssignmentStatuses` | Pending, Confirmed, Canceled |
| `Genders` | Male, Female, Other |
| `FollowUpRecommendations` | Fit to Work, Not Fit, Needs Follow-Up |
| `FollowUpSchedules` | 1 Week, 2 Weeks, 1 Month, 3 Months |
| `RecommendationTypes` | Normal Exam, Advised, Follow up with Specialist |
| `Urgencies` | Routine, Urgent, Emergency |
| `IntakeFormResponseStatuses` | Draft, Submitted, Reviewed |
| `HealthAssessmentFormTypes` | Occupation Details, General Health History, Wellness and Lifestyle |

### Self-Assessment Form Type IDs (seeded, fixed)

| Form Type | ID prefix |
|-----------|-----------|
| Occupation Details | `bcad133b-...` |
| General Health History | `4ab8a5f2-...` |
| Wellness and Lifestyle | `bd5e98e0-...` |

---

### RBAC Tables

| Table | Description |
|-------|-------------|
| `Roles` | Named roles (Admin, Doctor, Concierge, Subcontractor, Patient, etc.) |
| `UserRoles` | Many-to-many: User ↔ Role |
| `Permissions` | Fine-grained permission atoms |
| `PermissionGroups` | Named groups of permissions |
| `PermissionGroupPermissions` | Many-to-many: PermissionGroup ↔ Permission |
| `RolePermissionGroups` | Many-to-many: Role ↔ PermissionGroup |

---

### Employees

| Column | Type | Notes |
|--------|------|-------|
| `UserId` | `uuid FK → Users` | |
| `OrganizationId` | `uuid FK → Organizations` | |
| `EmployeeNumber` | `varchar` | |
| `Department` | `varchar` | |
| `JobTitle` | `varchar` | |
| `BranchId` | `uuid FK → OrganizationBranches` | Nullable |

---

### PasswordResetTokens

| Column | Type | Notes |
|--------|------|-------|
| `UserId` | `uuid` | |
| `Token` | `varchar` | OTP or signed token |
| `ExpiresAt` | `timestamp` | |
| `IsUsed` | `boolean` | |

---

## Key Relationships Summary

```
User ←1:1→ Patient
User ←1:1→ Subcontractor
User ←M:N→ Role  (via UserRoles)
User ←M:N→ HealthCamp  (via HealthCampParticipants)
Organization ←1:N→ OrganizationBranch
Organization ←1:N→ HealthCamp
HealthCamp ←1:N→ HealthCampParticipant
HealthCamp ←1:N→ HealthCampPackage  (links to ServicePackages)
HealthCamp ←1:N→ HealthCampServiceAssignment  (Subcontractor stations)
HealthCampParticipant ←1:N→ HealthCampStationCheckIn
HealthCampParticipant ←1:1→ HealthCampParticipantPackage (active)
HealthCampParticipant ←N:1→ HealthCampPackage (FK on main row, synced)
IntakeFormResponse ←N:1→ HealthCamp  (scoped to prevent cross-camp leakage)
DoctorRecommendation ←N:1→ Patient + Camp
ServiceReferral ←N:1→ Participant + Camp
```

---

## Migrations

EF Core migrations live in:  
`Salubrity.Infrastructure/Migrations/`

To apply:
```bash
cd /srv/apps/salubrity-api/salubrity-api-working
export PATH=$PATH:/usr/share/dotnet
dotnet ef database update --project Salubrity.Infrastructure --startup-project Salubrity.Api
```

The deploy script at `deploy_and_migrate.sh` handles both publish + migrate in one step.
