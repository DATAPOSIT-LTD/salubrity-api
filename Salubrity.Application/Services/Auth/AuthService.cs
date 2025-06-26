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
            //  Validate terms agreement
            if (!input.AcceptTerms)
                throw new ValidationException(["You must accept the Terms & Conditions to register."]);

            //  Ensure passwords match
            if (input.Password != input.ConfirmPassword)
                throw new ValidationException(["Passwords do not match."]);

            //  Check if email already exists
            var existingUser = await _userRepository.FindUserByEmailAsync(input.Email);
            if (existingUser is not null)
                throw new ValidationException(["A user with this email already exists."]);

            //  Check if role exists
            var role = await _roleRepository.GetByIdAsync(input.RoleId);
            if (role is null)
                throw new NotFoundException("Role", input.RoleId.ToString());

            // Hash password
            var hashed = _passwordHasher.HashPassword(input.Password);

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = input.FirstName,
                MiddleName = input.MiddleName,
                LastName = input.LastName,
                Email = input.Email.Trim().ToLowerInvariant(),
                PasswordHash = hashed,
                IsActive = true,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            //  Assign role
            user.UserRoles = new List<UserRole>
    {
        new UserRole
        {
            UserId = user.Id,
            RoleId = input.RoleId
        }
    };

            //  Save user
            await _userRepository.AddUserAsync(user);

            //  Generate tokens
            var expiresAt = DateTime.UtcNow.AddMinutes(30);
            var roles = new[] { role.Name }; // since only one role on register
            var token = _jwtService.GenerateAccessToken(user.Id, user.Email, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            return new AuthResponseDto(token, refreshToken, expiresAt);
        }


        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto input)
        {
            var normalizedEmail = input.Email.Trim().ToLowerInvariant(); // 🛠 Normalize
            var user = await _userRepository.FindUserByEmailAsync(normalizedEmail);

            if (user is null)
                throw new UnauthorizedAccessException("Invalid credentials.");

            if (!_passwordHasher.VerifyPassword(input.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials.");

            var roles = user.UserRoles?.Select(ur => ur.Role?.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToArray()
                        ?? Array.Empty<string>();

            var token = _jwtService.GenerateAccessToken(user.Id, user.Email, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(30);

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
