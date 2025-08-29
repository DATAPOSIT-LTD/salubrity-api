using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Application.Interfaces;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Infrastructure.Persistence.Repositories.IntakeForms;

public sealed class IntakeFormResponseRepository : IIntakeFormResponseRepository
{
    private readonly AppDbContext _db;

    public IntakeFormResponseRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IntakeFormResponse> AddAsync(IntakeFormResponse response, CancellationToken ct = default)
    {
        _db.IntakeFormResponses.Add(response);
        await _db.SaveChangesAsync(ct);
        return response;
    }

    public Task<IntakeFormResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _db.IntakeFormResponses
            .AsNoTracking()
            .Include(r => r.FieldResponses)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public Task<bool> IntakeFormVersionExistsAsync(Guid versionId, CancellationToken ct = default)
    {
        return _db.IntakeFormVersions
            .AsNoTracking()
            .AnyAsync(v => v.Id == versionId, ct);
    }

    // public async Task<HashSet<Guid>> GetFieldIdsForVersionAsync(Guid versionId, CancellationToken ct = default)
    // {
    //     return (await _db.IntakeFormFields
    //         .AsNoTracking()
    //         .Where(f => f.Form.Versions.Contains == versionId)
    //         .Select(f => f.Id)
    //         .ToListAsync(ct))
    //         .ToHashSet();
    // }

    public async Task<HashSet<Guid>> GetFieldIdsForVersionAsync(Guid versionId, CancellationToken ct = default)
    {
        return await _db.IntakeFormFields
            .AsNoTracking()
            .Where(f => f.Section.IntakeFormVersionId == versionId)
            .Select(f => f.Id)
            .ToHashSetAsync(ct);
    }
    public async Task<Guid> GetStatusIdByNameAsync(string name, CancellationToken ct = default)
    {
        var id = await _db.IntakeFormResponseStatuses
            .Where(s => s.Name == name && !s.IsDeleted)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(ct);

        if (id == Guid.Empty)
            throw new NotFoundException($"Status with name '{name}' not found.");

        return id;
    }

}

