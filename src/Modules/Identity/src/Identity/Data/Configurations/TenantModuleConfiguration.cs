using Identity.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Data.Configurations;

public class TenantModuleConfiguration : IEntityTypeConfiguration<TenantModule>
{
    public void Configure(EntityTypeBuilder<TenantModule> builder)
    {
        builder.ToTable("TenantModules");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.TenantId)
            .IsRequired();
            
        builder.Property(x => x.ModuleId)
            .IsRequired();
            
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
            
        builder.Property(x => x.SubscribedAt)
            .IsRequired();
            
        builder.Property(x => x.ExpiresAt);
        
        builder.Property(x => x.DeactivatedAt);
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
            
        builder.Property(x => x.UpdatedAt);
        
        // Define relationships
        builder.HasOne(tm => tm.Tenant)
            .WithMany()
            .HasForeignKey(tm => tm.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(tm => tm.Module)
            .WithMany()
            .HasForeignKey(tm => tm.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Create a unique index on tenant + module
        builder.HasIndex(x => new { x.TenantId, x.ModuleId })
            .IsUnique();
            
        // Create an index for looking up tenant subscriptions
        builder.HasIndex(x => x.TenantId);
    }
} 