namespace JobCommandCenter.Shared.Models;

/// <summary>
/// Represents a job listing from LinkedIn.
/// </summary>
public class Job
{
    /// <summary>
    /// Unique identifier for the job.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// LinkedIn job ID.
    /// </summary>
    public required string LinkedInJobId { get; set; }

    /// <summary>
    /// Job title.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Company name.
    /// </summary>
    public required string Company { get; set; }

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
    public required string Url { get; set; }

    /// <summary>
    /// Calculated score from the scoring engine.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Current status in the pipeline.
    /// </summary>
    public JobStatus Status { get; set; } = JobStatus.Found;

    /// <summary>
    /// When the job was first discovered.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the job was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
