namespace JobCommandCenter.Shared.Models;

/// <summary>
/// Represents the status of a job in the pipeline.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// Job has been scraped but not yet reviewed.
    /// </summary>
    Found = 0,

    /// <summary>
    /// Job has been scored by the scoring engine.
    /// </summary>
    Scored = 1,

    /// <summary>
    /// User has approved the job for application.
    /// </summary>
    Pending = 2,

    /// <summary>
    /// Application has been submitted.
    /// </summary>
    Applied = 3,

    /// <summary>
    /// Interview has been scheduled.
    /// </summary>
    Interviewing = 4,

    /// <summary>
    /// Job is no longer relevant (archived).
    /// </summary>
    Archived = 5,

    /// <summary>
    /// User rejected the job.
    /// </summary>
    Rejected = 6
}
