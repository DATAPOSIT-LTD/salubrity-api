// Mappings/UserProfile.cs
using AutoMapper;
using Salubrity.Application.DTOs.Users;
using Salubrity.Domain.Entities.Identity;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // Map CreateUserDto → User
        CreateMap<RegisterRequestDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // password hashing happens in service

        //Map User → UserResponseDto with custom FullName
        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.FullName,
                       opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

        CreateMap<UserResponse, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id should not be updated
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // PasswordHash should not be updated here
    }
}
