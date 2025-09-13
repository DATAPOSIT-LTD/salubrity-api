using Microsoft.IdentityModel.Tokens;
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Application.DTOs.Auth;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.DTOs.Menus;
using Salubrity.Application.Interfaces.Rbac;
using Salubrity.Application.Interfaces.Repositories;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Application.Interfaces.Repositories.Patients;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Application.Interfaces.Repositories.Users;
using Salubrity.Application.Interfaces.Security;
using Salubrity.Application.Interfaces.Services.Auth;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Application.Interfaces.Services.Menus;
using Salubrity.Application.Interfaces.Services.Notifications;
using Salubrity.Application.Interfaces.Services.Users;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.Join;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Domain.Entities.Subcontractor;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITotpService _totpService;
        private readonly IRoleRepository _roleRepository;
        private readonly IRolePermissionGroupService _rolePermissionGroupService;
        private readonly IMenuRoleService _menuRoleService;
        private readonly IIndustryRepository _industryRepository;
        private readonly ISubcontractorRepository _subcontractorRepository;
        private readonly ILookupRepository<SubcontractorStatus> _subcontractorStatusRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IOnboardingService _onboardingService;
        private readonly INotificationService _notificationService;
        private readonly IHealthCampService _campService;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IPatientNumberGeneratorService _patientNumberGeneratorService;




        public AuthService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IPasswordHasher passwordHasher,
            ITotpService totpService,
            IRoleRepository roleRepository,
            IMenuRoleService menuRoleService,
            IRolePermissionGroupService rolePermissionGroupService,
            IIndustryRepository industryRepository,
            ISubcontractorRepository subcontractorRepository,
            ILookupRepository<SubcontractorStatus> subcontractorStatusRepository,
            IPatientRepository patientRepository,
            IOnboardingService onboardingService,
            INotificationService notificationService,
            IHealthCampService campService,
            IEmployeeRepository employeeRepository,
            IOrganizationRepository organizationRepository,
            IPatientNumberGeneratorService patientNumberGeneratorService
            )
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _totpService = totpService;
            _roleRepository = roleRepository;
            _menuRoleService = menuRoleService;
            _rolePermissionGroupService = rolePermissionGroupService;
            _industryRepository = industryRepository;
            _subcontractorRepository = subcontractorRepository;
            _subcontractorStatusRepository = subcontractorStatusRepository;
            _patientRepository = patientRepository;
            _onboardingService = onboardingService;
            _notificationService = notificationService;
            _campService = campService;
            _employeeRepository = employeeRepository;
            _organizationRepository = organizationRepository;
            _patientNumberGeneratorService = patientNumberGeneratorService;
        }




        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto input)
        {
            if (!input.AcceptTerms)
                throw new ValidationException(["You must accept the Terms & Conditions to register."]);

            if (input.Password != input.ConfirmPassword)
                throw new ValidationException(["Passwords do not match."]);

            var normalizedEmail = input.Email.Trim().ToLowerInvariant();
            var existingUser = await _userRepository.FindUserByEmailAsync(normalizedEmail);
            if (existingUser is not null)
                throw new ValidationException(["A user with this email already exists."]);

            var role = await _roleRepository.GetByIdAsync(input.RoleId)
                ?? throw new NotFoundException("Role", input.RoleId.ToString());

            // If OrganizationId is provided, make sure it exists
            if (input.OrganizationId.HasValue)
            {
                var orgExists = await _organizationRepository.GetByIdAsync(input.OrganizationId.Value);
                if (orgExists == null)
                    throw new NotFoundException("Organization", input.OrganizationId.Value.ToString());
            }

            var hashed = _passwordHasher.HashPassword(input.Password);
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                FirstName = input.FirstName,
                MiddleName = input.MiddleName,
                LastName = input.LastName,
                Email = normalizedEmail,
                PasswordHash = hashed,
                IsActive = true,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                UserRoles = [new() { UserId = userId, RoleId = input.RoleId }],
                OrganizationId = input.OrganizationId
            };

            // Optional: wrap in a transaction if your infra provides it
            // using var tx = await _unitOfWork.BeginTransactionAsync();

            await _userRepository.AddUserAsync(user);

            // ---- Create Employee if OrganizationId provided ----
            if (input.OrganizationId.HasValue)
            {
                var orgId = input.OrganizationId.Value;

                var existingEmployee = await _employeeRepository.FindByUserAndOrgAsync(user.Id, orgId);
                if (existingEmployee is null)
                {
                    var employee = new Employee
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        OrganizationId = orgId,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                        // JobTitleId, DepartmentId remain null unless you set defaults
                    };

                    await _employeeRepository.CreateAsync(employee);
                }
            }

            // ---- Role-specific entity ----
            switch (role.Name)
            {
                case "Subcontractor":
                    {
                        var industry = await _industryRepository.GetByNameAsync("General")
                            ?? throw new NotFoundException("Industry", "General");
                        var status = await _subcontractorStatusRepository.FindByNameAsync("Active")
                            ?? throw new NotFoundException("SubcontractorStatus", "Active");

                        var subcontractor = new Salubrity.Domain.Entities.Subcontractor.Subcontractor
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            IndustryId = industry.Id,
                            StatusId = status.Id,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        await _subcontractorRepository.AddAsync(subcontractor);

                        user.RelatedEntityType = "Subcontractor";
                        user.RelatedEntityId = subcontractor.Id;
                        await _userRepository.UpdateUserAsync(user);
                        break;
                    }

                case "Patient":
                    {
                        var patient = new Patient
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        var patientNumber = await _patientNumberGeneratorService.GenerateAsync();
                        patient.PatientNumber = patientNumber;
                        await _patientRepository.AddAsync(patient);

                        user.RelatedEntityType = "Patient";
                        user.RelatedEntityId = patient.Id;
                        await _userRepository.UpdateUserAsync(user);
                        break;
                    }
            }

            // ---- Best-effort camp linking ----
            CampLinkResultDto? campResult = null;
            if (!string.IsNullOrWhiteSpace(input.CampToken))
            {
                campResult = await _campService.TryLinkUserToCampAsync(user.Id, input.CampToken, CancellationToken.None);
            }

            // ---- Tokens ----
            var expiresAt = DateTime.UtcNow.AddMinutes(30);
            var rolesArr = new[] { role.Name };
            var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, rolesArr);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // await tx.CommitAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                CampLinked = campResult?.Linked,
                CampId = campResult?.CampId,
                Warnings = campResult?.Warnings,
                Info = campResult?.Info
            };
        }



        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto input)
        {
            Console.WriteLine("🔐 [Login] Starting login process...");

            var normalizedEmail = input.Email.Trim().ToLowerInvariant();
            Console.WriteLine($"📧 [Login] Normalized email: {normalizedEmail}");

            var user = await _userRepository.FindUserByEmailAsync(normalizedEmail);
            if (user == null)
            {
                Console.WriteLine("❌ [Login] No user found with that email.");
                throw new UnauthorizedException("Invalid credentials.");
            }

            Console.WriteLine($"✅ [Login] User found: {user.Email}");
            Console.WriteLine($"🔑 [Login] Stored hash: {user.PasswordHash}");

            var passwordValid = _passwordHasher.VerifyPassword(input.Password, user.PasswordHash);
            Console.WriteLine($"🧪 [Login] Password valid: {passwordValid}");

            if (!passwordValid)
            {
                Console.WriteLine("❌ [Login] Password verification failed.");
                throw new UnauthorizedException("Invalid credentials.");
            }

            var roles = user.UserRoles?.Select(ur => ur.Role?.Name).Where(name => !string.IsNullOrWhiteSpace(name)).ToArray()
                        ?? Array.Empty<string>();

            Console.WriteLine($"🔒 [Login] Roles assigned: {string.Join(", ", roles)}");

            var token = _jwtService.GenerateAccessToken(user.Id, user.Email, roles);
            Console.WriteLine("✅ [Login] Access token generated.");

            var refreshToken = _jwtService.GenerateRefreshToken();
            Console.WriteLine("🔁 [Login] Refresh token generated.");

            var expiresAt = DateTime.UtcNow.AddMinutes(30);
            Console.WriteLine("📅 [Login] Token expiry set.");

            Console.WriteLine("🎉 [Login] Login successful.");
            return new AuthResponseDto(token, refreshToken, expiresAt);
        }


        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto input)
        {
            var user = await _userRepository.FindUserByRefreshTokenAsync(input.RefreshToken);
            if (user is null) throw new UnauthorizedException("Invalid refresh token.");

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
            var token = _jwtService.GenerateAccessToken(user.Id, user.Email, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(30);

            return new AuthResponseDto(token, refreshToken, expiresAt);
        }

        public async Task LogoutAsync(Guid userId)
        {
            await _userRepository.RevokeUserRefreshTokenAsync(userId);
        }

        public async Task RequestPasswordResetAsync(ForgotPasswordRequestDto input)
        {
            // TODO: Implement optional email notification for reset
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto input)
        {
            var user = await _userRepository.FindUserByEmailAsync(input.Email);
            if (user == null) throw new InvalidOperationException("User not found.");

            user.PasswordHash = _passwordHasher.HashPassword(input.NewPassword);
            user.LastPasswordChangeAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto input)
        {
            var user = await _userRepository.FindUserByIdAsync(userId);
            if (user == null || !_passwordHasher.VerifyPassword(input.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedException("Invalid current password.");

            user.PasswordHash = _passwordHasher.HashPassword(input.NewPassword);
            user.LastPasswordChangeAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);
        }

        public async Task<SetupTotpResponseDto> SetupMfaAsync(string email)
        {
            var user = await _userRepository.FindUserByEmailAsync(email)
                ?? throw new UnauthorizedException("User not found");

            var secret = _totpService.GenerateSecretKey();
            var qr = _totpService.GenerateQrCodeUri(user.Email, "Salubrity", secret);

            user.TotpSecret = secret;
            await _userRepository.UpdateUserAsync(user);

            return new SetupTotpResponseDto
            {
                SecretKey = secret,
                QrCodeUri = qr
            };
        }

        public async Task<bool> VerifyTotpCodeAsync(VerifyTotpCodeRequestDto input)
        {
            var user = await _userRepository.FindUserByEmailAsync(input.Email)
                ?? throw new UnauthorizedException("Invalid user");

            if (string.IsNullOrWhiteSpace(user.TotpSecret))
                throw new InvalidOperationException("TOTP not configured for this user.");

            return _totpService.VerifyCode(user.TotpSecret, input.Code);
        }

        public async Task<MeResponseDto> GetMeAsync(Guid userId)
        {
            var user = await _userRepository.FindUserByIdAsync(userId)
                ?? throw new NotFoundException("User");

            var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
            var roles = new List<string>();
            var permissions = new HashSet<string>();
            var menus = new List<MenuResponseDto>();

            foreach (var roleId in roleIds)
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role != null) roles.Add(role.Name);

                var rolePerms = await _rolePermissionGroupService.GetPermissionGroupsByRoleAsync(roleId);
                foreach (var p in rolePerms) permissions.Add(p.Name);

                var roleMenus = await _menuRoleService.GetMenusByRoleAsync(roleId);
                menus.AddRange(roleMenus);
            }

            var uniqueMenus = menus
                .GroupBy(m => m.Id)
                .Select(g => g.First())
                .Where(m => m.IsActive)
                .OrderBy(m => m.Order)
                .ToList();

            var menuDtos = new Dictionary<Guid, MenuResponseDto>();
            var roots = new List<MenuResponseDto>();

            foreach (var menu in uniqueMenus)
            {
                var dto = new MenuResponseDto
                {
                    Id = menu.Id,
                    Label = menu.Label,
                    Path = menu.Path,
                    Icon = menu.Icon,
                    Children = []
                };
                menuDtos[menu.Id] = dto;

                if (menu.ParentId is null)
                {
                    roots.Add(dto);
                }
                else if (menuDtos.TryGetValue(menu.ParentId.Value, out var parent))
                {
                    parent.Children.Add(dto);
                }
            }

            var onboardingStatus = await _onboardingService.GetOnboardingStatusAsync(user.Id);
            var isOnboardingComplete = onboardingStatus?.IsOnboardingComplete ?? false;

            if (onboardingStatus == null)
            {
                isOnboardingComplete = await _onboardingService.CheckAndUpdateOnboardingStatusAsync(user.Id);
            }

            return new MeResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Roles = roles,
                Permissions = [.. permissions],
                Menus = roots,
                RelatedEntityType = user.RelatedEntityType,
                RelatedEntityId = user.RelatedEntityId,
                OnboardingComplete = isOnboardingComplete
            };
        }
    }
}
