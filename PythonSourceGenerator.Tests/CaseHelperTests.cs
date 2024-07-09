namespace PythonSourceGenerator.Tests;

public class CaseHelperTests
{
    [Fact]
    public void VerifyToPascalCase()
    {
        Assert.Equal("Hello", CaseHelper.ToPascalCase("hello"));
        Assert.Equal("HelloWorld", CaseHelper.ToPascalCase("hello_world"));
        Assert.Equal("Hello_", CaseHelper.ToPascalCase("hello_"));
        Assert.Equal("Hello_World", CaseHelper.ToPascalCase("hello__world"));
        Assert.Equal("_Hello_World", CaseHelper.ToPascalCase("_hello__world"));
    }

    [Fact]
    public void VerifyToLowerPascalCase()
    {
        Assert.Equal("hello", CaseHelper.ToLowerPascalCase("hello"));
        Assert.Equal("helloWorld", CaseHelper.ToLowerPascalCase("hello_world"));
        Assert.Equal("hello_", CaseHelper.ToLowerPascalCase("hello_"));
        Assert.Equal("hello_World", CaseHelper.ToLowerPascalCase("hello__world"));
        // TODO: This instance could arguably be _hello_World although the name is already weird
        Assert.Equal("_Hello_World", CaseHelper.ToLowerPascalCase("_hello__world"));
    }
}
