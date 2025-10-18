````C:.
├───.github
│   └───workflows
├───EmailTemplates
├───Salubrity.Api
│   ├───bin
│   │   ├───Debug
│   │   │   └───net9.0
│   │   │       ├───cs
│   │   │       ├───de
│   │   │       ├───EmailTemplates
│   │   │       ├───es
│   │   │       ├───fr
│   │   │       ├───it
│   │   │       ├───ja
│   │   │       ├───ko
│   │   │       ├───pl
│   │   │       ├───pt-BR
│   │   │       ├───ru
│   │   │       ├───runtimes
│   │   │       │   ├───unix
│   │   │       │   │   └───lib
│   │   │       │   │       └───net6.0
│   │   │       │   ├───win
│   │   │       │   │   └───lib
│   │   │       │   │       ├───net6.0
│   │   │       │   │       └───net9.0
│   │   │       │   ├───win-arm
│   │   │       │   │   └───native
│   │   │       │   ├───win-arm64
│   │   │       │   │   └───native
│   │   │       │   ├───win-x64
│   │   │       │   │   └───native
│   │   │       │   └───win-x86
│   │   │       │       └───native
│   │   │       ├───tr
│   │   │       ├───zh-Hans
│   │   │       └───zh-Hant
│   │   ├───Production
│   │   │   └───net9.0
│   │   │       ├───cs
│   │   │       ├───de
│   │   │       ├───es
│   │   │       ├───fr
│   │   │       ├───it
│   │   │       ├───ja
│   │   │       ├───ko
│   │   │       ├───pl
│   │   │       ├───pt-BR
│   │   │       ├───ru
│   │   │       ├───runtimes
│   │   │       │   ├───unix
│   │   │       │   │   └───lib
│   │   │       │   │       └───net6.0
│   │   │       │   ├───win
│   │   │       │   │   └───lib
│   │   │       │   │       └───net6.0
│   │   │       │   ├───win-arm
│   │   │       │   │   └───native
│   │   │       │   ├───win-arm64
│   │   │       │   │   └───native
│   │   │       │   ├───win-x64
│   │   │       │   │   └───native
│   │   │       │   └───win-x86
│   │   │       │       └───native
│   │   │       ├───tr
│   │   │       ├───zh-Hans
│   │   │       └───zh-Hant
│   │   └───Release
│   │       └───net9.0
│   ├───Controllers
│   │   ├───Auth
│   │   ├───Clinical
│   │   ├───Common
│   │   ├───Concierge
│   │   ├───Employee
│   │   ├───Forms
│   │   ├───HealthcampAssessment
│   │   ├───HealthCamps
│   │   ├───HealthcareServices
│   │   ├───HomepageOverview
│   │   ├───Lookups
│   │   ├───Menus
│   │   ├───Notifications
│   │   ├───Organizations
│   │   ├───Patients
│   │   ├───Rbac
│   │   ├───Reporting
│   │   ├───Subcontractors
│   │   └───Users
│   ├───Middleware
│   ├───obj
│   │   ├───Debug
│   │   │   └───net9.0
│   │   │       ├───ref
│   │   │       ├───refint
│   │   │       └───staticwebassets
│   │   ├───Production
│   │   │   └───net9.0
│   │   │       ├───ref
│   │   │       ├───refint
│   │   │       └───staticwebassets
│   │   └───Release
│   │       └───net9.0
│   │           ├───ref
│   │           └───refint
│   ├───Properties
│   ├───Uploads
│   │   └───Employees
│   └───wwwroot
│       ├───assets
│       └───qrcodes
│           └───healthcamps
├───Salubrity.Application
│   ├───bin
│   │   ├───Debug
│   │   │   └───net9.0
│   │   ├───Production
│   │   │   └───net9.0
│   │   └───Release
│   │       └───net9.0
│   ├───Configurations
│   ├───DTOs
│   │   ├───Auth
│   │   ├───Clinical
│   │   ├───Concierge
│   │   ├───Email
│   │   ├───Employees
│   │   ├───Forms
│   │   ├───HealthAssessment
│   │   ├───HealthCamps
│   │   ├───HealthcareServices
│   │   ├───HomepageOverview
│   │   ├───IntakeForms
│   │   ├───Lookups
│   │   ├───Menu
│   │   ├───Notifications
│   │   ├───Organizations
│   │   ├───Rbac
│   │   ├───Reporting
│   │   ├───Subcontractor
│   │   └───Users
│   ├───Extensions
│   ├───Interfaces
│   │   ├───Repositories
│   │   │   ├───Clinical
│   │   │   ├───Common
│   │   │   ├───Concierge
│   │   │   ├───Configurations
│   │   │   ├───Employees
│   │   │   ├───HealthAssessment
│   │   │   ├───HealthcampParticipant
│   │   │   ├───HealthCamps
│   │   │   ├───HealthcareServices
│   │   │   ├───HomepageOverview
│   │   │   ├───Identity
│   │   │   ├───IntakeForms
│   │   │   ├───Lookups
│   │   │   ├───Menus
│   │   │   ├───Notifications
│   │   │   ├───Organizations
│   │   │   ├───Patients
│   │   │   ├───Rbac
│   │   │   ├───Reporting
│   │   │   ├───Security
│   │   │   ├───Subcontactors
│   │   │   └───Users
│   │   └───Services
│   │       ├───Auth
│   │       ├───Clinical
│   │       ├───Common
│   │       ├───Concierge
│   │       ├───Employee
│   │       ├───HealthAssessment
│   │       ├───HealthCamps
│   │       ├───HealthcareServices
│   │       ├───HomepageOverview
│   │       ├───IntakeForms
│   │       ├───Lookups
│   │       ├───Menus
│   │       ├───Notifications
│   │       ├───Organizations
│   │       ├───Patients
│   │       ├───Rbac
│   │       ├───Reporting
│   │       ├───Security
│   │       ├───Storage
│   │       ├───Subcontractors
│   │       └───Users
│   ├───Mappings
│   ├───obj
│   │   ├───Debug
│   │   │   └───net9.0
│   │   │       ├───ref
│   │   │       └───refint
│   │   ├───Production
│   │   │   └───net9.0
│   │   │       ├───ref
│   │   │       └───refint
│   │   └───Release
│   │       └───net9.0
│   │           ├───ref
│   │           └───refint
│   ├───Services
│   │   ├───Auth
│   │   ├───BackgroundJobs
│   │   ├───Clinical
│   │   ├───Concierge
│   │   ├───Employee
│   │   ├───Forms
│   │   ├───HealthAssessment
│   │   ├───HealthCamps
│   │   ├───HealthcareServices
│   │   ├───HomepageOverview
│   │   ├───IntakeForms
│   │   ├───Lookups
│   │   ├───Menus
│   │   ├───Notifications
│   │   ├───Organizations
│   │   ├───Patient
│   │   ├───QrCodeService
│   │   ├───Rbac
│   │   ├───Reporting
│   │   ├───Security
│   │   ├───Storage
│   │   ├───Subcontractors
│   │   └───Users
│   └───Validators
│       └───Rbac
├───Salubrity.Domain
│   ├───bin
│   │   ├───Debug
│   │   │   └───net9.0
│   │   ├───Production
│   │   │   └───net9.0
│   │   └───Release
│   │       └───net9.0
│   ├───Common
│   ├───Entities
│   │   ├───Auth
│   │   ├───Clinical
│   │   ├───Configurations
│   │   ├───Employee
│   │   ├───HealthAssesment
│   │   ├───HealthCamps
│   │   ├───HealthcareServices
│   │   ├───IntakeForms
│   │   ├───Lookup
│   │   ├───Menus
│   │   ├───Notifications
│   │   ├───Organization
│   │   ├───Patient
│   │   ├───Rbac
│   │   ├───Reporting
│   │   ├───Subcontractor
│   │   └───User
│   ├───Events
│   │   └───Audit
│   ├───Join
│   ├───obj
│   │   ├───Debug
│   │   │   └───net9.0
│   │   │       ├───ref
│   │   │       └───refint
│   │   ├───Production
│   │   │   └───net9.0
│   │   │       ├───ref
│   │   │       └───refint
│   │   └───Release
│   │       └───net9.0
│   │           ├───ref
│   │           └───refint
│   └───Seeders
├───Salubrity.Infrastructure
│   ├───bin
│   │   ├───Debug
│   │   │   └───net9.0
│   │   ├───Production
│   │   │   └───net9.0
│   │   └───Release
│   │       └───net9.0
│   ├───Clinical
│   ├───Common
│   ├───Configurations
│   │   ├───HealthAssessment
│   │   ├───IntakeForms
│   │   └───Reporting
│   ├───EventHandlers
│   ├───Migrations
│   ├───obj
│   │   ├───Debug
│   │   │   └───net9.0
│   │   │       ├───ref
│   │   │       └───refint
│   │   ├───Production
│   │   │   └───net9.0
│   │   │       ├───ref
│   │   │       └───refint
│   │   └───Release
│   │       └───net9.0
│   │           ├───ref
│   │           └───refint
│   ├───Patients
│   ├───Persistence
│   ├───Repositories
│   │   ├───Auth
│   │   ├───Common
│   │   ├───Concierge
│   │   ├───Employees
│   │   ├───HealthAssessment
│   │   ├───HealthcampParticipants
│   │   ├───HealthCamps
│   │   ├───HealthcareServices
│   │   ├───HomepageOverview
│   │   ├───Identity
│   │   ├───IntakeForms
│   │   ├───Lookups
│   │   ├───Menus
│   │   ├───Notifications
│   │   ├───Organizations
│   │   ├───Patients
│   │   ├───Rbac
│   │   ├───Reporting
│   │   ├───Subcontactors
│   │   └───Users
│   ├───Security
│   │   ├───Adapters
│   │   └───Policies
│   └───Seeders
└───Salubrity.Shared
    ├───bin
    │   ├───Debug
    │   │   └───net9.0
    │   ├───Production
    │   │   └───net9.0
    │   └───Release
    │       └───net9.0
    ├───Constants
    ├───Exceptions
    ├───Extensions
    ├───obj
    │   ├───Debug
    │   │   └───net9.0
    │   │       ├───ref
    │   │       └───refint
    │   ├───Production
    │   │   └───net9.0
    │   │       ├───ref
    │   │       └───refint
    │   └───Release
    │       └───net9.0
    │           ├───ref
    │           └───refint
    ├───Responses
    └───Security
        └───Config
        ```
```
````
