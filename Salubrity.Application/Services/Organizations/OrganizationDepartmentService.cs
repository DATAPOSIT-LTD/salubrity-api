using ClosedXML.Excel;
using Salubrity.Application.DTOs.Organizations;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Application.Interfaces.Services.Organizations;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Organizations;

public class OrganizationDepartmentService : IOrganizationDepartmentService
{
    private readonly IOrganizationDepartmentRepository _repo;

    public OrganizationDepartmentService(IOrganizationDepartmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<OrganizationDepartmentDto>> ListAsync(Guid organizationId, CancellationToken ct = default)
    {
        var rows = await _repo.GetByOrganizationAsync(organizationId, ct);
        return rows.Select(r => new OrganizationDepartmentDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
        }).ToList();
    }

    public async Task<OrganizationDepartmentDto> AddAsync(Guid organizationId, CreateOrgDepartmentDto dto, CancellationToken ct = default)
    {
        var name = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException(["Department name is required."]);
        if (await _repo.ExistsByNameAsync(organizationId, name, ct))
            throw new ValidationException(["A department with that name already exists for this organization."]);

        var entity = new Department
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = dto.Description?.Trim(),
            OrganizationId = organizationId,
            CreatedAt = DateTime.UtcNow,
        };
        await _repo.AddAsync(entity, ct);
        return new OrganizationDepartmentDto { Id = entity.Id, Name = entity.Name, Description = entity.Description };
    }

    public async Task<BulkDepartmentUploadResultDto> UploadAsync(Guid organizationId, Stream xlsxStream, CancellationToken ct = default)
    {
        if (xlsxStream == null) throw new ValidationException(["Upload is required."]);

        var result = new BulkDepartmentUploadResultDto();
        var existing = (await _repo.GetByOrganizationAsync(organizationId, ct))
            .Select(d => d.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        using var workbook = new XLWorkbook(xlsxStream);
        var sheet = workbook.Worksheets.FirstOrDefault()
            ?? throw new ValidationException(["The workbook is empty."]);

        var rowsToAdd = new List<Department>();
        var rowNum = 0;
        foreach (var row in sheet.RowsUsed())
        {
            rowNum++;
            // Skip header row if it looks like one.
            if (rowNum == 1)
            {
                var firstText = row.Cell(1).GetString().Trim();
                if (string.Equals(firstText, "Name", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(firstText, "Department", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(firstText, "Department Name", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            var name = row.Cell(1).GetString().Trim();
            var desc = row.Cell(2).GetString().Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                result.Skipped++;
                result.SkippedReasons.Add($"Row {rowNum}: empty name");
                continue;
            }
            if (existing.Contains(name))
            {
                result.Skipped++;
                result.SkippedReasons.Add($"Row {rowNum}: \"{name}\" already exists");
                continue;
            }

            rowsToAdd.Add(new Department
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = string.IsNullOrWhiteSpace(desc) ? null : desc,
                OrganizationId = organizationId,
                CreatedAt = DateTime.UtcNow,
            });
            existing.Add(name);
            result.Added++;
        }

        if (rowsToAdd.Count > 0) await _repo.AddRangeAsync(rowsToAdd, ct);
        return result;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _repo.SoftDeleteAsync(id, ct);
    }

    public Task<byte[]> GenerateTemplateAsync(CancellationToken ct = default)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Departments");

        // Headers
        ws.Cell(1, 1).Value = "Name";
        ws.Cell(1, 2).Value = "Description (optional)";
        var header = ws.Range("A1:B1");
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#0E3D3B");
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(1).Height = 22;

        // Example rows so the admin can see the shape
        ws.Cell(2, 1).Value = "Engineering";
        ws.Cell(2, 2).Value = "Software / hardware teams";
        ws.Cell(3, 1).Value = "Finance";
        ws.Cell(3, 2).Value = "Accounts, payroll, treasury";
        ws.Cell(4, 1).Value = "Human Resources";
        ws.Cell(4, 2).Value = string.Empty;

        var example = ws.Range("A2:B4");
        example.Style.Font.Italic = true;
        example.Style.Font.FontColor = XLColor.FromHtml("#6B7280");

        // Help note in row 6
        ws.Cell(6, 1).Value = "Tip: replace the example rows above with your real departments, then upload this file.";
        ws.Range("A6:B6").Merge();
        ws.Cell(6, 1).Style.Font.Italic = true;
        ws.Cell(6, 1).Style.Font.FontColor = XLColor.FromHtml("#9CA3AF");

        ws.Column(1).Width = 28;
        ws.Column(2).Width = 42;

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return Task.FromResult(ms.ToArray());
    }
}
