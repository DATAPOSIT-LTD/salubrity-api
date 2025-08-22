// File: Salubrity.Application/Mapping/ServiceMappingProfile.cs
#nullable enable
using AutoMapper;
using Salubrity.Application.DTOs.Forms;
using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Mapping
{
    /// <summary>
    /// Maps Service entities to Service DTOs with full hierarchy support
    /// </summary>
    public class ServiceMappingProfile : Profile
    {
        public ServiceMappingProfile()
        {
            // ========== ENTITY TO READ DTO MAPPINGS ==========

            // Main Service entity -> Response DTO (includes full tree)
            CreateMap<Service, ServiceResponseDto>();

            // Category entity -> Category DTO (includes subcategories)  
            CreateMap<ServiceCategory, ServiceCategoryDto>();

            // Subcategory entity -> Subcategory DTO
            CreateMap<ServiceSubcategory, ServiceSubcategoryDto>();

            CreateMap<IntakeForm, FormResponseDto>();
            CreateMap<IntakeFormSection, FormSectionResponseDto>();
            CreateMap<IntakeFormField, FormFieldResponseDto>();


            // ========== WRITE DTO TO ENTITY MAPPINGS ==========

            // Create DTOs -> Entities
            CreateMap<CreateServiceDto, Service>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // EF will generate
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // BaseAuditableEntity
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Industry, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.IntakeForm, opt => opt.Ignore()); // Navigation property

            CreateMap<Service, ServiceResponseDto>()
                .ForMember(dest => dest.IntakeForm, opt => opt.MapFrom(src => src.IntakeForm));



            CreateMap<CreateServiceCategoryDto, ServiceCategory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceId, opt => opt.Ignore()) // Will be set by service
                .ForMember(dest => dest.Service, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.Subcategories, opt => opt.Ignore()) //  Prevent AutoMapper from mapping these
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());


            CreateMap<CreateServiceSubcategoryDto, ServiceSubcategory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceCategoryId, opt => opt.Ignore()) // Will be set by category
                .ForMember(dest => dest.ServiceCategory, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

            // Update DTOs -> Entities (for updates, we map over existing entities)
            CreateMap<UpdateServiceDto, Service>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Keep existing ID
                .ForMember(dest => dest.Categories, opt => opt.Ignore()) // Handle separately in service layer
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Keep existing
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore()) // Keep existing
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // Will be updated by interceptor
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore()) // Will be updated by interceptor
                .ForMember(dest => dest.Industry, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.IntakeForm, opt => opt.Ignore()); // Navigation property

            CreateMap<UpdateServiceCategoryDto, ServiceCategory>()
                .ForMember(dest => dest.ServiceId, opt => opt.Ignore()) // Keep existing
                .ForMember(dest => dest.Service, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.Subcategories, opt => opt.Ignore()) // Handle separately
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Keep existing
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore()) // Keep existing
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // Will be updated
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore()); // Will be updated

            CreateMap<UpdateServiceSubcategoryDto, ServiceSubcategory>()
                .ForMember(dest => dest.ServiceCategoryId, opt => opt.Ignore()) // Keep existing
                .ForMember(dest => dest.ServiceCategory, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Keep existing
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore()) // Keep existing
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // Will be updated
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore()); // Will be updated

            // ========== REVERSE MAPPINGS (if needed) ==========

            // Sometimes useful for cloning or converting back
            CreateMap<ServiceResponseDto, Service>()
                .ForMember(dest => dest.Industry, opt => opt.Ignore())
                .ForMember(dest => dest.IntakeForm, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
        }
    }
}