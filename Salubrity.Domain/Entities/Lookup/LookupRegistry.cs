// File: Application/Lookups/LookupRegistry.cs
using System;
using System.Collections.Generic;
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Domain.Entities.Subcontractor;

namespace Salubrity.Application.Lookups;

public static class LookupRegistry
{
    public static readonly Dictionary<string, Type> LookupMap = new()
    {
        { "languages", typeof(Language) },
        { "genders", typeof(Gender) },
        { "departments", typeof(Department) },
        { "jobtitles", typeof(JobTitle) },
        { "subcontractorroles", typeof(SubcontractorRole) },
        { "subcontractorstatuses", typeof(SubcontractorStatus) },
        { "subcontractorhealthcampassignmentstatuses", typeof(SubcontractorHealthCampAssignmentStatus) },
        { "healthcampstatuses", typeof(HealthCampStatus) },
        {"intakeformresponsestatuses", typeof(IntakeFormResponseStatus) },
        {"healthassessmentformtypes", typeof(HealthAssessmentFormType) },
        {"billingstatus", typeof(BillingStatus) }
    };

}
