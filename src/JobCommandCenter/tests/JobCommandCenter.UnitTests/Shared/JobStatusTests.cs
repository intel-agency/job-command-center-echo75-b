using System.ComponentModel;
using FluentAssertions;
using JobCommandCenter.Shared.Models;
using Xunit;

namespace JobCommandCenter.UnitTests.Shared;

/// <summary>
/// Unit tests for the JobStatus enum.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Domain Models")]
public class JobStatusTests
{
    [Fact]
    public void JobStatus_ShouldHaveExactly9Values()
    {
        // Arrange & Act
        var enumValues = Enum.GetValues<JobStatus>();

        // Assert
        enumValues.Should().HaveCount(9, "because we expect exactly 9 job status values");
    }

    [Fact]
    public void JobStatus_Found_ShouldHaveValue0()
    {
        // Arrange & Act
        var value = (int)JobStatus.Found;

        // Assert
        value.Should().Be(0);
    }

    [Fact]
    public void JobStatus_Saved_ShouldHaveValue1()
    {
        // Arrange & Act
        var value = (int)JobStatus.Saved;

        // Assert
        value.Should().Be(1);
    }

    [Fact]
    public void JobStatus_Scored_ShouldHaveValue2()
    {
        // Arrange & Act
        var value = (int)JobStatus.Scored;

        // Assert
        value.Should().Be(2);
    }

    [Fact]
    public void JobStatus_Pending_ShouldHaveValue3()
    {
        // Arrange & Act
        var value = (int)JobStatus.Pending;

        // Assert
        value.Should().Be(3);
    }

    [Fact]
    public void JobStatus_Applied_ShouldHaveValue4()
    {
        // Arrange & Act
        var value = (int)JobStatus.Applied;

        // Assert
        value.Should().Be(4);
    }

    [Fact]
    public void JobStatus_Interviewing_ShouldHaveValue5()
    {
        // Arrange & Act
        var value = (int)JobStatus.Interviewing;

        // Assert
        value.Should().Be(5);
    }

    [Fact]
    public void JobStatus_Offered_ShouldHaveValue6()
    {
        // Arrange & Act
        var value = (int)JobStatus.Offered;

        // Assert
        value.Should().Be(6);
    }

    [Fact]
    public void JobStatus_Archived_ShouldHaveValue7()
    {
        // Arrange & Act
        var value = (int)JobStatus.Archived;

        // Assert
        value.Should().Be(7);
    }

    [Fact]
    public void JobStatus_Rejected_ShouldHaveValue8()
    {
        // Arrange & Act
        var value = (int)JobStatus.Rejected;

        // Assert
        value.Should().Be(8);
    }

    [Theory]
    [InlineData(JobStatus.Found, "Found")]
    [InlineData(JobStatus.Saved, "Saved")]
    [InlineData(JobStatus.Scored, "Scored")]
    [InlineData(JobStatus.Pending, "Pending")]
    [InlineData(JobStatus.Applied, "Applied")]
    [InlineData(JobStatus.Interviewing, "Interviewing")]
    [InlineData(JobStatus.Offered, "Offered")]
    [InlineData(JobStatus.Archived, "Archived")]
    [InlineData(JobStatus.Rejected, "Rejected")]
    public void JobStatus_ShouldHaveCorrectDescriptionAttribute(JobStatus status, string expectedDescription)
    {
        // Arrange
        var fieldInfo = status.GetType().GetField(status.ToString());

        // Act
        var attribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;

        // Assert
        attribute.Should().NotBeNull($"because {status} should have a Description attribute");
        attribute!.Description.Should().Be(expectedDescription);
    }

    [Fact]
    public void JobStatus_ValuesShouldBeSequential()
    {
        // Arrange
        var values = Enum.GetValues<JobStatus>().Cast<int>().OrderBy(v => v).ToList();

        // Act & Assert
        for (int i = 0; i < values.Count; i++)
        {
            values[i].Should().Be(i, $"because enum values should be sequential starting from 0");
        }
    }

    [Fact]
    public void JobStatus_AllValuesShouldHaveDescriptionAttribute()
    {
        // Arrange
        var enumValues = Enum.GetValues<JobStatus>();

        // Act & Assert
        foreach (var status in enumValues)
        {
            var fieldInfo = status.GetType().GetField(status.ToString());
            var attribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;

            attribute.Should().NotBeNull($"because {status} should have a Description attribute");
        }
    }

    [Theory]
    [InlineData(0, JobStatus.Found)]
    [InlineData(1, JobStatus.Saved)]
    [InlineData(2, JobStatus.Scored)]
    [InlineData(3, JobStatus.Pending)]
    [InlineData(4, JobStatus.Applied)]
    [InlineData(5, JobStatus.Interviewing)]
    [InlineData(6, JobStatus.Offered)]
    [InlineData(7, JobStatus.Archived)]
    [InlineData(8, JobStatus.Rejected)]
    public void JobStatus_ShouldParseFromInt(int intValue, JobStatus expectedStatus)
    {
        // Arrange & Act
        var result = (JobStatus)intValue;

        // Assert
        result.Should().Be(expectedStatus);
    }

    [Fact]
    public void JobStatus_ShouldNotContainUnexpectedValues()
    {
        // Arrange
        var expectedNames = new[]
        {
            "Found", "Saved", "Scored", "Pending", "Applied",
            "Interviewing", "Offered", "Archived", "Rejected"
        };

        // Act
        var actualNames = Enum.GetNames<JobStatus>();

        // Assert
        actualNames.Should().BeEquivalentTo(expectedNames);
    }
}
