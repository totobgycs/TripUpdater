using NodaTime;
using TripUpdater.Domain.Entities;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Infrastructure.Persistence;

public static class SeedData
{
    public static void Seed(AppDbContext db)
    {
        if (db.Operators.Any() || db.Lines.Any() || db.Trips.Any())
        {
            return;
        }

        var op1 = Operator.Create("GVB", "Gemeentelijk Vervoerbedrijf Amsterdam");
        var op2 = Operator.Create("NS", "Nederlandse Spoorwegen");
        var op3 = Operator.Create("RET", "Rotterdamse Elektrische Tram");

        db.Operators.AddRange(op1, op2, op3);

        var line1 = Line.Create(1, "GVB", "GVB-1-AMS", op1);
        var line2 = Line.Create(2, "GVB", "GVB-2-AMS", op1);
        var line3 = Line.Create(3, "NS", "NS-3-UTR", op2);
        var line4 = Line.Create(4, "RET", "RET-4-RTD", op3);

        db.Lines.AddRange(line1, line2, line3, line4);

        var baseDate = Instant.FromUtc(2026, 7, 29, 8, 0, 0);

        var trips = new[]
        {
            Trip.Create(1001, 1, baseDate,                              baseDate + Duration.FromMinutes(30)),
            Trip.Create(1002, 1, baseDate + Duration.FromMinutes(15),   baseDate + Duration.FromMinutes(45)),
            Trip.Create(1003, 1, baseDate + Duration.FromMinutes(30),   baseDate + Duration.FromMinutes(60)),
            Trip.Create(1004, 2, baseDate + Duration.FromHours(1),      baseDate + Duration.FromHours(1) + Duration.FromMinutes(25)),
            Trip.Create(1005, 3, baseDate + Duration.FromHours(2),      baseDate + Duration.FromHours(2) + Duration.FromMinutes(40)),
            Trip.Create(1006, 4, baseDate + Duration.FromHours(3),      baseDate + Duration.FromHours(3) + Duration.FromMinutes(20)),
            Trip.Create(1007, 1, baseDate + Duration.FromHours(4),      baseDate + Duration.FromHours(4) + Duration.FromMinutes(30)),
            Trip.Create(1008, 2, baseDate + Duration.FromHours(5),      baseDate + Duration.FromHours(5) + Duration.FromMinutes(25))
        };

        db.Trips.AddRange(trips);
        db.SaveChanges();
    }
}
