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
        var op2 = Operator.Create("NS",  "Nederlandse Spoorwegen");
        var op3 = Operator.Create("RET", "Rotterdamse Elektrische Tram");

        db.Operators.AddRange(op1, op2, op3);

        var line1 = Line.Create(1, "GVB", "GVB-1-AMS", op1);
        var line2 = Line.Create(2, "GVB", "GVB-2-AMS", op1);
        var line3 = Line.Create(3, "NS",  "NS-3-UTR",  op2);
        var line4 = Line.Create(4, "RET", "RET-4-RTD", op3);

        db.Lines.AddRange(line1, line2, line3, line4);

        var baseDate = new DateTimeOffset(2026, 7, 29, 8, 0, 0, TimeSpan.Zero);

        var trips = new[]
        {
            Trip.Create(1001, 1, baseDate,                     baseDate.AddMinutes(30)),
            Trip.Create(1002, 1, baseDate.AddMinutes(15),      baseDate.AddMinutes(45)),
            Trip.Create(1003, 1, baseDate.AddMinutes(30),      baseDate.AddMinutes(60)),
            Trip.Create(1004, 2, baseDate.AddHours(1),         baseDate.AddHours(1).AddMinutes(25)),
            Trip.Create(1005, 3, baseDate.AddHours(2),         baseDate.AddHours(2).AddMinutes(40)),
            Trip.Create(1006, 4, baseDate.AddHours(3),         baseDate.AddHours(3).AddMinutes(20)),
            Trip.Create(1007, 1, baseDate.AddHours(4),         baseDate.AddHours(4).AddMinutes(30)),
            Trip.Create(1008, 2, baseDate.AddHours(5),         baseDate.AddHours(5).AddMinutes(25))
        };

        db.Trips.AddRange(trips);
        db.SaveChanges();
    }
}
