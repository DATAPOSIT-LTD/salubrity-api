using Salubrity.Application.DTOs.Employees;
using Salubrity.Application.DTOs.Users;
using Salubrity.Application.Interfaces.Repositories;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Application.Interfaces.Security;
using Salubrity.Application.Interfaces.Services.Employee;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.EmployeeServices;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repo;
    private readonly IRoleRepository _roleRepo;
    private readonly IPasswordHasher _passwordHasher;

    public EmployeeService(IEmployeeRepository repo, IPasswordHasher passwordHasher, IRoleRepository roleRepo)
    {
        _repo = repo;
        _passwordHasher = passwordHasher;
        _roleRepo = roleRepo;
    }

    public async Task<List<EmployeeResponseDto>> GetAllAsync()
    {
        var employees = await _repo.GetAllAsync();
        return [.. employees.Select(e => new EmployeeResponseDto
        {
            Id = e.Id,
            OrganizationId = e.OrganizationId,
            JobTitleId = e.JobTitleId,
            JobTitleName = e.JobTitle?.Name,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department?.Name,
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
        })];
    }

    public async Task<EmployeeResponseDto?> GetByIdAsync(Guid id)
    {
        var employee = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Employee not found");

        return new EmployeeResponseDto
        {
            Id = employee.Id,
            OrganizationId = employee.OrganizationId,
            JobTitleId = employee.JobTitleId,
            JobTitleName = employee.JobTitle?.Name,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name,
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
        var hashed = _passwordHasher.HashPassword(dto.User.Password);
        var normalizedEmail = dto.User.Email.Trim().ToLowerInvariant();
        var userId = Guid.NewGuid();

        //  Get the Patient Role ID
        var patientRole = await _roleRepo.FindByNameAsync("Patient");
        if (patientRole == null)
            throw new Exception("Patient role not found");

        //  Create User
        var user = new User
        {
            FirstName = dto.User.FirstName.Trim(),
            MiddleName = dto.User.MiddleName?.Trim(),
            LastName = dto.User.LastName.Trim(),
            Id = userId,
            Phone = dto.User.Phone?.Trim(),
            GenderId = dto.User.GenderId,
            Email = normalizedEmail,
            PasswordHash = hashed,
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UserRoles =
        [
            new UserRole
            {
                UserId = userId,
                RoleId = patientRole.Id //  Assign Patient Role
            }
        ]
        };

        // Create Employee
        var entity = new Employee
        {
            Id = Guid.NewGuid(),
            OrganizationId = dto.OrganizationId,
            JobTitleId = dto.JobTitleId,
            DepartmentId = dto.DepartmentId,
            User = user
        };

        await _repo.CreateAsync(entity);

        return new EmployeeResponseDto
        {
            Id = entity.Id,
            OrganizationId = entity.OrganizationId,
            JobTitleId = entity.JobTitleId,
            DepartmentId = entity.DepartmentId,
            User = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                DateOfBirth = user.DateOfBirth
            }
        };
    }


    public async Task<EmployeeResponseDto> UpdateAsync(Guid id, EmployeeRequestDto dto)
    {
        Employee existingEmployee = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Employee not found");

        existingEmployee.JobTitleId = dto.JobTitleId;
        existingEmployee.DepartmentId = dto.DepartmentId;
        existingEmployee.OrganizationId = dto.OrganizationId;

        existingEmployee.User.FirstName = dto.User.FirstName;
        existingEmployee.User.MiddleName = dto.User.MiddleName;
        existingEmployee.User.LastName = dto.User.LastName;
        existingEmployee.User.Email = dto.User.Email;
        existingEmployee.User.Phone = dto.User.Phone;
        existingEmployee.User.DateOfBirth = dto.User.DateOfBirth;

        await _repo.UpdateAsync(existingEmployee);

        return new EmployeeResponseDto
        {
            Id = existingEmployee.Id,
            OrganizationId = existingEmployee.OrganizationId,
            JobTitleId = existingEmployee.JobTitleId,
            JobTitleName = existingEmployee.JobTitle?.Name,
            DepartmentId = existingEmployee.DepartmentId,
            DepartmentName = existingEmployee.Department?.Name,
            User = new UserResponse
            {
                Id = existingEmployee.User.Id,
                FirstName = existingEmployee.User.FirstName,
                MiddleName = existingEmployee.User.MiddleName,
                LastName = existingEmployee.User.LastName,
                Email = existingEmployee.User.Email,
                Phone = existingEmployee.User.Phone,
                DateOfBirth = existingEmployee.User.DateOfBirth
            }
        };
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

    public async Task<List<EmployeeLeanResponseDto>> GetByOrganizationAsync(Guid organizationId)
    {
        var employees = await _repo.WhereAsync(e =>
            !e.IsDeleted && e.OrganizationId == organizationId);

        return [.. employees.Select(e => new EmployeeLeanResponseDto
        {
            Id = e.Id,
            Email = e.User.Email,
            FirstName = e.User.FirstName,
            MiddleName = e.User.MiddleName,
            LastName = e.User.LastName,
            Phone = e.User.Phone,
            NationalId = e.User.NationalId,
            DateOfBirth = e.User.DateOfBirth,
            PrimaryLanguage = e.User.PrimaryLanguage,
            ProfileImage = e.User.ProfileImage,
            GenderId = e.User.GenderId,
            OrganizationId = e.OrganizationId,
            IsActive = e.User.IsActive,
            IsVerified = e.User.IsVerified,
            LastLoginAt = e.User.LastLoginAt
        })];
    }

}
