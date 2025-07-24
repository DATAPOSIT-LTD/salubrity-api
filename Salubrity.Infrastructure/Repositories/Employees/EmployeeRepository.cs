using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Employees;
using Salubrity.Application.DTOs.Users;
using Salubrity.Application.Interfaces.Repositories;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Infrastructure.Repositories.Employees;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;


    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeeResponseDto>> GetAllAsync()//reference a Dto
    {
        //return what is in the entitty
        var employees = await _context.Employees
            .Include(e => e.User)
            .Include(e => e.Organization)
            .ToListAsync();

        var list = new List<EmployeeResponseDto>();
        foreach (var employee in employees)
        {
            list.Add(new EmployeeResponseDto
            {
                Id = employee.Id,
                OrganizationId = employee.OrganizationId,
                OrganizationName = employee.Organization?.BusinessName ?? "N/A",
                JobTitle = employee.JobTitle,
                Department = employee.Department,
                User = new UserResponse
                {
                    Id = employee.User.Id,
                    FirstName = employee.User.FirstName,
                    MiddleName = employee.User.MiddleName,
                    LastName = employee.User.LastName,
                    Email = employee.User.Email,
                    Phone = employee.User.Phone,
                    DateOfBirth = employee.User.DateOfBirth
                }
            });
        }
        return list;

    }

    public async Task<EmployeeResponseDto?> GetByIdAsync(Guid id)
    {
        var employeeId = await _context.Employees
            .Include(e => e.User)
            .Include(e => e.Organization)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (employeeId == null) return null;
        return new EmployeeResponseDto
        {
            Id = employeeId.Id,
            OrganizationId = employeeId.OrganizationId,
            OrganizationName = employeeId.Organization?.BusinessName ?? "N/A",
            JobTitle = employeeId.JobTitle,
            Department = employeeId.Department,
            User = new UserResponse
            {
                Id = employeeId.User.Id,
                FirstName = employeeId.User.FirstName,
                MiddleName = employeeId.User.MiddleName,
                LastName = employeeId.User.LastName,
                Email = employeeId.User.Email,
                Phone = employeeId.User.Phone,
                DateOfBirth = employeeId.User.DateOfBirth,
               
            }
        };
    }

    public async Task <Employee> CreateAsync(Employee entity)
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
        if (employee == null) throw new NotFoundException("Employee not found");

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Employees.AnyAsync(e => e.Id == id);
    }

}