using Mediator;
using Microsoft.EntityFrameworkCore;
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
            query = query.Where(l => l.UpdateTimestamp >= request.From);
        }

        if (request.To is not null)
        {
            query = query.Where(l => l.UpdateTimestamp <= request.To);
        }

        if (request.Status is not null)
        {
            query = query.Where(l => l.Status == request.Status);
        }

        var logs = await query
            .OrderByDescending(l => l.UpdateTimestamp)
            .Select(l => new UpdateLogDto(l.UpdateLogId, l.TripId, l.UpdateTimestamp, l.Status))
            .ToListAsync(cancellationToken);

        return Result<List<UpdateLogDto>>.Success(logs);
    }
}
