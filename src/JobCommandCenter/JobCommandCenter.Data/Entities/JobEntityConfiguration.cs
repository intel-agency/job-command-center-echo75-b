using JobCommandCenter.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobCommandCenter.Data.Entities;

/// <summary>
/// Entity type configuration for <see cref="JobEntity"/>.
/// Configures column types, indexes, and constraints for PostgreSQL.
/// </summary>
public class JobEntityConfiguration : IEntityTypeConfiguration<JobEntity>
{
    /// <summary>
    /// Configures the entity type.
    /// </summary>
    /// <param name="builder">The builder to configure the entity.</param>
    public void Configure(EntityTypeBuilder<JobEntity> builder)
    {
        builder.ToTable("Jobs");

        // Primary key
        builder.HasKey(e => e.Id);

        // LinkedIn job ID - unique identifier from LinkedIn
        builder.Property(e => e.LinkedInJobId)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnOrder(1);

        builder.HasIndex(e => e.LinkedInJobId)
            .IsUnique()
            .HasDatabaseName("IX_Jobs_LinkedInJobId");

        // Job title
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnOrder(2);

        // Company name
        builder.Property(e => e.Company)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnOrder(3);

        // Location (optional)
        builder.Property(e => e.Location)
            .HasMaxLength(500)
            .HasColumnOrder(4);

        // Pay rate (optional)
        builder.Property(e => e.PayRate)
            .HasMaxLength(200)
            .HasColumnOrder(7);

        // Job description - use text type for unlimited length
        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasColumnOrder(11);

        // Job URL
        builder.Property(e => e.Url)
            .IsRequired()
            .HasMaxLength(2000)
            .HasColumnOrder(12);

        // Score (calculated by scoring engine)
        builder.Property(e => e.Score)
            .HasDefaultValue(0)
            .HasColumnOrder(13);

        builder.HasIndex(e => e.Score)
            .HasDatabaseName("IX_Jobs_Score");

        // Status - store as integer for efficiency
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(JobStatus.Found)
            .HasColumnOrder(14);

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Jobs_Status");

        // Timestamps
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()")
            .HasColumnOrder(15);

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()")
            .HasColumnOrder(16);

        // Composite index for common query patterns
        builder.HasIndex(e => new { e.Status, e.CreatedAt })
            .HasDatabaseName("IX_Jobs_Status_CreatedAt");

        // Index for finding jobs by company
        builder.HasIndex(e => e.Company)
            .HasDatabaseName("IX_Jobs_Company");

        // Index for remote jobs
        builder.HasIndex(e => e.IsRemote)
            .HasDatabaseName("IX_Jobs_IsRemote");
    }
}
