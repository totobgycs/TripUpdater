using Mediator;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TripUpdater.Application.Common.Interfaces;
using TripUpdater.Domain.Common;

namespace TripUpdater.Application.UpdateLogs.Queries.GetUpdateLogs;

public sealed class GetUpdateLogsHandler(IAppDbContext db) : IRequestHandler<GetUpdateLogsQuery, Result<List<UpdateLogDto>>>
{
    public async ValueTask<Result<List<UpdateLogDto>>> Handle(GetUpdateLogsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.UpdateLog> query = db.UpdateLogs.AsNoTracking();

        if (request.From is not null)
        {
            var fromInstant = Instant.FromDateTimeOffset(request.From.Value);
            query = query.Where(l => l.UpdateTimestamp >= fromInstant);
        }

        if (request.To is not null)
        {
            var toInstant = Instant.FromDateTimeOffset(request.To.Value);
            query = query.Where(l => l.UpdateTimestamp <= toInstant);
        }

        if (request.Status is not null)
        {
            query = query.Where(l => l.Status == request.Status);
        }

        var logs = await query
            .OrderByDescending(l => l.UpdateTimestamp)
            .Select(l => new UpdateLogDto(l.Id, l.TripId, l.UpdateTimestamp.ToDateTimeOffset(), l.Status))
            .ToListAsync(cancellationToken);

        return Result<List<UpdateLogDto>>.Success(logs);
    }
}
