using FluentValidation;

namespace TripUpdater.Application.Updates.Commands.ProcessTripUpdates;

public sealed class ProcessTripUpdatesValidator : AbstractValidator<ProcessTripUpdatesCommand>
{
    public ProcessTripUpdatesValidator()
    {
        RuleFor(x => x.Updates)
            .NotEmpty()
            .WithMessage("At least one trip update is required.");

        RuleForEach(x => x.Updates)
            .ChildRules(update =>
            {
                update.RuleFor(u => u.TripId)
                    .GreaterThan(0)
                    .WithMessage("TripId must be positive.");
            });
    }
}
