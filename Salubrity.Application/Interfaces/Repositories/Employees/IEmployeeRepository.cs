using Salubrity.Application.DTOs.Employees;
using Salubrity.Domain.Entities.Identity;

namespace Salubrity.Application.Interfaces.Repositories;

public interface IEmployeeRepository
{
    Task<List<EmployeeResponseDto>> GetAllAsync();
    Task<EmployeeResponseDto?> GetByIdAsync(Guid id);
    Task <Employee> CreateAsync(Employee entity);
    Task<Employee>UpdateAsync(Employee entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    
}
