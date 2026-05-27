using CSnakes.Runtime.Python;
using System;

namespace Integration.Tests;

public class UuidTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void TestUuid_Roundtrip()
    {
        var module = Env.TestUuid();
        var guid = Guid.Parse("12345678-1234-5678-1234-567812345678");
        var result = module.TestUuidRoundtrip(guid);
        Assert.Equal(guid, result);
    }

    [Fact]
    public void TestUuid_EmptyGuid()
    {
        var module = Env.TestUuid();
        var result = module.TestUuidRoundtrip(Guid.Empty);
        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public void TestUuid_CreateFromString()
    {
        var module = Env.TestUuid();
        var expected = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
        var result = module.TestCreateUuid("550e8400-e29b-41d4-a716-446655440000");
        Assert.Equal(expected, result);
    }
}
