using FluentAssertions;
using JobCommandCenter.Shared.Models;
using JobCommandCenter.Shared.Services;

namespace JobCommandCenter.UnitTests.Shared;

public class ScoringEngineTests
{
    #region Remote Job Scoring

    [Fact]
    public void CalculateScore_Should_AddRemotePoints_When_JobIsRemote()
    {
        // Arrange
        var config = CreateDefaultConfig();
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsRemote = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(50); // Default RemotePoints
    }

    [Fact]
    public void CalculateScore_Should_NotAddRemotePoints_When_JobIsNotRemote()
    {
        // Arrange
        var config = CreateDefaultConfig();
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsRemote = false;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(0);
    }

    [Fact]
    public void CalculateScore_Should_AddCustomRemotePoints_When_Configured()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.RemotePoints = 75;
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsRemote = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(75);
    }

    [Fact]
    public void CalculateScore_Should_NotAddPoints_When_RemotePointsIsZero()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.RemotePoints = 0;
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsRemote = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(0);
    }

    #endregion

    #region Contract Job Scoring

    [Fact]
    public void CalculateScore_Should_AddContractPoints_When_JobIsContract()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.ContractPoints = 25;
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsContract = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(25);
    }

    [Fact]
    public void CalculateScore_Should_NotAddContractPoints_When_JobIsNotContract()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.ContractPoints = 25;
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsContract = false;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(0);
    }

    [Fact]
    public void CalculateScore_Should_SubtractPoints_When_ContractPointsIsNegative()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.ContractPoints = -20;
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsContract = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(-20);
    }

    [Fact]
    public void CalculateScore_Should_NotAddPoints_When_ContractPointsIsZero()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.ContractPoints = 0;
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsContract = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(0);
    }

    #endregion

    #region Promoted Job Scoring (Penalty)

    [Fact]
    public void CalculateScore_Should_SubtractPromotedPenalty_When_JobIsPromoted()
    {
        // Arrange
        var config = CreateDefaultConfig();
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsPromoted = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(-20); // Default PromotedPenalty
    }

    [Fact]
    public void CalculateScore_Should_NotSubtractPromotedPenalty_When_JobIsNotPromoted()
    {
        // Arrange
        var config = CreateDefaultConfig();
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsPromoted = false;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(0);
    }

    [Fact]
    public void CalculateScore_Should_SubtractCustomPromotedPenalty_When_Configured()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.PromotedPenalty = 35;
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsPromoted = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(-35);
    }

    [Fact]
    public void CalculateScore_Should_NotSubtract_When_PromotedPenaltyIsZero()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.PromotedPenalty = 0;
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsPromoted = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(0);
    }

    #endregion

    #region Top Applicant Scoring

    [Fact]
    public void CalculateScore_Should_AddTopApplicantPoints_When_JobHasTopApplicantStatus()
    {
        // Arrange
        var config = CreateDefaultConfig();
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsTopApplicant = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(30); // Default TopApplicantPoints
    }

    [Fact]
    public void CalculateScore_Should_NotAddTopApplicantPoints_When_JobDoesNotHaveTopApplicantStatus()
    {
        // Arrange
        var config = CreateDefaultConfig();
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsTopApplicant = false;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(0);
    }

    [Fact]
    public void CalculateScore_Should_AddCustomTopApplicantPoints_When_Configured()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.TopApplicantPoints = 50;
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsTopApplicant = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(50);
    }

    #endregion

    #region Keyword Matching

    [Fact]
    public void CalculateScore_Should_AddKeywordPoints_When_DescriptionContainsKeyword()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.KeywordWeights.Add("python", 15);
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.Description = "We are looking for a python developer";

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(15);
    }

    [Fact]
    public void CalculateScore_Should_BeCaseInsensitive_When_MatchingKeywords()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.KeywordWeights.Add("PYTHON", 15);
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.Description = "We are looking for a python developer";

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(15);
    }

    [Fact]
    public void CalculateScore_Should_AddMultipleKeywordPoints_When_DescriptionContainsMultipleKeywords()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.KeywordWeights.Add("python", 15);
        config.KeywordWeights.Add("django", 10);
        config.KeywordWeights.Add("sql", 8);
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.Description = "We need a python developer with django and sql experience";

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(33); // 15 + 10 + 8
    }

    [Fact]
    public void CalculateScore_Should_NotAddKeywordPoints_When_DescriptionDoesNotContainKeyword()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.KeywordWeights.Add("python", 15);
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.Description = "We are looking for a java developer";

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(0);
    }

    [Fact]
    public void CalculateScore_Should_SubtractPoints_When_KeywordWeightIsNegative()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.KeywordWeights.Add("legacy", -20);
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.Description = "Maintain legacy codebase";

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(-20);
    }

    [Fact]
    public void CalculateScore_Should_MatchPartialWords()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.KeywordWeights.Add("architect", 25);
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.Description = "We need a software architect for our team";

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(25);
    }

    #endregion

    #region Combined Factors

    [Fact]
    public void CalculateScore_Should_CombineRemoteAndTopApplicant()
    {
        // Arrange
        var config = CreateDefaultConfig();
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsRemote = true;
        job.IsTopApplicant = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(80); // 50 (remote) + 30 (top applicant)
    }

    [Fact]
    public void CalculateScore_Should_CombineRemoteAndContract()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.ContractPoints = 15;
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsRemote = true;
        job.IsContract = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(65); // 50 (remote) + 15 (contract)
    }

    [Fact]
    public void CalculateScore_Should_CombineRemoteAndKeywords()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.KeywordWeights.Add("senior", 20);
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsRemote = true;
        job.Description = "Senior developer position";

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(70); // 50 (remote) + 20 (keyword)
    }

    [Fact]
    public void CalculateScore_Should_CombineAllPositiveFactors()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.ContractPoints = 10;
        config.KeywordWeights.Add("senior", 15);
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsRemote = true;
        job.IsContract = true;
        job.IsTopApplicant = true;
        job.Description = "Senior developer position";

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(105); // 50 (remote) + 10 (contract) + 30 (top applicant) + 15 (keyword)
    }

    [Fact]
    public void CalculateScore_Should_CombinePositiveAndNegativeFactors()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.KeywordWeights.Add("senior", 15);
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsRemote = true;
        job.IsPromoted = true;
        job.Description = "Senior developer position";

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(45); // 50 (remote) - 20 (promoted) + 15 (keyword)
    }

    [Fact]
    public void CalculateScore_Should_CalculateComplexCombination()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.ContractPoints = 10;
        config.KeywordWeights.Add("senior", 15);
        config.KeywordWeights.Add("legacy", -25);
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsRemote = true;
        job.IsContract = true;
        job.IsTopApplicant = true;
        job.IsPromoted = true;
        job.Description = "Senior developer to work on legacy systems";

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        // 50 (remote) + 10 (contract) + 30 (top applicant) - 20 (promoted) + 15 (senior) - 25 (legacy)
        score.Should().Be(60);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CalculateScore_Should_ReturnZero_When_NoFactorsApply()
    {
        // Arrange
        var config = CreateDefaultConfig();
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        // All boolean properties are false by default, no description

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(0);
    }

    [Fact]
    public void CalculateScore_Should_HandleNullDescription()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.KeywordWeights.Add("senior", 15);
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.Description = null;
        job.IsRemote = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(50); // Only remote points, no keyword crash
    }

    [Fact]
    public void CalculateScore_Should_HandleEmptyDescription()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.KeywordWeights.Add("senior", 15);
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.Description = "";
        job.IsRemote = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(50); // Only remote points, empty description doesn't match
    }

    [Fact]
    public void CalculateScore_Should_HandleWhitespaceOnlyDescription()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.KeywordWeights.Add("senior", 15);
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.Description = "   ";
        job.IsRemote = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(50); // Only remote points
    }

    [Fact]
    public void CalculateScore_Should_HandleEmptyKeywordWeights()
    {
        // Arrange
        var config = CreateDefaultConfig();
        // KeywordWeights is empty by default
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.Description = "Senior python developer";

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(0); // No keywords to match
    }

    [Fact]
    public void CalculateScore_Should_HandleNegativeScore()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.KeywordWeights.Add("legacy", -30);
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsPromoted = true;
        job.Description = "Maintain legacy systems";

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(-50); // -20 (promoted) - 30 (legacy keyword)
    }

    [Fact]
    public void CalculateScore_Should_NotModifyJob()
    {
        // Arrange
        var config = CreateDefaultConfig();
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsRemote = true;
        var originalScore = job.Score;

        // Act
        engine.CalculateScore(job);

        // Assert
        job.Score.Should().Be(originalScore); // Score not modified by CalculateScore
    }

    #endregion

    #region RecalculateScores Tests

    [Fact]
    public void RecalculateScores_Should_UpdateScoresForMultipleJobs()
    {
        // Arrange
        var config = CreateDefaultConfig();
        var engine = new ScoringEngine(config);
        var job1 = CreateValidJob();
        job1.IsRemote = true;
        var job2 = CreateValidJob();
        job2.IsTopApplicant = true;
        var job3 = CreateValidJob();
        job3.IsPromoted = true;
        var jobs = new List<Job> { job1, job2, job3 };

        // Act
        engine.RecalculateScores(jobs);

        // Assert
        jobs[0].Score.Should().Be(50);  // Remote
        jobs[1].Score.Should().Be(30);  // Top Applicant
        jobs[2].Score.Should().Be(-20); // Promoted
    }

    [Fact]
    public void RecalculateScores_Should_HandleEmptyList()
    {
        // Arrange
        var config = CreateDefaultConfig();
        var engine = new ScoringEngine(config);
        var jobs = new List<Job>();

        // Act & Assert - Should not throw
        var act = () => engine.RecalculateScores(jobs);
        act.Should().NotThrow();
    }

    [Fact]
    public void RecalculateScores_Should_UpdateAllJobScoresInList()
    {
        // Arrange
        var config = CreateDefaultConfig();
        config.KeywordWeights.Add("senior", 20);
        var engine = new ScoringEngine(config);
        var job1 = CreateValidJob();
        job1.IsRemote = true;
        job1.Description = "Senior role";
        job1.Score = 999;
        var job2 = CreateValidJob();
        job2.IsTopApplicant = true;
        job2.Score = 999;
        var jobs = new List<Job> { job1, job2 };

        // Act
        engine.RecalculateScores(jobs);

        // Assert
        jobs[0].Score.Should().Be(70);  // 50 (remote) + 20 (keyword)
        jobs[1].Score.Should().Be(30);  // Top Applicant
    }

    [Fact]
    public void RecalculateScores_Should_OverwriteExistingScores()
    {
        // Arrange
        var config = CreateDefaultConfig();
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.Score = 1000;
        var jobs = new List<Job> { job };

        // Act
        engine.RecalculateScores(jobs);

        // Assert
        job.Score.Should().Be(0); // Reset to calculated value (no factors)
    }

    [Fact]
    public void RecalculateScores_Should_HandleLargeJobList()
    {
        // Arrange
        var config = CreateDefaultConfig();
        var engine = new ScoringEngine(config);
        var jobs = new List<Job>();
        for (int i = 0; i < 100; i++)
        {
            var job = CreateValidJob();
            job.IsRemote = i % 2 == 0;
            jobs.Add(job);
        }

        // Act
        engine.RecalculateScores(jobs);

        // Assert
        for (int i = 0; i < 100; i++)
        {
            jobs[i].Score.Should().Be(i % 2 == 0 ? 50 : 0);
        }
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void ScoringEngine_Should_AcceptValidConfig()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act & Assert
        var act = () => new ScoringEngine(config);
        act.Should().NotThrow();
    }

    [Fact]
    public void ScoringEngine_Should_UseProvidedConfig()
    {
        // Arrange
        var config = new ScoringConfig
        {
            RemotePoints = 100,
            ContractPoints = 50,
            PromotedPenalty = 30,
            TopApplicantPoints = 75
        };
        var engine = new ScoringEngine(config);
        var job = CreateValidJob();
        job.IsRemote = true;
        job.IsContract = true;
        job.IsTopApplicant = true;
        job.IsPromoted = true;

        // Act
        var score = engine.CalculateScore(job);

        // Assert
        score.Should().Be(195); // 100 + 50 + 75 - 30
    }

    #endregion

    #region Helper Methods

    private static ScoringConfig CreateDefaultConfig()
    {
        return new ScoringConfig();
    }

    private static Job CreateValidJob()
    {
        return new Job
        {
            LinkedInJobId = "test-job-id",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test"
        };
    }

    #endregion
}
