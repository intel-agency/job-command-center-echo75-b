using JobCommandCenter.Shared.Models;

namespace JobCommandCenter.Shared.Services;

/// <summary>
/// Engine for calculating job scores based on configurable weights.
/// </summary>
public class ScoringEngine
{
    private readonly ScoringConfig _config;

    /// <summary>
    /// Creates a new scoring engine with the specified configuration.
    /// </summary>
    public ScoringEngine(ScoringConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Calculates the score for a job based on the current configuration.
    /// </summary>
    public int CalculateScore(Job job)
    {
        int score = 0;

        // Remote bonus
        if (job.IsRemote)
        {
            score += _config.RemotePoints;
        }

        // Contract bonus/penalty
        if (job.IsContract)
        {
            score += _config.ContractPoints;
        }

        // Top applicant bonus
        if (job.IsTopApplicant)
        {
            score += _config.TopApplicantPoints;
        }

        // Promoted penalty
        if (job.IsPromoted)
        {
            score -= _config.PromotedPenalty;
        }

        // Keyword matching
        if (!string.IsNullOrEmpty(job.Description))
        {
            foreach (var (keyword, points) in _config.KeywordWeights)
            {
                if (job.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    score += points;
                }
            }
        }

        return score;
    }

    /// <summary>
    /// Recalculates scores for all jobs.
    /// </summary>
    public void RecalculateScores(IEnumerable<Job> jobs)
    {
        foreach (var job in jobs)
        {
            job.Score = CalculateScore(job);
        }
    }
}
