using AutoMapper;
using Salubrity.Application.DTOs.Users;
using Salubrity.Application.Interfaces.Repositories.Users;
using Salubrity.Application.Interfaces.Services.Users;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Shared.Responses;

namespace Salubrity.Application.Services.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleReadRepository _userRoleReadRepository;
    private readonly IMapper _mapper;
    private readonly IOnboardingService _onboardingService;


    public UserService(IUserRepository userRepository, IMapper mapper, IUserRoleReadRepository userRoleRepository, IOnboardingService onboardingService)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _userRoleReadRepository = userRoleRepository;
        _onboardingService = onboardingService;
    }

    public async Task<ApiResponse<UserResponse>> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.FindUserByIdAsync(id);
        if (user is null)
            return ApiResponse<UserResponse>.CreateFailure("User not found.");

        var dto = _mapper.Map<UserResponse>(user);
        return ApiResponse<UserResponse>.CreateSuccess(dto);
    }

    public async Task<ApiResponse<List<UserResponse>>> GetAllAsync()
    {
        return ApiResponse<List<UserResponse>>.CreateFailure("Bulk retrieval not implemented.");
    }

    public async Task<ApiResponse<Guid>> CreateAsync(UserCreateRequest request)
    {
        var existing = await _userRepository.FindUserByEmailAsync(request.Email);
        if (existing is not null)
            return ApiResponse<Guid>.CreateFailure("A user with this email already exists.");

        var user = _mapper.Map<User>(request);

        await _userRepository.AddUserAsync(user);
        return ApiResponse<Guid>.CreateSuccess(user.Id, "User created successfully.");
    }

    public async Task<ApiResponse<string>> UpdateAsync(Guid id, UserUpdateRequest request)
    {
        var user = await _userRepository.FindUserByIdAsync(id);
        if (user is null)
            return ApiResponse<string>.CreateFailure("User not found.");

        // Check for duplicate phone number
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var existingWithPhone = await _userRepository.FindUserByPhoneAsync(request.Phone);
            if (existingWithPhone != null && existingWithPhone.Id != id)
            {
                return ApiResponse<string>.CreateFailure("A user with this phone number already exists.");
            }
        }

        _mapper.Map(request, user);
        await _userRepository.UpdateUserAsync(user);

        await _onboardingService.CheckAndUpdateOnboardingStatusAsync(id);

        return ApiResponse<string>.CreateSuccessMessage("User updated successfully.");
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid id)
    {
        var user = await _userRepository.FindUserByIdAsync(id);
        if (user is null)
            return ApiResponse<string>.CreateFailure("User not found.");

        user.IsActive = false;
        await _userRepository.UpdateUserAsync(user);

        return ApiResponse<string>.CreateSuccessMessage("User deactivated successfully.");
    }
    public async Task<bool> IsInRoleAsync(Guid userId, string roleName)
    {
        return await _userRoleReadRepository.HasRoleAsync(userId, roleName);
    }

}
