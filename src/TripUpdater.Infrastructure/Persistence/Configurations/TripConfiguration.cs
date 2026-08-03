using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripUpdater.Domain.Entities;

namespace TripUpdater.Infrastructure.Persistence.Configurations;

internal sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("Trips");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TripNo).IsRequired();
        builder.Property(t => t.LineId).IsRequired();
        builder.Property(t => t.LineNo).IsRequired();
        builder.Property(t => t.DepartureTime).IsRequired();
        builder.Property(t => t.OriginalArrivalTime).IsRequired();
        builder.Property(t => t.ArrivalTime);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(t => t.TripNo).IsUnique();
        builder.HasIndex(t => t.DepartureTime);
        builder.HasIndex(t => t.LineId);
        builder.HasIndex(t => t.LineNo);

        builder.HasOne(t => t.Line)
            .WithMany()
            .HasForeignKey(t => t.LineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.UpdateLogs)
            .WithOne(l => l.Trip)
            .HasForeignKey(l => l.TripId)
            .HasPrincipalKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.UpdateLogs).AutoInclude(false);
    }
}
