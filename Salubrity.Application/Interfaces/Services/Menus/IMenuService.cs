using Salubrity.Application.DTOs.Menus;

namespace Salubrity.Application.Interfaces.Services.Menus
{
    public interface IMenuService
    {
        Task<MenuResponseDto> CreateAsync(MenuCreateDto input);
        Task<MenuResponseDto> UpdateAsync(Guid id, MenuUpdateDto input);
        Task<List<MenuResponseDto>> GetAllAsync();
        Task<MenuResponseDto> GetByIdAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}
