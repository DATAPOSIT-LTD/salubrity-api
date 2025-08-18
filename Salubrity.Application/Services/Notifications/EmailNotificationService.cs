#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Salubrity.Application.Interfaces;
using Salubrity.Application.DTOs.Email;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.Join;
using Salubrity.Domain.Entities.HealthAssesment;
using Salubrity.Application.DTOs.HealthAssessment;

namespace Salubrity.Application.Services;

public class EmailNotificationService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IEmailService emailService,
        ILogger<EmailNotificationService> logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendAppointmentConfirmationAsync(HealthCampParticipant healthCampParticipant, HealthCamp healthCamp)
    {
        if (healthCampParticipant is null) throw new ArgumentNullException(nameof(healthCampParticipant));
        if (healthCamp is null) throw new ArgumentNullException(nameof(healthCamp));
        if (healthCampParticipant.User is null)
            throw new InvalidOperationException("Participant.User is null. Ensure User is loaded before emailing.");

        var toEmail = healthCampParticipant.User.Email;
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new InvalidOperationException("Recipient email is missing.");

        try
        {
            var request = new EmailRequestDto
            {
                ToEmail = toEmail,
                Subject = "Appointment Confirmation - Salubrity Centre",
                TemplateKey = "appointment-confirmation",
                Model = new
                {
                    patient = new
                    {
                        first_name = healthCampParticipant.User.FirstName ?? string.Empty,
                        last_name = healthCampParticipant.User.LastName ?? string.Empty,
                        email = healthCampParticipant.User.Email ?? string.Empty
                    },
                    appointment = new
                    {
                        date = healthCamp.StartDate,
                        time = healthCamp.StartTime,
                        location = new
                        {
                            name = healthCamp.Location ?? string.Empty,
                            address = healthCamp.Location ?? string.Empty
                        },
                    },
                    cta_url = $"https://portal.salubritycentre.com/appointments/{healthCamp.Id}",
                    cta_text = "View Appointment Details"
                }
            };

            await _emailService.SendAsync(request);

            _logger.LogInformation(
                "Appointment confirmation sent to participant {ParticipantId} for camp {CampId}",
                healthCampParticipant.Id, healthCamp.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send appointment confirmation for participant {ParticipantId} / camp {CampId}",
                healthCampParticipant.Id, healthCamp.Id);
            throw;
        }
    }

    public async Task SendHealthAssessmentResultsAsync(HealthCampParticipant healthCampParticipant, HealthAssessmentResult results)
    {
        if (healthCampParticipant is null) throw new ArgumentNullException(nameof(healthCampParticipant));
        if (results is null) throw new ArgumentNullException(nameof(results));
        if (healthCampParticipant.User is null)
            throw new InvalidOperationException("Participant.User is null. Ensure User is loaded before emailing.");

        var toEmail = healthCampParticipant.User.Email;
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new InvalidOperationException("Recipient email is missing.");

        try
        {
            var metrics = (results.Metrics ?? Enumerable.Empty<AssessmentMetricDto>())
                .Select(m =>
                {
                    // Derive a status string robustly whether it's a lookup entity, enum, or something else.
                    var statusString = m.HealthMetricStatusName switch
                    {
                        null => "Unknown",
                        _ => m.HealthMetricStatusName.ToString() ?? "Unknown"
                    };

                    return new
                    {
                        name = m.Name ?? string.Empty,
                        value = m.Value,
                        reference_range = m.ReferenceRange ?? string.Empty,
                        status = statusString,
                        status_color = GetStatusColor(statusString),
                        badge_bg = GetBadgeBackground(statusString),
                        badge_color = GetBadgeTextColor(statusString)
                    };
                });

            var recommendations = (results.Recommendations ?? Array.Empty<AssessmentRecommendationDto>())
                .Select(r => new
                {
                    title = r.Title ?? string.Empty,
                    description = r.Description ?? string.Empty,
                    priority = r.Priority?.ToString()
                });

            var request = new EmailRequestDto
            {
                ToEmail = toEmail,
                Subject = "Your Health Assessment Results - Salubrity Centre",
                TemplateKey = "health-assessment-results",
                Model = new
                {
                    patient = new
                    {
                        first_name = healthCampParticipant.User.FirstName ?? string.Empty,
                        last_name = healthCampParticipant.User.LastName ?? string.Empty
                    },
                    results = new
                    {
                        overall_score = results.OverallScore,
                        score_color = GetScoreColor(results.OverallScore),
                        score_description = GetScoreDescription(results.OverallScore),
                        metrics,
                        recommendations,
                        reviewed_by = results.ReviewedBy?.FullName ?? "Clinician"
                    },
                    cta_url = $"https://portal.salubritycentre.com/health-results/{results.Id}",
                    cta_text = "View Full Results"
                }
            };

            await _emailService.SendAsync(request);

            _logger.LogInformation(
                "Health assessment results sent to participant {ParticipantId} (results {ResultsId})",
                healthCampParticipant.Id, results.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send health assessment results to participant {ParticipantId} (results {ResultsId})",
                healthCampParticipant.Id, results.Id);
            throw;
        }
    }

    // Score helpers
    private static string GetScoreColor(int score) => score switch
    {
        >= 80 => "#28a745", // Green
        >= 60 => "#ffc107", // Yellow
        _ => "#dc3545"       // Red
    };

    private static string GetScoreDescription(int score) => score switch
    {
        >= 80 => "Excellent - Keep up the great work!",
        >= 60 => "Good - Room for improvement",
        _ => "Needs attention - Let's work together"
    };

    // Status helpers (string-based; works with lookup names, enums, etc.)
    private static string GetStatusColor(string status) => Normalize(status) switch
    {
        "optimal" => "#28a745",
        "normal" => "#17a2b8",
        "borderline" => "#ffc107",
        "high" => "#fd7e14",
        "critical" => "#dc3545",
        _ => "#6c757d"
    };

    private static string GetBadgeBackground(string status) => Normalize(status) switch
    {
        "optimal" => "#d4edda",
        "normal" => "#d1ecf1",
        "borderline" => "#fff3cd",
        "high" => "#ffeaa7",
        "critical" => "#f8d7da",
        _ => "#e2e3e5"
    };

    private static string GetBadgeTextColor(string status) => Normalize(status) switch
    {
        "optimal" => "#155724",
        "normal" => "#0c5460",
        "borderline" => "#856404",
        "high" => "#fd7e14",
        "critical" => "#721c24",
        _ => "#6c757d"
    };

    private static string Normalize(string? s) => (s ?? string.Empty).Trim().ToLowerInvariant();
}
