namespace Salubrity.Application.Enums;

public enum CampParticipantServeStatus
{
    All = 0,

    Served = 1,        // Intake form submitted for this station/service

    NotServed = 2,     // No intake form submission exists (yet)

    Suspended = 3      // FUTURE: explicitly excluded from service workflow
}
