using System.ComponentModel;

namespace JobCommandCenter.Shared.Models;

/// <summary>
/// Represents the status of a job in the pipeline.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// Job has been scraped but not yet reviewed.
    /// </summary>
    [Description("Found")]
    Found = 0,

    /// <summary>
    /// Job has been saved for later review.
    /// </summary>
    [Description("Saved")]
    Saved = 1,

    /// <summary>
    /// Job has been scored by the scoring engine.
    /// </summary>
    [Description("Scored")]
    Scored = 2,

    /// <summary>
    /// User has approved the job for application.
    /// </summary>
    [Description("Pending")]
    Pending = 3,

    /// <summary>
    /// Application has been submitted.
    /// </summary>
    [Description("Applied")]
    Applied = 4,

    /// <summary>
    /// Interview has been scheduled.
    /// </summary>
    [Description("Interviewing")]
    Interviewing = 5,

    /// <summary>
    /// Job offer has been received.
    /// </summary>
    [Description("Offered")]
    Offered = 6,

    /// <summary>
    /// Job is no longer relevant (archived).
    /// </summary>
    [Description("Archived")]
    Archived = 7,

    /// <summary>
    /// User rejected the job.
    /// </summary>
    [Description("Rejected")]
    Rejected = 8
}
