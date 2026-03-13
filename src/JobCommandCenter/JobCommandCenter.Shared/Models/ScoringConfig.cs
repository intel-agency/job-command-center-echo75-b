namespace JobCommandCenter.Shared.Models;

/// <summary>
/// Configuration for the scoring engine weights.
/// </summary>
public class ScoringConfig
{
    /// <summary>
    /// Points awarded for remote positions.
    /// </summary>
    public int RemotePoints { get; set; } = 50;

    /// <summary>
    /// Points awarded for contract positions.
    /// </summary>
    public int ContractPoints { get; set; } = 0;

    /// <summary>
    /// Points deducted for promoted/sponsored jobs.
    /// </summary>
    public int PromotedPenalty { get; set; } = 20;

    /// <summary>
    /// Points awarded for "Top Applicant" status.
    /// </summary>
    public int TopApplicantPoints { get; set; } = 30;

    /// <summary>
    /// Keyword weights (keyword -> points).
    /// </summary>
    public Dictionary<string, int> KeywordWeights { get; set; } = new();
}
