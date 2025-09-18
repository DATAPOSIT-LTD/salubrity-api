using AutoMapper;
using Salubrity.Application.DTOs.Subcontractor;
using Salubrity.Application.Interfaces.Repositories;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Application.Interfaces.Repositories.Users;
using Salubrity.Application.Interfaces.Security;
using Salubrity.Application.Interfaces.Services;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Domain.Entities.Subcontractor;
using Salubrity.Shared.Exceptions;
using Salubrity.Shared.Extensions;

namespace Salubrity.Application.Services.Subcontractor
{
    public class SubcontractorService : ISubcontractorService
    {
        private readonly ISubcontractorRepository _repo;
        private readonly IRoleRepository _roleRepo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IPasswordGenerator _passwordGenerator;
        private readonly IUserRepository _userRepository;
        private readonly IIndustryService _industryService;
        private readonly ISubcontractorCampAssignmentRepository _campAssignmentRepo;
        private readonly IMapper _mapper;

        public SubcontractorService(
            ISubcontractorRepository repo,
            IRoleRepository roleRepo,
            IPasswordHasher passwordHasher,
            IPasswordGenerator passwordGenerator,
            IUserRepository userRepository,
            IIndustryService industryService,
            ISubcontractorCampAssignmentRepository campAssignmentRepository,
            IMapper mapper)
        {
            _repo = repo;
            _roleRepo = roleRepo;
            _passwordHasher = passwordHasher;
            _passwordGenerator = passwordGenerator;
            _mapper = mapper;
            _userRepository = userRepository;
            _industryService = industryService;
            _campAssignmentRepo = campAssignmentRepository;
        }

        // public async Task<SubcontractorDto> CreateAsync(CreateSubcontractorDto dto)
        // {
        //     if (dto.StatusId is null)
        //         throw new ValidationException(["Status is required"]);

        //     if (dto.User == null)
        //         throw new ValidationException(["User info is required"]);

        //     var industry = dto.IndustryId.HasValue
        //         ? await _industryService.GetByIdAsync(dto.IndustryId.Value)
        //         : await _industryService.GetDefaultIndustryAsync();

        //     if (industry == null)
        //         throw new NotFoundException("Industry not found");

        //     var existing = await _userRepository.FindUserByEmailAsync(dto.User.Email.Trim().ToLowerInvariant());
        //     if (existing is not null)
        //         throw new ValidationException(["A user with this email already exists."]);

        //     var subcontractorRole = await _roleRepo.FindByNameAsync("Subcontractor") ?? throw new NotFoundException("Subcontractor role not found");

        //     var userId = Guid.NewGuid();
        //     var rawPassword = _passwordGenerator.Generate();

        //     var user = new User
        //     {
        //         Id = userId,
        //         FirstName = dto.User.FirstName?.Trim(),
        //         MiddleName = dto.User.MiddleName?.Trim(),
        //         LastName = dto.User.LastName?.Trim(),
        //         Email = dto.User.Email.Trim().ToLowerInvariant(),
        //         Phone = dto.User.Phone?.Trim(),
        //         // NationalId = dto.User.NationalId?.Trim(),
        //         GenderId = dto.User.GenderId,
        //         DateOfBirth = dto.User.DateOfBirth.ToUtcSafe(),
        //         // PrimaryLanguage = dto.User.PrimaryLanguage,
        //         PasswordHash = _passwordHasher.HashPassword(rawPassword),
        //         IsActive = true,
        //         IsVerified = false,
        //         CreatedAt = DateTime.UtcNow,
        //         UserRoles =
        //         [
        //             new UserRole
        //             {
        //                 RoleId = subcontractorRole.Id,
        //                 UserId = userId
        //             }
        //         ]
        //     };

        //     var subcontractor = new Domain.Entities.Subcontractor.Subcontractor
        //     {
        //         Id = Guid.NewGuid(),
        //         User = user,
        //         IndustryId = industry.Id,
        //         LicenseNumber = dto.LicenseNumber,
        //         Bio = dto.Bio,
        //         StatusId = dto.StatusId.Value,
        //         Specialties = dto.SpecialtyIds?
        //             .Select(id => new SubcontractorSpecialty { ServiceId = id })
        //             .ToList() ?? []
        //     };

        //     await _repo.AddAsync(subcontractor);

        //     foreach (var roleId in dto.SubcontractorRoleIds)
        //     {
        //         var isPrimary = dto.PrimaryRoleId.HasValue && dto.PrimaryRoleId.Value == roleId;
        //         await _repo.AssignRoleAsync(subcontractor.Id, roleId, isPrimary);
        //         await _repo.SaveChangesAsync();
        //     }

        //     await _repo.SaveChangesAsync();

        //     return _mapper.Map<SubcontractorDto>(subcontractor);
        // }

        public async Task<SubcontractorDto> CreateAsync(CreateSubcontractorDto dto)
        {
            if (dto.StatusId is null)
                throw new ValidationException(["Status is required"]);

            if (dto.User == null)
                throw new ValidationException(["User info is required"]);

            var industry = dto.IndustryId.HasValue
                ? await _industryService.GetByIdAsync(dto.IndustryId.Value)
                : await _industryService.GetDefaultIndustryAsync();

            if (industry == null)
                throw new NotFoundException("Industry not found");

            var email = dto.User.Email?.Trim().ToLowerInvariant();
            var phone = dto.User.Phone?.Trim();

            // 🔎 Look for existing user by email or phone
            var existingUser = await _userRepository.FindUserByEmailAsync(email)
                ?? (!string.IsNullOrWhiteSpace(phone) ? await _userRepository.FindUserByPhoneAsync(phone) : null);

            User user;
            if (existingUser is not null)
            {
                // ✅ Update existing user
                existingUser.FirstName = dto.User.FirstName?.Trim();
                existingUser.MiddleName = dto.User.MiddleName?.Trim();
                existingUser.LastName = dto.User.LastName?.Trim();
                existingUser.Phone = phone;
                existingUser.GenderId = dto.User.GenderId;
                existingUser.DateOfBirth = dto.User.DateOfBirth.ToUtcSafe();
                existingUser.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateUserAsync(existingUser);

                // Ensure subcontractor role
                var subcontractorRole = await _roleRepo.FindByNameAsync("Subcontractor")
                    ?? throw new NotFoundException("Subcontractor role not found");

                if (!existingUser.UserRoles.Any(ur => ur.RoleId == subcontractorRole.Id))
                {
                    existingUser.UserRoles.Add(new UserRole
                    {
                        RoleId = subcontractorRole.Id,
                        UserId = existingUser.Id
                    });

                    await _userRepository.UpdateUserAsync(existingUser);
                }

                user = existingUser;
            }
            else
            {
                // New user
                var userId = Guid.NewGuid();
                var rawPassword = _passwordGenerator.Generate();

                var subcontractorRole = await _roleRepo.FindByNameAsync("Subcontractor")
                    ?? throw new NotFoundException("Subcontractor role not found");

                user = new User
                {
                    Id = userId,
                    FirstName = dto.User.FirstName?.Trim(),
                    MiddleName = dto.User.MiddleName?.Trim(),
                    LastName = dto.User.LastName?.Trim(),
                    Email = email,
                    Phone = phone,
                    GenderId = dto.User.GenderId,
                    DateOfBirth = dto.User.DateOfBirth.ToUtcSafe(),
                    PasswordHash = _passwordHasher.HashPassword(rawPassword),
                    IsActive = true,
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    UserRoles =
                    [
                        new UserRole
                {
                    RoleId = subcontractorRole.Id,
                    UserId = userId
                }
                    ]
                };

                await _userRepository.AddUserAsync(user);
            }

            //  Do not attach User navigation, only set FK
            var existingSubcontractor = await _repo.GetByUserIdAsync(user.Id);
            if (existingSubcontractor is not null)
            {
                throw new ValidationException(["This user is already registered as a subcontractor."]);
            }

            var subcontractor = new Domain.Entities.Subcontractor.Subcontractor
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,   //  only FK, avoids duplicate insert
                IndustryId = industry.Id,
                LicenseNumber = dto.LicenseNumber,
                Bio = dto.Bio,
                StatusId = dto.StatusId.Value,
                Specialties = dto.SpecialtyIds?
                    .Select(id => new SubcontractorSpecialty { ServiceId = id })
                    .ToList() ?? []
            };

            await _repo.AddAsync(subcontractor);

            foreach (var roleId in dto.SubcontractorRoleIds)
            {
                var isPrimary = dto.PrimaryRoleId.HasValue && dto.PrimaryRoleId.Value == roleId;
                await _repo.AssignRoleAsync(subcontractor.Id, roleId, isPrimary);
            }

            await _repo.SaveChangesAsync();

            return _mapper.Map<SubcontractorDto>(subcontractor);
        }



        public async Task<List<SubcontractorDto>> GetAllAsync()
        {
            var subs = await _repo.GetAllWithDetailsAsync();
            return _mapper.Map<List<SubcontractorDto>>(subs);
        }

        public async Task<SubcontractorDto> UpdateAsync(Guid id, UpdateSubcontractorDto dto)
        {
            var sub = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Subcontractor not found");

            sub.LicenseNumber = dto.LicenseNumber;
            sub.Bio = dto.Bio;

            if (dto.StatusId is null)
                throw new ValidationException(["Status is required"]);

            sub.StatusId = dto.StatusId.Value;

            await _repo.UpdateSubAsync(sub, CancellationToken.None);
            return _mapper.Map<SubcontractorDto>(sub);
        }

        public async Task<SubcontractorDto> GetByIdAsync(Guid id)
        {
            var sub = await _repo.GetByIdWithDetailsAsync(id) ?? throw new NotFoundException("Subcontractor not found");
            return _mapper.Map<SubcontractorDto>(sub);
        }

        public async Task AssignRoleAsync(Guid subcontractorId, AssignSubcontractorRoleDto dto)
        {
            foreach (var roleId in dto.SubcontractorRoleIds)
            {
                var isPrimary = dto.PrimaryRoleId.HasValue && dto.PrimaryRoleId == roleId;
                await _repo.AssignRoleAsync(subcontractorId, roleId, isPrimary);
            }

            await _repo.SaveChangesAsync();
        }

        public async Task AssignSpecialtyAsync(Guid subcontractorId, CreateSubcontractorSpecialtyDto dto)
        {
            await _repo.AssignSpecialtyAsync(subcontractorId, dto.ServiceId);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid subcontractorId, Guid performedByUserId, CancellationToken ct = default)
        {
            var sub = await _repo.GetByIdAsync(subcontractorId)
                      ?? throw new NotFoundException("Subcontractor not found");

            // Guardrails: block if they have active/ongoing camp assignments
            if (await _campAssignmentRepo.HasActiveAssignmentsAsync(sub.Id, ct))
                throw new ValidationException(["Cannot delete subcontractor while assigned to an active camp."]);

            // Optional: block if they’re the only assignee for an upcoming camp, etc.

            // Soft-delete
            sub.IsDeleted = true;
            sub.DeletedAt = DateTime.UtcNow;
            sub.DeletedBy = performedByUserId;

            // Optional: also mark their User account inactive (business-dependent)
            // if (sub.User is not null) sub.User.IsActive = false;

            await _repo.UpdateSubAsync(sub, ct);
        }

    }

}
