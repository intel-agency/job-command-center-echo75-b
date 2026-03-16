using JobCommandCenter.Shared.Models;

namespace JobCommandCenter.Data.Entities;

/// <summary>
/// Entity representation of a job for EF Core persistence.
/// </summary>
/// <remarks>
/// <para>
/// This entity is configured via <see cref="JobEntityConfiguration"/> which defines
/// column types, indexes, and constraints for PostgreSQL storage.
/// </para>
/// <para>
/// Use <see cref="FromModel"/> to create an entity from a domain model,
/// and <see cref="ToModel"/> to convert back to a domain model.
/// </para>
/// </remarks>
public class JobEntity
{
    /// <summary>
    /// Primary key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// LinkedIn job ID.
    /// </summary>
    public string LinkedInJobId { get; set; } = string.Empty;

    /// <summary>
    /// Job title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Company name.
    /// </summary>
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Job location.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Whether the job is remote.
    /// </summary>
    public bool IsRemote { get; set; }

    /// <summary>
    /// Whether the job is a contract position.
    /// </summary>
    public bool IsContract { get; set; }

    /// <summary>
    /// Pay rate if available.
    /// </summary>
    public string? PayRate { get; set; }

    /// <summary>
    /// Whether the user is a "Top Applicant".
    /// </summary>
    public bool IsTopApplicant { get; set; }

    /// <summary>
    /// Whether the job is promoted/sponsored.
    /// </summary>
    public bool IsPromoted { get; set; }

    /// <summary>
    /// Job description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// URL to the job posting.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Calculated score from the scoring engine.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Current status in the pipeline.
    /// </summary>
    public JobStatus Status { get; set; }

    /// <summary>
    /// When the job was first discovered.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the job was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Converts a domain model to an entity.
    /// </summary>
    public static JobEntity FromModel(Job model) => new()
    {
        Id = model.Id,
        LinkedInJobId = model.LinkedInJobId,
        Title = model.Title,
        Company = model.Company,
        Location = model.Location,
        IsRemote = model.IsRemote,
        IsContract = model.IsContract,
        PayRate = model.PayRate,
        IsTopApplicant = model.IsTopApplicant,
        IsPromoted = model.IsPromoted,
        Description = model.Description,
        Url = model.Url,
        Score = model.Score,
        Status = model.Status,
        CreatedAt = model.CreatedAt,
        UpdatedAt = model.UpdatedAt
    };

    /// <summary>
    /// Converts an entity to a domain model.
    /// </summary>
    public Job ToModel() => new()
    {
        Id = Id,
        LinkedInJobId = LinkedInJobId,
        Title = Title,
        Company = Company,
        Location = Location,
        IsRemote = IsRemote,
        IsContract = IsContract,
        PayRate = PayRate,
        IsTopApplicant = IsTopApplicant,
        IsPromoted = IsPromoted,
        Description = Description,
        Url = Url,
        Score = Score,
        Status = Status,
        CreatedAt = CreatedAt,
        UpdatedAt = UpdatedAt
    };
}
