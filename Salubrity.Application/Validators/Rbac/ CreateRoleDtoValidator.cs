// CreateRoleDtoValidator.cs
using FluentValidation;
using Salubrity.Application.DTOs.Rbac;

public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(100);

        RuleFor(x => x)
            .Must(x => !(x.IsGlobal && x.OrganizationId.HasValue))
            .WithMessage("A global role cannot be assigned to a specific organization.");

        RuleFor(x => x)
            .Must(x => x.IsGlobal || x.OrganizationId.HasValue)
            .WithMessage("A non-global role must have an organization assigned.");
    }
}
