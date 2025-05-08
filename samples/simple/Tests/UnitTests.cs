using CSnakes.Runtime;
using System.Globalization;

namespace Tests;

[Collection("Python")]
public class UnitTests(PythonFixture fixture)
{
    [Fact]
    public void TestDictInterop()
    {
        // Arrange
        var typeDemos = fixture.PythonEnvironment.TypeDemos();

        // Act
        var result = typeDemos.ReturnDict();
        var equal = typeDemos.TakeDict(result);

        // Assert
        Assert.Equal(1, result["a"]);
        Assert.Equal(2, result["b"]);
        Assert.Equal(3, result["c"]);
        Assert.True(equal);
    }

    /// <summary>
    /// .NET Guid is little endian, while Python's UUID is big endian it doesn't matter which side handles the conversion
    /// </summary>
    [Fact]
    public void TestUuidGuidInterop()
    {
        // Arrange
        var typeDemos = fixture.PythonEnvironment.TypeDemos();
        var guid = Guid.NewGuid();

        // Act
        var (bytes, str) = typeDemos.ReturnUuid();
        var result = typeDemos.TakeUuid(guid.ToByteArray(), guid.ToString());

        // Assert
        Assert.Equal(Guid.Parse(str), new Guid(bytes));
        Assert.True(result);
    }

    /// <summary>
    /// Python's ordinal is one day ahead of .NET's DayNumber I'm making the weak in Python, but it could easily be done here too
    /// </summary>
    [Fact]
    public void TestDateInterop()
    {
        // Arrange
        var typeDemos = fixture.PythonEnvironment.TypeDemos();
        var date = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var (ordinal, str) = typeDemos.ReturnDate();
        var result = typeDemos.TakeDate(date.DayNumber, $"{date:O}");

        // Assert
        Assert.Equal(DateOnly.Parse(str, CultureInfo.InvariantCulture), DateOnly.FromDayNumber((int)ordinal));
        Assert.True(result);
    }

    /// <summary>
    /// Unfortunately there isn't a single value to convert .NET TimeOnly & Python time. I added 3 strategies
    /// 1: Convert to TimeSpan/timedelta and use total seconds
    /// 2: Convert to tuple (hour, minute, second, millisecond, microsecond)
    /// 3: Convert to string using the ISO8601 format
    /// </summary>
    [Fact]
    public void TestTimeInterop()
    {
        // Arrange
        var typeDemos = fixture.PythonEnvironment.TypeDemos();
        var time = TimeOnly.FromDateTime(DateTime.Now);

        // Act
        var (totalSeconds, (hour, minute, second, millisecond, microsecond), str) = typeDemos.ReturnTime();
        var (secondsTest, tupleTest) = typeDemos.TakeTime(time.ToTimeSpan().TotalSeconds, (time.Hour, time.Minute, time.Second, time.Millisecond, time.Microsecond), $"{time:O}");

        // Assert
        Assert.Equal(TimeOnly.Parse(str, CultureInfo.InvariantCulture), TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(totalSeconds)));
        Assert.Equal(TimeOnly.Parse(str, CultureInfo.InvariantCulture), new TimeOnly((int)hour, (int)minute, (int)second, (int)millisecond, (int)microsecond));
        Assert.True(secondsTest);
        Assert.True(tupleTest);
    }

    /// <summary>
    /// The easiest way to convert .NET TimeSpan to Python's timedelta is to use the total seconds
    /// </summary>
    [Fact]
    public void TestTimeSpanInterop()
    {
        // Arrange
        var typeDemos = fixture.PythonEnvironment.TypeDemos();
        var timeSpan = new TimeSpan(6, 5, 4, 3, 2, 1);

        // Act
        var (totalSeconds, str) = typeDemos.ReturnTimeDelta();
        var result = typeDemos.TakeTimeSpan(timeSpan.TotalSeconds, $"{timeSpan:G}");

        // Assert
        Assert.Equal(TimeSpan.Parse(str, CultureInfo.InvariantCulture), TimeSpan.FromSeconds(totalSeconds));
        Assert.True(result);
    }

    [Fact]
    public void TestDateTimeInterop()
    {
        // Arrange
        var typeDemos = fixture.PythonEnvironment.TypeDemos();

        // Act
        var strUtc = typeDemos.ReturnDateTime();
        var strLocal = typeDemos.ReturnDateTime(false);

        // Assert
        Assert.Equal(TimeSpan.Zero, DateTimeOffset.Parse(strUtc, CultureInfo.InvariantCulture).Offset);
        Assert.Equal(DateTimeOffset.Now.Offset, DateTimeOffset.Parse(strLocal, CultureInfo.InvariantCulture).Offset);
    }
}
