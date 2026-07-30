using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TripUpdater.Domain.Entities;

namespace TripUpdater.Infrastructure.Persistence.Configurations;

internal sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("Trips");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TripId).IsRequired();
        builder.Property(t => t.LineNo).IsRequired();
        builder.Property(t => t.DepartureTime).IsRequired().HasConversion<DateTimeOffsetToTicksConverter>();
        builder.Property(t => t.OriginalArrivalTime).IsRequired().HasConversion<DateTimeOffsetToTicksConverter>();
        builder.Property(t => t.ArrivalTime).HasConversion<DateTimeOffsetToTicksConverter>();
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(t => t.TripId).IsUnique();
        builder.HasIndex(t => t.DepartureTime);

        builder.HasOne(t => t.Line)
            .WithMany()
            .HasForeignKey(t => t.LineNo)
            .HasPrincipalKey(l => l.LineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class DateTimeOffsetToTicksConverter : ValueConverter<DateTimeOffset, long>
{
    public DateTimeOffsetToTicksConverter()
        : base(v => v.UtcTicks, v => new DateTimeOffset(v, TimeSpan.Zero)) { }
}
