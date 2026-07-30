using FluentValidation;
using Mediator;
using TripUpdater.Domain.Common;

namespace TripUpdater.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(request, cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Count > 0)
            .SelectMany(r => r.Errors)
            .Select(f => new Error(f.PropertyName, f.ErrorMessage))
            .ToList();

        if (failures.Count > 0)
        {
            return CreateFailureResult(failures);
        }

        return await next(request, cancellationToken);
    }

    private static TResponse CreateFailureResult(List<Error> errors)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure([.. errors]);
        }

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = typeof(Result<>).MakeGenericType(typeof(TResponse).GetGenericArguments()[0]);
            var failureMethod = resultType.GetMethod(nameof(Result<object>.Failure), [typeof(Error[])])!;
            return (TResponse)failureMethod.Invoke(null, [errors.ToArray()])!;
        }

        throw new InvalidOperationException(
            $"ValidationBehavior only supports Result and Result<T> response types, got {typeof(TResponse).Name}.");
    }
}
