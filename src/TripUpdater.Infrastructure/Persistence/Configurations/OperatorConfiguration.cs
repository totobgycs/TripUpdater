using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripUpdater.Domain.Entities;

namespace TripUpdater.Infrastructure.Persistence.Configurations;

internal sealed class OperatorConfiguration : IEntityTypeConfiguration<Operator>
{
    public void Configure(EntityTypeBuilder<Operator> builder)
    {
        builder.ToTable("Operators");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.OperatorNo).IsRequired().HasMaxLength(50);
        builder.Property(o => o.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(o => o.OperatorNo).IsUnique();
    }
}
