using AutoMapper;
using Microsoft.Extensions.Logging;
using Salubrity.Application.Interfaces.Repositories.HealthAssessment;
using Salubrity.Application.Interfaces.Services.HealthAssessment;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Health;

public class HealthAssessmentService : IHealthAssessmentService
{
    private readonly IHealthAssessmentRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<HealthAssessmentService> _logger;

    public HealthAssessmentService(
        IHealthAssessmentRepository repo,
        IMapper mapper,
        ILogger<HealthAssessmentService> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HealthAssessmentDto> CreateAsync(CreateHealthAssessmentDto dto)
    {
        var entity = _mapper.Map<Salubrity.Domain.Entities.HealthAssesment.HealthAssessment>(dto);
        var created = await _repo.CreateAsync(entity);
        _logger.LogInformation("Created HealthAssessment {Id}", created.Id);
        return _mapper.Map<HealthAssessmentDto>(await _repo.GetByIdAsync(created.Id));
    }

    public async Task<HealthAssessmentDto> GetByIdAsync(Guid id)
    {
        var item = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("HealthAssessment not found");
        return _mapper.Map<HealthAssessmentDto>(item);
    }

    public async Task<List<HealthAssessmentDto>> GetByParticipantAsync(Guid participantId)
    {
        var items = await _repo.GetByParticipantAsync(participantId);
        return _mapper.Map<List<HealthAssessmentDto>>(items);
    }

    public async Task<List<HealthAssessmentDto>> GetByCampAsync(Guid healthCampId)
    {
        var items = await _repo.GetByCampAsync(healthCampId);
        return _mapper.Map<List<HealthAssessmentDto>>(items);
    }
}
