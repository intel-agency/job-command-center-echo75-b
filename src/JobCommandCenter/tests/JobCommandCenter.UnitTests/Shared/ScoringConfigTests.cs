namespace JobCommandCenter.UnitTests.Shared;

/// <summary>
/// Unit tests for the ScoringConfig model.
/// </summary>
public class ScoringConfigTests
{
    #region Default Values Tests

    [Fact]
    public void ScoringConfig_ShouldInitializeWithDefaultRemotePoints()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.RemotePoints.Should().Be(50);
    }

    [Fact]
    public void ScoringConfig_ShouldInitializeWithDefaultContractPoints()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.ContractPoints.Should().Be(0);
    }

    [Fact]
    public void ScoringConfig_ShouldInitializeWithDefaultPromotedPenalty()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.PromotedPenalty.Should().Be(20);
    }

    [Fact]
    public void ScoringConfig_ShouldInitializeWithDefaultTopApplicantPoints()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.TopApplicantPoints.Should().Be(30);
    }

    [Fact]
    public void ScoringConfig_ShouldInitializeWithDefaultMinimumScore()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.MinimumScore.Should().Be(0);
    }

    [Fact]
    public void ScoringConfig_ShouldInitializeWithDefaultAutoApplyThreshold()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.AutoApplyThreshold.Should().Be(80);
    }

    [Fact]
    public void ScoringConfig_ShouldInitializeKeywordWeightsAsEmptyDictionary()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.KeywordWeights.Should().NotBeNull();
        config.KeywordWeights.Should().BeEmpty();
    }

    [Fact]
    public void ScoringConfig_ShouldInitializePreferredLocationsAsEmptyList()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.PreferredLocations.Should().NotBeNull();
        config.PreferredLocations.Should().BeEmpty();
    }

    [Fact]
    public void ScoringConfig_ShouldInitializePreferredCompaniesAsEmptyList()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.PreferredCompanies.Should().NotBeNull();
        config.PreferredCompanies.Should().BeEmpty();
    }

    #endregion

    #region Range Validation Tests (0-100)

    [Fact]
    public void ScoringConfig_RemotePoints_ShouldValidateMinimumValue0()
    {
        // Arrange
        var config = new ScoringConfig { RemotePoints = -1 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.RemotePoints)));
    }

    [Fact]
    public void ScoringConfig_RemotePoints_ShouldValidateMaximumValue100()
    {
        // Arrange
        var config = new ScoringConfig { RemotePoints = 101 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.RemotePoints)));
    }

    [Fact]
    public void ScoringConfig_RemotePoints_ShouldAcceptValue0()
    {
        // Arrange
        var config = new ScoringConfig { RemotePoints = 0 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains(nameof(ScoringConfig.RemotePoints)));
    }

    [Fact]
    public void ScoringConfig_RemotePoints_ShouldAcceptValue100()
    {
        // Arrange
        var config = new ScoringConfig { RemotePoints = 100 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains(nameof(ScoringConfig.RemotePoints)));
    }

    [Fact]
    public void ScoringConfig_ContractPoints_ShouldValidateMinimumValue0()
    {
        // Arrange
        var config = new ScoringConfig { ContractPoints = -1 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.ContractPoints)));
    }

    [Fact]
    public void ScoringConfig_ContractPoints_ShouldValidateMaximumValue100()
    {
        // Arrange
        var config = new ScoringConfig { ContractPoints = 101 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.ContractPoints)));
    }

    [Fact]
    public void ScoringConfig_ContractPoints_ShouldAcceptValue0()
    {
        // Arrange
        var config = new ScoringConfig { ContractPoints = 0 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains(nameof(ScoringConfig.ContractPoints)));
    }

    [Fact]
    public void ScoringConfig_ContractPoints_ShouldAcceptValue100()
    {
        // Arrange
        var config = new ScoringConfig { ContractPoints = 100 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains(nameof(ScoringConfig.ContractPoints)));
    }

    [Fact]
    public void ScoringConfig_PromotedPenalty_ShouldValidateMinimumValue0()
    {
        // Arrange
        var config = new ScoringConfig { PromotedPenalty = -1 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.PromotedPenalty)));
    }

    [Fact]
    public void ScoringConfig_PromotedPenalty_ShouldValidateMaximumValue100()
    {
        // Arrange
        var config = new ScoringConfig { PromotedPenalty = 101 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.PromotedPenalty)));
    }

    [Fact]
    public void ScoringConfig_PromotedPenalty_ShouldAcceptValue0()
    {
        // Arrange
        var config = new ScoringConfig { PromotedPenalty = 0 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains(nameof(ScoringConfig.PromotedPenalty)));
    }

    [Fact]
    public void ScoringConfig_PromotedPenalty_ShouldAcceptValue100()
    {
        // Arrange
        var config = new ScoringConfig { PromotedPenalty = 100 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains(nameof(ScoringConfig.PromotedPenalty)));
    }

    [Fact]
    public void ScoringConfig_TopApplicantPoints_ShouldValidateMinimumValue0()
    {
        // Arrange
        var config = new ScoringConfig { TopApplicantPoints = -1 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.TopApplicantPoints)));
    }

    [Fact]
    public void ScoringConfig_TopApplicantPoints_ShouldValidateMaximumValue100()
    {
        // Arrange
        var config = new ScoringConfig { TopApplicantPoints = 101 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.TopApplicantPoints)));
    }

    [Fact]
    public void ScoringConfig_TopApplicantPoints_ShouldAcceptValue0()
    {
        // Arrange
        var config = new ScoringConfig { TopApplicantPoints = 0 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains(nameof(ScoringConfig.TopApplicantPoints)));
    }

    [Fact]
    public void ScoringConfig_TopApplicantPoints_ShouldAcceptValue100()
    {
        // Arrange
        var config = new ScoringConfig { TopApplicantPoints = 100 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains(nameof(ScoringConfig.TopApplicantPoints)));
    }

    [Fact]
    public void ScoringConfig_MinimumScore_ShouldValidateMinimumValue0()
    {
        // Arrange
        var config = new ScoringConfig { MinimumScore = -1 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.MinimumScore)));
    }

    [Fact]
    public void ScoringConfig_MinimumScore_ShouldValidateMaximumValue100()
    {
        // Arrange
        var config = new ScoringConfig { MinimumScore = 101 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.MinimumScore)));
    }

    [Fact]
    public void ScoringConfig_MinimumScore_ShouldAcceptValue0()
    {
        // Arrange
        var config = new ScoringConfig { MinimumScore = 0 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains(nameof(ScoringConfig.MinimumScore)));
    }

    [Fact]
    public void ScoringConfig_MinimumScore_ShouldAcceptValue100()
    {
        // Arrange
        var config = new ScoringConfig { MinimumScore = 100 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains(nameof(ScoringConfig.MinimumScore)));
    }

    [Fact]
    public void ScoringConfig_AutoApplyThreshold_ShouldValidateMinimumValue0()
    {
        // Arrange
        var config = new ScoringConfig { AutoApplyThreshold = -1 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.AutoApplyThreshold)));
    }

    [Fact]
    public void ScoringConfig_AutoApplyThreshold_ShouldValidateMaximumValue100()
    {
        // Arrange
        var config = new ScoringConfig { AutoApplyThreshold = 101 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.AutoApplyThreshold)));
    }

    [Fact]
    public void ScoringConfig_AutoApplyThreshold_ShouldAcceptValue0()
    {
        // Arrange
        var config = new ScoringConfig { AutoApplyThreshold = 0 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains(nameof(ScoringConfig.AutoApplyThreshold)));
    }

    [Fact]
    public void ScoringConfig_AutoApplyThreshold_ShouldAcceptValue100()
    {
        // Arrange
        var config = new ScoringConfig { AutoApplyThreshold = 100 };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains(nameof(ScoringConfig.AutoApplyThreshold)));
    }

    #endregion

    #region Collection Properties Tests

    [Fact]
    public void ScoringConfig_KeywordWeights_CanBeModified()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.KeywordWeights.Add("C#", 10);
        config.KeywordWeights.Add(".NET", 15);
        config.KeywordWeights.Add("Azure", 8);

        // Assert
        config.KeywordWeights.Should().HaveCount(3);
        config.KeywordWeights["C#"].Should().Be(10);
        config.KeywordWeights[".NET"].Should().Be(15);
        config.KeywordWeights["Azure"].Should().Be(8);
    }

    [Fact]
    public void ScoringConfig_PreferredLocations_CanBeModified()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.PreferredLocations.Add("San Francisco");
        config.PreferredLocations.Add("Seattle");
        config.PreferredLocations.Add("Remote");

        // Assert
        config.PreferredLocations.Should().HaveCount(3);
        config.PreferredLocations.Should().Contain("San Francisco");
        config.PreferredLocations.Should().Contain("Seattle");
        config.PreferredLocations.Should().Contain("Remote");
    }

    [Fact]
    public void ScoringConfig_PreferredCompanies_CanBeModified()
    {
        // Arrange
        var config = new ScoringConfig();

        // Act
        config.PreferredCompanies.Add("Microsoft");
        config.PreferredCompanies.Add("Google");
        config.PreferredCompanies.Add("Amazon");

        // Assert
        config.PreferredCompanies.Should().HaveCount(3);
        config.PreferredCompanies.Should().Contain("Microsoft");
        config.PreferredCompanies.Should().Contain("Google");
        config.PreferredCompanies.Should().Contain("Amazon");
    }

    #endregion

    #region Complete Configuration Tests

    [Fact]
    public void ScoringConfig_ShouldBeValid_WhenAllPropertiesAreWithinRange()
    {
        // Arrange
        var config = new ScoringConfig
        {
            RemotePoints = 50,
            ContractPoints = 25,
            PromotedPenalty = 10,
            TopApplicantPoints = 30,
            MinimumScore = 40,
            AutoApplyThreshold = 75
        };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void ScoringConfig_ShouldAllowFullCustomization()
    {
        // Arrange & Act
        var config = new ScoringConfig
        {
            RemotePoints = 75,
            ContractPoints = 50,
            PromotedPenalty = 5,
            TopApplicantPoints = 40,
            MinimumScore = 60,
            AutoApplyThreshold = 90
        };

        // Add keyword weights
        config.KeywordWeights["senior"] = 10;
        config.KeywordWeights["lead"] = 15;
        config.KeywordWeights["architect"] = 20;

        // Add preferred locations
        config.PreferredLocations.Add("New York");
        config.PreferredLocations.Add("Boston");

        // Add preferred companies
        config.PreferredCompanies.Add("Netflix");
        config.PreferredCompanies.Add("Stripe");

        // Assert
        config.RemotePoints.Should().Be(75);
        config.ContractPoints.Should().Be(50);
        config.PromotedPenalty.Should().Be(5);
        config.TopApplicantPoints.Should().Be(40);
        config.MinimumScore.Should().Be(60);
        config.AutoApplyThreshold.Should().Be(90);
        config.KeywordWeights.Should().HaveCount(3);
        config.PreferredLocations.Should().HaveCount(2);
        config.PreferredCompanies.Should().HaveCount(2);
    }

    [Fact]
    public void ScoringConfig_AllRangeProperties_ShouldAcceptMidRangeValues()
    {
        // Arrange
        var config = new ScoringConfig
        {
            RemotePoints = 50,
            ContractPoints = 50,
            PromotedPenalty = 50,
            TopApplicantPoints = 50,
            MinimumScore = 50,
            AutoApplyThreshold = 50
        };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void ScoringConfig_ShouldDetectMultipleValidationErrors()
    {
        // Arrange
        var config = new ScoringConfig
        {
            RemotePoints = -1,
            ContractPoints = 101,
            PromotedPenalty = -5,
            TopApplicantPoints = 150
        };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().HaveCount(4);
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.RemotePoints)));
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.ContractPoints)));
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.PromotedPenalty)));
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(ScoringConfig.TopApplicantPoints)));
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
