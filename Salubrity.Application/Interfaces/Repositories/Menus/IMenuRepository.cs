using Salubrity.Domain.Entities.Menus;

namespace Salubrity.Application.Interfaces.Repositories.Menus;

public interface IMenuRepository
{
    Task<Menu> AddAsync(Menu entity);
    Task<Menu?> GetByIdAsync(Guid id);
    Task<List<Menu>> GetAllAsync();
    Task<Menu> UpdateAsync(Menu entity);
    Task DeleteAsync(Menu entity);
}
