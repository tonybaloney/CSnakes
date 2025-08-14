namespace CSnakes.Tests;

public class NamespaceHelperTests
{
    [Theory]
    [InlineData("foo/bar/baz.py", "foo.bar.baz")]
    [InlineData("foo/bar/__init__.py", "foo.bar")]
    [InlineData("abc.pyi", "abc")]
    public void VerifyAsPythonImportPath(string path, string expected) =>
        Assert.Equal(expected, NamespaceHelper.AsPythonImportPath(path));

    [Theory]
    [InlineData("foo/bar/baz.py", "Foo.Bar")]
    [InlineData("foo/bar/__init__.py", "Foo")]
    [InlineData("_csv.py", "")]
    [InlineData("/_csv.py", "")]
    [InlineData("abc.pyi", "")]
    public void VerifyAsDotNetNamespace(string path, string expected) =>
        Assert.Equal(expected, NamespaceHelper.AsDotNetNamespace(path));

    [Theory]
    [InlineData("foo/bar/baz.py", "Baz")]
    [InlineData("foo/bar/__init__.py", "Bar")]
    [InlineData("abc.py", "Abc")]
    public void VerifyAsDotNetClassName(string path, string expected) =>
        Assert.Equal(expected, NamespaceHelper.AsDotNetClassName(path));
}
