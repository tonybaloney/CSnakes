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
        var (bigEndian, littleEndian, str) = typeDemos.ReturnUuid();
        var (beResult, leResult) = typeDemos.TakeUuid(guid.ToByteArray(true), guid.ToByteArray(), guid.ToString());
        guid = Guid.Parse(str);

        // Assert
        Assert.Equal(guid, new Guid(bigEndian, true));
        Assert.Equal(guid, new Guid(littleEndian));
        Assert.True(beResult);
        Assert.True(leResult);
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
    /// 1: Convert to TimeSpan/timedelta and use total seconds - appears to work on Windows but is buggy on Linux
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
        var (_, (hour, minute, second, millisecond, microsecond), str) = typeDemos.ReturnTime();
        var (_, tupleTest) = typeDemos.TakeTime(time.ToTimeSpan().TotalSeconds, (time.Hour, time.Minute, time.Second, time.Millisecond, time.Microsecond), $"{time:O}");

        // Assert
        //Assert.Equal(TimeOnly.Parse(str, CultureInfo.InvariantCulture), TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(totalSeconds)));
        Assert.Equal(TimeOnly.Parse(str, CultureInfo.InvariantCulture), new TimeOnly((int)hour, (int)minute, (int)second, (int)millisecond, (int)microsecond));
        //Assert.True(secondsTest);
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
    public void TestPythonDateTimeOffsetInterop()
    {
        // Arrange
        var typeDemos = fixture.PythonEnvironment.TypeDemos();

        // Act
        var strUtc = typeDemos.ReturnDateTime();
        var strLocal = typeDemos.ReturnDateTime(false);

        // Assert
        Assert.Equal(DateTimeOffset.UtcNow.Offset, DateTimeOffset.Parse(strUtc, CultureInfo.InvariantCulture).Offset);
        Assert.Equal(DateTimeOffset.Now.Offset, DateTimeOffset.Parse(strLocal, CultureInfo.InvariantCulture).Offset);
    }

    /// <summary>
    /// .NET DateTimeOffset is a bit tricky to convert to Python's datetime. The easiest way is to use the ISO8601 format
    ///
    /// Python's datetime only goes down to the microsecond level while .NET's DateTimeOffset goes down to the 100 nanosecond level.
    /// </summary>
    /// <param name="offset"></param>
    [Theory]
    [InlineData("Z")] // UTC (Coordinated Universal Time), no offset (e.g., Greenwich, UK).
    [InlineData("+00:00")] // Same as Z, explicitly written (Western Europe, Iceland).
    [InlineData("+01:00")] // Central European Time (e.g., Germany, France, Nigeria).
    [InlineData("+02:00")] // Eastern European Time (e.g., Finland, Ukraine, Egypt).
    [InlineData("+03:00")] // Moscow Time, Arabia Standard Time (e.g., Russia, Saudi Arabia).
    [InlineData("+03:30")] // Iran Standard Time (e.g., Iran).
    [InlineData("+04:00")] // Gulf Standard Time, Samara Time (e.g., UAE, Mauritius).
    [InlineData("+04:30")] // Afghanistan Time (e.g., Afghanistan).
    [InlineData("+05:00")] // Pakistan Standard Time, Uzbekistan Time (e.g., Pakistan, Maldives).
    [InlineData("+05:30")] // Indian Standard Time (e.g., India, Sri Lanka).
    [InlineData("+05:45")] // Nepal Time (e.g., Nepal).
    [InlineData("+06:00")] // Bangladesh Standard Time, Bhutan Time (e.g., Bangladesh, Bhutan).
    [InlineData("+06:30")] // Cocos Islands Time, Myanmar Time (e.g., Myanmar).
    [InlineData("+07:00")] // Indochina Time, Western Indonesia Time (e.g., Thailand, Vietnam).
    [InlineData("+08:00")] // China Standard Time, Singapore Time (e.g., China, Singapore).
    [InlineData("+08:45")] // Australian Central Western Time (e.g., Eucla, Australia).
    [InlineData("+09:00")] // Japan Standard Time, Korea Standard Time (e.g., Japan, South Korea).
    [InlineData("+09:30")] // Australian Central Standard Time (e.g., Northern Territory, Australia).
    [InlineData("+10:00")] // Australian Eastern Standard Time (e.g., Sydney, Melbourne).
    [InlineData("+10:30")] // Lord Howe Standard Time (e.g., Lord Howe Island, Australia).
    [InlineData("+11:00")] // Solomon Islands Time, Vanuatu Time (e.g., Solomon Islands, Norfolk Island).
    [InlineData("+12:00")] // New Zealand Standard Time, Fiji Time (e.g., New Zealand, Fiji).
    [InlineData("+12:45")] // Chatham Islands Standard Time (e.g., Chatham Islands, New Zealand).
    [InlineData("+13:00")] // Tonga Time, Phoenix Islands Time (e.g., Tonga, Kiribati).
    [InlineData("+14:00")] // Line Islands Time (e.g., Kiritimati, Kiribati).
    [InlineData("-01:00")] // Azores Standard Time (e.g., Azores, Portugal).
    [InlineData("-02:00")] // South Georgia Time, Eastern Greenland Time (e.g., South Georgia).
    [InlineData("-03:00")] // Argentina Time, Brasilia Time (e.g., Argentina, eastern Brazil).
    [InlineData("-03:30")] // Newfoundland Standard Time (e.g., Newfoundland, Canada).
    [InlineData("-04:00")] // Atlantic Standard Time, Chile Standard Time (e.g., eastern Caribbean, Chile).
    [InlineData("-05:00")] // Eastern Standard Time, Colombia Time (e.g., New York, Peru).
    [InlineData("-06:00")] // Central Standard Time, Central America (e.g., Chicago, Costa Rica).
    [InlineData("-07:00")] // Mountain Standard Time (e.g., Denver, Arizona without DST).
    [InlineData("-08:00")] // Pacific Standard Time (e.g., Los Angeles, Vancouver).
    [InlineData("-09:00")] // Alaska Standard Time, Gambier Islands (e.g., Alaska).
    [InlineData("-09:30")] // Marquesas Islands Time (e.g., Marquesas Islands, French Polynesia).
    [InlineData("-10:00")] // Hawaii Standard Time (e.g., Hawaii, Cook Islands).
    [InlineData("-11:00")] // Niue Time, American Samoa Time (e.g., American Samoa).
    [InlineData("-12:00")] // Baker Island Time (e.g., Baker Island, uninhabited US territory).
    public void TestDateTimeOffsetRoundtrip(string offset)
    {
        // Arrange
        // Note: We have to trim the ISO time value to the microsecond level to match Python's time value to pass the tests
        var expected = DateTimeOffset.Parse($"{DateOnly.FromDateTime(DateTime.Today):O}T{$"{TimeOnly.FromDateTime(DateTime.Now):O}"[..15]}{offset}", CultureInfo.InvariantCulture);
        var typeDemos = fixture.PythonEnvironment.TypeDemos();

        // Act
        var actual = DateTimeOffset.Parse(typeDemos.TakeDateTimeOffset($"{expected:O}"), CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(expected, actual);
    }
}
