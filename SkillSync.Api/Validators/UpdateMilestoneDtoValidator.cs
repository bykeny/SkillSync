using FluentValidation;
using SkillSync.Api.DTOs.Milestones;

namespace SkillSync.Api.Validators;

public class UpdateMilestoneDtoValidator : AbstractValidator<UpdateMilestoneDto>
{
    public UpdateMilestoneDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Milestone title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.ProgressPercentage)
            .InclusiveBetween(0, 100).WithMessage("Progress must be between 0 and 100");
    }
}