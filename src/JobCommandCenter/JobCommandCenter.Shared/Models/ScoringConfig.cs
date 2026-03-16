using System.ComponentModel.DataAnnotations;

namespace JobCommandCenter.Shared.Models;

/// <summary>
/// Configuration for the scoring engine weights.
/// </summary>
public class ScoringConfig
{
    /// <summary>
    /// Points awarded for remote positions.
    /// </summary>
    [Range(0, 100)]
    public int RemotePoints { get; set; } = 50;

    /// <summary>
    /// Points awarded for contract positions.
    /// </summary>
    [Range(0, 100)]
    public int ContractPoints { get; set; } = 0;

    /// <summary>
    /// Points deducted for promoted/sponsored jobs.
    /// </summary>
    [Range(0, 100)]
    public int PromotedPenalty { get; set; } = 20;

    /// <summary>
    /// Points awarded for "Top Applicant" status.
    /// </summary>
    [Range(0, 100)]
    public int TopApplicantPoints { get; set; } = 30;

    /// <summary>
    /// Keyword weights (keyword -> points).
    /// </summary>
    public Dictionary<string, int> KeywordWeights { get; set; } = new();

    /// <summary>
    /// Location preferences for scoring.
    /// </summary>
    public List<string> PreferredLocations { get; set; } = new();

    /// <summary>
    /// Company preferences for scoring.
    /// </summary>
    public List<string> PreferredCompanies { get; set; } = new();

    /// <summary>
    /// Minimum score threshold for consideration.
    /// </summary>
    [Range(0, 100)]
    public int MinimumScore { get; set; } = 0;

    /// <summary>
    /// Score threshold for auto-apply feature.
    /// </summary>
    [Range(0, 100)]
    public int AutoApplyThreshold { get; set; } = 80;
}
