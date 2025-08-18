using AutoMapper;
using Salubrity.Domain.Entities.HealthAssesment;

namespace Salubrity.Application.Mapping.Profiles;

public class HealthAssessmentProfile : Profile
{
    public HealthAssessmentProfile()
    {
        CreateMap<CreateHealthAssessmentDto, HealthAssessment>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.Metrics, o => o.MapFrom(s => s.Metrics))
            .ForMember(d => d.Recommendations, o => o.MapFrom(s => s.Recommendations));

        CreateMap<CreateAssessmentMetricDto, HealthAssessmentMetric>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore());

        CreateMap<CreateAssessmentRecommendationDto, HealthAssessmentRecommendation>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore());

        CreateMap<HealthAssessment, HealthAssessmentDto>()
            .ForMember(d => d.ReviewedBy, o => o.MapFrom(s => s.ReviewedBy == null
                ? null
                : new ReviewerDto { Id = s.ReviewedBy.Id, FullName = s.ReviewedBy.User.FullName }));

        CreateMap<HealthAssessmentMetric, AssessmentMetricDto>()
            .ForMember(d => d.HealthMetricStatusName, o => o.MapFrom(s => s.Status != null ? s.Status.Name : null));

        CreateMap<HealthAssessmentRecommendation, AssessmentRecommendationDto>();
    }
}
