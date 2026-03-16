using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using JobCommandCenter.Shared.Models;
using Xunit;

namespace JobCommandCenter.UnitTests.Shared;

/// <summary>
/// Unit tests for the ScoringConfig model.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Domain Models")]
public class ScoringConfigTests
{
    #region Default Values Tests

    [Fact]
    public void ScoringConfig_DefaultConstructor_ShouldSetMinimumScoreTo0()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.MinimumScore.Should().Be(0);
    }

    [Fact]
    public void ScoringConfig_DefaultConstructor_ShouldSetAutoApplyThresholdTo80()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.AutoApplyThreshold.Should().Be(80);
    }

    [Fact]
    public void ScoringConfig_DefaultConstructor_ShouldSetRemotePointsTo50()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.RemotePoints.Should().Be(50);
    }

    [Fact]
    public void ScoringConfig_DefaultConstructor_ShouldSetContractPointsTo0()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.ContractPoints.Should().Be(0);
    }

    [Fact]
    public void ScoringConfig_DefaultConstructor_ShouldSetPromotedPenaltyTo20()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.PromotedPenalty.Should().Be(20);
    }

    [Fact]
    public void ScoringConfig_DefaultConstructor_ShouldSetTopApplicantPointsTo30()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.TopApplicantPoints.Should().Be(30);
    }

    #endregion

    #region Dictionary Initialization Tests

    [Fact]
    public void ScoringConfig_DefaultConstructor_ShouldInitializeKeywordWeightsAsEmpty()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.KeywordWeights.Should().NotBeNull();
        config.KeywordWeights.Should().BeEmpty();
    }

    [Fact]
    public void ScoringConfig_DefaultConstructor_ShouldInitializeLocationPreferencesAsEmpty()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.LocationPreferences.Should().NotBeNull();
        config.LocationPreferences.Should().BeEmpty();
    }

    [Fact]
    public void ScoringConfig_DefaultConstructor_ShouldInitializeCompanyPreferencesAsEmpty()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.CompanyPreferences.Should().NotBeNull();
        config.CompanyPreferences.Should().BeEmpty();
    }

    #endregion

    #region MinimumScore Range Validation Tests

    [Theory]
    [InlineData(0, true)]     // Minimum boundary
    [InlineData(100, true)]   // Maximum boundary
    [InlineData(50, true)]    // Middle value
    [InlineData(-1, false)]   // Below minimum
    [InlineData(101, false)]  // Above maximum
    public void ScoringConfig_MinimumScore_ShouldValidateRange(int value, bool isValid)
    {
        // Arrange
        var config = new ScoringConfig { MinimumScore = value };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        if (isValid)
        {
            validationResults.Should().NotContain(r => r.MemberNames.Contains("MinimumScore"));
        }
        else
        {
            validationResults.Should().Contain(r => r.MemberNames.Contains("MinimumScore"));
        }
    }

    #endregion

    #region AutoApplyThreshold Range Validation Tests

    [Theory]
    [InlineData(0, true)]     // Minimum boundary
    [InlineData(100, true)]   // Maximum boundary
    [InlineData(80, true)]    // Default value
    [InlineData(-1, false)]   // Below minimum
    [InlineData(101, false)]  // Above maximum
    public void ScoringConfig_AutoApplyThreshold_ShouldValidateRange(int value, bool isValid)
    {
        // Arrange
        var config = new ScoringConfig { AutoApplyThreshold = value };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        if (isValid)
        {
            validationResults.Should().NotContain(r => r.MemberNames.Contains("AutoApplyThreshold"));
        }
        else
        {
            validationResults.Should().Contain(r => r.MemberNames.Contains("AutoApplyThreshold"));
        }
    }

    #endregion

    #region RemotePoints Range Validation Tests

    [Theory]
    [InlineData(0, true)]     // Minimum boundary
    [InlineData(100, true)]   // Maximum boundary
    [InlineData(50, true)]    // Default value
    [InlineData(-1, false)]   // Below minimum
    [InlineData(101, false)]  // Above maximum
    public void ScoringConfig_RemotePoints_ShouldValidateRange(int value, bool isValid)
    {
        // Arrange
        var config = new ScoringConfig { RemotePoints = value };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        if (isValid)
        {
            validationResults.Should().NotContain(r => r.MemberNames.Contains("RemotePoints"));
        }
        else
        {
            validationResults.Should().Contain(r => r.MemberNames.Contains("RemotePoints"));
        }
    }

    #endregion

    #region ContractPoints Range Validation Tests

    [Theory]
    [InlineData(0, true)]     // Minimum boundary
    [InlineData(100, true)]   // Maximum boundary
    [InlineData(50, true)]    // Middle value
    [InlineData(-1, false)]   // Below minimum
    [InlineData(101, false)]  // Above maximum
    public void ScoringConfig_ContractPoints_ShouldValidateRange(int value, bool isValid)
    {
        // Arrange
        var config = new ScoringConfig { ContractPoints = value };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        if (isValid)
        {
            validationResults.Should().NotContain(r => r.MemberNames.Contains("ContractPoints"));
        }
        else
        {
            validationResults.Should().Contain(r => r.MemberNames.Contains("ContractPoints"));
        }
    }

    #endregion

    #region PromotedPenalty Range Validation Tests

    [Theory]
    [InlineData(0, true)]     // Minimum boundary
    [InlineData(100, true)]   // Maximum boundary
    [InlineData(20, true)]    // Default value
    [InlineData(-1, false)]   // Below minimum
    [InlineData(101, false)]  // Above maximum
    public void ScoringConfig_PromotedPenalty_ShouldValidateRange(int value, bool isValid)
    {
        // Arrange
        var config = new ScoringConfig { PromotedPenalty = value };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        if (isValid)
        {
            validationResults.Should().NotContain(r => r.MemberNames.Contains("PromotedPenalty"));
        }
        else
        {
            validationResults.Should().Contain(r => r.MemberNames.Contains("PromotedPenalty"));
        }
    }

    #endregion

    #region TopApplicantPoints Range Validation Tests

    [Theory]
    [InlineData(0, true)]     // Minimum boundary
    [InlineData(100, true)]   // Maximum boundary
    [InlineData(30, true)]    // Default value
    [InlineData(-1, false)]   // Below minimum
    [InlineData(101, false)]  // Above maximum
    public void ScoringConfig_TopApplicantPoints_ShouldValidateRange(int value, bool isValid)
    {
        // Arrange
        var config = new ScoringConfig { TopApplicantPoints = value };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        if (isValid)
        {
            validationResults.Should().NotContain(r => r.MemberNames.Contains("TopApplicantPoints"));
        }
        else
        {
            validationResults.Should().Contain(r => r.MemberNames.Contains("TopApplicantPoints"));
        }
    }

    #endregion

    #region Set and Retrieve Properties Tests

    [Fact]
    public void ScoringConfig_CanSetAndRetrieveMinimumScore()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.MinimumScore = 25;

        // Assert
        config.MinimumScore.Should().Be(25);
    }

    [Fact]
    public void ScoringConfig_CanSetAndRetrieveAutoApplyThreshold()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.AutoApplyThreshold = 90;

        // Assert
        config.AutoApplyThreshold.Should().Be(90);
    }

    [Fact]
    public void ScoringConfig_CanSetAndRetrieveRemotePoints()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.RemotePoints = 75;

        // Assert
        config.RemotePoints.Should().Be(75);
    }

    [Fact]
    public void ScoringConfig_CanSetAndRetrieveContractPoints()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.ContractPoints = 40;

        // Assert
        config.ContractPoints.Should().Be(40);
    }

    [Fact]
    public void ScoringConfig_CanSetAndRetrievePromotedPenalty()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.PromotedPenalty = 15;

        // Assert
        config.PromotedPenalty.Should().Be(15);
    }

    [Fact]
    public void ScoringConfig_CanSetAndRetrieveTopApplicantPoints()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.TopApplicantPoints = 45;

        // Assert
        config.TopApplicantPoints.Should().Be(45);
    }

    #endregion

    #region Dictionary Operations Tests

    [Fact]
    public void ScoringConfig_KeywordWeights_CanAddAndRetrieveItems()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.KeywordWeights.Add("C#", 20);
        config.KeywordWeights.Add(".NET", 15);
        config.KeywordWeights.Add("Azure", 10);

        // Assert
        config.KeywordWeights.Should().HaveCount(3);
        config.KeywordWeights["C#"].Should().Be(20);
        config.KeywordWeights[".NET"].Should().Be(15);
        config.KeywordWeights["Azure"].Should().Be(10);
    }

    [Fact]
    public void ScoringConfig_LocationPreferences_CanAddAndRetrieveItems()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.LocationPreferences.Add("Remote", 30);
        config.LocationPreferences.Add("San Francisco", 20);
        config.LocationPreferences.Add("New York", -10);

        // Assert
        config.LocationPreferences.Should().HaveCount(3);
        config.LocationPreferences["Remote"].Should().Be(30);
        config.LocationPreferences["San Francisco"].Should().Be(20);
        config.LocationPreferences["New York"].Should().Be(-10);
    }

    [Fact]
    public void ScoringConfig_CompanyPreferences_CanAddAndRetrieveItems()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.CompanyPreferences.Add("Microsoft", 25);
        config.CompanyPreferences.Add("Google", 30);
        config.CompanyPreferences.Add("Bad Company", -50);

        // Assert
        config.CompanyPreferences.Should().HaveCount(3);
        config.CompanyPreferences["Microsoft"].Should().Be(25);
        config.CompanyPreferences["Google"].Should().Be(30);
        config.CompanyPreferences["Bad Company"].Should().Be(-50);
    }

    [Fact]
    public void ScoringConfig_Dictionaries_CanBeReplaced()
    {
        // Arrange
        var config = new ScoringConfig();
        var newKeywords = new Dictionary<string, int>
        {
            { "React", 25 },
            { "TypeScript", 20 }
        };

        // Act
        config.KeywordWeights = newKeywords;

        // Assert
        config.KeywordWeights.Should().BeSameAs(newKeywords);
        config.KeywordWeights.Should().HaveCount(2);
    }

    #endregion

    #region Complete Configuration Tests

    [Fact]
    public void ScoringConfig_CanCreateFullyCustomizedInstance()
    {
        // Arrange & Act
        var config = new ScoringConfig
        {
            MinimumScore = 15,
            AutoApplyThreshold = 85,
            RemotePoints = 60,
            ContractPoints = 25,
            PromotedPenalty = 30,
            TopApplicantPoints = 40,
            KeywordWeights = new Dictionary<string, int>
            {
                { "C#", 20 },
                { ".NET", 15 },
                { "Azure", 10 }
            },
            LocationPreferences = new Dictionary<string, int>
            {
                { "Remote", 30 },
                { "Hybrid", 10 }
            },
            CompanyPreferences = new Dictionary<string, int>
            {
                { "Microsoft", 25 }
            }
        };

        // Assert
        config.MinimumScore.Should().Be(15);
        config.AutoApplyThreshold.Should().Be(85);
        config.RemotePoints.Should().Be(60);
        config.ContractPoints.Should().Be(25);
        config.PromotedPenalty.Should().Be(30);
        config.TopApplicantPoints.Should().Be(40);
        config.KeywordWeights.Should().HaveCount(3);
        config.LocationPreferences.Should().HaveCount(2);
        config.CompanyPreferences.Should().HaveCount(1);
    }

    [Fact]
    public void ScoringConfig_ValidInstance_ShouldPassAllValidations()
    {
        // Arrange
        var config = new ScoringConfig
        {
            MinimumScore = 10,
            AutoApplyThreshold = 75,
            RemotePoints = 50,
            ContractPoints = 0,
            PromotedPenalty = 20,
            TopApplicantPoints = 30
        };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().BeEmpty("because the config instance should be valid");
    }

    [Fact]
    public void ScoringConfig_MultipleInvalidProperties_ShouldReturnMultipleValidationErrors()
    {
        // Arrange
        var config = new ScoringConfig
        {
            MinimumScore = -1,
            AutoApplyThreshold = 101,
            RemotePoints = -5,
            ContractPoints = 150,
            PromotedPenalty = -10,
            TopApplicantPoints = 200
        };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().HaveCount(6);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void ScoringConfig_AllZeroValues_ShouldBeValid()
    {
        // Arrange
        var config = new ScoringConfig
        {
            MinimumScore = 0,
            AutoApplyThreshold = 0,
            RemotePoints = 0,
            ContractPoints = 0,
            PromotedPenalty = 0,
            TopApplicantPoints = 0
        };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void ScoringConfig_AllMaxValues_ShouldBeValid()
    {
        // Arrange
        var config = new ScoringConfig
        {
            MinimumScore = 100,
            AutoApplyThreshold = 100,
            RemotePoints = 100,
            ContractPoints = 100,
            PromotedPenalty = 100,
            TopApplicantPoints = 100
        };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }

    #endregion
}
