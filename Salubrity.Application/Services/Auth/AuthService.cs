using Salubrity.Application.DTOs.Auth;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Application.Interfaces.Repositories.Users;
using Salubrity.Application.Interfaces.Security;
using Salubrity.Application.Interfaces.Services.Auth;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Shared.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Salubrity.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITotpService _totpService;
        private readonly IRoleRepository _roleRepository;

        public AuthService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IPasswordHasher passwordHasher,
            ITotpService totpService,
            IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _totpService = totpService;
            _roleRepository = roleRepository;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto input)
        {
            Console.WriteLine("📝 [Register] Starting registration process... Password: " + input.Password);

            if (!input.AcceptTerms)
            {
                Console.WriteLine("❌ [Register] Terms not accepted.");
                throw new ValidationException(["You must accept the Terms & Conditions to register."]);
            }

            if (input.Password != input.ConfirmPassword)
            {
                Console.WriteLine("❌ [Register] Passwords do not match.");
                throw new ValidationException(["Passwords do not match."]);
            }

            var normalizedEmail = input.Email.Trim().ToLowerInvariant();
            Console.WriteLine($"📧 [Register] Normalized email: {normalizedEmail}");

            var existingUser = await _userRepository.FindUserByEmailAsync(normalizedEmail);
            if (existingUser is not null)
            {
                Console.WriteLine("❌ [Register] Email already exists.");
                throw new ValidationException(["A user with this email already exists."]);
            }

            var role = await _roleRepository.GetByIdAsync(input.RoleId);
            if (role is null)
            {
                Console.WriteLine($"❌ [Register] Role not found for ID: {input.RoleId}");
                throw new NotFoundException("Role", input.RoleId.ToString());
            }

            Console.WriteLine("🔐 [Register] Hashing password...");
            var hashed = _passwordHasher.HashPassword(input.Password);
            Console.WriteLine("✅ [Register] Password hashed: " + hashed);

            var userId = Guid.NewGuid();
            Console.WriteLine($"🆔 [Register] Generated User ID: {userId}");

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
                UserRoles = new List<UserRole>
        {
            new UserRole
            {
                UserId = userId,
                RoleId = input.RoleId
            }
        }
            };

            Console.WriteLine("💾 [Register] Saving user...");
            await _userRepository.AddUserAsync(user);
            Console.WriteLine("✅ [Register] User saved.");

            var expiresAt = DateTime.UtcNow.AddMinutes(30);
            var roles = new[] { role.Name };

            Console.WriteLine($"🔑 [Register] Generating access token for user with role: {role.Name}");
            var token = _jwtService.GenerateAccessToken(user.Id, user.Email, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            Console.WriteLine("🎉 [Register] Registration successful.");
            return new AuthResponseDto(token, refreshToken, expiresAt);
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
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            Console.WriteLine($"✅ [Login] User found: {user.Email}");
            Console.WriteLine($"🔑 [Login] Stored hash: {user.PasswordHash}");

            var passwordValid = _passwordHasher.VerifyPassword(input.Password, user.PasswordHash);
            Console.WriteLine($"🧪 [Login] Password valid: {passwordValid}");

            if (!passwordValid)
            {
                Console.WriteLine("❌ [Login] Password verification failed.");
                throw new UnauthorizedAccessException("Invalid credentials.");
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
            if (user is null) throw new UnauthorizedAccessException("Invalid refresh token.");

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
                throw new UnauthorizedAccessException("Invalid current password.");

            user.PasswordHash = _passwordHasher.HashPassword(input.NewPassword);
            user.LastPasswordChangeAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);
        }

        public async Task<SetupTotpResponseDto> SetupMfaAsync(string email)
        {
            var user = await _userRepository.FindUserByEmailAsync(email)
                ?? throw new UnauthorizedAccessException("User not found");

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
                ?? throw new UnauthorizedAccessException("Invalid user");

            if (string.IsNullOrWhiteSpace(user.TotpSecret))
                throw new InvalidOperationException("TOTP not configured for this user.");

            return _totpService.VerifyCode(user.TotpSecret, input.Code);
        }
    }
}
