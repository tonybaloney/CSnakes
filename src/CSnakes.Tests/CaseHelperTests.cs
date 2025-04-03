using CSnakes;

namespace CSnakes.Tests;

public class CaseHelperTests
{
    [Theory]
    [InlineData("Hello", "hello")]
    [InlineData("HelloWorld", "hello_world")]
    [InlineData("Hello_", "hello_")]
    [InlineData("Hello_World", "hello__world")]
    [InlineData("_Hello_World", "_hello__world")]
    public void VerifyToPascalCase(string input, string expected) =>
        Assert.Equal(input, expected.ToPascalCase());

    [Theory]
    [InlineData("hello", "hello")]
    [InlineData("helloWorld", "hello_world")]
    [InlineData("hello_", "hello_")]
    [InlineData("hello_World", "hello__world")]
    // TODO: (track) This instance could arguably be _hello_World although the name is already weird
    [InlineData("_Hello_World", "_hello__world")]
    public void VerifyToLowerPascalCase(string input, string expected) =>
        Assert.Equal(input, expected.ToLowerPascalCase());
}
