using AutoMapper;
using Salubrity.Application.DTOs.Subcontractor;
using Salubrity.Application.Interfaces.Repositories;
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
        private readonly IMapper _mapper;

        public SubcontractorService(
            ISubcontractorRepository repo,
            IRoleRepository roleRepo,
            IPasswordHasher passwordHasher,
            IPasswordGenerator passwordGenerator,
            IUserRepository userRepository,
            IIndustryService industryService,
            IMapper mapper)
        {
            _repo = repo;
            _roleRepo = roleRepo;
            _passwordHasher = passwordHasher;
            _passwordGenerator = passwordGenerator;
            _mapper = mapper;
            _userRepository = userRepository;
            _industryService = industryService;
        }

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

            var existing = await _userRepository.FindUserByEmailAsync(dto.User.Email.Trim().ToLowerInvariant());
            if (existing is not null)
                throw new ValidationException(["A user with this email already exists."]);

            var subcontractorRole = await _roleRepo.FindByNameAsync("Subcontractor") ?? throw new NotFoundException("Subcontractor role not found");

            var userId = Guid.NewGuid();
            var rawPassword = _passwordGenerator.Generate();

            var user = new User
            {
                Id = userId,
                FirstName = dto.User.FirstName?.Trim(),
                MiddleName = dto.User.MiddleName?.Trim(),
                LastName = dto.User.LastName?.Trim(),
                Email = dto.User.Email.Trim().ToLowerInvariant(),
                Phone = dto.User.Phone?.Trim(),
                // NationalId = dto.User.NationalId?.Trim(),
                GenderId = dto.User.GenderId,
                DateOfBirth = dto.User.DateOfBirth.ToUtcSafe(),
                // PrimaryLanguage = dto.User.PrimaryLanguage,
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

            var subcontractor = new Domain.Entities.Subcontractor.Subcontractor
            {
                Id = Guid.NewGuid(),
                User = user,
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

            await _repo.SaveChangesAsync();
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
    }
}
