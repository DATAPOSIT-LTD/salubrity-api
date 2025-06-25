using AutoMapper;
using Salubrity.Application.DTOs.Menus;
using Salubrity.Application.Interfaces.Repositories.Menus;
using Salubrity.Application.Interfaces.Services.Menus;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Menus;

public class MenuRoleService : IMenuRoleService
{
    private readonly IMenuRoleRepository _menuRoleRepository;
    private readonly IMapper _mapper;

    public MenuRoleService(IMenuRoleRepository menuRoleRepository, IMapper mapper)
    {
        _menuRoleRepository = menuRoleRepository;
        _mapper = mapper;
    }

    public async Task AssignRolesAsync(MenuRoleCreateDto input)
    {
        await _menuRoleRepository.ReplaceRolesForMenuAsync(input.MenuId, input.RoleIds);
    }

    public async Task ReplaceRolesAsync(Guid menuId, IEnumerable<Guid> roleIds)
    {
        await _menuRoleRepository.ReplaceRolesForMenuAsync(menuId, roleIds);
    }

    public async Task<List<MenuRoleResponseDto>> GetRolesForMenuAsync(Guid menuId)
    {
        var roles = await _menuRoleRepository.GetRolesForMenuAsync(menuId);
        return _mapper.Map<List<MenuRoleResponseDto>>(roles);
    }

    public async Task RemoveRoleAsync(Guid menuId, Guid roleId)
    {
        var removed = await _menuRoleRepository.RemoveRoleAsync(menuId, roleId);
        if (!removed)
            throw new NotFoundException("MenuRole", $"Menu {menuId} / Role {roleId}");
    }

    public async Task RemoveAllRolesAsync(Guid menuId)
    {
        await _menuRoleRepository.RemoveAllRolesFromMenuAsync(menuId);
    }

    public async Task<List<MenuResponseDto>> GetMenusByRoleAsync(Guid roleId)
    {
        var menus = await _menuRoleRepository.GetMenusByRoleAsync(roleId);
        return _mapper.Map<List<MenuResponseDto>>(menus);
    }

    public async Task<List<Guid>> GetMenuIdsByRoleAsync(Guid roleId)
    {
        return await _menuRoleRepository.GetMenuIdsByRoleAsync(roleId);
    }
}
