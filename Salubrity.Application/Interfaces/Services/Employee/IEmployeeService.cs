using Salubrity.Application.DTOs.Employees;

namespace Salubrity.Application.Interfaces.Services.Employee;

public interface IEmployeeService
{
    Task<List<EmployeeResponseDto>> GetAllAsync();
    Task<EmployeeResponseDto?> GetByIdAsync(Guid id);

    Task<EmployeeResponseDto> CreateAsync(EmployeeRequestDto dto);
    Task<EmployeeResponseDto> UpdateAsync(Guid id, EmployeeRequestDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}