using AutoMapper;
using Salubrity.Application.DTOs.Menus;
using Salubrity.Domain.Entities.Menus;

namespace Salubrity.Application.Mappings
{
    public class MenuMappingProfile : Profile
    {
        public MenuMappingProfile()
        {
            CreateMap<MenuCreateDto, Menu>();
            CreateMap<MenuUpdateDto, Menu>();
            CreateMap<Menu, MenuResponseDto>();

            CreateMap<MenuRole, MenuRoleResponseDto>();
            CreateMap<MenuRoleCreateDto, MenuRole>();
        }
    }
}
