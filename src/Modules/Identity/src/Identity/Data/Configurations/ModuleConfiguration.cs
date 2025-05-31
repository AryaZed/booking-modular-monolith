using Identity.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Data.Configurations;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("Modules");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(x => x.Description)
            .HasMaxLength(500);
            
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
            
        builder.Property(x => x.CreatedAt)
            .IsRequired();
            
        builder.Property(x => x.UpdatedAt);
        
        // Create unique index on module code
        builder.HasIndex(x => x.Code)
            .IsUnique();
    }
} 