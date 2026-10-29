using Elwise.Core.Scheduling;

namespace Elwise.Tests.Scheduling;

public class AllowedHoursTests
{
    [Theory]
    [InlineData(7, 23, 6, false)]
    [InlineData(7, 23, 7, true)]
    [InlineData(7, 23, 22, true)]
    [InlineData(7, 23, 23, false)]
    [InlineData(0, 24, 0, true)]
    [InlineData(22, 7, 23, true)]
    [InlineData(22, 7, 3, true)]
    [InlineData(22, 7, 12, false)]
    public void Contains_ReturnsExpected(int from, int to, int hour, bool expected)
    {
        // Arrange
        var allowed = new AllowedHours(from, to);

        // Act
        var actual = allowed.Contains(hour);

        //Assert
        Assert.Equal(expected, actual);
    }
}