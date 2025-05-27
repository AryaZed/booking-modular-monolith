using Identity.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Data.Configurations;

public class OneTimePasswordConfiguration : IEntityTypeConfiguration<OneTimePassword>
{
    public void Configure(EntityTypeBuilder<OneTimePassword> builder)
    {
        builder.ToTable("OneTimePasswords");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.UserId)
            .IsRequired();
            
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(x => x.CreatedAt)
            .IsRequired();
            
        builder.Property(x => x.ExpiresAt)
            .IsRequired();
            
        builder.Property(x => x.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(x => x.Purpose)
            .IsRequired()
            .HasConversion<int>();
            
        builder.Property(x => x.Reference)
            .HasMaxLength(100);
            
        // Create index on UserId
        builder.HasIndex(x => x.UserId);
        
        // Create index on UserId + Purpose (common lookup pattern)
        builder.HasIndex(x => new { x.UserId, x.Purpose });
        
        // Ensure we can clean up expired and used OTPs efficiently
        builder.HasIndex(x => new { x.IsUsed, x.ExpiresAt });
    }
} 