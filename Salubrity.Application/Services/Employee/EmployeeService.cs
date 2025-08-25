using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Salubrity.Application.DTOs.Employees;
using Salubrity.Application.DTOs.Users;
using Salubrity.Application.Interfaces.Repositories;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Application.Interfaces.Repositories.Patients;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Application.Interfaces.Repositories.Users;
using Salubrity.Application.Interfaces.Security;
using Salubrity.Application.Interfaces.Services.Employee;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Shared.Exceptions;
using Salubrity.Shared.Extensions;

namespace Salubrity.Application.Services.EmployeeServices;


public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repo;
    private readonly IRoleRepository _roleRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILookupRepository<JobTitle> _jobTitleRepo;
    private readonly ILookupRepository<Department> _departmentRepo;
    private readonly IOrganizationRepository _organizationRepo;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly ILookupRepository<Gender> _genderRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IUserRepository _userRepo;

    public EmployeeService(
     IEmployeeRepository repo,
     IPasswordHasher passwordHasher,
     IPasswordGenerator passwordGenerator,
     IRoleRepository roleRepo,
     ILookupRepository<JobTitle> jobTitleRepo,
     ILookupRepository<Department> departmentRepo,
     ILookupRepository<Gender> genderRepo,
     IOrganizationRepository organizationRepo,
     IPatientRepository patientRepo,
     IUserRepository userRepo
     )
    {
        _repo = repo;
        _passwordHasher = passwordHasher;
        _passwordGenerator = passwordGenerator;
        _roleRepo = roleRepo;
        _jobTitleRepo = jobTitleRepo;
        _departmentRepo = departmentRepo;
        _organizationRepo = organizationRepo;
        _genderRepo = genderRepo;
        _patientRepo = patientRepo;
        _userRepo = userRepo;
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

    public async Task<EmployeeResponseDto> CreateAsync(EmployeeRequestDto dto, CancellationToken ct = default)
    {
        var normalizedEmail = dto.User.Email.Trim().ToLowerInvariant();

        // 1) Uniqueness checks
        if (await _userRepo.FindUserByEmailAsync(normalizedEmail) is not null)
            throw new ConflictException("A user with this email already exists.");

        var patientRole = await _roleRepo.FindByNameAsync("Patient");
        if (patientRole is null)
            throw new NotFoundException("Patient role not found.");

        var userId = Guid.NewGuid();
        var hashed = _passwordHasher.HashPassword(_passwordGenerator.Generate());

        // 2) Build User
        var user = new User
        {
            Id = userId,
            FirstName = dto.User.FirstName.Trim(),
            MiddleName = dto.User.MiddleName?.Trim(),
            LastName = dto.User.LastName.Trim(),
            Phone = dto.User.Phone?.Trim(),
            GenderId = dto.User.GenderId,
            Email = normalizedEmail,
            PasswordHash = hashed,
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UserRoles =
            [
                new UserRole { UserId = userId, RoleId = patientRole.Id }
            ]
        };

        // 3) Build Employee (attach user)
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            OrganizationId = dto.OrganizationId,
            JobTitleId = dto.JobTitleId,
            DepartmentId = dto.DepartmentId,
            User = user
        };

        // 4) Create Patient
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PrimaryOrganizationId = dto.OrganizationId,
            Notes = "Auto-created for employee",
            CreatedAt = DateTime.UtcNow
        };

        // 5) Delegate full transactional save to repository
        await _repo.CreateEmployeeAndPatientAsync(employee, patient, ct);

        // 6) Return DTO
        return new EmployeeResponseDto
        {
            Id = employee.Id,
            OrganizationId = employee.OrganizationId,
            JobTitleId = employee.JobTitleId,
            DepartmentId = employee.DepartmentId,
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





    public async Task<BulkUploadResultDto> BulkCreateFromCsvAsync(IFormFile file)
    {
        var result = new BulkUploadResultDto();
        var patientRole = await _roleRepo.FindByNameAsync("Patient");
        if (patientRole == null)
            throw new Exception("Patient role not found");

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        var rowIndex = 1;
        var records = csv.GetRecordsAsync<EmployeeCsvRowDto>();
        await foreach (var row in records)
        {
            rowIndex++;
            try
            {
                if (string.IsNullOrWhiteSpace(row.Email))
                    throw new Exception("Missing required field: Email");

                var jobTitle = await _jobTitleRepo.FindByNameAsync(row.JobTitleName.Trim());
                if (jobTitle == null) throw new Exception($"Invalid job title: {row.JobTitleName}");

                var department = await _departmentRepo.FindByNameAsync(row.DepartmentName.Trim());
                if (department == null) throw new Exception($"Invalid department: {row.DepartmentName}");

                var organization = await _organizationRepo.FindByNameAsync(row.OrganizationName.Trim());
                if (organization == null) throw new Exception($"Invalid organization: {row.OrganizationName}");

                var gender = await _genderRepo.FindByNameAsync(row.Gender.Trim());
                if (gender == null) throw new Exception($"Invalid gender: {row.Gender}");

                var GenderId = gender.Id;

                var rawPassword = _passwordGenerator.Generate();
                var userId = Guid.NewGuid();

                var user = new User
                {
                    Id = userId,
                    FirstName = row.FirstName?.Trim(),
                    MiddleName = row.MiddleName?.Trim(),
                    LastName = row.LastName?.Trim(),
                    Email = row.Email.Trim().ToLowerInvariant(),
                    Phone = row.Phone?.Trim(),
                    NationalId = row.NationalId?.Trim(),
                    GenderId = GenderId,
                    PrimaryLanguage = row.PrimaryLanguage,
                    DateOfBirth = row.DateOfBirth.ToUtcSafe(),
                    PasswordHash = _passwordHasher.HashPassword(rawPassword),
                    IsActive = true,
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    UserRoles =
                    [
                        new UserRole
                    {
                        RoleId = patientRole.Id,
                        UserId = userId
                    }
                    ]
                };

                var employee = new Domain.Entities.Identity.Employee
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organization.Id,
                    JobTitleId = jobTitle.Id,
                    DepartmentId = department.Id,
                    User = user
                };

                await _repo.CreateAsync(employee);

                //  Baked-in: Create matching Patient record
                var patient = new Patient
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PrimaryOrganizationId = organization.Id,
                    Notes = "Auto-created from employee bulk upload"
                };

                await _patientRepo.AddAsync(patient);

                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Errors.Add(new BulkUploadError
                {
                    Row = rowIndex,
                    Message = ex.Message
                });
            }
        }

        return result;
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
