using Salubrity.Application.DTOs.Forms;

public class ServiceStationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<string> AssignedStaff { get; set; } = []; // Names
    public int PatientsServed { get; set; }
    public int PendingService { get; set; }
    public string AvgTimePerPatient { get; set; } = "3 min";
    public int OutlierAlerts { get; set; } = 0;
}


// Returned per service the user will perform in this camp
public class AssignedServiceWithFormDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = default!;
    public Guid? ProfessionId { get; set; }   // SubcontractorRoleAssignment.Id (if any)
    public string? AssignedRole { get; set; } // Nurse / Doctor / Nutritionist (from Role.Name)

    public FormResponseDto? Form { get; set; } // null if service has no IntakeForm
}

// Whole payload for the patient page
public class CampPatientDetailWithFormsDto
{
    public Guid ParticipantId { get; set; }
    public Guid UserId { get; set; }
    public string? PatientCode { get; set; }
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public Guid CampId { get; set; }
    public string ClientName { get; set; } = default!;
    public string Venue { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = default!;

    public bool Served { get; set; }

    public List<AssignedServiceWithFormDto> Assignments { get; set; } = new();
}
