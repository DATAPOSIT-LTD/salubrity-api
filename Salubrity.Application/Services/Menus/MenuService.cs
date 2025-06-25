using AutoMapper;
using Salubrity.Application.DTOs.Menus;
using Salubrity.Application.Interfaces.Repositories.Menus;
using Salubrity.Application.Interfaces.Services.Menus;
using Salubrity.Domain.Entities.Menus;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Menus;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IMapper _mapper;

    public MenuService(IMenuRepository menuRepository, IMapper mapper)
    {
        _menuRepository = menuRepository;
        _mapper = mapper;
    }

    public async Task<MenuResponseDto> CreateAsync(MenuCreateDto input)
    {
        var menu = _mapper.Map<Menu>(input);
        await _menuRepository.AddAsync(menu);
        return _mapper.Map<MenuResponseDto>(menu);
    }

    public async Task<MenuResponseDto> UpdateAsync(Guid id, MenuUpdateDto input)
    {
        var existing = await _menuRepository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException("Menu", id.ToString());

        _mapper.Map(input, existing);
        await _menuRepository.UpdateAsync(existing);
        return _mapper.Map<MenuResponseDto>(existing);
    }

    public async Task<List<MenuResponseDto>> GetAllAsync()
    {
        var list = await _menuRepository.GetAllAsync();
        return _mapper.Map<List<MenuResponseDto>>(list);
    }

    public async Task<MenuResponseDto> GetByIdAsync(Guid id)
    {
        var menu = await _menuRepository.GetByIdAsync(id);
        if (menu == null)
            throw new NotFoundException("Menu", id.ToString());

        return _mapper.Map<MenuResponseDto>(menu);
    }

    public async Task DeleteAsync(Guid id)
    {
        var menu = await _menuRepository.GetByIdAsync(id);
        if (menu == null)
            throw new NotFoundException("Menu", id.ToString());

        await _menuRepository.DeleteAsync(menu);
    }
}
