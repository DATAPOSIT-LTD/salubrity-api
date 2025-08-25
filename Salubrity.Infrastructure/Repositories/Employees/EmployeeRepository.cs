using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Shared.Exceptions;
using System.Linq.Expressions;

namespace Salubrity.Infrastructure.Repositories.Employees;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Employee>> GetAllAsync()
    {
        return await _context.Employees
            .Include(e => e.User)
            .Include(e => e.Organization)
            .Include(e => e.JobTitle)
            .Include(e => e.Department)
            .ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        return await _context.Employees
            .Include(e => e.User)
            .Include(e => e.Organization)
            .Include(e => e.JobTitle)
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<Employee>> WhereAsync(Expression<Func<Employee, bool>> predicate)
    {
        return await _context.Employees
            .Include(e => e.User)
            .Include(e => e.JobTitle)
            .Include(e => e.Department)
            .Where(predicate)
            .ToListAsync();
    }

    public async Task CreateEmployeeAndPatientAsync(Employee employee, Patient patient, CancellationToken ct = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            _context.Employees.Add(employee);  // Includes navigation to User
            _context.Patients.Add(patient);

            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<Employee> CreateAsync(Employee entity)
    {
        _context.Employees.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Employee> UpdateAsync(Employee entity)
    {
        _context.Employees.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            throw new NotFoundException("Employee not found");

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Employees.AnyAsync(e => e.Id == id);
    }
}
