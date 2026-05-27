using System;

namespace Integration.Tests;

public class DateTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void TestDate_Roundtrip()
    {
        var module = Env.TestDate();
        var date = new DateOnly(2026, 3, 8);
        var result = module.TestDateRoundtrip(date);
        Assert.Equal(date, result);
    }

    [Fact]
    public void TestDate_MinValue()
    {
        var module = Env.TestDate();
        var result = module.TestDateRoundtrip(DateOnly.MinValue);
        Assert.Equal(DateOnly.MinValue, result);
    }

    [Fact]
    public void TestDate_CreateFromComponents()
    {
        var module = Env.TestDate();
        var expected = new DateOnly(2026, 3, 8);
        var result = module.TestCreateDate(2026, 3, 8);
        Assert.Equal(expected, result);
    }
}
