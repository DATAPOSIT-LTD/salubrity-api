// File: Application/Interfaces/Services/AdminDashboard/IAdminDashboardOverviewService.cs
using Salubrity.Application.DTOs.AdminDashboard;

namespace Salubrity.Application.Interfaces.Services.AdminDashboard;

public interface IAdminDashboardOverviewService
{
    Task<AdminDashboardOverviewDto> GetOverviewAsync(int? year = null, CancellationToken ct = default);
}
