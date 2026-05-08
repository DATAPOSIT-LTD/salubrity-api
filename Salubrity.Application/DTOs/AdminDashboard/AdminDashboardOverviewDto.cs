// File: Application/DTOs/AdminDashboard/AdminDashboardOverviewDto.cs
namespace Salubrity.Application.DTOs.AdminDashboard;

public class AdminDashboardOverviewDto
{
    public OngoingCampSummaryDto? OngoingCamp { get; set; }
    public UpcomingCampSummaryDto? NextUpcomingCamp { get; set; }
    public LastCompletedCampSummaryDto? LastCompletedCamp { get; set; }
    public YtdTotalsDto YtdTotals { get; set; } = new();
    public List<PendingFinalReportDto> PendingFinalReportPublishes { get; set; } = new();
}

public class OngoingCampSummaryDto
{
    public Guid CampId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int DayNumber { get; set; }
    public int DaysTotal { get; set; }
    public int ExpectedAttendees { get; set; }
    public int TotalAttendees { get; set; }
    public int ParticipationRate { get; set; }
    public int FormsSubmittedToday { get; set; }
    public int FormsSubmittedTotal { get; set; }
    public int ActiveStationsToday { get; set; }
    public int RegisteredSubcontractors { get; set; }
}

public class UpcomingCampSummaryDto
{
    public Guid CampId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public int DaysUntil { get; set; }
    public int ExpectedAttendees { get; set; }
}

public class LastCompletedCampSummaryDto
{
    public Guid CampId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateTime? EndDate { get; set; }
    public int TotalAttendees { get; set; }
    public int ExpectedAttendees { get; set; }
    public int ParticipationRate { get; set; }
    public string? TopFindingName { get; set; }
    public int TopFindingPercent { get; set; }
}

public class YtdTotalsDto
{
    public int CampsRun { get; set; }
    public int PatientsScreened { get; set; }
    public int OrganizationsServed { get; set; }
    public int Year { get; set; }
}

public class PendingFinalReportDto
{
    public Guid CampId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateTime? EndDate { get; set; }
    public int DaysSinceCompletion { get; set; }
}
