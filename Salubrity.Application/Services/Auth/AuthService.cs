using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Application.DTOs.Auth;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.DTOs.Identity;
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
using Salubrity.Domain.Entities.Auth;
using Salubrity.Domain.Entities.HealthCamps;
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
        private readonly IHealthCampParticipantRepository _healthCampParticipantRepository;
        private readonly IHealthCampRepository _healthCampRepository;





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
            IPatientNumberGeneratorService patientNumberGeneratorService,
            IHealthCampParticipantRepository healthCampParticipantRepository,
            IHealthCampRepository healthCampRepository
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
            _healthCampParticipantRepository = healthCampParticipantRepository;
            _healthCampRepository = healthCampRepository;
        }





        // public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto input)
        // {
        //     if (!input.AcceptTerms)
        //         throw new ValidationException(["You must accept the Terms & Conditions to register."]);

        //     if (input.Password != input.ConfirmPassword)
        //         throw new ValidationException(["Passwords do not match."]);

        //     var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        //     var existingUser = await _userRepository.FindUserByEmailAsync(normalizedEmail);
        //     if (existingUser is not null)
        //         throw new ValidationException(["A user with this email already exists."]);

        //     ClaimsPrincipal? principal = null;

        //     // ─────────────────────────────────────────────
        //     // CAMP TOKEN HANDLING (POSTER-ONLY)
        //     // ─────────────────────────────────────────────
        //     if (!string.IsNullOrWhiteSpace(input.CampToken))
        //     {

        //         principal = _jwtService.ValidateToken(input.CampToken);


        //         if (principal == null)
        //             throw new ValidationException(["Invalid camp token."]);

        //         // Must be a poster token
        //         var isPoster = principal.Claims.Any(c => c.Type == "poster" && c.Value == "1");
        //         if (!isPoster)
        //             throw new ValidationException(["Invalid camp token."]);

        //         // Defensive expiry check
        //         var expClaim = principal.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        //         if (expClaim != null &&
        //             DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)) < DateTimeOffset.UtcNow)
        //         {
        //             throw new ValidationException(["Camp token has expired."]);
        //         }

        //         // Extract roleId (mandatory)
        //         var roleIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "roleId")?.Value;
        //         if (!Guid.TryParse(roleIdClaim, out var tokenRoleId))
        //             throw new ValidationException(["Invalid or missing roleId in camp token."]);

        //         input.RoleId = tokenRoleId;

        //         // Extract organizationId (optional)
        //         var orgIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "organizationId")?.Value;
        //         if (!string.IsNullOrWhiteSpace(orgIdClaim) &&
        //             Guid.TryParse(orgIdClaim, out var tokenOrgId))
        //         {
        //             input.OrganizationId = tokenOrgId;
        //         }
        //     }

        //     // ─────────────────────────────────────────────
        //     // ROLE + ORGANIZATION VALIDATION
        //     // ─────────────────────────────────────────────
        //     var roleId = input.RoleId ?? throw new ValidationException(["RoleId is required."]);

        //     var role = await _roleRepository.GetByIdAsync(roleId)
        //         ?? throw new NotFoundException("Role", roleId.ToString());

        //     if (input.OrganizationId.HasValue)
        //     {
        //         var orgExists = await _organizationRepository.GetByIdAsync(input.OrganizationId.Value);
        //         if (orgExists == null)
        //             throw new NotFoundException("Organization", input.OrganizationId.Value.ToString());
        //     }

        //     // ─────────────────────────────────────────────
        //     // USER CREATION
        //     // ─────────────────────────────────────────────
        //     var userId = Guid.NewGuid();
        //     var hashedPassword = _passwordHasher.HashPassword(input.Password);

        //     var user = new User
        //     {
        //         Id = userId,
        //         FirstName = input.FirstName,
        //         MiddleName = input.MiddleName,
        //         LastName = input.LastName,
        //         Email = normalizedEmail,
        //         PasswordHash = hashedPassword,
        //         IsActive = true,
        //         IsVerified = false,
        //         CreatedAt = DateTime.UtcNow,
        //         OrganizationId = input.OrganizationId,
        //         UserRoles =
        //         [
        //             new UserRole
        //     {
        //         UserId = userId,
        //         RoleId = roleId
        //     }
        //         ]
        //     };

        //     await _userRepository.AddUserAsync(user);

        //     // ─────────────────────────────────────────────
        //     // EMPLOYEE (IF ORG SCOPED)
        //     // ─────────────────────────────────────────────
        //     if (input.OrganizationId.HasValue)
        //     {
        //         var orgId = input.OrganizationId.Value;

        //         var existingEmployee = await _employeeRepository.FindByUserAndOrgAsync(user.Id, orgId);
        //         if (existingEmployee == null)
        //         {
        //             var employee = new Employee
        //             {
        //                 Id = Guid.NewGuid(),
        //                 UserId = user.Id,
        //                 OrganizationId = orgId,
        //                 CreatedAt = DateTime.UtcNow,
        //                 IsDeleted = false
        //             };

        //             await _employeeRepository.CreateAsync(employee);
        //         }
        //     }

        //     // ─────────────────────────────────────────────
        //     // ROLE-SPECIFIC ENTITY CREATION
        //     // ─────────────────────────────────────────────
        //     switch (role.Name)
        //     {
        //         case "Subcontractor":
        //             {
        //                 var industry = await _industryRepository.GetByNameAsync("General")
        //                     ?? throw new NotFoundException("Industry", "General");

        //                 var status = await _subcontractorStatusRepository.FindByNameAsync("Active")
        //                     ?? throw new NotFoundException("SubcontractorStatus", "Active");

        //                 var subcontractor = new Domain.Entities.Subcontractor.Subcontractor
        //                 {
        //                     Id = Guid.NewGuid(),
        //                     UserId = user.Id,
        //                     IndustryId = industry.Id,
        //                     StatusId = status.Id,
        //                     CreatedAt = DateTime.UtcNow,
        //                     IsDeleted = false
        //                 };

        //                 await _subcontractorRepository.AddAsync(subcontractor);

        //                 user.RelatedEntityType = "Subcontractor";
        //                 user.RelatedEntityId = subcontractor.Id;
        //                 await _userRepository.UpdateUserAsync(user);
        //                 break;
        //             }

        //         case "Patient":
        //             {
        //                 var patient = new Patient
        //                 {
        //                     Id = Guid.NewGuid(),
        //                     UserId = user.Id,
        //                     CreatedAt = DateTime.UtcNow,
        //                     IsDeleted = false
        //                 };

        //                 patient.PatientNumber = await _patientNumberGeneratorService.GenerateAsync();
        //                 await _patientRepository.AddAsync(patient);

        //                 user.RelatedEntityType = "Patient";
        //                 user.RelatedEntityId = patient.Id;
        //                 await _userRepository.UpdateUserAsync(user);
        //                 break;
        //             }
        //     }

        //     // ─────────────────────────────────────────────
        //     // CAMP LINKING (BEST-EFFORT)
        //     // ─────────────────────────────────────────────
        //     CampLinkResultDto? campResult = null;
        //     if (!string.IsNullOrWhiteSpace(input.CampToken))
        //     {
        //         campResult = await _campService.TryLinkUserToCampAsync(
        //             user.Id,
        //             input.CampToken,
        //             CancellationToken.None
        //         );
        //     }

        //     // ─────────────────────────────────────────────
        //     // AUTH TOKENS
        //     // ─────────────────────────────────────────────
        //     var expiresAt = DateTime.UtcNow.AddMinutes(30);

        //     var accessToken = _jwtService.GenerateAccessToken(
        //         user.Id,
        //         user.Email,
        //         new[] { role.Name }
        //     );

        //     var refreshToken = _jwtService.GenerateRefreshToken();

        //     return new AuthResponseDto
        //     {
        //         AccessToken = accessToken,
        //         RefreshToken = refreshToken,
        //         ExpiresAt = expiresAt,
        //         CampLinked = campResult?.Linked,
        //         CampId = campResult?.CampId,
        //         Warnings = campResult?.Warnings,
        //         Info = campResult?.Info
        //     };
        // }


        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto input)
        {
            // ─────────────────────────────────────────────
            // BASIC VALIDATION
            // ─────────────────────────────────────────────
            if (!input.AcceptTerms)
                throw new ValidationException(["You must accept the Terms & Conditions to register."]);

            if (input.Password != input.ConfirmPassword)
                throw new ValidationException(["Passwords do not match."]);

            if (input.RoleId == null && string.IsNullOrWhiteSpace(input.Role))
                throw new ValidationException(["Either RoleId or Role must be provided."]);

            var normalizedEmail = input.Email.Trim().ToLowerInvariant();
            if (await _userRepository.FindUserByEmailAsync(normalizedEmail) != null)
                throw new ValidationException(["A user with this email already exists."]);

            // ─────────────────────────────────────────────
            // CAMP RESOLUTION (SLUG → CAMP)
            // ─────────────────────────────────────────────
            HealthCamp? camp = null;

            if (!string.IsNullOrWhiteSpace(input.CampSlug))
            {
                camp = await _healthCampRepository.GetBySlugAsync(input.CampSlug.Trim())
                    ?? throw new ValidationException(["Invalid or expired camp link."]);
            }

            // ─────────────────────────────────────────────
            // ORGANIZATION RESOLUTION (EXACTLY LIKE TOKEN FLOW)
            // ─────────────────────────────────────────────
            Guid? organizationId = null;

            if (camp != null)
            {
                organizationId = camp.OrganizationId;
            }
            else if (input.OrganizationId.HasValue)
            {
                organizationId = input.OrganizationId.Value;
            }

            if (organizationId.HasValue)
            {
                var orgExists = await _organizationRepository.GetByIdAsync(organizationId.Value);
                if (orgExists == null)
                    throw new NotFoundException("Organization", organizationId.Value.ToString());
            }

            // ─────────────────────────────────────────────
            // ROLE RESOLUTION (ROLE ID WINS)
            // ─────────────────────────────────────────────
            Guid roleId;

            if (input.RoleId.HasValue)
            {
                roleId = input.RoleId.Value;
            }
            else
            {
                var normalizedRole = input.Role!.Trim().ToLowerInvariant();

                roleId = normalizedRole switch
                {
                    // participant == Patient (same as original semantics)
                    "participant" => (await _roleRepository.FindByNameAsync("Patient"))?.Id
                        ?? throw new ValidationException(["Patient role not found."]),

                    "subcontractor" => (await _roleRepository.FindByNameAsync("Subcontractor"))?.Id
                        ?? throw new ValidationException(["Subcontractor role not found."]),

                    _ => throw new ValidationException([
                        $"Invalid role '{input.Role}'. Allowed values: participant, subcontractor."
                    ])
                };
            }

            var role = await _roleRepository.GetByIdAsync(roleId)
                ?? throw new NotFoundException("Role", roleId.ToString());

            // ─────────────────────────────────────────────
            // USER CREATION
            // ─────────────────────────────────────────────
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                FirstName = input.FirstName,
                MiddleName = input.MiddleName,
                LastName = input.LastName,
                Email = normalizedEmail,
                PasswordHash = _passwordHasher.HashPassword(input.Password),
                IsActive = true,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                OrganizationId = organizationId,
                UserRoles =
                [
                    new UserRole
            {
                UserId = userId,
                RoleId = roleId
            }
                ]
            };

            await _userRepository.AddUserAsync(user);

            // ─────────────────────────────────────────────
            // EMPLOYEE CREATION (PRESERVED ORIGINAL BEHAVIOR)
            // ─────────────────────────────────────────────
            if (organizationId.HasValue && role.Name != "Subcontractor")
            {
                var existingEmployee =
                    await _employeeRepository.FindByUserAndOrgAsync(user.Id, organizationId.Value);

                if (existingEmployee == null)
                {
                    await _employeeRepository.CreateAsync(new Employee
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        OrganizationId = organizationId.Value,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }
            }

            // ─────────────────────────────────────────────
            // ROLE-SPECIFIC ENTITY CREATION
            // ─────────────────────────────────────────────
            switch (role.Name)
            {
                case "Subcontractor":
                    {
                        var industry = await _industryRepository.GetByNameAsync("General")
                            ?? throw new NotFoundException("Industry", "General");

                        var status = await _subcontractorStatusRepository.FindByNameAsync("Active")
                            ?? throw new NotFoundException("SubcontractorStatus", "Active");

                        var subcontractor = new Domain.Entities.Subcontractor.Subcontractor
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
                            IsDeleted = false,
                            PatientNumber = await _patientNumberGeneratorService.GenerateAsync()
                        };

                        await _patientRepository.AddAsync(patient);

                        user.RelatedEntityType = "Patient";
                        user.RelatedEntityId = patient.Id;
                        await _userRepository.UpdateUserAsync(user);
                        break;
                    }
            }

            // ─────────────────────────────────────────────
            // CAMP LINKING (BEST-EFFORT, SLUG-BASED)
            // ─────────────────────────────────────────────
            CampLinkResultDto? campResult = null;

            if (camp != null)
            {
                campResult = await _campService.LinkUserToCampAsync(
                    user.Id,
                    camp.Id,
                    CancellationToken.None
                );
            }

            // ─────────────────────────────────────────────
            // AUTH TOKENS
            // ─────────────────────────────────────────────
            var expiresAt = DateTime.UtcNow.AddMinutes(30);

            return new AuthResponseDto
            {
                AccessToken = _jwtService.GenerateAccessToken(
                    user.Id,
                    user.Email,
                    new[] { role.Name }
                ),
                RefreshToken = _jwtService.GenerateRefreshToken(),
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
            if (string.IsNullOrWhiteSpace(input.Email))
                throw new ValidationException(["Email is required."]);

            // Step 1: Find user
            var user = await _userRepository.FindUserByEmailAsync(input.Email);
            if (user == null)
                throw new NotFoundException("User", input.Email);

            // Step 2: Generate reset token
            var token = Guid.NewGuid().ToString("N");
            var expiresAt = DateTime.UtcNow.AddHours(1);

            var resetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = token,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            };


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

            // ─────────────────────────────────────────────
            // ROLES, PERMISSIONS, MENUS
            // ─────────────────────────────────────────────
            var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
            var roles = new List<string>();
            var permissions = new HashSet<string>();
            var menus = new List<MenuResponseDto>();

            foreach (var roleId in roleIds)
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role != null) roles.Add(role.Name);

                var rolePerms = await _rolePermissionGroupService.GetPermissionGroupsByRoleAsync(roleId);
                foreach (var p in rolePerms)
                    permissions.Add(p.Name);

                var roleMenus = await _menuRoleService.GetMenusByRoleAsync(roleId);
                menus.AddRange(roleMenus);
            }

            // Deduplicate & order menus
            var uniqueMenus = menus
                .GroupBy(m => m.Id)
                .Select(g => g.First())
                .Where(m => m.IsActive)
                .OrderBy(m => m.Order)
                .ToList();

            // Build hierarchy
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
                    Children = new()
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

            // ─────────────────────────────────────────────
            // ONBOARDING
            // ─────────────────────────────────────────────
            var onboardingStatus = await _onboardingService.GetOnboardingStatusAsync(user.Id);
            var isOnboardingComplete = onboardingStatus?.IsOnboardingComplete ?? false;

            if (onboardingStatus == null)
            {
                isOnboardingComplete = await _onboardingService.CheckAndUpdateOnboardingStatusAsync(user.Id);
            }

            // ─────────────────────────────────────────────
            // PATIENT BILLING STATUS
            // ─────────────────────────────────────────────
            string? billingStatus = null;
            var patient = await _patientRepository.GetByUserIdAsync(userId);

            if (patient != null)
            {
                var participantId = await _healthCampParticipantRepository
                    .GetParticipantIdByPatientIdAsync(patient.Id);

                if (participantId.HasValue)
                {
                    var participant = await _healthCampParticipantRepository
                        .GetParticipantWithBillingStatusByIdAsync(participantId.Value);

                    billingStatus = participant?.BillingStatus?.Name;
                }
            }

            // ─────────────────────────────────────────────
            // EMPLOYEE: EXTRACT ORGANIZATION + BRANCH (HYBRID LOGIC)
            // ─────────────────────────────────────────────
            MiniOrganizationDto? orgDto = null;
            MiniBranchDto? branchDto = null;
            Guid? employeeId = null;

            Employee? employee = null;

            // Try clean architecture lookup
            if (user.RelatedEntityType == "Employee" && user.RelatedEntityId.HasValue)
            {
                employee = await _employeeRepository
                    .GetByIdWithOrgAndBranchAsync(user.RelatedEntityId.Value);
            }

            // Fallback: Try get employee by UserId
            if (employee == null)
            {
                employee = await _employeeRepository
                    .GetByUserIdWithOrgAndBranchAsync(user.Id);
            }

            // If employee found → map organization and branch
            if (employee != null)
            {
                employeeId = employee.Id;

                if (employee.Organization != null)
                {
                    orgDto = new MiniOrganizationDto
                    {
                        Id = employee.Organization.Id,
                        BusinessName = employee.Organization.BusinessName
                    };
                }

                if (employee.Branch != null)
                {
                    branchDto = new MiniBranchDto
                    {
                        Id = employee.Branch.Id,
                        BranchName = employee.Branch.BranchName
                    };
                }
            }

            // ─────────────────────────────────────────────
            // FINAL RESPONSE
            // ─────────────────────────────────────────────
            return new MeResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Roles = roles,
                Permissions = permissions.ToList(),
                Menus = roots,

                RelatedEntityType = user.RelatedEntityType,
                RelatedEntityId = user.RelatedEntityId,

                OnboardingComplete = isOnboardingComplete,

                BillingStatus = new BillingStatusDto
                {
                    CanProceed = billingStatus == "Billed" || billingStatus == "Proceed without billing",
                    Status = billingStatus
                },

                EmployeeId = employeeId,
                Organization = orgDto,
                Branch = branchDto
            };
        }


    }
}
