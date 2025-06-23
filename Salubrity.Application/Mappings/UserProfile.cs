// Mappings/UserProfile.cs
using AutoMapper;
using Salubrity.Application.DTOs.Auth;
using Salubrity.Domain.Entities;
using Salubrity.Domain.Entities.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    }
}
