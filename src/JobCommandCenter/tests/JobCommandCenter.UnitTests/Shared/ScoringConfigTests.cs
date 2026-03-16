using FluentAssertions;
using JobCommandCenter.Shared.Models;

namespace JobCommandCenter.UnitTests.Shared;

public class ScoringConfigTests
{
    #region Default Property Values

    [Fact]
    public void ScoringConfig_Should_DefaultRemotePointsToFifty()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.RemotePoints.Should().Be(50);
    }

    [Fact]
    public void ScoringConfig_Should_DefaultContractPointsToZero()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.ContractPoints.Should().Be(0);
    }

    [Fact]
    public void ScoringConfig_Should_DefaultPromotedPenaltyToTwenty()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.PromotedPenalty.Should().Be(20);
    }

    [Fact]
    public void ScoringConfig_Should_DefaultTopApplicantPointsToThirty()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.TopApplicantPoints.Should().Be(30);
    }

    #endregion

    #region KeywordWeights Dictionary

    [Fact]
    public void ScoringConfig_Should_InitializeKeywordWeightsAsEmpty()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.KeywordWeights.Should().NotBeNull();
        config.KeywordWeights.Should().BeEmpty();
    }

    [Fact]
    public void ScoringConfig_Should_AllowAddingKeywordWeights()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.KeywordWeights.Add("senior", 10);
        config.KeywordWeights.Add("lead", 15);
        config.KeywordWeights.Add("architect", 20);

        // Assert
        config.KeywordWeights.Should().HaveCount(3);
        config.KeywordWeights["senior"].Should().Be(10);
        config.KeywordWeights["lead"].Should().Be(15);
        config.KeywordWeights["architect"].Should().Be(20);
    }

    [Fact]
    public void ScoringConfig_Should_AllowUpdatingKeywordWeights()
    {
        // Arrange
        var config = new ScoringConfig();
        config.KeywordWeights["senior"] = 10;

        // Act
        config.KeywordWeights["senior"] = 25;

        // Assert
        config.KeywordWeights["senior"].Should().Be(25);
    }

    [Fact]
    public void ScoringConfig_Should_AllowRemovingKeywordWeights()
    {
        // Arrange
        var config = new ScoringConfig();
        config.KeywordWeights.Add("senior", 10);

        // Act
        config.KeywordWeights.Remove("senior");

        // Assert
        config.KeywordWeights.Should().BeEmpty();
    }

    [Fact]
    public void ScoringConfig_Should_AllowClearingKeywordWeights()
    {
        // Arrange
        var config = new ScoringConfig();
        config.KeywordWeights.Add("senior", 10);
        config.KeywordWeights.Add("lead", 15);

        // Act
        config.KeywordWeights.Clear();

        // Assert
        config.KeywordWeights.Should().BeEmpty();
    }

    [Fact]
    public void ScoringConfig_Should_HandleNegativeKeywordWeights()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.KeywordWeights.Add("legacy", -10);

        // Assert
        config.KeywordWeights["legacy"].Should().Be(-10);
    }

    [Fact]
    public void ScoringConfig_Should_HandleZeroKeywordWeights()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.KeywordWeights.Add("neutral", 0);

        // Assert
        config.KeywordWeights["neutral"].Should().Be(0);
    }

    #endregion

    #region Custom Configuration Scenarios

    [Fact]
    public void ScoringConfig_Should_AllowCustomRemotePoints()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.RemotePoints = 100;

        // Assert
        config.RemotePoints.Should().Be(100);
    }

    [Fact]
    public void ScoringConfig_Should_AllowCustomContractPoints()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.ContractPoints = 25;

        // Assert
        config.ContractPoints.Should().Be(25);
    }

    [Fact]
    public void ScoringConfig_Should_AllowCustomPromotedPenalty()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.PromotedPenalty = 50;

        // Assert
        config.PromotedPenalty.Should().Be(50);
    }

    [Fact]
    public void ScoringConfig_Should_AllowCustomTopApplicantPoints()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.TopApplicantPoints = 40;

        // Assert
        config.TopApplicantPoints.Should().Be(40);
    }

    [Fact]
    public void ScoringConfig_Should_AllowNegativeContractPoints()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.ContractPoints = -15;

        // Assert
        config.ContractPoints.Should().Be(-15);
    }

    [Fact]
    public void ScoringConfig_Should_AllowZeroRemotePoints()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.RemotePoints = 0;

        // Assert
        config.RemotePoints.Should().Be(0);
    }

    [Fact]
    public void ScoringConfig_Should_AllowZeroPromotedPenalty()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.PromotedPenalty = 0;

        // Assert
        config.PromotedPenalty.Should().Be(0);
    }

    [Fact]
    public void ScoringConfig_Should_AllowZeroTopApplicantPoints()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.TopApplicantPoints = 0;

        // Assert
        config.TopApplicantPoints.Should().Be(0);
    }

    #endregion

    #region Combined Configuration Tests

    [Fact]
    public void ScoringConfig_Should_SupportFullCustomConfiguration()
    {
        // Arrange & Act
        var config = new ScoringConfig
        {
            RemotePoints = 75,
            ContractPoints = -10,
            PromotedPenalty = 30,
            TopApplicantPoints = 45,
            KeywordWeights = new Dictionary<string, int>
            {
                { "senior", 20 },
                { "remote-first", 15 },
                { "on-call", -25 }
            }
        };

        // Assert
        config.RemotePoints.Should().Be(75);
        config.ContractPoints.Should().Be(-10);
        config.PromotedPenalty.Should().Be(30);
        config.TopApplicantPoints.Should().Be(45);
        config.KeywordWeights.Should().HaveCount(3);
    }

    [Fact]
    public void ScoringConfig_Should_SupportContractBonusConfiguration()
    {
        // Arrange & Act
        var config = new ScoringConfig
        {
            ContractPoints = 25
        };

        // Assert
        config.ContractPoints.Should().Be(25);
    }

    [Fact]
    public void ScoringConfig_Should_SupportMultipleKeywordConfigurations()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.KeywordWeights = new Dictionary<string, int>
        {
            { "python", 15 },
            { "c#", 20 },
            { "javascript", 10 },
            { "sql", 12 },
            { "docker", 8 },
            { "kubernetes", 12 }
        };

        // Assert
        config.KeywordWeights.Should().HaveCount(6);
        config.KeywordWeights["python"].Should().Be(15);
        config.KeywordWeights["c#"].Should().Be(20);
        config.KeywordWeights["javascript"].Should().Be(10);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ScoringConfig_Should_AllowLargePointValues()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.RemotePoints = 1000;
        config.ContractPoints = 500;
        config.TopApplicantPoints = 750;

        // Assert
        config.RemotePoints.Should().Be(1000);
        config.ContractPoints.Should().Be(500);
        config.TopApplicantPoints.Should().Be(750);
    }

    [Fact]
    public void ScoringConfig_Should_AllowLargePenaltyValues()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.PromotedPenalty = 500;

        // Assert
        config.PromotedPenalty.Should().Be(500);
    }

    [Fact]
    public void ScoringConfig_KeywordWeights_ShouldBeCaseSensitiveByKey()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.KeywordWeights["Senior"] = 10;
        config.KeywordWeights["senior"] = 5;

        // Assert
        config.KeywordWeights.Should().HaveCount(2);
        config.KeywordWeights["Senior"].Should().Be(10);
        config.KeywordWeights["senior"].Should().Be(5);
    }

    [Fact]
    public void ScoringConfig_Should_AllowReplacingKeywordWeightsDictionary()
    {
        // Arrange
        var config = new ScoringConfig();
        config.KeywordWeights.Add("old", 5);

        // Act
        config.KeywordWeights = new Dictionary<string, int>
        {
            { "new", 10 }
        };

        // Assert
        config.KeywordWeights.Should().HaveCount(1);
        config.KeywordWeights.ContainsKey("old").Should().BeFalse();
        config.KeywordWeights["new"].Should().Be(10);
    }

    #endregion

    #region Configuration Validation Scenarios

    [Fact]
    public void ScoringConfig_Should_SupportPenaltyOnlyConfiguration()
    {
        // Arrange & Act
        var config = new ScoringConfig
        {
            RemotePoints = 0,
            ContractPoints = 0,
            PromotedPenalty = 100,
            TopApplicantPoints = 0
        };

        // Assert - All bonuses disabled, only penalty
        config.RemotePoints.Should().Be(0);
        config.ContractPoints.Should().Be(0);
        config.PromotedPenalty.Should().Be(100);
        config.TopApplicantPoints.Should().Be(0);
    }

    [Fact]
    public void ScoringConfig_Should_SupportBonusOnlyConfiguration()
    {
        // Arrange & Act
        var config = new ScoringConfig
        {
            RemotePoints = 100,
            ContractPoints = 50,
            PromotedPenalty = 0,
            TopApplicantPoints = 75
        };

        // Assert - All penalties disabled, only bonuses
        config.RemotePoints.Should().Be(100);
        config.ContractPoints.Should().Be(50);
        config.PromotedPenalty.Should().Be(0);
        config.TopApplicantPoints.Should().Be(75);
    }

    [Fact]
    public void ScoringConfig_Should_SupportNeutralConfiguration()
    {
        // Arrange & Act
        var config = new ScoringConfig
        {
            RemotePoints = 0,
            ContractPoints = 0,
            PromotedPenalty = 0,
            TopApplicantPoints = 0
        };

        // Assert - All scoring disabled
        config.RemotePoints.Should().Be(0);
        config.ContractPoints.Should().Be(0);
        config.PromotedPenalty.Should().Be(0);
        config.TopApplicantPoints.Should().Be(0);
    }

    #endregion
}
