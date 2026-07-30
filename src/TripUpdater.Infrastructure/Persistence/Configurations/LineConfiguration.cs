using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripUpdater.Domain.Entities;

namespace TripUpdater.Infrastructure.Persistence.Configurations;

internal sealed class LineConfiguration : IEntityTypeConfiguration<Line>
{
    public void Configure(EntityTypeBuilder<Line> builder)
    {
        builder.ToTable("Lines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.LineId).IsRequired();
        builder.Property(l => l.OperatorNo).IsRequired().HasMaxLength(50);
        builder.Property(l => l.LinePlanningNumber).IsRequired().HasMaxLength(100);
        builder.HasIndex(l => l.LineId).IsUnique();

        builder.HasOne(l => l.Operator)
            .WithMany()
            .HasForeignKey(l => l.OperatorNo)
            .HasPrincipalKey(o => o.OperatorNo)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
