// Salubrity.Application/Interfaces/Repositories/Identity/IUserRoleReadRepository.cs
public interface IUserRoleReadRepository
{
    Task<bool> HasRoleAsync(Guid userId, string roleName, CancellationToken ct = default);
}