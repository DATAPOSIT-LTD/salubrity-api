using Salubrity.Domain.Entities.Identity;
using System.Linq.Expressions;

namespace Salubrity.Application.Interfaces.Repositories;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetAllAsync();              
    Task<Employee?> GetByIdAsync(Guid id);          
    Task<Employee> CreateAsync(Employee entity);     
    Task<Employee> UpdateAsync(Employee entity);     
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<List<Employee>> WhereAsync(Expression<Func<Employee, bool>> predicate);

}
