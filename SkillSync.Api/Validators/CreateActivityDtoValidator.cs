using FluentValidation;
using SkillSync.Api.DTOs.Activities;

namespace SkillSync.Api.Validators;

public class CreateActivityDtoValidator : AbstractValidator<CreateActivityDto>
{
    public CreateActivityDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Activity title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0");

        RuleFor(x => x.SkillId)
            .GreaterThan(0).WithMessage("Valid skill must be selected");

        RuleFor(x => x.ResourceUrl)
            .MaximumLength(500).WithMessage("Resource URL cannot exceed 500 characters");
    }
}