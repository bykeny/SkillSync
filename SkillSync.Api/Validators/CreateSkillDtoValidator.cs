using FluentValidation;
using SkillSync.Api.DTOs.Skills;

namespace SkillSync.Api.Validators;

public class CreateSkillDtoValidator : AbstractValidator<CreateSkillDto>
{
    public CreateSkillDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Skill name is required")
            .MaximumLength(100).WithMessage("Skill name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.ProficiencyLevel)
            .InclusiveBetween(1, 5).WithMessage("Proficiency level must be between 1 and 5");

        RuleFor(x => x.TargetLevel)
            .InclusiveBetween(1, 5).WithMessage("Target level must be between 1 and 5");

        RuleFor(x => x.TargetLevel)
            .GreaterThanOrEqualTo(x => x.ProficiencyLevel)
            .WithMessage("Target level must be greater than or equal to current proficiency level");
    }
}