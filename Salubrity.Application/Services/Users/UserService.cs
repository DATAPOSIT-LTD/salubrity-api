﻿using AutoMapper;
using Salubrity.Application.DTOs.Users;
using Salubrity.Application.Interfaces.Repositories.Users;
using Salubrity.Application.Interfaces.Services.Users;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Shared.Responses;

namespace Salubrity.Application.Services.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
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

        _mapper.Map(request, user);
        await _userRepository.UpdateUserAsync(user);

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
}
