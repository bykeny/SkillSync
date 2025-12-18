using FluentValidation;
using SkillSync.Api.DTOs.Milestones;

namespace SkillSync.Api.Validators;

public class CreateMilestoneDtoValidator : AbstractValidator<CreateMilestoneDto>
{
    public CreateMilestoneDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Milestone title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.TargetDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Target date must be in the future");

        RuleFor(x => x.SkillId)
            .GreaterThan(0).WithMessage("Valid skill must be selected");
    }
}