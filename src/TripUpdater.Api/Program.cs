using System.Text.Json;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TripUpdater.Api;
using TripUpdater.Api.Endpoints;
using TripUpdater.Application.Common.Behaviors;
using TripUpdater.Application.Trips.Queries.GetTrips;
using TripUpdater.Application.UpdateLogs.Queries.GetUpdateLogs;
using TripUpdater.Application.Updates.Commands.ProcessTripUpdates;
using TripUpdater.Domain.Common;
using TripUpdater.Infrastructure;
using TripUpdater.Infrastructure.Persistence;

[assembly: MediatorOptions(ServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped)]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddMediator();
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddValidatorsFromAssemblyContaining<TripUpdater.Application.Updates.Commands.ProcessTripUpdates.ProcessTripUpdatesValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // InMemory provider needs no schema creation; seed restores demo data each run.
    await db.Database.EnsureCreatedAsync();
    SeedData.Seed(db);
}

app.MapEndpoints();

app.Run();

namespace TripUpdater.Api
{
    public partial class Program { }

    public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later."
            };

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/problem+json";
            await JsonSerializer.SerializeAsync(httpContext.Response.Body, problem, cancellationToken: cancellationToken);
            return true;
        }
    }
}
