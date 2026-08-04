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
        builder.Property(l => l.Id).ValueGeneratedOnAdd();
        builder.Property(l => l.LineNo).IsRequired();
        builder.Property(l => l.OperatorId).IsRequired();
        builder.Property(l => l.OperatorNo).IsRequired().HasMaxLength(50);
        builder.Property(l => l.LinePlanningNumber).IsRequired().HasMaxLength(100);
        builder.HasIndex(l => l.LineNo).IsUnique();
        builder.HasIndex(l => l.OperatorId);
        builder.HasIndex(l => l.OperatorNo);

        builder.HasOne(l => l.Operator)
            .WithMany()
            .HasForeignKey(l => l.OperatorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
