namespace JobCommandCenter.UnitTests.Shared;

/// <summary>
/// Unit tests for the JobStatus enum.
/// </summary>
public class JobStatusTests
{
    [Fact]
    public void JobStatus_ShouldHaveFound_WithValueZero()
    {
        // Arrange & Act
        var status = JobStatus.Found;

        // Assert
        status.Should().Be(JobStatus.Found);
        ((int)status).Should().Be(0);
    }

    [Fact]
    public void JobStatus_ShouldHaveSaved_WithValueOne()
    {
        // Arrange & Act
        var status = JobStatus.Saved;

        // Assert
        status.Should().Be(JobStatus.Saved);
        ((int)status).Should().Be(1);
    }

    [Fact]
    public void JobStatus_ShouldHaveApplied_WithValueTwo()
    {
        // Arrange & Act
        var status = JobStatus.Applied;

        // Assert
        status.Should().Be(JobStatus.Applied);
        ((int)status).Should().Be(2);
    }

    [Fact]
    public void JobStatus_ShouldHaveInterviewing_WithValueThree()
    {
        // Arrange & Act
        var status = JobStatus.Interviewing;

        // Assert
        status.Should().Be(JobStatus.Interviewing);
        ((int)status).Should().Be(3);
    }

    [Fact]
    public void JobStatus_ShouldHaveOffered_WithValueFour()
    {
        // Arrange & Act
        var status = JobStatus.Offered;

        // Assert
        status.Should().Be(JobStatus.Offered);
        ((int)status).Should().Be(4);
    }

    [Fact]
    public void JobStatus_ShouldHaveRejected_WithValueFive()
    {
        // Arrange & Act
        var status = JobStatus.Rejected;

        // Assert
        status.Should().Be(JobStatus.Rejected);
        ((int)status).Should().Be(5);
    }

    [Fact]
    public void JobStatus_ShouldHaveArchived_WithValueSix()
    {
        // Arrange & Act
        var status = JobStatus.Archived;

        // Assert
        status.Should().Be(JobStatus.Archived);
        ((int)status).Should().Be(6);
    }

    [Fact]
    public void JobStatus_ShouldHaveSevenValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<JobStatus>();

        // Assert
        values.Should().HaveCount(7);
    }

    [Fact]
    public void JobStatus_Found_ShouldHaveDescriptionAttribute()
    {
        // Arrange & Act
        var fieldInfo = typeof(JobStatus).GetField(nameof(JobStatus.Found));
        var attribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Description.Should().Be("Found");
    }

    [Fact]
    public void JobStatus_Saved_ShouldHaveDescriptionAttribute()
    {
        // Arrange & Act
        var fieldInfo = typeof(JobStatus).GetField(nameof(JobStatus.Saved));
        var attribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Description.Should().Be("Saved");
    }

    [Fact]
    public void JobStatus_Applied_ShouldHaveDescriptionAttribute()
    {
        // Arrange & Act
        var fieldInfo = typeof(JobStatus).GetField(nameof(JobStatus.Applied));
        var attribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Description.Should().Be("Applied");
    }

    [Fact]
    public void JobStatus_Interviewing_ShouldHaveDescriptionAttribute()
    {
        // Arrange & Act
        var fieldInfo = typeof(JobStatus).GetField(nameof(JobStatus.Interviewing));
        var attribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Description.Should().Be("Interviewing");
    }

    [Fact]
    public void JobStatus_Offered_ShouldHaveDescriptionAttribute()
    {
        // Arrange & Act
        var fieldInfo = typeof(JobStatus).GetField(nameof(JobStatus.Offered));
        var attribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Description.Should().Be("Offered");
    }

    [Fact]
    public void JobStatus_Rejected_ShouldHaveDescriptionAttribute()
    {
        // Arrange & Act
        var fieldInfo = typeof(JobStatus).GetField(nameof(JobStatus.Rejected));
        var attribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Description.Should().Be("Rejected");
    }

    [Fact]
    public void JobStatus_Archived_ShouldHaveDescriptionAttribute()
    {
        // Arrange & Act
        var fieldInfo = typeof(JobStatus).GetField(nameof(JobStatus.Archived));
        var attribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Description.Should().Be("Archived");
    }

    [Fact]
    public void JobStatus_AllValuesShouldBeSequential()
    {
        // Arrange & Act
        var values = Enum.GetValues<JobStatus>().Cast<int>().OrderBy(v => v).ToList();

        // Assert
        for (int i = 0; i < values.Count; i++)
        {
            values[i].Should().Be(i);
        }
    }

    [Fact]
    public void JobStatus_ShouldParseFromString()
    {
        // Arrange & Act
        var parsed = Enum.Parse<JobStatus>("Applied");

        // Assert
        parsed.Should().Be(JobStatus.Applied);
    }

    [Fact]
    public void JobStatus_ShouldConvertFromInt()
    {
        // Arrange & Act
        JobStatus status = (JobStatus)3;

        // Assert
        status.Should().Be(JobStatus.Interviewing);
    }
}
