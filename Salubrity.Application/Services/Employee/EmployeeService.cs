using Salubrity.Application.DTOs.Employees;
using Salubrity.Application.DTOs.Users;
using Salubrity.Application.Interfaces.Repositories;
using Salubrity.Application.Interfaces.Services.Employee;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.EmployeeServices;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repo;
    

    public EmployeeService(IEmployeeRepository repo)
    {
        _repo = repo;
       
    }

    public async Task<List<EmployeeResponseDto>> GetAllAsync()
    {
        var employees = await _repo.GetAllAsync();
        return employees.Select(e => new EmployeeResponseDto
        {
            Id = e.Id,
            OrganizationId = e.OrganizationId,
            JobTitle = e.JobTitle,
            Department = e.Department,
            User = new UserResponse
            {
                Id = e.User.Id,
                FirstName = e.User.FirstName,
                MiddleName = e.User.MiddleName,
                LastName = e.User.LastName,
                Email = e.User.Email,
                Phone = e.User.Phone,
                DateOfBirth = e.User.DateOfBirth
            }
        }).ToList();
    }

    public async Task<EmployeeResponseDto?> GetByIdAsync(Guid id)
    {
        var employee = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Employee not found");
        return new EmployeeResponseDto
        {
            Id = employee.Id,
            OrganizationId = employee.OrganizationId,
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
        };
    }

    public async Task<EmployeeResponseDto> CreateAsync(EmployeeRequestDto dto)
{
    var entity = new Employee
    {
        Id = Guid.NewGuid(),
        OrganizationId = dto.OrganizationId,
        JobTitle = dto.JobTitle,
        Department = dto.Department,
        User = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.User.FirstName,
            MiddleName = dto.User.MiddleName,
            LastName = dto.User.LastName,
            Email = dto.User.Email,
            Phone = dto.User.Phone,
            DateOfBirth = dto.User.DateOfBirth,
        }
    };
        return new EmployeeResponseDto
        {
            Id = entity.Id,
            OrganizationId = entity.OrganizationId,
            JobTitle = entity.JobTitle,
            Department = entity.Department,
            User = new UserResponse
            {
                Id = entity.User.Id,
                FirstName = entity.User.FirstName,
                MiddleName = entity.User.MiddleName,
                LastName = entity.User.LastName,
                Email = entity.User.Email,
                Phone = entity.User.Phone,
                DateOfBirth = entity.User.DateOfBirth
            }
        };
}

    public async Task<EmployeeResponseDto> UpdateAsync(Guid id, EmployeeRequestDto dto)
    {
       throw new NotImplementedException("Update functionality is not implemented yet.");
        // You would typically fetch the existing employee, update its properties, and save it back.
        // var existingEmployee = await _repo.GetByIdAsync(id);
        // if (existingEmployee == null) throw new NotFoundException("Employee not found");
        // Update properties here...
        // await _repo.UpdateAsync(existingEmployee);
        // return _repo.Map<EmployeeResponseDto>(existingEmployee);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (!await _repo.ExistsAsync(id))
            throw new NotFoundException("Employee not found");

        await _repo.DeleteAsync(id);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _repo.ExistsAsync(id);
    }
}
