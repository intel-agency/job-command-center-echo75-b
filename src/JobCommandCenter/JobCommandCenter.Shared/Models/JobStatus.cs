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
    /// Application has been submitted.
    /// </summary>
    [Description("Applied")]
    Applied = 2,

    /// <summary>
    /// Interview has been scheduled.
    /// </summary>
    [Description("Interviewing")]
    Interviewing = 3,

    /// <summary>
    /// An offer has been received.
    /// </summary>
    [Description("Offered")]
    Offered = 4,

    /// <summary>
    /// User rejected the job.
    /// </summary>
    [Description("Rejected")]
    Rejected = 5,

    /// <summary>
    /// Job is no longer relevant (archived).
    /// </summary>
    [Description("Archived")]
    Archived = 6
}
