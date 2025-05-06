using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Python;
public class ImportTests : RuntimeTestBase
{
    [Fact]
    public void TestImportModule()
    {
        using PyObject sys = Import.ImportModule("sys");
        Assert.NotNull(sys);
        Assert.Equal("<module 'sys' (built-in)>", sys.ToString());
    }

    [Fact]
    public void TestReloadModule()
    {
        PyObject sys = Import.ImportModule("sys");
        Assert.NotNull(sys);
        Import.ReloadModule(ref sys);
        Assert.Equal("<module 'sys' (built-in)>", sys.ToString());
    }

    [Fact]
    public void TestReloadModuleThatIsntAModule()
    {
        PyObject sys = PyObject.From(42); // definitely not a module
        Assert.NotNull(sys);
        Assert.Throws<PythonInvocationException>(() => Import.ReloadModule(ref sys));
    }

    [Fact]
    public void TestImportFromString()
    {
        string source = "print('hello world')"; // base64 encoded "print("Hello, World")"
        string path = Environment.CurrentDirectory;
        using PyObject module = Import.ImportModule("test_module", source, path);
        Assert.NotNull(module);
        Assert.StartsWith("<module 'test_module' ", module.ToString());
    }
}
