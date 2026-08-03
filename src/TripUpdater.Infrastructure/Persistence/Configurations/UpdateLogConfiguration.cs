using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripUpdater.Domain.Entities;

namespace TripUpdater.Infrastructure.Persistence.Configurations;

internal sealed class UpdateLogConfiguration : IEntityTypeConfiguration<UpdateLog>
{
    public void Configure(EntityTypeBuilder<UpdateLog> builder)
    {
        builder.ToTable("UpdateLogs");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.TripId).IsRequired();
        builder.Property(l => l.TripNo).IsRequired();
        builder.Property(l => l.UpdateTimestamp).IsRequired();
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(l => l.TripId);
        builder.HasIndex(l => l.TripNo);
        builder.HasIndex(l => l.UpdateTimestamp);

        builder.HasOne(l => l.Trip)
            .WithMany()
            .HasForeignKey(l => l.TripId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
